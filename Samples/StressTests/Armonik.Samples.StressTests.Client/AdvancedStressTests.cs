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
using System.Text.Json;

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
  /// <summary>
  /// Advanced ArmoniK Stress Test Suite
  /// Comprehensive testing framework for ArmoniK performance and reliability
  /// </summary>
  internal class AdvancedStressTests
  {
    private readonly ObjectPool<GrpcChannel>? channelPool_;
    private readonly StressTests baseStressTest_;
    private readonly string reportDirectory_;

    public AdvancedStressTests(IConfiguration configuration,
                               ILoggerFactory factory,
                               string         partition,
                               int            nbTaskPerBufferValue,
                               int            nbBufferPerChannelValue,
                               int            nbChannel)
    {
      // Initialize base stress test for reuse
      baseStressTest_ = new StressTests(configuration, factory, partition, 
                                       nbTaskPerBufferValue, nbBufferPerChannelValue, nbChannel);

      TaskOptions = new TaskOptions
                    {
                      MaxDuration = new Duration { Seconds = 3600 * 24 },
                      MaxRetries           = 3,
                      Priority             = 1,
                      EngineType           = EngineType.Unified.ToString(),
                      ApplicationVersion   = "1.0.0-700",
                      ApplicationService   = "ServiceApps",
                      ApplicationName      = "Armonik.Samples.StressTests.Worker",
                      ApplicationNamespace = "Armonik.Samples.StressTests.Worker",
                      PartitionId          = partition,
                    };

      Props = new Properties(configuration, TaskOptions)
              {
                MaxConcurrentBuffers = nbBufferPerChannelValue,
                MaxTasksPerBuffer    = nbTaskPerBufferValue,
                MaxParallelChannels  = nbChannel,
                TimeTriggerBuffer    = TimeSpan.FromSeconds(10),
              };

      Logger = factory.CreateLogger<AdvancedStressTests>();

      channelPool_ = ClientServiceConnector.ControlPlaneConnectionPool(Props, factory);
      Service = ServiceFactory.CreateService(Props, factory);

      // Create reports directory
      reportDirectory_ = Path.Combine(Environment.CurrentDirectory, "stress-test-reports", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
      Directory.CreateDirectory(reportDirectory_);
    }

    // Logger for detailed test output
    public ILogger<AdvancedStressTests> Logger { get; set; }
    /// ArmoniK configuration properties
    public Properties Props { get; set; }
    // Task options for submissions
    public TaskOptions TaskOptions { get; set; }
    // Submitter service for task submissions
    private ISubmitterService Service { get; }

    /// <summary>
    /// Comprehensive stress test suite with multiple test scenarios
    /// </summary>
    public async Task RunComprehensiveStressTest()
    {
      var suiteStartTime = DateTime.Now;
      var suiteId = Guid.NewGuid().ToString("N")[..8];
      var testResults = new List<TestResult>();

      LogSuiteHeader(suiteId);

      try
      {
        // Test Scenario 1: High Volume Quick Tasks
        Logger.LogInformation("Starting Test Scenario 1: High Volume Quick Tasks");
        var result1 = await RunTestScenario("HighVolumeQuick", 5000, 512000, 8, 10);
        testResults.Add(result1);

        // Test Scenario 2: Medium Volume Standard Tasks
        Logger.LogInformation("Starting Test Scenario 2: Medium Volume Standard Tasks");
        var result2 = await RunTestScenario("MediumVolumeStandard", 2000, 1024000, 8, 100);
        testResults.Add(result2);

        // Test Scenario 3: Low Volume Heavy Tasks
        Logger.LogInformation("Starting Test Scenario 3: Low Volume Heavy Tasks");
        var result3 = await RunTestScenario("LowVolumeHeavy", 500, 2048000, 8, 1000);
        testResults.Add(result3);

        // Test Scenario 4: Burst Test (Multiple rapid submissions)
        Logger.LogInformation("Starting Test Scenario 4: Burst Load Test");
        var result4 = await RunBurstTest("BurstLoad", 3, 1000, 512000, 8, 50);
        testResults.Add(result4);

        // Parameter sweep: volumetry × payload/output variability × submission latency
        Logger.LogInformation("Starting Test Scenario 5: Volumetry and Payload Variation and Latency Test");
        var volumes = new[] { 500 };
        var payloadVariations = new[] { 0, 20 };
        var submissionDelays = new[] { 0, 5 }; // ms

        // Sweep through combinations 
        foreach (var v in volumes)
        {
          // Sweep through payload variations 
          foreach (var pv in payloadVariations)
          {
            // Sweep through submission delays
            foreach (var sd in submissionDelays)
            {
              var name = $"ParamSweep_v{v}_pv{pv}_d{sd}";
              Logger.LogInformation($"Starting parameter test: {name}");
              var res = await RunParamTest(name, v, 512000, 8192, 100, sd, pv, 0, "gaussian"); // Launch a parameterized test with Gaussian payload variation
              testResults.Add(res);
            }
          }
        }

  // Generate comprehensive report (JSON only)
  await GenerateComprehensiveReport(suiteId, suiteStartTime, testResults);

        LogSuiteFooter(suiteId, DateTime.Now - suiteStartTime, testResults);
      }
      catch (Exception ex)
      {
        Logger.LogError($"Stress test suite failed: {ex.Message}");
        throw;
      }
      finally
      {
        Service.Dispose();
      }
    }

    /// <summary>
    /// Run a single test scenario with detailed metrics collection
    /// </summary>
    private async Task<TestResult> RunTestScenario(string scenarioName, int nbTasks, long nbInputBytes, 
                                                   long nbOutputBytes, int workloadTimeInMs)
    {
      var testStart = DateTime.Now;
      var testId = Guid.NewGuid().ToString("N")[..8];
      
      Logger.LogInformation($"========== Test Scenario: {scenarioName} ==========");
      Logger.LogInformation($"Test ID: {testId}");
      Logger.LogInformation($"Tasks: {nbTasks:N0}, Payload: {nbInputBytes/1024.0:N1} KB, Workload: {workloadTimeInMs} ms");

      var result = new TestResult
      {
        TestId = testId,
        ScenarioName = scenarioName,
        StartTime = testStart,
        TaskCount = nbTasks,
        PayloadSizeKB = nbInputBytes / 1024.0,
        WorkloadTimeMs = workloadTimeInMs
      };

      try
      {
        // Create a new result handler for this test
        var resultHandler = new AdvancedResultHandler(Logger, testId);
        
        // Execute the test using the base stress test functionality
        var submissionSw = Stopwatch.StartNew();
        var taskIds = await SubmitTasks(nbTasks, nbInputBytes, nbOutputBytes, workloadTimeInMs, resultHandler);
        submissionSw.Stop();

        result.SubmissionDuration = submissionSw.Elapsed;
        result.SubmissionThroughput = nbTasks / submissionSw.Elapsed.TotalSeconds;

        // Wait for completion
        var executionSw = Stopwatch.StartNew();
        await resultHandler.WaitForResult(nbTasks, CancellationToken.None);
        executionSw.Stop();

        result.ExecutionDuration = executionSw.Elapsed;
        result.ExecutionThroughput = nbTasks / executionSw.Elapsed.TotalSeconds;
        result.SuccessfulTasks = resultHandler.NbResults;
        result.FailedTasks = resultHandler.NbErrors;
        result.MissingTasks = nbTasks - (resultHandler.NbResults + resultHandler.NbErrors);
        result.SuccessRate = (double)resultHandler.NbResults / nbTasks * 100;
        result.EndTime = DateTime.Now;
        result.IsSuccessful = result.MissingTasks == 0 && result.FailedTasks == 0;

        // Save individual test report
        await SaveTestReport(result);

        Logger.LogInformation($"Test {scenarioName} completed: {result.SuccessfulTasks}/{nbTasks} tasks successful ({result.SuccessRate:N1}%)");
        
        return result;
      }
      catch (Exception ex)
      {
        result.EndTime = DateTime.Now;
        result.IsSuccessful = false;
        result.ErrorMessage = ex.Message;
        Logger.LogError($"Test {scenarioName} failed: {ex.Message}");
        return result;
      }
    }

    /// <summary>
    /// Run burst test - multiple rapid successive submissions
    /// </summary>
    private async Task<TestResult> RunBurstTest(string scenarioName, int burstCount, int tasksPerBurst, 
                                                long nbInputBytes, long nbOutputBytes, int workloadTimeInMs)
    {
      var testStart = DateTime.Now;
      var testId = Guid.NewGuid().ToString("N")[..8];
      var totalTasks = burstCount * tasksPerBurst;
      
      Logger.LogInformation($"========== Burst Test: {scenarioName} ==========");
      Logger.LogInformation($"Test ID: {testId}");
      Logger.LogInformation($"Bursts: {burstCount}, Tasks per burst: {tasksPerBurst}, Total: {totalTasks:N0}");
      // Create a new result handler for this test
      var result = new TestResult
      {
        TestId = testId,
        ScenarioName = scenarioName,
        StartTime = testStart,
        TaskCount = totalTasks,
        PayloadSizeKB = nbInputBytes / 1024.0,
        WorkloadTimeMs = workloadTimeInMs
      };

      try
      {
        // Initialize result handler
        var resultHandler = new AdvancedResultHandler(Logger, testId);
        var allTaskIds = new List<string>();

        // Submit bursts with small delays
        var submissionSw = Stopwatch.StartNew();
        for (int burst = 0; burst < burstCount; burst++)
        {
          Logger.LogInformation($"Submitting burst {burst + 1}/{burstCount}...");
          var taskIds = await SubmitTasks(tasksPerBurst, nbInputBytes, nbOutputBytes, workloadTimeInMs, resultHandler);
          allTaskIds.AddRange(taskIds);
          
          if (burst < burstCount - 1)
            await Task.Delay(1000); // 1 second between bursts
        }
        submissionSw.Stop();

        result.SubmissionDuration = submissionSw.Elapsed;
        result.SubmissionThroughput = totalTasks / submissionSw.Elapsed.TotalSeconds;

        // Wait for all tasks to complete
        var executionSw = Stopwatch.StartNew();
        await resultHandler.WaitForResult(totalTasks, CancellationToken.None);
        executionSw.Stop();

        // Collect metrics
        result.ExecutionDuration = executionSw.Elapsed;
        result.ExecutionThroughput = totalTasks / executionSw.Elapsed.TotalSeconds;
        result.SuccessfulTasks = resultHandler.NbResults;
        result.FailedTasks = resultHandler.NbErrors;
        result.MissingTasks = totalTasks - (resultHandler.NbResults + resultHandler.NbErrors);
        result.SuccessRate = (double)resultHandler.NbResults / totalTasks * 100;
        result.EndTime = DateTime.Now;
        result.IsSuccessful = result.MissingTasks == 0 && result.FailedTasks == 0;

        await SaveTestReport(result);

        Logger.LogInformation($"Burst test {scenarioName} completed: {result.SuccessfulTasks}/{totalTasks} tasks successful ({result.SuccessRate:N1}%)");
        
        return result;
      }
      catch (Exception ex)
      {
        result.EndTime = DateTime.Now;
        result.IsSuccessful = false;
        result.ErrorMessage = ex.Message;
        Logger.LogError($"Burst test {scenarioName} failed: {ex.Message}");
        return result;
      }
    }

    /// <summary>
    /// Submit tasks and return task IDs
    /// </summary>
    private Task<List<string>> SubmitTasks(int nbTasks, long nbInputBytes, long nbOutputBytes,
                                                 int workloadTimeInMs, AdvancedResultHandler resultHandler,
                                                 int submissionDelayMs = 0, int payloadVariation = 0, int outputVariation = 0, string variationDistribution = "uniform")
    {
      // If no variation and no delay, submit in bulk
      if (submissionDelayMs <= 0 && payloadVariation <= 0 && outputVariation <= 0)
      {
        var inputArrayOfBytes = Enumerable.Range(0, (int)(nbInputBytes / 8))
                                         .Select(x => Math.Pow(42.0 * 8 / nbInputBytes, 1 / 3.0))
                                         .ToArray();

        var taskIds = Service.Submit("ComputeWorkLoad",
                                     Enumerable.Range(0, nbTasks)
                                               .Select(_ => Utils.ParamsHelper(inputArrayOfBytes, nbOutputBytes, workloadTimeInMs)),
                                     resultHandler)
                             .ToList();

        resultHandler.SubmittedTaskIds = new HashSet<string>(taskIds);
        return Task.FromResult(taskIds);
      }

      // Iterative submission: create generators and submit one-by-one (or small batches)
      var payloadGenerator = new PayloadGenerator(nbInputBytes, payloadVariation, variationDistribution, Logger);
      var outputGenerator = new PayloadGenerator(nbOutputBytes, outputVariation, variationDistribution, Logger);

      var ids = new List<string>();
      for (int i = 0; i < nbTasks; i++)
      {
        var inputPayload = payloadGenerator.GeneratePayload();
        var outSize = outputGenerator.GenerateSize();

        // compute approximate input size from generator
        var inputSize = (long)inputPayload.Length * 8L;

        try
        {
          TaskOptions.Options["PayloadSize"] = inputSize.ToString();
        }
        catch { }

        var batch = Service.Submit("ComputeWorkLoad",
                                   new[] { Utils.ParamsHelper(inputPayload, outSize, workloadTimeInMs) },
                                   resultHandler);

        ids.AddRange(batch);

        if (submissionDelayMs > 0 && i < nbTasks - 1)
        {
          Thread.Sleep(submissionDelayMs);
        }
      }

      payloadGenerator.LogStatistics("Input Payload");
      outputGenerator.LogStatistics("Output Size");

      resultHandler.SubmittedTaskIds = new HashSet<string>(ids);
      return Task.FromResult(ids);
    }

    /// <summary>
    /// Run a single parameterized test (volumetry, variation, latency)
    /// </summary>
    private async Task<TestResult> RunParamTest(string scenarioName, int nbTasks, long nbInputBytes, long nbOutputBytes, int workloadTimeInMs,
                                                int submissionDelayMs, int payloadVariation, int outputVariation, string variationDistribution)
    {
      var testStart = DateTime.Now;
      var testId = Guid.NewGuid().ToString("N")[..8];

      Logger.LogInformation($"========== Param Test: {scenarioName} ==========");
      Logger.LogInformation($"Test ID: {testId}");
      Logger.LogInformation($"Tasks: {nbTasks:N0}, Payload base: {nbInputBytes/1024.0:N1} KB, payloadVar: {payloadVariation}%, delay: {submissionDelayMs} ms");

      // Create a new result handler for this test
      var result = new TestResult
      {
        TestId = testId,
        ScenarioName = scenarioName,
        StartTime = testStart,
        TaskCount = nbTasks,
        PayloadSizeKB = nbInputBytes / 1024.0,
        WorkloadTimeMs = workloadTimeInMs
      };

      try
      {
        var resultHandler = new AdvancedResultHandler(Logger, testId);

        var submissionSw = Stopwatch.StartNew();
        var taskIds = await SubmitTasks(nbTasks, nbInputBytes, nbOutputBytes, workloadTimeInMs, resultHandler, submissionDelayMs, payloadVariation, outputVariation, variationDistribution);
        submissionSw.Stop();

        result.SubmissionDuration = submissionSw.Elapsed;
        result.SubmissionThroughput = nbTasks / submissionSw.Elapsed.TotalSeconds;

        var executionSw = Stopwatch.StartNew();
        await resultHandler.WaitForResult(nbTasks, CancellationToken.None);
        executionSw.Stop();

        result.ExecutionDuration = executionSw.Elapsed;
        result.ExecutionThroughput = nbTasks / executionSw.Elapsed.TotalSeconds;
        result.SuccessfulTasks = resultHandler.NbResults;
        result.FailedTasks = resultHandler.NbErrors;
        result.MissingTasks = nbTasks - (resultHandler.NbResults + resultHandler.NbErrors);
        result.SuccessRate = (double)resultHandler.NbResults / nbTasks * 100;
        result.EndTime = DateTime.Now;
        result.IsSuccessful = result.MissingTasks == 0 && result.FailedTasks == 0;

        await SaveTestReport(result);

        Logger.LogInformation($"Param test {scenarioName} completed: {result.SuccessfulTasks}/{nbTasks} tasks successful ({result.SuccessRate:N1}%)");
        return result;
      }
      catch (Exception ex)
      {
        result.EndTime = DateTime.Now;
        result.IsSuccessful = false;
        result.ErrorMessage = ex.Message;
        Logger.LogError($"Param test {scenarioName} failed: {ex.Message}");
        return result;
      }
    }

    /// <summary>
    /// Generate comprehensive test suite report
    /// </summary>
    private async Task GenerateComprehensiveReport(string suiteId, DateTime suiteStartTime, List<TestResult> testResults)
    {
      var reportPath = Path.Combine(reportDirectory_, "comprehensive-report.json");

      var report = new ComprehensiveReport
      {
        SuiteId = suiteId,
        StartTime = suiteStartTime,
        EndTime = DateTime.Now,
        TotalDuration = DateTime.Now - suiteStartTime,
        Environment = new EnvironmentInfo
        {
          MachineName = Environment.MachineName,
          ProcessorCount = Environment.ProcessorCount,
          OSVersion = Environment.OSVersion.ToString(),
          WorkingSet = Environment.WorkingSet,
          ArmoniKConfiguration = new ArmoniKConfig
          {
            MaxTasksPerBuffer = Props.MaxTasksPerBuffer,
            MaxParallelChannels = Props.MaxParallelChannels,
            MaxConcurrentBuffers = Props.MaxConcurrentBuffers,
            Partition = TaskOptions.PartitionId,
            MaxRetries = TaskOptions.MaxRetries
          }
        },
        TestResults = testResults,
        Summary = new TestSummary
        {
          TotalTests = testResults.Count,
          SuccessfulTests = testResults.Count(r => r.IsSuccessful),
          FailedTests = testResults.Count(r => !r.IsSuccessful),
          TotalTasks = testResults.Sum(r => r.TaskCount),
          TotalSuccessfulTasks = testResults.Sum(r => r.SuccessfulTasks),
          TotalFailedTasks = testResults.Sum(r => r.FailedTasks),
          TotalMissingTasks = testResults.Sum(r => r.MissingTasks),
          OverallSuccessRate = testResults.Sum(r => r.SuccessfulTasks) * 100.0 / testResults.Sum(r => r.TaskCount),
          AverageExecutionThroughput = testResults.Where(r => r.ExecutionThroughput > 0).Average(r => r.ExecutionThroughput),
          PeakExecutionThroughput = testResults.Where(r => r.ExecutionThroughput > 0).Max(r => r.ExecutionThroughput)
        }
      };

      // Save JSON report
      var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
      var jsonReport = JsonSerializer.Serialize(report, jsonOptions);
      await File.WriteAllTextAsync(reportPath, jsonReport);

     Logger.LogInformation($"Comprehensive report saved to: {reportPath}");
    }



    /// <summary>
    /// Save individual test report
    /// </summary>
    private async Task SaveTestReport(TestResult result)
    {
      var fileName = $"test-{result.ScenarioName}-{result.TestId}.json";
      var filePath = Path.Combine(reportDirectory_, fileName);
      
      var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
      var json = JsonSerializer.Serialize(result, jsonOptions);
      await File.WriteAllTextAsync(filePath, json);
    }

    private void LogSuiteHeader(string suiteId)
    {
      Logger.LogInformation("");
      Logger.LogInformation("################################################################################");
      Logger.LogInformation("                    ARMONIK ADVANCED STRESS TEST SUITE");
      Logger.LogInformation("################################################################################");
      Logger.LogInformation($"Suite ID         : {suiteId}");
      Logger.LogInformation($"Start Time       : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
      Logger.LogInformation($"Report Directory : {reportDirectory_}");
      Logger.LogInformation("################################################################################");
      Logger.LogInformation("");
    }

    private void LogSuiteFooter(string suiteId, TimeSpan totalDuration, List<TestResult> results)
    {
      var successfulTests = results.Count(r => r.IsSuccessful);
      var totalTasks = results.Sum(r => r.TaskCount);
      var successfulTasks = results.Sum(r => r.SuccessfulTasks);
      var overallSuccessRate = successfulTasks * 100.0 / totalTasks;

      Logger.LogInformation("");
      Logger.LogInformation("################################################################################");
      Logger.LogInformation("                         STRESS TEST SUITE COMPLETED");
      Logger.LogInformation("################################################################################");
      Logger.LogInformation($"Suite ID         : {suiteId}");
      Logger.LogInformation($"Total Duration   : {totalDuration:mm\\:ss\\.ff}");
      Logger.LogInformation($"Tests Executed   : {results.Count} ({successfulTests} successful, {results.Count - successfulTests} failed)");
      Logger.LogInformation($"Total Tasks      : {totalTasks:N0}");
      Logger.LogInformation($"Successful Tasks : {successfulTasks:N0} ({overallSuccessRate:N1}%)");
      Logger.LogInformation($"Reports Location : {reportDirectory_}");
      Logger.LogInformation("################################################################################");
      Logger.LogInformation("");
    }

    /// <summary>
    /// Advanced result handler with enhanced tracking
    /// </summary>
    private class AdvancedResultHandler : IServiceInvocationHandler
    {
      private readonly ILogger logger_;
      private readonly string testId_;
      private int nbResults_;
      private int nbErrors_;

      public AdvancedResultHandler(ILogger logger, string testId)
      {
        logger_ = logger;
        testId_ = testId;
      }

      public int NbResults => nbResults_;
      public int NbErrors => nbErrors_;
      public double Total { get; private set; }

      public ISet<string> SubmittedTaskIds { get; set; } = new HashSet<string>();
      private readonly ConcurrentDictionary<string, byte> receivedTaskIds_ = new ConcurrentDictionary<string, byte>();

      public void HandleError(ServiceInvocationException e, string taskId)
      {
        try
        {
          if (!string.IsNullOrEmpty(taskId))
          {
            receivedTaskIds_.TryAdd(taskId, 0);
          }

          Interlocked.Increment(ref nbErrors_);

          if (e.StatusCode != ArmonikStatusCode.TaskCancelled)
          {
            logger_.LogWarning($"Task error in test {testId_}: {taskId} - {e.Message}");
          }
        }
        catch (Exception ex)
        {
          logger_.LogError($"Exception in HandleError for test {testId_}, task {taskId}: {ex}");
        }
      }

      public void HandleResponse(object? response, string taskId)
      {
        try
        {
          switch (response)
          {
            case double[] doubles:
              Total += doubles.Sum();
              break;
          }

          if (!string.IsNullOrEmpty(taskId))
          {
            receivedTaskIds_.TryAdd(taskId, 0);
          }

          Interlocked.Increment(ref nbResults_);
        }
        catch (Exception ex)
        {
          logger_.LogError($"Exception in HandleResponse for test {testId_}, task {taskId}: {ex}");
        }
      }

      public async Task WaitForResult(int nbTasks, CancellationToken token)
      {
        var timeout = TimeSpan.FromMinutes(15); // Longer timeout for comprehensive tests
        var sw = Stopwatch.StartNew();

        while (NbResults + NbErrors < nbTasks && !token.IsCancellationRequested && sw.Elapsed < timeout)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(500), token).ConfigureAwait(false);
        }

        if (sw.Elapsed >= timeout)
        {
          logger_.LogWarning($"Test {testId_} timeout: {NbResults + NbErrors}/{nbTasks} results received");
        }
      }

      public IEnumerable<string> GetMissingIds()
      {
        return SubmittedTaskIds?.Where(id => !receivedTaskIds_.ContainsKey(id)) ?? Enumerable.Empty<string>();
      }
    }
  }

  #region Data Models

  public class TestResult
  {
    public string TestId { get; set; } = "";
    public string ScenarioName { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan SubmissionDuration { get; set; }
    public TimeSpan ExecutionDuration { get; set; }
    public int TaskCount { get; set; }
    public double PayloadSizeKB { get; set; }
    public int WorkloadTimeMs { get; set; }
    public int SuccessfulTasks { get; set; }
    public int FailedTasks { get; set; }
    public int MissingTasks { get; set; }
    public double SuccessRate { get; set; }
    public double SubmissionThroughput { get; set; }
    public double ExecutionThroughput { get; set; }
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
  }

  public class ComprehensiveReport
  {
    public string SuiteId { get; set; } = "";
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public EnvironmentInfo Environment { get; set; } = new();
    public List<TestResult> TestResults { get; set; } = new();
    public TestSummary Summary { get; set; } = new();
  }

  public class EnvironmentInfo
  {
    public string MachineName { get; set; } = "";
    public int ProcessorCount { get; set; }
    public string OSVersion { get; set; } = "";
    public long WorkingSet { get; set; }
    public ArmoniKConfig ArmoniKConfiguration { get; set; } = new();
  }

  public class ArmoniKConfig
  {
    public int MaxTasksPerBuffer { get; set; }
    public int MaxParallelChannels { get; set; }
    public int MaxConcurrentBuffers { get; set; }
    public string Partition { get; set; } = "";
    public int MaxRetries { get; set; }
  }

  public class TestSummary
  {
    public int TotalTests { get; set; }
    public int SuccessfulTests { get; set; }
    public int FailedTests { get; set; }
    public int TotalTasks { get; set; }
    public int TotalSuccessfulTasks { get; set; }
    public int TotalFailedTasks { get; set; }
    public int TotalMissingTasks { get; set; }
    public double OverallSuccessRate { get; set; }
    public double AverageExecutionThroughput { get; set; }
    public double PeakExecutionThroughput { get; set; }
  }

  #endregion
}