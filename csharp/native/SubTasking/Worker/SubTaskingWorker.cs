// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-2024. All rights reserved.
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Api.Common.Channel.Utils;
using ArmoniK.Api.Common.Options;
using ArmoniK.Api.Common.Utils;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Agent;
using ArmoniK.Api.Worker.Worker;

using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

using Empty = ArmoniK.Api.gRPC.V1.Empty;

namespace ArmoniK.Samples.SubTasking.Worker
{
  public class SubTaskingWorker : WorkerStreamWrapper
  {
    /// <summary>
    ///   Initializes an instance of <see cref="SubTaskingWorker" />
    /// </summary>
    /// <param name="loggerFactory">Factory to create loggers</param>
    /// <param name="computePlane">Compute Plane</param>
    /// <param name="provider">gRPC channel provider to send tasks and results to ArmoniK Scheduler</param>
    public SubTaskingWorker(ILoggerFactory      loggerFactory,
                            ComputePlane        computePlane,
                            GrpcChannelProvider provider)
      : base(loggerFactory,
             computePlane,
             provider)
      => logger_ = loggerFactory.CreateLogger<SubTaskingWorker>();

    /// <summary>
    ///   Function that represents the processing of a task.
    /// </summary>
    /// <param name="taskHandler">Handler that holds the payload, the task metadata and helpers to submit tasks and results</param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///   An <see cref="Output" /> representing the status of the current task. This is the final step of the task.
    /// </returns>
    public override async Task<Output> ProcessAsync(ITaskHandler      taskHandler,
                                                    CancellationToken cancellationToken)
    {
      using var scopedLog = logger_.BeginNamedScope("Execute task",
                                                    ("sessionId", taskHandler.SessionId),
                                                    ("taskId", taskHandler.TaskId));
      try
      {
        // We may use TaskOptions.Options to send a field UseCase where we inform
        // what should be executed
        var useCase = taskHandler.TaskOptions.Options["UseCase"];
        logger_.LogDebug("Start task");

        switch (useCase)
        {
          case "Launch":
            var resultIds = await SubmitWorkers(taskHandler);
            await SubmitJoiner(taskHandler,
                               resultIds);
            break;
          case "Joiner":
            await Joiner(taskHandler);
            break;
          case "HelloWorker":
            await HelloWorker(taskHandler);
            break;
          default:
            return new Output
                   {
                     Error = new Output.Types.Error
                             {
                               Details = "UseCase not found",
                             },
                   };
        }
      }
      catch (Exception e)
      {
        logger_.LogError(e,
                         "Error during task computing.");
        return new Output
               {
                 Error = new Output.Types.Error
                         {
                           Details = e.Message,
                         },
               };
      }

      return new Output
             {
               Ok = new Empty(),
             };
    }

    private async Task<List<string>> SubmitWorkers(ITaskHandler taskHandler)
    {
      logger_.LogDebug("Submitting Workers");

      var input = Encoding.ASCII.GetString(taskHandler.Payload);

      var taskOptions = new TaskOptions
                        {
                          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                          MaxRetries  = 2,
                          Priority    = 1,
                          PartitionId = taskHandler.TaskOptions.PartitionId,
                          Options =
                          {
                            new MapField<string, string>
                            {
                              {
                                "UseCase", "HelloWorker"
                              },
                            },
                          },
                        };

      var subTaskResults = await taskHandler.CreateResultsMetaDataAsync(Enumerable.Range(1,
                                                                                         5)
                                                                                  .Select(i => new CreateResultsMetaDataRequest.Types.ResultCreate
                                                                                               {
                                                                                                 Name = Guid.NewGuid() + "_" + i,
                                                                                               })
                                                                                  .ToList());

      var subTasksResultIds = subTaskResults.Results.Select(result => result.ResultId)
                                            .ToList();

      var payloads = await taskHandler.CreateResultsAsync(Enumerable.Range(1,
                                                                          5)
                                                                    .Select(i => new CreateResultsRequest.Types.ResultCreate
                                                                    {
                                                                        Data = UnsafeByteOperations.UnsafeWrap(
                                                                            Encoding.ASCII.GetBytes($"{input}_FatherId_{taskHandler.TaskId}")
                                                                        ),
                                                                        Name = $"Payload_{i}",
                                                                    }));

      var payloadIds = payloads.Results.Select(result => result.ResultId).ToList();

      await taskHandler.SubmitTasksAsync(new List<SubmitTasksRequest.Types.TaskCreation>(payloadIds.Zip(subTasksResultIds, (payloadId, subTaskId) => new SubmitTasksRequest.Types.TaskCreation
                                                                                              {
                                                                                                  PayloadId = payloadId,
                                                                                                  ExpectedOutputKeys = 
                                                                                                  {
                                                                                                    subTaskId
                                                                                                  },
                                                                                              })
                                                                                          .ToList()),
                                                                                      taskOptions);

      return subTasksResultIds;
    }

    private async Task SubmitJoiner(ITaskHandler        taskHandler,
                                    IEnumerable<string> expectedOutputIds)
    {
      logger_.LogDebug("Submitting Joiner");
      var taskOptions = new TaskOptions
                        {
                          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                          MaxRetries  = 2,
                          Priority    = 1,
                          PartitionId = taskHandler.TaskOptions.PartitionId,
                          Options =
                          {
                            new MapField<string, string>
                            {
                              {
                                "UseCase", "Joiner"
                              },
                            },
                          },
                        };

      var subTaskResultId = taskHandler.ExpectedResults.Single();

      var payload = await taskHandler.CreateResultsAsync(new List<CreateResultsRequest.Types.ResultCreate>
                                                         {
                                                           new()
                                                           {
                                                             Data = UnsafeByteOperations.UnsafeWrap("Submiting Joiner"u8.ToArray()),
                                                             Name = "Payload",
                                                           },
                                                         });

      var payloadId = payload.Results.Single()
                             .ResultId;

      await taskHandler.SubmitTasksAsync(new List<SubmitTasksRequest.Types.TaskCreation>
                                         {
                                           new()
                                           {
                                             PayloadId = payloadId,
                                             ExpectedOutputKeys =
                                             {
                                               subTaskResultId,
                                             },
                                             DataDependencies =
                                             {
                                               expectedOutputIds,
                                             },
                                           },
                                         },
                                         taskOptions);
    }

    private static async Task HelloWorker(ITaskHandler taskHandler)
    {
      var input = Encoding.ASCII.GetString(taskHandler.Payload);

      var resultId = taskHandler.ExpectedResults.Single();
      // We add the SubTaskId to the result 
      await taskHandler.SendResult(resultId,
                                   Encoding.ASCII.GetBytes($"{input}_SonId_{taskHandler.TaskId}"))
                       .ConfigureAwait(false);
    }

    private async Task Joiner(ITaskHandler taskHandler)
    {
      logger_.LogDebug("Starting Joiner useCase");
      var resultId = taskHandler.ExpectedResults.Single();

      var resultsArray = taskHandler.DataDependencies.Values.Select(dependency => Encoding.ASCII.GetString(dependency))
                                    .Select(result => $"{result}_Joined")
                                    .ToList();

      await taskHandler.SendResult(resultId,
                                   resultsArray.SelectMany(s => Encoding.ASCII.GetBytes(s + "\n"))
                                               .ToArray());
    }
  }
}
