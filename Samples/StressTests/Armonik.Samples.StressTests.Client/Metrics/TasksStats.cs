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

using System.Text;
using System.Text.Json;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.SortDirection;
using ArmoniK.Api.gRPC.V1.Tasks;
using ArmoniK.DevelopmentKit.Client.Common;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

namespace Armonik.Samples.StressTests.Client.Metrics
{
  internal class TasksStats
  {
    public enum KpiKeys
    {
      TEST                       = 0,
      COMPLETED_TASKS            = 1,
      TIME_SUBMITTED_TASKS       = 2,
      TIME_THROUGHPUT_SUBMISSION = 3,
      TIME_THROUGHPUT_PROCESS    = 4,
      TIME_PROCESSED_TASKS       = 5,
      TIME_RETRIEVE_RESULTS      = 6,
      TIME_THROUGHPUT_RESULTS    = 7,
      TOTAL_TIME                 = 8,

      NB_TASKS            = 9,
      NB_INPUTBYTES       = 10,
      NB_OUTPUTBYTES      = 11,
      TIME_WORKLOAD_IN_MS = 12,

      TASKS_PER_BUFFER                 = 13,
      NB_CHANNEL                       = 14,
      NB_CONCURRENT_BUFFER_PER_CHANNEL = 15,
      UPLOAD_SPEED_KB                  = 16,
      DOWNLOAD_SPEED_KB                = 17,
      NB_POD_USED                      = 18,
    }

    public TasksStats(int        nbTasks,
                      long       nbInputBytes,
                      long       nbOutputBytes,
                      int        workloadTimeInMs,
                      Properties props)
    {
      Kpi[KpiKeys.TEST]                             = "StressTest";
      Kpi[KpiKeys.NB_TASKS]                         = nbTasks.ToString();
      Kpi[KpiKeys.NB_INPUTBYTES]                    = nbInputBytes.ToString();
      Kpi[KpiKeys.NB_OUTPUTBYTES]                   = nbOutputBytes.ToString();
      Kpi[KpiKeys.TIME_WORKLOAD_IN_MS]              = workloadTimeInMs.ToString();
      Kpi[KpiKeys.TASKS_PER_BUFFER]                 = props.MaxTasksPerBuffer.ToString();
      Kpi[KpiKeys.NB_CHANNEL]                       = props.MaxParallelChannels.ToString();
      Kpi[KpiKeys.NB_CONCURRENT_BUFFER_PER_CHANNEL] = props.MaxConcurrentBuffers.ToString();
    }


    public Dictionary<KpiKeys, string> Kpi      { get; set; } = new();
    public IList<TaskDetailed>         TasksRaw { get; set; } = new List<TaskDetailed>();
    public int?    SubmissionDelayMs        { get; set; }
    public int?    PayloadVariationPercent  { get; set; }
    public int?    OutputVariationPercent   { get; set; }
    public string? VariationDistribution    { get; set; }
    public string? Endpoint                 { get; set; }

    ///<summary>
    /// Retrieve all tasks matching the given filter, using pagination.
    /// </summary>
    /// <param name="channel">gRPC channel to use</param>
    /// <param name="filter">Filter to apply</param>
    /// <param name="sort">Sorting options</param>
    private async IAsyncEnumerable<TaskDetailed> RetrieveAllTasksStats(ChannelBase                 channel,
                                                                       Filters                     filter,
                                                                       ListTasksRequest.Types.Sort sort)
    {
      var               read       = 0;
      var               page       = 0;
      var               taskClient = new Tasks.TasksClient(channel);
      ListTasksResponse res;

      while ((res = await taskClient.ListTasksAsync(new ListTasksRequest
                                                    {
                                                      Filters  = filter,
                                                      Sort     = sort,
                                                      PageSize = 50,
                                                      Page     = page,
                                                    })
                                    .ConfigureAwait(false)).Total > read)
      {
        foreach (var taskSummary in res.Tasks)
        {
          var taskRaw = taskClient.GetTask(new GetTaskRequest
                                           {
                                             TaskId = taskSummary.Id,
                                           })
                                  .Task;
          read++;
          yield return taskRaw;
        }

        page++;
      }
    }

