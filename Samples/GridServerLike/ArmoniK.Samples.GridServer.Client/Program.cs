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
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using ArmoniK.Core.gRPC.V1;
using ArmoniK.DevelopmentKit.GridServer.Client;
using ArmoniK.DevelopmentKit.WorkerApi.Common;
using ArmoniK.Samples.GridServer.Client.Services;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;

namespace ArmoniK.Samples.GridServer.Client
{
  internal class Program
  {
    private static IConfiguration   configuration_;
    private static ILogger<Program> logger_;

    private static void Main(string[] args)
    {
      Console.WriteLine("Hello Armonik SymphonyLike Sample !");

      Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Override("Microsoft",
                                          LogEventLevel.Information)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .CreateBootstrapLogger();

      var factory = new LoggerFactory().AddSerilog();

      logger_ = factory.CreateLogger<Program>();

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();

      configuration_ = builder.Build();

      var taskOptions = new TaskOptions
      {
        MaxDuration = new Duration
        {
          Seconds = 300,
        },
        MaxRetries = 3,
        Priority   = 1,
        IdTag      = "ArmonikTag",
      };

      taskOptions.Options.Add(AppsOptions.EngineTypeNameKey,
                              EngineType.DataSynapse.ToString());

      taskOptions.Options.Add(AppsOptions.GridAppNameKey,
                              "ArmoniK.Samples.GridServer.Client");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "1.0.0");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.Samples.GridServer.Client.Services");

      taskOptions.Options.Add(AppsOptions.GridServiceNameKey,
                              "ServiceContainer");

      var props = new Properties(configuration_,
                                 taskOptions);

      var cs = ServiceFactory.GetInstance().CreateService("ArmoniK.Samples.GridServer.Package",
                                                          props);

      object[] arguments = { 5.0 };
      var sum = cs.LocalExecute(new ServiceContainer(),
                                "ComputeSquare",
                                arguments);

      logger_.LogInformation($"Result of computation : {(double) sum}");

      var handler = new AdderHandler();
      for (var i = 0; i < 10; i++)
        cs.Submit("Add",
                  new object[] { (double) i, (double) i },
                  handler);
      cs.Submit("Add",
                new object[] { (double) 2, (double) 4 },
                handler);

      Task.WaitAll(cs.TaskWarehouse.Values.ToArray());
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
    private static TaskOptions InitializeSimpleTaskOptions()
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
                              "ArmoniK.Samples.SymphonyPackage");

      taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                              "1.0.0");

      taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                              "ArmoniK.Samples.Symphony.Packages");

      return taskOptions;
    }


    // Handler for Service Clients
    public class AdderHandler : IServiceInvocationHandler
    {
      private readonly double _total = 0;

      public void HandleError(ServiceInvocationException e, string taskId)
      {
        Console.Out.WriteLine("Error from " + taskId + ": " + e);
      }

      public void HandleResponse(object response, string id)
      {
        Console.Out.WriteLine("Response from " + id + ": " + response);
      }

      public double getTotal()
      {
        return _total;
      }
    }

    ///// <summary>
    ///// The first test developed to validate dependencies subTasking 
    ///// </summary>
    ///// <param name="client"></param>
    //public static void ClientStartup1(ArmonikSymphonyClient client)
    //{
    //    List<int> numbers = new List<int>() { 1, 2, 3};
    //    var clientPaylaod = new ClientPayload()
    //        { IsRootTask = true, numbers = numbers, Type = ClientPayload.TaskType.ComputeSquare };
    //    string taskId = client.SubmitTask(clientPaylaod.serialize());

    //    byte[] taskResult = WaitForSubTaskResult(client, taskId);
    //    ClientPayload result = ClientPayload.deserialize(taskResult);

    //    logger_.LogInformation($"output result : {result.result}");
    //}

    ///// <summary>
    ///// The ClientStartUp2 is used to check some execution performance
    ///// (Need to investigate performance with this test. Not yet investigate)
    ///// </summary>
    ///// <param name="client"></param>
    //public static void ClientStartup2(ArmonikSymphonyClient client)
    //{
    //    List<int> numbers = new List<int>() { 2 };
    //    var clientPayload = new ClientPayload() { numbers = numbers, Type = ClientPayload.TaskType.ComputeCube };
    //    byte[] payload = clientPayload.serialize();
    //    StringBuilder outputMessages = new StringBuilder();
    //    outputMessages.AppendLine("In this serie of samples we're creating N jobs of one task.");
    //    outputMessages.AppendLine(@"In the loop we have :
    //1 sending job of one task
    //2 wait for result
    //3 get associated payload");
    //    N_Jobs_of_1_Task(client, payload, 1, outputMessages);
    //    N_Jobs_of_1_Task(client, payload, 10, outputMessages);
    //    N_Jobs_of_1_Task(client, payload, 100, outputMessages);
    //    N_Jobs_of_1_Task(client, payload, 200, outputMessages);
    //    // N_Jobs_of_1_Task(client, payload, 500, outputMessages);

    //    outputMessages.AppendLine("In this serie of samples we're creating 1 job of N tasks.");

    //    _1_Job_of_N_Tasks(client, payload, 1, outputMessages);
    //    _1_Job_of_N_Tasks(client, payload, 10, outputMessages);
    //    _1_Job_of_N_Tasks(client, payload, 100, outputMessages);
    //    _1_Job_of_N_Tasks(client, payload, 200, outputMessages);
    //    _1_Job_of_N_Tasks(client, payload, 500, outputMessages);

    //    logger_.LogInformation(outputMessages.ToString());
    //}

    /// <summary>
    /// A test to execute Several Job with 1 task by jub
    /// </summary>
    /// <param name="client">The client to connect to the Control plane Service</param>
    /// <param name="payload">A default payload to execute by each task</param>
    /// <param name="nbJobs">The Number of jobs</param>
    /// <param name="outputMessages">The print log stored in a StringBuilder object</param>
    //private static void N_Jobs_of_1_Task(ArmonikSymphonyClient client, byte[] payload, int nbJobs,
    //    StringBuilder outputMessages)
    //{
    //    Stopwatch sw = Stopwatch.StartNew();
    //    int finalResult = 0;
    //    for (int i = 0; i < nbJobs; i++)
    //    {
    //        string taskId = client.SubmitTask(payload);
    //        var taskResult = WaitForSubTaskResult(client, taskId);
    //        ClientPayload result = ClientPayload.deserialize(taskResult);
    //        finalResult += result.result;
    //    }

    //    long elapsedMilliseconds = sw.ElapsedMilliseconds;
    //    outputMessages.AppendLine(
    //        $"Client called {nbJobs} jobs of one task in {elapsedMilliseconds} ms agregated result = {finalResult}");
    //}


    /// <summary>
    /// The function to execute 1 job with several tasks inside
    /// </summary>
    /// <param name="client">The client to connect to the Control plane Service</param>
    /// <param name="payload">A default payload to execute by each task</param>
    /// <param name="nbTasks">The Number of jobs</param>
    /// <param name="outputMessages">The print log stored in a StringBuilder object</param>
    //private static void _1_Job_of_N_Tasks(ArmonikSymphonyClient client, byte[] payload, int nbTasks,
    //    StringBuilder outputMessages)
    //{
    //    List<byte[]> payloads = new List<byte[]>(nbTasks);
    //    for (int i = 0; i < nbTasks; i++)
    //    {
    //        payloads.Add(payload);
    //    }

    //    Stopwatch sw = Stopwatch.StartNew();
    //    int finalResult = 0;
    //    var taskIds = client.SubmitTasks(payloads);
    //    foreach (var taskId in taskIds)
    //    {
    //        var taskResult = WaitForSubTaskResult(client, taskId);
    //        ClientPayload result = ClientPayload.deserialize(taskResult);

    //        finalResult += result.result;
    //    }

    //    long elapsedMilliseconds = sw.ElapsedMilliseconds;
    //    outputMessages.AppendLine(
    //        $"Client called {nbTasks} tasks in {elapsedMilliseconds} ms agregated result = {finalResult}");
    //}
  }
}