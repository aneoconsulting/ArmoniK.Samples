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
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft",
                    LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            var factory = new LoggerFactory().AddSerilog();

            logger_ = factory.CreateLogger<ServiceContainer>();
        }

        public byte[] ComputeSquare(double a)
        {
            logger_.LogInformation($"Enter in function : ComputeSquare");

            double res = a * a;

            return JsonSerializer.SerializeToUtf8Bytes(res);
        }

        public byte[] ComputeCube(int a)
        {
            int value = a * a * a;

            return JsonSerializer.SerializeToUtf8Bytes(value);
        }
    }
}