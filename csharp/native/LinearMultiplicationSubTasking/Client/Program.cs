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

      // Create client for task submission
      var taskClient = new Tasks.TasksClient(channel);

      // Create client for result creation
      var resultClient = new Results.ResultsClient(channel);

      // Create client for session creation
      var sessionClient = new Sessions.SessionsClient(channel);

      // Create client for events listening
      var eventClient = new Events.EventsClient(channel);

      // Default task options that will be used by each task if not overwritten when submitting tasks
      var taskOptions = new TaskOptions
      {
        MaxDuration = Duration.FromTimeSpan(TimeSpan.FromHours(1)),
        MaxRetries = 2,
        Priority = 1,
        PartitionId = partition,
      };

      var createSessionReply = sessionClient.CreateSession(new CreateSessionRequest
      {
        DefaultTaskOption = taskOptions,
        PartitionIds = { partition },
      });

      Console.WriteLine($"sessionId: {createSessionReply.SessionId}");

      // Create the result metadata and keep the id for task submission
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

      // Create the payload for the task
      var parameters = new Parameters { X = x, Y = y };
      var jsonString = JsonSerializer.Serialize(parameters);
      var jsonBytes = Encoding.ASCII.GetBytes(jsonString);

      // Create the payload metadata (a result) and upload data at the same time
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

      // Submit task with payload and result ids
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
      Console.WriteLine($"Submitted task with ID: {taskId}");

      // Wait for task end and result availability
      await eventClient.WaitForResultsAsync(createSessionReply.SessionId,
                                            new List<string> { resultId }, 100, 1,
                                            CancellationToken.None);

      // Télécharger les résultats (assurez-vous que 'test' contient bien plusieurs valeurs concaténées)
      var test = await resultClient.DownloadResultData(createSessionReply.SessionId, resultId, CancellationToken.None);

      // Vérifiez si des résultats ont été reçus
      if (test == null || !test.Any())
      {
        Console.WriteLine("No results available yet.");
        throw new Exception("No results available yet.");
      }

      // Affichage du nombre de bytes reçus
      Console.WriteLine($"Total bytes received: {test.Length}");
      // Convertir les bytes en une chaîne de caractères
      string resultString = Encoding.ASCII.GetString(test);

      // Loguer pour vérifier ce qui est reçu
      Console.WriteLine($"Decoded result as string: {resultString}");

      // Séparer chaque chiffre individuel de la chaîne
      var results = resultString.Split(',').Select(int.Parse).ToList();

      // Vérification des résultats calculés
      var totalResult = results.Sum();
      var expectedResult = x * Math.Abs(y);

      // Ajuster le résultat si y était négatif
      if (y < 0)
      {
        totalResult = -totalResult;
      }

      Console.WriteLine("Results: " + string.Join(", ", results));  // Affiche les résultats extraits
      Console.WriteLine($"Total result: {totalResult}, Expected result: {expectedResult}");

      if (totalResult != expectedResult)
      {
        throw new ArithmeticException($"The result of {x} * {y} is not equal to {totalResult} but to {expectedResult}");
      }
    }

    public static async Task<int> Main(string[] args)
    {
      var endpoint = new Option<string>("--endpoint",
                                        description: "Endpoint for the connection to ArmoniK control plane.",
                                        getDefaultValue: () => "http://192.168.252.47:5001");
      var partition = new Option<string>("--partition",
                                         description: "Name of the partition to which submit tasks.",
                                         getDefaultValue: () => "multiplicate");
      var x = new Option<int>("--x",
                              description: "First integer to multiply.",
                              getDefaultValue: () => 5);
      var y = new Option<int>("--y",
                              description: "Second integer to multiply.",
                              getDefaultValue: () => 3);

      var rootCommand = new RootCommand($"Calculate the result of x * y with subtask example for ArmoniK in the given partition <{partition.Name}>.\nIt sends a task to ArmoniK with x and y as input and waits for the result.\nArmoniK endpoint location is provided through <{endpoint.Name}>");

      rootCommand.AddOption(endpoint);
      rootCommand.AddOption(partition);
      rootCommand.AddOption(x);
      rootCommand.AddOption(y);

      rootCommand.SetHandler(Run, endpoint, partition, x, y);

      return await rootCommand.InvokeAsync(args);
    }
  }


  internal class Parameters
  {
    public int X { get; set; }
    public int Y { get; set; }
  }
}