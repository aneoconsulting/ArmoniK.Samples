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

using static System.String;

namespace Armonik.Samples.Symphony.Client
{
  internal class Program
  {
    private static IConfiguration   _configuration;
    private static ILogger<Program> _logger;
    private static string[]         arguments;

    private static void Main(string[] args)
    {
      arguments = args;

      Console.WriteLine("Hello Armonik SymphonyLike Sample !");

      var armonikWaitClient = Environment.GetEnvironmentVariable("ARMONIK_DEBUG_WAIT_CLIENT");
      if (!IsNullOrEmpty(armonikWaitClient))
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

      var factory = new LoggerFactory(new[]
                                      {
                                        new SerilogLoggerProvider(new LoggerConfiguration()
                                                                  .ReadFrom
                                                                  .KeyValuePairs(_configuration.AsEnumerable())
                                                                  .MinimumLevel.Override("Microsoft",
                                                                                         LogEventLevel.Information)
                                                                  .Enrich.FromLogContext()
                                                                  .WriteTo.Console()
                                                                  .CreateLogger()),
                                      },
                                      new LoggerFilterOptions().AddFilter("Grpc",
                                                                          LogLevel.Error));


      _logger = factory.CreateLogger<Program>();

      var client = new ArmonikSymphonyClient(_configuration,
                                             factory);

      //get environment variable
      var _ = Environment.GetEnvironmentVariable("ARMONIK_DEBUG_WAIT_TASK");

      _logger.LogInformation("Configure taskOptions");
      var taskOptions = InitializeSimpleTaskOptions();


      var sessionService = client.CreateSession(taskOptions);

      _logger.LogInformation($"New session created : {sessionService}");


      ModeExecutor(arguments,
                   sessionService);
    }

    private static void ModeExecutor(string[] argv, SessionService sessionService)
    {
      var usage = $"Usage : ./ArmoniK.Samples.SymphonyClient \n" + 
                  "or subTask nbRun nbVectorElements\n" + 
                  "or pTask nbParallel_task [workload_Time_In_Ms]\n" + 
                  "or randomFailure [[nbTasks] | [nbTasks percentageOfFailure]]\n" +
                  "or largePayloads [[nbTasks] | [nbTasks payloadSize]]";

      if (argv is not { Length: >= 1 })
      {
        ExecuteVectorSubtasking(sessionService,
                                1,
                                3);

        ExecuteLargeSubmissionSquare(sessionService,
                                     0);
        return;
      }

      if (string.Equals(argv[0],
                        "subtask",
                        StringComparison.CurrentCultureIgnoreCase))
      {
        if (argv.Length <= 2)
        {
          ExecuteVectorSubtasking(sessionService,
                                  1,
                                  3);
        }
        else
        {
          ExecuteVectorSubtasking(sessionService,
                                  int.Parse(argv[1]),
                                  int.Parse(argv[2]));
        }
      }
      else if (string.Equals(argv[0],
                             "pTask",
                             StringComparison.CurrentCultureIgnoreCase))
      {
        ExecuteLargeSubmissionSquare(sessionService,
                                     argv.Length < 2 ? 0 : int.Parse(argv[1]),
                                     argv.Length < 3 ? 0 : int.Parse(argv[2]));
      }
      else if (string.Equals(argv[0].ToLower(),
                             "randomFailure",
                             StringComparison.CurrentCultureIgnoreCase))
      {
        ExecuteRandomTasksFailure(sessionService,
                                  argv.Length < 2 ? 0 : int.Parse(argv[1]),
                                  argv.Length < 3 ? 0 : int.Parse(argv[2]));
      }
      else if (string.Equals(argv[0].ToLower(),
                             "largePayloads",
                             StringComparison.CurrentCultureIgnoreCase))
      {
        if (argv.Length == 2)
        {
          ExecuteLargePayloads(sessionService,
                               int.Parse(argv[1]));
        }
        else
        {
          ExecuteLargePayloads(sessionService,
                               int.Parse(argv[1]),
                               int.Parse(argv[2]));
        }
      }
      else if (string.Equals(argv[0],
                             "-h",
                             StringComparison.CurrentCultureIgnoreCase))
      {
        Console.WriteLine(usage);
        System.Environment.Exit(0);
      }
      else
      {
        throw new ArgumentException($"Unknown arguments ${argv[0]} \n {usage}");
      }
    }


