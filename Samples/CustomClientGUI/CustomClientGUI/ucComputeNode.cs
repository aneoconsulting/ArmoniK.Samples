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

using TaskStatus = ArmoniK.Api.gRPC.V1.TaskStatus;

namespace CustomClientGUI
{
  public partial class ucComputeNode : MetroUserControl
  {
    public ucComputeNode(string idxNode)
    {
      InitializeComponent();
      this.Name              = "Node_" + idxNode;
      this.mLblInstance.Text = "Node " + idxNode;

      ListCores = Enumerable.Range(0,
                                   36)
                            .Select(idx =>
                                    {
                                      var lbl = new Label();
                                      lbl.Name = "core" + idx;
                                      lbl.Text = "";
                                      lbl.Margin = new Padding(0,
                                                               1,
                                                               1,
                                                               0);

                                      lbl.Dock      = DockStyle.Fill;
                                      lbl.BackColor = Color.Gray;
                                      return lbl;
                                    })
                            .ToList();
      foreach (var core in ListCores)
      {
        tableLayoutPanel1.Controls.Add(core);
      }
    }

    
    public List<Label> ListCores { get; set; }


    private void urlTxtBox_Click(object    sender,
                                 EventArgs e)
    {
    }

    private void portTxtBox_Click(object    sender,
                                  EventArgs e)
    {
    }

    private void metroLabel3_Click(object    sender,
                                   EventArgs e)
    {
    }

    private void urlTxtBox_Enter(object    sender,
                                 EventArgs e)
    {
    }

    private void tableLayoutPanel1_Paint(object         sender,
                                         PaintEventArgs e)
    {
    }

    private void metroLabel2_Click(object    sender,
                                   EventArgs e)
    {
    }

    private void label1_Click(object    sender,
                              EventArgs e)
    {
    }
  }
}
