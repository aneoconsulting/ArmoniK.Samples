using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using ArmoniK.Api.Client;
using ArmoniK.Api.Client.Options;
using ArmoniK.Api.Client.Submitter;
using ArmoniK.Api.gRPC.V1;
using ArmoniK.Api.gRPC.V1.Events;
using ArmoniK.Api.gRPC.V1.Results;
using ArmoniK.Api.gRPC.V1.Sessions;
using ArmoniK.Api.gRPC.V1.Tasks;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace ArmoniK.Samples.LinearMultiplicationSubTasking.Client
{
  internal static class Program
  {
    internal static async Task Run(string endpoint, string partition, int x, int y)
    {
      var channel = GrpcChannelFactory.CreateChannel(new GrpcClient
      {
        Endpoint = endpoint,
      });

      // Creation of the clients
      var taskClient = new Tasks.TasksClient(channel);
      var resultClient = new Results.ResultsClient(channel);
      var sessionClient = new Sessions.SessionsClient(channel);
      var eventClient = new Events.EventsClient(channel);

      // Configuration for the task
      var taskOptions = new TaskOptions
      {
        MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
        MaxRetries = 2,
        Priority = 1,
        PartitionId = partition,
      };

      // Session creation
      var createSessionReply = sessionClient.CreateSession(new CreateSessionRequest
      {
        DefaultTaskOption = taskOptions,
        PartitionIds = { partition },
      });

      Console.WriteLine($"Session created: {createSessionReply.SessionId}");

      // Retrieve the sign of the result
      int sign = ((x < 0) ^ (y < 0)) ? -1 : 1;
      int absX = Math.Abs(x);
      int absY = Math.Abs(y);

      // Payload contains x, y as the parameters to multiply, z as the result and sign as the sign of the result
      var payload = new int[] { absX, absY, 0, sign };
      var payloadBytes = payload.SelectMany(BitConverter.GetBytes).ToArray();
      Console.WriteLine($"Sending payload: x = {x}, y = {y}, z = 0, sign = {sign}");

      // Creation of the metadata for the result
      var resultId = resultClient.CreateResultsMetaData(new CreateResultsMetaDataRequest
      {
        SessionId = createSessionReply.SessionId,
        Results =
                {
                    new CreateResultsMetaDataRequest.Types.ResultCreate
                    {
                        Name = "Result",
                    },
                },
      }).Results.Single().ResultId;
      Console.WriteLine($"Expected resultId : {resultId}");

      // Creation of the payload for the task and its ID
      var payloadId = resultClient.CreateResults(new CreateResultsRequest
      {
        SessionId = createSessionReply.SessionId,
        Results =
                {
                    new CreateResultsRequest.Types.ResultCreate
                    {
                        Data = UnsafeByteOperations.UnsafeWrap(payloadBytes),
                        Name = "Payload",
                    },
                },
      }).Results.Single().ResultId;
      Console.WriteLine($"Payload created : {payloadId}");

      // Submit the task to the worker
      var submitTasksResponse = taskClient.SubmitTasks(new SubmitTasksRequest
      {
        SessionId = createSessionReply.SessionId,
        TaskCreations =
                {
                    new SubmitTasksRequest.Types.TaskCreation
                    {
                        PayloadId = payloadId,
                        ExpectedOutputKeys = { resultId },
                    },
                },

      });
      var taskId = submitTasksResponse.TaskInfos.Single().TaskId;
      Console.WriteLine($"Submitted task : {taskId}");

      // Waiting for the result to be available
      await eventClient.WaitForResultsAsync(createSessionReply.SessionId, new List<string> { resultId }, 100, 1, CancellationToken.None);

      // Download the result when available
      var resultData = await resultClient.DownloadResultData(createSessionReply.SessionId, resultId, CancellationToken.None);

      if (resultData == null || !resultData.Any())
      {
        throw new Exception("No result available.");
      }

      var finalResult = BitConverter.ToInt32(resultData, 0);
      var expectedResult = x * y;
      Console.WriteLine($"Final result from Worker: {finalResult}, Expected result: {expectedResult}");

      if (finalResult != expectedResult)
      {
        throw new ArithmeticException($"Calculated result {finalResult} does not match expected result {expectedResult}");
      }
    }

    public static async Task<int> Main(string[] args)
    {
      var endpoint = new Option<string>("--endpoint",
                                         description: "Endpoint for the connection to ArmoniK control plane.",
                                       getDefaultValue: () => "http://localhost:5001");
      var partition = new Option<string>("--partition",
                                         description: "Partition name in which you can submit tasks.",
                                         getDefaultValue: () => "multiplicate");
      var x = new Option<int>("--x",
                              description: "First integer to multiply.",
                              getDefaultValue: () => 6);
      var y = new Option<int>("--y",
                              description: "Second integer to multiply.",
                              getDefaultValue: () => -7);

      var rootCommand = new RootCommand("Client used to multiply x and y in ArmoniK.")
            {
                endpoint, partition, x, y
            };

      rootCommand.SetHandler(Run, endpoint, partition, x, y);
      return await rootCommand.InvokeAsync(args);
    }
  }
}