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

using ArmoniK.Extensions.CSharp.Client;
using ArmoniK.Extensions.CSharp.Client.Common;
using ArmoniK.Extensions.CSharp.Client.Common.Domain.Blob;
using ArmoniK.Extensions.CSharp.Client.Common.Domain.Task;
using ArmoniK.Extensions.CSharp.Client.Handles;
using ArmoniK.Extensions.CSharp.Common.Common.Domain.Task;
using ArmoniK.Extensions.CSharp.Common.Library;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;
// ReSharper disable ArrangeNamespaceBody

namespace CppDynamicLibrary;

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

  public ValueTask OnErrorAsync(BlobHandle        blob,
                                Exception?        exception,
                                CancellationToken cancellationToken)
  {
    Console.WriteLine(exception?.Message ?? $"blob {blob.BlobInfo.BlobId} aborted");
    return ValueTask.CompletedTask;
  }
}

 /// <summary>
 /// Demonstrates dynamic library loading for a C++ worker: upload a .so at runtime and execute tasks with it.
 /// <para>
 /// The .so is uploaded once as a blob. Each task carries the blob ID in its task options (LibraryBlobId).
 /// The C++ DynamicWorker fetches the blob from the task's data dependencies, writes it to a temp file,
 /// dlopen()s it, and dispatches to the function named by Symbol via armonik_call.
 /// </para>
 /// <para>
 /// Configuration (appsettings.json, environment variables, or command-line flags, in increasing priority):<br/>
 /// <c>LibraryPath</c> — absolute path to the .so to upload  (e.g. --LibraryPath /path/to/libfoo.so)<br/>
 /// <c>Partition</c>   — ArmoniK partition to target          (e.g. --Partition cppdynamic)<br/>
 /// <c>Symbol</c>      — function_name passed to armonik_call (e.g. --Symbol multiply)
 /// </para>
 /// </summary>
 internal static class Program
 {
   private static async Task RunAsync(string[] args)
   {
     var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                             .AddJsonFile("appsettings.json",
                                                          optional: true,
                                                          reloadOnChange: false)
                                             .AddEnvironmentVariables()
                                             // Command-line args have the highest priority.
                                             // Usage: --LibraryPath /path/to/lib.so --Partition cppdynamic --Symbol multiply
                                             .AddCommandLine(args);

     var config = builder.Build();

     // Read parameters — command-line > environment > appsettings.json.
     var libraryPath = config["LibraryPath"]
                    ?? throw new InvalidOperationException(
                         "LibraryPath is required. Set it in appsettings.json, as an environment variable, or pass --LibraryPath.");
     var partition   = config["Partition"]!;
     var symbol      = config["Symbol"]!;

     var loggerFactory = new LoggerFactory([
                                             new SerilogLoggerProvider(new LoggerConfiguration().ReadFrom.Configuration(config)
                                                                                                .CreateLogger()),
                                           ],
                                           new LoggerFilterOptions().AddFilter("Grpc",
                                                                               LogLevel.Error));
     var logger = loggerFactory.CreateLogger(nameof(Program));

     logger.Log(LogLevel.Information, "Library : {libraryPath}", libraryPath);
     logger.Log(LogLevel.Information, "Partition: {partition}",  partition);
     logger.Log(LogLevel.Information, "Symbol   : {symbol}",     symbol);

     var properties        = new Properties(config);
     var taskConfiguration = new TaskConfiguration(3, 1, partition, TimeSpan.FromSeconds(300));
     var client            = new ArmoniKClient(properties, loggerFactory);

     logger.Log(LogLevel.Information, "Opening session on partition '{partition}'...", partition);
     var sessionHandle = await client.CreateSessionAsync([partition],
                                                         taskConfiguration,
                                                         true)
                                     .ConfigureAwait(false);
     logger.Log(LogLevel.Information, "Session opened: {sessionId}", sessionHandle.SessionInfo.SessionId);

     // Upload the .so as a blob. The blob ID is attached to every task so the worker
     // can retrieve the library content from its data dependencies at execution time.
     var content      = await File.ReadAllBytesAsync(libraryPath).ConfigureAwait(false);
     var libraryBlob  = await client.BlobService.CreateBlobAsync(sessionHandle,
                                                                  "cppLibrary",
                                                                  content,
                                                                  false,
                                                                  CancellationToken.None)
                                    .ConfigureAwait(false);

     logger.Log(LogLevel.Information, "Uploaded library blob: {blobId}", libraryBlob.BlobId);

     // Symbol is stored in the task options under the "Symbol" key.
     // The DynamicWorker passes it as function_name to armonik_call so the library
     // can dispatch to the right handler.
     var workerLibrary = new DynamicLibrary
                         {
                           Symbol        = symbol,
                           LibraryBlobId = libraryBlob.BlobId,
                         };

     var callBack       = new Callback();
     var taskDefinition = new TaskDefinition().WithTaskOptions(taskConfiguration)
                                              .WithLibrary(workerLibrary)
                                              .WithInput("num1",
                                                         BlobDefinition.FromString("two",
                                                                                   2.ToString()))
                                              .WithInput("num2",
                                                         BlobDefinition.FromString("three",
                                                                                   3.ToString()))
                                              .WithOutput("result",
                                                          BlobDefinition.CreateOutput("resultBlob").WithCallback(callBack));

     logger.Log(LogLevel.Information, "Submitting task (symbol: {symbol})...", symbol);
     sessionHandle.Submit(taskDefinition);

     logger.Log(LogLevel.Information, "Waiting for task result...");
     await sessionHandle.WaitCallbacksAsync().ConfigureAwait(false);

     var resultString = Encoding.UTF8.GetString(callBack.Result);
     logger.Log(LogLevel.Information, "Result: {result}", resultString);

     logger.Log(LogLevel.Information, "Closing session {sessionId}...", sessionHandle.SessionInfo.SessionId);
     await sessionHandle.CloseSessionAsync(CancellationToken.None).ConfigureAwait(false);
     logger.Log(LogLevel.Information, "Session {sessionId} closed.", sessionHandle.SessionInfo.SessionId);
   }

   private static async Task Main(string[] args)
   {
     try
     {
       await RunAsync(args).ConfigureAwait(false);
     }
     catch (Exception ex)
     {
       await Console.Error.WriteLineAsync($"Error: {ex.Message}");
       Environment.Exit(1);
     }
   }
 }
