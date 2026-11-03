using System.Runtime.InteropServices;

namespace valstat
{
    public partial class valstat : Form
    {
        // Set to true to load your last finished match instead of waiting for a live one
        private const bool TestMode = false;

        // Variables
        private Point mouseLocation;
        private readonly RiotClient riot = new();
        private readonly PlayerRow[] allyRows;
        private readonly PlayerRow[] enemyRows;
        private readonly Dictionary<string, PlayerRow> rowByPuuid = new();
        private readonly Dictionary<string, Task<(RankSummary Rank, double? Kd)>> statsCache = new();

        private MatchPhase phase = MatchPhase.Menus;
        private string? pregameMatchId;
        private string? coreMatchId;
        private string? currentSeasonId;
        private bool busy;
        private DateTime lastHistoryCheck = DateTime.MinValue;

        private static readonly Color AllyColor = Color.FromArgb(72, 180, 150);
        private static readonly Color EnemyColor = Color.IndianRed;

        // Win32 (show on top without stealing focus from the game)
        private static readonly IntPtr HWND_TOPMOST = new(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new(-2);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOWMINNOACTIVE = 7;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_NOREPEAT = 0x4000;
        private const int ToggleHotkeyId = 1;

        // Press this anywhere (even in game) to show or minimize valstat without leaving VALORANT
        private const Keys ToggleKey = Keys.F6;

        private bool overlayShown;

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        // Never take focus: showing, clicking or dragging valstat keeps you in the game
        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_APPWINDOW; // APPWINDOW keeps the taskbar button
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterHotKey(Handle, ToggleHotkeyId, MOD_NOREPEAT, (uint)ToggleKey);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            UnregisterHotKey(Handle, ToggleHotkeyId);
            base.OnHandleDestroyed(e);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == ToggleHotkeyId)
            {
                if (overlayShown && WindowState != FormWindowState.Minimized)
                    MinimizeOverlay();
                else
                    ShowOverlay(center: false);
            }

            base.WndProc(ref m);
        }

        // Constructors
        public valstat()
        {
            InitializeComponent();
            allyRows = new[] { AllyRow1, AllyRow2, AllyRow3, AllyRow4, AllyRow5 };
            enemyRows = new[] { EnemyRow1, EnemyRow2, EnemyRow3, EnemyRow4, EnemyRow5 };
            ClearAllRows();
        }

        // Form events
        private async void valstat_Load(object sender, EventArgs e)
        {
            await TryConnectAsync();
            PollTimer.Start();
        }

        private void valstat_FormClosing(object sender, FormClosingEventArgs e)
        {
            PollTimer.Stop();
            riot.Dispose();
        }

        private async void PollTimer_Tick(object sender, EventArgs e)
        {
            if (busy)
                return;
            busy = true;

            try
            {
                KeepOnTop();

                if (!await TryConnectAsync())
                    return;

                if (TestMode)
                {
                    // Check match history every 30 seconds and reload when a newer match shows up
                    if (DateTime.Now - lastHistoryCheck >= TimeSpan.FromSeconds(30))
                    {
                        lastHistoryCheck = DateTime.Now;
                        var latestId = await riot.GetLatestMatchIdAsync();

                        if (latestId == null)
                            StatusLabel.Text = $"Test mode: no recent match found (checked {DateTime.Now:h:mm:ss tt})";
                        else if (latestId != coreMatchId)
                            await LoadTestMatchAsync(latestId);
                    }
                    return;
                }

                // In game: minimize and load both teams
                var coreId = await riot.GetCoreGameMatchIdAsync();
                if (coreId != null)
                {
                    if (phase != MatchPhase.InGame)
                    {
                        phase = MatchPhase.InGame;
                        MinimizeOverlay();
                    }

                    if (coreId != coreMatchId)
                        await LoadCoreGameAsync(coreId);
                    return;
                }

                // Agent select: pop up on top, centered
                var preId = await riot.GetPregameMatchIdAsync();
                if (preId != null)
                {
                    if (preId != pregameMatchId)
                    {
                        // New match: forget everything from the previous one
                        coreMatchId = null;
                        statsCache.Clear();
                        phase = MatchPhase.AgentSelect;
                        ShowOverlay();
                        await LoadPregameAsync(preId);
                    }
                    else
                    {
                        await UpdatePregameAgentsAsync(preId);
                    }
                    return;
                }

                // Menus: reset for the next match
                if (phase != MatchPhase.Menus)
                {
                    phase = MatchPhase.Menus;
                    pregameMatchId = null;
                    coreMatchId = null;
                    statsCache.Clear();
                    ClearAllRows();
                    ReleaseOverlay();
                }
                StatusLabel.Text = $"In menus - waiting for a match (checked {DateTime.Now:h:mm:ss tt})";
            }
            catch (HttpRequestException)
            {
                riot.Disconnect(); // Riot Client restarted, reconnect next tick
                StatusLabel.Text = "Lost connection, retrying...";
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Error: " + ex.Message;
            }
            finally
            {
                busy = false;
            }
        }

