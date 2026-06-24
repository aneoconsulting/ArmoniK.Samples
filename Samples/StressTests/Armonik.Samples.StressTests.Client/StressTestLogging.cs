using System.Diagnostics;

using Microsoft.Extensions.Logging;

namespace Armonik.Samples.StressTests.Client
{
  internal static class StressTestLogging
  {
    public static void LogTestHeader(ILogger logger,
                                     string  testId,
                                     int     nbTasks,
                                     long    nbInputBytes,
                                     long    nbOutputBytes,
                                     int     workloadTimeInMs)
    {
      logger.LogInformation("================================================================================");
      logger.LogInformation($"Test Id: {testId}");
      logger.LogInformation($"Tasks Count      : {nbTasks:N0}");
      logger.LogInformation($"Input size (base): {nbInputBytes   / 1024.0:N1} KB");
      logger.LogInformation($"Output size (base): {nbOutputBytes / 1024.0:N1} KB");
      logger.LogInformation($"Workload per task: {workloadTimeInMs} ms");
      logger.LogInformation("================================================================================");
    }

    public static void LogParameters(ILogger logger,
                                     int     submissionDelayMs,
                                     int     payloadVariation,
                                     int     outputVariation,
                                     string  variationDistribution)
      => logger.LogInformation($"Parameters: submissionDelayMs={submissionDelayMs}, payloadVariation={payloadVariation}%, outputVariation={outputVariation}%, variationDistribution={variationDistribution}");

    public static void LogSubmissionComplete(ILogger   logger,
                                             int       nbTasks,
                                             long      nbInputBytes,
                                             Stopwatch submissionSw)
    {
      logger.LogInformation($"Submission completed: {nbTasks:N0} tasks in {submissionSw.Elapsed.TotalSeconds:N2} s");
      logger.LogInformation($"Total data submitted: {nbTasks * nbInputBytes / 1024.0 / 1024.0:N2} MB");
    }

    public static void LogPeriodicInfo(ILogger logger,
                                       int     nbResults,
                                       bool    allSubmitted)
    {
      if (allSubmitted)
      {
        logger.LogInformation($"Got {nbResults} results. All tasks submitted.");
      }
      else
      {
        // While submission is ongoing, show an informative message so the user
        // knows results are being collected but submission hasn't finished yet.
        logger.LogInformation($"Submission in progress");
      }
    }

    public static void LogFinalResults(ILogger  logger,
                                       TimeSpan waitDuration,
                                       int      submittedTasks,
                                       int      receivedCallbacks,
                                       int      nbResults,
                                       int      nbErrors)
    {
      logger.LogInformation("=== FINAL RESULTS ===");
      logger.LogInformation($"Waited {waitDuration.TotalSeconds:N2} seconds for results");
      logger.LogInformation($"Submitted tasks: {submittedTasks}");
      logger.LogInformation($"Received callbacks: {receivedCallbacks}");
      logger.LogInformation($"Success results: {nbResults}");
      logger.LogInformation($"Error results: {nbErrors}");
      logger.LogInformation($"Total processed: {nbResults + nbErrors}");
    }

    public static void LogDiscrepancy(ILogger logger,
                                      int     expectedTotal,
                                      int     actualTotal)
      => logger.LogError($"DISCREPANCY: Expected {expectedTotal} results based on callbacks, but got {actualTotal} in counters!");

    public static void LogMissingTasks(ILogger               logger,
                                       int                   missingCount,
                                       IReadOnlyList<string> missingIds)
    {
      logger.LogWarning($"MISSING TASKS: {missingCount} tasks did not complete");
      if (missingIds != null && missingIds.Count > 0)
      {
        logger.LogWarning($"Missing Task IDs ({missingIds.Count} identified):");
        for (var i = 0; i < Math.Min(20,
                                     missingIds.Count); i++)
        {
          logger.LogWarning($"  Missing Task ID: {missingIds[i]}");
        }

        if (missingIds.Count > 20)
        {
          logger.LogWarning($"  ... and {missingIds.Count - 20} more missing task IDs");
        }
      }
      else
      {
        logger.LogWarning("Could not identify specific missing task IDs (callback system issue)");
      }
    }

    public static void LogRegisteredTaskIds(ILogger logger,
                                            int     count)
      => logger.LogInformation($"Registered {count} task IDs for tracking");
  }
}
