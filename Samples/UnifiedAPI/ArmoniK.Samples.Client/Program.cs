// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2025. All rights reserved.
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
                                     "Execute Parallel tasks with different number of tasks and/or different sizes of payload");
      var numberTaskOption = new Option<int>("--nbTask",
                                             description: "An option to set the number of tasks",
                                             getDefaultValue: () => 100);
      var numberOfDoubleElement = new Option<int>("--nbElement",
                                                  description: "An option to set the number of Double elements in a vector as Client payload",
                                                  getDefaultValue: () => 64000);
      var numberOfBytes = new Option<long>("--nbBytes",
                                           description:
                                           $"An option to set the number of Bytes for the client payload. Setting this option will override --nbElement value Default {64000 * 8} Bytes",
                                           getDefaultValue: () => 0);

      var workLoadTimeInMs = new Option<int>("--workLoadTimeInMs",
                                             description: "Workload time in milliseconds. Time spent by a task to execute itself in the worker",
                                             getDefaultValue: () => 1);

      var partition = new Option<string>("--partition",
                                         () => "",
                                         "specify the partition to use for the session.");


      pTaskCommand.Add(numberTaskOption);
      pTaskCommand.Add(numberOfDoubleElement);
      pTaskCommand.Add(numberOfBytes);
      pTaskCommand.Add(workLoadTimeInMs);
      pTaskCommand.Add(partition);


      pTaskCommand.SetHandler((numberTaskOption,
                               numberOfDoubleElement,
                               numberOfBytes,
                               workLoadTimeInMs,
                               partition) =>
                              {
                                logger_.LogInformation("Option Parallel task Run");
                                logger_.LogInformation($"--nbTask             = {numberTaskOption}");
                                logger_.LogInformation($"--nbElement          = {numberOfDoubleElement}");
                                logger_.LogInformation($"--nbBytes            = {numberOfBytes}");
                                logger_.LogInformation($"--workLoadTimeInMs   = {workLoadTimeInMs}");
                                logger_.LogInformation($"--partition          = {partition}");

                                numberOfDoubleElement = numberOfBytes == 0
                                                          ? numberOfDoubleElement
                                                          : (int)(numberOfBytes / 8);

                                var test1 = new LargePayloadTests(configuration_,
                                                                  factory,
                                                                  partition);

                                test1.LargePayloadSubmit(numberTaskOption,
                                                         numberOfDoubleElement,
                                                         workLoadTimeInMs);
                              },
                              numberTaskOption,
                              numberOfDoubleElement,
                              numberOfBytes,
                              workLoadTimeInMs,
                              partition);


      var simpleTestCommand = new Command("simple",
                                          "Execute Simple Unified API test. It will submit 5 quick tasks and wait for 5 results");

      simpleTestCommand.SetHandler(() =>
                                   {
                                     logger_.LogInformation("Running Simple execution test with UnifiedApi");
                                     using var submitter = new SimpleUnifiedAPI(configuration_,
                                                                                1,
                                                                                factory);
                                     submitter.SimpleExecution();
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


      var additionCommand = new Command("addition",
                                        "Addition test with healthcheck after 10 calls");
      var addTaskOption = new Option<int>("--nbTask",
                                          () => 20,
                                          "Number of tasks to execute"); // Default 20
      var addProgressiveOption = new Option<bool>("--progressive",
                                                  () => false,
                                                  "Progressive test");
      var twoNumbersOption = new Option<bool>("--two",
                                              () => false,
                                              "Two numbers addition test");

      additionCommand.Add(addTaskOption);
      additionCommand.Add(addProgressiveOption);
      additionCommand.Add(twoNumbersOption);

      additionCommand.SetHandler((nbTask,
                                  progressive,
                                  twoNumbers) =>
                                 {
                                   logger_.LogInformation("Running Addition test with HealthCheck (10 calls limit, worker restart)");

                                   using var test = new AdditionTest(configuration_,
                                                                     factory);

                                   if (progressive)
                                   {
                                     test.ProgressiveTest();
                                   }
                                   else if (twoNumbers)
                                   {
                                     test.TwoNumbersTest(nbTask);
                                   }
                                   else
                                   {
                                     test.SimpleAdditionTest(nbTask);
                                   }
                                 },
                                 addTaskOption,
                                 addProgressiveOption,
                                 twoNumbersOption);


      rootCommand.Add(pTaskCommand);
      rootCommand.Add(simpleTestCommand);
      rootCommand.Add(nTaskCommand);
      rootCommand.Add(additionCommand); // Keep the command but with disabled functionality

      //Default without parameters
      rootCommand.SetHandler(() =>
                             {
                               logger_.LogInformation("Running Simple execution test with UnifiedApi");
                               using var submitter = new SimpleUnifiedAPI(configuration_,
                                                                          1,
                                                                          factory);
                               submitter.SimpleExecution();
                             });

      await rootCommand.InvokeAsync(args);
    }
  }
}
