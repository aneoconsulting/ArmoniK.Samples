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

using ArmoniK.Samples.HtcMock.Adapter;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Extensions.Logging;

namespace ArmoniK.Samples.HtcMock.GridWorker
{
  public static class Program
  {
    private static readonly string SocketPath = "/cache/armonik.sock";

    public static int Main(string[] args)
    {
      Log.Logger = new LoggerConfiguration()
                   .Enrich.FromLogContext()
                   .WriteTo.Console()
                   .CreateBootstrapLogger();

      try
      {
        Log.Information("Starting web host");


        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json",
                            true,
                            true)
               .AddEnvironmentVariables()
               .AddCommandLine(args);

        builder.Logging.AddSerilog();

        var serilogLogger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration)
                                                     .Enrich.FromLogContext()
                                                     .CreateLogger();

        var loggerFactory = LoggerFactory.Create(loggingBuilder => loggingBuilder.AddSerilog(serilogLogger));
        var logger        = loggerFactory.CreateLogger("root");

        builder.Host
               .UseSerilog((context, services, config)
                             => config.ReadFrom.Configuration(context.Configuration)
                                      .ReadFrom.Services(services)
                                      .Enrich.FromLogContext());

        builder.WebHost.ConfigureKestrel(options =>
        {
          if (File.Exists(SocketPath))
          {
            File.Delete(SocketPath);
          }

          options.ListenUnixSocket(SocketPath,
                                   listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
        });

        builder.Services
               .AddSingleton<ApplicationLifeTimeManager>()
               .AddSingleton(sp => loggerFactory)
               .AddLogging()
               .AddGrpc(options => options.MaxReceiveMessageSize = null);


        var app = builder.Build();

        if (app.Environment.IsDevelopment())
          app.UseDeveloperExceptionPage();

        app.UseSerilogRequestLogging();

        app.UseRouting();


        app.UseEndpoints(endpoints =>
        {
          endpoints.MapGrpcService<SampleComputerService>();

          if (app.Environment.IsDevelopment())
          {
            endpoints.MapGrpcReflectionService();
            logger.LogInformation("Grpc Reflection Activated");
          }
        });

        app.Run();

        return 0;
      }
      catch (Exception ex)
      {
        Log.Fatal(ex,
                  "Host terminated unexpectedly");
        return 1;
      }
      finally
      {
        Log.CloseAndFlush();
      }
    }
  }
}