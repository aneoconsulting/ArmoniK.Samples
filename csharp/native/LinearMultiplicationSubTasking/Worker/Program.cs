using System.IO;

using ArmoniK.Api.Worker.Utils;
using ArmoniK.Samples.LinearMultiplicationSubTasking.Worker;

using Microsoft.Extensions.Configuration;

// var configuration = new ConfigurationBuilder()
//     .SetBasePath(Directory.GetCurrentDirectory())
//     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//     .Build();

WorkerServer.Create<LinearMultiplicationSubTaskingWorker>()
            .Run();