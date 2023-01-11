// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-$CURRENT_YEAR$. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

namespace CustomClientGUI
{
  partial class ucDashBoard
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

    #region Code généré par le Concepteur de composants

    /// <summary> 
    /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
    /// le contenu de cette méthode avec l'éditeur de code.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
      System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.mPanelTaskInfo = new MetroFramework.Controls.MetroPanel();
      this.mTxtPartition = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel5 = new MetroFramework.Controls.MetroLabel();
      this.mTxtWorkloadInMs = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
      this.mTxtNbCurrencies = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
      this.mTxtNbUnderlyings = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
      this.mTxtNbTasks = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
      this.metroPanel2 = new MetroFramework.Controls.MetroPanel();
      this.metroButton1 = new MetroFramework.Controls.MetroButton();
      this.metroGrid1 = new MetroFramework.Controls.MetroGrid();
      this.SessionId = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Username = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.TaskId = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.StartTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.EndTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Duration = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ResultStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.ErrorDetails = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.bgWorkerSubmit = new System.ComponentModel.BackgroundWorker();
      this.mBtnClear = new MetroFramework.Controls.MetroButton();
      this.tableLayoutPanel1.SuspendLayout();
      this.mPanelTaskInfo.SuspendLayout();
      this.metroPanel2.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).BeginInit();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.InsetDouble;
      this.tableLayoutPanel1.ColumnCount = 1;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.Controls.Add(this.mPanelTaskInfo, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.metroPanel2, 0, 1);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 180F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 87F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(811, 479);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // mPanelTaskInfo
      // 
      this.mPanelTaskInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mPanelTaskInfo.Controls.Add(this.mBtnClear);
      this.mPanelTaskInfo.Controls.Add(this.metroButton1);
      this.mPanelTaskInfo.Controls.Add(this.mTxtPartition);
      this.mPanelTaskInfo.Controls.Add(this.metroLabel5);
      this.mPanelTaskInfo.Controls.Add(this.mTxtWorkloadInMs);
      this.mPanelTaskInfo.Controls.Add(this.metroLabel4);
      this.mPanelTaskInfo.Controls.Add(this.mTxtNbCurrencies);
      this.mPanelTaskInfo.Controls.Add(this.metroLabel3);
      this.mPanelTaskInfo.Controls.Add(this.mTxtNbUnderlyings);
      this.mPanelTaskInfo.Controls.Add(this.metroLabel2);
      this.mPanelTaskInfo.Controls.Add(this.mTxtNbTasks);
      this.mPanelTaskInfo.Controls.Add(this.metroLabel1);
      this.mPanelTaskInfo.HorizontalScrollbarBarColor = true;
      this.mPanelTaskInfo.HorizontalScrollbarHighlightOnWheel = false;
      this.mPanelTaskInfo.HorizontalScrollbarSize = 10;
      this.mPanelTaskInfo.Location = new System.Drawing.Point(6, 6);
      this.mPanelTaskInfo.Name = "mPanelTaskInfo";
      this.mPanelTaskInfo.Size = new System.Drawing.Size(799, 174);
      this.mPanelTaskInfo.TabIndex = 0;
      this.mPanelTaskInfo.VerticalScrollbarBarColor = true;
      this.mPanelTaskInfo.VerticalScrollbarHighlightOnWheel = false;
      this.mPanelTaskInfo.VerticalScrollbarSize = 10;
      this.mPanelTaskInfo.Paint += new System.Windows.Forms.PaintEventHandler(this.mPanelTaskInfo_Paint);
      // 
      // mTxtPartition
      // 
      this.mTxtPartition.BackColor = System.Drawing.SystemColors.AppWorkspace;
      // 
      // 
      // 
      this.mTxtPartition.CustomButton.Image = null;
      this.mTxtPartition.CustomButton.Location = new System.Drawing.Point(100, 2);
      this.mTxtPartition.CustomButton.Name = "";
      this.mTxtPartition.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.mTxtPartition.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.mTxtPartition.CustomButton.TabIndex = 1;
      this.mTxtPartition.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.mTxtPartition.CustomButton.UseSelectable = true;
      this.mTxtPartition.CustomButton.Visible = false;
      this.mTxtPartition.Lines = new string[] {
        "Default"};
      this.mTxtPartition.Location = new System.Drawing.Point(137, 52);
      this.mTxtPartition.MaxLength = 32767;
      this.mTxtPartition.Name = "mTxtPartition";
      this.mTxtPartition.PasswordChar = '\0';
      this.mTxtPartition.ReadOnly = true;
      this.mTxtPartition.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.mTxtPartition.SelectedText = "";
      this.mTxtPartition.SelectionLength = 0;
      this.mTxtPartition.SelectionStart = 0;
      this.mTxtPartition.ShortcutsEnabled = true;
      this.mTxtPartition.Size = new System.Drawing.Size(124, 26);
      this.mTxtPartition.TabIndex = 12;
      this.mTxtPartition.Text = "Default";
      this.mTxtPartition.UseSelectable = true;
      this.mTxtPartition.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.mTxtPartition.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel5
      // 
      this.metroLabel5.AutoSize = true;
      this.metroLabel5.Location = new System.Drawing.Point(18, 55);
      this.metroLabel5.Name = "metroLabel5";
      this.metroLabel5.Size = new System.Drawing.Size(58, 19);
      this.metroLabel5.TabIndex = 11;
      this.metroLabel5.Text = "Partition";
      // 
      // mTxtWorkloadInMs
      // 
      this.mTxtWorkloadInMs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.mTxtWorkloadInMs.CustomButton.Image = null;
      this.mTxtWorkloadInMs.CustomButton.Location = new System.Drawing.Point(100, 2);
      this.mTxtWorkloadInMs.CustomButton.Name = "";
      this.mTxtWorkloadInMs.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.mTxtWorkloadInMs.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.mTxtWorkloadInMs.CustomButton.TabIndex = 1;
      this.mTxtWorkloadInMs.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.mTxtWorkloadInMs.CustomButton.UseSelectable = true;
      this.mTxtWorkloadInMs.CustomButton.Visible = false;
      this.mTxtWorkloadInMs.Lines = new string[] {
        "10"};
      this.mTxtWorkloadInMs.Location = new System.Drawing.Point(537, 88);
      this.mTxtWorkloadInMs.MaxLength = 32767;
      this.mTxtWorkloadInMs.Name = "mTxtWorkloadInMs";
      this.mTxtWorkloadInMs.PasswordChar = '\0';
      this.mTxtWorkloadInMs.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.mTxtWorkloadInMs.SelectedText = "";
      this.mTxtWorkloadInMs.SelectionLength = 0;
      this.mTxtWorkloadInMs.SelectionStart = 0;
      this.mTxtWorkloadInMs.ShortcutsEnabled = true;
      this.mTxtWorkloadInMs.Size = new System.Drawing.Size(124, 26);
      this.mTxtWorkloadInMs.TabIndex = 10;
      this.mTxtWorkloadInMs.Text = "10";
      this.mTxtWorkloadInMs.UseSelectable = true;
      this.mTxtWorkloadInMs.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.mTxtWorkloadInMs.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel4
      // 
      this.metroLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.metroLabel4.AutoSize = true;
      this.metroLabel4.Location = new System.Drawing.Point(364, 92);
      this.metroLabel4.Name = "metroLabel4";
      this.metroLabel4.Size = new System.Drawing.Size(153, 19);
      this.metroLabel4.TabIndex = 9;
      this.metroLabel4.Text = "Fake tasks workload (ms)";
      this.metroLabel4.Click += new System.EventHandler(this.metroLabel4_Click);
      // 
      // mTxtNbCurrencies
      // 
      this.mTxtNbCurrencies.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.mTxtNbCurrencies.CustomButton.Image = null;
      this.mTxtNbCurrencies.CustomButton.Location = new System.Drawing.Point(100, 2);
      this.mTxtNbCurrencies.CustomButton.Name = "";
      this.mTxtNbCurrencies.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.mTxtNbCurrencies.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.mTxtNbCurrencies.CustomButton.TabIndex = 1;
      this.mTxtNbCurrencies.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.mTxtNbCurrencies.CustomButton.UseSelectable = true;
      this.mTxtNbCurrencies.CustomButton.Visible = false;
      this.mTxtNbCurrencies.Lines = new string[] {
        "1"};
      this.mTxtNbCurrencies.Location = new System.Drawing.Point(537, 49);
      this.mTxtNbCurrencies.MaxLength = 32767;
      this.mTxtNbCurrencies.Name = "mTxtNbCurrencies";
      this.mTxtNbCurrencies.PasswordChar = '\0';
      this.mTxtNbCurrencies.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.mTxtNbCurrencies.SelectedText = "";
      this.mTxtNbCurrencies.SelectionLength = 0;
      this.mTxtNbCurrencies.SelectionStart = 0;
      this.mTxtNbCurrencies.ShortcutsEnabled = true;
      this.mTxtNbCurrencies.Size = new System.Drawing.Size(124, 26);
      this.mTxtNbCurrencies.TabIndex = 8;
      this.mTxtNbCurrencies.Text = "1";
      this.mTxtNbCurrencies.UseSelectable = true;
      this.mTxtNbCurrencies.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.mTxtNbCurrencies.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel3
      // 
      this.metroLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.metroLabel3.AutoSize = true;
      this.metroLabel3.Location = new System.Drawing.Point(364, 52);
      this.metroLabel3.Name = "metroLabel3";
      this.metroLabel3.Size = new System.Drawing.Size(89, 19);
      this.metroLabel3.TabIndex = 7;
      this.metroLabel3.Text = "Nb currencies";
      this.metroLabel3.Click += new System.EventHandler(this.metroLabel3_Click);
      // 
      // mTxtNbUnderlyings
      // 
      this.mTxtNbUnderlyings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.mTxtNbUnderlyings.CustomButton.Image = null;
      this.mTxtNbUnderlyings.CustomButton.Location = new System.Drawing.Point(100, 2);
      this.mTxtNbUnderlyings.CustomButton.Name = "";
      this.mTxtNbUnderlyings.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.mTxtNbUnderlyings.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.mTxtNbUnderlyings.CustomButton.TabIndex = 1;
      this.mTxtNbUnderlyings.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.mTxtNbUnderlyings.CustomButton.UseSelectable = true;
      this.mTxtNbUnderlyings.CustomButton.Visible = false;
      this.mTxtNbUnderlyings.Lines = new string[] {
        "1"};
      this.mTxtNbUnderlyings.Location = new System.Drawing.Point(537, 12);
      this.mTxtNbUnderlyings.MaxLength = 32767;
      this.mTxtNbUnderlyings.Name = "mTxtNbUnderlyings";
      this.mTxtNbUnderlyings.PasswordChar = '\0';
      this.mTxtNbUnderlyings.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.mTxtNbUnderlyings.SelectedText = "";
      this.mTxtNbUnderlyings.SelectionLength = 0;
      this.mTxtNbUnderlyings.SelectionStart = 0;
      this.mTxtNbUnderlyings.ShortcutsEnabled = true;
      this.mTxtNbUnderlyings.Size = new System.Drawing.Size(124, 26);
      this.mTxtNbUnderlyings.TabIndex = 6;
      this.mTxtNbUnderlyings.Text = "1";
      this.mTxtNbUnderlyings.UseSelectable = true;
      this.mTxtNbUnderlyings.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.mTxtNbUnderlyings.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel2
      // 
      this.metroLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.metroLabel2.AutoSize = true;
      this.metroLabel2.Location = new System.Drawing.Point(364, 15);
      this.metroLabel2.Name = "metroLabel2";
      this.metroLabel2.Size = new System.Drawing.Size(97, 19);
      this.metroLabel2.TabIndex = 5;
      this.metroLabel2.Text = "Nb underlyings";
      // 
      // mTxtNbTasks
      // 
      // 
      // 
      // 
      this.mTxtNbTasks.CustomButton.Image = null;
      this.mTxtNbTasks.CustomButton.Location = new System.Drawing.Point(100, 2);
      this.mTxtNbTasks.CustomButton.Name = "";
      this.mTxtNbTasks.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.mTxtNbTasks.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.mTxtNbTasks.CustomButton.TabIndex = 1;
      this.mTxtNbTasks.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.mTxtNbTasks.CustomButton.UseSelectable = true;
      this.mTxtNbTasks.CustomButton.Visible = false;
      this.mTxtNbTasks.Lines = new string[] {
        "10"};
      this.mTxtNbTasks.Location = new System.Drawing.Point(137, 15);
      this.mTxtNbTasks.MaxLength = 32767;
      this.mTxtNbTasks.Name = "mTxtNbTasks";
      this.mTxtNbTasks.PasswordChar = '\0';
      this.mTxtNbTasks.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.mTxtNbTasks.SelectedText = "";
      this.mTxtNbTasks.SelectionLength = 0;
      this.mTxtNbTasks.SelectionStart = 0;
      this.mTxtNbTasks.ShortcutsEnabled = true;
      this.mTxtNbTasks.Size = new System.Drawing.Size(124, 26);
      this.mTxtNbTasks.TabIndex = 4;
      this.mTxtNbTasks.Text = "10";
      this.mTxtNbTasks.UseSelectable = true;
      this.mTxtNbTasks.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.mTxtNbTasks.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel1
      // 
      this.metroLabel1.AutoSize = true;
      this.metroLabel1.Location = new System.Drawing.Point(18, 19);
      this.metroLabel1.Name = "metroLabel1";
      this.metroLabel1.Size = new System.Drawing.Size(74, 19);
      this.metroLabel1.TabIndex = 2;
      this.metroLabel1.Text = "Nb of tasks";
      // 
      // metroPanel2
      // 
      this.metroPanel2.Controls.Add(this.metroGrid1);
      this.metroPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
      this.metroPanel2.HorizontalScrollbarBarColor = true;
      this.metroPanel2.HorizontalScrollbarHighlightOnWheel = false;
      this.metroPanel2.HorizontalScrollbarSize = 10;
      this.metroPanel2.Location = new System.Drawing.Point(6, 189);
      this.metroPanel2.Name = "metroPanel2";
      this.metroPanel2.Size = new System.Drawing.Size(799, 194);
      this.metroPanel2.TabIndex = 1;
      this.metroPanel2.VerticalScrollbarBarColor = true;
      this.metroPanel2.VerticalScrollbarHighlightOnWheel = false;
      this.metroPanel2.VerticalScrollbarSize = 10;
      // 
      // metroButton1
      // 
      this.metroButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.metroButton1.Location = new System.Drawing.Point(653, 135);
      this.metroButton1.Name = "metroButton1";
      this.metroButton1.Size = new System.Drawing.Size(133, 29);
      this.metroButton1.TabIndex = 15;
      this.metroButton1.Text = "Submit";
      this.metroButton1.UseSelectable = true;
      this.metroButton1.Click += new System.EventHandler(this.metroButton1_Click);
      // 
      // metroGrid1
      // 
      this.metroGrid1.AllowUserToAddRows = false;
      this.metroGrid1.AllowUserToDeleteRows = false;
      this.metroGrid1.AllowUserToResizeRows = false;
      this.metroGrid1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
      this.metroGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.metroGrid1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
      this.metroGrid1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
      dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
      dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
      dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
      dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
      dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
      dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.metroGrid1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
      this.metroGrid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.metroGrid1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.SessionId,
            this.Username,
            this.TaskId,
            this.StartTime,
            this.EndTime,
            this.Duration,
            this.Status,
            this.ResultStatus,
            this.ErrorDetails});
      dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
      dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
      dataGridViewCellStyle2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(136)))), ((int)(((byte)(136)))), ((int)(((byte)(136)))));
      dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
      dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
      dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
      this.metroGrid1.DefaultCellStyle = dataGridViewCellStyle2;
      this.metroGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.metroGrid1.EnableHeadersVisualStyles = false;
      this.metroGrid1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
      this.metroGrid1.GridColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
      this.metroGrid1.Location = new System.Drawing.Point(0, 0);
      this.metroGrid1.Name = "metroGrid1";
      this.metroGrid1.ReadOnly = true;
      this.metroGrid1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
      dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
      dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(174)))), ((int)(((byte)(219)))));
      dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
      dataGridViewCellStyle3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
      dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(198)))), ((int)(((byte)(247)))));
      dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(17)))), ((int)(((byte)(17)))));
      dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
      this.metroGrid1.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
      this.metroGrid1.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
      this.metroGrid1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.metroGrid1.Size = new System.Drawing.Size(799, 194);
      this.metroGrid1.TabIndex = 2;
      // 
      // SessionId
      // 
      this.SessionId.HeaderText = "SessionId";
      this.SessionId.MinimumWidth = 300;
      this.SessionId.Name = "SessionId";
      this.SessionId.ReadOnly = true;
      this.SessionId.Width = 300;
      // 
      // Username
      // 
      this.Username.HeaderText = "Username";
      this.Username.MinimumWidth = 100;
      this.Username.Name = "Username";
      this.Username.ReadOnly = true;
      // 
      // TaskId
      // 
      this.TaskId.HeaderText = "TaskId";
      this.TaskId.MinimumWidth = 300;
      this.TaskId.Name = "TaskId";
      this.TaskId.ReadOnly = true;
      this.TaskId.Width = 300;
      // 
      // StartTime
      // 
      this.StartTime.HeaderText = "StartTime";
      this.StartTime.MinimumWidth = 200;
      this.StartTime.Name = "StartTime";
      this.StartTime.ReadOnly = true;
      this.StartTime.Width = 200;
      // 
      // EndTime
      // 
      this.EndTime.HeaderText = "EndTime";
      this.EndTime.MinimumWidth = 200;
      this.EndTime.Name = "EndTime";
      this.EndTime.ReadOnly = true;
      this.EndTime.Width = 200;
      // 
      // Duration
      // 
      this.Duration.HeaderText = "Duration";
      this.Duration.MinimumWidth = 100;
      this.Duration.Name = "Duration";
      this.Duration.ReadOnly = true;
      // 
      // Status
      // 
      this.Status.HeaderText = "Status";
      this.Status.MinimumWidth = 100;
      this.Status.Name = "Status";
      this.Status.ReadOnly = true;
      // 
      // ResultStatus
      // 
      this.ResultStatus.HeaderText = "ResultStatus";
      this.ResultStatus.Name = "ResultStatus";
      this.ResultStatus.ReadOnly = true;
      // 
      // ErrorDetails
      // 
      this.ErrorDetails.HeaderText = "ErrorDetails";
      this.ErrorDetails.MinimumWidth = 500;
      this.ErrorDetails.Name = "ErrorDetails";
      this.ErrorDetails.ReadOnly = true;
      this.ErrorDetails.Width = 500;
      // 
      // bgWorkerSubmit
      // 
      this.bgWorkerSubmit.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorkerSubmit_DoWork);
      this.bgWorkerSubmit.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorkerSubmit_ProgressChanged);
      // 
      // mBtnClear
      // 
      this.mBtnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.mBtnClear.Location = new System.Drawing.Point(3, 135);
      this.mBtnClear.Name = "mBtnClear";
      this.mBtnClear.Size = new System.Drawing.Size(133, 29);
      this.mBtnClear.TabIndex = 16;
      this.mBtnClear.Text = "ClearTable";
      this.mBtnClear.UseSelectable = true;
      this.mBtnClear.Click += new System.EventHandler(this.mBtnClear_Click);
      // 
      // ucDashBoard
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "ucDashBoard";
      this.Size = new System.Drawing.Size(812, 478);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.mPanelTaskInfo.ResumeLayout(false);
      this.mPanelTaskInfo.PerformLayout();
      this.metroPanel2.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.metroGrid1)).EndInit();
      this.ResumeLayout(false);

    }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroPanel mPanelTaskInfo;
        private MetroFramework.Controls.MetroTextBox mTxtPartition;
        private MetroFramework.Controls.MetroLabel metroLabel5;
        private MetroFramework.Controls.MetroTextBox mTxtWorkloadInMs;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private MetroFramework.Controls.MetroTextBox mTxtNbCurrencies;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroTextBox mTxtNbUnderlyings;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroTextBox mTxtNbTasks;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroPanel metroPanel2;
    private MetroFramework.Controls.MetroButton metroButton1;
    private MetroFramework.Controls.MetroGrid metroGrid1;
    private System.Windows.Forms.DataGridViewTextBoxColumn SessionId;
    private System.Windows.Forms.DataGridViewTextBoxColumn Username;
    private System.Windows.Forms.DataGridViewTextBoxColumn TaskId;
    private System.Windows.Forms.DataGridViewTextBoxColumn StartTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn EndTime;
    private System.Windows.Forms.DataGridViewTextBoxColumn Duration;
    private System.Windows.Forms.DataGridViewTextBoxColumn Status;
    private System.Windows.Forms.DataGridViewTextBoxColumn ResultStatus;
    private System.Windows.Forms.DataGridViewTextBoxColumn ErrorDetails;
    private System.ComponentModel.BackgroundWorker bgWorkerSubmit;
    private MetroFramework.Controls.MetroButton mBtnClear;
  }
}
