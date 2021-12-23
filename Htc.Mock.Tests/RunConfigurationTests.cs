// RunConfigurationTests.cs is part of the Htc.Mock solution.
// 
// Copyright (c) 2021-2021 ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (https://github.com/wkirschenmann)
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
using System.Linq;

using Htc.Mock.Utils;

using NUnit.Framework;

namespace Htc.Mock.Tests
{
  [TestFixture]
  public class RunConfigurationTests
  {
    [Test]
    public void GetDistributedValueAvgTest()
    {
      var ran = new Random(0);

      var samples = Enumerable.Range(0, 100000)
                              .Select(_ => Beta.Sample(ran, 50, 100, 20000))
                              .ToList();

      var average = (int)samples.Average();

      Console.WriteLine($"[Htc.Mock] min={samples.Min()}");
      Console.WriteLine($"[Htc.Mock] max={samples.Max()}");
      Console.WriteLine($"[Htc.Mock] avg={average}");

      Assert.LessOrEqual((int)(100 * 0.98), average);
      Assert.GreaterOrEqual((int)(100 * 1.02), average);
    }
  }
}
