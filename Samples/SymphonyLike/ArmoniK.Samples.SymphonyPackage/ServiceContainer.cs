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
using System.Diagnostics;
using System.Linq;
using System.Threading;

using ArmoniK.DevelopmentKit.Common;
using ArmoniK.DevelopmentKit.SymphonyApi;
using ArmoniK.DevelopmentKit.WorkerApi.Common.Exceptions;

using Armonik.Samples.Symphony.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ArmoniK.Samples.Symphony.Packages
{
  public class ServiceContainer : ServiceContainerBase
  {
    private readonly IConfiguration configuration_;

    public override void OnCreateService(ServiceContext serviceContext)
    {
      //END USER PLEASE FIXME
    }

    public override void OnSessionEnter(SessionContext sessionContext)
    {
      //END USER PLEASE FIXME
    }

    public byte[] ComputeSquare(TaskContext taskContext, ClientPayload clientPayload)
    {
      Log.LogInformation($"Enter in function : ComputeSquare with taskId {taskContext.TaskId}");

      if (clientPayload.numbers.Count == 0)
        return new ClientPayload
          {
            Type   = ClientPayload.TaskType.Result,
            result = 0,
          }
          .serialize(); // Nothing to do

      if (clientPayload.numbers.Count == 1)
      {
        var value = clientPayload.numbers[0] * clientPayload.numbers[0];
        Log.LogInformation($"Compute {value}             with taskId {taskContext.TaskId}");

        return new ClientPayload
          {
            Type   = ClientPayload.TaskType.Result,
            result = value,
          }
          .serialize();
      }
      else // if (clientPayload.numbers.Count > 1)
      {
        var value  = clientPayload.numbers[0];
        var square = value * value;

        var subTaskPaylaod = new ClientPayload();
        clientPayload.numbers.RemoveAt(0);
        subTaskPaylaod.numbers = clientPayload.numbers;
        subTaskPaylaod.Type    = clientPayload.Type;
        Log.LogInformation($"Compute {value} in                 {taskContext.TaskId}");

        Log.LogInformation($"Submitting subTask from task          : {taskContext.TaskId} from Session {SessionId.PackSessionId()}");
        var subTaskId = this.SubmitSubTask(subTaskPaylaod.serialize(),
                                           taskContext.TaskId);
        Log.LogInformation($"Submitted  subTask                    : {subTaskId}");

        ClientPayload aggPayload = new()
        {
          Type   = ClientPayload.TaskType.Aggregation,
          result = square,
        };

        Log.LogInformation($"Submitting aggregate task             : {taskContext.TaskId} from Session {SessionId.PackSessionId()}");

        var aggTaskId = this.SubmitTaskWithDependencies(aggPayload.serialize(),
                                                        new[] { subTaskId });
        Log.LogInformation($"Submitted  SubmitTaskWithDependencies : {aggTaskId} with task dependencies      {subTaskId}");

        return new ClientPayload
          {
            Type      = ClientPayload.TaskType.Aggregation,
            SubTaskId = aggTaskId,
          }
          .serialize(); //nothing to do
      }
    }

    private void _1_Job_of_N_Tasks(TaskContext taskContext, byte[] payload, int nbTasks)
    {
      var payloads = new List<byte[]>(nbTasks);
      for (var i = 0; i < nbTasks; i++)
        payloads.Add(payload);

      var sw          = Stopwatch.StartNew();
      var finalResult = 0;
      var taskIds = SubmitSubTasks(payloads,
                                   taskContext.TaskId);

      foreach (var taskId in taskIds)
      {
        var taskResult = GetResult(taskId);
        finalResult += BitConverter.ToInt32(taskResult);
      }

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      Log.LogInformation($"Server called {nbTasks} tasks in {elapsedMilliseconds} ms agregated result = {finalResult}");
    }

    public byte[] ComputeCube(TaskContext taskContext, ClientPayload clientPayload)
    {
      var value = clientPayload.numbers[0] * clientPayload.numbers[0] * clientPayload.numbers[0];
      return new ClientPayload
        {
          Type   = ClientPayload.TaskType.Result,
          result = value,
        }
        .serialize(); //nothing to do
    }

    public override byte[] OnInvoke(SessionContext sessionContext, TaskContext taskContext)
    {
      var clientPayload = ClientPayload.deserialize(taskContext.TaskInput);

      if (clientPayload.Type == ClientPayload.TaskType.ComputeSquare)
        return ComputeSquare(taskContext,
                             clientPayload);

      if (clientPayload.Type == ClientPayload.TaskType.ComputeCube)
      {
        return ComputeCube(taskContext,
                           clientPayload);
      }

      if (clientPayload.Type == ClientPayload.TaskType.Sleep)
      {
        Log.LogInformation($"Empty task, sessionId : {sessionContext.SessionId}, taskId : {taskContext.TaskId}, sessionId from task : {taskContext.SessionId}");
        Thread.Sleep(clientPayload.sleep * 1000);
      }
      else if (clientPayload.Type == ClientPayload.TaskType.JobOfNTasks)
      {
        var newPayload = new ClientPayload
        {
          Type  = ClientPayload.TaskType.Sleep,
          sleep = clientPayload.sleep,
        };

        var bytePayload = newPayload.serialize();

        _1_Job_of_N_Tasks(taskContext,
                          bytePayload,
                          clientPayload.numbers[0] - 1);

        return new ClientPayload
          {
            Type   = ClientPayload.TaskType.Result,
            result = 42,
          }
          .serialize(); //nothing to do
      }
      else if (clientPayload.Type == ClientPayload.TaskType.Aggregation)
      {
        return AggregateValues(taskContext,
                               clientPayload);
      }
      else
      {
        Log.LogInformation($"Task type is unManaged {clientPayload.Type}");
        throw new WorkerApiException($"Task type is unManaged {clientPayload.Type}");
      }

      return new ClientPayload
        {
          Type   = ClientPayload.TaskType.Result,
          result = 42,
        }
        .serialize(); //nothing to do
    }

    private byte[] AggregateValues(TaskContext taskContext, ClientPayload clientPayload)
    {
      Log.LogInformation($"Aggregate Task request result from Dependencies TaskIds : [{string.Join(", ", taskContext.DependenciesTaskIds)}]");
      var parentResult = GetResult(taskContext.DependenciesTaskIds?.Single());

      if (parentResult == null || parentResult.Length == 0)
        throw new WorkerApiException($"Cannot retrieve Result from taskId {taskContext.DependenciesTaskIds?.Single()}");

      var parentResultPayload = ClientPayload.deserialize(parentResult);
      if (parentResultPayload.SubTaskId != null)
      {
        parentResult        = GetResult(parentResultPayload.SubTaskId);
        parentResultPayload = ClientPayload.deserialize(parentResult);
      }

      var value = clientPayload.result + parentResultPayload.result;

      ClientPayload childResult = new()
      {
        Type   = ClientPayload.TaskType.Result,
        result = value,
      };

      return childResult.serialize();
    }

    public override void OnSessionLeave(SessionContext sessionContext)
    {
      //END USER PLEASE FIXME
    }

    public override void OnDestroyService(ServiceContext serviceContext)
    {
      //END USER PLEASE FIXME
    }
  }
}