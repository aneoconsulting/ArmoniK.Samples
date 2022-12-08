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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

using ArmoniK.DevelopmentKit.Common.Exceptions;
using ArmoniK.DevelopmentKit.Worker.Unified;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace Armonik.Samples.StressTests.Worker
{
  public class ServiceApps : BaseService<ServiceApps>
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


    public double[] ComputeWorkLoad(double[] input,
                                    long     nbOutputBytes,
                                    int      workLoadTimeInMs)
    {
      if (input is not
          {
            Length: > 0,
          } || nbOutputBytes <= 0)
      {
        logger_.LogInformation("Cannot execute function with nb element <= 0");
        throw new WorkerApiException("Cannot execute function with nb bytes <= 0");
      }

      if (workLoadTimeInMs <= 0)
      {
        workLoadTimeInMs = 1;
      }

      var output = Enumerable.Range(0,
                                    (int)(nbOutputBytes / 8))
                             .Select(x => (double)x)
                             .ToArray();

      var result = input.Select(x => Math.Pow(x,
                                              3.0))
                        .Sum();
      // Record start time
      var start = Stopwatch.StartNew();

      while (start.ElapsedMilliseconds < workLoadTimeInMs)
      {
        for (var rIdx = 0; rIdx < output.Length; rIdx++)
        {
          output[rIdx] = result / output.Length;
        }
      }


      return output;
    }
  }
}
