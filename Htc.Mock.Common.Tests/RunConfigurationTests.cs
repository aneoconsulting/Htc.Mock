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

      var ran = new Random(0);

      var samples = Enumerable.Range(0, 100000)
                              .Select(i => Beta.Sample(ran, 50, 100, 20000))
                              .ToList();

      var average = (int) samples.Average();

      Console.WriteLine($"min={samples.Min()}");
      Console.WriteLine($"max={samples.Max()}");
      Console.WriteLine($"avg={average}");

      Assert.LessOrEqual((int)(100 * 0.98), average);
      Assert.GreaterOrEqual((int)(100 * 1.02), average);
    }
  }
}