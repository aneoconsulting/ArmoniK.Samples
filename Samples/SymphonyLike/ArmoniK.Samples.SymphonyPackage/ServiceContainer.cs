using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Threading;
using ArmoniK.DevelopmentKit.Common;
using ArmoniK.DevelopmentKit.SymphonyApi;
using ArmoniK.DevelopmentKit.WorkerApi.Common.Exceptions;
using Armonik.Samples.Symphony.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ArmoniK.Samples.Symphony.Packages
{
    public class ServiceContainer : ServiceContainerBase
    {
        private readonly IConfiguration configuration_;

        public ServiceContainer()
        {
           
        }

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
                return (new ClientPayload() { Type = ClientPayload.TaskType.Result, result = 0 })
                    .serialize(); // Nothing to do

            if (clientPayload.numbers.Count == 1)
            {
                int value = clientPayload.numbers[0] * clientPayload.numbers[0];
                Log.LogInformation($"Compute {value}             with taskId {taskContext.TaskId}");
                
                return (new ClientPayload() { Type = ClientPayload.TaskType.Result, result = value })
                    .serialize();
            }
            else // if (clientPayload.numbers.Count > 1)
            {
                int value = clientPayload.numbers[0];
                int square = value * value;

                var subTaskPaylaod = new ClientPayload();
                clientPayload.numbers.RemoveAt(0);
                subTaskPaylaod.numbers = clientPayload.numbers;
                subTaskPaylaod.Type = clientPayload.Type;
                Log.LogInformation($"Compute {value} in                 {taskContext.TaskId}");

                Log.LogInformation($"Submitting subTask from task          : {taskContext.TaskId} from Session {SessionId.PackSessionId()}");
                var subTaskId = this.SubmitSubTask(subTaskPaylaod.serialize(), taskContext.TaskId);
                Log.LogInformation($"Submitted  subTask                    : {subTaskId}");

                ClientPayload aggPayload = new() { Type = ClientPayload.TaskType.Aggregation, result = square };

                Log.LogInformation($"Submitting aggregate task             : {taskContext.TaskId} from Session {SessionId.PackSessionId()}");

                var aggTaskId = this.SubmitTaskWithDependencies(aggPayload.serialize(),
                    new[] { subTaskId });
                Log.LogInformation($"Submitted  SubmitTaskWithDependencies : {aggTaskId} with task dependencies      {subTaskId}");

                return (new ClientPayload() { Type = ClientPayload.TaskType.Aggregation, SubTaskId = aggTaskId })
                    .serialize(); //nothing to do
            }
        }

        private void _1_Job_of_N_Tasks(TaskContext taskContext, byte[] payload, int nbTasks)
        {
            List<byte[]> payloads = new List<byte[]>(nbTasks);
            for (int i = 0; i < nbTasks; i++)
            {
                payloads.Add(payload);
            }

            Stopwatch sw = Stopwatch.StartNew();
            int finalResult = 0;
            var taskIds = this.SubmitSubTasks(payloads, taskContext.TaskId);

            foreach (var taskId in taskIds)
            {
                byte[] taskResult = this.GetResult(taskId);
                finalResult += BitConverter.ToInt32(taskResult);
            }

            long elapsedMilliseconds = sw.ElapsedMilliseconds;
            Log.LogInformation(
                $"Server called {nbTasks} tasks in {elapsedMilliseconds} ms agregated result = {finalResult}");
        }

        public byte[] ComputeCube(TaskContext taskContext, ClientPayload clientPayload)
        {
            int value = clientPayload.numbers[0] * clientPayload.numbers[0] * clientPayload.numbers[0];
            return (new ClientPayload() { Type = ClientPayload.TaskType.Result, result = value })
                .serialize(); //nothing to do
        }

        public override byte[] OnInvoke(SessionContext sessionContext, TaskContext taskContext)
        {
            var clientPayload = ClientPayload.deserialize(taskContext.TaskInput);

            if (clientPayload.Type == ClientPayload.TaskType.ComputeSquare)
            {
                return ComputeSquare(taskContext, clientPayload);
            }
            else if (clientPayload.Type == ClientPayload.TaskType.ComputeCube)
            {
                return ComputeCube(taskContext, clientPayload);
            }
            else if (clientPayload.Type == ClientPayload.TaskType.Sleep)
            {
                Log.LogInformation(
                    $"Empty task, sessionId : {sessionContext.SessionId}, taskId : {taskContext.TaskId}, sessionId from task : {taskContext.SessionId}");
                Thread.Sleep(clientPayload.sleep * 1000);
            }
            else if (clientPayload.Type == ClientPayload.TaskType.JobOfNTasks)
            {
                var newPayload = new ClientPayload()
                    { Type = ClientPayload.TaskType.Sleep, sleep = clientPayload.sleep };

                byte[] bytePayload = newPayload.serialize();

                _1_Job_of_N_Tasks(taskContext, bytePayload, clientPayload.numbers[0] - 1);

                return (new ClientPayload() { Type = ClientPayload.TaskType.Result, result = 42 })
                    .serialize(); //nothing to do
            }
            else if (clientPayload.Type == ClientPayload.TaskType.Aggregation)
            {
                return AggregateValues(taskContext, clientPayload);
            }
            else
            {
                Log.LogInformation($"Task type is unManaged {clientPayload.Type}");
                throw new WorkerApiException($"Task type is unManaged {clientPayload.Type}");
            }

            return (new ClientPayload() { Type = ClientPayload.TaskType.Result, result = 42 })
                .serialize(); //nothing to do
        }

        private byte[] AggregateValues(TaskContext taskContext, ClientPayload clientPayload)
        {
            Log.LogInformation($"Aggregate Task request result from Dependencies TaskIds : [{string.Join(", ", taskContext.DependenciesTaskIds)}]");
            byte[] parentResult = this.GetResult(taskContext.DependenciesTaskIds?.Single());

            if (parentResult == null || parentResult.Length == 0)
            {
                throw new WorkerApiException(
                    $"Cannot retrieve Result from taskId {taskContext.DependenciesTaskIds?.Single()}");
            }

            ClientPayload parentResultPayload = ClientPayload.deserialize(parentResult);
            if (parentResultPayload.SubTaskId != null)
            {
                parentResult = this.GetResult(parentResultPayload.SubTaskId);
                parentResultPayload = ClientPayload.deserialize(parentResult);
            }

            int value = clientPayload.result + parentResultPayload.result;

            ClientPayload childResult = new() { Type = ClientPayload.TaskType.Result, result = value };

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