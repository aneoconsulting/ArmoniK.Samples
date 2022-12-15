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
using System.Diagnostics;
using System.Linq;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;
using ArmoniK.DevelopmentKit.Client.Unified.Factory;
using ArmoniK.DevelopmentKit.Client.Unified.Services.Submitter;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.Samples.Common;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.Client
{
  internal class NSubmitTest : IDisposable
  {
    public NSubmitTest(IConfiguration configuration,
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

      Logger = factory.CreateLogger<NSubmitTest>();

      Service      = ServiceFactory.CreateService(Props);
      ResultHandle = new ResultHandler(Logger);
    }

    private ResultHandler ResultHandle { get; }

    public ILogger<NSubmitTest> Logger { get; set; }

    public Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    private Service Service { get; }

    public void Dispose()
      => Service?.Dispose();


    public void BatchExecution(int nbTask)
    {
      var toSum = new List<double>
                  {
                    10,
                    14,
                    18,
                  }.ToArray();
      Logger.LogInformation("Execute {nTask} in one batch",
                            nbTask);
      var sw = Stopwatch.StartNew();
      var periodicInfo = Utils.PeriodicInfo(() =>
                                            {
                                              Logger.LogInformation($"Got {ResultHandle.NbResults} results. " +
                                                                    $"{ResultHandle.NbResults / (sw.ElapsedMilliseconds / 1000.0):0.00} Result/s (avg)");
                                            },
                                            30);
      var tasks = Service.Submit("ComputeReduce",
                                 Enumerable.Range(1,
                                                  nbTask)
                                           .Select(_ => Utils.ParamsHelper(toSum)),
                                 ResultHandle);
      if (tasks.Count() is var count && count != nbTask)
      {
        throw new ApplicationException($"Expected {nbTask} submitted tasks, got {count}");
      }

      Logger.LogInformation($"Number of requested task {nbTask}, Submitted tasks {tasks.Count()} in {sw.ElapsedMilliseconds / 1000.0:0.00} seconds");

      Service.Dispose();
      periodicInfo.Dispose();
    }

    // Handler for Service Clients
    private class ResultHandler : IServiceInvocationHandler
    {
      private readonly ILogger<NSubmitTest> logger_;

      public ResultHandler(ILogger<NSubmitTest> logger)
        => logger_ = logger;

      public int NbResults { get; set; }


      public void HandleError(ServiceInvocationException e,
                              string                     taskId)
      {
        logger_.LogError($"Error from {taskId} : " + e.Message);
        NbResults++;
        throw new ApplicationException($"Error from {taskId}",
                                       e);
      }

      public void HandleResponse(object response,
                                 string taskId)
      {
        switch (response)
        {
          case null:
            logger_.LogWarning("Task finished but nothing returned in Result");
            break;
          case double value:
            logger_.LogDebug($"Task finished with result {value}");
            break;
          case double[] doubles:
            logger_.LogDebug("Result is " + string.Join(", ",
                                                        doubles));
            break;
          case byte[] values:
            logger_.LogDebug("Result is " + string.Join(", ",
                                                        values.ConvertToArray()));
            break;
        }

        NbResults++;
      }
    }
  }
}
