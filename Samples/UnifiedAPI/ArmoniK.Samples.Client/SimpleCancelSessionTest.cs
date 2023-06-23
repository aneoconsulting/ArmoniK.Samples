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
using System.Linq;
using System.Threading;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;
using ArmoniK.DevelopmentKit.Client.Unified.Factory;
using ArmoniK.DevelopmentKit.Client.Unified.Services.Admin;
using ArmoniK.DevelopmentKit.Client.Unified.Services.Submitter;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.Samples.Common;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.Client
{
  internal class SimpleCancelSessionTest
  {
    public SimpleCancelSessionTest(IConfiguration configuration,
                                   ILoggerFactory factory)
    {
      TaskOptions = new TaskOptions
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

      Props = new Properties(TaskOptions,
                             configuration.GetSection("Grpc")["EndPoint"]);

      Logger = factory.CreateLogger<SimpleCancelSessionTest>();

      Service = ServiceFactory.CreateService(Props,
                                             factory);
      ServiceAdmin = ServiceFactory.GetServiceAdmin(Props,
                                                    factory);
      ResultHandle = new ResultHandler(Logger);
    }

    public ServiceAdmin ServiceAdmin { get; set; }

    private ResultHandler ResultHandle { get; }

    public ILogger<SimpleCancelSessionTest> Logger { get; set; }

    public Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    private Service Service { get; }


    /// <summary>
    ///   The first test developed to validate the Session cancellation
    /// </summary>
    private void RunningAndCancelSession(int nbTask = 100)
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

      var tasks = Service.Submit("ComputeBasicArrayCube",
                                 Enumerable.Range(1,
                                                  nbTask)
                                           .Select(_ => Common.Utils.ParamsHelper(numbers)),
                                 ResultHandle);
      if (tasks.Count() is var count && count != nbTask)
      {
        throw new ApplicationException($"Expected {nbTask} submitted tasks, got {count}");
      }

      //Get the count of running tasks after 10 s
      Thread.Sleep(15000);

      var countRunningTasks = ServiceAdmin.AdminMonitoringService.CountCompletedTasksBySession(Service.SessionId);
      Logger.LogInformation($"Number of completed tasks after 15 seconds is {countRunningTasks}");

      //Cancel all the session
      Logger.LogInformation("Cancel the whole session");
      ServiceAdmin.AdminMonitoringService.CancelSession(Service.SessionId);

      //Get the count of running tasks after 10 s
      Thread.Sleep(10000);
      //Cancel all the session
      var countCancelTasks = ServiceAdmin.AdminMonitoringService.CountCancelTasksBySession(Service.SessionId);
      Logger.LogInformation($"Number of canceled tasks after Session cancel is {countCancelTasks}");

      countRunningTasks = ServiceAdmin.AdminMonitoringService.CountCompletedTasksBySession(Service.SessionId);
      Logger.LogInformation($"Number of running tasks after Session cancel is {countRunningTasks}");


      var countErrorTasks = ServiceAdmin.AdminMonitoringService.CountErrorTasksBySession(Service.SessionId);
      Logger.LogInformation($"Number of error tasks after Session cancel is {countErrorTasks}");
    }

    // Handler for Service Clients
    private class ResultHandler : IServiceInvocationHandler
    {
      private readonly ILogger<SimpleCancelSessionTest> logger_;

      public ResultHandler(ILogger<SimpleCancelSessionTest> logger)
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
