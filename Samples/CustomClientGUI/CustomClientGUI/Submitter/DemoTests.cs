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
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.DevelopmentKit.Client.Common;
using ArmoniK.DevelopmentKit.Client.Common.Exceptions;
using ArmoniK.DevelopmentKit.Client.Unified.Factory;
using ArmoniK.DevelopmentKit.Client.Unified.Services.Submitter;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.DevelopmentKit.Common.Extensions;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomClientGUI.Submitter
{
  public class DemoTests
  {
    public DemoTests(IConfiguration              configuration,
                     string                      address,
                     ILoggerFactory              factory,
                     ResultForStressTestsHandler responseHandler,
                     string                      partition)
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

      Props = new ArmoniK.DevelopmentKit.Client.Common.Properties(TaskOptions,
                                                                  address)
              {
                MaxConcurrentBuffers = 5,
                MaxTasksPerBuffer    = 50,
                MaxParallelChannels  = 5,
                TimeTriggerBuffer    = TimeSpan.FromSeconds(5),
              };

      Logger = factory.CreateLogger<DemoTests>();

      Service = ServiceFactory.CreateService(Props,
                                             factory);

      ResultHandle = responseHandler;
    }

    private ResultForStressTestsHandler ResultHandle { get; }

    public ILogger<DemoTests> Logger { get; set; }

    public ArmoniK.DevelopmentKit.Client.Common.Properties Props { get; set; }

    public TaskOptions TaskOptions { get; set; }

    public Service Service { get; }

    public List<Task<string>> LargePayloadSubmit(int  nbTasks          = 100,
                                                 long nbInputBytes     = 64000,
                                                 long nbOutputBytes    = 8,
                                                 int  workloadTimeInMs = 1)
    {
      var pTaskIdsList = ComputeVector(nbTasks,
                                       nbInputBytes,
                                       nbOutputBytes,
                                       workloadTimeInMs);

      Logger.LogInformation($"Total result is {ResultHandle.Total}");

      return pTaskIdsList;
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="nbTasks">The number of task to submit</param>
    /// <param name="nbInputBytes">The number of element n x M in the vector</param>
    /// <param name="nbOutputBytes">The number of bytes to expect as result</param>
    /// <param name="workloadTimeInMs">The time spent to compute task</param>
    private List<Task<string>> ComputeVector(int  nbTasks,
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
      //var periodicInfo = Utils.PeriodicInfo(() =>
      //                                      {
      //                                        Logger.LogInformation($"Got {ResultHandle.NbResults} results. All tasks submitted ? {(indexTask == nbTasks).ToString()}");
      //                                      },
      //                                      elapsed);

      var result = Enumerable.Range(0,
                                    nbTasks)
                             .Select(idx => Service.SubmitAsync("ComputeWorkLoad",
                                                                Utils.ParamsHelper(inputArrayOfBytes,
                                                                                   nbOutputBytes,
                                                                                   workloadTimeInMs),
                                                                ResultHandle))
                             .ToList();
      return result;
    }
  }
}
