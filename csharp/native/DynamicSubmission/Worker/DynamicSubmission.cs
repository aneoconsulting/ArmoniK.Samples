using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ArmoniK.Api.Common.Channel.Utils;
using ArmoniK.Api.Common.Options;
using ArmoniK.Api.Common.Utils;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Agent;
using ArmoniK.Api.Worker.Worker;
using ArmoniK.Samples.DynamicSubmission.Common;
using ArmoniK.Utils;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

using Empty = ArmoniK.Api.gRPC.V1.Empty;

namespace ArmoniK.Samples.DynamicSubmission.Worker
{
  public class DynamicSubmissionWorker : WorkerStreamWrapper
  {
    public DynamicSubmissionWorker(ILoggerFactory      loggerFactory,
                                   ComputePlane        computePlane,
                                   GrpcChannelProvider provider)
      : base(loggerFactory,
             computePlane,
             provider)
      => logger_ = loggerFactory.CreateLogger<DynamicSubmissionWorker>();

    /// <summary>
    ///   Function that represents the processing of a task.
    /// </summary>
    /// <param name="taskHandler">Handler that holds the payload, the task metadata and helpers to submit tasks and results</param>
    /// <returns>
    ///   An <see cref="Output" /> representing the status of the current task. This is the final step of the task.
    /// </returns>
    public override async Task<Output> Process(ITaskHandler taskHandler)
    {
      using var scopedLog = logger_.BeginNamedScope("Execute task",
                                                    ("sessionId", taskHandler.SessionId),
                                                    ("taskId", taskHandler.TaskId));

      try
      {
        // We convert the binary payload from the handler back to the string sent by the client
        var tableInput = Table.Deserialize(taskHandler.Payload);
        // We get the result that the task should produce
        // The handler has this information
        // It also contains other information such as the data dependencies (id and binary data) if any
        var resultId = taskHandler.ExpectedResults.Single();

        if (tableInput == null)
        {
          throw new ArgumentNullException(nameof(tableInput),
                                          "The threshold is inferior to 1 but should be superior");
        }

        var size = tableInput.Size;

        if (size > tableInput.Threshold && taskHandler.DataDependencies.Count == 0 && tableInput!.Threshold > 1)
        {
          logger_.LogDebug("Started Calculate nbrOfTasks");
          // Calculate the nbr of tasks to create
          var nbrOfTasks = size / tableInput.Threshold + (size % tableInput.Threshold != 0
                                                            ? 1U
                                                            : 0U);
          logger_.LogInformation("nbrOfTasks = {nbrOfTasks}",
                                 nbrOfTasks);

          // Create all sub-table needed for subtask arguments 
          IList<Table> inputs = new List<Table>();
          logger_.LogDebug("Table List create start splitting");

          for (uint i = 0; i / tableInput.Threshold < nbrOfTasks; i += tableInput.Threshold)
          {
            var len = tableInput.Threshold;
            if (i / tableInput.Threshold == nbrOfTasks - 1 && tableInput.Size % tableInput.Threshold != 0)
            {
              len = tableInput.Size % tableInput.Threshold;
            }

            inputs.Add(new Table(tableInput.Values[i],
                                 len,
                                 tableInput.Threshold));
          }
          // Subtask creation begin 

          logger_.LogDebug("Submitting Workers");

          var taskOptions = new TaskOptions
                            {
                              MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                              MaxRetries  = 2,
                              Priority    = 1,
                              PartitionId = taskHandler.TaskOptions.PartitionId,
                            };

          var subTaskResultIds = (await taskHandler.CreateResultsMetaDataAsync(Enumerable.Range(1,
                                                                                                (int)nbrOfTasks)
                                                                                         .Select(i => new CreateResultsMetaDataRequest.Types.ResultCreate
                                                                                                      {
                                                                                                        Name = "Result_" + i,
                                                                                                      }))).Results.Select(data => data.ResultId)
                                                                                                          .AsIList();
          logger_.LogDebug("Created ResultsMetaData for Sub-workers");

          var payloadIds = (await taskHandler.CreateResultsAsync(inputs.Select((table,
                                                                                i) => new CreateResultsRequest.Types.ResultCreate
                                                                                      {
                                                                                        Data = UnsafeByteOperations.UnsafeWrap(table.Serialize()),
                                                                                        Name = "Payload_" + (i + 1),
                                                                                      }))).Results.Select(data => data.ResultId);
          logger_.LogDebug("Created Results Async for Sub-workers Submit Tasks Async ");

          await taskHandler.SubmitTasksAsync(new List<SubmitTasksRequest.Types.TaskCreation>(subTaskResultIds.Zip(payloadIds)
                                                                                                             .Select(tuple => new SubmitTasksRequest.Types.TaskCreation
                                                                                                                              {
                                                                                                                                PayloadId = tuple.Second,
                                                                                                                                ExpectedOutputKeys =
                                                                                                                                {
                                                                                                                                  tuple.First,
                                                                                                                                },
                                                                                                                              })),
                                             taskOptions);
          logger_.LogDebug("Sub-workers submitted");

          logger_.LogDebug("Submitting Aggregation start");

          var payload = await taskHandler.CreateResultsAsync(new List<CreateResultsRequest.Types.ResultCreate>
                                                             {
                                                               new()
                                                               {
                                                                 Data = UnsafeByteOperations.UnsafeWrap(tableInput.Serialize()),
                                                                 Name = "Payload",
                                                               },
                                                             });
          logger_.LogDebug("Created Results Async for Sub-workers Submit Tasks Async ");

          var payloadId = payload.Results.Single()
                                 .ResultId;

          await taskHandler.SubmitTasksAsync(new List<SubmitTasksRequest.Types.TaskCreation>
                                             {
                                               new()
                                               {
                                                 PayloadId = payloadId,
                                                 ExpectedOutputKeys =
                                                 {
                                                   resultId,
                                                 },
                                                 DataDependencies =
                                                 {
                                                   subTaskResultIds,
                                                 },
                                               },
                                             },
                                             taskOptions);
          logger_.LogDebug("Aggregation submitted");
        }
        else
        {
          logger_.LogDebug("Debug : Enter in subtask process");

          uint result = 0;

          if (taskHandler.DataDependencies.Any())
          {
            result = taskHandler.DataDependencies.Values.Select(res => BitConverter.ToUInt32(res,
                                                                                             0))
                                .Aggregate((u,
                                            u1) => u + u1);
          }
          // if we have dependencies this mean that it is the aggregation task
          // We add all result of the dependencies together to get the final result
          else
          {
            result = tableInput.Values.Aggregate((u,
                                                  u1) => u + u1);
          }

          // We get the result of the task using through the handler
          await taskHandler.SendResult(resultId,
                                       BitConverter.GetBytes(result))
                           .ConfigureAwait(false);
        }
      }
      // If there is an exception, we put the task in error
      // The task will not be retried by ArmoniK
      // An uncatched exception means that the task will be retried
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
  }
}
