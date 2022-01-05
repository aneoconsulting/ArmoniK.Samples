using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace ArmoniK.Samples.GridServer.Client.Services
{
    public class ServiceContainer
    {
        private readonly IConfiguration configuration_;
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

            var factory = new LoggerFactory().AddSerilog();

            logger_ = factory.CreateLogger<ServiceContainer>();
        }

        public double ComputeSquare(double a)
        {
            logger_.LogInformation($"Enter in function : ComputeSquare");

            double res = a * a;

            return res;
        }

        public int ComputeCube(int a)
        {
            int value = a * a * a;

            return value;
        }

        public int ComputeDivideByZero(int a)
        {
            int value = a / 0;

            return value;
        }

        public double Add(double value1, double value2)
        {
            return value1 + value2;
        }

        public double AddGenerateException(double value1, double value2)
        {
            throw new NotImplementedException("Fake Method to generate an NotYetImplementedException");
        }
    }
}