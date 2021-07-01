/* LocalRequestRunnerTests.cs is part of the Htc.Mock.Common.Tests solution.
    
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


using NUnit.Framework;
using Htc.Mock.Common;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Htc.Mock.Common.Tests
{
  [TestFixture]
  public class LocalRequestRunnerTests
  {
    private const bool PrintSubtasks = false;

    [Test]
    public void ProcessRequestMinimalTest()
    {
      var config = RunConfiguration.Minimal;
      
      var runner = new LocalRequestRunner(config);

      var nbProcessedTasks     = 0;
      var totalCalculationTime = new TimeSpan();

      runner.StartRequestProcessingEvent += nestedRequest =>
                                            {
                                              nbProcessedTasks++;
                                              totalCalculationTime += new TimeSpan(nestedRequest.DurationMs * 10000);
                                            };

      var nbSubTasks = 0;
      runner.SpawningRequestEvent += (localNbSubtasks, depth) =>
                                     {
                                       nbSubTasks += localNbSubtasks;
                                       if (PrintSubtasks)
                                         Console.WriteLine($"{nameof(localNbSubtasks)}={localNbSubtasks} ({nameof(depth)}={depth})");
                                     };

      var request = config.DefaultHeadRequest();

      var result = runner.ProcessRequest(request);

      Console.WriteLine($"{nameof(nbProcessedTasks)}={nbProcessedTasks}");
      Console.WriteLine($"{nameof(nbSubTasks)}={nbSubTasks}");
      Console.WriteLine($"{nameof(totalCalculationTime)}={totalCalculationTime}");
      Console.WriteLine($"{nameof(result)}={result}s");

      Assert.AreEqual("HeadId_result", result);
    }

    [Test]
    public void ProcessRequestXSmallTest()
    {
      var config = RunConfiguration.XSmall;
      
      var runner = new LocalRequestRunner(config);

      var nbProcessedTasks     = 0;
      var totalCalculationTime = new TimeSpan();

      runner.StartRequestProcessingEvent += nestedRequest =>
                                            {
                                              nbProcessedTasks++;
                                              totalCalculationTime += new TimeSpan(nestedRequest.DurationMs * 10000);
                                            };

      var nbSubTasks = 0;
      runner.SpawningRequestEvent += (localNbSubtasks, depth) =>
                                     {
                                       nbSubTasks += localNbSubtasks;
                                       if (PrintSubtasks)
                                         Console.WriteLine($"{nameof(localNbSubtasks)}={localNbSubtasks} ({nameof(depth)}={depth})");
                                     };

      var request = config.DefaultHeadRequest();

      var result = runner.ProcessRequest(request);

      Console.WriteLine($"{nameof(nbProcessedTasks)}={nbProcessedTasks}");
      Console.WriteLine($"{nameof(nbSubTasks)}={nbSubTasks}");
      Console.WriteLine($"{nameof(totalCalculationTime)}={totalCalculationTime}");
      Console.WriteLine($"{nameof(result)}={result}s");
    }

    [Test]
    public void ProcessRequestSmallTest()
    {
      var config = RunConfiguration.Small;
      
      var runner = new LocalRequestRunner(config);

      var nbProcessedTasks     = 0;
      var totalCalculationTime = new TimeSpan();

      var startRequestProcessingEventLock = new object();
      runner.StartRequestProcessingEvent += nestedRequest =>
                                            {
                                              Interlocked.Increment(ref nbProcessedTasks);
                                              lock (startRequestProcessingEventLock)
                                              {
                                                totalCalculationTime += new TimeSpan(nestedRequest.DurationMs * 10000);
                                              }
                                            };

      var nbSubTasks               = 0;
      runner.SpawningRequestEvent += (localNbSubtasks, depth) =>
                                     {
                                       Interlocked.Add(ref nbSubTasks, localNbSubtasks);
                                       if (PrintSubtasks)
                                         Console.WriteLine($"{nameof(localNbSubtasks)}={localNbSubtasks} ({nameof(depth)}={depth})");
                                     };

      var request = config.DefaultHeadRequest();

      var result = runner.ProcessRequest(request);

      Console.WriteLine($"{nameof(nbProcessedTasks)}={nbProcessedTasks}");
      Console.WriteLine($"{nameof(nbSubTasks)}={nbSubTasks}");
      Console.WriteLine($"{nameof(totalCalculationTime)}={totalCalculationTime}");
      Console.WriteLine($"{nameof(result)}={result}s");
    }

    [Test]
    public void ProcessRequestMediumTest()
    {
      var config = RunConfiguration.Medium;
      
      var runner = new LocalRequestRunner(config);

      var nbProcessedTasks     = 0;
      var totalCalculationTime = new TimeSpan();

      runner.StartRequestProcessingEvent += nestedRequest =>
                                            {
                                              nbProcessedTasks++;
                                              totalCalculationTime += new TimeSpan(nestedRequest.DurationMs * 10000);
                                            };

      var nbSubTasks = 0;
      runner.SpawningRequestEvent += (localNbSubtasks, depth) =>
                                     {
                                       nbSubTasks += localNbSubtasks;
                                       if (PrintSubtasks)
                                         Console.WriteLine($"{nameof(localNbSubtasks)}={localNbSubtasks} ({nameof(depth)}={depth})");
                                     };

      var request = config.DefaultHeadRequest();

      var result = runner.ProcessRequest(request);

      Console.WriteLine($"{nameof(nbProcessedTasks)}={nbProcessedTasks}");
      Console.WriteLine($"{nameof(nbSubTasks)}={nbSubTasks}");
      Console.WriteLine($"{nameof(totalCalculationTime)}={totalCalculationTime}");
      Console.WriteLine($"{nameof(result)}={result}s");
    }

    [Test]
    public void ProcessRequestLargeTest()
    {
      var config = RunConfiguration.Large;
      
      var runner = new LocalRequestRunner(config);

      var nbProcessedTasks           = 0;
      var totalCalculationTime = new TimeSpan();

      runner.StartRequestProcessingEvent += nestedRequest =>
                                      {
                                        nbProcessedTasks++;
                                        totalCalculationTime += new TimeSpan(nestedRequest.DurationMs * 10000);
                                      };

      var nbSubTasks = 0;
      runner.SpawningRequestEvent += (localNbSubtasks, depth) =>
                                     {
                                       nbSubTasks += localNbSubtasks;
                                       if (PrintSubtasks)
                                         Console.WriteLine($"{nameof(localNbSubtasks)}={localNbSubtasks} ({nameof(depth)}={depth})");
                                     };

      var request = config.DefaultHeadRequest();

      var result = runner.ProcessRequest(request);

      Console.WriteLine($"{nameof(nbProcessedTasks)}={nbProcessedTasks}");
      Console.WriteLine($"{nameof(nbSubTasks)}={nbSubTasks}");
      Console.WriteLine($"{nameof(totalCalculationTime)}={totalCalculationTime}");
      Console.WriteLine($"{nameof(result)}={result}s");
    }

    [Test]
    public void ProcessRequestXLargeTest()
    {
      var config = RunConfiguration.XLarge;
      
      var runner = new LocalRequestRunner(config);

      var nbProcessedTasks     = 0;
      var totalCalculationTime = new TimeSpan();

      runner.StartRequestProcessingEvent += nestedRequest =>
                                            {
                                              nbProcessedTasks++;
                                              totalCalculationTime += new TimeSpan(nestedRequest.DurationMs * 10000);
                                            };

      var nbSubTasks = 0;
      runner.SpawningRequestEvent += (localNbSubtasks, depth) =>
                                     {
                                       nbSubTasks += localNbSubtasks;
                                       if (PrintSubtasks)
                                         Console.WriteLine($"{nameof(localNbSubtasks)}={localNbSubtasks} ({nameof(depth)}={depth})");
                                     };

      var request = config.DefaultHeadRequest();

      var result = runner.ProcessRequest(request);

      Console.WriteLine($"{nameof(nbProcessedTasks)}={nbProcessedTasks}");
      Console.WriteLine($"{nameof(nbSubTasks)}={nbSubTasks}");
      Console.WriteLine($"{nameof(totalCalculationTime)}={totalCalculationTime}");
      Console.WriteLine($"{nameof(result)}={result}s");
    }
  }
}