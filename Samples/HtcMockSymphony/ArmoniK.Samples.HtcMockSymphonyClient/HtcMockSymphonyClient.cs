using System.Diagnostics;
using System.Text;

using ArmoniK.DevelopmentKit.SymphonyApi.Client.api;
using Htc.Mock.Core;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMockSymphonyClient
{
  public class HtcMockSymphonyClient
  {

    private readonly SessionService _sessionService;
    private readonly ILogger<Htc.Mock.Client> _logger;
    public HtcMockSymphonyClient(SessionService sessionService, ILogger<Htc.Mock.Client> logger)
    {
      _sessionService = sessionService;
      _logger = logger;
    }

    public void Start(RunConfiguration config)
    {
      _logger.LogInformation("Start new run with {configuration}",
        config.ToString());
      var watch = Stopwatch.StartNew();

      var request = config.BuildRequest(out int[] shape, _logger);

      var taskId = _sessionService.SubmitTask(DataAdapter.BuildPayload(config, request));

      _logger.LogInformation("Submitted root task {taskId}",
        taskId);
      _sessionService.WaitForTaskCompletion(taskId);

      var result = Encoding.Default.GetString(_sessionService.GetResult(taskId));

      _logger.LogWarning("Final result is {result}",
        result);
      _logger.LogWarning("Expected result is 1.{result}",
        string.Join(".",
          shape));

      watch.Stop();
      _logger.LogWarning("Client was executed in {time}s",
        watch.Elapsed.TotalSeconds);
    }
  }
}
