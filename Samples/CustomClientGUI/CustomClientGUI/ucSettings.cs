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

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Common;

using Google.Protobuf.WellKnownTypes;

using MetroFramework.Controls;

namespace CustomClientGUI
{
  public partial class ucSettings : MetroUserControl
  {
    public ucSettings(string ucSettingsName)
    {
      InitializeComponent();
      this.Name = ucSettingsName;
    }

    public void savedSettings()
    {
      frmMain.Instance.SessionConfiguration.TaskOptions = new TaskOptions()
                                                          {
                                                            ApplicationName      = txtAppsName.Text,
                                                            ApplicationVersion   = txtAppsVersion.Text,
                                                            ApplicationService   = txtServiceName.Text,
                                                            ApplicationNamespace = txtServiceNamespace.Text,

                                                            MaxDuration = new Duration()
                                                                          {
                                                                            Seconds = int.Parse(txtMaxDuration.Text),
                                                                          },
                                                            MaxRetries  = int.Parse(txtMaxRetries.Text),
                                                            Priority    = int.Parse(txtPriority.Text),
                                                            EngineType  = EngineType.Unified.ToString(),
                                                            PartitionId = "",
                                                          };

      var idx = mMethodName.SelectedIndex < 0
                  ? 0
                  : mMethodName.SelectedIndex;

      frmMain.Instance.SessionConfiguration.MethodName = mMethodName.Items[idx].ToString();
    }

    private void mSaveSettings_Click(object sender, EventArgs e)
    {
      
      savedSettings();
      

    }
  }
}