    private static void ExecuteLargePayloads(SessionService sessionService, int nbTasks = 100, int payloadSizeByte = 1)
    {
      _logger.LogInformation("Running Large Payload test");

      var rnd       = new Random();
      var dataBytes = new byte[payloadSizeByte * 1024 * 10];
      rnd.NextBytes(dataBytes);

      var clientPayload = new ClientPayload
      {
        Type  = ClientPayload.TaskType.LargePayload,
        Sleep = 0,
        Data  = dataBytes,
      };
      var payload = clientPayload.Serialize();

      var sw = Stopwatch.StartNew();

      var taskIds  = new List<string>(nbTasks);
      var payloads = new List<byte[]>(nbTasks);

      // submit 1 jobs of nbTasks  (default 100)
      for (var i = 0; i < nbTasks; i++)
      {
        payloads.Add(payload);
      }

      taskIds.AddRange(sessionService.SubmitTasks(payloads));

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      _logger.LogInformation("Client submitted {nbTasks} with payloads of size {payloadSizeByte} KBytes tasks in {elapsedSeconds} s",
                             nbTasks,
                             payloadSizeByte * 10,
                             elapsedMilliseconds/1000);
    }

    private static void ExecuteRandomTasksFailure(SessionService sessionService, int nbTasks = 100, double nbFailure = 0.25)
    {
      _logger.LogInformation("Running End to End test to check error management");

      var numbers = new List<int>
      {
        2,
      };
      var clientPayload = new ClientPayload
      {
        Numbers = numbers,
        Type    = ClientPayload.TaskType.RandomFailure,
        Sleep   = 0,
        NbRandomFailure = nbFailure
      };
      var payload = clientPayload.Serialize();

      var sw = Stopwatch.StartNew();

      var taskIds  = new List<string>(nbTasks);
      var payloads = new List<byte[]>(nbTasks);

      // submit 1 jobs of nbTasks  (default 100)
      for (var i = 0; i < nbTasks; i++)
      {
        payloads.Add(payload);
      }

      taskIds.AddRange(sessionService.SubmitTasks(payloads));

      try
      {
        GetTryResults(sessionService,
                      taskIds);
      }
      catch (Exception e)
      {
        _logger.LogError(e,
                         "Expected exception during the results retrieving");
      }


      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      _logger.LogInformation($"Client called {nbTasks} tasks in {elapsedMilliseconds} ms");
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
        MaxRetries = 2,
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
      sessionService.WaitForTaskCompletion(taskId);
      var taskResult = sessionService.GetResult(taskId);

      return taskResult;
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="sessionService"></param>
    /// <param name="nbRun">The number of execution to produce a average time spent</param>
    /// <param name="nbElements">The number of element in the vector to compute</param>
    private static void ExecuteVectorSubtasking(SessionService sessionService, int nbRun = 1, int nbElements = 3)
    {
      _logger.LogInformation("Running End to End test to compute Square value with SubTasking");

      var numbers = Enumerable.Range(1,
                                     nbElements).ToList();

      var timeSpans = new List<TimeSpan>();

      Enumerable.Range(1,
                       nbRun).ToList().ForEach(nRun =>
      {
        //Start Submission tasks
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var payload = new ClientPayload
        {
          IsRootTask = true,
          Numbers    = numbers,
          Type       = ClientPayload.TaskType.ComputeSquare,
        };
        var taskId = sessionService.SubmitTask(payload.Serialize());

        var taskResult = WaitForTaskResult(sessionService,
                                           taskId);
        var result = ClientPayload.Deserialize(taskResult);

        stopWatch.Stop();

        _logger.LogInformation($"Run: {nRun} output Result : {result.Result}");
        var ts = stopWatch.Elapsed;
        timeSpans.Add(ts);
      });
      var tsm = timeSpans.Average();
      // Format and display the TimeSpan value.
      var elapsedTime = $"{tsm.Hours:00}:{tsm.Minutes:00}:{tsm.Seconds:00}.{tsm.Milliseconds / 10:00}";
      _logger.LogInformation($"Time elapsed average for {nbRun} Runs " + elapsedTime);
    }

    /// <summary>
    ///   The ClientStartUp2 is used to check some execution performance
    ///   (Need to investigate performance with this test. Not yet investigate)
    /// </summary>
    /// <param name="sessionService"></param>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="workLoadTimeInMs"></param>
    private static void ExecuteLargeSubmissionSquare(SessionService sessionService, int nbTasks = 0, int workLoadTimeInMs = 10)
    {
      _logger.LogInformation("Running End to End test to check task average time per milliseconds");

      var numbers = new List<int>
      {
        2,
      };
      var clientPayload = new ClientPayload
      {
        Numbers = numbers,
        Type    = ClientPayload.TaskType.ComputeCube,
        Sleep   = workLoadTimeInMs,
      };
      var payload        = clientPayload.Serialize();
      var outputMessages = new StringBuilder();

      if (nbTasks <= 0)
      {
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
        _1_Job_of_N_Tasks(sessionService,
                          payload,
                          100,
                          outputMessages);
        _1_Job_of_N_Tasks(sessionService,
                          payload,
                          200,
                          outputMessages);
        _1_Job_of_N_Tasks(sessionService,
                          payload,
                          500,
                          outputMessages);

        outputMessages.AppendLine("In this series of samples we're creating N batchs of M jobs of 1 task.");

        N_Jobs_of_1_Task_With_Results_At_The_End(sessionService,
                                                 payload,
                                                 1,
                                                 1,
                                                 outputMessages);
        N_Jobs_of_1_Task_With_Results_At_The_End(sessionService,
                                                 payload,
                                                 1,
                                                 10,
                                                 outputMessages);
        N_Jobs_of_1_Task_With_Results_At_The_End(sessionService,
                                                 payload,
                                                 1,
                                                 1000,
                                                 outputMessages);
      }
      else
      {
        outputMessages.AppendLine($"In this series of samples we're creating 1 batch of {nbTasks} jobs of 1 task.");
        clientPayload.Type = ClientPayload.TaskType.ParallelTask;
        payload = clientPayload.Serialize();
        N_Jobs_of_1_Task_With_Results_At_The_End(sessionService,
                                                 payload,
                                                 1,
                                                 nbTasks,
                                                 outputMessages);
      }

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
      var finalResult = 0L;
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
      outputMessages.AppendLine($"Client called {nbJobs} jobs of one task in {elapsedMilliseconds} ms aggregated Result = {finalResult}");
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
      var finalResult = 0L;
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
      var ids      = taskIds.ToList();
      var missing  = ids;
      var results  = new List<Tuple<string, byte[]>>();
      var cts      = new CancellationTokenSource();
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
        missing.Batch(10000).ToList().ForEach(bucket =>
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
    /// <param name="workLoadTimeInMs">WorkloadTime in miliseconds</param>
    private static void N_Jobs_of_1_Task_With_Results_At_The_End(SessionService sessionService,
                                                                 byte[]         payload,
                                                                 int            nbBatchs,
                                                                 int            totalNbJobs,
                                                                 StringBuilder  outputMessages)
    {
      var sw        = Stopwatch.StartNew();
      var batchSize = totalNbJobs / nbBatchs;
      var restJobs  = totalNbJobs % batchSize;
      var taskIds   = new List<string>(totalNbJobs);

      // submit nbBatchs batchs of batchSize jobs of 1 task
      for (var i = 0; i < nbBatchs; i++)
      {
        var payloads = new List<byte[]>(batchSize);
        for (var j = 0; j < batchSize; j++)
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

      outputMessages.AppendLine(
        $"Client called (Results_At_The_End) {nbBatchs} batchs of {batchSize} jobs of one task in {sw.ElapsedMilliseconds / 1000} sec (only job creation time)");

      var finalResult = 0L;
      sw.Restart();
      var results = GetTryResults(sessionService,
                                  taskIds).ToList();
      var requestedTaskCount = taskIds.Count;
      foreach (var resultItem in results)
      {
        var result = ClientPayload.Deserialize(resultItem.Item2);
        if (result.Result == 0)
          _logger.LogError($"The taskId {resultItem.Item1} returns [{result.Result}]");
        finalResult += result.Result;
      }

      outputMessages.AppendLine($"  => requested/received {requestedTaskCount}/{results.Count} in {sw.ElapsedMilliseconds / 1000} sec (only job creation time)");
      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      outputMessages.AppendLine(
        $"Client called (Results_At_The_End) {nbBatchs} batchs of {batchSize} jobs of one task in {elapsedMilliseconds / 1000} sec agregated Result = {finalResult}");
    }
  }
}

public static class TimeSpanExt
{
  /// <summary>
  /// Calculates the average of the given timeSpans.
  /// </summary>
  public static TimeSpan Average(this IEnumerable<TimeSpan> timeSpans)
  {
    IEnumerable<long> ticksPerTimeSpan = timeSpans.Select(t => t.Ticks);
    double            averageTicks     = ticksPerTimeSpan.Average();
    long              averageTicksLong = Convert.ToInt64(averageTicks);

    TimeSpan averageTimeSpan = TimeSpan.FromTicks(averageTicksLong);

    return averageTimeSpan;
  }
}