    /// <summary>
    /// Retrieve all task statistics for a specific session.
    /// </summary>
    /// <param name="channel">gRPC channel to use</param>
    /// <param name="sessionId">Session identifier</param>
    private async Task GetAllStatsAsync(ChannelBase channel,
                                        string      sessionId)
    {
      await foreach (var taskRaw in RetrieveAllTasksStats(channel,
                                                          new Filters
                                                          {
                                                            Or =
                                                            {
                                                              new FiltersAnd
                                                              {
                                                                And =
                                                                {
                                                                  new FilterField
                                                                  {
                                                                    Field = new TaskField
                                                                            {
                                                                              TaskSummaryField = new TaskSummaryField
                                                                                                 {
                                                                                                   Field = TaskSummaryEnumField.SessionId,
                                                                                                 },
                                                                            },
                                                                    FilterString = new FilterString
                                                                                   {
                                                                                     Operator = FilterStringOperator.Equal,
                                                                                     Value    = sessionId,
                                                                                   },
                                                                  },
                                                                },
                                                              },
                                                            },
                                                          },
                                                          new ListTasksRequest.Types.Sort
                                                          {
                                                            Direction = SortDirection.Asc,
                                                            Field = new TaskField
                                                                    {
                                                                      TaskSummaryField = new TaskSummaryField
                                                                                         {
                                                                                           Field = TaskSummaryEnumField.TaskId,
                                                                                         },
                                                                    },
                                                          })
                       .ConfigureAwait(false))
      {
        TasksRaw.Add(taskRaw);
      }
    }

    /// <summary>
    /// Get the time taken to submit tasks for a specific session.
    /// </summary>
    /// <param name="channel">gRPC channel to use</param>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="start">Start time</param>
    public async Task GetTimeToSubmitTasks(ChannelBase channel,
                                           string sessionId,
                                           DateTime start)
    {
      if (TasksRaw.Count == 0)
      {
        await GetAllStatsAsync(channel,
                               sessionId)
          .ConfigureAwait(false);
      }

      var timeSpentList = TasksRaw.Select(raw => (raw.CreatedAt - Timestamp.FromDateTime(start.ToUniversalTime())).ToTimeSpan()
                                                                                                                  .TotalMilliseconds / 1000)
                                  .ToList();

      Kpi[KpiKeys.COMPLETED_TASKS] = TasksRaw.Count.ToString();
      Kpi[KpiKeys.TIME_SUBMITTED_TASKS] = TimeSpan.FromSeconds(timeSpentList.Max())
                                                  .ToString();

      Kpi[KpiKeys.TIME_THROUGHPUT_SUBMISSION] = (TasksRaw.Count() / timeSpentList.Max()).ToString("F02");
      Kpi[KpiKeys.UPLOAD_SPEED_KB] = (TasksRaw.Count() * (int.Parse(Kpi[KpiKeys.NB_INPUTBYTES]) / 1024.0) / timeSpentList.Max()).ToString("F02");
    }


    /// <summary>
    /// Get the time taken to process tasks for a specific session.
    /// </summary>
    /// <param name="channel">gRPC channel to use</param>
    /// <param name="sessionId">Session identifier</param>
    private async Task GetTimeToProcessTasks(ChannelBase channel,
                                             string sessionId)
    {
      if (TasksRaw.Count == 0)
      {
        await GetAllStatsAsync(channel,
                               sessionId)
          .ConfigureAwait(false);
      }


      var timeDiff = TasksRaw.Select(raw => raw.EndedAt)
                             .Max() - TasksRaw.Select(raw => raw.CreatedAt)
                                              .Min(); // total time between first submission and last completion
      var withMs = timeDiff.Seconds + timeDiff.Nanos / 1e9; // in seconds
      Kpi[KpiKeys.TIME_PROCESSED_TASKS] = TimeSpan.FromSeconds(withMs)
                                                  .ToString();
      Kpi[KpiKeys.TIME_THROUGHPUT_PROCESS] = (TasksRaw.Count() / withMs).ToString("F02");
        // Total time (human readable) and number of distinct pods used
        try
        {
          // total wall-clock time between first creation and last end
          Kpi[KpiKeys.TOTAL_TIME] = TimeSpan.FromSeconds(withMs).ToString();

          // count distinct owner pod ids if available
          var podIds = TasksRaw.Select(r =>
                                          {
                                            try
                                            {
                                              // TaskDetailed likely exposes OwnerPodId
                                              var prop = r.GetType().GetProperty("OwnerPodId");
                                              if (prop != null)
                                              {
                                                return prop.GetValue(r)?.ToString();
                                              }
                                              // fallback to Owner if different naming
                                              var alt = r.GetType().GetProperty("Owner");
                                              if (alt != null)
                                              {
                                                return alt.GetValue(r)?.ToString();
                                              }
                                            }
                                            catch
                                            {
                                              // ignore
                                            }

                                            return null;
                                          })
                                 .Where(id => !string.IsNullOrEmpty(id))
                                 .Distinct()
                                 .Count();

          Kpi[KpiKeys.NB_POD_USED] = podIds.ToString();
        }
        catch
        {
          // best effort only, don't break KPI calculation
        }
    }

