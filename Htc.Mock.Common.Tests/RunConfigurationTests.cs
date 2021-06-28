using System;
using System.Linq;

using NUnit.Framework;

namespace Htc.Mock.Common.Tests
{
  [TestFixture]
  public class RunConfigurationTests
  {
    [Test]
    public void GetDistributedValueAvgTest()
    {
      var nbSamples = 100000;
      var target    = 100;

      var ran = new Random(0);

      var samples = Enumerable.Range(0, nbSamples)
                              .Select(i => RunConfiguration.GetDistributedValue(50, 20000, target, ran))
                              .ToList();

      var average = (int) samples.Average();

      Console.WriteLine($"min={samples.Min()}");
      Console.WriteLine($"max={samples.Max()}");
      Console.WriteLine($"avg={average}");

      Assert.LessOrEqual((int)(target*0.98), average);
      Assert.GreaterOrEqual((int)(target * 1.02), average);
    }
  }
}