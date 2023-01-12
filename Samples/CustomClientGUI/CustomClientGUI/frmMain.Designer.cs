namespace CustomClientGUI
{
  partial class frmMain
  {
    /// <summary>
    /// Variable nécessaire au concepteur.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Nettoyage des ressources utilisées.
    /// </summary>
    /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Code généré par le Concepteur Windows Form

    /// <summary>
    /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
    /// le contenu de cette méthode avec l'éditeur de code.
    /// </summary>
    private void InitializeComponent()
    {
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.mPanel = new MetroFramework.Controls.MetroPanel();
      this.menuPanel = new MetroFramework.Controls.MetroPanel();
      this.mtc = new MetroFramework.Controls.MetroTabControl();
      this.mtp1 = new MetroFramework.Controls.MetroTabPage();
      this.metroTabPage2 = new MetroFramework.Controls.MetroTabPage();
      this.metroTabPage3 = new MetroFramework.Controls.MetroTabPage();
      this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
      this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
      this.metroPanel2 = new MetroFramework.Controls.MetroPanel();
      this.mTileDashBoard = new MetroFramework.Controls.MetroTile();
      this.mTileExit = new MetroFramework.Controls.MetroTile();
      this.mTileSettings = new MetroFramework.Controls.MetroTile();
      this.mTileActivities = new MetroFramework.Controls.MetroTile();
      this.mTileLogs = new MetroFramework.Controls.MetroTile();
      this.tableLayoutPanel1.SuspendLayout();
      this.menuPanel.SuspendLayout();
      this.mtc.SuspendLayout();
      this.tableLayoutPanel2.SuspendLayout();
      this.metroPanel1.SuspendLayout();
      this.metroPanel2.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.mPanel, 1, 1);
      this.tableLayoutPanel1.Controls.Add(this.menuPanel, 0, 1);
      this.tableLayoutPanel1.Controls.Add(this.mtc, 1, 0);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(2, 55);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 2;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.Size = new System.Drawing.Size(818, 403);
      this.tableLayoutPanel1.TabIndex = 0;
      this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
      // 
      // mPanel
      // 
      this.mPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mPanel.AutoScroll = true;
      this.mPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
      this.mPanel.HorizontalScrollbar = true;
      this.mPanel.HorizontalScrollbarBarColor = true;
      this.mPanel.HorizontalScrollbarHighlightOnWheel = false;
      this.mPanel.HorizontalScrollbarSize = 10;
      this.mPanel.Location = new System.Drawing.Point(103, 53);
      this.mPanel.Name = "mPanel";
      this.mPanel.Size = new System.Drawing.Size(712, 347);
      this.mPanel.TabIndex = 0;
      this.mPanel.VerticalScrollbar = true;
      this.mPanel.VerticalScrollbarBarColor = true;
      this.mPanel.VerticalScrollbarHighlightOnWheel = false;
      this.mPanel.VerticalScrollbarSize = 10;
      // 
      // menuPanel
      // 
      this.menuPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.menuPanel.Controls.Add(this.tableLayoutPanel2);
      this.menuPanel.HorizontalScrollbarBarColor = true;
      this.menuPanel.HorizontalScrollbarHighlightOnWheel = false;
      this.menuPanel.HorizontalScrollbarSize = 10;
      this.menuPanel.Location = new System.Drawing.Point(3, 53);
      this.menuPanel.Name = "menuPanel";
      this.menuPanel.Size = new System.Drawing.Size(94, 347);
      this.menuPanel.TabIndex = 1;
      this.menuPanel.VerticalScrollbarBarColor = true;
      this.menuPanel.VerticalScrollbarHighlightOnWheel = false;
      this.menuPanel.VerticalScrollbarSize = 10;
      // 
      // mtc
      // 
      this.mtc.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mtc.Controls.Add(this.mtp1);
      this.mtc.Controls.Add(this.metroTabPage2);
      this.mtc.Controls.Add(this.metroTabPage3);
      this.mtc.ItemSize = new System.Drawing.Size(157, 34);
      this.mtc.Location = new System.Drawing.Point(103, 3);
      this.mtc.Name = "mtc";
      this.mtc.SelectedIndex = 0;
      this.mtc.Size = new System.Drawing.Size(712, 44);
      this.mtc.Style = MetroFramework.MetroColorStyle.Green;
      this.mtc.TabIndex = 2;
      this.mtc.UseSelectable = true;
      this.mtc.SelectedIndexChanged += new System.EventHandler(this.mtc_SelectedIndexChanged);
      // 
      // mtp1
      // 
      this.mtp1.HorizontalScrollbarBarColor = true;
      this.mtp1.HorizontalScrollbarHighlightOnWheel = false;
      this.mtp1.HorizontalScrollbarSize = 10;
      this.mtp1.Location = new System.Drawing.Point(4, 38);
      this.mtp1.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
      this.mtp1.Name = "mtp1";
      this.mtp1.Size = new System.Drawing.Size(704, 2);
      this.mtp1.TabIndex = 0;
      this.mtp1.Text = "Demo 1";
      this.mtp1.VerticalScrollbarBarColor = true;
      this.mtp1.VerticalScrollbarHighlightOnWheel = false;
      this.mtp1.VerticalScrollbarSize = 10;
      // 
      // metroTabPage2
      // 
      this.metroTabPage2.HorizontalScrollbarBarColor = true;
      this.metroTabPage2.HorizontalScrollbarHighlightOnWheel = false;
      this.metroTabPage2.HorizontalScrollbarSize = 10;
      this.metroTabPage2.Location = new System.Drawing.Point(4, 38);
      this.metroTabPage2.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
      this.metroTabPage2.Name = "metroTabPage2";
      this.metroTabPage2.Size = new System.Drawing.Size(705, 2);
      this.metroTabPage2.TabIndex = 1;
      this.metroTabPage2.Text = "Demo 2";
      this.metroTabPage2.VerticalScrollbarBarColor = true;
      this.metroTabPage2.VerticalScrollbarHighlightOnWheel = false;
      this.metroTabPage2.VerticalScrollbarSize = 10;
      // 
      // metroTabPage3
      // 
      this.metroTabPage3.HorizontalScrollbarBarColor = true;
      this.metroTabPage3.HorizontalScrollbarHighlightOnWheel = false;
      this.metroTabPage3.HorizontalScrollbarSize = 10;
      this.metroTabPage3.Location = new System.Drawing.Point(4, 38);
      this.metroTabPage3.Margin = new System.Windows.Forms.Padding(30, 3, 3, 3);
      this.metroTabPage3.Name = "metroTabPage3";
      this.metroTabPage3.Size = new System.Drawing.Size(705, 2);
      this.metroTabPage3.TabIndex = 2;
      this.metroTabPage3.Text = "Demo 3";
      this.metroTabPage3.VerticalScrollbarBarColor = true;
      this.metroTabPage3.VerticalScrollbarHighlightOnWheel = false;
      this.metroTabPage3.VerticalScrollbarSize = 10;
      // 
      // tableLayoutPanel2
      // 
      this.tableLayoutPanel2.ColumnCount = 1;
      this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.Controls.Add(this.mTileLogs, 0, 2);
      this.tableLayoutPanel2.Controls.Add(this.mTileSettings, 0, 1);
      this.tableLayoutPanel2.Controls.Add(this.metroPanel1, 0, 0);
      this.tableLayoutPanel2.Controls.Add(this.metroPanel2, 0, 4);
      this.tableLayoutPanel2.Controls.Add(this.mTileActivities, 0, 3);
      this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel2.Name = "tableLayoutPanel2";
      this.tableLayoutPanel2.RowCount = 5;
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
      this.tableLayoutPanel2.Size = new System.Drawing.Size(94, 347);
      this.tableLayoutPanel2.TabIndex = 2;
      // 
      // metroPanel1
      // 
      this.metroPanel1.Controls.Add(this.mTileDashBoard);
      this.metroPanel1.HorizontalScrollbarBarColor = true;
      this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
      this.metroPanel1.HorizontalScrollbarSize = 10;
      this.metroPanel1.Location = new System.Drawing.Point(0, 0);
      this.metroPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.metroPanel1.Name = "metroPanel1";
      this.metroPanel1.Size = new System.Drawing.Size(94, 50);
      this.metroPanel1.TabIndex = 0;
      this.metroPanel1.VerticalScrollbarBarColor = true;
      this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
      this.metroPanel1.VerticalScrollbarSize = 10;
      // 
      // metroPanel2
      // 
      this.metroPanel2.Controls.Add(this.mTileExit);
      this.metroPanel2.HorizontalScrollbarBarColor = true;
      this.metroPanel2.HorizontalScrollbarHighlightOnWheel = false;
      this.metroPanel2.HorizontalScrollbarSize = 10;
      this.metroPanel2.Location = new System.Drawing.Point(0, 307);
      this.metroPanel2.Margin = new System.Windows.Forms.Padding(0);
      this.metroPanel2.Name = "metroPanel2";
      this.metroPanel2.Size = new System.Drawing.Size(94, 38);
      this.metroPanel2.TabIndex = 1;
      this.metroPanel2.VerticalScrollbarBarColor = true;
      this.metroPanel2.VerticalScrollbarHighlightOnWheel = false;
      this.metroPanel2.VerticalScrollbarSize = 10;
      // 
      // mTileDashBoard
      // 
      this.mTileDashBoard.ActiveControl = null;
      this.mTileDashBoard.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mTileDashBoard.Location = new System.Drawing.Point(0, 0);
      this.mTileDashBoard.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.mTileDashBoard.Name = "mTileDashBoard";
      this.mTileDashBoard.Size = new System.Drawing.Size(94, 50);
      this.mTileDashBoard.TabIndex = 2;
      this.mTileDashBoard.Text = "Dashboard";
      this.mTileDashBoard.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.mTileDashBoard.UseSelectable = true;
      this.mTileDashBoard.Click += new System.EventHandler(this.mTileDashBoard_Click);
      // 
      // mTileExit
      // 
      this.mTileExit.ActiveControl = null;
      this.mTileExit.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mTileExit.Location = new System.Drawing.Point(0, 0);
      this.mTileExit.Name = "mTileExit";
      this.mTileExit.Size = new System.Drawing.Size(94, 38);
      this.mTileExit.TabIndex = 3;
      this.mTileExit.Text = "Exit";
      this.mTileExit.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.mTileExit.UseSelectable = true;
      this.mTileExit.Click += new System.EventHandler(this.mTileExit_Click);
      // 
      // mTileSettings
      // 
      this.mTileSettings.ActiveControl = null;
      this.mTileSettings.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mTileSettings.Location = new System.Drawing.Point(0, 53);
      this.mTileSettings.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.mTileSettings.Name = "mTileSettings";
      this.mTileSettings.Size = new System.Drawing.Size(94, 44);
      this.mTileSettings.TabIndex = 4;
      this.mTileSettings.Text = "Settings";
      this.mTileSettings.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.mTileSettings.UseSelectable = true;
      this.mTileSettings.Click += new System.EventHandler(this.mTileSettings_Click);
      // 
      // mTileActivities
      // 
      this.mTileActivities.ActiveControl = null;
      this.mTileActivities.Location = new System.Drawing.Point(0, 153);
      this.mTileActivities.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.mTileActivities.Name = "mTileActivities";
      this.mTileActivities.Size = new System.Drawing.Size(94, 39);
      this.mTileActivities.TabIndex = 5;
      this.mTileActivities.Text = "Activities";
      this.mTileActivities.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.mTileActivities.UseSelectable = true;
      this.mTileActivities.Click += new System.EventHandler(this.mTileActivities_Click);
      // 
      // mTileLogs
      // 
      this.mTileLogs.ActiveControl = null;
      this.mTileLogs.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mTileLogs.Location = new System.Drawing.Point(0, 103);
      this.mTileLogs.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
      this.mTileLogs.Name = "mTileLogs";
      this.mTileLogs.Size = new System.Drawing.Size(94, 44);
      this.mTileLogs.TabIndex = 6;
      this.mTileLogs.Text = "Logs";
      this.mTileLogs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.mTileLogs.UseSelectable = true;
      this.mTileLogs.Click += new System.EventHandler(this.mTileLogs_Click);
      // 
      // frmMain
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(836, 474);
      this.Controls.Add(this.tableLayoutPanel1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
      this.Name = "frmMain";
      this.Padding = new System.Windows.Forms.Padding(13, 39, 13, 13);
      this.Text = "ArmoniK .Net Submit";
      this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
      this.Load += new System.EventHandler(this.Form1_Load);
      this.Resize += new System.EventHandler(this.frmMain_Resize);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.menuPanel.ResumeLayout(false);
      this.mtc.ResumeLayout(false);
      this.tableLayoutPanel2.ResumeLayout(false);
      this.metroPanel1.ResumeLayout(false);
      this.metroPanel2.ResumeLayout(false);
      this.ResumeLayout(false);

    }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroPanel mPanel;
        private MetroFramework.Controls.MetroPanel menuPanel;
        private MetroFramework.Controls.MetroTabControl mtc;
        private MetroFramework.Controls.MetroTabPage mtp1;
        private MetroFramework.Controls.MetroTabPage metroTabPage2;
        private MetroFramework.Controls.MetroTabPage metroTabPage3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroTile mTileDashBoard;
        private MetroFramework.Controls.MetroPanel metroPanel2;
        private MetroFramework.Controls.MetroTile mTileExit;
    private MetroFramework.Controls.MetroTile mTileActivities;
    private MetroFramework.Controls.MetroTile mTileSettings;
    private MetroFramework.Controls.MetroTile mTileLogs;
  }
}

