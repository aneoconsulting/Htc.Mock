using System;

using NUnit.Framework;

namespace Htc.Mock.Common.Tests
{
  [TestFixture]
  public class RequestProcessorTests
  {
    [Test]
    [TestCase(true, true, true)]
    [TestCase(true, true, false)]
    [TestCase(true, false, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    [TestCase(false, false, false)]
    public void GetResultHeadTest(bool fastCompute, bool useLowMem, bool smallOutput)
    {
      var configuration    = new RunConfiguration(new TimeSpan(0, 0, 1), 0, 1, 1, 0);
      var requestProcessor = new RequestProcessor(fastCompute, useLowMem, smallOutput, configuration);

      var request = new Request(1, 1, 1, 0);

      var requestResult = requestProcessor.GetResult(request, Array.Empty<string>());

      Assert.IsTrue(requestResult.HasResult);
      Assert.IsTrue(requestResult.SubRequests.Count == 0);
      Assert.AreEqual("HeadId_result", requestResult.Result);
      Assert.AreEqual("HeadId", requestResult.RequestId);
    }

    [Test]
    [TestCase(true, true, true)]
    [TestCase(true, true, false)]
    [TestCase(true, false, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    [TestCase(false, false, false)]
    public void GetResultAggregateTest(bool fastCompute, bool useLowMem, bool smallOutput)
    {
      var configuration    = new RunConfiguration(new TimeSpan(0, 0, 1), 2, 1, 1, 1, minSubTasks:1);
      var requestProcessor = new RequestProcessor(fastCompute, useLowMem, smallOutput, configuration);

      var deps     = new[] {"Id1", "Id2"};

      var request = new Request("AggregateTest",1, 1, 1, deps, "ParentTest",2,1);

      var inputs           = new[] {"Id1_result", "Id2_result"};

      var requestResult = requestProcessor.GetResult(request, inputs);

      var res = HashCode.Combine(0, inputs[0]);
      res = HashCode.Combine(res, inputs[1]);

      Assert.IsTrue(requestResult.HasResult);
      Assert.IsTrue(requestResult.SubRequests.Count == 0);
      Assert.AreEqual($"Aggregate_{res}_result", requestResult.Result);
      Assert.AreEqual("AggregateTest", requestResult.RequestId);
    }

    [Test]
    [TestCase(true, true, true)]
    [TestCase(true, true, false)]
    [TestCase(true, false, true)]
    [TestCase(true, false, false)]
    [TestCase(false, true, true)]
    [TestCase(false, true, false)]
    [TestCase(false, false, true)]
    [TestCase(false, false, false)]
    public void GetResultNestedTest(bool fastCompute, bool useLowMem, bool smallOutput)
    {
      var configuration    = new RunConfiguration(new TimeSpan(0, 0, 1), 2, 1, 1, 1, subTaskFraction:1,minSubTasks:1);
      var requestProcessor = new RequestProcessor(fastCompute, useLowMem, smallOutput, configuration);

      var request = new Request("NestTest", 1, 1, 1,1,0);

      var requestResult = requestProcessor.GetResult(request, Array.Empty<string>());

      Assert.IsFalse(requestResult.HasResult);
      Assert.AreEqual(2, requestResult.SubRequests.Count);
      Assert.IsTrue(string.IsNullOrEmpty(requestResult.Result));
      Assert.IsTrue(((Request)requestResult.SubRequests[0]).ResultIdsRequired.Count==0);
      Assert.AreEqual($"NestTest_0", requestResult.SubRequests[0].Id);

      // This task is an aggregation
      Assert.IsTrue(((Request)requestResult.SubRequests[1]).ResultIdsRequired.Count == 1);
      var res = HashCode.Combine(0, "NestTest_0_result"); // Do not hard-code in case HashCode.Combine changes someday
      Assert.AreEqual($"Aggregate_{res}", requestResult.SubRequests[1].Id);
    }
  }
}
