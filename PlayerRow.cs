namespace valstat
{
    public partial class PlayerRow : UserControl
    {
        // Colors
        private static readonly Color Good = Color.FromArgb(72, 180, 150);
        private static readonly Color Bad = Color.IndianRed;
        private static readonly Color Muted = Color.FromArgb(150, 150, 150);
        private static readonly Color EmptyAccent = Color.FromArgb(45, 45, 45);

        private bool isPrivate;

        // Constructors
        public PlayerRow()
        {
            InitializeComponent();
            ClearRow();
        }

        // Methods
        public void ClearRow()
        {
            isPrivate = false;
            AccentBar.BackColor = EmptyAccent;
            SetImage(AgentIcon, "");
            NameLabel.Text = "";
            AgentLabel.Text = "";
            PrivateLabel.Visible = false;
            SetStatsVisible(true);
            SetImage(CurrentRankIcon, "");
            SetImage(PeakRankIcon, "");
            CurrentRankLabel.Text = "";
            PeakRankLabel.Text = "";
            KdLabel.Text = "";
            WinLabel.Text = "";
        }

        public void SetPlayer(string name, AgentInfo agent, Color teamColor, bool isPrivate)
        {
            this.isPrivate = isPrivate;
            AccentBar.BackColor = teamColor;
            NameLabel.Text = name;
            AgentLabel.Text = agent.Name;
            SetImage(AgentIcon, agent.IconUrl);

            PrivateLabel.Visible = isPrivate;
            SetStatsVisible(!isPrivate);

            if (!isPrivate)
            {
                CurrentRankLabel.Text = "Loading...";
                PeakRankLabel.Text = "Loading...";
                CurrentRankLabel.ForeColor = Muted;
                PeakRankLabel.ForeColor = Muted;
                KdLabel.Text = "...";
                WinLabel.Text = "...";
                KdLabel.ForeColor = Muted;
                WinLabel.ForeColor = Muted;
            }
        }

        /// <summary>Agent select: update the agent as the player hovers or locks in.</summary>
        public void SetAgent(AgentInfo agent, string selectionState)
        {
            string text = selectionState switch
            {
                "locked" => agent.Name,
                "selected" => agent.Name + " (hovering)",
                _ => "Picking..."
            };

            // Private players go by their agent name, like in-game streamer mode
            if (isPrivate)
                NameLabel.Text = selectionState == "" ? "Picking..." : agent.Name;

            if (AgentLabel.Text == text)
                return;

            AgentLabel.Text = text;
            SetImage(AgentIcon, selectionState == "" ? "" : agent.IconUrl);
        }

        public void SetStats(TierInfo current, TierInfo peak, double? kd, double? winRate)
        {
            SetImage(CurrentRankIcon, current.IconUrl);
            SetImage(PeakRankIcon, peak.IconUrl);
            CurrentRankLabel.Text = current.Name;
            PeakRankLabel.Text = peak.Name;
            CurrentRankLabel.ForeColor = Color.White;
            PeakRankLabel.ForeColor = Color.White;

            KdLabel.Text = kd is null ? "—" : kd.Value.ToString("0.00");
            KdLabel.ForeColor = kd is null ? Muted : kd >= 1.0 ? Good : Bad;

            WinLabel.Text = winRate is null ? "—" : $"{winRate:0}%";
            WinLabel.ForeColor = winRate is null ? Muted : winRate >= 50 ? Good : Bad;
        }

        public void SetError()
        {
            CurrentRankLabel.Text = "Unavailable";
            PeakRankLabel.Text = "Unavailable";
            KdLabel.Text = "—";
            WinLabel.Text = "—";
        }

        private void SetStatsVisible(bool visible)
        {
            CurrentRankIcon.Visible = visible;
            CurrentRankLabel.Visible = visible;
            PeakRankIcon.Visible = visible;
            PeakRankLabel.Visible = visible;
            KdLabel.Visible = visible;
            WinLabel.Visible = visible;
        }

        private static void SetImage(PictureBox box, string url)
        {
            box.CancelAsync(); // stop any icon still loading from a previous match
            box.Image = null;
            if (string.IsNullOrEmpty(url))
                box.ImageLocation = null;
            else
                box.LoadAsync(url);
        }
    }
}