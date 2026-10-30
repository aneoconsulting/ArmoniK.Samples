// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-2026. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text;
using System.Text.Json.Nodes;

using ArmoniK.Extensions.CSharp.Client;
using ArmoniK.Extensions.CSharp.Client.Common;
using ArmoniK.Extensions.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extensions.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extensions.CSharp.Client.Handles;
using ArmoniK.Extensions.CSharp.Common.Common.Domain.Task;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;
// ReSharper disable ArrangeNamespaceBody

namespace QuantLib;

/// <summary>
///   Keeps the content of a task output, or the reason why it is not available.
/// </summary>
internal class Callback : ICallback
{
  public byte[]? Result { get; private set; }
  public string? Error  { get; private set; }

  public ValueTask OnSuccessAsync(BlobHandle        blob,
                                  byte[]            rawData,
                                  CancellationToken cancellationToken)
  {
    Result = rawData;
    return ValueTask.CompletedTask;
  }

  public ValueTask OnErrorAsync(BlobHandle        blob,
                                Exception         exception,
                                CancellationToken cancellationToken)
  {
    Error = exception?.Message ?? "output aborted";
    return ValueTask.CompletedTask;
  }
}

/// <summary>
///   Sends pricing requests to the QuantLib worker (a Python worker) with the ArmoniK C# SDK.
/// <para>
///   The SDK follows the ArmoniK SDK convention v1, also implemented by the worker: each task has a named input,
///   <c>requests</c> (the JSON document of a batch of pricing requests), and a named output, <c>results</c>.
///   The SDK uploads the input, creates the output and the task payload referencing them by name.
/// </para>
/// <para>
///   Configuration (appsettings.json, environment variables, or command-line flags, in increasing priority):<br />
///   <c>Input</c> — JSON file with the pricing requests (e.g. --Input samples/mixed_batch.json)<br />
///   <c>Partition</c> — ArmoniK partition running the worker (e.g. --Partition quantlib)<br />
///   <c>BatchSize</c> — pricing requests per task (e.g. --BatchSize 3)<br />
/// </para>
/// </summary>
internal static class Program
{
  private static async Task<int> RunAsync(string[] args)
  {
    var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                                           .AddJsonFile("appsettings.json",
                                                        optional: true,
                                                        reloadOnChange: false)
                                           .AddEnvironmentVariables()
                                           .AddCommandLine(args)
                                           .Build();

    var input = config["Input"] ?? throw new InvalidOperationException("Input is required: pass --Input <file.json>.");
    var partition = config["Partition"]!;
    var batchSize = config.GetValue("BatchSize",
                                    10);

    var loggerFactory = new LoggerFactory([
                                            new SerilogLoggerProvider(new LoggerConfiguration().ReadFrom.Configuration(config)
                                                                                               .CreateLogger()),
                                          ],
                                          new LoggerFilterOptions().AddFilter("Grpc",
                                                                              LogLevel.Error));
    var logger = loggerFactory.CreateLogger(nameof(Program));

    // The input is a full payload: {"evaluationDate": ..., "requests": [...]}
    var document = JsonNode.Parse(await File.ReadAllTextAsync(input)
                                            .ConfigureAwait(false))!;
    var evaluationDate = document["evaluationDate"]
                           ?.GetValue<string>() ?? DateTime.Today.ToString("yyyy-MM-dd");
    var batches = document["requests"]!.AsArray()
                                       .Chunk(batchSize)
                                       .ToList();

    var taskConfiguration = new TaskConfiguration(2,
                                                  1,
                                                  partition,
                                                  TimeSpan.FromSeconds(300));
    var client = new ArmoniKClient(new Properties(config),
                                   loggerFactory);
    await using var sessionHandle = await client.CreateSessionAsync([partition],
                                                                    taskConfiguration,
                                                                    true)
                                                .ConfigureAwait(false);
    logger.LogInformation("Session {sessionId}: {batches} tasks on partition '{partition}'",
                          sessionHandle.SessionInfo.SessionId,
                          batches.Count,
                          partition);

    // One task per batch: input "requests" and output "results", the names expected by the worker
    var callbacks = batches.Select(_ => new Callback())
                           .ToList();
    sessionHandle.Submit(batches.Select((batch,
                                         i) => new TaskDefinition().WithTaskOptions(taskConfiguration)
                                                                   .WithInput("requests",
                                                                              InputBlobDefinition.FromString($"requests-{i}",
                                                                                                             new JsonObject
                                                                                                             {
                                                                                                               ["version"]        = 1,
                                                                                                               ["evaluationDate"] = evaluationDate,
                                                                                                               ["requests"] =
                                                                                                                 new JsonArray(batch.Select(r => r?.DeepClone())
                                                                                                                                    .ToArray()),
                                                                                                             }.ToJsonString()))
                                                                   .WithOutput("results",
                                                                               OutputBlobDefinition.CreateOutput($"results-{i}")
                                                                                                   .WithCallback(callbacks[i])))
                                .ToList());

    await sessionHandle.WaitCallbacksAsync()
                       .ConfigureAwait(false);

    // Reassemble the results in request order; a failed task yields one error per request of its batch
    var results = new JsonArray();
    for (var i = 0; i < batches.Count; i++)
    {
      if (callbacks[i].Result is { } data)
      {
        foreach (var r in JsonNode.Parse(Encoding.UTF8.GetString(data))!["results"]!.AsArray())
        {
          results.Add(r?.DeepClone());
        }
      }
      else
      {
        foreach (var request in batches[i])
        {
          results.Add(new JsonObject
                      {
                        ["id"]    = request?["id"]?.DeepClone(),
                        ["error"] = $"task failed: {callbacks[i].Error}",
                      });
        }
      }
    }

    Console.WriteLine(new JsonObject
                      {
                        ["version"]        = 1,
                        ["sessionId"]      = sessionHandle.SessionInfo.SessionId,
                        ["evaluationDate"] = evaluationDate,
                        ["results"]        = results,
                      }.ToJsonString(new System.Text.Json.JsonSerializerOptions
                                     {
                                       WriteIndented = true,
                                     }));

    var errors = results.Count(r => r?["error"] is not null);
    logger.LogInformation("{results} results, {errors} errors",
                          results.Count,
                          errors);
    return errors == 0
             ? 0
             : 1;
  }

  private static async Task<int> Main(string[] args)
  {
    try
    {
      return await RunAsync(args)
               .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      await Console.Error.WriteLineAsync($"Error: {ex.Message}");
      return 1;
    }
  }
}