        // Riot
        private async Task<bool> TryConnectAsync()
        {
            if (riot.IsConnected)
                return true;

            try
            {
                if (!await riot.ConnectAsync())
                {
                    StatusLabel.Text = "Waiting for VALORANT...";
                    return false;
                }

                currentSeasonId = await riot.GetCurrentSeasonIdAsync();
                StatusLabel.Text = $"Connected ({riot.Region.ToUpper()})";
                return true;
            }
            catch (Exception ex)
            {
                StatusLabel.Text = "Not ready: " + ex.Message;
                return false;
            }
        }

        private async Task LoadPregameAsync(string matchId)
        {
            pregameMatchId = matchId;

            var match = await riot.GetPregameMatchAsync(matchId);
            if (match is null)
                return;

            StatusLabel.Text = $"Agent select - {match.MapName} - enemy team shows when the match starts";
            ClearAllRows();

            var names = await riot.GetUsernamesAsync(match.Players.Where(p => !IsPrivate(p)).Select(p => p.Puuid));
            await Task.WhenAll(FillTeam(allyRows, match.Players, AllyColor, names));
        }

        private async Task UpdatePregameAgentsAsync(string matchId)
        {
            var match = await riot.GetPregameMatchAsync(matchId);
            if (match is null)
                return;

            foreach (var p in match.Players)
                if (rowByPuuid.TryGetValue(p.Puuid, out var row))
                    row.SetAgent(riot.GetAgent(p.AgentId), p.SelectionState);
        }

        private async Task LoadCoreGameAsync(string matchId)
        {
            coreMatchId = matchId;

            var match = await riot.GetCoreGameMatchAsync(matchId);
            if (match is null)
                return;

            await ShowMatchAsync(match, $"In game - {match.MapName}");
        }

        private async Task LoadTestMatchAsync(string matchId)
        {
            var match = await riot.GetFinishedMatchAsLiveAsync(matchId);
            if (match is null)
            {
                StatusLabel.Text = "Test mode: couldn't load the latest match, retrying...";
                return; // coreMatchId unchanged, so the next history check tries again
            }

            coreMatchId = matchId;
            statsCache.Clear();

            await ShowMatchAsync(match, $"Test mode - {match.MapName}");
        }

        private async Task ShowMatchAsync(LiveMatch match, string status)
        {
            StatusLabel.Text = status + " - loading stats...";
            ClearAllRows();

            // Your team on top, enemies below
            var myTeam = match.Players.FirstOrDefault(p => p.Puuid == riot.Puuid)?.TeamId;
            var allies = match.Players.Where(p => p.TeamId == myTeam).Take(5).ToList();
            var enemies = match.Players.Where(p => p.TeamId != myTeam).Take(5).ToList();

            // Never resolve names for players who hid them (streamer mode)
            var names = await riot.GetUsernamesAsync(match.Players.Where(p => !IsPrivate(p)).Select(p => p.Puuid));

            var loads = new List<Task>();
            loads.AddRange(FillTeam(allyRows, allies, AllyColor, names));
            loads.AddRange(FillTeam(enemyRows, enemies, EnemyColor, names));
            await Task.WhenAll(loads);

            StatusLabel.Text = status;
        }

