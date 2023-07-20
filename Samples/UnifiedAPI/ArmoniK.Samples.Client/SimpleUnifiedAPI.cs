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
  internal class SimpleUnifiedAPI : IDisposable
  {
    private readonly int workloadTimeInMs_ = 10;

    public SimpleUnifiedAPI(IConfiguration configuration,
                            int            workLoadTimeInMs,
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

      var endpoint = configuration.GetSection("Grpc")["EndPoint"];

      Props = new Properties(configuration,
                             TaskOptions);

      Logger = factory.CreateLogger<SimpleUnifiedAPI>();

      Service           = ServiceFactory.CreateService(Props);
      ResultHandle      = new ResultHandler(Logger);
      workloadTimeInMs_ = workLoadTimeInMs;
    }

    private ResultHandler ResultHandle { get; }

    public ILogger<SimpleUnifiedAPI> Logger { get; set; }

    public Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    private Service Service { get; }

    public void Dispose()
      => Service?.Dispose();


    public void SimpleExecution()
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

      Service.Submit("ComputeBasicArrayCube",
                     Common.Utils.ParamsHelper(numbers),
                     ResultHandle);

      Service.Submit("ComputeReduceCube",
                     Common.Utils.ParamsHelper(numbers,
                                               workloadTimeInMs_),
                     ResultHandle);

      Service.Submit("ComputeReduceCube",
                     Common.Utils.ParamsHelper(numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray()),
                     ResultHandle);

      Service.Submit("ComputeMadd",
                     Common.Utils.ParamsHelper(numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               4.0),
                     ResultHandle);

      Service.Submit("NonStaticComputeMadd",
                     Common.Utils.ParamsHelper(numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               numbers.SelectMany(BitConverter.GetBytes)
                                                      .ToArray(),
                                               4.0),
                     ResultHandle);
    }

    // Handler for Service Clients
    private class ResultHandler : IServiceInvocationHandler
    {
      private readonly double                    _total = 0;
      private readonly ILogger<SimpleUnifiedAPI> logger_;

      public ResultHandler(ILogger<SimpleUnifiedAPI> logger)
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
