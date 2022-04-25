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
using System.Linq;
using System.Threading.Tasks;

using ArmoniK.Api.gRPC.V1;
using ArmoniK.Extensions.Common.StreamWrapper.Client;
using ArmoniK.Samples.HtcMock.Adapter;

using Google.Protobuf;

using Htc.Mock;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.Client
{
  public class SessionClient : ISessionClient
  {
    private readonly Submitter.SubmitterClient client_;
    private readonly ILogger<GridClient>       logger_;
    private readonly string                 sessionId_;

    public SessionClient(Submitter.SubmitterClient client, string sessionId, ILogger<GridClient> logger)
    {
      client_    = client;
      logger_    = logger;
      sessionId_ = sessionId;
    }


    public void Dispose()
    {
    }

    public byte[] GetResult(string id)
    {
      using var _      = logger_.LogFunction(id);
      var resultRequest = new ResultRequest
      {
        Key     = id,
        Session = sessionId_,
      };

      var availabilityReply = client_.WaitForAvailability(resultRequest);

      switch (availabilityReply.TypeCase)
      {
        case AvailabilityReply.TypeOneofCase.None:
          throw new Exception("Issue with Server !");
        case AvailabilityReply.TypeOneofCase.Ok:
          break;
        case AvailabilityReply.TypeOneofCase.Error:
          throw new Exception($"Task in Error - {id}");
        case AvailabilityReply.TypeOneofCase.NotCompletedTask:
          throw new Exception($"Task not completed - {id}");
        default:
          throw new ArgumentOutOfRangeException();
      }

      var taskOutput = client_.TryGetTaskOutput(resultRequest);

      switch (taskOutput.TypeCase)
      {
        case Output.TypeOneofCase.None:
          throw new Exception("Issue with Server !");
        case Output.TypeOneofCase.Ok:
          break;
        case Output.TypeOneofCase.Error:
          throw new Exception($"Task in Error - {id}");
        default:
          throw new ArgumentOutOfRangeException();
      }

      var response = client_.GetResultAsync(resultRequest);
      return response.Result;
    }

    public async Task WaitSubtasksCompletion(string id)
    {
      using var _      = logger_.LogFunction(id);
      var waitForCompletion = client_.WaitForCompletion(new WaitRequest
      {
        Filter = new TaskFilter
        {
          Task = new TaskFilter.Types.IdsRequest
          {
            Ids =
            {
              sessionId_ + "%" + id,
            },
          },
        },
        StopOnFirstTaskCancellation = true,
        StopOnFirstTaskError        = true,
      });
    }

    public IEnumerable<string> SubmitTasksWithDependencies(IEnumerable<Tuple<byte[], IList<string>>> payloadsWithDependencies)
    {
      using var _         = logger_.LogFunction();
      logger_.LogDebug("payload with dependencies {len}", payloadsWithDependencies.Count());
      var taskRequests = new List<TaskRequest>();

      foreach (var (payload, dependencies) in payloadsWithDependencies)
      {
        var taskId = Guid.NewGuid().ToString();
        logger_.LogDebug("Create task {task}", taskId);
        var taskRequest = new TaskRequest
        {
          Id      = sessionId_ + "%" + taskId,
          Payload =  ByteString.CopyFrom(payload),
          DataDependencies =
          {
            // p.Item2,
            dependencies.Select(i => sessionId_ + "%" + i),
          },
          ExpectedOutputKeys =
          {
            sessionId_ + "%" + taskId,
          },
        };
        logger_.LogDebug("Dependencies : {dep}",
                         string.Join(", ",
                                     dependencies.Select(item => item.ToString())));
        taskRequests.Add(taskRequest);
      }

      var createTaskReply = client_.CreateTasksAsync(sessionId_,
                                                          null,
                                                          taskRequests).Result;
      switch (createTaskReply.DataCase)
      {
        case CreateTaskReply.DataOneofCase.NonSuccessfullIds:
          throw new Exception($"NonSuccessfullIds : {createTaskReply.NonSuccessfullIds}");
        case CreateTaskReply.DataOneofCase.None:
          throw new Exception("Issue with Server !");
        case CreateTaskReply.DataOneofCase.Successfull:
          Console.WriteLine("Task Created");
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      var taskCreated = taskRequests.Select(t => t.Id);

      logger_.LogDebug("Tasks created : {ids}",
                                   taskCreated);
      return taskCreated;
    }
  }
}