    /// <summary>
    /// Get the time taken to retrieve results for a specific session.
    /// </summary>
    /// <param name="channel">gRPC channel to use</param>
    /// <param name="sessionId">Session identifier</param>
    /// <param name="dateTimeFinished">Time when the results were retrieved</param>
    public async Task GetTimeToRetrieveResults(ChannelBase channel,
                                               string sessionId,
                                               DateTime dateTimeFinished)
    {
      if (TasksRaw.Count == 0)
      {
        await GetAllStatsAsync(channel,
                               sessionId);
      }

      var timeDiff = Timestamp.FromDateTime(dateTimeFinished.ToUniversalTime()) - TasksRaw.Select(raw => raw.StartedAt)
                                                                                          .Min();
      var withMs = timeDiff.Seconds + timeDiff.Nanos / 1e9;

      Kpi[KpiKeys.TIME_RETRIEVE_RESULTS] = TimeSpan.FromSeconds(withMs)
                                                   .ToString();

      Kpi[KpiKeys.TIME_THROUGHPUT_RESULTS] = (TasksRaw.Count() / withMs).ToString("F02");
      Kpi[KpiKeys.DOWNLOAD_SPEED_KB] = (TasksRaw.Count() * (int.Parse(Kpi[KpiKeys.NB_OUTPUTBYTES]) / 1024.0) / withMs).ToString("F02");
    }

    /// <summary>
    /// Public wrapper that retrieves all stats for a session and computes KPIs.
    /// Kept for backward compatibility with older callers.
    /// </summary>
    public async Task GetAllStats(ChannelBase channel,
                                  string sessionId,
                                  DateTime startTime,
                                  DateTime finishedTime)
    {
      // retrieve raw tasks
      await GetAllStatsAsync(channel, sessionId).ConfigureAwait(false);

      // compute derived KPIs
      await GetTimeToSubmitTasks(channel, sessionId, startTime).ConfigureAwait(false);
      await GetTimeToProcessTasks(channel, sessionId).ConfigureAwait(false);
      await GetTimeToRetrieveResults(channel, sessionId, finishedTime).ConfigureAwait(false);
    }

