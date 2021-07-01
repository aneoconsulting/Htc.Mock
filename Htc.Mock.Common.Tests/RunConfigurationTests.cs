/* RunConfigurationTests.cs is part of the Htc.Mock.Common.Tests solution.
    
   Copyright (c) 2021-2021 ANEO. 
     W. Kirschenmann (https://github.com/wkirschenmann)
  
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
   
       http://www.apache.org/licenses/LICENSE-2.0
   
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

*/ 


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