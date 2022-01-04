using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ArmoniK.Core.gRPC.V1;
using ArmoniK.DevelopmentKit.GridServer;
using ArmoniK.DevelopmentKit.GridServer.Client;
using ArmoniK.DevelopmentKit.WorkerApi.Common;
using Armonik.Samples.Symphony.Common;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ArmoniK.Samples.GridServer.Client
{
    class Program
    {
        private static IConfiguration configuration_;
        private static ILogger<Program> logger_;


        // Handler for Service Clients
        public class AdderHandler : IServiceInvocationHandler
        {
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

            private double _total = 0;
        }

        static void Main(string[] args)
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
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            configuration_ = builder.Build();

            Properties props = new Properties(configuration_,
                ArmonikDataSynapseClientService.InitializeDefaultTaskOptions());

            Service cs = ServiceFactory.GetInstance().CreateService("ArmoniK.DevelopmentKit.GridServer",
                props);

            object[] arguments = { 5.0, 6.0 };
            var sum = BitConverter.ToDouble(cs.Execute("add",
                arguments));

            AdderHandler handler = new AdderHandler();
            for (int i = 0; i < 10; i++)
            {
                cs.Submit("add",
                    new Object[] { (double)i, (double)i },
                    handler);
            }
        }

        /// <summary>
        /// Initialize Setting for task i.e :
        /// Duration :
        ///         The max duration of a task
        /// Priority :
        ///         Work in Progress. Setting priority of task
        ///
        /// AppName  :
        ///         The name of the Application dll (Without Extension)
        ///
        /// VersionName :
        ///         The version of the package to unzip and execute
        ///
        /// Namespace :
        ///         The namespace where the service can find
        ///         the ServiceContainer object develop by the customer
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
                Priority = 1,
                IdTag = "ArmonikTag",
            };
            taskOptions.Options.Add(AppsOptions.GridAppNameKey,
                "ArmoniK.Samples.SymphonyPackage");

            taskOptions.Options.Add(AppsOptions.GridAppVersionKey,
                "1.0.0");

            taskOptions.Options.Add(AppsOptions.GridAppNamespaceKey,
                "ArmoniK.Samples.Symphony.Packages");

            return taskOptions;
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
