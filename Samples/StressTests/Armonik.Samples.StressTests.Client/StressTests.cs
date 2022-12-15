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

using System.Diagnostics;

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

namespace Armonik.Samples.StressTests.Client
{
  internal class StressTests
  {
    public StressTests(IConfiguration configuration,
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
                      ApplicationName      = "Armonik.Samples.StressTests.Worker",
                      ApplicationNamespace = "Armonik.Samples.StressTests.Worker",
                      PartitionId          = partition,
                    };

      var props = new Properties(TaskOptions,
                             configuration.GetSection("Grpc")["EndPoint"])
              {
                MaxConcurrentBuffer = 5,
                MaxTasksPerBuffer   = 50,
                MaxParallelChannel  = 5,
                TimeTriggerBuffer   = TimeSpan.FromSeconds(10),
              };

      Logger = factory.CreateLogger<StressTests>();

      Service = ServiceFactory.CreateService(Props,
                                             factory);

      ResultHandle = new ResultForStressTestsHandler(Logger);
    }

    private ResultForStressTestsHandler ResultHandle { get; }

    public ILogger<StressTests> Logger { get; set; }

    public Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    private Service Service { get; }

    internal void LargePayloadSubmit(int  nbTasks          = 100,
                                     long nbInputBytes     = 64000,
                                     long nbOutputBytes    = 8,
                                     int  workloadTimeInMs = 1)
    {
      var periodicInfo = ComputeVector(nbTasks,
                                       nbInputBytes,
                                       nbOutputBytes,
                                       workloadTimeInMs);

      Service.Dispose();
      periodicInfo.Dispose();

      Logger.LogInformation($"Total result is {ResultHandle.Total}");
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="nbInputBytes">The number of element n x M in the vector</param>
    /// <param name="nbOutputBytes">The number of bytes to expect as result</param>
    /// <param name="workloadTimeInMs">The time spent to compute task</param>
    private IDisposable ComputeVector(int  nbTasks,
                                      long nbInputBytes,
                                      long nbOutputBytes    = 8,
                                      int  workloadTimeInMs = 1)
    {
      var       indexTask = 0;
      const int elapsed   = 30;

      var inputArrayOfBytes = Enumerable.Range(0,
                                               (int)(nbInputBytes / 8))
                                        .Select(x => Math.Pow(42.0 * 8 / nbInputBytes,
                                                              1        / 3.0))
                                        .ToArray();

      Logger.LogInformation($"===  Running from {nbTasks} tasks with payload by task {nbInputBytes / 1024.0} Ko Total : {nbTasks * nbInputBytes / 1024.0} Ko...   ===");
      var sw = Stopwatch.StartNew();
      var periodicInfo = Utils.PeriodicInfo(() =>
                                            {
                                              Logger.LogInformation($"Got {ResultHandle.NbResults} results. All tasks submitted ? {(indexTask == nbTasks).ToString()}");
                                            },
                                            elapsed);

      var result = Enumerable.Range(0,
                                    nbTasks)
                             .Chunk(nbTasks / Props.MaxParallelChannel)
                             .AsParallel()
                             .Select(subInt => subInt.Select(idx => Service.SubmitAsync("ComputeWorkLoad",
                                                                                        Utils.ParamsHelper(inputArrayOfBytes,
                                                                                                           nbOutputBytes,
                                                                                                           workloadTimeInMs),
                                                                                        ResultHandle))
                                                     .ToList());

      var taskIds = result.SelectMany(t => Task.WhenAll(t)
                                               .Result)
                          .ToHashSet();


      indexTask = taskIds.Count();

      Logger.LogInformation($"{taskIds.Count}/{nbTasks} tasks executed in : {sw.ElapsedMilliseconds / 1000.0:0.00} secs with Total bytes {nbTasks * nbInputBytes / 1024.0:0.00} Ko");

      return periodicInfo;
    }

    private class ResultForStressTestsHandler : IServiceInvocationHandler
    {
      private readonly ILogger<StressTests> Logger_;

      public ResultForStressTestsHandler(ILogger<StressTests> Logger)
        => Logger_ = Logger;

      public int    NbResults { get; private set; }
      public double Total     { get; private set; }

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
          case double[] doubles:
            Total += doubles.Sum();
            break;
          case null:
            Logger_.LogInformation("Task finished but nothing returned in Result");
            break;
        }

        NbResults++;
      }
    }
  }
}
