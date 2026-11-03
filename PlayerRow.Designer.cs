namespace valstat
{
    partial class PlayerRow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            AccentBar = new Panel();
            AgentIcon = new PictureBox();
            NameLabel = new Label();
            AgentLabel = new Label();
            CurrentRankIcon = new PictureBox();
            CurrentRankLabel = new Label();
            PeakRankIcon = new PictureBox();
            PeakRankLabel = new Label();
            KdLabel = new Label();
            WinLabel = new Label();
            PrivateLabel = new Label();
            ((System.ComponentModel.ISupportInitialize)AgentIcon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)CurrentRankIcon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)PeakRankIcon).BeginInit();
            SuspendLayout();
            // 
            // AccentBar
            // 
            AccentBar.BackColor = Color.IndianRed;
            AccentBar.Location = new Point(0, 0);
            AccentBar.Name = "AccentBar";
            AccentBar.Size = new Size(4, 36);
            AccentBar.TabIndex = 0;
            // 
            // AgentIcon
            // 
            AgentIcon.Location = new Point(12, 2);
            AgentIcon.Name = "AgentIcon";
            AgentIcon.Size = new Size(32, 32);
            AgentIcon.SizeMode = PictureBoxSizeMode.Zoom;
            AgentIcon.TabIndex = 1;
            AgentIcon.TabStop = false;
            // 
            // NameLabel
            // 
            NameLabel.AutoEllipsis = true;
            NameLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            NameLabel.ForeColor = Color.White;
            NameLabel.Location = new Point(52, 1);
            NameLabel.Name = "NameLabel";
            NameLabel.Size = new Size(205, 19);
            NameLabel.TabIndex = 2;
            NameLabel.Text = "Player#TAG";
            // 
            // AgentLabel
            // 
            AgentLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            AgentLabel.ForeColor = Color.FromArgb(150, 150, 150);
            AgentLabel.Location = new Point(52, 19);
            AgentLabel.Name = "AgentLabel";
            AgentLabel.Size = new Size(205, 15);
            AgentLabel.TabIndex = 3;
            AgentLabel.Text = "Agent";
            // 
            // CurrentRankIcon
            // 
            CurrentRankIcon.Location = new Point(270, 4);
            CurrentRankIcon.Name = "CurrentRankIcon";
            CurrentRankIcon.Size = new Size(28, 28);
            CurrentRankIcon.SizeMode = PictureBoxSizeMode.Zoom;
            CurrentRankIcon.TabIndex = 4;
            CurrentRankIcon.TabStop = false;
            // 
            // CurrentRankLabel
            // 
            CurrentRankLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            CurrentRankLabel.ForeColor = Color.White;
            CurrentRankLabel.Location = new Point(302, 0);
            CurrentRankLabel.Name = "CurrentRankLabel";
            CurrentRankLabel.Size = new Size(140, 36);
            CurrentRankLabel.TabIndex = 5;
            CurrentRankLabel.Text = "Unranked";
            CurrentRankLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PeakRankIcon
            // 
            PeakRankIcon.Location = new Point(450, 4);
            PeakRankIcon.Name = "PeakRankIcon";
            PeakRankIcon.Size = new Size(28, 28);
            PeakRankIcon.SizeMode = PictureBoxSizeMode.Zoom;
            PeakRankIcon.TabIndex = 6;
            PeakRankIcon.TabStop = false;
            // 
            // PeakRankLabel
            // 
            PeakRankLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            PeakRankLabel.ForeColor = Color.White;
            PeakRankLabel.Location = new Point(482, 0);
            PeakRankLabel.Name = "PeakRankLabel";
            PeakRankLabel.Size = new Size(130, 36);
            PeakRankLabel.TabIndex = 7;
            PeakRankLabel.Text = "Unranked";
            PeakRankLabel.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // KdLabel
            // 
            KdLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            KdLabel.ForeColor = Color.White;
            KdLabel.Location = new Point(620, 0);
            KdLabel.Name = "KdLabel";
            KdLabel.Size = new Size(70, 36);
            KdLabel.TabIndex = 8;
            KdLabel.Text = "0.00";
            KdLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // WinLabel
            // 
            WinLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            WinLabel.ForeColor = Color.White;
            WinLabel.Location = new Point(700, 0);
            WinLabel.Name = "WinLabel";
            WinLabel.Size = new Size(80, 36);
            WinLabel.TabIndex = 9;
            WinLabel.Text = "0%";
            WinLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // PrivateLabel
            // 
            PrivateLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            PrivateLabel.ForeColor = Color.FromArgb(150, 150, 150);
            PrivateLabel.Location = new Point(270, 0);
            PrivateLabel.Name = "PrivateLabel";
            PrivateLabel.Size = new Size(510, 36);
            PrivateLabel.TabIndex = 10;
            PrivateLabel.Text = "Profile Set to Private";
            PrivateLabel.TextAlign = ContentAlignment.MiddleCenter;
            PrivateLabel.Visible = false;
            // 
            // PlayerRow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(32, 32, 32);
            Controls.Add(PrivateLabel);
            Controls.Add(WinLabel);
            Controls.Add(KdLabel);
            Controls.Add(PeakRankLabel);
            Controls.Add(PeakRankIcon);
            Controls.Add(CurrentRankLabel);
            Controls.Add(CurrentRankIcon);
            Controls.Add(AgentLabel);
            Controls.Add(NameLabel);
            Controls.Add(AgentIcon);
            Controls.Add(AccentBar);
            Name = "PlayerRow";
            Size = new Size(800, 36);
            ((System.ComponentModel.ISupportInitialize)AgentIcon).EndInit();
            ((System.ComponentModel.ISupportInitialize)CurrentRankIcon).EndInit();
            ((System.ComponentModel.ISupportInitialize)PeakRankIcon).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel AccentBar;
        private PictureBox AgentIcon;
        private Label NameLabel;
        private Label AgentLabel;
        private PictureBox CurrentRankIcon;
        private Label CurrentRankLabel;
        private PictureBox PeakRankIcon;
        private Label PeakRankLabel;
        private Label KdLabel;
        private Label WinLabel;
        private Label PrivateLabel;
    }
}