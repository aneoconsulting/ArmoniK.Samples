// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2022. All rights reserved.
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
using System.Reactive.Disposables;

using ArmoniK.Core.gRPC.V1;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Htc.Mock;

using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.Adapter
{
  public class GridClient : IGridClient
  {
    private readonly ClientService.ClientServiceClient client_;
    private readonly ILogger<GridClient>               logger_;

    public GridClient(ClientService.ClientServiceClient client, ILogger<GridClient> logger)
    {
      client_ = client;
      logger_ = logger;
    }

    public SessionId SessionId { get; private set; }
    public SessionId SubSessionId { get; private set; }

    public byte[] GetResult(string id)
    {
      using var _          = logger_.LogFunction();
      var       taskFilter = new TaskFilter();
      taskFilter.IncludedTaskIds.Add(id);
      taskFilter.SessionId    = SessionId.Session;
      taskFilter.SubSessionId = SessionId.SubSession;
      var response = client_.TryGetResult(taskFilter);
      return response.Payloads.Single().Data.Data.ToByteArray();
    }

    public void WaitCompletion(string id)
    {
      using var _          = logger_.LogFunction();
      var       taskFilter = new TaskFilter();
      taskFilter.IncludedTaskIds.Add(id);
      taskFilter.SessionId    = SessionId.Session;
      taskFilter.SubSessionId = SessionId.SubSession;
      client_.WaitForCompletion(new()
      {
        Filter                  = taskFilter,
        ThrowOnTaskCancellation = true,
        ThrowOnTaskError        = true,
      });
    }

    public void WaitSubtasksCompletion(string id)
    {
      using var _ = logger_.LogFunction();
      var taskFilter = new TaskFilter
      {
        SessionId    = SessionId.Session,
        SubSessionId = SubSessionId is null ? SessionId.SubSession : SubSessionId.Session,
      };
      taskFilter.IncludedTaskIds.Add(id);
      var count = client_.WaitForSubTasksCompletion(new()
      {
        Filter                  = taskFilter,
        ThrowOnTaskCancellation = true,
        ThrowOnTaskError        = true,
      });
      logger_.LogDebug(string.Join(", ",
                                   count.Values.Select(p => p.ToString())));
    }

    public IEnumerable<string> SubmitTasks(string session, IEnumerable<byte[]> payloads)
    {
      using var _ = logger_.LogFunction();
      var taskRequests = payloads.Select(p => new TaskRequest
      {
        Payload = new Payload
        {
          Data = ByteString.CopyFrom(p),
        },
      });
      var createTaskRequest = new CreateTaskRequest
      {
        SessionId = SessionId,
      };
      createTaskRequest.TaskRequests.Add(taskRequests);
      var createTaskReply = client_.CreateTask(createTaskRequest);
      return createTaskReply.TaskIds.Select(id => id.Task);
    }

    public IEnumerable<string> SubmitSubtasks(string session, string parentId, IEnumerable<byte[]> payloads)
    {
      using var _ = logger_.LogFunction();
      if (SubSessionId is null)
        CreateSubSession(parentId);
      var taskRequests = payloads.Select(p => new TaskRequest
      {
        Payload = new Payload
        {
          Data = ByteString.CopyFrom(p),
        },
      });
      var createTaskRequest = new CreateTaskRequest
      {
        SessionId = SubSessionId,
      };
      createTaskRequest.TaskRequests.Add(taskRequests);
      var createTaskReply = client_.CreateTask(createTaskRequest);
      return createTaskReply.TaskIds.Select(id => id.Task);
    }

    public string SubmitTaskWithDependencies(string session, byte[] payload, IList<string> dependencies)
    {
      return SubmitTaskWithDependencies(session,
                                        new[]
                                        {
                                          Tuple.Create(payload,
                                                       dependencies),
                                        }).Single();
    }

    public IEnumerable<string> SubmitTaskWithDependencies(string session, IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies)
    {
      using var _ = logger_.LogFunction();
      var taskRequests = payloadWithDependencies.Select(p =>
      {
        var output = new TaskRequest
        {
          Payload = new Payload
          {
            Data = ByteString.CopyFrom(p.Item1),
          },
        };
        output.DependenciesTaskIds.Add(p.Item2);
        logger_.LogDebug("Dependencies : {dep}",
                         string.Join(", ",
                                     p.Item2.Select(item => item.ToString())));
        return output;
      });
      var createTaskRequest = new CreateTaskRequest
      {
        SessionId = SessionId,
      };
      createTaskRequest.TaskRequests.Add(taskRequests);
      var createTaskReply = client_.CreateTask(createTaskRequest);
      logger_.LogDebug("Tasks created : {ids}",
                       string.Join(", ",
                                   createTaskReply.TaskIds.Select(item => item.Task)));
      return createTaskReply.TaskIds.Select(id => id.Task);
    }

    public string SubmitSubtaskWithDependencies(string session, string parentId, byte[] payload, IList<string> dependencies)
    {
      return SubmitSubtaskWithDependencies(session,
                                           parentId,
                                           new[]
                                           {
                                             Tuple.Create(payload,
                                                          dependencies),
                                           }).Single();
    }

    public IEnumerable<string> SubmitSubtaskWithDependencies(string session, string parentId, IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies)
    {
      using var _ = logger_.LogFunction();
      if (SubSessionId is null)
        CreateSubSession(parentId);
      var taskRequests = payloadWithDependencies.Select(p =>
      {
        var output = new TaskRequest
        {
          Payload = new Payload
          {
            Data = ByteString.CopyFrom(p.Item1),
          },
        };
        output.DependenciesTaskIds.Add(p.Item2);
        logger_.LogDebug("Dependencies : {dep}",
                         string.Join(", ",
                                     p.Item2.Select(item => item.ToString())));
        return output;
      });
      var createTaskRequest = new CreateTaskRequest
      {
        SessionId = SubSessionId,
      };
      createTaskRequest.TaskRequests.Add(taskRequests);
      var createTaskReply = client_.CreateTask(createTaskRequest);
      logger_.LogDebug("Tasks created : {ids}",
                       string.Join(", ",
                                   createTaskReply.TaskIds.Select(item => item.Task)));
      return createTaskReply.TaskIds.Select(id => id.Task);
    }

    public string CreateSession()
    {
      using var _ = logger_.LogFunction();
      var sessionOptions = new SessionOptions
      {
        DefaultTaskOption = new TaskOptions
        {
          MaxDuration = Duration.FromTimeSpan(TimeSpan.FromMinutes(20)),
          MaxRetries  = 2,
          Priority    = 1,
        },
      };
      SessionId = client_.CreateSession(sessionOptions);
      return SessionId.ToHtcMockId();
    }


    public IDisposable OpenSession(string session)
    {
      using var _ = logger_.LogFunction();
      SessionId = session.ToArmoniKId();
      return Disposable.Create(() => { SubSessionId = null; });
    }

    public void CancelSession(string session)
    {
      using var _ = logger_.LogFunction();
      client_.CancelSession(session.ToArmoniKId());
    }

    public void CancelTask(string taskId)
    {
      using var _          = logger_.LogFunction();
      var       taskFilter = new TaskFilter();
      taskFilter.IncludedTaskIds.Add(taskId);
      client_.CancelTask(taskFilter);
    }

    public void CreateSubSession(string parentId)
    {
      using var _ = logger_.LogFunction(parentId);
      lock (this)
      {
        if (SubSessionId is null)
        {
          var sessionOptions = new SessionOptions
          {
            DefaultTaskOption = new TaskOptions
            {
              MaxDuration = Duration.FromTimeSpan(TimeSpan.FromMinutes(20)),
              MaxRetries  = 2,
              Priority    = 1,
            },
            ParentTask = new TaskId
            {
              Session    = SessionId.Session,
              SubSession = SessionId.SubSession,
              Task       = parentId,
            },
          };
          SubSessionId = client_.CreateSession(sessionOptions);
        }
      }
    }
  }
}