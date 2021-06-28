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