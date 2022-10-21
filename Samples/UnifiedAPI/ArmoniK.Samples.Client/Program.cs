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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Exceptions;
using ArmoniK.DevelopmentKit.Client.Factory;
using ArmoniK.DevelopmentKit.Client.Services;
using ArmoniK.DevelopmentKit.Client.Services.Admin;
using ArmoniK.DevelopmentKit.Client.Services.Submitter;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.Samples.Common;

using Google.Protobuf.WellKnownTypes;

using JetBrains.Annotations;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.Client
{
  internal class Program
  {
    private static IConfiguration   _configuration;
    private static ILogger<Program> _logger;

    private static void Main(string[] args)
    {
      Console.WriteLine("Hello Armonik Unified Sample !");

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();

      _configuration = builder.Build();

      var factory = new LoggerFactory(new[]
                                      {
                                        new SerilogLoggerProvider(new LoggerConfiguration().ReadFrom.Configuration(_configuration)
                                                                                           .MinimumLevel.Override("Microsoft",
                                                                                                                  LogEventLevel.Information)
                                                                                           .Enrich.FromLogContext()
                                                                                           .WriteTo.Console()
                                                                                           .CreateLogger()),
                                      },
                                      new LoggerFilterOptions().AddFilter("Grpc",
                                                                          LogLevel.Error));


      _logger = factory.CreateLogger<Program>();

      _configuration = builder.Build();

      var taskOptions = new TaskOptions
                        {
                          MaxDuration = new Duration
                                        {
                                          Seconds = 3600 * 24,
                                        },
                          MaxRetries           = 3,
                          Priority             = 1,
                          EngineType           = EngineType.Unified.ToString(),
                          ApplicationVersion   = "1.0.0-700",
                          ApplicationService   = "ServiceApps",
                          ApplicationName      = "ArmoniK.Samples.Unified.Worker",
                          ApplicationNamespace = "ArmoniK.Samples.Unified.Worker.Services",
                        };

      var props = new Properties(taskOptions,
                                 _configuration.GetSection("Grpc")["EndPoint"]);

      using var sessionService = ServiceFactory.CreateService(props,
                                                              factory);
      using var sessionServiceAdmin = ServiceFactory.GetServiceAdmin(props,
                                                                     factory);
      var handler = new ResultHandler(_logger);

      _logger.LogInformation("Running Simple execution test with UnifiedApi");


      SimpleExecution(sessionService,
                      handler);

      RunningAndCancelSession(sessionService,
                              sessionServiceAdmin,
                              handler);

      LargePayloadTests.LargePayloadSubmit(props,
                                           factory);
    }

    private static void SimpleExecution(Service       sessionService,
                                        ResultHandler handler)
    {
      var numbers = new List<double>
                    {
                      1.0,
                      2.0,
                      3.0,
                      3.0,
                      3.0,
                      3.0,
                      3.0,
                      3.0,
                    }.ToArray();
      var workloadInMs = 10;

      sessionService.Submit("ComputeBasicArrayCube",
                            Utils.ParamsHelper(numbers),
                            handler);

      sessionService.Submit("ComputeReduceCube",
                            Utils.ParamsHelper(numbers,
                                               workloadInMs),
                            handler);

      sessionService.Submit("ComputeReduceCube",
                            Utils.ParamsHelper(numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray()),
                            handler);

      sessionService.Submit("ComputeMadd",
                            Utils.ParamsHelper(numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               4.0),
                            handler);

      sessionService.Submit("NonStaticComputeMadd",
                            Utils.ParamsHelper(numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               4.0),
                            handler);
    }

    /// <summary>
    ///   The first test developed to validate the Session cancellation
    /// </summary>
    /// <param name="sessionService"></param>
    private static void RunningAndCancelSession(Service                 sessionService,
                                                ServiceAdmin            serviceAdmin,
                                                [NotNull] ResultHandler handler)
    {
      if (handler == null)
      {
        throw new ArgumentNullException(nameof(handler));
      }

      var numbers = new List<double>
                    {
                      1.0,
                      2.0,
                      3.0,
                      3.0,
                      3.0,
                      3.0,
                      3.0,
                      3.0,
                    }.ToArray();

      const int wantedCount = 100;
      var tasks = sessionService.Submit("ComputeBasicArrayCube",
                                        Enumerable.Range(1,
                                                         wantedCount)
                                                  .Select(n => Utils.ParamsHelper(numbers)),
                                        handler);
      if (tasks.Count() is var count && count != wantedCount)
      {
        throw new ApplicationException($"Expected {wantedCount} submitted tasks, got {count}");
      }

      //Get the count of running tasks after 10 s
      Thread.Sleep(15000);

      var countRunningTasks = serviceAdmin.AdminMonitoringService.CountCompletedTasksBySession(sessionService.SessionId);
      _logger.LogInformation($"Number of completed tasks after 15 seconds is {countRunningTasks}");

      //Cancel all the session
      _logger.LogInformation("Cancel the whole session");
      serviceAdmin.AdminMonitoringService.CancelSession(sessionService.SessionId);

      //Get the count of running tasks after 10 s
      Thread.Sleep(10000);
      //Cancel all the session
      var countCancelTasks = serviceAdmin.AdminMonitoringService.CountCancelTasksBySession(sessionService.SessionId);
      _logger.LogInformation($"Number of canceled tasks after Session cancel is {countCancelTasks}");

      countRunningTasks = serviceAdmin.AdminMonitoringService.CountCompletedTasksBySession(sessionService.SessionId);
      _logger.LogInformation($"Number of running tasks after Session cancel is {countRunningTasks}");


      var countErrorTasks = serviceAdmin.AdminMonitoringService.CountErrorTasksBySession(sessionService.SessionId);
      _logger.LogInformation($"Number of error tasks after Session cancel is {countErrorTasks}");
    }

    // Handler for Service Clients
    private class ResultHandler : IServiceInvocationHandler
    {
      private readonly double           _total = 0;
      private readonly ILogger<Program> logger_;

      public ResultHandler(ILogger<Program> logger)
        => logger_ = logger;


      public void HandleError(ServiceInvocationException e,
                              string                     taskId)
      {
        logger_.LogError($"Error from {taskId} : " + e.Message);
        throw new ApplicationException($"Error from {taskId}",
                                       e);
      }

      public void HandleResponse(object response,
                                 string taskId)
      {
        switch (response)
        {
          case null:
            logger_.LogInformation("Task finished but nothing returned in Result");
            break;
          case double value:
            logger_.LogInformation($"Task finished with result {value}");
            break;
          case double[] doubles:
            logger_.LogInformation("Result is " + string.Join(", ",
                                                              doubles));
            break;
          case byte[] values:
            logger_.LogInformation("Result is " + string.Join(", ",
                                                              values.ConvertToArray()));
            break;
        }
      }
    }
  }
}
