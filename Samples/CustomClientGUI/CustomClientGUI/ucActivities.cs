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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MetroFramework.Controls;

namespace CustomClientGUI
{
  public partial class ucActivities : MetroUserControl
  {
    public ucActivities()
    {
      InitializeComponent();
      var listSimplePanel = Enumerable.Range(0, 20).Select(idx =>
                                               {
                                                 var panel = new ucComputeNode(idx.ToString());
                                                 panel.Dock   = DockStyle.Fill;
                                                 panel.Margin = Padding.Empty;
                                                 return panel;
                                               }).ToList();

      foreach (var panel in listSimplePanel)
      {
        tableLayoutPanel1.Controls.Add(panel);
      }


    }

    private void urlTxtBox_Click(object sender, EventArgs e)
    {

    }

    private void portTxtBox_Click(object sender, EventArgs e)
    {

    }

    private void metroLabel3_Click(object sender, EventArgs e)
    {

    }

    private void urlTxtBox_Enter(object sender, EventArgs e)
    {

    }

    private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
    {

    }

    private void flowLayout_Paint(object sender, PaintEventArgs e)
    {
      
    }

    private void tableLayoutPanel1_Paint_1(object sender, PaintEventArgs e)
    {
      
    }
  }
}
