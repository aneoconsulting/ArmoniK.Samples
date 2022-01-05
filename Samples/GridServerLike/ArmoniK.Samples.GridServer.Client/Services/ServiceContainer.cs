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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.GridServer.Client.Services
{
  public class ServiceContainer
  {
    private readonly IConfiguration            configuration_;
    private readonly ILogger<ServiceContainer> logger_;

    public ServiceContainer()
    {
      var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json",
                                 true,
                                 true)
                    .AddEnvironmentVariables();


      var Configuration = builder.Build();

      Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Override("Microsoft",
                                          LogEventLevel.Information)
                   .ReadFrom.Configuration(Configuration)
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .CreateBootstrapLogger();

      var logProvider = new SerilogLoggerProvider(Log.Logger);
      var factory     = new LoggerFactory(new[] { logProvider });

      logger_ = factory.CreateLogger<ServiceContainer>();
    }

    public double ComputeSquare(double a)
    {
      logger_.LogInformation("Enter in function : ComputeSquare");

      var res = a * a;

      return res;
    }

    public int ComputeCube(int a)
    {
      logger_.LogInformation("Enter in function : ComputeCube");
      var value = a * a * a;

      return value;
    }

    public int ComputeDivideByZero(int a)
    {
      logger_.LogInformation("Enter in function : ComputeDivideByZero");
      var value = a / 0;

      return value;
    }

    public double Add(double value1, double value2)
    {
      logger_.LogInformation("Enter in function : Add");
      return value1 + value2;
    }

    public double AddGenerateException(double value1, double value2)
    {
      throw new NotImplementedException("Fake Method to generate an NotYetImplementedException");
    }
  }
}