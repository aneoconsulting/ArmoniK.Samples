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

      // Create clients for the different services
      var taskClient = new Tasks.TasksClient(channel);
      var resultClient = new Results.ResultsClient(channel);
      var sessionClient = new Sessions.SessionsClient(channel);
      var eventClient = new Events.EventsClient(channel);

      // Configure the task options for the session
      var taskOptions = new TaskOptions
      {
        MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
        MaxRetries = 2,
        Priority = 1,
        PartitionId = partition,
      };

      // Create the session
      var createSessionReply = sessionClient.CreateSession(new CreateSessionRequest
      {
        DefaultTaskOption = taskOptions,
        PartitionIds = { partition },
      });

      Console.WriteLine($"Session created : {createSessionReply.SessionId}");
      // Parameters to send to the worker
      var parameters = new List<int> { x, y };

      var jsonString = JsonSerializer.Serialize(parameters);
      var jsonBytes = Encoding.ASCII.GetBytes(jsonString);

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

      Console.WriteLine($"Result ID : {resultId}");

      // Creation of the metadata for the payload
      var payloadId = resultClient.CreateResults(new CreateResultsRequest
      {
        SessionId = createSessionReply.SessionId,
        Results =
                {
                    new CreateResultsRequest.Types.ResultCreate
                    {
                        Data = UnsafeByteOperations.UnsafeWrap(jsonBytes),
                        Name = "Payload",
                    },
                },
      }).Results.Single().ResultId;

      Console.WriteLine($"Payload ID : {payloadId}");

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
      Console.WriteLine($"Task Submitted : {taskId}");

      // Wait for the result from the worker
      await eventClient.WaitForResultsAsync(createSessionReply.SessionId, new List<string> { resultId }, 100, 1, CancellationToken.None);

      // Download the result 
      var resultData = await resultClient.DownloadResultData(createSessionReply.SessionId, resultId, CancellationToken.None);


      if (resultData == null || !resultData.Any())
      {
        throw new Exception("No results available.");
      }

      // Get the string result
      string resultString = Encoding.ASCII.GetString(resultData);
      Console.WriteLine($"Raw result: {resultString}");
      // Parse it & compare to expected result
      var totalResult = int.Parse(resultString);
      var expectedResult = x * Math.Abs(y);

      // Negative case
      if (y < 0)
      {
        totalResult = -totalResult;
      }

      Console.WriteLine($"Total result: {totalResult}, Expected result: {expectedResult}");

      if (totalResult != expectedResult)
      {
        throw new ArithmeticException($"Calculated result {totalResult} does not match expected result {expectedResult}");
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
                              getDefaultValue: () => 2);
      var y = new Option<int>("--y",
                              description: "Second integer to multiply.",
                              getDefaultValue: () => 30);

      var rootCommand = new RootCommand("Client used to multiply x and y in ArmoniK.")
                  {
                      endpoint, partition, x, y
                  };

      rootCommand.SetHandler(Run, endpoint, partition, x, y);

      return await rootCommand.InvokeAsync(args);
    }
  }
}