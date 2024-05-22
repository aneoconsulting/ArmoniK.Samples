using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArmoniK.Api.Common.Channel.Utils;
using ArmoniK.Api.Common.Options;
using ArmoniK.Api.Common.Utils;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Agent;
using ArmoniK.Api.Worker.Worker;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

using Empty = ArmoniK.Api.gRPC.V1.Empty;

namespace ArmoniK.Samples.LinearSubTasking.Worker
{
  public class SubTaskingWorker : WorkerStreamWrapper
  {
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
        var input = BitConverter.ToInt32(taskHandler.Payload,
                                         0);

        // We get the result that the task should produce
        // The handler has this information
        // It also contains other information such as the data dependencies (id and binary data) if any
        var resultId = taskHandler.ExpectedResults.Single();

        // We send a task to the worker while the result is different of 0 or 1
        if (input != 0 && input != 1)
        {
          input = input > 1
                    ? input - 2
                    : input + 2;
        }

        //The new input is different of 0 or 1
        //We need to do another subtraction
        //We submit a new task with the new value of input
        if (input != 0 && input != 1)
        {
          // Default task options that will be used by each task if not overwritten when submitting tasks
          var taskOptions = new TaskOptions
                            {
                              MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
                              MaxRetries  = 2,
                              Priority    = 1,
                              PartitionId = taskHandler.TaskOptions.PartitionId,
                            };
          logger_.LogInformation("Entered in Submit Worker input :{input}",
                                 input);

          // Create the payload metadata (a result) and upload data at the same time
          var payload = await taskHandler.CreateResultsAsync(new List<CreateResultsRequest.Types.ResultCreate>
                                                             {
                                                               new()
                                                               {
                                                                 Data = UnsafeByteOperations.UnsafeWrap(BitConverter.GetBytes(input)),
                                                                 Name = "Payload",
                                                               },
                                                             });

          var payloadId = payload.Results.Single()
                                 .ResultId;

          // Submit task with payload and result id
          var subtask = await taskHandler.SubmitTasksAsync(new List<SubmitTasksRequest.Types.TaskCreation>
                                                           {
                                                             new()
                                                             {
                                                               PayloadId = payloadId,
                                                               ExpectedOutputKeys =
                                                               {
                                                                 resultId,
                                                               },
                                                             },
                                                           },
                                                           taskOptions);
        }
        else
        {
          // We get the result of the task using through the handler
          // We send the final result to the client
          await taskHandler.SendResult(resultId,
                                       Encoding.ASCII.GetBytes($"{input}"))
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
