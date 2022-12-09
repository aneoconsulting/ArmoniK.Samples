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

using System.IO;
using System.Linq;
using System.Threading;

using ArmoniK.DevelopmentKit.Worker.Grid;
using ArmoniK.Samples.Common;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.Unified.Worker.Services
{
  public class ServiceApps : TaskSubmitterWorkerService
  {
    private readonly IConfiguration       configuration_;
    private readonly ILogger<ServiceApps> logger_;

    public ServiceApps()
    {
      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddJsonFile("appsettings.json",
                                                           true,
                                                           true)
                                              .AddEnvironmentVariables();


      configuration_ = builder.Build();

      Log.Logger = new LoggerConfiguration().MinimumLevel.Override("Microsoft",
                                                                   LogEventLevel.Information)
                                            .ReadFrom.Configuration(configuration_)
                                            .Enrich.FromLogContext()
                                            .WriteTo.Console()
                                            .CreateBootstrapLogger();

      var logProvider = new SerilogLoggerProvider(Log.Logger);
      var factory = new LoggerFactory(new[]
                                      {
                                        logProvider,
                                      });

      logger_ = factory.CreateLogger<ServiceApps>();
    }

    public static double[] ComputeBasicArrayCube(double[] inputs)
      => inputs.Select(x => x * x * x)
               .ToArray();

    public static double ComputeReduce(double[] inputs)
      => inputs.Sum();

    public static double ComputeReduceCube(double[] inputs,
                                           int      workloadTimeInMs = 10)
    {
      Thread.Sleep(workloadTimeInMs);

      return inputs.Select(x => x * x * x)
                   .Sum();
    }

    public static double ComputeReduceCube(byte[] inputs)
    {
      var doubles = inputs.ConvertToArray();

      return doubles.Select(x => x * x * x)
                    .Sum();
    }

    public static double[] ComputeMadd(byte[] inputs1,
                                       byte[] inputs2,
                                       double k)
    {
      var doubles1 = inputs1.ConvertToArray()
                            .ToArray();
      var doubles2 = inputs2.ConvertToArray()
                            .ToArray();


      return doubles1.Select((x,
                              idx) => k * x * doubles2[idx])
                     .ToArray();
    }

    public double[] NonStaticComputeMadd(byte[] inputs1,
                                         byte[] inputs2,
                                         double k)
    {
      var doubles1 = inputs1.ConvertToArray()
                            .ToArray();
      var doubles2 = inputs2.ConvertToArray()
                            .ToArray();


      return doubles1.Select((x,
                              idx) => k * x * doubles2[idx])
                     .ToArray();
    }
  }
}
