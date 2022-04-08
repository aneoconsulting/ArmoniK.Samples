// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.SymphonyApi.Client;
using ArmoniK.DevelopmentKit.SymphonyApi.Client.api;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.Samples.HtcMock.Client;
using ArmoniK.Samples.HtcMockSymphonyClient;
using Google.Protobuf.WellKnownTypes;
using Htc.Mock.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Armonik.Samples.HtcMockSymphony.Client
{
  internal class Program
  {
    private static IConfiguration   _configuration;
    private static ILogger<Program> _logger;

    private static void Main(string[] args)
    {
      Console.WriteLine("Hello Armonik HtcMock SymphonyLike Sample !");


      var armonikWaitClient = Environment.GetEnvironmentVariable("ARMONIK_DEBUG_WAIT_CLIENT");
      if (!string.IsNullOrEmpty(armonikWaitClient))
      {
        var armonikDebugWaitClient = int.Parse(armonikWaitClient);

        if (armonikDebugWaitClient > 0)
        {
          Console.WriteLine($"Debug: Sleep {armonikDebugWaitClient} seconds");
          Thread.Sleep(armonikDebugWaitClient * 1000);
        }
      }

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();

      _configuration = builder.Build();

      Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Override("Microsoft",
                                          LogEventLevel.Information)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .CreateBootstrapLogger();


      var factory = new LoggerFactory(new[]
      {
        new SerilogLoggerProvider(new LoggerConfiguration()
                                  .ReadFrom
                                  .Configuration(_configuration)
                                  .CreateLogger()),
      });

      _logger = factory.CreateLogger<Program>();

      var client = new ArmonikSymphonyClient(_configuration,
                                             factory);

      //get environment variable
      var _ = Environment.GetEnvironmentVariable("ARMONIK_DEBUG_WAIT_TASK");

      _logger.LogInformation("Configure taskOptions");
      var taskOptions = InitializeSimpleTaskOptions();


      var sessionService = client.CreateSession(taskOptions);

      _logger.LogInformation($"New session created : {sessionService}");

      var runConfiguration = new RunConfiguration(
        new TimeSpan(0, 0, 0, 0, 100), 100, 1, 1, 4);

      var htcClient = new HtcMockSymphonyClient(sessionService, factory.CreateLogger<Htc.Mock.Client>());

      _logger.LogInformation("Running Small HtcMock test, 1 execution");
      ClientSeqExec(htcClient, runConfiguration, 1);
    }

    /// <summary>
    ///   Initialize Setting for task i.e :
    ///   Duration :
    ///   The max duration of a task
    ///   Priority :
    ///   Work in Progress. Setting priority of task
    ///   AppName  :
    ///   The name of the Application dll (Without Extension)
    ///   VersionName :
    ///   The version of the package to unzip and execute
    ///   Namespace :
    ///   The namespace where the service can find
    ///   the ServiceContainer object develop by the customer
    /// </summary>
    /// <returns></returns>
    private static TaskOptions InitializeSimpleTaskOptions()
    {
      TaskOptions taskOptions = new()
      {
        MaxDuration = new Duration
        {
          Seconds = 300,
        },
        MaxRetries = 5,
        Priority   = 1,
      };
      taskOptions.Options.Add(AppsOptions.GridAppNameKey,
                              "ArmoniK.Samples.HtcMockSymphonyPackage");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "2.0.0");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.Samples.HtcMockSymphony.Packages");

      return taskOptions;
    }

    /// <summary>
    ///   Simple function to wait and get the Result from subTasking and Result delegation
    ///   to a subTask
    /// </summary>
    /// <param name="sessionService">The sessionService API to connect to the Control plane Service</param>
    /// <param name="taskId">The task which is waiting for</param>
    /// <returns></returns>
    private static byte[] WaitForTaskResult(SessionService sessionService, string taskId)
    {
      var taskResult = sessionService.GetResult(taskId);

      return taskResult;
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="sessionService"></param>
    private static void ClientSeqExec(HtcMockSymphonyClient client, RunConfiguration runConfiguration, int nRun)
    {
      var sw = Stopwatch.StartNew();
      for (var i = 0; i < nRun; i++)
      {
        client.Start(runConfiguration);
      }
      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      var stat = new SimpleStats
      {
        EllapsedTime = elapsedMilliseconds,
        Test         = "SeqExec",
        NRun         = nRun,
      };
      Console.WriteLine("JSON Result : " + stat.ToJson());
    }
  }
}
