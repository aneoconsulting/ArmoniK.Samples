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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using MetroFramework.Controls;

using TaskStatus = ArmoniK.Api.gRPC.V1.TaskStatus;

namespace CustomClientGUI
{
  public partial class ucActivities : MetroUserControl
  {
    public CancellationTokenSource                  CancellationTokenSource { get; private set; }
    public ConcurrentDictionary<string, NodeStatus> TaskIds                 { get; private set; }

    public class NodeStatus
    {
      public enum EnumTask
      {
        Queued,
        Processing,
        Completed,
        Error,
        Cancelled
      }

      public EnumTask EnumTaskStatus = EnumTask.Queued;
      public int      NodeIdx { get; set; } = 0;
      public int      PodIdx  { get; set; } = 0;

      public TaskStatus TaskStatus { get; set; }
    }

    public const int MaxNodes       = 20;
    public const int MaxPodsPerNode = 36;

    public bool AddOrUpdateTasks(IEnumerable<Tuple<string, ArmoniK.Api.gRPC.V1.TaskStatus>> keyValue)
    {
      foreach (var pair in keyValue)
      {
        if (!TaskIds.ContainsKey(pair.Item1))
        {
          TaskIds[pair.Item1] = new NodeStatus()
                                {
                                  NodeIdx        = -1,
                                  PodIdx         = -1,
                                  TaskStatus     = pair.Item2,
                                  EnumTaskStatus = NodeStatus.EnumTask.Queued,
                                };
        }

        TaskIds[pair.Item1]
          .EnumTaskStatus = pair.Item2 switch
                            {
                              TaskStatus.Processing => NodeStatus.EnumTask.Processing,
                              TaskStatus.Creating   => NodeStatus.EnumTask.Processing,
                              TaskStatus.Dispatched => NodeStatus.EnumTask.Processing,
                              TaskStatus.Submitted  => NodeStatus.EnumTask.Queued,
                              TaskStatus.Completed  => NodeStatus.EnumTask.Completed,
                              TaskStatus.Cancelled  => NodeStatus.EnumTask.Cancelled,
                              TaskStatus.Cancelling => NodeStatus.EnumTask.Cancelled,
                              TaskStatus.Error      => NodeStatus.EnumTask.Error,
                              TaskStatus.Timeout    => NodeStatus.EnumTask.Error,
                              _                     => NodeStatus.EnumTask.Queued,
                            };
      }

      return true;
    }

    private bool UpdatePanel()
    {
      foreach (var pair in TaskIds)
      {
        if (pair.Value.NodeIdx == -1 && pair.Value.PodIdx == -1)
        {
          bool found = false;
          for (int i = 0; i < ListSimplePanel.Count; i++)
          {
            for (int j = 0; j < ListSimplePanel[i]
                                .ListCores.Count; j++)
            {
              var ucPod = ListSimplePanel[i]
                .ListCores[j];

              if (ucPod.BackColor == Color.Gray && pair.Value.EnumTaskStatus == NodeStatus.EnumTask.Queued)
              {
                ucPod.BackColor           = Color.Khaki;
                pair.Value.EnumTaskStatus = NodeStatus.EnumTask.Processing;
                pair.Value.NodeIdx        = i;
                pair.Value.PodIdx         = j;
                found                     = true;
                break;
              }
              else if (ucPod.BackColor == Color.Green)
              {
                Task.Run(() =>
                         {
                           Thread.Sleep(5000);
                           ucPod.BackColor = Color.Gray;
                         });
                ucPod.BackColor = Color.Aquamarine;
              }
            }

            if (found)
            {
              break;
            }
          }
        }
        else if (pair.Value.EnumTaskStatus == NodeStatus.EnumTask.Processing)
        {
          var ucPod = ListSimplePanel[pair.Value.NodeIdx]
            .ListCores[pair.Value.PodIdx];
        }
        else if (pair.Value.EnumTaskStatus == NodeStatus.EnumTask.Error)
        {
          var ucPod = ListSimplePanel[pair.Value.NodeIdx]
            .ListCores[pair.Value.PodIdx];
          ucPod.BackColor = Color.Red;
          TaskIds.TryRemove(pair.Key,
                            out var _);
        }
        else if (pair.Value.EnumTaskStatus == NodeStatus.EnumTask.Cancelled)
        {
          var ucPod = ListSimplePanel[pair.Value.NodeIdx]
            .ListCores[pair.Value.PodIdx];
          ucPod.BackColor = Color.DarkOrange;
          TaskIds.TryRemove(pair.Key,
                            out var _);
        }
        else if (pair.Value.EnumTaskStatus == NodeStatus.EnumTask.Completed)
        {
          var ucPod = ListSimplePanel[pair.Value.NodeIdx]
            .ListCores[pair.Value.PodIdx];
          ucPod.BackColor = Color.Green;
          TaskIds.TryRemove(pair.Key,
                            out var _);
        }
      }

      return true;
    }

    public bool InsertOrUpdateLabel(NodeStatus node)
    {
      for (int i = 0; i < ListSimplePanel.Count; i++)
      {
        for (int j = 0; j < ListSimplePanel[i]
                            .ListCores.Count; j++)
        {
          var ucPod = ListSimplePanel[i]
            .ListCores[j];

          if (ucPod.BackColor == Color.Gray)
          {
          }
        }
      }

      return false;
    }

    public ucActivities()
    {
      InitializeComponent();
      ListSimplePanel = Enumerable.Range(0,
                                         20)
                                  .Select(idx =>
                                          {
                                            var panel = new ucComputeNode(idx.ToString());
                                            panel.Dock   = DockStyle.Fill;
                                            panel.Margin = Padding.Empty;
                                            return panel;
                                          })
                                  .ToList();

      foreach (var panel in ListSimplePanel)
      {
        tableLayoutPanel1.Controls.Add(panel);
      }

      CancellationTokenSource                      = new CancellationTokenSource();
      backgroundWorker1.WorkerSupportsCancellation = true;
      backgroundWorker1.WorkerReportsProgress      = true;
      TaskIds                                      = new ConcurrentDictionary<string, NodeStatus>();

      if (!backgroundWorker1.IsBusy)
      {
        backgroundWorker1.RunWorkerAsync();
      }
    }

    public List<ucComputeNode> ListSimplePanel { get; set; }

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

    private void flowLayout_Paint(object         sender,
                                  PaintEventArgs e)
    {
    }

    private void tableLayoutPanel1_Paint_1(object         sender,
                                           PaintEventArgs e)
    {
    }

    private void backgroundWorker1_DoWork(object          sender,
                                          DoWorkEventArgs e)
    {
      while (!CancellationTokenSource.IsCancellationRequested)
      {
        if (!TaskIds.Any())
        {
          Thread.Sleep(100);
          continue;
        }

        UpdatePanel();
      }
    }
  }
}
