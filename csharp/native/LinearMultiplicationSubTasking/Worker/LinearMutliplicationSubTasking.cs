using System;
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

using Microsoft.Extensions.Logging;

using Empty = ArmoniK.Api.gRPC.V1.Empty;



namespace ArmoniK.Samples.LinearMultiplicationSubTasking.Worker
{
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
            try
            {
                // Retrive the parameters from the payload
                var payloadBytes = taskHandler.Payload;
                int x = BitConverter.ToInt32(payloadBytes, 0);
                int y = BitConverter.ToInt32(payloadBytes, 4);
                int z = BitConverter.ToInt32(payloadBytes, 8);
                int sign = BitConverter.ToInt32(payloadBytes, 12);
                logger_.LogInformation("Parameters received: X = {X}, Y = {Y}, Z = {Z}", x, y, z);

                x = Math.Abs(x);
                y = Math.Abs(y);

                if (y > 0)
                {
                    logger_.LogInformation($"Creating subtask with x = {x}, y = {y - 1}, z = {z + x}");
                    // Create the subtask payload with the new parameters 
                    var subTaskPayload = new int[] { x, y - 1, z + x, sign };
                    var subTaskPayloadBytes = subTaskPayload.SelectMany(BitConverter.GetBytes).ToArray();

                    // Create the subtaskResultId
                    var subTaskResultId = (await taskHandler.CreateResultsAsync(new[]
                   {
                        new CreateResultsRequest.Types.ResultCreate
                        {
                            Data = UnsafeByteOperations.UnsafeWrap(subTaskPayloadBytes)
                        }
                    })).Results.Single().ResultId;

                    // Submit the subtask
                    await taskHandler.SubmitTasksAsync(new[]
                    {
                        new SubmitTasksRequest.Types.TaskCreation
                        {
                            PayloadId = subTaskResultId,
                            ExpectedOutputKeys = { taskHandler.ExpectedResults.Single() }
                        }
                    }, taskHandler.TaskOptions);
                    logger_.LogInformation("Task Payload: {Payload}", Encoding.ASCII.GetString(taskHandler.Payload));
                    logger_.LogInformation("Parameters received: X = {X}, Y = {Y}, Z = {Z}", x, y, z);
                }
                else
                {
                    var resultId = taskHandler.ExpectedResults.Single();

                    // Multiply the result by the sign
                    int finalResult = z * sign;
                    logger_.LogInformation("Final Result reached , Values: X = {originalX}, Y = {originalY}, Z = {Z}", x, y, z);
                    await taskHandler.SendResult(resultId, BitConverter.GetBytes(finalResult)).ConfigureAwait(false);
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
            return new Output
            {
                Ok = new Empty(),
            };
        }
    }
}