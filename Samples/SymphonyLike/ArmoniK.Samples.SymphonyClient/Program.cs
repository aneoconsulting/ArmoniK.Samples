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
//   D. Brasseur       <dbrasseur@aneo.fr>
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
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.SymphonyApi.Client;
using ArmoniK.DevelopmentKit.SymphonyApi.Client.api;
using ArmoniK.DevelopmentKit.Common;

using Armonik.Samples.Symphony.Common;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Armonik.Samples.Symphony.Client
{
  internal class Program
  {
    private static IConfiguration   _configuration;
    private static ILogger<Program> _logger;

    private static void Main(string[] args)
    {
      Console.WriteLine("Hello Armonik SymphonyLike Sample !");


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

      _logger.LogInformation("Running End to End test to compute Square value with SubTasking");
      ClientStartup1(sessionService);

      _logger.LogInformation("Running End to End test to check task average time per milliseconds");
      ClientStartup2(sessionService);
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
                              "ArmoniK.Samples.SymphonyPackage");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "2.0.0");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.Samples.Symphony.Packages");

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
    private static void ClientStartup1(SessionService sessionService)
    {
      var numbers = new List<int>
      {
        1,
        2,
        3,
      };
      var payload = new ClientPayload
      {
        IsRootTask = true,
        numbers    = numbers,
        Type       = ClientPayload.TaskType.ComputeSquare,
      };
      var taskId = sessionService.SubmitTask(payload.Serialize());

      var taskResult = WaitForTaskResult(sessionService,
                                         taskId);
      var result = ClientPayload.Deserialize(taskResult);

      _logger.LogInformation($"output Result : {result.Result}");
    }

    /// <summary>
    ///   The ClientStartUp2 is used to check some execution performance
    ///   (Need to investigate performance with this test. Not yet investigate)
    /// </summary>
    /// <param name="sessionService"></param>
    private static void ClientStartup2(SessionService sessionService)
    {
      var numbers = new List<int>
      {
        2,
      };
      var clientPayload = new ClientPayload
      {
        numbers = numbers,
        Type    = ClientPayload.TaskType.ComputeCube,
      };
      var payload        = clientPayload.Serialize();
      var outputMessages = new StringBuilder();
      outputMessages.AppendLine("In this series of samples we're creating N jobs of one task.");
      outputMessages.AppendLine(@"In the loop we have :
        1 sending job of one task
        2 wait for Result
        3 get associated payload");
      N_Jobs_of_1_Task(sessionService,
                       payload,
                       1,
                       outputMessages);
      N_Jobs_of_1_Task(sessionService,
                       payload,
                       10,
                       outputMessages);
      //N_Jobs_of_1_Task(sessionService, payload, 100, outputMessages);
      //N_Jobs_of_1_Task(sessionService, payload, 200, outputMessages);
      // N_Jobs_of_1_Task(sessionService, payload, 500, outputMessages);

      outputMessages.AppendLine("In this series of samples we're creating 1 job of N tasks.");

      _1_Job_of_N_Tasks(sessionService,
                        payload,
                        1,
                        outputMessages);
      _1_Job_of_N_Tasks(sessionService,
                        payload,
                        10,
                        outputMessages);
      _1_Job_of_N_Tasks(sessionService, payload, 100, outputMessages);
      _1_Job_of_N_Tasks(sessionService, payload, 200, outputMessages);
      _1_Job_of_N_Tasks(sessionService, payload, 500, outputMessages);

      outputMessages.AppendLine("In this series of samples we're creating N batchs of M jobs of 1 task.");

      N_Jobs_of_1_Task_With_Results_At_The_End(sessionService, payload, 1, 1, outputMessages);
      N_Jobs_of_1_Task_With_Results_At_The_End(sessionService, payload, 1, 10, outputMessages);
      N_Jobs_of_1_Task_With_Results_At_The_End(sessionService, payload, 1, 1000, outputMessages);

      _logger.LogInformation(outputMessages.ToString());
    }

    /// <summary>
    ///   A test to execute Several Job with 1 task by jub
    /// </summary>
    /// <param name="sessionService">The sessionService to connect to the Control plane Service</param>
    /// <param name="payload">A default payload to execute by each task</param>
    /// <param name="nbJobs">The Number of jobs</param>
    /// <param name="outputMessages">The print log stored in a StringBuilder object</param>
    private static void N_Jobs_of_1_Task(SessionService sessionService,
                                         byte[]         payload,
                                         int            nbJobs,
                                         StringBuilder  outputMessages)
    {
      var sw          = Stopwatch.StartNew();
      var finalResult = 0;
      for (var i = 0; i < nbJobs; i++)
      {
        var taskId = sessionService.SubmitTask(payload);
        var taskResult = WaitForTaskResult(sessionService,
                                           taskId);
        var result = ClientPayload.Deserialize(taskResult);
        if (result.Result == 0)
          Log.Error($"The taskId {taskId} returns [{result.Result}]");

        finalResult += result.Result;
      }

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      outputMessages.AppendLine($"Client called {nbJobs} jobs of one task in {elapsedMilliseconds} ms agregated Result = {finalResult}");
    }

    /// <summary>
    ///   The function to execute 1 job with several tasks inside
    /// </summary>
    /// <param name="sessionService">The sessionService to connect to the Control plane Service</param>
    /// <param name="payload">A default payload to execute by each task</param>
    /// <param name="nbTasks">The Number of jobs</param>
    /// <param name="outputMessages">The print log stored in a StringBuilder object</param>
    private static void _1_Job_of_N_Tasks(SessionService sessionService,
                                          byte[]         payload,
                                          int            nbTasks,
                                          StringBuilder  outputMessages)
    {
      var payloads = new List<byte[]>(nbTasks);
      for (var i = 0; i < nbTasks; i++)
        payloads.Add(payload);

      var sw          = Stopwatch.StartNew();
      var finalResult = 0;
      var taskIds     = sessionService.SubmitTasks(payloads);
      foreach (var taskId in taskIds)
      {
        var taskResult = WaitForTaskResult(sessionService,
                                           taskId);
        var result = ClientPayload.Deserialize(taskResult);

        finalResult += result.Result;
      }

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      outputMessages.AppendLine($"Client called {nbTasks} tasks in {elapsedMilliseconds} ms aggregated Result = {finalResult}");
    }

    private static void PeriodicInfo(Action action, int seconds, CancellationToken token = default)
    {
      if (action == null)
        return;
      Task.Run(async () =>
               {
                 while (!token.IsCancellationRequested)
                 {
                   action();
                   await Task.Delay(TimeSpan.FromSeconds(seconds),
                                    token);
                 }
               },
               token);
    }



    private static IEnumerable<Tuple<string, byte[]>> GetTryResults(SessionService sessionService, IEnumerable<string> taskIds)
    {
      var ids       = taskIds.ToList();
      var missing   = ids;
      var results   = new List<Tuple<string, byte[]>>();
      var cts       = new CancellationTokenSource();
      var holdPrev = 0;
      var waitInSeconds = new List<int>
      {
        1000,
        5000,
        10000,
        20000,
        30000
      };
      var idx = 0;

      PeriodicInfo(() => { _logger.LogInformation($"Got {results.Count} / {ids.Count} result(s) "); },
                   20,
                   cts.Token);

      while (missing.Count != 0)
      {
        missing.Batch(100).ToList().ForEach(bucket =>
        {
          var partialResults = sessionService.TryGetResults(bucket);

          var listPartialResults = partialResults.ToList();

          if (listPartialResults.Count() != 0)
          {
            results.AddRange(listPartialResults);
          }

          missing = missing.Where(x => listPartialResults.ToList().All(rId => rId.Item1 != x)).ToList();



          if (holdPrev == results.Count)
          {
            idx = idx >= waitInSeconds.Count - 1 ? waitInSeconds.Count - 1 : idx + 1;
          }
          else
          {
            idx = 0;
          }

          holdPrev = results.Count;

          Thread.Sleep(waitInSeconds[idx]);

        });
      }

      cts.Cancel();

      return results;
    }

    /// <summary>
    ///   The function to execute batchs of several jobs with 1 task each
    /// </summary>
    /// <param name="sessionService">The sessionService to connect to the Control plane Service</param>
    /// <param name="payload">A default payload to execute by each task</param>
    /// <param name="nbBatchs">The Number of batchs of jobs</param>
    /// <param name="totalNbJobs">The total number of jobs (in all batchs)</param>
    /// <param name="outputMessages">The print log stored in a StringBuilder object</param>
    private static void N_Jobs_of_1_Task_With_Results_At_The_End(SessionService sessionService, byte[] payload, int nbBatchs, int totalNbJobs, StringBuilder outputMessages)
    {
        var sw = Stopwatch.StartNew();
        var batchSize = totalNbJobs / nbBatchs;
        var restJobs = totalNbJobs % batchSize;
        List<string> taskIds = new List<string>(totalNbJobs);

        // submit nbBatchs batchs of batchSize jobs of 1 task
        for (var i = 0; i < nbBatchs; i++)
        {
            List<byte[]> payloads = new List<byte[]>(batchSize);
            for(var j = 0; j < batchSize; j++)
            {
                payloads.Add(payload);
            }
            taskIds.AddRange(sessionService.SubmitTasks(payloads));
            payloads.Clear();
        }

        // submit restJobs jobs of 1 task
        for (var i = 0; i < restJobs; i++)
        {
            string taskId = sessionService.SubmitTask(payload);
            taskIds.Add(taskId);
        }

        outputMessages.AppendLine($"Client called (Results_At_The_End) {nbBatchs} batchs of {batchSize} jobs of one task in {sw.ElapsedMilliseconds / 1000} sec (only job creation time)");

        var finalResult = 0;
        sw.Restart();
        List<Tuple<string, byte[]>> results = GetTryResults(sessionService, taskIds).ToList();
        var requestedTaskCount = taskIds.Count;
        foreach (Tuple<string, byte[]> resultItem in results)
        {
            ClientPayload result = ClientPayload.Deserialize(resultItem.Item2);
            if (result.Result == 0)
                _logger.LogError($"The taskId {resultItem.Item1} returns [{result.Result}]");
            finalResult += result.Result;
        }

        outputMessages.AppendLine($"  => requested/received {requestedTaskCount}/{results.Count} in {sw.ElapsedMilliseconds / 1000} sec (only job creation time)");
        var elapsedMilliseconds = sw.ElapsedMilliseconds;
        outputMessages.AppendLine($"Client called (Results_At_The_End) {nbBatchs} batchs of {batchSize} jobs of one task in {elapsedMilliseconds / 1000} sec agregated Result = {finalResult}");
    }
  }
}
