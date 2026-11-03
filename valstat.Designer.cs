namespace valstat
{
    partial class valstat
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(valstat));
            TopBar = new Panel();
            Title = new Label();
            MinimizeButton = new Button();
            ExitButton = new Button();
            HeaderPanel = new Panel();
            PlayerHeader = new Label();
            CurrentRankHeader = new Label();
            PeakRankHeader = new Label();
            KdHeader = new Label();
            WinHeader = new Label();
            AllyTeamLabel = new Label();
            EnemyTeamLabel = new Label();
            AllyRow1 = new PlayerRow();
            AllyRow2 = new PlayerRow();
            AllyRow3 = new PlayerRow();
            AllyRow4 = new PlayerRow();
            AllyRow5 = new PlayerRow();
            EnemyRow1 = new PlayerRow();
            EnemyRow2 = new PlayerRow();
            EnemyRow3 = new PlayerRow();
            EnemyRow4 = new PlayerRow();
            EnemyRow5 = new PlayerRow();
            StatusLabel = new Label();
            PollTimer = new System.Windows.Forms.Timer(components);
            TopBar.SuspendLayout();
            HeaderPanel.SuspendLayout();
            SuspendLayout();
            // 
            // TopBar
            // 
            TopBar.BackColor = Color.IndianRed;
            TopBar.Controls.Add(Title);
            TopBar.Controls.Add(MinimizeButton);
            TopBar.Controls.Add(ExitButton);
            TopBar.Location = new Point(-8, -9);
            TopBar.Name = "TopBar";
            TopBar.Size = new Size(816, 38);
            TopBar.TabIndex = 0;
            TopBar.MouseDown += TopBar_MouseDown;
            TopBar.MouseMove += TopBar_MouseMove;
            // 
            // Title
            // 
            Title.AutoSize = true;
            Title.BackColor = Color.Transparent;
            Title.Font = new Font("G-OTF Shin Go Pro B", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 128);
            Title.ForeColor = Color.White;
            Title.Location = new Point(3, 6);
            Title.Name = "Title";
            Title.Size = new Size(105, 29);
            Title.TabIndex = 1;
            Title.Text = " valstat";
            // 
            // MinimizeButton
            // 
            MinimizeButton.BackColor = Color.Transparent;
            MinimizeButton.FlatAppearance.BorderSize = 0;
            MinimizeButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            MinimizeButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            MinimizeButton.FlatStyle = FlatStyle.Flat;
            MinimizeButton.Font = new Font("Noto Sans Lisu", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            MinimizeButton.ForeColor = Color.White;
            MinimizeButton.Location = new Point(753, -6);
            MinimizeButton.Name = "MinimizeButton";
            MinimizeButton.Size = new Size(33, 41);
            MinimizeButton.TabIndex = 2;
            MinimizeButton.Text = "-";
            MinimizeButton.TextAlign = ContentAlignment.TopCenter;
            MinimizeButton.UseVisualStyleBackColor = false;
            MinimizeButton.Click += MinimizeButton_Click;
            // 
            // ExitButton
            // 
            ExitButton.BackColor = Color.Transparent;
            ExitButton.FlatAppearance.BorderSize = 0;
            ExitButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
            ExitButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            ExitButton.FlatStyle = FlatStyle.Flat;
            ExitButton.Font = new Font("Noto Sans Lisu", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            ExitButton.ForeColor = Color.White;
            ExitButton.Location = new Point(780, 0);
            ExitButton.Name = "ExitButton";
            ExitButton.Size = new Size(33, 44);
            ExitButton.TabIndex = 1;
            ExitButton.Text = "X";
            ExitButton.UseVisualStyleBackColor = false;
            ExitButton.Click += ExitButton_Click;
            // 
            // HeaderPanel
            // 
            HeaderPanel.BackColor = Color.FromArgb(25, 25, 25);
            HeaderPanel.Controls.Add(PlayerHeader);
            HeaderPanel.Controls.Add(CurrentRankHeader);
            HeaderPanel.Controls.Add(PeakRankHeader);
            HeaderPanel.Controls.Add(KdHeader);
            HeaderPanel.Controls.Add(WinHeader);
            HeaderPanel.Location = new Point(0, 34);
            HeaderPanel.Name = "HeaderPanel";
            HeaderPanel.Size = new Size(800, 22);
            HeaderPanel.TabIndex = 3;
            // 
            // PlayerHeader
            // 
            PlayerHeader.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            PlayerHeader.ForeColor = Color.FromArgb(150, 150, 150);
            PlayerHeader.Location = new Point(12, 0);
            PlayerHeader.Name = "PlayerHeader";
            PlayerHeader.Size = new Size(240, 22);
            PlayerHeader.TabIndex = 0;
            PlayerHeader.Text = "AGENT / PLAYER";
            PlayerHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // CurrentRankHeader
            // 
            CurrentRankHeader.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            CurrentRankHeader.ForeColor = Color.FromArgb(150, 150, 150);
            CurrentRankHeader.Location = new Point(270, 0);
            CurrentRankHeader.Name = "CurrentRankHeader";
            CurrentRankHeader.Size = new Size(170, 22);
            CurrentRankHeader.TabIndex = 1;
            CurrentRankHeader.Text = "CURRENT RANK";
            CurrentRankHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PeakRankHeader
            // 
            PeakRankHeader.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            PeakRankHeader.ForeColor = Color.FromArgb(150, 150, 150);
            PeakRankHeader.Location = new Point(450, 0);
            PeakRankHeader.Name = "PeakRankHeader";
            PeakRankHeader.Size = new Size(160, 22);
            PeakRankHeader.TabIndex = 2;
            PeakRankHeader.Text = "PEAK RANK";
            PeakRankHeader.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // KdHeader
            // 
            KdHeader.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            KdHeader.ForeColor = Color.FromArgb(150, 150, 150);
            KdHeader.Location = new Point(620, 0);
            KdHeader.Name = "KdHeader";
            KdHeader.Size = new Size(70, 22);
            KdHeader.TabIndex = 3;
            KdHeader.Text = "K/D (L5)";
            KdHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // WinHeader
            // 
            WinHeader.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            WinHeader.ForeColor = Color.FromArgb(150, 150, 150);
            WinHeader.Location = new Point(700, 0);
            WinHeader.Name = "WinHeader";
            WinHeader.Size = new Size(80, 22);
            WinHeader.TabIndex = 4;
            WinHeader.Text = "WIN %";
            WinHeader.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // AllyTeamLabel
            // 
            AllyTeamLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            AllyTeamLabel.ForeColor = Color.FromArgb(72, 180, 150);
            AllyTeamLabel.Location = new Point(12, 58);
            AllyTeamLabel.Name = "AllyTeamLabel";
            AllyTeamLabel.Size = new Size(200, 22);
            AllyTeamLabel.TabIndex = 4;
            AllyTeamLabel.Text = "YOUR TEAM";
            AllyTeamLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // EnemyTeamLabel
            // 
            EnemyTeamLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            EnemyTeamLabel.ForeColor = Color.IndianRed;
            EnemyTeamLabel.Location = new Point(12, 276);
            EnemyTeamLabel.Name = "EnemyTeamLabel";
            EnemyTeamLabel.Size = new Size(200, 22);
            EnemyTeamLabel.TabIndex = 10;
            EnemyTeamLabel.Text = "ENEMY TEAM";
            EnemyTeamLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // AllyRow1
            // 
            AllyRow1.BackColor = Color.FromArgb(32, 32, 32);
            AllyRow1.Location = new Point(0, 82);
            AllyRow1.Name = "AllyRow1";
            AllyRow1.Size = new Size(800, 36);
            AllyRow1.TabIndex = 5;
            // 
            // AllyRow2
            // 
            AllyRow2.BackColor = Color.FromArgb(38, 38, 38);
            AllyRow2.Location = new Point(0, 120);
            AllyRow2.Name = "AllyRow2";
            AllyRow2.Size = new Size(800, 36);
            AllyRow2.TabIndex = 6;
            // 
            // AllyRow3
            // 
            AllyRow3.BackColor = Color.FromArgb(32, 32, 32);
            AllyRow3.Location = new Point(0, 158);
            AllyRow3.Name = "AllyRow3";
            AllyRow3.Size = new Size(800, 36);
            AllyRow3.TabIndex = 7;
            // 
            // AllyRow4
            // 
            AllyRow4.BackColor = Color.FromArgb(38, 38, 38);
            AllyRow4.Location = new Point(0, 196);
            AllyRow4.Name = "AllyRow4";
            AllyRow4.Size = new Size(800, 36);
            AllyRow4.TabIndex = 8;
            // 
            // AllyRow5
            // 
            AllyRow5.BackColor = Color.FromArgb(32, 32, 32);
            AllyRow5.Location = new Point(0, 234);
            AllyRow5.Name = "AllyRow5";
            AllyRow5.Size = new Size(800, 36);
            AllyRow5.TabIndex = 9;
            // 
            // EnemyRow1
            // 
            EnemyRow1.BackColor = Color.FromArgb(32, 32, 32);
            EnemyRow1.Location = new Point(0, 300);
            EnemyRow1.Name = "EnemyRow1";
            EnemyRow1.Size = new Size(800, 36);
            EnemyRow1.TabIndex = 11;
            // 
            // EnemyRow2
            // 
            EnemyRow2.BackColor = Color.FromArgb(38, 38, 38);
            EnemyRow2.Location = new Point(0, 338);
            EnemyRow2.Name = "EnemyRow2";
            EnemyRow2.Size = new Size(800, 36);
            EnemyRow2.TabIndex = 12;
            // 
            // EnemyRow3
            // 
            EnemyRow3.BackColor = Color.FromArgb(32, 32, 32);
            EnemyRow3.Location = new Point(0, 376);
            EnemyRow3.Name = "EnemyRow3";
            EnemyRow3.Size = new Size(800, 36);
            EnemyRow3.TabIndex = 13;
            // 
            // EnemyRow4
            // 
            EnemyRow4.BackColor = Color.FromArgb(38, 38, 38);
            EnemyRow4.Location = new Point(0, 414);
            EnemyRow4.Name = "EnemyRow4";
            EnemyRow4.Size = new Size(800, 36);
            EnemyRow4.TabIndex = 14;
            // 
            // EnemyRow5
            // 
            EnemyRow5.BackColor = Color.FromArgb(32, 32, 32);
            EnemyRow5.Location = new Point(0, 452);
            EnemyRow5.Name = "EnemyRow5";
            EnemyRow5.Size = new Size(800, 36);
            EnemyRow5.TabIndex = 15;
            // 
            // StatusLabel
            // 
            StatusLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            StatusLabel.ForeColor = Color.FromArgb(150, 150, 150);
            StatusLabel.Location = new Point(12, 494);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(776, 22);
            StatusLabel.TabIndex = 16;
            StatusLabel.Text = "Starting...";
            StatusLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PollTimer
            // 
            PollTimer.Interval = 5000;
            PollTimer.Tick += PollTimer_Tick;
            // 
            // valstat
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(25, 25, 25);
            ClientSize = new Size(800, 520);
            Controls.Add(StatusLabel);
            Controls.Add(EnemyRow5);
            Controls.Add(EnemyRow4);
            Controls.Add(EnemyRow3);
            Controls.Add(EnemyRow2);
            Controls.Add(EnemyRow1);
            Controls.Add(EnemyTeamLabel);
            Controls.Add(AllyRow5);
            Controls.Add(AllyRow4);
            Controls.Add(AllyRow3);
            Controls.Add(AllyRow2);
            Controls.Add(AllyRow1);
            Controls.Add(AllyTeamLabel);
            Controls.Add(HeaderPanel);
            Controls.Add(TopBar);
            ForeColor = SystemColors.ControlText;
            FormBorderStyle = FormBorderStyle.None;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "valstat";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "valstat";
            FormClosing += valstat_FormClosing;
            Load += valstat_Load;
            TopBar.ResumeLayout(false);
            TopBar.PerformLayout();
            HeaderPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel TopBar;
        private Button ExitButton;
        private Button MinimizeButton;
        private Label Title;
        private Panel HeaderPanel;
        private Label PlayerHeader;
        private Label CurrentRankHeader;
        private Label PeakRankHeader;
        private Label KdHeader;
        private Label WinHeader;
        private Label AllyTeamLabel;
        private Label EnemyTeamLabel;
        private PlayerRow AllyRow1;
        private PlayerRow AllyRow2;
        private PlayerRow AllyRow3;
        private PlayerRow AllyRow4;
        private PlayerRow AllyRow5;
        private PlayerRow EnemyRow1;
        private PlayerRow EnemyRow2;
        private PlayerRow EnemyRow3;
        private PlayerRow EnemyRow4;
        private PlayerRow EnemyRow5;
        private Label StatusLabel;
        private System.Windows.Forms.Timer PollTimer;
    }
}