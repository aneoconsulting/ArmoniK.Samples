using System;
using System.Linq;
using System.Threading;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;
using ArmoniK.DevelopmentKit.Client.Common.Submitter;
using ArmoniK.DevelopmentKit.Client.Unified.Factory;
using ArmoniK.DevelopmentKit.Common;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.Client
{
  internal class HealthCheckTest : IDisposable
  {
    public HealthCheckTest(IConfiguration configuration,
                           ILoggerFactory factory)
    {
      TaskOptions = new TaskOptions
                    {
                      MaxDuration = new Duration
                                    {
                                      Seconds = 300,
                                    },
                      MaxRetries           = 1,
                      Priority             = 1,
                      EngineType           = EngineType.Unified.ToString(),
                      ApplicationVersion   = "1.0.0-700",
                      ApplicationService   = "ServiceAppsAddition",
                      ApplicationName      = "ArmoniK.Samples.Unified.Worker",
                      ApplicationNamespace = "ArmoniK.Samples.Unified.Worker.Services",
                    };

      Props = new Properties(configuration,
                             TaskOptions);
      Logger       = factory.CreateLogger<HealthCheckTest>();
      Service      = ServiceFactory.CreateService(Props);
      ResultHandle = new ResultHandler(Logger);
    }

    private ResultHandler            ResultHandle { get; }
    public  ILogger<HealthCheckTest> Logger       { get; set; }
    public  Properties               Props        { get; set; }
    public  TaskOptions              TaskOptions  { get; set; }
    private ISubmitterService        Service      { get; }

    public void Dispose()
      => Service?.Dispose();

    /// <summary>
    ///   Test with 20 tasks:
    ///   - First 10 tasks succeed
    ///   - Worker restarts
    ///   - Next 10 tasks succeed
    /// </summary>
    public void SimpleAdditionTest(int nbTasks = 20) // Default 20 tasks
    {
      Logger.LogInformation("=== Addition Test: First 10 succeed, worker restarts, next 10 succeed ===");

      var tasks = Service.Submit("AddArray",
                                 Enumerable.Range(1,
                                                  nbTasks)
                                           .Select(i =>
                                                   {
                                                     var numbers = new double[]
                                                                   {
                                                                     i,
                                                                     i + 1,
                                                                   };
                                                     return Common.Utils.ParamsHelper(numbers);
                                                   }),
                                 ResultHandle);

      Logger.LogInformation($"  {tasks.Count()} tasks submitted");

      while (ResultHandle.NbResults + ResultHandle.NbErrors < nbTasks)
      {
        Thread.Sleep(1000);
        var currentTotal = ResultHandle.NbResults + ResultHandle.NbErrors;
        Logger.LogInformation($" Progress: {ResultHandle.NbResults} success, {ResultHandle.NbErrors} errors ({currentTotal}/{nbTasks})");
      }

      Logger.LogInformation("=== Final Results ===");
      Logger.LogInformation($"Successful tasks: {ResultHandle.NbResults}");
      Logger.LogInformation($"Failed tasks: {ResultHandle.NbErrors}");

      if (ResultHandle.NbResults == nbTasks && ResultHandle.NbErrors == 0)
      {
        Logger.LogInformation("  SUCCESS: All tasks completed after worker restart");
      }
      else
      {
        Logger.LogWarning($" Unexpected result: {ResultHandle.NbResults} success, {ResultHandle.NbErrors} errors");
      }
    }

    public void TwoNumbersTest(int nbTasks = 20)
      => SimpleAdditionTest(nbTasks);

    public void ProgressiveTest()
      => SimpleAdditionTest();

    private class ResultHandler : IServiceInvocationHandler
    {
      private readonly ILogger<HealthCheckTest> _logger;
      private volatile int                      _nbErrors;
      private volatile int                      _nbResults;

      public ResultHandler(ILogger<HealthCheckTest> logger)
        => _logger = logger;

      public int NbResults
        => _nbResults;

      public int NbErrors
        => _nbErrors;

      public void HandleError(ServiceInvocationException e,
                              string                     taskId)
      {
        Interlocked.Increment(ref _nbErrors);
        _logger.LogWarning("Task {taskId} failed: {message}",
                           taskId,
                           e.Message);
      }

      public void HandleResponse(object response,
                                 string taskId)
      {
        Interlocked.Increment(ref _nbResults);
        _logger.LogDebug("Task {taskId} → {response}",
                         taskId,
                         response);
      }
    }
  }
}
