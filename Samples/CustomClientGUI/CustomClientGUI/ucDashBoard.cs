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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using ArmoniK.Api.gRPC.V1;

using CustomClientGUI.Data;
using CustomClientGUI.Submitter;

using MetroFramework.Controls;

using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CustomClientGUI
{
  public partial class ucDashBoard : MetroFramework.Controls.MetroUserControl
  {
    public ucDashBoard(string name)
    {
      InitializeComponent();
      this.Name                            = name;
      mLblDashBoard.Text                   = name;
      bgWorkerSubmit.WorkerReportsProgress = true;
    }

    private void metroTile2_Click(object    sender,
                                  EventArgs e)
    {
    }

    private void metroLabel3_Click(object    sender,
                                   EventArgs e)
    {
    }

    private void mPanelTaskInfo_Paint(object         sender,
                                      PaintEventArgs e)
    {
    }

    private void metroLabel4_Click(object    sender,
                                   EventArgs e)
    {
    }

    private void metroButton1_Click(object    sender,
                                    EventArgs e)
    {
      if (!bgWorkerSubmit.IsBusy)
      {
        int nbTasks = int.Parse(mTxtNbTasks.Text);
        var nbRows = Enumerable.Range(0,
                                      nbTasks)
                               .Select(_ => metroGrid1.Rows.Add())
                               .Count();
        bgWorkerSubmit.RunWorkerAsync();
      }


      metroButton1.Enabled = false;
      metroButton1.Text    = "Executing ...";
    }


    public LoggerFactory LoggerFactory { get; set; }

    public Task WatchTask { get; set; }

    public List<Task<string>> Tasks { get; set; }

    private void splitter1_SplitterMoved(object            sender,
                                         SplitterEventArgs e)
    {
    }

    private void bgWorkerSubmit_DoWork(object          sender,
                                       DoWorkEventArgs e)
    {
      var nbTasks          = int.Parse(mTxtNbTasks.Text);
      var nbCurrencies     = int.Parse(mTxtNbCurrencies.Text);
      var nbUnderLying     = int.Parse(mTxtNbUnderlyings.Text);
      var workloadTimeInMs = int.Parse(mTxtWorkloadInMs.Text);
      var offset           = metroGrid1.Rows.Count - nbTasks;


      for (var row = offset; row < offset + nbTasks; row++)
      {
        metroGrid1.Rows[row]
                  .Cells["SessionId"]
                  .Value = "Waiting for Server response";

        metroGrid1.Rows[row]
                  .Cells["TaskId"]
                  .Value = "Waiting for Server response";

        metroGrid1.Rows[row]
                  .Cells["Status"]
                  .Value = "Sending";
      }

      ArmonikClientSession.LoggerSink.NewLogHandler += LogHandler;


      SessionRun = new ArmonikClientSession(frmMain.Instance.SessionConfiguration.Host,
                                            frmMain.Instance.SessionConfiguration.TaskOptions,
                                            frmMain.Instance.SessionConfiguration.MethodName,
                                            metroGrid1,
                                            bgWorkerSubmit,
                                            offset);

      SessionRun.Submit(nbTasks,
                        (nbUnderLying) * 8,
                        8,
                        workloadTimeInMs);

      foreach (var task in SessionRun.AsyncTaskIds)
      {
        task.Wait();
      }

      SessionRun.DemoRun.Service.Dispose();

      bgWorkerSubmit.ReportProgress(100);
      ArmonikClientSession.LoggerSink.NewLogHandler -= LogHandler;

      SessionRun?.Dispose();
    }

    public ArmonikClientSession SessionRun { get; set; }

    private void bgWorkerSubmit_ProgressChanged(object                   sender,
                                                ProgressChangedEventArgs e)
    {
      if (e.UserState is Serilog.Events.LogEvent log)
      {
        var lines = mTxtDashBoardLog.Text.Split('\n')
                                    .ToList();
        if (lines.Count > 512)
        {
          mTxtDashBoardLog.Text = string.Join("\n",
                                              lines.GetRange(lines.Count - 129,
                                                             128));
          ((ucLogs)frmMain.Instance.MetroContainer.Controls["ucLogs_" + frmMain.Instance.Mtc.TabPages[frmMain.Instance.Mtc.SelectedIndex]
                                                                               .Text]).Logs.Text = mTxtDashBoardLog.Text;
        }

        StringBuilder sb = new StringBuilder(mTxtDashBoardLog.Text);
        sb.Append($@"{log.Timestamp.DateTime:HH:mm:ss}: {log.Level} {log.MessageTemplate}" + Environment.NewLine);
        mTxtDashBoardLog.Text           = sb.ToString();

        ((ucLogs)frmMain.Instance.MetroContainer.Controls["ucLogs_" + frmMain.Instance.Mtc.TabPages[frmMain.Instance.Mtc.SelectedIndex]
                                                                             .Text]).Logs.Text = sb.ToString();
      }
      else
      {
        SessionRun.DemoRun.Service.Dispose();
        metroButton1.Text    = "Submit";
        metroButton1.Enabled = true;
      }
    }

    private void mBtnClear_Click(object    sender,
                                 EventArgs e)
    {
      metroGrid1.Rows.Clear();
    }

    private void tableLayoutPanel1_Paint(object         sender,
                                         PaintEventArgs e)
    {
    }

    private void LogHandler(object    sender,
                            EventArgs e)
    {
      var log = ((LogEventArgs)e).Log;
      bgWorkerSubmit.ReportProgress(50,
                                    log);
    }
  }
}
