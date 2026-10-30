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

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
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

using Microsoft.Extensions.Configuration;

using static System.Console;

namespace ArmoniK.Samples.QuantLib.Client
{
  internal static class Program
  {
    /// <summary>
    ///   Reads pricing requests from a JSON file, sends them to the QuantLib worker in tasks of
    ///   <paramref name="batchSize" /> requests, and writes the results, in request order, as JSON.
    /// </summary>
    /// <param name="input">JSON file: a full payload, a list of requests or a single request</param>
    /// <param name="endpoint">The endpoint url of ArmoniK's control plane</param>
    /// <param name="partition">Partition Id of the QuantLib worker, empty for the default partition</param>
    /// <param name="batchSize">Pricing requests per task</param>
    /// <param name="repeat">Number of times the requests are submitted, for load tests</param>
    /// <param name="evaluationDate">Evaluation date used when the input does not give one</param>
    /// <param name="maxRetries">Task retries on worker failure</param>
    /// <param name="output">File receiving the results, stdout when empty</param>
    /// <returns>0 when every request was priced, 1 otherwise</returns>
    internal static async Task<int> Run(FileInfo input,
                                        string   endpoint,
                                        string   partition,
                                        int      batchSize,
                                        int      repeat,
                                        string   evaluationDate,
                                        int      maxRetries,
                                        string   output)
    {
      var (date, requests) = LoadRequests(input,
                                          evaluationDate,
                                          repeat);
      var batches = requests.Chunk(batchSize)
                            .ToList();

      var configuration = new ConfigurationBuilder().AddEnvironmentVariables()
                                                    .Build();
      var options = configuration.GetSection(GrpcClient.SettingSection)
                                 .Get<GrpcClient>() ?? new GrpcClient();
      options.Endpoint = endpoint;

      var stopwatch = Stopwatch.StartNew();

      // Create gRPC channel and clients to connect with ArmoniK control plane
      var channel       = GrpcChannelFactory.CreateChannel(options);
      var sessionClient = new Sessions.SessionsClient(channel);
      var resultClient  = new Results.ResultsClient(channel);
      var taskClient    = new Tasks.TasksClient(channel);
      var eventClient   = new Events.EventsClient(channel);

      var sessionRequest = new CreateSessionRequest
                           {
                             DefaultTaskOption = new TaskOptions
                                                 {
                                                   MaxDuration = Duration.FromTimeSpan(TimeSpan.FromMinutes(5)),
                                                   MaxRetries  = maxRetries,
                                                   Priority    = 1,
                                                   PartitionId = partition,
                                                   // ArmoniK SDK convention v1 (inherited by the tasks): the worker reads
                                                   // its input and output blob ids from the task payload
                                                   Options =
                                                   {
                                                     ["ConventionVersion"] = "v1",
                                                   },
                                                 },
                           };
      if (!string.IsNullOrEmpty(partition))
      {
        sessionRequest.PartitionIds.Add(partition);
      }

      var sessionId = (await sessionClient.CreateSessionAsync(sessionRequest)).SessionId;
      Console.Error.WriteLine($"Session {sessionId}: {requests.Count} requests in {batches.Count} tasks");

      // One input per batch: the pricing requests, as the JSON document expected by the worker
      var inputIds = (await resultClient.CreateResultsAsync(new CreateResultsRequest
                                                            {
                                                              SessionId = sessionId,
                                                              Results =
                                                              {
                                                                batches.Select((batch,
                                                                                i) => new CreateResultsRequest.Types.ResultCreate
                                                                                      {
                                                                                        Name = $"requests-{i}",
                                                                                        Data = ByteString.CopyFromUtf8(new JsonObject
                                                                                                                       {
                                                                                                                         ["version"]        = 1,
                                                                                                                         ["evaluationDate"] = date,
                                                                                                                         ["requests"] =
                                                                                                                           new JsonArray(batch
                                                                                                                                         .Select(r => r?.DeepClone())
                                                                                                                                         .ToArray()),
                                                                                                                       }.ToJsonString()),
                                                                                      }),
                                                              },
                                                            })).Results.ToDictionary(r => r.Name,
                                                                                     r => r.ResultId);

      // One output per batch, filled by the worker with the JSON results
      var outputIds = (await resultClient.CreateResultsMetaDataAsync(new CreateResultsMetaDataRequest
                                                                     {
                                                                       SessionId = sessionId,
                                                                       Results =
                                                                       {
                                                                         batches.Select((_,
                                                                                         i) => new CreateResultsMetaDataRequest.Types.ResultCreate
                                                                                               {
                                                                                                 Name = $"results-{i}",
                                                                                               }),
                                                                       },
                                                                     })).Results.ToDictionary(r => r.Name,
                                                                                              r => r.ResultId);
      var orderedOutputIds = batches.Select((_,
                                             i) => outputIds[$"results-{i}"])
                                    .ToList();

      // The task payload only references the input and the output by name
      var payloadIds = (await resultClient.CreateResultsAsync(new CreateResultsRequest
                                                              {
                                                                SessionId = sessionId,
                                                                Results =
                                                                {
                                                                  batches.Select((_,
                                                                                  i) => new CreateResultsRequest.Types.ResultCreate
                                                                                        {
                                                                                          Name = $"payload-{i}",
                                                                                          Data = ByteString.CopyFromUtf8(new JsonObject
                                                                                                                         {
                                                                                                                           ["inputs"] = new JsonObject
                                                                                                                                        {
                                                                                                                                          ["requests"] =
                                                                                                                                            inputIds[$"requests-{i}"],
                                                                                                                                        },
                                                                                                                           ["outputs"] = new JsonObject
                                                                                                                                         {
                                                                                                                                           ["results"] =
                                                                                                                                             outputIds[$"results-{i}"],
                                                                                                                                         },
                                                                                                                         }.ToJsonString()),
                                                                                        }),
                                                                },
                                                              })).Results.ToDictionary(r => r.Name,
                                                                                       r => r.ResultId);

      await taskClient.SubmitTasksAsync(new SubmitTasksRequest
                                        {
                                          SessionId = sessionId,
                                          TaskCreations =
                                          {
                                            batches.Select((_,
                                                            i) => new SubmitTasksRequest.Types.TaskCreation
                                                                  {
                                                                    PayloadId = payloadIds[$"payload-{i}"],
                                                                    ExpectedOutputKeys =
                                                                    {
                                                                      outputIds[$"results-{i}"],
                                                                    },
                                                                    DataDependencies =
                                                                    {
                                                                      inputIds[$"requests-{i}"],
                                                                    },
                                                                  }),
                                          },
                                        });
      Console.Error.WriteLine($"Submitted {batches.Count} tasks in {stopwatch.Elapsed.TotalSeconds:F2}s");

      await WaitForOutputs(eventClient,
                           resultClient,
                           sessionId,
                           orderedOutputIds);

      // Reassemble the results in request order; a failed task yields one error per request of its batch
      var results     = new JsonArray();
      var failedTasks = new JsonArray();
      for (var i = 0; i < batches.Count; i++)
      {
        var result = (await resultClient.GetResultAsync(new GetResultRequest
                                                        {
                                                          ResultId = orderedOutputIds[i],
                                                        })).Result;
        if (result.Status == ResultStatus.Completed)
        {
          var data = await resultClient.DownloadResultData(sessionId,
                                                           result.ResultId,
                                                           CancellationToken.None);
          foreach (var r in JsonNode.Parse(Encoding.UTF8.GetString(data))!["results"]!.AsArray())
          {
            results.Add(r?.DeepClone());
          }

          continue;
        }

        var task = (await taskClient.GetTaskAsync(new GetTaskRequest
                                                  {
                                                    TaskId = result.OwnerTaskId,
                                                  })).Task;
        var error = string.IsNullOrEmpty(task.Output?.Error)
                      ? task.StatusMessage
                      : task.Output.Error;
        failedTasks.Add(new JsonObject
                        {
                          ["taskId"] = task.Id,
                          ["error"]  = error,
                        });
        foreach (var request in batches[i])
        {
          results.Add(new JsonObject
                      {
                        ["id"] = request?["id"]
                          ?.DeepClone(),
                        ["error"] = $"task failed: {error}",
                      });
        }
      }

      await sessionClient.CloseSessionAsync(new CloseSessionRequest
                                            {
                                              SessionId = sessionId,
                                            });

      var document = new JsonObject
                     {
                       ["version"]        = 1,
                       ["sessionId"]      = sessionId,
                       ["evaluationDate"] = date,
                       ["results"]        = results,
                       ["failedTasks"]    = failedTasks,
                     }.ToJsonString(new JsonSerializerOptions
                                    {
                                      WriteIndented = true,
                                    });
      if (string.IsNullOrEmpty(output))
      {
        WriteLine(document);
      }
      else
      {
        await File.WriteAllTextAsync(output,
                                     document + "\n");
      }

      var errors = results.Count(r => r?["error"] is not null);
      Console.Error.WriteLine($"{results.Count} results, {errors} errors, {failedTasks.Count} failed tasks in {stopwatch.Elapsed.TotalSeconds:F2}s");
      return errors == 0 && failedTasks.Count == 0
               ? 0
               : 1;
    }

