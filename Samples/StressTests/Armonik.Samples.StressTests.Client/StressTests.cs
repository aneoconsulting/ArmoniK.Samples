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
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

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

    internal void LargePayloadSubmit(int    nbTasks               = 100,
                                     long   nbInputBytes          = 64000,
                                     long   nbOutputBytes         = 8,
                                     int    workloadTimeInMs      = 1,
                                     string jsonPath              = "",
                                     int    submissionDelayMs     = 0,
                                     int    payloadVariation      = 0,
                                     int    outputVariation       = 0,
                                     string variationDistribution = "uniform")
    {
      // Test initialization
      var testStartTime = DateTime.Now;
      var testId = Guid.NewGuid().ToString("N")[..8];
      
      StressTestLogging.LogTestHeader(Logger, testId, nbTasks, nbInputBytes, nbOutputBytes, workloadTimeInMs);
      
      // Log variation settings if enabled
      if (payloadVariation > 0 || outputVariation > 0)
      {
        Logger.LogInformation("Payload Variability Configuration:");
        if (payloadVariation > 0)
        {
          Logger.LogInformation($"  Input size variation : ±{payloadVariation}% (base: {nbInputBytes / 1024.0:N1} KB)");
        }
        if (outputVariation > 0)
        {
          Logger.LogInformation($"  Output size variation: ±{outputVariation}% (base: {nbOutputBytes / 1024.0:N1} KB)");
        }
        Logger.LogInformation($"  Distribution type    : {variationDistribution}");
      }

      // Phase 1: Task Submission
      Logger.LogInformation("Phase 1: Starting task submission...");
      var submissionSw = Stopwatch.StartNew();
      
      var periodicInfo = ComputeVector(nbTasks,
                                       nbInputBytes,
                                       nbOutputBytes,
                                       workloadTimeInMs,
                                       submissionDelayMs,
                                       payloadVariation,
                                       outputVariation,
                                       variationDistribution);
      
      submissionSw.Stop();
  StressTestLogging.LogSubmissionComplete(Logger, nbTasks, nbInputBytes, submissionSw);

      // Phase 2: Task Execution and Results Collection
      Logger.LogInformation("Phase 2: Waiting for task completion...");
      var waitSw = Stopwatch.StartNew();
      
      ResultHandle.WaitForResult(nbTasks, new CancellationToken()).Wait();
      
      waitSw.Stop();
      periodicInfo.Dispose();

      // Phase 3: Results Analysis
      Logger.LogInformation("Phase 3: Analyzing results...");
  StressTestLogging.LogResultsAnalysis(Logger, nbTasks, waitSw, ResultHandle.NbResults, ResultHandle.NbErrors);

      // Phase 4: Performance Statistics
      Logger.LogInformation("Phase 4: Gathering performance statistics...");
  StressTestLogging.LogPerformanceStatistics(Logger, nbTasks, nbInputBytes, nbOutputBytes, workloadTimeInMs, testStartTime, jsonPath);

      // Phase 5: JSON Report Generation
      if (!string.IsNullOrEmpty(jsonPath))
      {
        Logger.LogInformation("Phase 5: Generating JSON report...");
        try
        {
          var taskStats = new TasksStats(nbTasks, taskOptions, Service.TasksClient.Channel, Service.SessionId);
          await taskStats.PrintToJson(jsonPath);
          Logger.LogInformation($"JSON report saved to: {jsonPath}");
        }
        catch (Exception ex)
        {
          Logger.LogError(ex, $"Failed to generate JSON report at {jsonPath}");
        }
      }

      Service.Dispose();
  StressTestLogging.LogTestFooter(Logger, testId, DateTime.Now - testStartTime);
    }

    // Logging methods moved to StressTestLogging.cs

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="nbInputBytes">The base number of bytes for input payload</param>
    /// <param name="nbOutputBytes">The base number of bytes for output payload</param>
    /// <param name="workloadTimeInMs">The time spent to compute task</param>
    /// <param name="submissionDelayMs">The delay in milliseconds between task submissions (0 = no delay)</param>
    /// <param name="payloadVariation">Payload size variation in percent (0-100)</param>
    /// <param name="outputVariation">Output size variation in percent (0-100)</param>
    /// <param name="variationDistribution">Distribution type: uniform, gaussian, exponential</param>
    private IDisposable ComputeVector(int    nbTasks,
                                      long   nbInputBytes,
                                      long   nbOutputBytes       = 8,
                                      int    workloadTimeInMs    = 1,
                                      int    submissionDelayMs   = 0,
                                      int    payloadVariation    = 0,
                                      int    outputVariation     = 0,
                                      string variationDistribution = "uniform")
    {
      var       indexTask = 0;
      const int elapsed   = 30;

      var periodicInfo = Utils.PeriodicInfo(() =>
                                            {
                                              var completed = ResultHandle.NbResults + ResultHandle.NbErrors;
                                              var completionRate = indexTask > 0 ? (completed * 100.0 / indexTask) : 0;
                                              var throughput = completed / Math.Max(1, elapsed);
                                              
                                              Logger.LogInformation($"Progress Update          : {completed}/{indexTask} tasks ({completionRate:N1}%) - Rate: {throughput:N1}/sec");
                                            },
                                            elapsed);

      Logger.LogInformation("Submitting tasks to ArmoniK...");
      
      // Create payload generator with variation support
      var payloadGenerator = new PayloadGenerator(nbInputBytes, payloadVariation, variationDistribution, Logger);
      var outputGenerator = new PayloadGenerator(nbOutputBytes, outputVariation, variationDistribution, Logger);
      
      HashSet<string> taskIds;
      
      // Determine submission mode based on delay and variation
      bool needsIterativeSubmission = submissionDelayMs > 0 || payloadVariation > 0 || outputVariation > 0;
      
      if (!needsIterativeSubmission)
      {
        // Fast path: No delay, no variation - submit all tasks at once
        var basePayload = payloadGenerator.GeneratePayload();
        // Ensure bench workers receive the payload size via TaskOptions
        try
        {
          TaskOptions.Options["PayloadSize"] = nbInputBytes.ToString();
        }
        catch
        {
          throw new ApplicationException("TaskOptions.Options map is not available"); 
        }
        taskIds = Service.Submit("ComputeWorkLoad",
                                 Enumerable.Range(0, nbTasks)
                                           .Select(_ => Utils.ParamsHelper(basePayload,
                                                                           nbOutputBytes,
                                                                           workloadTimeInMs)),
                                 ResultHandle)
                         .ToHashSet();
      }
      else
      {
        // Iterative submission: with delay and/or variation
        if (submissionDelayMs > 0)
        {
          Logger.LogInformation($"Submission Mode          : Throttled ({submissionDelayMs} ms delay between tasks)");
        }
        if (payloadVariation > 0 || outputVariation > 0)
        {
          Logger.LogInformation($"Submission Mode          : Variable payloads (input: ±{payloadVariation}%, output: ±{outputVariation}%)");
        }
        
        taskIds = new HashSet<string>();
        
        for (int i = 0; i < nbTasks; i++)
        {
          // Generate variable payloads for each task
          var taskInputPayload = payloadGenerator.GeneratePayload();
          var taskOutputSize = outputGenerator.GenerateSize();

          // Compute generated input size from payload array structure (generator uses size/8)
          var taskInputSize = (long)taskInputPayload.Length * 8L;

          // Ensure bench workers receive the payload size via TaskOptions for this submission
          try
          {
            TaskOptions.Options["PayloadSize"] = taskInputSize.ToString();
          }
          catch
          {
            // ignore if map not available for some reason
          }
          
          var batchTaskIds = Service.Submit("ComputeWorkLoad",
                                           new[] { Utils.ParamsHelper(taskInputPayload,
                                                                     taskOutputSize,
                                                                     workloadTimeInMs) },
                                           ResultHandle);
          
          foreach (var id in batchTaskIds)
          {
            taskIds.Add(id);
          }
          
          indexTask = i + 1;
          
          // Apply delay between submissions (except after the last task)
          if (submissionDelayMs > 0 && i < nbTasks - 1)
          {
            Thread.Sleep(submissionDelayMs);
          }
          
          // Log progress every 100 tasks or at completion
          if ((i + 1) % 100 == 0 || i == nbTasks - 1)
          {
            Logger.LogInformation($"Submission Progress      : {i + 1}/{nbTasks} tasks submitted ({(i + 1) * 100.0 / nbTasks:N1}%)");
          }
        }
        
        // Log statistics about generated sizes
        if (payloadVariation > 0 || outputVariation > 0)
        {
          payloadGenerator.LogStatistics("Input Payload");
          outputGenerator.LogStatistics("Output Size");
        }
      }

      // Store submitted task IDs for missing task detection
      ResultHandle.SubmittedTaskIds = taskIds;
      Logger.LogInformation($"Task Registration        : {taskIds.Count:N0} task IDs registered for tracking");

      indexTask = taskIds.Count();

      return periodicInfo;
    }


    private class ResultForStressTestsHandler : IServiceInvocationHandler
    {
      private readonly ILogger<StressTests> Logger_;

      public ResultForStressTestsHandler(ILogger<StressTests> Logger)
        => Logger_ = Logger;

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
          // Track this task as received (even if error)
          if (!string.IsNullOrEmpty(taskId))
          {
            receivedTaskIds_.TryAdd(taskId, 0);
          }

          Interlocked.Increment(ref nbErrors_);

          if (e.StatusCode == ArmonikStatusCode.TaskCancelled)
          {
            Logger_.LogWarning($"Task cancelled           : {taskId} - {e.Message}");
          }
          else
          {
            // Log full exception details (stack trace, status code) to help debugging why tasks are retried.
            try
            {
              Logger_.LogError(e, $"Task error               : {taskId} - StatusCode={e.StatusCode} - Message={e.Message}");
            }
            catch
            {
              // Fallback to simple message if structured logging fails
              Logger_.LogError($"Task error               : {taskId} - {e.Message} (StatusCode={e.StatusCode})");
            }
            // Do not throw here: keep collecting other results and let the harness report failures.
          }
        }
        catch (Exception ex)
        {
          Logger_.LogError($"Exception in HandleError for task {taskId}: {ex}");
          throw;
        }
      }

      /// <summary>
      ///   The callBack method which has to be implemented to retrieve response from the server
      /// </summary>
      /// <param name="response">The object receive from the server as result the method called by the client</param>
      /// <param name="taskId">The task identifier which has invoke the response callBack</param>
      public void HandleResponse(object? response,
                                 string taskId)

      {
        try 
        {
          switch (response)
          {
            case double[] doubles:
              Total += doubles.Sum();
              break;
            case null:
              Logger_.LogDebug($"Task {taskId} completed with no result data");
              break;
          }

          // Track this task as received (success)
          if (!string.IsNullOrEmpty(taskId))
          {
            receivedTaskIds_.TryAdd(taskId, 0);
          }

          Interlocked.Increment(ref nbResults_);
        }
        catch (Exception ex)
        {
          Logger_.LogError($"Exception in HandleResponse for task {taskId}: {ex}");
          throw;
        }
      }

      public async Task WaitForResult(int               nbTasks,
                                      CancellationToken token)
      {
        var timeout = TimeSpan.FromMinutes(10);
        var sw = Stopwatch.StartNew();
        var lastLogTime = DateTime.Now;
        var lastCount = 0;
        
        while (NbResults + NbErrors < nbTasks && !token.IsCancellationRequested && sw.Elapsed < timeout)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(100), token);
          
          // Log progress every 30 seconds
          if (DateTime.Now - lastLogTime > TimeSpan.FromSeconds(30))
          {
            var currentCount = NbResults + NbErrors;
            var rate = (currentCount - lastCount) / 30.0;
            var remaining = nbTasks - currentCount;
            var eta = rate > 0 ? TimeSpan.FromSeconds(remaining / rate) : TimeSpan.Zero;
            
            Logger_.LogInformation($"Execution Progress       : {currentCount}/{nbTasks} tasks ({currentCount * 100.0 / nbTasks:N1}%) - Rate: {rate:N1}/sec - ETA: {eta:mm\\:ss}");
            
            lastLogTime = DateTime.Now;
            lastCount = currentCount;
          }
        }
        
        if (sw.Elapsed >= timeout)
        {
          Logger_.LogWarning($"Execution timeout reached: {timeout.TotalMinutes} minutes - {NbResults + NbErrors}/{nbTasks} results received");
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
