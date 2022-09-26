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

using ArmoniK.DevelopmentKit.Client.Exceptions;
using ArmoniK.DevelopmentKit.Client.Factory;
using ArmoniK.DevelopmentKit.Client.Services;
using ArmoniK.DevelopmentKit.Client.Services.Submitter;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.Client
{
  internal class LargePayloadTests
  {
    internal static void LargePayloadSubmit(Properties     properties,
                                            ILoggerFactory loggerFactory)
    {
      var logger         = loggerFactory.CreateLogger<LargePayloadTests>();
      var sessionService = ServiceFactory.CreateService(properties);

      var handler = new ResultForLargeTaskHandler(logger);

      var periodicInfo = ComputeVector(sessionService,
                                       logger,
                                       1000,
                                       64000,
                                       handler);

      sessionService.Dispose();
      periodicInfo.Dispose();
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="sessionService"></param>
    /// <param name="logger"></param>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="nbElement">The number of element n x M in the vector</param>
    /// <param name="resultForLargeTaskHandler"></param>
    private static IDisposable ComputeVector(Service                    sessionService,
                                             ILogger<LargePayloadTests> logger,
                                             int                        nbTasks,
                                             int                        nbElement,
                                             ResultForLargeTaskHandler  resultForLargeTaskHandler)
    {
      var       indexTask        = 0;
      var       prevIndex        = 0;
      const int elapsed          = 30;
      const int workloadTimeInMs = 1;

      var numbers = Enumerable.Range(0,
                                     nbElement)
                              .Select(x => (double)x)
                              .ToArray();
      logger.LogInformation($"===  Running from {nbTasks} tasks with payload by task {nbElement * 128} Ko Total : {nbTasks * nbElement / 128} Ko...   ===");
      var sw = Stopwatch.StartNew();
      var periodicInfo = Utils.PeriodicInfo(() =>
                                            {
                                              logger.LogInformation($"{indexTask}/{nbTasks} Tasks. " + $"Got {resultForLargeTaskHandler.NbResults} results. " +
                                                                    $"Check Submission perf : Payload {(indexTask - prevIndex) * nbElement * 128.0 / elapsed:0.0} Ko/s (inst), " +
                                                                    $"{(indexTask - prevIndex) / (double)elapsed:0.00} tasks/s (inst), " +
                                                                    $"{indexTask * 1000.0 / sw.ElapsedMilliseconds:0.00} task/s (avg), " +
                                                                    $"{indexTask * nbElement / 128.0 / (sw.ElapsedMilliseconds / 1000.0):0.00} Ko/s (avg)");
                                              prevIndex = indexTask;
                                            },
                                            elapsed);


      for (indexTask = 0; indexTask < nbTasks; indexTask++)
      {
        sessionService.Submit("ComputeReduceCube",
                              Utils.ParamsHelper(numbers,
                                                 workloadTimeInMs),
                              resultForLargeTaskHandler);
      }

      logger.LogInformation($"{nbTasks} tasks executed in : {sw.ElapsedMilliseconds / 1000} secs with Total bytes {nbTasks * nbElement / 128} Ko");

      return periodicInfo;
    }

    private class ResultForLargeTaskHandler : IServiceInvocationHandler
    {
      private readonly ILogger<LargePayloadTests> logger_;

      public ResultForLargeTaskHandler(ILogger<LargePayloadTests> logger)
        => logger_ = logger;

      public int NbResults { get; private set; }

      /// <summary>
      ///   The callBack method which has to be implemented to retrieve error or exception
      /// </summary>
      /// <param name="e">The exception sent to the client from the control plane</param>
      /// <param name="taskId">The task identifier which has invoke the error callBack</param>
      public void HandleError(ServiceInvocationException e,
                              string                     taskId)

      {
        if (e.StatusCode == ArmonikStatusCode.TaskCanceled)
        {
          logger_.LogWarning($"Warning from {taskId} : " + e.Message);
        }
        else
        {
          logger_.LogError($"Error from {taskId} : " + e.Message);
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
            logger_.LogInformation("Task finished but nothing returned in Result");
            break;
        }

        NbResults++;
      }
    }
  }
}
