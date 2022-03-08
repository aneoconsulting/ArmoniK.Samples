// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.DevelopmentKit.GridServer.Client;
using ArmoniK.DevelopmentKit.WorkerApi.Common;
using ArmoniK.Samples.GridServer.Client.Services;

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
                   .CreateBootstrapLogger();

      var factory = new LoggerFactory().AddSerilog();

      logger_ = factory.CreateLogger<Program>();

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();

      configuration_ = builder.Build();

      var taskOptions = InitializeSimpleTaskOptions();

      var props = new Properties(configuration_,
                                 taskOptions);


      var cs = ServiceFactory.GetInstance().CreateService("ArmoniK.Samples.GridServer.Package",
                                                          props);

      object[] arguments = { 5.0 };
      var sum = cs.LocalExecute(new ServiceContainer(),
                                "ComputeSquare",
                                arguments);

      logger_.LogInformation($"Result of computation : {(double)sum.Result}");

      var handler = new AdderHandler();
      for (var i = 0; i < 10; i++)
      {
        var taskId = cs.Submit("Add",
                               new object[] { (double)i, (double)i },
                               handler);

        logger_.LogInformation($"Running taskId {taskId}");
      }


      Task.WaitAll(cs.TaskWarehouse.Values.ToArray());
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
          Seconds = 300,
        },
        MaxRetries = 3,
        Priority   = 1,
      };

      taskOptions.Options.Add(AppsOptions.EngineTypeNameKey,
                              EngineType.DataSynapse.ToString());

      taskOptions.Options.Add(AppsOptions.GridAppNameKey,
                              "ArmoniK.Samples.GridServer.Client");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "1.0.0-700");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.Samples.GridServer.Client.Services");

      taskOptions.Options.Add(AppsOptions.GridServiceNameKey,
                              "ServiceContainer");

      return taskOptions;
    }


    // Handler for Service Clients
    public class AdderHandler : IServiceInvocationHandler
    {
      private readonly double _total = 0;

      public void HandleError(ServiceInvocationException e, string taskId)
      {
        Console.Out.WriteLine("Error from " + taskId + ": " + e);
      }

      public void HandleResponse(object response, string taskId)
      {
        Console.Out.WriteLine("Response from " + taskId + ": " + response);
      }

      public double getTotal()
      {
        return _total;
      }
    }
  }
}