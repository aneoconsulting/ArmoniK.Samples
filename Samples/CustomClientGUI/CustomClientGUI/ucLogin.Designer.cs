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
  partial class ucLogin
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
      this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
      this.mConnect = new MetroFramework.Controls.MetroButton();
      this.metroLabel3 = new MetroFramework.Controls.MetroLabel();
      this.metroLabel2 = new MetroFramework.Controls.MetroLabel();
      this.metroLabel1 = new MetroFramework.Controls.MetroLabel();
      this.portTxtBox = new MetroFramework.Controls.MetroTextBox();
      this.urlTxtBox = new MetroFramework.Controls.MetroTextBox();
      this.tableLayoutPanel1.SuspendLayout();
      this.metroPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // tableLayoutPanel1
      // 
      this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.tableLayoutPanel1.ColumnCount = 3;
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
      this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Controls.Add(this.metroPanel1, 1, 1);
      this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
      this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
      this.tableLayoutPanel1.Name = "tableLayoutPanel1";
      this.tableLayoutPanel1.RowCount = 3;
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
      this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
      this.tableLayoutPanel1.Size = new System.Drawing.Size(832, 417);
      this.tableLayoutPanel1.TabIndex = 0;
      this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
      // 
      // metroPanel1
      // 
      this.metroPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.metroPanel1.Controls.Add(this.mConnect);
      this.metroPanel1.Controls.Add(this.metroLabel3);
      this.metroPanel1.Controls.Add(this.metroLabel2);
      this.metroPanel1.Controls.Add(this.metroLabel1);
      this.metroPanel1.Controls.Add(this.portTxtBox);
      this.metroPanel1.Controls.Add(this.urlTxtBox);
      this.metroPanel1.HorizontalScrollbarBarColor = true;
      this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
      this.metroPanel1.HorizontalScrollbarSize = 6;
      this.metroPanel1.Location = new System.Drawing.Point(69, 88);
      this.metroPanel1.Margin = new System.Windows.Forms.Padding(2);
      this.metroPanel1.Name = "metroPanel1";
      this.metroPanel1.Size = new System.Drawing.Size(693, 240);
      this.metroPanel1.TabIndex = 0;
      this.metroPanel1.VerticalScrollbarBarColor = true;
      this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
      this.metroPanel1.VerticalScrollbarSize = 7;
      // 
      // mConnect
      // 
      this.mConnect.Location = new System.Drawing.Point(495, 166);
      this.mConnect.Margin = new System.Windows.Forms.Padding(2);
      this.mConnect.Name = "mConnect";
      this.mConnect.Size = new System.Drawing.Size(60, 20);
      this.mConnect.TabIndex = 7;
      this.mConnect.Text = "Connect";
      this.mConnect.UseSelectable = true;
      this.mConnect.Click += new System.EventHandler(this.mConnect_Click);
      // 
      // metroLabel3
      // 
      this.metroLabel3.AutoSize = true;
      this.metroLabel3.FontSize = MetroFramework.MetroLabelSize.Tall;
      this.metroLabel3.Location = new System.Drawing.Point(31, 37);
      this.metroLabel3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.metroLabel3.Name = "metroLabel3";
      this.metroLabel3.Size = new System.Drawing.Size(582, 25);
      this.metroLabel3.Style = MetroFramework.MetroColorStyle.Blue;
      this.metroLabel3.TabIndex = 6;
      this.metroLabel3.Text = "Enter here the Control plane service Link i.e http[s]://Connectionstring:Port/";
      this.metroLabel3.Click += new System.EventHandler(this.metroLabel3_Click);
      // 
      // metroLabel2
      // 
      this.metroLabel2.AutoSize = true;
      this.metroLabel2.Location = new System.Drawing.Point(31, 140);
      this.metroLabel2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.metroLabel2.Name = "metroLabel2";
      this.metroLabel2.Size = new System.Drawing.Size(41, 19);
      this.metroLabel2.TabIndex = 5;
      this.metroLabel2.Text = "Port :";
      // 
      // metroLabel1
      // 
      this.metroLabel1.AutoSize = true;
      this.metroLabel1.Location = new System.Drawing.Point(31, 92);
      this.metroLabel1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.metroLabel1.Name = "metroLabel1";
      this.metroLabel1.Size = new System.Drawing.Size(119, 19);
      this.metroLabel1.TabIndex = 4;
      this.metroLabel1.Text = "ControlPlane URL :";
      // 
      // portTxtBox
      // 
      // 
      // 
      // 
      this.portTxtBox.CustomButton.Image = null;
      this.portTxtBox.CustomButton.Location = new System.Drawing.Point(75, 2);
      this.portTxtBox.CustomButton.Margin = new System.Windows.Forms.Padding(2);
      this.portTxtBox.CustomButton.Name = "";
      this.portTxtBox.CustomButton.Size = new System.Drawing.Size(15, 15);
      this.portTxtBox.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.portTxtBox.CustomButton.TabIndex = 1;
      this.portTxtBox.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.portTxtBox.CustomButton.UseSelectable = true;
      this.portTxtBox.CustomButton.Visible = false;
      this.portTxtBox.Lines = new string[0];
      this.portTxtBox.Location = new System.Drawing.Point(154, 139);
      this.portTxtBox.Margin = new System.Windows.Forms.Padding(2);
      this.portTxtBox.MaxLength = 32767;
      this.portTxtBox.Name = "portTxtBox";
      this.portTxtBox.PasswordChar = '\0';
      this.portTxtBox.PromptText = "Port Number";
      this.portTxtBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.portTxtBox.SelectedText = "";
      this.portTxtBox.SelectionLength = 0;
      this.portTxtBox.SelectionStart = 0;
      this.portTxtBox.ShortcutsEnabled = true;
      this.portTxtBox.Size = new System.Drawing.Size(93, 20);
      this.portTxtBox.TabIndex = 3;
      this.portTxtBox.UseSelectable = true;
      this.portTxtBox.WaterMark = "Port Number";
      this.portTxtBox.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.portTxtBox.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      this.portTxtBox.Click += new System.EventHandler(this.portTxtBox_Click);
      // 
      // urlTxtBox
      // 
      this.urlTxtBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      // 
      // 
      // 
      this.urlTxtBox.CustomButton.Image = null;
      this.urlTxtBox.CustomButton.Location = new System.Drawing.Point(509, 2);
      this.urlTxtBox.CustomButton.Margin = new System.Windows.Forms.Padding(2);
      this.urlTxtBox.CustomButton.Name = "";
      this.urlTxtBox.CustomButton.Size = new System.Drawing.Size(25, 25);
      this.urlTxtBox.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.urlTxtBox.CustomButton.TabIndex = 1;
      this.urlTxtBox.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.urlTxtBox.CustomButton.UseSelectable = true;
      this.urlTxtBox.CustomButton.Visible = false;
      this.urlTxtBox.FontSize = MetroFramework.MetroTextBoxSize.Medium;
      this.urlTxtBox.Lines = new string[] {
        "http://a90edfa6dfd6f473db88852d5692ba98-1340302511.eu-west-3.elb.amazonaws.com:50" +
            "01"};
      this.urlTxtBox.Location = new System.Drawing.Point(154, 91);
      this.urlTxtBox.Margin = new System.Windows.Forms.Padding(7, 2, 2, 2);
      this.urlTxtBox.MaxLength = 32767;
      this.urlTxtBox.Name = "urlTxtBox";
      this.urlTxtBox.PasswordChar = '\0';
      this.urlTxtBox.PromptText = "http[s]://ControlPlane_address[:Port]/";
      this.urlTxtBox.ScrollBars = System.Windows.Forms.ScrollBars.None;
      this.urlTxtBox.SelectedText = "";
      this.urlTxtBox.SelectionLength = 0;
      this.urlTxtBox.SelectionStart = 0;
      this.urlTxtBox.ShortcutsEnabled = true;
      this.urlTxtBox.ShowClearButton = true;
      this.urlTxtBox.Size = new System.Drawing.Size(537, 30);
      this.urlTxtBox.TabIndex = 2;
      this.urlTxtBox.Text = "http://a90edfa6dfd6f473db88852d5692ba98-1340302511.eu-west-3.elb.amazonaws.com:50" +
    "01";
      this.urlTxtBox.UseSelectable = true;
      this.urlTxtBox.WaterMark = "http[s]://ControlPlane_address[:Port]/";
      this.urlTxtBox.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.urlTxtBox.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      this.urlTxtBox.Click += new System.EventHandler(this.urlTxtBox_Click);
      this.urlTxtBox.Enter += new System.EventHandler(this.urlTxtBox_Enter);
      // 
      // ucLogin
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.ActiveCaption;
      this.Controls.Add(this.tableLayoutPanel1);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "ucLogin";
      this.Size = new System.Drawing.Size(832, 424);
      this.tableLayoutPanel1.ResumeLayout(false);
      this.metroPanel1.ResumeLayout(false);
      this.metroPanel1.PerformLayout();
      this.ResumeLayout(false);

    }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    private MetroFramework.Controls.MetroPanel metroPanel1;
    private MetroFramework.Controls.MetroLabel metroLabel2;
    private MetroFramework.Controls.MetroLabel metroLabel1;
    private MetroFramework.Controls.MetroTextBox portTxtBox;
    private MetroFramework.Controls.MetroTextBox urlTxtBox;
    private MetroFramework.Controls.MetroLabel metroLabel3;
    private MetroFramework.Controls.MetroButton mConnect;
  }
}
