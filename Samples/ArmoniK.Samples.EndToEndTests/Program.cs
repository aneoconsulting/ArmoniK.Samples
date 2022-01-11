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
using System.IO;
using System.Text;
using System.Threading;

using ArmoniK.Core.gRPC.V1;
using ArmoniK.DevelopmentKit.SymphonyApi.Client;
using ArmoniK.DevelopmentKit.WorkerApi.Common;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.EndToEndTests
{
  internal class Program
  {
    private static IConfiguration   configuration_;
    private static ILogger<Program> logger_;

    private static void Main(string[] args)
    {
      Console.WriteLine("Hello Armonik End to End Tests !");


      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();

      configuration_ = builder.Build();

      Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(configuration_)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .CreateBootstrapLogger();

      var factory = new LoggerFactory(new[]
      {
        new SerilogLoggerProvider(Log.Logger),
      });

      logger_ = factory.CreateLogger<Program>();

      var client = new ArmonikSymphonyClient(configuration_);

      logger_.LogInformation("Configure taskOptions");
      var taskOptions = InitializeTaskOptions();

      var sessionId = client.CreateSession(taskOptions);

      logger_.LogInformation($"New session created : {sessionId}");

      logger_.LogInformation("Running End to End test to compute Square value with SubTasking");
      ClientStartup1(client);

      logger_.LogInformation("Running End to End test to check task average time per milliseconds");
      ClientStartup2(client);
    }

    /// <summary>
    ///   Initialize Setting for task i.e :
    ///   Duration :
    ///   The max duration of a task
    ///   Priority :
    ///   Work in Progress. Setting priority of task
    ///   AppName  :
    ///   The name of the Application dll (Without Extension)
    ///   VersionName :
    ///   The version of the package to unzip and execute
    ///   Namespace :
    ///   The namespace where the service can find
    ///   the ServiceContainer object develop by the customer
    /// </summary>
    /// <returns></returns>
    private static TaskOptions InitializeTaskOptions()
    {
      TaskOptions taskOptions = new()
      {
        MaxDuration = new Duration
        {
          Seconds = 300,
        },
        MaxRetries = 5,
        Priority   = 1,
        IdTag      = "ArmonikTag",
      };
      taskOptions.Options.Add(AppsOptions.GridAppNameKey,
                              "ArmoniK.Samples.EndToEndTests");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "1.0.0");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.Samples.EndToEndTests");

      return taskOptions;
    }

    /// <summary>
    ///   Simple function to wait and get the result from subTasking and result delegation
    ///   to a subTask
    /// </summary>
    /// <param name="client">The client API to connect to the Control plane Service</param>
    /// <param name="taskId">The task which is waiting for</param>
    /// <returns></returns>
    private static byte[] WaitForSubTaskResult(ArmonikSymphonyClient client, string taskId)
    {
      client.WaitSubtasksCompletion(taskId);
      var taskResult = client.GetResult(taskId);
      var result     = ClientPayload.Deserialize(taskResult);

      if (!string.IsNullOrEmpty(result.SubTaskId))
      {
        client.WaitSubtasksCompletion(result.SubTaskId);
        taskResult = client.GetResult(result.SubTaskId);
      }

      return taskResult;
    }

    /// <summary>
    ///   The first test developed to validate dependencies subTasking
    /// </summary>
    /// <param name="client"></param>
    public static void ClientStartup1(ArmonikSymphonyClient client)
    {
      var numbers = new List<int>
      {
        1,
        2,
        3,
      };
      var clientPaylaod = new ClientPayload
      {
        IsRootTask = true,
        Numbers    = numbers,
        Type       = ClientPayload.TaskType.ComputeSquare,
      };
      var taskId = client.SubmitTask(clientPaylaod.Serialize());

      var taskResult = WaitForSubTaskResult(client,
                                            taskId);
      var result = ClientPayload.Deserialize(taskResult);

      logger_.LogInformation($"output result : {result.Result}");
    }

    /// <summary>
    ///   The ClientStartUp2 is used to check some execution performance
    ///   (Need to investigate performance with this test. Not yet investigate)
    /// </summary>
    /// <param name="client"></param>
    public static void ClientStartup2(ArmonikSymphonyClient client)
    {
      var numbers = new List<int>
      {
        2,
      };
      var clientPayload = new ClientPayload
      {
        Numbers = numbers,
        Type    = ClientPayload.TaskType.ComputeCube,
      };
      var payload        = clientPayload.Serialize();
      var outputMessages = new StringBuilder();
      outputMessages.AppendLine("In this serie of samples we're creating N jobs of one task.");
      outputMessages.AppendLine(@"In the loop we have :
        1 sending job of one task
        2 wait for result
        3 get associated payload");
      N_Jobs_of_1_Task(client,
                       payload,
                       1,
                       outputMessages);
      N_Jobs_of_1_Task(client,
                       payload,
                       10,
                       outputMessages);
      //N_Jobs_of_1_Task(client, payload, 100, outputMessages);
      //N_Jobs_of_1_Task(client, payload, 200, outputMessages);
      // N_Jobs_of_1_Task(client, payload, 500, outputMessages);

      outputMessages.AppendLine("In this serie of samples we're creating 1 job of N tasks.");

      _1_Job_of_N_Tasks(client,
                        payload,
                        1,
                        outputMessages);
      _1_Job_of_N_Tasks(client,
                        payload,
                        10,
                        outputMessages);
      //_1_Job_of_N_Tasks(client, payload, 100, outputMessages);
      //_1_Job_of_N_Tasks(client, payload, 200, outputMessages);
      //_1_Job_of_N_Tasks(client, payload, 500, outputMessages);

      logger_.LogInformation(outputMessages.ToString());
    }

    /// <summary>
    ///   A test to execute Several Job with 1 task by jub
    /// </summary>
    /// <param name="client">The client to connect to the Control plane Service</param>
    /// <param name="payload">A default payload to execute by each task</param>
    /// <param name="nbJobs">The Number of jobs</param>
    /// <param name="outputMessages">The print log stored in a StringBuilder object</param>
    private static void N_Jobs_of_1_Task(ArmonikSymphonyClient client,
                                         byte[]                payload,
                                         int                   nbJobs,
                                         StringBuilder         outputMessages)
    {
      var sw          = Stopwatch.StartNew();
      var finalResult = 0;
      for (var i = 0; i < nbJobs; i++)
      {
        var taskId = client.SubmitTask(payload);
        var taskResult = WaitForSubTaskResult(client,
                                              taskId);
        var result = ClientPayload.Deserialize(taskResult);
        finalResult += result.Result;
      }

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      outputMessages.AppendLine($"Client called {nbJobs} jobs of one task in {elapsedMilliseconds} ms agregated result = {finalResult}");
    }


    /// <summary>
    ///   The function to execute 1 job with several tasks inside
    /// </summary>
    /// <param name="client">The client to connect to the Control plane Service</param>
    /// <param name="payload">A default payload to execute by each task</param>
    /// <param name="nbTasks">The Number of jobs</param>
    /// <param name="outputMessages">The print log stored in a StringBuilder object</param>
    private static void _1_Job_of_N_Tasks(ArmonikSymphonyClient client,
                                          byte[]                payload,
                                          int                   nbTasks,
                                          StringBuilder         outputMessages)
    {
      var payloads = new List<byte[]>(nbTasks);
      for (var i = 0; i < nbTasks; i++)
        payloads.Add(payload);

      var sw          = Stopwatch.StartNew();
      var finalResult = 0;
      var taskIds     = client.SubmitTasks(payloads);
      foreach (var taskId in taskIds)
      {
        var taskResult = WaitForSubTaskResult(client,
                                              taskId);
        var result = ClientPayload.Deserialize(taskResult);

        finalResult += result.Result;
      }

      var elapsedMilliseconds = sw.ElapsedMilliseconds;
      outputMessages.AppendLine($"Client called {nbTasks} tasks in {elapsedMilliseconds} ms agregated result = {finalResult}");
    }
  }
}