    /// <summary>
    ///   Reads the input file. Accepts a full payload (<c>{"evaluationDate": ..., "requests": [...]}</c>), a list of
    ///   requests or a single request.
    /// </summary>
    private static (string Date, List<JsonNode?> Requests) LoadRequests(FileInfo input,
                                                                        string   evaluationDate,
                                                                        int      repeat)
    {
      var document = JsonNode.Parse(File.ReadAllText(input.FullName)) ?? throw new ArgumentException($"{input} is empty");

      List<JsonNode?> requests;
      var             date = evaluationDate;
      if (document is JsonObject obj && obj["requests"] is JsonArray array)
      {
        requests = array.ToList();
        if (string.IsNullOrEmpty(date))
        {
          date = obj["evaluationDate"]
                   ?.GetValue<string>() ?? "";
        }
      }
      else
      {
        requests = document is JsonArray list
                     ? list.ToList()
                     : new List<JsonNode?>
                       {
                         document,
                       };
      }

      if (string.IsNullOrEmpty(date))
      {
        date = DateTime.Today.ToString("yyyy-MM-dd");
      }

      // Repeated requests get a "#k" suffix so their results stay distinguishable
      var repeated = Enumerable.Range(0,
                                      repeat)
                               .SelectMany(k => requests.Select(r =>
                                                                {
                                                                  var copy = r?.DeepClone();
                                                                  if (repeat > 1 && copy is JsonObject o)
                                                                  {
                                                                    o["id"] = $"{o["id"]}#{k}";
                                                                  }

                                                                  return copy;
                                                                }))
                               .ToList();
      return (date, repeated);
    }

