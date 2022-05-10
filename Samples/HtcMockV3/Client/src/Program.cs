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
using System.IO;

using Htc.Mock.Core;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Formatting.Compact;

namespace ArmoniK.Samples.HtcMock.Client
{
  internal class Program
  {
    private static void Main()
    {
      Console.WriteLine("Hello Mock V3!");

      var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                              .AddEnvironmentVariables();
      var configuration = builder.Build();
      Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Console(new CompactJsonFormatter())
                   .CreateBootstrapLogger();

      var factory = new LoggerFactory().AddSerilog();
      var serviceProvider = new ServiceCollection().AddComponents(configuration)
                                                   .AddSingleton(factory)
                                                   .BuildServiceProvider();


      var gridClient = serviceProvider.GetRequiredService<GridClient>();

      var client = new HtcMockClient(gridClient,
                                       factory.CreateLogger<Htc.Mock.Client>());

      // Timespan(heures, minutes, secondes)
      // RunConfiguration runConfiguration = RunConfiguration.XSmall; // result : Aggregate_1871498793_result
      var runConfiguration = new RunConfiguration(new TimeSpan(0,
                                                               0,
                                                               0,
                                                               0,
                                                               100),
                                                  100,
                                                  1,
                                                  1,
                                                  4);
      // client.ParallelExec(RunConfiguration.XSmall, 5);
      // client.SeqExec(RunConfiguration.XSmall, 5);
      client.SeqExec(runConfiguration,
                     1);
    }
  }
}