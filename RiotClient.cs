using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace valstat
{
    // ---------- Data the front end receives ----------

    public enum MatchPhase { Menus, AgentSelect, InGame }

    public record LockFileInfo(string Name, int Pid, int Port, string Password, string Protocol);
    public record AgentInfo(string Name, string IconUrl);
    public record TierInfo(int Tier, string Name, string IconUrl);
    public record MatchPlayer(string Puuid, string TeamId, string AgentId, int AccountLevel, bool Incognito,
                              string SelectionState = ""); // agent select only: "", "selected", "locked"
    public record LiveMatch(string MatchId, string MapName, List<MatchPlayer> Players);

    public record RankSummary(int CurrentTier, int PeakTier, int Wins, int Games)
    {
        public double? WinRate => Games > 0 ? 100.0 * Wins / Games : null;
    }

    // ---------- Riot data access ----------

    public sealed class RiotClient : IDisposable
    {
        // Paths and constants
        private static readonly string LockFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Riot Games", "Riot Client", "Config", "lockfile");

        private static readonly string ShooterLogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VALORANT", "Saved", "Logs", "ShooterGame.log");

        private static readonly string ClientPlatform = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new
            {
                platformType = "PC",
                platformOS = "Windows",
                platformOSVersion = "10.0.19042.1.256.64bit",
                platformChipset = "Unknown"
            })));

        // HTTP
        private HttpClient? local;                         // 127.0.0.1 Riot Client (self-signed cert)
        // Riot servers + valorant-api.com. Short timeout so a stuck request can't stall polling.
        private readonly HttpClient remote = new() { Timeout = TimeSpan.FromSeconds(15) };
        private readonly SemaphoreSlim throttle = new(4);  // keeps us under Riot's rate limits

        // Auth
        private string accessToken = "";
        private string entitlement = "";
        private string clientVersion = "";
        private DateTime tokenFetchedUtc;

        // Content lookups
        private readonly Dictionary<string, AgentInfo> agents = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, string> maps = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<int, TierInfo> tiers = new();
        private readonly HashSet<string> preAscendantSeasons = new(StringComparer.OrdinalIgnoreCase);

        // Cache: matchId -> (puuid -> kills, deaths)
        private readonly ConcurrentDictionary<string, Task<Dictionary<string, (int Kills, int Deaths)>>> matchStats = new();

        // Connection info
        public bool IsConnected => local != null;
        public string Puuid { get; private set; } = "";
        public string Region { get; private set; } = "";
        public string Shard { get; private set; } = "";

        private string Glz => $"https://glz-{Region}-1.{Shard}.a.pvp.net";
        private string Pd => $"https://pd.{Shard}.a.pvp.net";
        private string Shared => $"https://shared.{Shard}.a.pvp.net";

        // ================= Connection =================

        /// <summary>Reads the Riot Client lockfile. Returns null if the client isn't running.</summary>
        public static LockFileInfo? ReadLockFile()
        {
            if (!File.Exists(LockFilePath))
                return null;

            using var fs = new FileStream(LockFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs, Encoding.UTF8);
            var parts = reader.ReadToEnd().Trim().Split(':');

            if (parts.Length < 5)
                return null;

            return new LockFileInfo(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), parts[3], parts[4]);
        }

        /// <summary>Connects to the running game. Returns false if VALORANT isn't running.</summary>
        public async Task<bool> ConnectAsync()
        {
            var lf = ReadLockFile();
            if (lf is null)
                return false;

            Disconnect();
            try
            {
                var handler = new HttpClientHandler
                {
                    // Only the local Riot Client uses a self-signed cert
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
                local = new HttpClient(handler)
                {
                    BaseAddress = new Uri($"https://127.0.0.1:{lf.Port}"),
                    Timeout = TimeSpan.FromSeconds(10)
                };
                local.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"riot:{lf.Password}")));

                await RefreshTokensAsync();
                ReadRegionAndShard();

                if (agents.Count == 0)
                    await LoadContentAsync();

                return true;
            }
            catch
            {
                Disconnect();
                throw;
            }
        }

        public void Disconnect()
        {
            local?.Dispose();
            local = null;
        }

        // ================= Live match =================

        /// <summary>Match ID while in agent select, otherwise null.</summary>
        public async Task<string?> GetPregameMatchIdAsync()
        {
            using var doc = await RiotGetAsync($"{Glz}/pregame/v1/players/{Puuid}");
            return doc is null ? null : NullIfEmpty(Str(doc.RootElement, "MatchID"));
        }

        /// <summary>Match ID while in a live game, otherwise null.</summary>
        public async Task<string?> GetCoreGameMatchIdAsync()
        {
            using var doc = await RiotGetAsync($"{Glz}/core-game/v1/players/{Puuid}");
            return doc is null ? null : NullIfEmpty(Str(doc.RootElement, "MatchID"));
        }

        /// <summary>Map and all 10 players of a live game.</summary>
        public async Task<LiveMatch?> GetCoreGameMatchAsync(string matchId)
        {
            using var doc = await RiotGetAsync($"{Glz}/core-game/v1/matches/{matchId}");
            if (doc is null)
                return null;

            var root = doc.RootElement;
            var players = new List<MatchPlayer>();

            if (root.TryGetProperty("Players", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var p in arr.EnumerateArray())
                {
                    TryObj(p, "PlayerIdentity", out var id);
                    bool hideLevel = id.ValueKind == JsonValueKind.Object && Bool(id, "HideAccountLevel");

                    players.Add(new MatchPlayer(
                        Str(p, "Subject"),
                        Str(p, "TeamID"),
                        Str(p, "CharacterID"),
                        hideLevel || id.ValueKind != JsonValueKind.Object ? 0 : Int(id, "AccountLevel"),
                        id.ValueKind == JsonValueKind.Object && Bool(id, "Incognito")));
                }
            }

            return new LiveMatch(Str(root, "MatchID"), GetMapName(Str(root, "MapID")), players);
        }

        /// <summary>Map and your team (the enemy team is hidden until the match starts) during agent select.</summary>
        public async Task<LiveMatch?> GetPregameMatchAsync(string matchId)
        {
            using var doc = await RiotGetAsync($"{Glz}/pregame/v1/matches/{matchId}");
            if (doc is null)
                return null;

            var root = doc.RootElement;
            var players = new List<MatchPlayer>();

            if (TryObj(root, "AllyTeam", out var team) &&
                team.TryGetProperty("Players", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                string teamId = Str(team, "TeamID");
                foreach (var p in arr.EnumerateArray())
                {
                    TryObj(p, "PlayerIdentity", out var id);
                    bool hasId = id.ValueKind == JsonValueKind.Object;

                    players.Add(new MatchPlayer(
                        Str(p, "Subject"),
                        teamId,
                        Str(p, "CharacterID"),
                        hasId && !Bool(id, "HideAccountLevel") ? Int(id, "AccountLevel") : 0,
                        hasId && Bool(id, "Incognito"),
                        Str(p, "CharacterSelectionState")));
                }
            }

            return new LiveMatch(Str(root, "ID"), GetMapName(Str(root, "MapID")), players);
        }

        /// <summary>ID of your most recent finished match, or null if you have none.</summary>
        public async Task<string?> GetLatestMatchIdAsync()
        {
            using var history = await RiotGetAsync($"{Pd}/match-history/v1/history/{Puuid}?startIndex=0&endIndex=1");
            if (history is null ||
                !history.RootElement.TryGetProperty("History", out var arr) ||
                arr.ValueKind != JsonValueKind.Array ||
                arr.GetArrayLength() == 0)
                return null;

            return NullIfEmpty(Str(arr[0], "MatchID"));
        }

        /// <summary>TEST ONLY: loads a finished match in the same shape as a live match.</summary>
        public async Task<LiveMatch?> GetFinishedMatchAsLiveAsync(string matchId)
        {
            using var doc = await RiotGetAsync($"{Pd}/match-details/v1/matches/{matchId}");
            if (doc is null)
                return null;

            var root = doc.RootElement;
            var players = new List<MatchPlayer>();

            if (root.TryGetProperty("players", out var ps) && ps.ValueKind == JsonValueKind.Array)
                foreach (var p in ps.EnumerateArray())
                    players.Add(new MatchPlayer(
                        Str(p, "subject"),
                        Str(p, "teamId"),
                        Str(p, "characterId"),
                        Int(p, "accountLevel"),
                        false));

            string mapId = TryObj(root, "matchInfo", out var info) ? Str(info, "mapId") : "";
            return new LiveMatch(matchId, GetMapName(mapId) + " (replay)", players);
        }

        // ================= Players =================

        /// <summary>PUUID -> "Name#Tag".</summary>
        public async Task<Dictionary<string, string>> GetUsernamesAsync(IEnumerable<string> puuids)
        {
            var result = new Dictionary<string, string>();
            var list = puuids.Distinct().ToList();
            if (list.Count == 0)
                return result;

            using var doc = await RiotSendAsync(HttpMethod.Put, $"{Pd}/name-service/v2/players", JsonSerializer.Serialize(list));
            if (doc is null || doc.RootElement.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var e in doc.RootElement.EnumerateArray())
                result[Str(e, "Subject")] = $"{Str(e, "GameName")}#{Str(e, "TagLine")}";

            return result;
        }

        /// <summary>Current act rank, peak rank across all acts, and current act wins/games.</summary>
        public async Task<RankSummary> GetRankSummaryAsync(string puuid, string? currentSeasonId)
        {
            using var doc = await RiotGetAsync($"{Pd}/mmr/v1/players/{puuid}");
            if (doc is null)
                return new RankSummary(0, 0, 0, 0);

            int current = 0, peak = 0, wins = 0, games = 0;

            if (TryObj(doc.RootElement, "QueueSkills", out var qs) &&
                TryObj(qs, "competitive", out var comp) &&
                TryObj(comp, "SeasonalInfoBySeasonID", out var seasons))
            {
                foreach (var season in seasons.EnumerateObject())
                {
                    var info = season.Value;
                    if (info.ValueKind != JsonValueKind.Object)
                        continue;

                    int finalTier = Normalize(season.Name, Int(info, "CompetitiveTier"));
                    peak = Math.Max(peak, finalTier);

                    if (TryObj(info, "WinsByTier", out var byTier))
                        foreach (var t in byTier.EnumerateObject())
                            if (int.TryParse(t.Name, out var tier))
                                peak = Math.Max(peak, Normalize(season.Name, tier));

                    if (string.Equals(season.Name, currentSeasonId, StringComparison.OrdinalIgnoreCase))
                    {
                        current = finalTier;
                        wins = Int(info, "NumberOfWinsWithPlacements");
                        games = Int(info, "NumberOfGames");
                    }
                }
            }

            return new RankSummary(current, peak, wins, games);
        }

        /// <summary>K/D over the player's last few competitive matches, or null if none found.</summary>
        public async Task<double?> GetRecentKdAsync(string puuid, int matchCount = 5)
        {
            using var doc = await RiotGetAsync(
                $"{Pd}/mmr/v1/players/{puuid}/competitiveupdates?startIndex=0&endIndex={matchCount}&queue=competitive");
            if (doc is null || !doc.RootElement.TryGetProperty("Matches", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return null;

            var ids = arr.EnumerateArray().Select(m => Str(m, "MatchID")).Where(id => id != "").ToList();

            int kills = 0, deaths = 0, found = 0;
            foreach (var id in ids)
            {
                var stats = await GetMatchStatsAsync(id);
                if (stats.TryGetValue(puuid, out var s))
                {
                    kills += s.Kills;
                    deaths += s.Deaths;
                    found++;
                }
            }

            return found == 0 ? null : (double)kills / Math.Max(1, deaths);
        }

        // ================= Seasons and content =================

        /// <summary>ID of the active competitive act.</summary>
        public async Task<string?> GetCurrentSeasonIdAsync()
        {
            using var doc = await RiotGetAsync($"{Shared}/content-service/v3/content");
            if (doc is null || !doc.RootElement.TryGetProperty("Seasons", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return null;

            foreach (var s in arr.EnumerateArray())
                if (Str(s, "Type") == "act" && Bool(s, "IsActive"))
                    return Str(s, "ID");

            return null;
        }

        public AgentInfo GetAgent(string agentId) =>
            agents.TryGetValue(agentId ?? "", out var a) ? a : new AgentInfo("Unknown", "");

        public TierInfo GetTier(int tier) =>
            tiers.TryGetValue(tier, out var t) ? t : new TierInfo(0, "Unranked", tiers.TryGetValue(0, out var u) ? u.IconUrl : "");

        public string GetMapName(string mapPath) =>
            maps.TryGetValue(mapPath ?? "", out var name) ? name : "Unknown map";

        // ================= Internals =================

        private async Task RefreshTokensAsync()
        {
            if (local is null)
                throw new InvalidOperationException("Not connected.");

            using var doc = JsonDocument.Parse(await local.GetStringAsync("/entitlements/v1/token"));
            accessToken = Str(doc.RootElement, "accessToken");
            entitlement = Str(doc.RootElement, "token");
            Puuid = Str(doc.RootElement, "subject");
            tokenFetchedUtc = DateTime.UtcNow;
        }

        private void ReadRegionAndShard()
        {
            using var fs = new FileStream(ShooterLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            var match = Regex.Match(reader.ReadToEnd(), @"https://glz-([a-z0-9]+)-1\.([a-z0-9]+)\.a\.pvp\.net");

            if (!match.Success)
                throw new InvalidOperationException("Region not found in ShooterGame.log. Wait for the main menu.");

            Region = match.Groups[1].Value;
            Shard = match.Groups[2].Value;
        }

        private async Task LoadContentAsync()
        {
            using (var doc = await GetJsonAsync("https://valorant-api.com/v1/version"))
                clientVersion = Str(doc.RootElement.GetProperty("data"), "riotClientVersion");

            using (var doc = await GetJsonAsync("https://valorant-api.com/v1/agents?isPlayableCharacter=true"))
                foreach (var a in doc.RootElement.GetProperty("data").EnumerateArray())
                    agents[Str(a, "uuid")] = new AgentInfo(Str(a, "displayName"), Str(a, "displayIconSmall"));

            using (var doc = await GetJsonAsync("https://valorant-api.com/v1/maps"))
                foreach (var m in doc.RootElement.GetProperty("data").EnumerateArray())
                    if (Str(m, "mapUrl") is string url && url != "")
                        maps[url] = Str(m, "displayName");

            // Rank tables. Before Ascendant was added, tiers 21-24 were Immortal 1-3 / Radiant,
            // so those seasons' tiers are shifted up by 3 to compare with today's table.
            var preAscendantTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var doc = await GetJsonAsync("https://valorant-api.com/v1/competitivetiers"))
            {
                JsonElement latest = default;
                foreach (var table in doc.RootElement.GetProperty("data").EnumerateArray())
                {
                    latest = table;
                    foreach (var t in table.GetProperty("tiers").EnumerateArray())
                        if (Int(t, "tier") == 21 && Str(t, "tierName").Contains("IMMORTAL", StringComparison.OrdinalIgnoreCase))
                            preAscendantTables.Add(Str(table, "uuid"));
                }

                var newTiers = new Dictionary<int, TierInfo>();
                if (latest.ValueKind == JsonValueKind.Object)
                    foreach (var t in latest.GetProperty("tiers").EnumerateArray())
                        newTiers[Int(t, "tier")] = new TierInfo(Int(t, "tier"), TitleCase(Str(t, "tierName")), Str(t, "smallIcon"));
                tiers = newTiers;
            }

            using (var doc = await GetJsonAsync("https://valorant-api.com/v1/seasons/competitive"))
                foreach (var s in doc.RootElement.GetProperty("data").EnumerateArray())
                    if (preAscendantTables.Contains(Str(s, "competitiveTiersUuid")))
                        preAscendantSeasons.Add(Str(s, "seasonUuid"));
        }

        private int Normalize(string seasonId, int tier) =>
            preAscendantSeasons.Contains(seasonId) && tier >= 21 ? tier + 3 : tier;

        private Task<Dictionary<string, (int Kills, int Deaths)>> GetMatchStatsAsync(string matchId)
        {
            var task = matchStats.GetOrAdd(matchId, FetchMatchStatsAsync);
            if (task.IsFaulted)
                matchStats.TryRemove(matchId, out _);
            return task;
        }

        private async Task<Dictionary<string, (int Kills, int Deaths)>> FetchMatchStatsAsync(string matchId)
        {
            var result = new Dictionary<string, (int, int)>();
            using var doc = await RiotGetAsync($"{Pd}/match-details/v1/matches/{matchId}");
            if (doc is null || !doc.RootElement.TryGetProperty("players", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return result;

            foreach (var p in arr.EnumerateArray())
                if (TryObj(p, "stats", out var stats))
                    result[Str(p, "subject")] = (Int(stats, "kills"), Int(stats, "deaths"));

            return result;
        }

        private Task<JsonDocument?> RiotGetAsync(string url) => RiotSendAsync(HttpMethod.Get, url, null);

        private async Task<JsonDocument?> RiotSendAsync(HttpMethod method, string url, string? body)
        {
            if (DateTime.UtcNow - tokenFetchedUtc > TimeSpan.FromMinutes(50))
                await RefreshTokensAsync();

            await throttle.WaitAsync();
            try
            {
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    using var req = new HttpRequestMessage(method, url);
                    if (body != null)
                        req.Content = new StringContent(body, Encoding.UTF8, "application/json");

                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    req.Headers.Add("X-Riot-Entitlements-JWT", entitlement);
                    req.Headers.Add("X-Riot-ClientPlatform", ClientPlatform);
                    req.Headers.Add("X-Riot-ClientVersion", clientVersion);

                    using var res = await remote.SendAsync(req);

                    if (res.StatusCode == HttpStatusCode.NotFound)
                        return null; // e.g. not in pregame / not in a game

                    if (res.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(res.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(3));
                        continue;
                    }

                    if (res.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.BadRequest)
                    {
                        await RefreshTokensAsync(); // token rotated
                        continue;
                    }

                    res.EnsureSuccessStatusCode();
                    return JsonDocument.Parse(await res.Content.ReadAsStringAsync());
                }
                return null;
            }
            finally
            {
                throttle.Release();
            }
        }

        private async Task<JsonDocument> GetJsonAsync(string url) =>
            JsonDocument.Parse(await remote.GetStringAsync(url));

        // JSON helpers
        private static string Str(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";

        private static int Int(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : 0;

        private static bool Bool(JsonElement e, string name) =>
            e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;

        private static bool TryObj(JsonElement e, string name, out JsonElement value) =>
            e.TryGetProperty(name, out value) && value.ValueKind == JsonValueKind.Object;

        private static string? NullIfEmpty(string s) => s == "" ? null : s;

        private static string TitleCase(string s) =>
            CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLowerInvariant());

        public void Dispose()
        {
            Disconnect();
            remote.Dispose();
            throttle.Dispose();
        }
    }
}