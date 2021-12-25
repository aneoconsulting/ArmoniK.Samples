// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2021. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ArmoniK.Core.gRPC.V1;

using Google.Protobuf;

using Htc.Mock;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.Adapter
{
  public class SessionClient : ISessionClient
  {
    private readonly ClientService.ClientServiceClient client_;
    private readonly ILogger<GridClient>               logger_;
    private readonly SessionId                         sessionId_;

    public SessionClient(ClientService.ClientServiceClient client, SessionId sessionId, ILogger<GridClient> logger)
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
      var       taskId = id.ToTaskId();
      var taskFilter = new TaskFilter
      {
        IncludedTaskIds =
        {
          taskId.Task,
        },
        SessionId    = taskId.Session,
        SubSessionId = taskId.SubSession,
      };
      var response = client_.TryGetResult(taskFilter);
      var bytes    = response.Payloads.Single().Data.Data.ToByteArray();
      logger_.LogDebug("Result : {res}",
                       Convert.ToBase64String(bytes));
      return bytes;
    }

    public async Task WaitSubtasksCompletion(string id)
    {
      using var _      = logger_.LogFunction(id);
      var       taskId = id.ToTaskId();
      var taskFilter = new TaskFilter
      {
        SessionId = taskId.Session,
        IncludedTaskIds =
        {
          taskId.Task,
        },
      };
      await client_.WaitForSubTasksCompletionAsync(new WaitRequest
      {
        Filter                  = taskFilter,
        ThrowOnTaskCancellation = true,
        ThrowOnTaskError        = true,
      });
    }

    public IEnumerable<string> SubmitTasksWithDependencies(IEnumerable<Tuple<byte[], IList<string>>> payloadsWithDependencies)
    {
      using var _ = logger_.LogFunction();
      var taskRequests = payloadsWithDependencies.Select(p =>
      {
        var output = new TaskRequest
        {
          Payload = new Payload
          {
            Data = ByteString.CopyFrom(p.Item1),
          },
          DependenciesTaskIds =
          {
            p.Item2.Select(i => i.ToTaskId().Task),
          },
        };
        logger_.LogDebug("Dependencies : {dep}",
                         string.Join(", ",
                                     p.Item2.Select(item => item.ToString())));
        return output;
      });
      var createTaskRequest = new CreateTaskRequest
      {
        SessionId = sessionId_,
      };
      createTaskRequest.TaskRequests.Add(taskRequests);
      var createTaskReply = client_.CreateTask(createTaskRequest);
      logger_.LogDebug("Tasks created : {ids}",
                       string.Join(", ",
                                   createTaskReply.TaskIds.Select(item => item.ToHtcMockId())));
      return createTaskReply.TaskIds.Select(id => id.ToHtcMockId());
    }
  }
}