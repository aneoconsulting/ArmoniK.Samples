using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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

namespace ArmoniK.Samples.LinearMultiplicationSubTasking.Worker
{
    public class Parameters
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class LinearMultiplicationSubTaskingWorker : WorkerStreamWrapper
    {
        public LinearMultiplicationSubTaskingWorker(ILoggerFactory loggerFactory,
                                                    ComputePlane computePlane,
                                                    GrpcChannelProvider provider)
            : base(loggerFactory, computePlane, provider)
        {
            logger_ = loggerFactory.CreateLogger<LinearMultiplicationSubTaskingWorker>();
        }

        public override async Task<Output> Process(ITaskHandler taskHandler)
        {
            using var scopedLog = logger_.BeginNamedScope("Execute task",
                                                          ("sessionId", taskHandler.SessionId),
                                                          ("taskId", taskHandler.TaskId));
            string resultId = null;
            int x = -1;
            int y = -1;
            try
            {
                // Convert the payload into json
                var jsonString = Encoding.ASCII.GetString(taskHandler.Payload);
                // Deserialize it into a parameter object
                var parameters = JsonSerializer.Deserialize<Parameters>(jsonString)!;

                x = parameters.X;
                y = parameters.Y;

                // Get the id of the expected result
                resultId = taskHandler.ExpectedResults.Single();

                // Handle negative y by converting it to positive and adjusting the result sign later
                bool isNegative = y < 0;
                y = Math.Abs(y);

                // Creating subtasks if y > 1
                if (y > 1)
                {
                    var subTasks = new List<SubmitTasksRequest.Types.TaskCreation>();

                    for (var i = 1; i <= y; i++)
                    {
                        // Créer les paramètres pour la sous-tâche
                        var subTaskParameters = new { X = x };
                        var subTaskJsonString = JsonSerializer.Serialize(subTaskParameters);
                        var subTaskJsonBytes = Encoding.ASCII.GetBytes(subTaskJsonString);
                        Console.WriteLine("bytes:", subTaskJsonBytes);

                        // Créer le payload pour la sous-tâche
                        var subTaskPayload = await taskHandler.CreateResultsAsync(new List<CreateResultsRequest.Types.ResultCreate>
                        {
                            new()
                            {
                                Data = UnsafeByteOperations.UnsafeWrap(subTaskJsonBytes),
                                Name = "Payload",
                            },
                        });
                        if (subTaskPayload == null || !subTaskPayload.Results.Any())
                        {
                            logger_.LogError("Failed to create payload for subtask {SubTaskIndex}.", i);
                            throw new InvalidOperationException($"Failed to create payload for subtask {i}.");
                        }

                        var subTaskPayloadId = subTaskPayload.Results.Single().ResultId;
                        Console.WriteLine("sub", subTaskPayloadId);
                        logger_.LogInformation("Subtask {SubTaskIndex} payload created with ID: {SubTaskPayloadId}", i, subTaskPayloadId);

                        // Ajouter la sous-tâche à la liste
                        subTasks.Add(new SubmitTasksRequest.Types.TaskCreation
                        {
                            PayloadId = subTaskPayloadId,
                            ExpectedOutputKeys = { resultId },
                        });
                    }
                    logger_.LogInformation("Submitting {SubTaskCount} subtasks.", subTasks.Count);

                    await taskHandler.SubmitTasksAsync(subTasks, taskHandler.TaskOptions);
                }
                else
                {
                    logger_.LogInformation("No subtasks needed, returning result directly.");
                    await taskHandler.SendResult(resultId, Encoding.ASCII.GetBytes(x.ToString())).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                logger_.LogError(e, "Error during task computing.");
                return new Output
                {
                    Error = new Output.Types.Error
                    {
                        Details = e.Message,
                    },
                };
            }

            // Ensure the result is sent back correctly
            await taskHandler.SendResult(resultId, Encoding.ASCII.GetBytes(x.ToString())).ConfigureAwait(false);

            return new Output
            {
                Ok = new Empty(),
            };
        }
    }
}