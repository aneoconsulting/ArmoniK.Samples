// This file is part of the ArmoniK project
//
// Copyright (C) ANEO, 2021-2022.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
//
// Licensed under the Apache License, Version 2.0 (the "License");
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

using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.Client
{
  internal class Program
  {
    private static IConfiguration   configuration_;
    private static ILogger<Program> logger_;

    private static async Task Main(string[] args)
    {
      Console.WriteLine("Hello Armonik Unified Sample !");


      Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft",
                                                                   LogEventLevel.Information)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Console()
                                            .CreateLogger();

      var factory = new LoggerFactory(new[]
                                      {
                                        new SerilogLoggerProvider(Log.Logger),
                                      },
                                      new LoggerFilterOptions().AddFilter("Grpc",
                                                                          LogLevel.Error));

      logger_ = factory.CreateLogger<Program>();

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           false)
                                              .AddEnvironmentVariables();

      configuration_ = builder.Build();


      var rootCommand = new RootCommand("Samples for unifiedAPI: Binary for simple tests and quick benchmarks");
      var pTaskCommand = new Command("ptask",
                                     "Execute Parallel task with different number of task and/or different size of payload");
      var numberTaskOption = new Option<int>("--nbTask",
                                             description: "An option to set the number of task",
                                             getDefaultValue: () => 100);
      var numberOfDoubleElement = new Option<int>("--nbElement",
                                                  description: "An option to set the number of Double element a vector as Client payload",
                                                  getDefaultValue: () => 64000);
      var numberOfBytes = new Option<long>("--nbBytes",
                                           description:
                                           $"An option to set the number of Bytes for the client payload. Setting this option will override --nbElement value Default {64000 * 8} Bytes",
                                           getDefaultValue: () => 0);

      var workLoadTimeInMs = new Option<int>("--workLoadTimeInMs",
                                             description:
                                             $"Workload time in milliseconds. Time spent by a task to execute itself in the worker",
                                             getDefaultValue: () => 1);

      pTaskCommand.Add(numberTaskOption);
      pTaskCommand.Add(numberOfDoubleElement);
      pTaskCommand.Add(numberOfBytes);
      pTaskCommand.Add(workLoadTimeInMs);

      pTaskCommand.SetHandler((numberTaskOption,
                               numberOfDoubleElement,
                               numberOfBytes,
                               workLoadTimeInMs) =>
                              {
                                logger_.LogInformation("Option Parallel task Run");
                                logger_.LogInformation($"--nbTask             = {numberTaskOption}");
                                logger_.LogInformation($"--nbElement          = {numberOfDoubleElement}");
                                logger_.LogInformation($"--nbBytes            = {numberOfBytes}");
                                logger_.LogInformation($"--workLoadTimeInMs   = {workLoadTimeInMs}");

                                numberOfDoubleElement = numberOfBytes == 0
                                                          ? numberOfDoubleElement
                                                          : (int)(numberOfBytes / 8);

                                var test1 = new LargePayloadTests(configuration_,
                                                                  factory);

                                test1.LargePayloadSubmit(numberTaskOption,
                                                         numberOfDoubleElement,
                                                         workLoadTimeInMs);
                              },
                              numberTaskOption,
                              numberOfDoubleElement,
                              numberOfBytes,
                              workLoadTimeInMs);


      var simpleTestCommand = new Command("simple",
                                          "Execute Simple Unified API test with some Execution");

      simpleTestCommand.SetHandler(() =>
                                   {
                                     logger_.LogInformation("Running Simple execution test with UnifiedApi");
                                     new SimpleUnifiedAPI(configuration_,
                                                          factory).SimpleExecution();
                                   });

      var nTaskCommand = new Command("ntask",
                                     "Execute several thousand tasks in a batch of execution");
      nTaskCommand.Add(numberTaskOption);
      nTaskCommand.SetHandler(numberTaskOption =>
                              {
                                logger_.LogInformation("Running Simple execution test with UnifiedApi");
                                new NSubmitTest(configuration_,
                                                factory).BatchExecution(numberTaskOption);
                              },
                              numberTaskOption);


      rootCommand.Add(pTaskCommand);
      rootCommand.Add(simpleTestCommand);
      rootCommand.Add(nTaskCommand);

      //Default without parameters
      rootCommand.SetHandler(() =>
                             {
                               logger_.LogInformation("Running Simple execution test with UnifiedApi");
                               new SimpleUnifiedAPI(configuration_,
                                                    factory).SimpleExecution();
                             });

      await rootCommand.InvokeAsync(args);
    }
  }
}
