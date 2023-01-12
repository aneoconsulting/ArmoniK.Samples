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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using MetroFramework.Controls;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;

using CustomClientGUI.Data;

using ArmoniK.Api.gRPC.V1.Sessions;

using TaskStatus = ArmoniK.Api.gRPC.V1.TaskStatus;


namespace CustomClientGUI.Submitter
{
  public class ArmonikClientSession : IDisposable
  {
    private readonly ILogger<ArmonikClientSession> logger_;
    private readonly IConfigurationRoot            configuration_;

    public List<Task<string>> AsyncTaskIds;

    public Task Monitoring;

    public CancellationTokenSource CancellationToken { get; set; }

    private readonly CultureInfo culture_ = new("en-US");

    private const string Format = "dddd, dd MMMM yyyy HH:mm:ss";

    public static TbsLoggerSink LoggerSink = new TbsLoggerSink();

    public ArmonikClientSession(string           address,
                                TaskOptions      taskOptions,
                                string           methodName,
                                MetroGrid        metroGrid1,
                                BackgroundWorker bgWorkerSubmit,
                                int              offset)
    {
      Console.WriteLine("Hello Armonik Demo test");

      CancellationToken = new CancellationTokenSource();

      Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft",
                                                                   LogEventLevel.Information)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Console()
                                            .WriteTo.Sink(LoggerSink)
                                            .CreateLogger();

      LoggerFactory = new LoggerFactory(new[]
                                        {
                                          new SerilogLoggerProvider(Log.Logger),
                                        },
                                        new LoggerFilterOptions().AddFilter("Grpc",
                                                                            LogLevel.Error));

      logger_ = LoggerFactory?.CreateLogger<ArmonikClientSession>();

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           false)
                                              .AddEnvironmentVariables();

      configuration_ = builder.Build();

      TableSession = metroGrid1;

      BgWorkerSubmit = bgWorkerSubmit;

      Offset = offset;

      ResultHandler = new ResultForStressTestsHandler(LoggerFactory)
                      {
                        SubActionResponse = (response,
                                             taskId) =>
                                            {
                                              var idx = FindRowsByTaskId(taskId);
                                              while (idx == -1)
                                              {
                                                idx = FindRowsByTaskId(taskId);
                                                Thread.Sleep(100);
                                              }

                                              var end = DateTime.Now;

                                              TableSession.Rows[idx]
                                                          .Cells["EndTime"]
                                                          .Value = end.ToString(Format,
                                                                                culture_);
                                              var start = DateTime.ParseExact(TableSession.Rows[idx]
                                                                                          .Cells["StartTime"]
                                                                                          .Value.ToString(),
                                                                              Format,
                                                                              culture_);
                                              var duration = end - start;

                                              TableSession.Rows[idx]
                                                          .Cells["Duration"]
                                                          .Value = $"{duration:G}";


                                              TableSession.Rows[idx]
                                                          .Cells["Status"]
                                                          .Value = "Completed";


                                              TableSession.Rows[idx]
                                                          .Cells["ResultStatus"]
                                                          .Value = "Received";

                                              frmMain.Instance.UcActivities.AddOrUpdateTasks(new List<Tuple<string, TaskStatus>>()
                                                                                             {
                                                                                               Tuple.Create(taskId,
                                                                                                            TaskStatus.Completed),
                                                                                             });
                                              ExecutingTasks.TryRemove(taskId,
                                                                       out var _);

                                              ResultHandler!.NbResponse++;
                                            },

                        SubActionError = (ex,
                                          taskId) =>
                                         {
                                           var idx = FindRowsByTaskId(taskId);
                                           while (idx == -1)
                                           {
                                             idx = FindRowsByTaskId(taskId);
                                             Thread.Sleep(100);
                                           }

                                           var end = DateTime.Now;
                                           var start = DateTime.ParseExact(TableSession.Rows[idx]
                                                                                       .Cells["StartTime"]
                                                                                       .Value.ToString(),
                                                                           Format,
                                                                           culture_);
                                           var duration = end - start;

                                           TableSession.Rows[idx]
                                                       .Cells["EndTime"]
                                                       .Value = end.ToString(Format,
                                                                             culture_);

                                           TableSession.Rows[idx]
                                                       .Cells["Duration"]
                                                       .Value = $"{duration:G}";

                                           if (ex.StatusCode == ArmonikStatusCode.TaskCancelled)
                                           {
                                             TableSession.Rows[idx]
                                                         .Cells["Status"]
                                                         .Value = "Cancelled";

                                             TableSession.Rows[idx]
                                                         .Cells["ResultStatus"]
                                                         .Value = "No Result";

                                             TableSession.Rows[idx]
                                                         .Cells["ErrorDetails"]
                                                         .Value = "Task was cancelled by user";
                                             frmMain.Instance.UcActivities.AddOrUpdateTasks(new List<Tuple<string, TaskStatus>>()
                                                                                            {
                                                                                              Tuple.Create(taskId,
                                                                                                           TaskStatus.Cancelled),
                                                                                            });
                                           }
                                           else
                                           {
                                             TableSession.Rows[idx]
                                                         .Cells["Status"]
                                                         .Value = "Error";

                                             TableSession.Rows[idx]
                                                         .Cells["ResultStatus"]
                                                         .Value = "Error";

                                             TableSession.Rows[idx]
                                                         .Cells["ErrorDetails"]
                                                         .Value = ex.Message.Substring(0,
                                                                                       Math.Min(8192,
                                                                                                ex.Message.Length)) + "...";
                                             frmMain.Instance.UcActivities.AddOrUpdateTasks(new List<Tuple<string, TaskStatus>>()
                                                                                            {
                                                                                              Tuple.Create(taskId,
                                                                                                           TaskStatus.Error),
                                                                                            });
                                           }
                                           ExecutingTasks.TryRemove(taskId,
                                                                    out var _);


                                           ResultHandler!.NbResponse++;
                                         },
                      };
      ;