    /// <summary>
    /// Create a JSON report file with all KPIs and per-task summaries.
    /// </summary>
    /// <param name="jsonPath">Path to the output JSON file</param>
    public async Task PrintToJson(string jsonPath)
    {
      // Build a rich report object containing parameters, KPIs and per-task summaries
      var report = new Dictionary<string, object>();


      report["kpis"] = Kpi.ToDictionary(k => k.Key.ToString(), k => (object)k.Value);

      // Add configuration/context information if present in Kpi
      var context = new Dictionary<string, object>();
      context["nbTasks"] = Kpi.ContainsKey(KpiKeys.NB_TASKS) ? Kpi[KpiKeys.NB_TASKS] : null;
      context["nbInputBytes"] = Kpi.ContainsKey(KpiKeys.NB_INPUTBYTES) ? Kpi[KpiKeys.NB_INPUTBYTES] : null;
      context["nbOutputBytes"] = Kpi.ContainsKey(KpiKeys.NB_OUTPUTBYTES) ? Kpi[KpiKeys.NB_OUTPUTBYTES] : null;
      context["workloadTimeInMs"] = Kpi.ContainsKey(KpiKeys.TIME_WORKLOAD_IN_MS) ? Kpi[KpiKeys.TIME_WORKLOAD_IN_MS] : null;
      context["tasksPerBuffer"] = Kpi.ContainsKey(KpiKeys.TASKS_PER_BUFFER) ? Kpi[KpiKeys.TASKS_PER_BUFFER] : null;
      context["nbChannel"] = Kpi.ContainsKey(KpiKeys.NB_CHANNEL) ? Kpi[KpiKeys.NB_CHANNEL] : null;
      context["nbConcurrentBufferPerChannel"] = Kpi.ContainsKey(KpiKeys.NB_CONCURRENT_BUFFER_PER_CHANNEL) ? Kpi[KpiKeys.NB_CONCURRENT_BUFFER_PER_CHANNEL] : null;

      report["context"] = context;

      if (SubmissionDelayMs.HasValue)
      {
        context["submissionDelayMs"] = SubmissionDelayMs.Value;
      }
      if (PayloadVariationPercent.HasValue)
      {
        context["payloadVariationPercent"] = PayloadVariationPercent.Value;
      }
      if (OutputVariationPercent.HasValue)
      {
        context["outputVariationPercent"] = OutputVariationPercent.Value;
      }
      if (!string.IsNullOrEmpty(VariationDistribution))
      {
        context["variationDistribution"] = VariationDistribution;
      }
      if (!string.IsNullOrEmpty(Endpoint))
      {
        context["grpcEndpoint"] = Endpoint;
      }

      // Per-task summary: map Task.Id -> useful fields
      var tasksList = new List<Dictionary<string, object>>();
      foreach (var t in TasksRaw)
      {
        try
        {
          var taskSummary = new Dictionary<string, object>();

          // use reflection to extract common fields safely
          var tt = t.GetType();

          // try TaskId / Id
          var idProp = tt.GetProperty("Id") ?? tt.GetProperty("TaskId");
          if (idProp != null)
          {
            taskSummary["taskId"] = idProp.GetValue(t)?.ToString();
          }

          var ownerProp = tt.GetProperty("OwnerPodId");
          if (ownerProp != null)
          {
            taskSummary["ownerPodId"] = ownerProp.GetValue(t)?.ToString();
          }

          var stateProp = tt.GetProperty("State") ?? tt.GetProperty("TaskState");
          if (stateProp != null)
          {
            taskSummary["state"] = stateProp.GetValue(t)?.ToString();
          }

          // timestamps (CreatedAt / StartedAt / EndedAt) are Google.Protobuf.WellKnownTypes.Timestamp
          var createdProp = tt.GetProperty("CreatedAt");
          if (createdProp != null)
          {
            var createdVal = createdProp.GetValue(t) as Google.Protobuf.WellKnownTypes.Timestamp;
            taskSummary["createdAt"] = createdVal?.ToDateTime().ToString("o");
          }

          var startedProp = tt.GetProperty("StartedAt");
          if (startedProp != null)
          {
            var startedVal = startedProp.GetValue(t) as Google.Protobuf.WellKnownTypes.Timestamp;
            taskSummary["startedAt"] = startedVal?.ToDateTime().ToString("o");
          }

          var endedProp = tt.GetProperty("EndedAt");
          if (endedProp != null)
          {
            var endedVal = endedProp.GetValue(t) as Google.Protobuf.WellKnownTypes.Timestamp;
            taskSummary["endedAt"] = endedVal?.ToDateTime().ToString("o");
          }

          // result size: try Result (byte[]), Results (repeated), or nothing
          var resultProp = tt.GetProperty("Result") ?? tt.GetProperty("Results");
          if (resultProp != null)
          {
            var resVal = resultProp.GetValue(t);
            if (resVal is byte[] bytes)
            {
              taskSummary["resultSizeBytes"] = bytes.Length;
            }
            else if (resVal is Google.Protobuf.ByteString bs)
            {
              taskSummary["resultSizeBytes"] = bs.Length;
            }
            else if (resVal is System.Collections.IEnumerable ie)
            {
              // try count of results
              int count = 0;
              foreach (var _ in ie)
                count++;
              taskSummary["resultCount"] = count;
            }
          }

          var errorProp = tt.GetProperty("Error");
          if (errorProp != null)
          {
            var errVal = errorProp.GetValue(t);
            var msgProp = errVal?.GetType().GetProperty("Message");
            if (msgProp != null)
            {
              taskSummary["errorMessage"] = msgProp.GetValue(errVal)?.ToString();
            }
            else
            {
              taskSummary["errorMessage"] = errVal?.ToString();
            }
          }

          tasksList.Add(taskSummary);
        }
        catch
        {
          // ignore per-task serialization problems, keep report generation resilient
        }
      }

      report["tasks"] = tasksList;

      // Additional auto fields
      report["generatedAt"] = DateTime.UtcNow.ToString("o");

      var options = new JsonSerializerOptions
      {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
      };

      // Ensure parent directory exists
      try
      {
        var parent = Path.GetDirectoryName(jsonPath);
        if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
        {
          Directory.CreateDirectory(parent);
        }
      }
      catch
      {
        // ignore
      }

      if (File.Exists(jsonPath))
      {
        File.Delete(jsonPath);
      }

      await using var file = File.OpenWrite(jsonPath);

      await file.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(report,
                                                                                                     options))))
                .ConfigureAwait(false);
    }


    /// <summary>
    /// Create a human-readable text report with all KPIs.
    /// </summary>
    /// <returns>String containing the text report</returns>
    public Task<string> PrintToText()
    {
      var sb = new StringBuilder();
      sb.Append("========      Statistics and performance      ========" + Environment.NewLine);
      sb.Append(Environment.NewLine);
      sb.Append("-------- Submission buffer configuration --------------" + Environment.NewLine);
      // helper to safely read KPI values (some keys may be absent)
      string kpiVal(KpiKeys key)
      {
        return Kpi.TryGetValue(key, out var v) ? v : "N/A";
      }

      sb.Append($"Max nb tasks per buffer          : {kpiVal(KpiKeys.TASKS_PER_BUFFER)}" + Environment.NewLine);
      sb.Append($"Nb Grpc channel                  : {kpiVal(KpiKeys.NB_CHANNEL)}" + Environment.NewLine);
      sb.Append($"Nb concurrent buffer per channel : {kpiVal(KpiKeys.NB_CONCURRENT_BUFFER_PER_CHANNEL)}" + Environment.NewLine);


      sb.Append(Environment.NewLine);
      sb.Append("-------- Context of stressTests          ---------------" + Environment.NewLine);
      if (SubmissionDelayMs.HasValue || PayloadVariationPercent.HasValue || OutputVariationPercent.HasValue || !string.IsNullOrEmpty(VariationDistribution) || !string.IsNullOrEmpty(Endpoint))
      {
        if (SubmissionDelayMs.HasValue) sb.Append($"Submission delay (ms)            : {SubmissionDelayMs.Value}" + Environment.NewLine);
        if (PayloadVariationPercent.HasValue) sb.Append($"Payload variation (%)           : {PayloadVariationPercent.Value}" + Environment.NewLine);
        if (OutputVariationPercent.HasValue) sb.Append($"Output variation (%)            : {OutputVariationPercent.Value}" + Environment.NewLine);
        if (!string.IsNullOrEmpty(VariationDistribution)) sb.Append($"Variation distribution          : {VariationDistribution}" + Environment.NewLine);
        if (!string.IsNullOrEmpty(Endpoint)) sb.Append($"gRPC endpoint                   : {Endpoint}" + Environment.NewLine);
        sb.Append(Environment.NewLine);
      }
  sb.Append($"Nb Task received and completed   : {kpiVal(KpiKeys.COMPLETED_TASKS)}" + Environment.NewLine);
  sb.Append($"Input bytes by payload in kB     : {kpiVal(KpiKeys.NB_INPUTBYTES)}" + Environment.NewLine);
  sb.Append($"Output bytes by result in kB     : {kpiVal(KpiKeys.NB_OUTPUTBYTES)}" + Environment.NewLine);
  sb.Append($"Workload time per task (ms)      : {kpiVal(KpiKeys.TIME_WORKLOAD_IN_MS)}" + Environment.NewLine);
      sb.Append(Environment.NewLine);

      sb.Append("-------- Statistics of execution         --------------" + Environment.NewLine);
  sb.Append($"Time to Submit all Tasks           : {kpiVal(KpiKeys.TIME_SUBMITTED_TASKS)}" + Environment.NewLine);
  sb.Append($"Submission throughPut (tasks/s)    : {kpiVal(KpiKeys.TIME_THROUGHPUT_SUBMISSION)}" + Environment.NewLine);
  sb.Append($"Upload speed (KB/s)                : {kpiVal(KpiKeys.UPLOAD_SPEED_KB)}" + Environment.NewLine);
      sb.Append(Environment.NewLine);
  sb.Append($"Time to process all Tasks          : {kpiVal(KpiKeys.TIME_PROCESSED_TASKS)}" + Environment.NewLine);
  sb.Append($"Processing throughPut (tasks/s)    : {kpiVal(KpiKeys.TIME_THROUGHPUT_PROCESS)}" + Environment.NewLine);
      sb.Append(Environment.NewLine);
  sb.Append($"Time to retrieve all results       : {kpiVal(KpiKeys.TIME_RETRIEVE_RESULTS)}" + Environment.NewLine);
  sb.Append($"Speed retrieving result (result/s) : {kpiVal(KpiKeys.TIME_THROUGHPUT_RESULTS)}" + Environment.NewLine);
  sb.Append($"Download speed (KB/s)              : {kpiVal(KpiKeys.DOWNLOAD_SPEED_KB)}" + Environment.NewLine);

      sb.Append(Environment.NewLine);
      sb.Append("-------- Total user time end to end      --------------" + Environment.NewLine);

  sb.Append($"Number of pod used               : {kpiVal(KpiKeys.NB_POD_USED)}" + Environment.NewLine);
  sb.Append($"Total time                       : {kpiVal(KpiKeys.TOTAL_TIME)}" + Environment.NewLine);

      return Task.FromResult(sb.ToString());
    }
  }
}
