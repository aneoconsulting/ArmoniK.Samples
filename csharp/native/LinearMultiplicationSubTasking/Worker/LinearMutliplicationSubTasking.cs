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
                // Deserialize the payload to get the parameters ( list of integers )
                var payloadDict = JsonSerializer.Deserialize<List<int>>(Encoding.ASCII.GetString(taskHandler.Payload));
                int x = Convert.ToInt32(payloadDict![0]);
                int y = Convert.ToInt32(payloadDict![1]);
                logger_.LogDebug($"Received task with x = {x} & y = {y}");

                // Result computation
                int z = x * y;

                // Extraction resultId
                var resultId = taskHandler.ExpectedResults.Single();

                logger_.LogInformation($"Calculated result: z = {z}");

                // Send the result to the control plane
                await taskHandler.SendResult(resultId, Encoding.ASCII.GetBytes(z.ToString())).ConfigureAwait(false);
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