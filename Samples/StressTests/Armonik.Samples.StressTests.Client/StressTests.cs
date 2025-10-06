// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2025. All rights reserved.
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
// along with this program.  If not, see <http://www.gnu.org/licenses/\>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;
using ArmoniK.DevelopmentKit.Client.Common.Status;
using ArmoniK.DevelopmentKit.Client.Common.Submitter;
using ArmoniK.DevelopmentKit.Client.Unified.Factory;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.Samples.Common;

using Armonik.Samples.StressTests.Client.Metrics;

using ArmoniK.Utils;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Armonik.Samples.StressTests.Client
{
  internal class StressTests
  {
    private readonly ObjectPool<GrpcChannel>? channelPool_;

    public StressTests(IConfiguration configuration,
                       ILoggerFactory factory,
                       string         partition,
                       int            nbTaskPerBufferValue,
                       int            nbBufferPerChannelValue,
                       int            nbChannel)
    {
      TaskOptions = new TaskOptions
                    {
                      MaxDuration = new Duration
                                    {
                                      Seconds = 3600 * 24,
                                    },
                      MaxRetries           = 3,
                      Priority             = 1,
                      EngineType           = EngineType.Unified.ToString(),
                      ApplicationVersion   = "1.0.0-700",
                      ApplicationService   = "ServiceApps",
                      ApplicationName      = "Armonik.Samples.StressTests.Worker",
                      ApplicationNamespace = "Armonik.Samples.StressTests.Worker",
                      PartitionId          = partition,
                    };

      Props = new Properties(configuration,
                             TaskOptions)
              {
                MaxConcurrentBuffers = nbBufferPerChannelValue,
                MaxTasksPerBuffer    = nbTaskPerBufferValue,
                MaxParallelChannels  = nbChannel,
                TimeTriggerBuffer    = TimeSpan.FromSeconds(10),
              };


      Logger = factory.CreateLogger<StressTests>();

      channelPool_ = ClientServiceConnector.ControlPlaneConnectionPool(Props,
                                                                       factory);

      Service = ServiceFactory.CreateService(Props,
                                             factory);

      ResultHandle = new ResultForStressTestsHandler(Logger);
    }

    private ResultForStressTestsHandler ResultHandle { get; }

    public ILogger<StressTests> Logger { get; set; }

    public Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    private ISubmitterService Service { get; }

    /// <summary>
    ///  A test submitting a number of tasks with customizable parameters
    /// </summary>
    /// <param name="nbTasks"></param>
    /// <param name="nbInputBytes"></param>
    /// <param name="nbOutputBytes"></param>
    /// <param name="workloadTimeInMs"></param>
    /// <param name="jsonPath"></param>
    /// <param name="submissionDelayMs"></param>
    /// <param name="payloadVariation"></param>
    /// <param name="outputVariation"></param>
    /// <param name="variationDistribution"></param>
    internal void LargePayloadSubmit(int nbTasks = 100,
                                     long nbInputBytes = 64000,
                                     long nbOutputBytes = 8,
                                     int workloadTimeInMs = 1,
                                     string jsonPath = "",
                                     int submissionDelayMs = 0,
                                     int payloadVariation = 0,
                                     int outputVariation = 0,
                                     string variationDistribution = "uniform")
    {
      var inputArrayOfBytes = Enumerable.Range(0,
                                               (int)(nbInputBytes / 8))
                                        .Select(x => Math.Pow(42.0 * 8 / nbInputBytes,
                                                              1 / 3.0))
                                        .ToArray(); // 8 bytes per double

      StressTestLogging.LogTestHeader(Logger, "LargePayloadSubmit", nbTasks, nbInputBytes, nbOutputBytes, workloadTimeInMs);
  StressTestLogging.LogParameters(Logger, submissionDelayMs, payloadVariation, outputVariation, variationDistribution);
      var sw = Stopwatch.StartNew();
      var dt = DateTime.Now;

      var periodicInfo = ComputeVector(nbTasks,
                   inputArrayOfBytes,
                   nbInputBytes,
                   payloadVariation,
                   variationDistribution,
                   nbOutputBytes,
                   outputVariation,
                   submissionDelayMs,
                   workloadTimeInMs); 
      StressTestLogging.LogSubmissionComplete(Logger, nbTasks, nbInputBytes, sw);

      var waitSw = Stopwatch.StartNew();
      ResultHandle.WaitForResult(nbTasks,
                                 new CancellationToken())
                  .Wait();
      waitSw.Stop();

      StressTestLogging.LogFinalResults(Logger, waitSw.Elapsed, ResultHandle.SubmittedTaskIds?.Count ?? 0, ResultHandle.ReceivedTaskCount, ResultHandle.NbResults, ResultHandle.NbErrors);

      // Debug discrepancy
      var expectedTotal = ResultHandle.ReceivedTaskCount;
      var actualTotal = ResultHandle.NbResults + ResultHandle.NbErrors;
      if (expectedTotal != actualTotal)
      {
        StressTestLogging.LogDiscrepancy(Logger, expectedTotal, actualTotal);
      }

      // Check for missing tasks and display their IDs
      var totalReceived = ResultHandle.NbResults + ResultHandle.NbErrors;
      var missingCount = nbTasks - totalReceived;

      if (missingCount > 0)
      {
        var missingIds = ResultHandle.GetMissingIds().ToList();
        StressTestLogging.LogMissingTasks(Logger, missingCount, missingIds);
      }
      else
      {
        Logger.LogInformation("All tasks completed successfully - no missing tasks detected");
      }

      periodicInfo.Dispose();
      var sb = new StringBuilder();

      var stats = new TasksStats(nbTasks,
                                 nbInputBytes,
                                 nbOutputBytes,
                                 workloadTimeInMs,
                                 Props);

  // Store test parameters into stats for report (explicit properties)
      try
      {
        stats.SubmissionDelayMs = submissionDelayMs > 0 ? submissionDelayMs : null;
        stats.PayloadVariationPercent = payloadVariation > 0 ? payloadVariation : null;
        stats.OutputVariationPercent = outputVariation > 0 ? outputVariation : null;
        stats.VariationDistribution = !string.IsNullOrEmpty(variationDistribution) ? variationDistribution : null;

        // read endpoint from environment if set (the runner exports Grpc__Endpoint)
        var envEndpoint = Environment.GetEnvironmentVariable("Grpc__Endpoint");
        if (!string.IsNullOrEmpty(envEndpoint))
        {
          stats.Endpoint = envEndpoint;
        }
      }
      catch
      {
        throw;
      }

      using var channel = channelPool_.Get();
      stats.GetAllStats(channel,
                        Service.SessionId,
                        dt,
                        DateTime.Now)
           .Wait();

      if (!string.IsNullOrEmpty(jsonPath))
      {
        stats.PrintToJson(jsonPath)
             .Wait();
      }

      Logger.LogInformation(stats.PrintToText()
                                 .Result);


      Service.Dispose(); // Close the session
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="nbInputBytes">The number of element n x M in the vector</param>
    /// <param name="nbOutputBytes">The number of bytes to expect as result</param>
    /// <param name="workloadTimeInMs">The time spent to compute task</param>
    private IDisposable ComputeVector(int      nbTasks,
                                      double[] inputArrayOfBytes,
                                      long     nbInputBytes,
                                      int      payloadVariation,
                                      string   variationDistribution,
                                      long     nbOutputBytes,
                                      int      outputVariation,
                                      int      submissionDelayMs,
                                      int      workloadTimeInMs = 1)
    {
      var indexTask = 0;
      const int elapsed = 30;

      var periodicInfo = Utils.PeriodicInfo(() =>
                                              { 
                                                StressTestLogging.LogPeriodicInfo(Logger, ResultHandle.NbResults, (indexTask == nbTasks));
                                              },
                                            elapsed);

      // Prepare per-task sizes if variations requested
      var random = new Random();
      var payloadSizes = new List<long>(nbTasks);
      var outputSizes = new List<long>(nbTasks);

      // Precompute all sizes to avoid delays during submission
      for (int i = 0; i < nbTasks; i++)
      {
        long payloadSize = nbInputBytes;
        if (payloadVariation > 0)
        {
          double variation = 0.0;
          if (variationDistribution == "gaussian")
          {
            // simple gaussian using Box-Muller
            var u1 = 1.0 - random.NextDouble();
            var u2 = 1.0 - random.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            variation = randStdNormal * (payloadVariation / 100.0);
          }
          else
          {
            variation = (random.NextDouble() - 0.5) * 2.0 * (payloadVariation / 100.0);
          }
          payloadSize = Math.Max(8, (long)(nbInputBytes * (1.0 + variation)));
        }
        payloadSizes.Add(payloadSize);

        long outSize = nbOutputBytes;
        if (outputVariation > 0)
        {
          double variation = (variationDistribution == "gaussian")
            ? (Math.Sqrt(-2.0 * Math.Log(1.0 - random.NextDouble())) * Math.Sin(2.0 * Math.PI * (1.0 - random.NextDouble()))) * (outputVariation / 100.0)
            : (random.NextDouble() - 0.5) * 2.0 * (outputVariation / 100.0);
          outSize = Math.Max(8, (long)(nbOutputBytes * (1.0 + variation)));
        }
        outputSizes.Add(outSize);
      }
      // Submit all tasks
      // Note: we use ToHashSet to force immediate evaluation of the enumerable and avoid delays
      var taskIds = Service.Submit("ComputeWorkLoad",
                                   Enumerable.Range(0, nbTasks)
                                             .Select(i =>
                                             {
                                               if (submissionDelayMs > 0)
                                               {
                                                 Thread.Sleep(submissionDelayMs);
                                               }

                                               var inputArray = payloadSizes[i] != nbInputBytes
                                                 ? Enumerable.Range(0, (int)(payloadSizes[i] / 8)).Select(x => Math.Pow(42.0 * 8 / payloadSizes[i], 1.0 / 3.0)).ToArray()
                                                 : inputArrayOfBytes;

                                               return Utils.ParamsHelper(inputArray,
                                                                         outputSizes[i],
                                                                         workloadTimeInMs);
                                             }),
                                   ResultHandle)
                           .ToHashSet();

      // Store submitted task IDs for missing task detection
      ResultHandle.SubmittedTaskIds = taskIds;
      StressTestLogging.LogRegisteredTaskIds(Logger, taskIds.Count);

      indexTask = taskIds.Count();

      return periodicInfo;
    }

    /// <summary>
    ///  Handler for results and errors from the service
    /// </summary>
    private class ResultForStressTestsHandler : IServiceInvocationHandler
    {
      private readonly ILogger<StressTests> logger_;

      public ResultForStressTestsHandler(ILogger<StressTests> Logger)
        => logger_ = Logger;

      private int nbResults_;
      private int nbErrors_;
      
      public int NbResults => nbResults_;
      public int NbErrors => nbErrors_;
      public double Total     { get; private set; }

      // Track submitted and received task IDs for missing task detection
      public ISet<string> SubmittedTaskIds { get; set; } = new HashSet<string>();
      private readonly ConcurrentDictionary<string, byte> receivedTaskIds_ = new ConcurrentDictionary<string, byte>();
      
      public int ReceivedTaskCount => receivedTaskIds_.Count;

      /// <summary>
      ///   The callBack method which has to be implemented to retrieve error or exception
      /// </summary>
      /// <param name="e">The exception sent to the client from the control plane</param>
      /// <param name="taskId">The task identifier which has invoke the error callBack</param>
      public void HandleError(ServiceInvocationException e,
                              string                     taskId)

      {
        try 
        {
          logger_.LogDebug($"HandleError called for task {taskId}: {e.Message}");
          
          Interlocked.Increment(ref nbErrors_);
          logger_.LogDebug($"Error count incremented to {nbErrors_} for task {taskId}");

          // Track this task as received (even if error)
          if (!string.IsNullOrEmpty(taskId))
          {
            receivedTaskIds_.TryAdd(taskId, 0);
            logger_.LogDebug($"Task {taskId} marked as received (error). Total received: {receivedTaskIds_.Count}");
          }

          if (e.StatusCode == ArmonikStatusCode.TaskCancelled)
          {
            logger_.LogWarning($"Warning from {taskId} : " + e.Message);
          }
          else
          {
            logger_.LogError($"Error from {taskId} : " + e.Message);
            throw new ApplicationException($"Error from {taskId}",
                                           e);
          }
        }
        catch (Exception ex)
        {
          logger_.LogError($"Exception in HandleError for task {taskId}: {ex}");
          throw;
        }
      }

      /// <summary>
      ///   The callBack method which has to be implemented to retrieve response from the server
      /// </summary>
      /// <param name="response">The object receive from the server as result the method called by the client</param>
      /// <param name="taskId">The task identifier which has invoke the response callBack</param>
      public void HandleResponse(object response,
                                 string taskId)
      {
        try 
        {
          logger_.LogDebug($"HandleResponse called for task {taskId}");
          
          switch (response)
          {
            case double[] doubles:
              Total += doubles.Sum();
              break;
            case null:
              logger_.LogInformation("Task finished but nothing returned in Result");
              break;
          }

          // Track this task as received (success)
          if (!string.IsNullOrEmpty(taskId))
          {
            receivedTaskIds_.TryAdd(taskId, 0);
            logger_.LogDebug($"Task {taskId} marked as received (success). Total received: {receivedTaskIds_.Count}");
          }

          Interlocked.Increment(ref nbResults_);
          logger_.LogDebug($"Result count incremented to {nbResults_} for task {taskId}");
        }
        catch (Exception ex)
        {
          logger_.LogError($"Exception in HandleResponse for task {taskId}: {ex}");
          throw;
        }
      }

      /// <summary>
      /// Wait for all results to be received, or timeout after 10 minutes
      /// </summary>
      /// <param name="nbTasks"></param>
      /// <param name="token"></param>
      /// <returns></returns>
      public async Task WaitForResult(int nbTasks,
                                      CancellationToken token)
      {
        var timeout = TimeSpan.FromMinutes(10);
        var sw = Stopwatch.StartNew();

        while (NbResults + NbErrors < nbTasks && !token.IsCancellationRequested && sw.Elapsed < timeout)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(100),
                           token);
        }

        if (sw.Elapsed >= timeout)
        {
          logger_.LogWarning($"Timeout reached after {timeout.TotalMinutes} minutes. Got {NbResults + NbErrors}/{nbTasks} results");
        }
      }

      /// <summary>
      /// Get the list of submitted task IDs that were never received as callbacks
      /// </summary>
      public IEnumerable<string> GetMissingIds()
      {
        if (SubmittedTaskIds == null || SubmittedTaskIds.Count == 0)
        {
          return Enumerable.Empty<string>();
        }

        return SubmittedTaskIds.Where(id => !receivedTaskIds_.ContainsKey(id));
      }
    }
  }
}
