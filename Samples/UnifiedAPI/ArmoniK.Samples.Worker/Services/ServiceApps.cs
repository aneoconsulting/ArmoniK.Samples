// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2025. All rights reserved.
//   W. Kirschenmann   <wkirschenmann@aneo.fr>
//   J. Gurhem         <jgurhem@aneo.fr>
//   D. Dubuc          <ddubuc@aneo.fr>
//   L. Ziane Khodja   <lzianekhodja@aneo.fr>
//   F. Lemaitre       <flemaitre@aneo.fr>
//   S. Djebbar        <sdjebbar@aneo.fr>
//   J. Fonseca        <jfonseca@aneo.fr>
//   D. Brasseur       <dbrasseur@aneo.fr>
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using System.Threading;

using ArmoniK.DevelopmentKit.Worker.Unified;
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
