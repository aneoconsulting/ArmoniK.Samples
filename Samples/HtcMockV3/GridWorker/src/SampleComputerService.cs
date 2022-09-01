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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extensions.Common.StreamWrapper.Worker;
using ArmoniK.Samples.HtcMock.Adapter;

using Google.Protobuf;

using Htc.Mock.Core;

using Microsoft.Extensions.Logging;

using TaskStatus = ArmoniK.Api.gRPC.V1.TaskStatus;

namespace ArmoniK.Samples.HtcMock.GridWorker
{
  public class SampleComputerService : WorkerStreamWrapper
  {
    [SuppressMessage("CodeQuality",
                     "IDE0052:Remove unread private members",
                     Justification = "Used for side effects")]
    private readonly ApplicationLifeTimeManager applicationLifeTime_;

    private readonly ILogger<SampleComputerService> logger_;
    private readonly ILoggerFactory                 loggerFactory_;

    public SampleComputerService(ILoggerFactory             loggerFactory,
                                 ApplicationLifeTimeManager applicationLifeTime)
      : base(loggerFactory)
    {
      logger_              = loggerFactory.CreateLogger<SampleComputerService>();
      loggerFactory_       = loggerFactory;
      applicationLifeTime_ = applicationLifeTime;
    }

    public override async Task<Output> Process(ITaskHandler taskHandler)
    {
      using var scopedLog = logger_.BeginNamedScope("Execute task",
                                                    ("Session", taskHandler.SessionId),
                                                    ("taskId", taskHandler.TaskId));
      logger_.LogTrace("DataDependencies {DataDependencies}",
                       taskHandler.DataDependencies.Keys);
      logger_.LogTrace("ExpectedResults {ExpectedResults}",
                       taskHandler.ExpectedResults);

      var output = new Output();
      try
      {
        var (runConfiguration, request) = DataAdapter.ReadPayload(taskHandler.Payload);

        var inputs = request.Dependencies.ToDictionary(id => id,
                                                       id =>
                                                       {
                                                         logger_.LogInformation("Looking for result for Id {id}",
                                                                                id);
                                                         var armonik_id = taskHandler.SessionId + "%" + id;
                                                         var isOkay = taskHandler.DataDependencies.TryGetValue(armonik_id,
                                                                                                               out var data);
                                                         if (!isOkay)
                                                         {
                                                           throw new KeyNotFoundException(armonik_id);
                                                         }

                                                         return Encoding.Default.GetString(data);
                                                       });
        logger_.LogDebug("Inputs {input}",
                         inputs);

        var requestProcessor = new RequestProcessor(true,
                                                    true,
                                                    true,
                                                    runConfiguration,
                                                    logger_);
        var res = requestProcessor.GetResult(request,
                                             inputs);
        logger_.LogDebug("Result for processing request is HasResult={hasResult}, Value={value}",
                         res.Result.HasResult,
                         res.Result.Value);

        if (res.Result.HasResult)
        {
          await taskHandler.SendResult(taskHandler.ExpectedResults.Single(),
                                       Encoding.Default.GetBytes(res.Result.Value));
        }
        else
        {
          var requests = res.SubRequests.GroupBy(r => r.Dependencies is null || r.Dependencies.Count == 0)
                            .ToDictionary(g => g.Key,
                                          g => g);
          logger_.LogDebug("Will submit {count} new tasks",
                           requests[true]
                             .Count());
          var readyRequests = requests[true];
          await taskHandler.CreateTasksAsync(readyRequests.Select(r =>
                                                                  {
                                                                    var taskId = taskHandler.SessionId + "%" + r.Id;
                                                                    logger_.LogDebug("Create task {task}",
                                                                                     taskId);
                                                                    return new TaskRequest
                                                                           {
                                                                             Id = taskId,
                                                                             Payload = ByteString.CopyFrom(DataAdapter.BuildPayload(runConfiguration,
                                                                                                                                    r)),
                                                                             DataDependencies =
                                                                             {
                                                                               r.Dependencies.Select(s => taskHandler.SessionId + "%" + s),
                                                                             },
                                                                             ExpectedOutputKeys =
                                                                             {
                                                                               taskId,
                                                                             },
                                                                           };
                                                                  }));
          // code à adapter pour créer le bon type de request
          //sessionClient.SubmitTasks(readyRequests.Select(r => DataAdapter.BuildPayload(runConfiguration, r)));
          var req = requests[false]
            .Single();
          await taskHandler.CreateTasksAsync(new[]
                                             {
                                               new TaskRequest
                                               {
                                                 Id = taskHandler.SessionId + "%" + req.Id,
                                                 Payload = ByteString.CopyFrom(DataAdapter.BuildPayload(runConfiguration,
                                                                                                        req)),
                                                 DataDependencies =
                                                 {
                                                   req.Dependencies.Select(s => taskHandler.SessionId + "%" + s),
                                                 },
                                                 ExpectedOutputKeys =
                                                 {
                                                   taskHandler.TaskId,
                                                 },
                                               },
                                             });
          // code à adapter pour créer le bon type de request
          // ici, le ExpectedDependency doit être celui de la tâche en cours
          //sessionClient.SubmitTaskWithDependencies(readyRequests.Select(r => DataAdapter.BuildPayload(runConfiguration, req), req.Dependencies));
        }

        output = new Output
                 {
                   Ok     = new Empty(),
                   Status = TaskStatus.Completed,
                 };
      }
      catch (Exception ex)
      {
        logger_.LogError(ex,
                         "Error while computing task");

        output = new Output
                 {
                   Error = new Output.Types.Error
                           {
                             Details      = ex.Message + ex.StackTrace,
                             KillSubTasks = true,
                           },
                   Status = TaskStatus.Error,
                 };
      }

      return output;
    }
  }
}
