using System;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Armonik.Samples.StressTests.Client
{
  internal class PayloadGenerator
  {
    private readonly long baseSize_;
    private readonly int variationPercent_;
    private readonly string distribution_;
    private readonly Random random_;
    private readonly ILogger logger_;
    
    private long minGenerated_ = long.MaxValue;
    private long maxGenerated_ = 0;
    private long totalGenerated_ = 0;
    private int countGenerated_ = 0;

    public PayloadGenerator(long baseSize, int variationPercent, string distribution, ILogger logger)
    {
      baseSize_ = baseSize;
      variationPercent_ = Math.Max(0, Math.Min(100, variationPercent));
      distribution_ = distribution?.ToLower() ?? "uniform";
      random_ = new Random();
      logger_ = logger;
    }

    public double[] GeneratePayload()
    {
      var size = GenerateSize();
      var arraySize = (int)Math.Max(1, size / 8);
      return Enumerable.Range(0, arraySize)
                       .Select(x => Math.Pow(42.0 * 8 / size, 1 / 3.0))
                       .ToArray();
    }

    public long GenerateSize()
    {
      long size;
      if (variationPercent_ <= 0)
      {
        size = baseSize_;
      }
      else
      {
        var variationAmount = baseSize_ * variationPercent_ / 100.0;
        double randomValue;
        switch (distribution_)
        {
          case "gaussian":
          case "normal":
            var u1 = 1.0 - random_.NextDouble();
            var u2 = 1.0 - random_.NextDouble();
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            randomValue = randStdNormal * (variationAmount / 3.0);
            randomValue = Math.Max(-variationAmount, Math.Min(variationAmount, randomValue));
            break;
          case "exponential":
            var exp = -Math.Log(1.0 - random_.NextDouble());
            var normalized = Math.Min(1.0, exp / 5.0);
            randomValue = (normalized * 2.0 - 1.0) * variationAmount;
            break;
          case "uniform":
          default:
            randomValue = (random_.NextDouble() * 2.0 - 1.0) * variationAmount;
            break;
        }
        size = (long)Math.Max(8, baseSize_ + randomValue);
      }

      minGenerated_ = Math.Min(minGenerated_, size);
      maxGenerated_ = Math.Max(maxGenerated_, size);
      totalGenerated_ += size;
      countGenerated_++;
      return size;
    }

    public void LogStatistics(string label)
    {
      if (countGenerated_ == 0 || variationPercent_ <= 0)
        return;
      var average = totalGenerated_ / (double)countGenerated_;
      logger_?.LogInformation($"{label} Statistics:");
      logger_?.LogInformation($"  Base size      : {baseSize_ / 1024.0:N1} KB");
      logger_?.LogInformation($"  Generated avg  : {average / 1024.0:N1} KB");
      logger_?.LogInformation($"  Min generated  : {minGenerated_ / 1024.0:N1} KB ({(minGenerated_ - baseSize_) * 100.0 / baseSize_:+0.0;-0.0}%)");
      logger_?.LogInformation($"  Max generated  : {maxGenerated_ / 1024.0:N1} KB ({(maxGenerated_ - baseSize_) * 100.0 / baseSize_:+0.0;-0.0}%)");
      logger_?.LogInformation($"  Samples        : {countGenerated_:N0}");
    }
  }
}
