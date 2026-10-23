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

using System.Globalization;
using System.Text;

using ArmoniK.Extensions.CSharp.Client;
using ArmoniK.Extensions.CSharp.Client.Common;
using ArmoniK.Extensions.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extensions.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extensions.CSharp.Client.Handles;
using ArmoniK.Extensions.CSharp.Client.Services;
using ArmoniK.Extensions.CSharp.Common.Common.Domain.Task;
using ArmoniK.Extensions.CSharp.Common.Library;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;

namespace SubTasking.Client;

internal class Callback : ICallback
{
  public byte[] Result { get; private set; } = [];

  public ValueTask OnSuccessAsync(BlobHandle        blob,
                                  byte[]            rawData,
                                  CancellationToken cancellationToken)
  {
    Result = rawData;
    return ValueTask.CompletedTask;
  }

  public async ValueTask OnErrorAsync(BlobHandle        blob,
                                      Exception?        exception,
                                      CancellationToken cancellationToken)
  {
    if (exception is not null)
    {
      Console.WriteLine(exception.Message);
      return;
    }

    var blobInfo = await blob.GetBlobInfoAsync()
                             .ConfigureAwait(false);
    Console.WriteLine($"blob {blobInfo.BlobId} aborted");
  }
}

/// <summary>
///   Submits one task computing sum of i^2 for i in [1, N] to the C# dynamic worker.
///   The worker library splits the range recursively by submitting subtasks; the client only waits for the root result.
///   <para>
///     Configuration (appsettings.json, environment variables, or command-line flags, in increasing priority):<br />
///     <c>Grpc:EndPoint</c> control plane endpoint<br />
///     <c>Partition</c>     partition running dockerhubaneo/armonik-dynamic-dotnet-worker<br />
///     <c>WorkerZip</c>     zip produced by <c>dotnet publish</c> of SubTasking.Worker<br />
///     <c>WorkerVersion</c> version of SubTasking.Worker, used to locate the dll in the zip<br />
///     <c>N</c>, <c>Grain</c> problem size and leaf size
///   </para>
/// </summary>
internal static class Program
{
  private static async Task RunAsync(string[] args)
  {
    var config = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
                                           .AddJsonFile("appsettings.json",
                                                        optional: true,
                                                        reloadOnChange: false)
                                           .AddEnvironmentVariables()
                                           .AddCommandLine(args)
                                           .Build();

    var partition     = config["Partition"]     ?? "default";
    var workerZip     = config["WorkerZip"]     ?? throw new InvalidOperationException("WorkerZip is required.");
    var workerVersion = config["WorkerVersion"] ?? "1.0.0";
    var n             = config.GetValue("N",     1000L);
    var grain         = config.GetValue("Grain", 100L);

    var loggerFactory = new LoggerFactory([
                                            new SerilogLoggerProvider(new LoggerConfiguration().ReadFrom.Configuration(config)
                                                                                               .CreateLogger()),
                                          ],
                                          new LoggerFilterOptions().AddFilter("Grpc",
                                                                              LogLevel.Error));
    var logger = loggerFactory.CreateLogger(nameof(Program));

    var properties        = new Properties(config);
    var taskConfiguration = new TaskConfiguration(3,
                                                  1,
                                                  partition,
                                                  TimeSpan.FromSeconds(300));
    var client = new ArmoniKClient(properties,
                                   loggerFactory);

    var sessionHandle = await client.CreateSessionAsync([partition],
                                                        taskConfiguration,
                                                        true)
                                    .ConfigureAwait(false);
    logger.LogInformation("Session opened: {sessionId}", sessionHandle.SessionInfo.SessionId);

    // Symbol: full name of the IWorker class. LibraryPath: dll location inside the zip.
    var workerLibrary = new DynamicLibrary
                        {
                          Symbol      = "SubTasking.Worker.RangeSumWorker",
                          LibraryPath = $"SubTasking.Worker/{workerVersion}/SubTasking.Worker.dll",
                        };
    // Uploads the zip as a blob and sets workerLibrary.LibraryBlobId.
    await client.BlobService.SendDllBlobAsync(sessionHandle,
                                              workerLibrary,
                                              workerZip,
                                              false,
                                              CancellationToken.None)
                .ConfigureAwait(false);
    logger.LogInformation("Uploaded worker library blob: {blobId}", workerLibrary.LibraryBlobId);

    var callback = new Callback();
    var task = new TaskDefinition().WithLibrary(workerLibrary)
                                   .WithTaskOptions(taskConfiguration)
                                   .WithInput("lo",
                                              InputBlobDefinition.FromString("lo",
                                                                             "1"))
                                   .WithInput("hi",
                                              InputBlobDefinition.FromString("hi",
                                                                             (n + 1).ToString(CultureInfo.InvariantCulture)))
                                   .WithInput("grain",
                                              InputBlobDefinition.FromString("grain",
                                                                             grain.ToString(CultureInfo.InvariantCulture)))
                                   .WithOutput("result",
                                               OutputBlobDefinition.CreateOutput("result")
                                                                   .WithCallback(callback));

    var taskInfos = await sessionHandle.Submit(task)
                                       .GetTaskInfosAsync()
                                       .ConfigureAwait(false);
    logger.LogInformation("Root task submitted: {taskId} (N = {n}, grain = {grain})", taskInfos.TaskId, n, grain);

    await sessionHandle.WaitCallbacksAsync()
                       .ConfigureAwait(false);

    var result   = Encoding.UTF8.GetString(callback.Result);
    var expected = n * (n + 1) * (2 * n + 1) / 6;
    logger.LogInformation("Result: {result}, expected: {expected}", result, expected);

    await sessionHandle.CloseSessionAsync(CancellationToken.None)
                       .ConfigureAwait(false);

    if (result != expected.ToString(CultureInfo.InvariantCulture))
    {
      throw new InvalidOperationException($"Wrong result {result}, expected {expected}");
    }
  }

  private static async Task Main(string[] args)
  {
    try
    {
      await RunAsync(args)
        .ConfigureAwait(false);
    }
    catch (Exception ex)
    {
      await Console.Error.WriteLineAsync($"Error: {ex.Message}");
      Environment.Exit(1);
    }
  }
}
