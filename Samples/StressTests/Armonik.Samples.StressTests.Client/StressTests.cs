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

using System.Diagnostics;
using System.Text;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;
using ArmoniK.DevelopmentKit.Client.Common.Status;
using ArmoniK.DevelopmentKit.Client.Common.Submitter;
using ArmoniK.DevelopmentKit.Client.Unified.Factory;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.Samples.Common;

using Armonik.Samples.StressTests.Client.Metrics;

using ArmoniK.Utils;

using Google.Protobuf.WellKnownTypes;

using Grpc.Net.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Armonik.Samples.StressTests.Client
{
  internal class StressTests
  {
    private readonly ObjectPool<GrpcChannel>? channelPool_;

    public StressTests(IConfiguration configuration,
                       ILoggerFactory factory,
                       string         partition,
                       int            nbTaskPerBufferValue,
                       int            nbBufferPerChannelValue,
                       int            nbChannel)
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

      Props = new Properties(configuration,
                             TaskOptions)
              {
                MaxConcurrentBuffers = nbBufferPerChannelValue,
                MaxTasksPerBuffer    = nbTaskPerBufferValue,
                MaxParallelChannels  = nbChannel,
                TimeTriggerBuffer    = TimeSpan.FromSeconds(10),
              };


      Logger = factory.CreateLogger<StressTests>();

      channelPool_ = ClientServiceConnector.ControlPlaneConnectionPool(Props,
                                                                       factory);

      Service = ServiceFactory.CreateService(Props,
                                             factory);

      ResultHandle = new ResultForStressTestsHandler(Logger);
    }

    private ResultForStressTestsHandler ResultHandle { get; }

    public ILogger<StressTests> Logger { get; set; }

    public Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    private ISubmitterService Service { get; }

    internal void LargePayloadSubmit(int    nbTasks          = 100,
                                     long   nbInputBytes     = 64000,
                                     long   nbOutputBytes    = 8,
                                     int    workloadTimeInMs = 1,
                                     string jsonPath         = "")
    {
      var inputArrayOfBytes = Enumerable.Range(0,
                                               (int)(nbInputBytes / 8))
                                        .Select(x => Math.Pow(42.0 * 8 / nbInputBytes,
                                                              1        / 3.0))
                                        .ToArray();

      Logger.LogInformation($"===  Running from {nbTasks} tasks with payload by task {nbInputBytes / 1024.0} KB Total : {nbTasks * nbInputBytes / 1024.0} KB...   ===");
      var sw = Stopwatch.StartNew();
      var dt = DateTime.Now;

      var periodicInfo = ComputeVector(nbTasks,
                                       inputArrayOfBytes,
                                       nbOutputBytes,
                                       workloadTimeInMs);
      Logger.LogInformation($"{nbTasks}/{nbTasks} tasks Submitted in : {sw.ElapsedMilliseconds / 1000.0:0.00} secs with Total bytes {nbTasks * nbInputBytes / 1024.0:0.00} KB");
      ResultHandle.WaitForResult(nbTasks,
                                 new CancellationToken())
                  .Wait();
      periodicInfo.Dispose();
      var sb = new StringBuilder();

      var stats = new TasksStats(nbTasks,
                                 nbInputBytes,
                                 nbOutputBytes,
                                 workloadTimeInMs,
                                 Props);

      using var channel = channelPool_.Get();
      stats.GetAllStats(channel,
                        Service.SessionId,
                        dt,
                        DateTime.Now)
           .Wait();

      if (!string.IsNullOrEmpty(jsonPath))
      {
        stats.PrintToJson(jsonPath)
             .Wait();
      }

      Logger.LogInformation(stats.PrintToText()
                                 .Result);


      Service.Dispose();
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="nbInputBytes">The number of element n x M in the vector</param>
    /// <param name="nbOutputBytes">The number of bytes to expect as result</param>
    /// <param name="workloadTimeInMs">The time spent to compute task</param>
    private IDisposable ComputeVector(int      nbTasks,
                                      double[] inputArrayOfBytes,
                                      long     nbOutputBytes    = 8,
                                      int      workloadTimeInMs = 1)
    {
      var       indexTask = 0;
      const int elapsed   = 30;


      var periodicInfo = Utils.PeriodicInfo(() =>
                                            {
                                              Logger.LogInformation($"Got {ResultHandle.NbResults} results. All tasks submitted ? {(indexTask == nbTasks).ToString()}");
                                            },
                                            elapsed);

      var result = Enumerable.Range(0,
                                    nbTasks)
                             .Chunk(nbTasks / Props.MaxParallelChannels)
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


      return periodicInfo;
    }


    private class ResultForStressTestsHandler : IServiceInvocationHandler
    {
      private readonly ILogger<StressTests> Logger_;

      public ResultForStressTestsHandler(ILogger<StressTests> Logger)
        => Logger_ = Logger;

      public int    NbResults { get; private set; }
      public int    NbErrors  { get; private set; }
      public double Total     { get; private set; }

      /// <summary>
      ///   The callBack method which has to be implemented to retrieve error or exception
      /// </summary>
      /// <param name="e">The exception sent to the client from the control plane</param>
      /// <param name="taskId">The task identifier which has invoke the error callBack</param>
      public void HandleError(ServiceInvocationException e,
                              string                     taskId)

      {
        NbErrors++;

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
          case double[] doubles:
            Total += doubles.Sum();
            break;
          case null:
            Logger_.LogInformation("Task finished but nothing returned in Result");
            break;
        }

        NbResults++;
      }

      public async Task WaitForResult(int               nbTasks,
                                      CancellationToken token)
      {
        while (NbResults + NbErrors < nbTasks && !token.IsCancellationRequested)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(100),
                           token);
        }
      }
    }
  }
}
