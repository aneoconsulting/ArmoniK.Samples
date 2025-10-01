using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Armonik.Samples.StressTests.Client
{
  internal static class StressTestLogging
  {
    public static void LogTestHeader(ILogger logger, string testId, int nbTasks, long nbInputBytes, long nbOutputBytes, int workloadTimeInMs)
    {
      logger.LogInformation("================================================================================");
      logger.LogInformation($"Test Id: {testId}");
      logger.LogInformation($"Tasks Count      : {nbTasks:N0}");
      logger.LogInformation($"Input size (base): {nbInputBytes / 1024.0:N1} KB");
      logger.LogInformation($"Output size (base): {nbOutputBytes / 1024.0:N1} KB");
      logger.LogInformation($"Workload per task: {workloadTimeInMs} ms");
      logger.LogInformation("================================================================================");
    }

    public static void LogSubmissionComplete(ILogger logger, int nbTasks, long nbInputBytes, Stopwatch submissionSw)
    {
      logger.LogInformation($"Submission completed: {nbTasks:N0} tasks in {submissionSw.Elapsed.TotalSeconds:N2} s");
      logger.LogInformation($"Total data submitted: {nbTasks * nbInputBytes / 1024.0 / 1024.0:N2} MB");
    }

    public static void LogResultsAnalysis(ILogger logger, int nbTasks, Stopwatch waitSw, int nbResults, int nbErrors)
    {
      var missing = nbTasks - (nbResults + nbErrors);
      logger.LogInformation($"Results collected: {nbResults}/{nbTasks} success, {nbErrors} errors, {missing} missing");
      logger.LogInformation($"Wait duration: {waitSw.Elapsed.TotalSeconds:N2} s");
    }

    public static void LogTestFooter(ILogger logger, string testId, TimeSpan totalDuration)
    {
      logger.LogInformation($"Test {testId} finished in {totalDuration.TotalSeconds:N2} s");
    }

    public static void LogPerformanceStatistics(ILogger logger, int nbTasks, long nbInputBytes, long nbOutputBytes, int workloadTimeInMs, DateTime testStartTime, string jsonPath)
    {
      var elapsed = DateTime.Now - testStartTime;
      logger.LogInformation($"Performance for {nbTasks} tasks over {elapsed.TotalSeconds:N1} s");
      logger.LogInformation($"  Input size: {nbInputBytes / 1024.0:N1} KB, Output size: {nbOutputBytes / 1024.0:N1} KB");
      if (!string.IsNullOrEmpty(jsonPath))
      {
        logger.LogInformation($"JSON report path: {jsonPath}");
      }
    }
  }
}
