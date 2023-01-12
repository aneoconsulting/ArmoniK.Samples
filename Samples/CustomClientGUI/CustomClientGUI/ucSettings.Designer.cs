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
    partial class ucSettings
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
      this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
      this.mAppConfig = new MetroFramework.Controls.MetroPanel();
      this.mTasksConfig = new MetroFramework.Controls.MetroPanel();
      this.lstEngine = new System.Windows.Forms.ListBox();
      this.metroLabel11 = new MetroFramework.Controls.MetroLabel();
      this.txtPartitionId = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel7 = new MetroFramework.Controls.MetroLabel();
      this.metroLabel8 = new MetroFramework.Controls.MetroLabel();
      this.txtPriority = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel9 = new MetroFramework.Controls.MetroLabel();
      this.txtMaxRetries = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel10 = new MetroFramework.Controls.MetroLabel();
      this.txtMaxDuration = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
      this.metroLabel12 = new MetroFramework.Controls.MetroLabel();
      this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
      this.metroLabel6 = new MetroFramework.Controls.MetroLabel();
      this.txtServiceName = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel5 = new MetroFramework.Controls.MetroLabel();
      this.txtServiceNamespace = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel4 = new MetroFramework.Controls.MetroLabel();
      this.txtAppsVersion = new MetroFramework.Controls.MetroTextBox();
      this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
      this.txtAppsName = new MetroFramework.Controls.MetroTextBox();
      this.mMethodName = new MetroFramework.Controls.MetroComboBox();
      this.mSaveSettings = new MetroFramework.Controls.MetroButton();
      this.tableLayoutPanel1.SuspendLayout();
      this.mAppConfig.SuspendLayout();
      this.mTasksConfig.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.ColumnCount = 2;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.mAppConfig, 0, 0);
      this.tableLayoutPanel1.Controls.Add(this.mTasksConfig, 1, 0);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 1;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(916, 424);
      this.tableLayoutPanel1.TabIndex = 0;
      // 
      // mAppConfig
      // 
      this.mAppConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mAppConfig.Controls.Add(this.mSaveSettings);
      this.mAppConfig.Controls.Add(this.mMethodName);
      this.mAppConfig.Controls.Add(this.metroLabel12);
      this.mAppConfig.Controls.Add(this.metroLabel2);
      this.mAppConfig.Controls.Add(this.metroLabel6);
      this.mAppConfig.Controls.Add(this.txtServiceName);
      this.mAppConfig.Controls.Add(this.metroLabel5);
      this.mAppConfig.Controls.Add(this.txtServiceNamespace);
      this.mAppConfig.Controls.Add(this.metroLabel4);
      this.mAppConfig.Controls.Add(this.txtAppsVersion);
      this.mAppConfig.Controls.Add(this.metroLabel3);
      this.mAppConfig.Controls.Add(this.txtAppsName);
      this.mAppConfig.HorizontalScrollbarBarColor = true;
      this.mAppConfig.HorizontalScrollbarHighlightOnWheel = false;
      this.mAppConfig.HorizontalScrollbarSize = 10;
      this.mAppConfig.Location = new System.Drawing.Point(1, 1);
      this.mAppConfig.Margin = new System.Windows.Forms.Padding(1);
      this.mAppConfig.Name = "mAppConfig";
      this.mAppConfig.Size = new System.Drawing.Size(456, 422);
      this.mAppConfig.TabIndex = 0;
      this.mAppConfig.VerticalScrollbarBarColor = true;
      this.mAppConfig.VerticalScrollbarHighlightOnWheel = false;
      this.mAppConfig.VerticalScrollbarSize = 10;
      // 
      // mTasksConfig
      // 
      this.mTasksConfig.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.mTasksConfig.Controls.Add(this.lstEngine);
      this.mTasksConfig.Controls.Add(this.metroLabel11);
      this.mTasksConfig.Controls.Add(this.txtPartitionId);
      this.mTasksConfig.Controls.Add(this.metroLabel7);
      this.mTasksConfig.Controls.Add(this.metroLabel8);
      this.mTasksConfig.Controls.Add(this.txtPriority);
      this.mTasksConfig.Controls.Add(this.metroLabel9);
      this.mTasksConfig.Controls.Add(this.txtMaxRetries);
      this.mTasksConfig.Controls.Add(this.metroLabel10);
      this.mTasksConfig.Controls.Add(this.txtMaxDuration);
      this.mTasksConfig.Controls.Add(this.metroLabel1);
      this.mTasksConfig.HorizontalScrollbarBarColor = true;
      this.mTasksConfig.HorizontalScrollbarHighlightOnWheel = false;
      this.mTasksConfig.HorizontalScrollbarSize = 10;
      this.mTasksConfig.Location = new System.Drawing.Point(459, 1);
      this.mTasksConfig.Margin = new System.Windows.Forms.Padding(1);
      this.mTasksConfig.Name = "mTasksConfig";
      this.mTasksConfig.Size = new System.Drawing.Size(456, 422);
      this.mTasksConfig.TabIndex = 1;
      this.mTasksConfig.VerticalScrollbarBarColor = true;
      this.mTasksConfig.VerticalScrollbarHighlightOnWheel = false;
      this.mTasksConfig.VerticalScrollbarSize = 10;
      // 
      // lstEngine
      // 
      this.lstEngine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.lstEngine.FormattingEnabled = true;
      this.lstEngine.Items.AddRange(new object[] {
            "Unified",
            "DataSynapse",
            "Symphony"});
      this.lstEngine.Location = new System.Drawing.Point(178, 260);
      this.lstEngine.Name = "lstEngine";
      this.lstEngine.Size = new System.Drawing.Size(271, 17);
      this.lstEngine.TabIndex = 44;
      // 
      // metroLabel11
      // 
      this.metroLabel11.AutoSize = true;
      this.metroLabel11.Location = new System.Drawing.Point(20, 287);
      this.metroLabel11.Name = "metroLabel11";
      this.metroLabel11.Size = new System.Drawing.Size(73, 19);
      this.metroLabel11.TabIndex = 43;
      this.metroLabel11.Text = "Partition Id";
      // 
      // txtPartitionId
      // 
      this.txtPartitionId.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtPartitionId.CustomButton.Image = null;
      this.txtPartitionId.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtPartitionId.CustomButton.Name = "";
      this.txtPartitionId.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtPartitionId.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtPartitionId.CustomButton.TabIndex = 1;
      this.txtPartitionId.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtPartitionId.CustomButton.UseSelectable = true;
      this.txtPartitionId.CustomButton.Visible = false;
      this.txtPartitionId.Lines = new string[] {
        "Default"};
      this.txtPartitionId.Location = new System.Drawing.Point(178, 285);
      this.txtPartitionId.MaxLength = 32767;
      this.txtPartitionId.Name = "txtPartitionId";
      this.txtPartitionId.PasswordChar = '\0';
      this.txtPartitionId.ReadOnly = true;
      this.txtPartitionId.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtPartitionId.SelectedText = "";
      this.txtPartitionId.SelectionLength = 0;
      this.txtPartitionId.SelectionStart = 0;
      this.txtPartitionId.ShortcutsEnabled = true;
      this.txtPartitionId.Size = new System.Drawing.Size(271, 23);
      this.txtPartitionId.TabIndex = 42;
      this.txtPartitionId.Text = "Default";
      this.txtPartitionId.UseSelectable = true;
      this.txtPartitionId.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtPartitionId.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel7
      // 
      this.metroLabel7.AutoSize = true;
      this.metroLabel7.Location = new System.Drawing.Point(20, 258);
      this.metroLabel7.Name = "metroLabel7";
      this.metroLabel7.Size = new System.Drawing.Size(79, 19);
      this.metroLabel7.TabIndex = 41;
      this.metroLabel7.Text = "Engine Type";
      // 
      // metroLabel8
      // 
      this.metroLabel8.AutoSize = true;
      this.metroLabel8.Location = new System.Drawing.Point(20, 229);
      this.metroLabel8.Name = "metroLabel8";
      this.metroLabel8.Size = new System.Drawing.Size(78, 19);
      this.metroLabel8.TabIndex = 40;
      this.metroLabel8.Text = "Task priority";
      // 
      // txtPriority
      // 
      this.txtPriority.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtPriority.CustomButton.Image = null;
      this.txtPriority.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtPriority.CustomButton.Name = "";
      this.txtPriority.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtPriority.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtPriority.CustomButton.TabIndex = 1;
      this.txtPriority.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtPriority.CustomButton.UseSelectable = true;
      this.txtPriority.CustomButton.Visible = false;
      this.txtPriority.Lines = new string[] {
        "1"};
      this.txtPriority.Location = new System.Drawing.Point(178, 227);
      this.txtPriority.MaxLength = 32767;
      this.txtPriority.Name = "txtPriority";
      this.txtPriority.PasswordChar = '\0';
      this.txtPriority.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtPriority.SelectedText = "";
      this.txtPriority.SelectionLength = 0;
      this.txtPriority.SelectionStart = 0;
      this.txtPriority.ShortcutsEnabled = true;
      this.txtPriority.Size = new System.Drawing.Size(271, 23);
      this.txtPriority.TabIndex = 39;
      this.txtPriority.Text = "1";
      this.txtPriority.UseSelectable = true;
      this.txtPriority.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtPriority.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel9
      // 
      this.metroLabel9.AutoSize = true;
      this.metroLabel9.Location = new System.Drawing.Point(20, 200);
      this.metroLabel9.Name = "metroLabel9";
      this.metroLabel9.Size = new System.Drawing.Size(100, 19);
      this.metroLabel9.TabIndex = 38;
      this.metroLabel9.Text = "Max task retries";
      // 
      // txtMaxRetries
      // 
      this.txtMaxRetries.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtMaxRetries.CustomButton.Image = null;
      this.txtMaxRetries.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtMaxRetries.CustomButton.Name = "";
      this.txtMaxRetries.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtMaxRetries.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtMaxRetries.CustomButton.TabIndex = 1;
      this.txtMaxRetries.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtMaxRetries.CustomButton.UseSelectable = true;
      this.txtMaxRetries.CustomButton.Visible = false;
      this.txtMaxRetries.Lines = new string[] {
        "3"};
      this.txtMaxRetries.Location = new System.Drawing.Point(178, 198);
      this.txtMaxRetries.MaxLength = 32767;
      this.txtMaxRetries.Name = "txtMaxRetries";
      this.txtMaxRetries.PasswordChar = '\0';
      this.txtMaxRetries.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtMaxRetries.SelectedText = "";
      this.txtMaxRetries.SelectionLength = 0;
      this.txtMaxRetries.SelectionStart = 0;
      this.txtMaxRetries.ShortcutsEnabled = true;
      this.txtMaxRetries.Size = new System.Drawing.Size(271, 23);
      this.txtMaxRetries.TabIndex = 37;
      this.txtMaxRetries.Text = "3";
      this.txtMaxRetries.UseSelectable = true;
      this.txtMaxRetries.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtMaxRetries.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel10
      // 
      this.metroLabel10.AutoSize = true;
      this.metroLabel10.Location = new System.Drawing.Point(20, 171);
      this.metroLabel10.Name = "metroLabel10";
      this.metroLabel10.Size = new System.Drawing.Size(130, 19);
      this.metroLabel10.TabIndex = 36;
      this.metroLabel10.Text = "Max task duration (s)";
      // 
      // txtMaxDuration
      // 
      this.txtMaxDuration.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtMaxDuration.CustomButton.Image = null;
      this.txtMaxDuration.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtMaxDuration.CustomButton.Name = "";
      this.txtMaxDuration.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtMaxDuration.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtMaxDuration.CustomButton.TabIndex = 1;
      this.txtMaxDuration.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtMaxDuration.CustomButton.UseSelectable = true;
      this.txtMaxDuration.CustomButton.Visible = false;
      this.txtMaxDuration.Lines = new string[] {
        "3600"};
      this.txtMaxDuration.Location = new System.Drawing.Point(178, 169);
      this.txtMaxDuration.MaxLength = 32767;
      this.txtMaxDuration.Name = "txtMaxDuration";
      this.txtMaxDuration.PasswordChar = '\0';
      this.txtMaxDuration.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtMaxDuration.SelectedText = "";
      this.txtMaxDuration.SelectionLength = 0;
      this.txtMaxDuration.SelectionStart = 0;
      this.txtMaxDuration.ShortcutsEnabled = true;
      this.txtMaxDuration.Size = new System.Drawing.Size(271, 23);
      this.txtMaxDuration.TabIndex = 35;
      this.txtMaxDuration.Text = "3600";
      this.txtMaxDuration.UseSelectable = true;
      this.txtMaxDuration.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtMaxDuration.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel1
      // 
      this.metroLabel1.AutoSize = true;
      this.metroLabel1.FontSize = MetroFramework.MetroLabelSize.Tall;
      this.metroLabel1.Location = new System.Drawing.Point(20, 112);
      this.metroLabel1.Name = "metroLabel1";
      this.metroLabel1.Size = new System.Drawing.Size(169, 25);
      this.metroLabel1.Style = MetroFramework.MetroColorStyle.Blue;
      this.metroLabel1.TabIndex = 27;
      this.metroLabel1.Text = "Tasks configuration : ";
      this.metroLabel1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // metroLabel12
      // 
      this.metroLabel12.AutoSize = true;
      this.metroLabel12.FontSize = MetroFramework.MetroLabelSize.Tall;
      this.metroLabel12.Location = new System.Drawing.Point(14, 112);
      this.metroLabel12.Name = "metroLabel12";
      this.metroLabel12.Size = new System.Drawing.Size(217, 25);
      this.metroLabel12.Style = MetroFramework.MetroColorStyle.Blue;
      this.metroLabel12.TabIndex = 58;
      this.metroLabel12.Text = "Application configuration : ";
      this.metroLabel12.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
      // 
      // metroLabel2
      // 
      this.metroLabel2.AutoSize = true;
      this.metroLabel2.Location = new System.Drawing.Point(14, 291);
      this.metroLabel2.Name = "metroLabel2";
      this.metroLabel2.Size = new System.Drawing.Size(104, 19);
      this.metroLabel2.TabIndex = 57;
      this.metroLabel2.Text = "Invoked method";
      // 
      // metroLabel6
      // 
      this.metroLabel6.AutoSize = true;
      this.metroLabel6.Location = new System.Drawing.Point(14, 256);
      this.metroLabel6.Name = "metroLabel6";
      this.metroLabel6.Size = new System.Drawing.Size(91, 19);
      this.metroLabel6.TabIndex = 55;
      this.metroLabel6.Text = "Service Name";
      // 
      // txtServiceName
      // 
      this.txtServiceName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtServiceName.CustomButton.Image = null;
      this.txtServiceName.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtServiceName.CustomButton.Name = "";
      this.txtServiceName.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtServiceName.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtServiceName.CustomButton.TabIndex = 1;
      this.txtServiceName.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtServiceName.CustomButton.UseSelectable = true;
      this.txtServiceName.CustomButton.Visible = false;
      this.txtServiceName.Lines = new string[] {
        "ServiceApps"};
      this.txtServiceName.Location = new System.Drawing.Point(172, 254);
      this.txtServiceName.MaxLength = 32767;
      this.txtServiceName.Name = "txtServiceName";
      this.txtServiceName.PasswordChar = '\0';
      this.txtServiceName.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtServiceName.SelectedText = "";
      this.txtServiceName.SelectionLength = 0;
      this.txtServiceName.SelectionStart = 0;
      this.txtServiceName.ShortcutsEnabled = true;
      this.txtServiceName.Size = new System.Drawing.Size(271, 23);
      this.txtServiceName.TabIndex = 54;
      this.txtServiceName.Text = "ServiceApps";
      this.txtServiceName.UseSelectable = true;
      this.txtServiceName.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtServiceName.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel5
      // 
      this.metroLabel5.AutoSize = true;
      this.metroLabel5.Location = new System.Drawing.Point(14, 227);
      this.metroLabel5.Name = "metroLabel5";
      this.metroLabel5.Size = new System.Drawing.Size(124, 19);
      this.metroLabel5.TabIndex = 53;
      this.metroLabel5.Text = "Service Namespace";
      // 
      // txtServiceNamespace
      // 
      this.txtServiceNamespace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtServiceNamespace.CustomButton.Image = null;
      this.txtServiceNamespace.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtServiceNamespace.CustomButton.Name = "";
      this.txtServiceNamespace.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtServiceNamespace.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtServiceNamespace.CustomButton.TabIndex = 1;
      this.txtServiceNamespace.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtServiceNamespace.CustomButton.UseSelectable = true;
      this.txtServiceNamespace.CustomButton.Visible = false;
      this.txtServiceNamespace.Lines = new string[] {
        "Armonik.Samples.StressTests.Worker"};
      this.txtServiceNamespace.Location = new System.Drawing.Point(172, 225);
      this.txtServiceNamespace.MaxLength = 32767;
      this.txtServiceNamespace.Name = "txtServiceNamespace";
      this.txtServiceNamespace.PasswordChar = '\0';
      this.txtServiceNamespace.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtServiceNamespace.SelectedText = "";
      this.txtServiceNamespace.SelectionLength = 0;
      this.txtServiceNamespace.SelectionStart = 0;
      this.txtServiceNamespace.ShortcutsEnabled = true;
      this.txtServiceNamespace.Size = new System.Drawing.Size(271, 23);
      this.txtServiceNamespace.TabIndex = 52;
      this.txtServiceNamespace.Text = "Armonik.Samples.StressTests.Worker";
      this.txtServiceNamespace.UseSelectable = true;
      this.txtServiceNamespace.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtServiceNamespace.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel4
      // 
      this.metroLabel4.AutoSize = true;
      this.metroLabel4.Location = new System.Drawing.Point(14, 198);
      this.metroLabel4.Name = "metroLabel4";
      this.metroLabel4.Size = new System.Drawing.Size(120, 19);
      this.metroLabel4.TabIndex = 51;
      this.metroLabel4.Text = "Application version";
      // 
      // txtAppsVersion
      // 
      this.txtAppsVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtAppsVersion.CustomButton.Image = null;
      this.txtAppsVersion.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtAppsVersion.CustomButton.Name = "";
      this.txtAppsVersion.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtAppsVersion.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtAppsVersion.CustomButton.TabIndex = 1;
      this.txtAppsVersion.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtAppsVersion.CustomButton.UseSelectable = true;
      this.txtAppsVersion.CustomButton.Visible = false;
      this.txtAppsVersion.Lines = new string[] {
        "1.0.0-700"};
      this.txtAppsVersion.Location = new System.Drawing.Point(172, 196);
      this.txtAppsVersion.MaxLength = 32767;
      this.txtAppsVersion.Name = "txtAppsVersion";
      this.txtAppsVersion.PasswordChar = '\0';
      this.txtAppsVersion.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtAppsVersion.SelectedText = "";
      this.txtAppsVersion.SelectionLength = 0;
      this.txtAppsVersion.SelectionStart = 0;
      this.txtAppsVersion.ShortcutsEnabled = true;
      this.txtAppsVersion.Size = new System.Drawing.Size(271, 23);
      this.txtAppsVersion.TabIndex = 50;
      this.txtAppsVersion.Text = "1.0.0-700";
      this.txtAppsVersion.UseSelectable = true;
      this.txtAppsVersion.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtAppsVersion.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // metroLabel3
      // 
      this.metroLabel3.AutoSize = true;
      this.metroLabel3.Location = new System.Drawing.Point(14, 169);
      this.metroLabel3.Name = "metroLabel3";
      this.metroLabel3.Size = new System.Drawing.Size(115, 19);
      this.metroLabel3.TabIndex = 49;
      this.metroLabel3.Text = "Application Name";
      // 
      // txtAppsName
      // 
      this.txtAppsName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.txtAppsName.CustomButton.Image = null;
      this.txtAppsName.CustomButton.Location = new System.Drawing.Point(249, 1);
      this.txtAppsName.CustomButton.Name = "";
      this.txtAppsName.CustomButton.Size = new System.Drawing.Size(21, 21);
      this.txtAppsName.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.txtAppsName.CustomButton.TabIndex = 1;
      this.txtAppsName.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.txtAppsName.CustomButton.UseSelectable = true;
      this.txtAppsName.CustomButton.Visible = false;
      this.txtAppsName.Lines = new string[] {
        "Armonik.Samples.StressTests.Worker"};
      this.txtAppsName.Location = new System.Drawing.Point(172, 167);
      this.txtAppsName.MaxLength = 32767;
      this.txtAppsName.Name = "txtAppsName";
      this.txtAppsName.PasswordChar = '\0';
      this.txtAppsName.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.txtAppsName.SelectedText = "";
      this.txtAppsName.SelectionLength = 0;
      this.txtAppsName.SelectionStart = 0;
      this.txtAppsName.ShortcutsEnabled = true;
      this.txtAppsName.Size = new System.Drawing.Size(271, 23);
      this.txtAppsName.TabIndex = 48;
      this.txtAppsName.Text = "Armonik.Samples.StressTests.Worker";
      this.txtAppsName.UseSelectable = true;
      this.txtAppsName.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.txtAppsName.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // mMethodName
      // 
      this.mMethodName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.mMethodName.FormattingEnabled = true;
      this.mMethodName.ItemHeight = 23;
      this.mMethodName.Items.AddRange(new object[] {
            "ComputeWorkLoad",
            "ComputeWorkLoadWithException"});
      this.mMethodName.Location = new System.Drawing.Point(172, 287);
      this.mMethodName.Name = "mMethodName";
      this.mMethodName.Size = new System.Drawing.Size(271, 29);
      this.mMethodName.TabIndex = 59;
      this.mMethodName.UseSelectable = true;
      // 
      // mSaveSettings
      // 
      this.mSaveSettings.Location = new System.Drawing.Point(9, 354);
      this.mSaveSettings.Name = "mSaveSettings";
      this.mSaveSettings.Size = new System.Drawing.Size(96, 29);
      this.mSaveSettings.TabIndex = 45;
      this.mSaveSettings.Text = "Save";
      this.mSaveSettings.UseSelectable = true;
      this.mSaveSettings.Click += new System.EventHandler(this.mSaveSettings_Click);
      // 
      // ucSettings
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.ActiveCaption;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "ucSettings";
      this.Size = new System.Drawing.Size(919, 427);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.mAppConfig.ResumeLayout(false);
      this.mAppConfig.PerformLayout();
      this.mTasksConfig.ResumeLayout(false);
      this.mTasksConfig.PerformLayout();
      this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private MetroFramework.Controls.MetroPanel mAppConfig;
        private MetroFramework.Controls.MetroLabel metroLabel12;
        private MetroFramework.Controls.MetroLabel metroLabel2;
        private MetroFramework.Controls.MetroLabel metroLabel6;
        private MetroFramework.Controls.MetroTextBox txtServiceName;
        private MetroFramework.Controls.MetroLabel metroLabel5;
        private MetroFramework.Controls.MetroTextBox txtServiceNamespace;
        private MetroFramework.Controls.MetroLabel metroLabel4;
        private MetroFramework.Controls.MetroTextBox txtAppsVersion;
        private MetroFramework.Controls.MetroLabel metroLabel3;
        private MetroFramework.Controls.MetroTextBox txtAppsName;
        private MetroFramework.Controls.MetroPanel mTasksConfig;
        private System.Windows.Forms.ListBox lstEngine;
        private MetroFramework.Controls.MetroLabel metroLabel11;
        private MetroFramework.Controls.MetroTextBox txtPartitionId;
        private MetroFramework.Controls.MetroLabel metroLabel7;
        private MetroFramework.Controls.MetroLabel metroLabel8;
        private MetroFramework.Controls.MetroTextBox txtPriority;
        private MetroFramework.Controls.MetroLabel metroLabel9;
        private MetroFramework.Controls.MetroTextBox txtMaxRetries;
        private MetroFramework.Controls.MetroLabel metroLabel10;
        private MetroFramework.Controls.MetroTextBox txtMaxDuration;
        private MetroFramework.Controls.MetroLabel metroLabel1;
        private MetroFramework.Controls.MetroComboBox mMethodName;
    private MetroFramework.Controls.MetroButton mSaveSettings;
  }
}