    /// <summary>
    ///   Waits until every output is completed or aborted: <c>WaitForResultsAsync</c> throws at the first aborted
    ///   result, so the statuses are checked and the wait resumes on the outputs still pending.
    /// </summary>
    private static async Task WaitForOutputs(Events.EventsClient   eventClient,
                                             Results.ResultsClient resultClient,
                                             string                sessionId,
                                             ICollection<string>   outputIds)
    {
      var pending = outputIds.ToList();
      while (pending.Count > 0)
      {
        try
        {
          await eventClient.WaitForResultsAsync(sessionId,
                                                pending,
                                                parallelism: 1,
                                                bucket_size: 100,
                                                cancellationToken: CancellationToken.None);
          return;
        }
        catch (Exception) when (pending.Count > 0)
        {
          var stillPending = new List<string>();
          foreach (var id in pending)
          {
            var status = (await resultClient.GetResultAsync(new GetResultRequest
                                                            {
                                                              ResultId = id,
                                                            })).Result.Status;
            if (status != ResultStatus.Completed && status != ResultStatus.Aborted)
            {
              stillPending.Add(id);
            }
          }

          if (stillPending.Count == pending.Count)
          {
            throw;
          }

          pending = stillPending;
        }
      }
    }

    public static async Task<int> Main(string[] args)
    {
      // Define the arguments and options for the application with their description and default value
      var input = new Argument<FileInfo>("input",
                                         "JSON file with the pricing requests (see python/quantlib/samples).");
      var endpoint = new Option<string>("--endpoint",
                                        description: "Endpoint for the connection to ArmoniK control plane.",
                                        getDefaultValue: () => "http://localhost:5001");
      var partition = new Option<string>("--partition",
                                         description: "Name of the partition running the QuantLib worker, empty for the default one.",
                                         getDefaultValue: () => "");
      var batchSize = new Option<int>("--batch-size",
                                      description: "Number of pricing requests per task.",
                                      getDefaultValue: () => 10);
      var repeat = new Option<int>("--repeat",
                                   description: "Submit the requests N times, for load tests.",
                                   getDefaultValue: () => 1);
      var evaluationDate = new Option<string>("--evaluation-date",
                                              description: "Evaluation date (yyyy-MM-dd) when the input has none; today by default.",
                                              getDefaultValue: () => "");
      var maxRetries = new Option<int>("--max-retries",
                                       description: "Task retries on worker failure.",
                                       getDefaultValue: () => 2);
      var output = new Option<string>("--output",
                                      description: "Write the results JSON to this file instead of stdout.",
                                      getDefaultValue: () => "");

      foreach (var option in new[]
                             {
                               batchSize,
                               repeat,
                             })
      {
        option.AddValidator(result =>
                            {
                              if (result.GetValueOrDefault<int>() < 1)
                              {
                                result.ErrorMessage = $"{option.Name} must be >= 1";
                              }
                            });
      }

      // Describe the application and its purpose
      var rootCommand =
        new
          RootCommand("QuantLib pricing client for ArmoniK.\nIt reads JSON pricing requests (options, bonds, swaps), sends them to the QuantLib worker in tasks of <--batch-size> requests and prints the results in request order as JSON.");
      rootCommand.AddArgument(input);
      rootCommand.AddOption(endpoint);
      rootCommand.AddOption(partition);
      rootCommand.AddOption(batchSize);
      rootCommand.AddOption(repeat);
      rootCommand.AddOption(evaluationDate);
      rootCommand.AddOption(maxRetries);
      rootCommand.AddOption(output);

      // Configure the handler to call the function that will do the work
      var exitCode = 0;
      rootCommand.SetHandler(async (i,
                                    e,
                                    p,
                                    b,
                                    r,
                                    d,
                                    m,
                                    o) => exitCode = await Run(i,
                                                               e,
                                                               p,
                                                               b,
                                                               r,
                                                               d,
                                                               m,
                                                               o),
                             input,
                             endpoint,
                             partition,
                             batchSize,
                             repeat,
                             evaluationDate,
                             maxRetries,
                             output);

      // Parse the command line parameters and call the function that represents the application
      var parseResult = await rootCommand.InvokeAsync(args);
      return parseResult != 0
               ? parseResult
               : exitCode;
    }
  }
}
