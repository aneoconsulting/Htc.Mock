/* RequestProcessorTests.cs is part of the Htc.Mock.Common.Tests solution.
    
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

using NUnit.Framework;
using Htc.Mock.Common;

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

      var request = new FinalRequest();

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
      var configuration    = new RunConfiguration(new TimeSpan(0, 0, 1), 2, 1, 1, 1, 1);
      var requestProcessor = new RequestProcessor(fastCompute, useLowMem, smallOutput, configuration);

      var deps     = new[] {"Id1", "Id2"};

      var request = new AggregationRequest("AggregateTest", "ParentTest", 1, deps);

      var inputs           = new[] {"Id1_result", "Id2_result"};

      var requestResult = requestProcessor.GetResult(request, inputs);

      var res = inputs.GetCryptoHashCode();

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
      var configuration    = new RunConfiguration(new TimeSpan(0, 0, 1), 2, 1, 1, 1);
      var requestProcessor = new RequestProcessor(fastCompute, useLowMem, smallOutput, configuration);

      var request = new ComputeRequest("NestTest", 1, 2);

      var requestResult = requestProcessor.GetResult(request, Array.Empty<string>());

      Assert.IsFalse(requestResult.HasResult);
      Assert.AreEqual(2, requestResult.SubRequests.Count);
      Assert.IsTrue(string.IsNullOrEmpty(requestResult.Result));
      Assert.AreEqual($"NestTest_0", requestResult.SubRequests[0].Id);

      // This task is an aggregation
      Assert.IsInstanceOf<AggregationRequest>(requestResult.SubRequests[1]);
      Assert.IsTrue(((AggregationRequest)requestResult.SubRequests[1]).ResultIdsRequired.Count == 1);
      var res = "NestTest_0_result".GetCryptoHashCode(); 
      Assert.AreEqual($"Aggregate_{res}", requestResult.SubRequests[1].Id);
    }
  }
}
