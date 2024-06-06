// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-$CURRENT_YEAR$. All rights reserved.
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

    public async Task GetTimeToSubmitTasks(ChannelBase channel,
                                           string      sessionId,
                                           DateTime    start)
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

      Kpi[KpiKeys.TIME_THROUGHPUT_SUBMISSION] = (TasksRaw.Count()                                                    / timeSpentList.Max()).ToString("F02");
      Kpi[KpiKeys.UPLOAD_SPEED_KB]            = (TasksRaw.Count() * (int.Parse(Kpi[KpiKeys.NB_INPUTBYTES]) / 1024.0) / timeSpentList.Max()).ToString("F02");
    }


    private async Task GetTimeToProcessTasks(ChannelBase channel,
                                             string      sessionId)
    {
      if (TasksRaw.Count == 0)
      {
        await GetAllStatsAsync(channel,
                               sessionId)
          .ConfigureAwait(false);
      }


      var timeDiff = TasksRaw.Select(raw => raw.EndedAt)
                             .Max() - TasksRaw.Select(raw => raw.CreatedAt)
                                              .Min();
      var withMs = timeDiff.Seconds + timeDiff.Nanos / 1e9;
      Kpi[KpiKeys.TIME_PROCESSED_TASKS] = TimeSpan.FromSeconds(withMs)
                                                  .ToString();
      Kpi[KpiKeys.TIME_THROUGHPUT_PROCESS] = (TasksRaw.Count() / withMs).ToString("F02");
    }

    public async Task GetTimeToRetrieveResults(ChannelBase channel,
                                               string      sessionId,
                                               DateTime    dateTimeFinished)
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

      Kpi[KpiKeys.TIME_THROUGHPUT_RESULTS] = (TasksRaw.Count()                                                     / withMs).ToString("F02");
      Kpi[KpiKeys.DOWNLOAD_SPEED_KB]       = (TasksRaw.Count() * (int.Parse(Kpi[KpiKeys.NB_OUTPUTBYTES]) / 1024.0) / withMs).ToString("F02");
    }

    public async Task<Dictionary<KpiKeys, string>> GetAllStats(ChannelBase channel,
                                                               string      sessionId,
                                                               DateTime    start,
                                                               DateTime?   end = null)
    {
      var dateTimeFinished = end ?? DateTime.Now;

      await GetTimeToSubmitTasks(channel,
                                 sessionId,
                                 start)
        .ConfigureAwait(false);

      await GetTimeToProcessTasks(channel,
                                  sessionId)
        .ConfigureAwait(false);

      await GetTimeToRetrieveResults(channel,
                                     sessionId,
                                     dateTimeFinished)
        .ConfigureAwait(false);

      Kpi[KpiKeys.TOTAL_TIME] = (end - start).ToString() ?? string.Empty;

      Kpi[KpiKeys.NB_POD_USED] = TasksRaw.DistinctBy(t => t.OwnerPodId)
                                         .Count()
                                         .ToString();

      return Kpi;
    }

    public async Task PrintToJson(string jsonPath)
    {
      var dictJson = new Dictionary<string, string>(Kpi.Select(pair => new KeyValuePair<string, string>(pair.Key.ToString(),
                                                                                                        pair.Value)));

      var options = new JsonSerializerOptions
                    {
                      WriteIndented = true,
                    };
      if (File.Exists(jsonPath))
      {
        File.Delete(jsonPath);
      }

      await using var file = File.OpenWrite(jsonPath);

      await file.WriteAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dictJson,
                                                                                                     options))))
                .ConfigureAwait(false);
    }


    public async Task<string> PrintToText()
    {
      var sb = new StringBuilder();
      sb.Append("========      Statistics and performance      ========" + Environment.NewLine);
      sb.Append(Environment.NewLine);
      sb.Append("-------- Submission buffer configuration --------------"                             + Environment.NewLine);
      sb.Append($"Max nb tasks per buffer          : {Kpi[KpiKeys.TASKS_PER_BUFFER]}"                 + Environment.NewLine);
      sb.Append($"Nb Grpc channel                  : {Kpi[KpiKeys.NB_CHANNEL]}"                       + Environment.NewLine);
      sb.Append($"Nb concurrent buffer per channel : {Kpi[KpiKeys.NB_CONCURRENT_BUFFER_PER_CHANNEL]}" + Environment.NewLine);


      sb.Append(Environment.NewLine);
      sb.Append("-------- Context of stressTests          --------------"                + Environment.NewLine);
      sb.Append($"Nb Task received and completed   : {Kpi[KpiKeys.COMPLETED_TASKS]}"     + Environment.NewLine);
      sb.Append($"Input bytes by payload in kB     : {Kpi[KpiKeys.NB_INPUTBYTES]}"       + Environment.NewLine);
      sb.Append($"Output bytes by result in kB     : {Kpi[KpiKeys.NB_OUTPUTBYTES]}"      + Environment.NewLine);
      sb.Append($"Workload time per task (ms)      : {Kpi[KpiKeys.TIME_WORKLOAD_IN_MS]}" + Environment.NewLine);
      sb.Append(Environment.NewLine);

      sb.Append("-------- Statistics of execution         --------------"                         + Environment.NewLine);
      sb.Append($"Time to Submit all Tasks           : {Kpi[KpiKeys.TIME_SUBMITTED_TASKS]}"       + Environment.NewLine);
      sb.Append($"Submission throughPut (tasks/s)    : {Kpi[KpiKeys.TIME_THROUGHPUT_SUBMISSION]}" + Environment.NewLine);
      sb.Append($"Upload speed (KB/s)                : {Kpi[KpiKeys.UPLOAD_SPEED_KB]}"            + Environment.NewLine);
      sb.Append(Environment.NewLine);
      sb.Append($"Time to process all Tasks          : {Kpi[KpiKeys.TIME_PROCESSED_TASKS]}"    + Environment.NewLine);
      sb.Append($"Processing throughPut (tasks/s)    : {Kpi[KpiKeys.TIME_THROUGHPUT_PROCESS]}" + Environment.NewLine);
      sb.Append(Environment.NewLine);
      sb.Append($"Time to retrieve all results       : {Kpi[KpiKeys.TIME_RETRIEVE_RESULTS]}"   + Environment.NewLine);
      sb.Append($"Speed retrieving result (result/s) : {Kpi[KpiKeys.TIME_THROUGHPUT_RESULTS]}" + Environment.NewLine);
      sb.Append($"Download speed (KB/s)              : {Kpi[KpiKeys.DOWNLOAD_SPEED_KB]}"       + Environment.NewLine);

      sb.Append(Environment.NewLine);
      sb.Append("-------- Total user time end to end      --------------" + Environment.NewLine);

      sb.Append($"Number of pod used               : {Kpi[KpiKeys.NB_POD_USED]}" + Environment.NewLine);
      sb.Append($"Total time                       : {Kpi[KpiKeys.TOTAL_TIME]}"  + Environment.NewLine);

      return sb.ToString();
    }
  }
}