      DemoRun = new DemoTests(configuration_,
                              address,
                              LoggerFactory,
                              taskOptions,
                              methodName,
                              ResultHandler,
                              "");

      Monitoring = Watcher();

      AsyncTaskIds = new List<Task<string>>();
    }

    public BackgroundWorker BgWorkerSubmit { get; set; }

    public int FindRowsByTaskId(string taskId)
    {
      try
      {
        for (var idx = 0; idx < TableSession.Rows.Count; idx++)
        {
          if (TableSession.Rows[idx]
                          .Cells["TaskId"]
                          .Value != null)
          {
            if (TableSession.Rows[idx]
                            .Cells["TaskId"]
                            .Value.ToString()
                            .Equals(taskId))
            {
              return idx;
            }
          }
        }
      }
      catch (Exception e)
      {
        return -1;
      }
      

      return -1;
    }

    public ConcurrentDictionary<string, TaskStatus> ExecutingTasks = new ConcurrentDictionary<string, TaskStatus>();

    public Task Watcher()
    {
      var task = Task.Run(() =>
                          {
                            var indexNewRow = 0;
                            while (!CancellationToken.IsCancellationRequested)
                            {
                              if (ExecutingTasks.Any())
                              {
                                var executingTasksKeys = ExecutingTasks.Keys.ToList();
                                var taskStatus = DemoRun.ServiceAdmin.AdminMonitoringService.GetTaskStatus(executingTasksKeys)
                                                        .ToList();

                                frmMain.Instance.UcActivities.AddOrUpdateTasks(taskStatus);

                              }

                              if (!AsyncTaskIds.Any())
                              {
                                Thread.Sleep(100);
                                continue;
                              }


                              foreach (var task in AsyncTaskIds)
                              {
                                if (task.IsCompleted)
                                {
                                  var idx = FindRowsByTaskId(task.Result);
                                  if (idx == -1)
                                  {
                                    var dataGridViewRow = TableSession.Rows[Offset + indexNewRow++];
                                    dataGridViewRow.Cells["Status"]
                                                   .Value = "Running";
                                    dataGridViewRow.Cells["ResultStatus"]
                                                   .Value = "Waiting for Result...";

                                    dataGridViewRow.Cells["SessionId"]
                                                   .Value = DemoRun.Service.SessionId;
                                    dataGridViewRow.Cells["UserName"]
                                                   .Value = "ddubuc-shock";
                                    dataGridViewRow.Cells["StartTime"]
                                                   .Value = DateTime.Now.ToString(Format,
                                                                                  culture_);
                                    dataGridViewRow.Cells["TaskId"]
                                                   .Value = task.Result;
                                  }

                                  ExecutingTasks[task.Result] = TaskStatus.Submitted;
                                }
                                else if (task.IsCanceled)

                                {
                                  logger_.LogError("Task {taskId} was canceled");
                                  AsyncTaskIds.Remove(task);
                                }

                                else if (task.IsFaulted)

                                {
                                  logger_.LogError(task.Exception?.InnerException,
                                                   task.Exception?.Message);
                                  AsyncTaskIds.Remove(task);
                                }
                              }

                              if (AsyncTaskIds.Any() && AsyncTaskIds.All(t => t.IsCompleted) && AsyncTaskIds.Count() <= ResultHandler.NbResponse)
                              {
                                AsyncTaskIds.Clear();
                                indexNewRow = 0;
                              }
                            }
                          });
      return task;
    }

    public int Offset { get; set; }


    public MetroGrid TableSession { get; set; }

    public DemoTests DemoRun { get; set; }

    public ILoggerFactory LoggerFactory { get; set; }

    public void Submit(int  nbTasks          = 1,
                       long nbInputBytes     = 64,
                       long nbOutputBytes    = 8,
                       int  workloadTimeInMs = 1)
    {
      AsyncTaskIds.AddRange(DemoRun.LargePayloadSubmit(nbTasks,
                                                       nbInputBytes,
                                                       nbOutputBytes,
                                                       workloadTimeInMs));
    }

    public ResultForStressTestsHandler ResultHandler { get; set; }

    public void Dispose()
    {
      CancellationToken.Cancel();
      Monitoring.Wait();

      Monitoring?.Dispose();

      CancellationToken?.Dispose();


      LoggerFactory?.Dispose();
    }
  }
}
