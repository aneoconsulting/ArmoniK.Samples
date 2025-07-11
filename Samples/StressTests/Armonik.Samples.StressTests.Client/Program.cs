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

using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using Armonik.Samples.StressTests.Client;

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
      Console.WriteLine("Hello Armonik StressTest");


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
      var pTaskCommand = new Command("stressTest",
                                     "Execute Parallel tasks with different number of tasks and/or different sizes of payload")
                         {
                           new Option<int>("--nbTask",
                                           description: "An option to set the number of tasks",
                                           getDefaultValue: () => 100),

                           new Option<long>("--nbInputBytes",
                                            description: $"An option to set the number of Bytes for the input payload. Default {64000 * 8} Bytes",
                                            getDefaultValue: () => 64000 * 8),
                           new Option<long>("--nbOutputBytes",
                                            description: "An option to set the number of Bytes for the result payload. Default 8 Bytes",
                                            getDefaultValue: () => 8),
                           new Option<int>("--workLoadTimeInMs",
                                           description: "Workload time in milliseconds. Time spent by a task to execute itself in the worker",
                                           getDefaultValue: () => 1),
                           new Option<string>("--partition",
                                              () => "",
                                              "specify the partition to use for the session."),
                           new Option<int>("--nbTaskPerBuffer",
                                           () => 50,
                                           "specify the number of task per buffer"),
                           new Option<int>("--nbBufferPerChannel",
                                           () => 5,
                                           "specify the number of concurrent buffer per channel"),
                           new Option<int>("--nbChannel",
                                           () => 5,
                                           "specify the number of Grpc Channel"),
                           new Option<string>("--jsonPath",
                                              () => "",
                                              "specify the jsonPath to get metrics. Options are Raw or Json"),
                         };

      pTaskCommand.Handler = CommandHandler.Create((ContainerOptions options) =>
                                                   {
                                                     logger_.LogInformation("Option Parallel task Run");
                                                     logger_.LogInformation($"--nbTask              = {options.NbTask}");
                                                     logger_.LogInformation($"--nbInputBytes        = {options.NbInputBytes}");
                                                     logger_.LogInformation($"--nbOutputBytes       = {options.NbOutputBytes}");
                                                     logger_.LogInformation($"--workLoadTimeInMs    = {options.WorkLoadTimeInMs}");
                                                     logger_.LogInformation($"--partition           = {options.Partition}");
                                                     logger_.LogInformation($"--nbTaskPerBuffer     = {options.NbTaskPerBuffer}");
                                                     logger_.LogInformation($"--nbBufferPerChannel  = {options.NbBufferPerChannel}");
                                                     logger_.LogInformation($"--nbChannel           = {options.NbChannel}");
                                                     logger_.LogInformation($"--jsonPath            = {options.JsonPath}");


                                                     var test1 = new StressTests(configuration_,
                                                                                 factory,
                                                                                 options.Partition,
                                                                                 options.NbTaskPerBuffer,
                                                                                 options.NbBufferPerChannel,
                                                                                 options.NbChannel);


                                                     test1.LargePayloadSubmit(options.NbTask,
                                                                              options.NbInputBytes,
                                                                              options.NbOutputBytes,
                                                                              options.WorkLoadTimeInMs,
                                                                              options.JsonPath);
                                                   });

      rootCommand.Add(pTaskCommand);

      //Default without parameters
      rootCommand.SetHandler(() =>
                             {
                               logger_.LogError("Please select one stress test to execute");
                             });

      await rootCommand.InvokeAsync(args);
    }

    public class ContainerOptions
    {
      public int    NbTask             { get; set; }
      public long   NbInputBytes       { get; set; }
      public long   NbOutputBytes      { get; set; }
      public int    WorkLoadTimeInMs   { get; set; }
      public string Partition          { get; set; }
      public int    NbTaskPerBuffer    { get; set; }
      public int    NbBufferPerChannel { get; set; }
      public int    NbChannel          { get; set; }
      public string JsonPath           { get; set; }
    }
  }
}
