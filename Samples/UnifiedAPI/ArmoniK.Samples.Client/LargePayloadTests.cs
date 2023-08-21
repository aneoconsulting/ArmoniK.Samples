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
using System.Diagnostics;
using System.Linq;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;
using ArmoniK.DevelopmentKit.Client.Common.Status;
using ArmoniK.DevelopmentKit.Client.Common.Submitter;
using ArmoniK.DevelopmentKit.Client.Unified.Factory;
using ArmoniK.DevelopmentKit.Client.Unified.Services.Admin;
using ArmoniK.DevelopmentKit.Client.Unified.Services.Submitter;
using ArmoniK.DevelopmentKit.Common;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.Client
{
  internal class LargePayloadTests
  {
    public LargePayloadTests(IConfiguration configuration,
                             ILoggerFactory factory,
                             string         partition)
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
                      PartitionId          = partition,
                    };

      Props = new Properties(configuration,
                             TaskOptions);

      Logger = factory.CreateLogger<LargePayloadTests>();

      Service = ServiceFactory.CreateService(Props,
                                             factory);
      ServiceAdmin = ServiceFactory.GetServiceAdmin(Props,
                                                    factory);
      ResultHandle = new ResultForLargeTaskHandler(Logger);
    }

    public ServiceAdmin ServiceAdmin { get; set; }

    private ResultForLargeTaskHandler ResultHandle { get; }

    public ILogger<LargePayloadTests> Logger { get; set; }

    public Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    private Service Service { get; }

    internal void LargePayloadSubmit(long nbTasks          = 100,
                                     int  nbElement        = 64000,
                                     int  workloadTimeInMs = 1)
    {
      var periodicInfo = ComputeVector(nbTasks,
                                       nbElement,
                                       workloadTimeInMs);

      Service.Dispose();
      periodicInfo.Dispose();
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="nbElement">The number of element n x M in the vector</param>
    private IDisposable ComputeVector(long nbTasks,
                                      int  nbElement,
                                      int  workloadTimeInMs = 1)
    {
      var       indexTask = 0;
      var       prevIndex = 0;
      const int elapsed   = 30;

      var numbers = Enumerable.Range(0,
                                     nbElement)
                              .Select(x => (double)x)
                              .ToArray();
      Logger.LogInformation($"===  Running from {nbTasks} tasks with payload by task {nbElement / 128} Ko Total : {nbTasks * nbElement / 128} Ko...   ===");
      var sw = Stopwatch.StartNew();
      var periodicInfo = Common.Utils.PeriodicInfo(() =>
                                                   {
                                                     Logger.LogInformation($"{indexTask}/{nbTasks} Tasks. " + $"Got {ResultHandle.NbResults} results. " +
                                                                           $"Check Submission perf : Payload {(indexTask - prevIndex) * nbElement / 128.0 / elapsed:0.0} Ko/s (inst), " +
                                                                           $"{(indexTask - prevIndex) / (double)elapsed:0.00} tasks/s (inst), " +
                                                                           $"{indexTask * 1000.0 / sw.ElapsedMilliseconds:0.00} task/s (avg), " +
                                                                           $"{indexTask * nbElement / 128.0 / (sw.ElapsedMilliseconds / 1000.0):0.00} Ko/s (avg)");
                                                     prevIndex = indexTask;
                                                   },
                                                   elapsed);


      for (indexTask = 0; indexTask < nbTasks; indexTask++)
      {
        Service.Submit("ComputeReduceCube",
                       Common.Utils.ParamsHelper(numbers,
                                                 workloadTimeInMs),
                       ResultHandle);
      }

      Logger.LogInformation($"{nbTasks} tasks executed in : {sw.ElapsedMilliseconds / 1000} secs with Total bytes {nbTasks * nbElement / 128} Ko");

      return periodicInfo;
    }

    private class ResultForLargeTaskHandler : IServiceInvocationHandler
    {
      private readonly ILogger<LargePayloadTests> Logger_;

      public ResultForLargeTaskHandler(ILogger<LargePayloadTests> Logger)
        => Logger_ = Logger;

      public int NbResults { get; private set; }

      /// <summary>
      ///   The callBack method which has to be implemented to retrieve error or exception
      /// </summary>
      /// <param name="e">The exception sent to the client from the control plane</param>
      /// <param name="taskId">The task identifier which has invoke the error callBack</param>
      public void HandleError(ServiceInvocationException e,
                              string                     taskId)

      {
        if (e.StatusCode == ArmonikStatusCode.TaskCancelled)
        {
          Logger_.LogWarning($"Warning from {taskId} : " + e.Message);
        }
        else
        {
          Logger_.LogError($"Error from {taskId} : " + e.Message);
          throw new ApplicationException($"Error from {taskId}",
                                         e);
        }
      }

      /// <summary>
      ///   The callBack method which has to be implemented to retrieve response from the server
      /// </summary>
      /// <param name="response">The object receive from the server as result the method called by the client</param>
      /// <param name="taskId">The task identifier which has invoke the response callBack</param>
      public void HandleResponse(object response,
                                 string taskId)

      {
        switch (response)
        {
          case null:
            Logger_.LogInformation("Task finished but nothing returned in Result");
            break;
        }

        NbResults++;
      }
    }
  }
}
