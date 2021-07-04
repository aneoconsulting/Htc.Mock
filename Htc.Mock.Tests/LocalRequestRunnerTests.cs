/* LocalRequestRunnerTests.cs is part of the Htc.Mock.Tests solution.
    
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
using System.Threading;

using Htc.Mock.Core;
using Htc.Mock.RequestRunners;

using NUnit.Framework;

namespace Htc.Mock.Tests
{
  [TestFixture]
  public class LocalRequestRunnerTests
  {
    private static void ProcessRequest(RunConfiguration config,
                                       out TimeSpan     totalCalculationTime,
                                       out string       result,
                                       out int          nbProcessedTask, 
                                       out int          nbSubTasks)
    {
      var runner = new LocalRequestRunner(config);

      var localNbProcessedTasks = 0;

      var localTotalCalculationTime = 0L;

      runner.StartRequestProcessingEvent += nestedRequest =>
                                            {
                                              Interlocked.Increment(ref localNbProcessedTasks);
                                              var duration = config.GetTaskDurationMs(nestedRequest.Id);
                                              Interlocked.Add(ref localTotalCalculationTime, duration);
                                            };

      var localNbSubTasks = 0;
      runner.SpawningRequestEvent += localNbSubtasks => Interlocked.Add(ref localNbSubTasks , localNbSubtasks);

      var request = config.DefaultHeadRequest();

      result               = runner.ProcessRequest(request, true);
      totalCalculationTime = TimeSpan.FromMilliseconds(localTotalCalculationTime);
      nbProcessedTask      = localNbProcessedTasks;
      nbSubTasks           = localNbSubTasks;

      Console.WriteLine($"{nameof(nbProcessedTask)}={nbProcessedTask}");
      Console.WriteLine($"{nameof(nbSubTasks)}={nbSubTasks}");
      Console.WriteLine($"{nameof(totalCalculationTime)}={totalCalculationTime}");
      Console.WriteLine($"{nameof(result)}={result}");

      Assert.AreEqual(nbSubTasks + 1, nbProcessedTask);
      Assert.AreEqual(config.TotalNbSubTasks, nbSubTasks);
    }

    [Test]
    public void ProcessRequestMinimalTest()
    {
      var config = RunConfiguration.Minimal;

      ProcessRequest(config, out _, out var result, out _, out _);

      Assert.AreEqual("HeadId_result", result);
    }

    [Test]
    public void ProcessRequestXSmallTest()
    {
      var config = RunConfiguration.XSmall;

      ProcessRequest(config, out var totalCalculationTime, out var result, out _, out _);

      Assert.AreEqual("Aggregate_1871498793_result", result);
      Assert.AreEqual(TimeSpan.Parse("00:04:16.8940000"), totalCalculationTime);
    }

    [Test]
    public void ProcessRequestSmallTest()
    {
      var config = RunConfiguration.Small;

      ProcessRequest(config, out var totalCalculationTime, out var result, out var _, out var _);

      Assert.AreEqual("Aggregate_2608871805_result", result);
      Assert.AreEqual(TimeSpan.Parse("00:09:48.9270000"), totalCalculationTime);
    }

    [Test]
    public void ProcessRequestMediumTest()
    {
      var config = RunConfiguration.Medium;

      ProcessRequest(config, out var totalCalculationTime, out var result, out _, out _);

      Assert.AreEqual("Aggregate_3926158863_result", result);
      Assert.AreEqual(TimeSpan.Parse("01:00:33.8280000"), totalCalculationTime);
    }

    [Test]
    public void ProcessRequestLargeTest()
    {
      var config = RunConfiguration.Large;

      ProcessRequest(config, out var totalCalculationTime, out var result, out _, out _);

      Assert.AreEqual("Aggregate_3409642680_result", result);
      Assert.AreEqual(TimeSpan.Parse("1.11:45:33.0270000"), totalCalculationTime);
    }

    [Test]
    public void ProcessRequestXLargeTest()
    {
      var config = RunConfiguration.XLarge;

      ProcessRequest(config, out var totalCalculationTime, out var result, out _, out _);

      Assert.AreEqual("Aggregate_1864571827_result", result);
      Assert.AreEqual(TimeSpan.Parse("999.14:56:01.9850000"), totalCalculationTime);
    }
  }
}