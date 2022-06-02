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
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.DevelopmentKit.GridServer.Client;
using ArmoniK.Samples.GridServer.Common;
using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

namespace ArmoniK.Samples.GridServer.Client
{
  internal class Program
  {
    private static IConfiguration   configuration_;
    private static ILogger<Program> logger_;

    private static void Main(string[] args)
    {
      Console.WriteLine("Hello Armonik SymphonyLike Sample !");

      Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Override("Microsoft",
                                          LogEventLevel.Information)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .CreateLogger();

      var factory = new LoggerFactory(Array.Empty<ILoggerProvider>(),
        new LoggerFilterOptions().AddFilter("Grpc",
          LogLevel.Error)).AddSerilog();

      logger_ = factory.CreateLogger<Program>();

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           false)
                                              .AddEnvironmentVariables();

      configuration_ = builder.Build();

      var taskOptions = InitializeSimpleTaskOptions();

      var props = new Properties(taskOptions, configuration_.GetSection("Grpc")["EndPoint"], 5001);

      using var sessionService = ServiceFactory.GetInstance().CreateService("ArmoniK.Samples.GridServer.Package",
                                                                            props);

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

      var handler = new ResultHandler(logger_);

      sessionService.Submit("ComputeBasicArrayCube",
                            ParamsHelper(numbers),
                            handler);

      sessionService.Submit("ComputeReduceCube",
                            ParamsHelper(numbers),
                            handler);

      sessionService.Submit("ComputeReduceCube",
                            ParamsHelper(numbers.SelectMany(BitConverter.GetBytes).ToArray()),
                            handler);

      sessionService.Submit("ComputeMadd",
                            ParamsHelper(numbers.SelectMany(BitConverter.GetBytes).ToArray(),
                                         numbers.SelectMany(BitConverter.GetBytes).ToArray(),
                                         4.0),
                            handler);

      sessionService.Submit("NonStaticComputeMadd",
                            ParamsHelper(numbers.SelectMany(BitConverter.GetBytes).ToArray(),
                                         numbers.SelectMany(BitConverter.GetBytes).ToArray(),
                                         4.0),
                            handler);
    }


    private static object[] ParamsHelper(params object[] elements)
    {
      return elements;
    }

    /// <summary>
    ///   Initialize Setting for task i.e :
    ///   Duration :
    ///   The max duration of a task
    ///   Priority :
    ///   Work in Progress. Setting priority of task
    ///   AppName  :
    ///   The name of the Application dll (Without Extension)
    ///   VersionName :
    ///   The version of the package to unzip and execute
    ///   Namespace :
    ///   The namespace where the service can find
    ///   the ServiceContainer object develop by the customer
    /// </summary>
    /// <returns></returns>
    private static TaskOptions InitializeSimpleTaskOptions()
    {
      TaskOptions taskOptions = new TaskOptions
      {
        MaxDuration = new Duration
        {
          Seconds = 3600 * 24,
        },
        MaxRetries = 3,
        Priority   = 1,
      };

      taskOptions.Options.Add(AppsOptions.EngineTypeNameKey,
                              EngineType.DataSynapse.ToString());

      taskOptions.Options.Add(AppsOptions.GridAppNameKey,
                              "ArmoniK.Samples.GridServer.Services");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "1.0.0-700");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.Samples.GridServer.Client.Services");

      taskOptions.Options.Add(AppsOptions.GridServiceNameKey,
                              "ServiceContainer");

      return taskOptions;
    }


    // Handler for Service Clients
    private class ResultHandler : IServiceInvocationHandler
    {
      private readonly double           _total = 0;
      private          ILogger<Program> logger_;
      public ResultHandler(ILogger<Program> logger)
      {
        logger_ = logger;
      }


      public void HandleError(ServiceInvocationException e, string taskId)
      {
        logger_.LogError($"Error from {taskId} : " + e.Message);
        throw new ApplicationException($"Error from {taskId}",
                                       e);
      }

      public void HandleResponse(object response, string taskId)
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
            logger_.LogInformation("Result is " +
                                   string.Join(", ",
                                               doubles));
            break;
          case byte[] values:
            logger_.LogInformation("Result is " +
                                   string.Join(", ",
                                               values.ConvertToArray()));
            break;
        }

      }
    }
  }
}