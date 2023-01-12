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
    partial class ucLogs
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
      this.metroPanel1 = new MetroFramework.Controls.MetroPanel();
      this.mTxtLogs = new MetroFramework.Controls.MetroTextBox();
      this.metroPanel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // metroPanel1
      // 
      this.metroPanel1.Controls.Add(this.mTxtLogs);
      this.metroPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
      this.metroPanel1.HorizontalScrollbarBarColor = true;
      this.metroPanel1.HorizontalScrollbarHighlightOnWheel = false;
      this.metroPanel1.HorizontalScrollbarSize = 10;
      this.metroPanel1.Location = new System.Drawing.Point(0, 0);
      this.metroPanel1.Name = "metroPanel1";
      this.metroPanel1.Size = new System.Drawing.Size(951, 426);
      this.metroPanel1.TabIndex = 0;
      this.metroPanel1.VerticalScrollbarBarColor = true;
      this.metroPanel1.VerticalScrollbarHighlightOnWheel = false;
      this.metroPanel1.VerticalScrollbarSize = 10;
      // 
      // mTxtLogs
      // 
      // 
      // 
      // 
      this.mTxtLogs.CustomButton.Image = null;
      this.mTxtLogs.CustomButton.Location = new System.Drawing.Point(527, 2);
      this.mTxtLogs.CustomButton.Name = "";
      this.mTxtLogs.CustomButton.Size = new System.Drawing.Size(421, 421);
      this.mTxtLogs.CustomButton.Style = MetroFramework.MetroColorStyle.Blue;
      this.mTxtLogs.CustomButton.TabIndex = 1;
      this.mTxtLogs.CustomButton.Theme = MetroFramework.MetroThemeStyle.Light;
      this.mTxtLogs.CustomButton.UseSelectable = true;
      this.mTxtLogs.CustomButton.Visible = false;
      this.mTxtLogs.Dock = System.Windows.Forms.DockStyle.Fill;
      this.mTxtLogs.Lines = new string[0];
      this.mTxtLogs.Location = new System.Drawing.Point(0, 0);
      this.mTxtLogs.MaxLength = 327670000;
      this.mTxtLogs.Multiline = true;
      this.mTxtLogs.Name = "mTxtLogs";
      this.mTxtLogs.PasswordChar = '\0';
      this.mTxtLogs.PromptText = "Waiting for Log event";
      this.mTxtLogs.ReadOnly = true;
      this.mTxtLogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
      this.mTxtLogs.SelectedText = "";
      this.mTxtLogs.SelectionLength = 0;
      this.mTxtLogs.SelectionStart = 0;
      this.mTxtLogs.ShortcutsEnabled = true;
      this.mTxtLogs.Size = new System.Drawing.Size(951, 426);
      this.mTxtLogs.TabIndex = 4;
      this.mTxtLogs.UseSelectable = true;
      this.mTxtLogs.WaterMark = "Waiting for Log event";
      this.mTxtLogs.WaterMarkColor = System.Drawing.Color.FromArgb(((int)(((byte)(109)))), ((int)(((byte)(109)))), ((int)(((byte)(109)))));
      this.mTxtLogs.WaterMarkFont = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Pixel);
      // 
      // ucLogs
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.ActiveCaption;
      this.Controls.Add(this.metroPanel1);
      this.Margin = new System.Windows.Forms.Padding(2);
      this.Name = "ucLogs";
      this.Size = new System.Drawing.Size(951, 426);
      this.metroPanel1.ResumeLayout(false);
      this.ResumeLayout(false);

        }

        #endregion

        private MetroFramework.Controls.MetroPanel metroPanel1;
        private MetroFramework.Controls.MetroTextBox mTxtLogs;
    }
}
