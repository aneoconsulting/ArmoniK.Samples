using System.IO;
using System.Linq;
using System.Threading;

using ArmoniK.DevelopmentKit.Worker.Unified;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.Unified.Worker.Services
{
  public class ServiceAppsAddition : TaskSubmitterWorkerService, ICheckHealth
  {
    private const    int                          MAX_CALLS = 10; // Worker becomes unhealthy after 10 calls
    private static   int                          _callCount;
    private readonly ILogger<ServiceAppsAddition> _logger;

    public ServiceAppsAddition()
    {
      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();

      var configuration = builder.Build();

      Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft",
                                                                   LogEventLevel.Information)
                                            .ReadFrom.Configuration(configuration)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Console()
                                            .CreateBootstrapLogger();

      var logProvider = new SerilogLoggerProvider(Log.Logger);
      var factory = new LoggerFactory(new[]
                                      {
                                        logProvider,
                                      });

      _logger = factory.CreateLogger<ServiceAppsAddition>();
      _logger.LogError("ServiceAppsAddition started");
    }

    public bool CheckHealth()
    {
      var currentCount = _callCount;
      var isHealthy    = currentCount < MAX_CALLS;

      _logger.LogError($"HealthCheck: {currentCount}/{MAX_CALLS} - {(isHealthy ? "HEALTHY" : "UNHEALTHY")}");

      return isHealthy;
    }

    public double AddArray(double[] numbers)
    {
      var currentCall = Interlocked.Increment(ref _callCount);
      _logger.LogError($"AddArray #{currentCall}: {numbers.Length} numbers");

      return numbers.Sum();
    }

    public double AddTwoNumbers(double a,
                                double b)
    {
      var currentCall = Interlocked.Increment(ref _callCount);
      _logger.LogError($"AddTwoNumbers #{currentCall}: {a} + {b}");

      return a + b;
    }

    public string TestWorkerIdentity()
    {
      _logger.LogError("Worker identity test");
      return "ServiceAppsAddition working";
    }

    public string GetCallCount()
    {
      var count = _callCount;
      _logger.LogError($"Call count: {count}/{MAX_CALLS}");
      return $"Calls: {count}/{MAX_CALLS}";
    }
  }
}
