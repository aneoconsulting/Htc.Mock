// RequestProcessorTests.cs is part of the Htc.Mock solution.
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

using System.Collections.Concurrent;
using System.Linq;

using Htc.Mock.Core;

using Microsoft.Extensions.Logging.Abstractions;

using NUnit.Framework;

namespace Htc.Mock.Tests
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
    public void NoSubLevelTest(bool fastCompute, bool useLowMem, bool smallOutput)
    {
      var configuration    = new RunConfiguration(new(0, 0, 1), 1, 1, 1, 0);
      var requestProcessor = new RequestProcessor(fastCompute, useLowMem, smallOutput, configuration, NullLogger.Instance);

      var request = configuration.BuildRequest(out _, NullLogger.Instance);

      var requestAnswer = requestProcessor.GetResult(request, new ConcurrentDictionary<string, string>());

      Assert.IsTrue(requestAnswer.Result.HasResult);
      Assert.IsFalse(requestAnswer.SubRequests.Any());
      Assert.AreEqual("1", requestAnswer.Result.Value);
    }


    [Test]
    public void TwoLevelTest()
    {
      var configuration    = new RunConfiguration(new(0, 0, 1), 1, 1, 1, 1);
      var requestProcessor = new RequestProcessor(true, true, true, configuration, NullLogger.Instance);

      var request = new ComputeRequest("root", new Tree(new(new[]
                                                       {
                                                         true,
                                                         true,
                                                         false,
                                                         true,
                                                         false,
                                                         false,
                                                       })));

      var requestAnswer = requestProcessor.GetResult(request, new ConcurrentDictionary<string, string>());

      Assert.IsFalse(requestAnswer.Result.HasResult);
      Assert.IsTrue(requestAnswer.SubRequests.Any());
      Assert.AreEqual("root", requestAnswer.RequestId);
      Assert.AreEqual(requestAnswer.SubRequests.Last().Id, requestAnswer.Result.Value);
      Assert.AreEqual("root.2", requestAnswer.Result.Value);
    }
  }
}