        private List<Task> FillTeam(PlayerRow[] rows, List<MatchPlayer> players, Color color, Dictionary<string, string> names)
        {
            var loads = new List<Task>();

            for (int i = 0; i < rows.Length && i < players.Count; i++)
            {
                var p = players[i];
                var row = rows[i];
                bool isPrivate = IsPrivate(p);
                var agent = riot.GetAgent(p.AgentId);

                // Private players go by their agent name, like in-game streamer mode
                string name = isPrivate
                    ? (string.IsNullOrEmpty(p.AgentId) ? "Picking..." : agent.Name)
                    : names.GetValueOrDefault(p.Puuid, "Unknown");

                row.Tag = p.Puuid;
                rowByPuuid[p.Puuid] = row;
                row.SetPlayer(name, agent, color, isPrivate);

                if (phase == MatchPhase.AgentSelect)
                    row.SetAgent(agent, p.SelectionState);

                if (!isPrivate)
                    loads.Add(LoadStatsAsync(row, p.Puuid));
            }

            return loads;
        }

        private async Task LoadStatsAsync(PlayerRow row, string puuid)
        {
            try
            {
                // Cached per match, so your teammates' stats from agent select show instantly in game
                if (!statsCache.TryGetValue(puuid, out var task) || task.IsFaulted)
                {
                    task = FetchStatsAsync(puuid);
                    statsCache[puuid] = task;
                }

                var (rank, kd) = await task;

                if (row.Tag as string == puuid) // row may have been reused for another player
                    row.SetStats(riot.GetTier(rank.CurrentTier), riot.GetTier(rank.PeakTier), kd, rank.WinRate);
            }
            catch
            {
                if (row.Tag as string == puuid)
                    row.SetError();
            }
        }

        private async Task<(RankSummary Rank, double? Kd)> FetchStatsAsync(string puuid)
        {
            var rank = await riot.GetRankSummaryAsync(puuid, currentSeasonId);
            var kd = await riot.GetRecentKdAsync(puuid, 5);
            return (rank, kd);
        }

        private bool IsPrivate(MatchPlayer p) => p.Incognito && p.Puuid != riot.Puuid;

        private void ClearAllRows()
        {
            rowByPuuid.Clear();
            foreach (var row in allyRows.Concat(enemyRows))
            {
                row.Tag = null;
                row.ClearRow();
            }
        }

        // Overlay window behavior
        private void ShowOverlay(bool center = true)
        {
            // Restore without taking focus away from VALORANT
            ShowWindow(Handle, SW_SHOWNOACTIVATE);

            if (center)
            {
                var screen = Screen.PrimaryScreen?.Bounds ?? Screen.FromControl(this).Bounds;
                int x = screen.Left + (screen.Width - Width) / 2;
                int y = screen.Top + (screen.Height - Height) / 2;
                SetWindowPos(Handle, HWND_TOPMOST, x, y, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }
            else
            {
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
            }

            overlayShown = true;
        }

        // VALORANT can push itself back in front, so re-claim the top spot while showing
        private void KeepOnTop()
        {
            if (overlayShown && WindowState != FormWindowState.Minimized)
                SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void MinimizeOverlay()
        {
            overlayShown = false;
            SetWindowPos(Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            ShowWindow(Handle, SW_SHOWMINNOACTIVE);
        }

        private void ReleaseOverlay()
        {
            overlayShown = false;
            SetWindowPos(Handle, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        // Window chrome
        private void TopBar_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = new Point(-e.X, -e.Y);
        }

        private void TopBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePose = Control.MousePosition;
                mousePose.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePose;
            }
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MinimizeButton_Click(object sender, EventArgs e)
        {
            MinimizeOverlay();
        }
    }
}