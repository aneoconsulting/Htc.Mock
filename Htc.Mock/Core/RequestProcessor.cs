// RequestProcessor.cs is part of the Htc.Mock solution.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Htc.Mock.Utils;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace Htc.Mock.Core
{
  [PublicAPI]
  public class RequestProcessor
  {
    private readonly bool             fastCompute_;
    private readonly ILogger          logger_;
    private readonly RunConfiguration runConfiguration_;
    private readonly bool             smallOutput_;
    private readonly bool             useLowMem_;

    public RequestProcessor(bool                       fastCompute,
                            bool                       useLowMem,
                            bool                       smallOutput,
                            [NotNull] RunConfiguration runConfiguration,
                            [NotNull] ILogger          logger)
    {
      fastCompute_      = fastCompute;
      useLowMem_        = useLowMem;
      smallOutput_      = smallOutput;
      runConfiguration_ = runConfiguration ?? throw new ArgumentNullException(nameof(runConfiguration));
      logger_           = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    [PublicAPI]
    public RequestAnswer GetResult(Request request, IDictionary<string, string> inputs)
    {
      Debug.Assert(inputs != null);
      var output = EmulateComputation(request);
      var res    = ComputeResultDispatch(request, inputs);
      output.Wait();
      return res;
    }

    protected RequestAnswer ComputeResult(AggregationRequest request, IDictionary<string, string> inputs)
    {
      logger_.LogTrace($"{nameof(ComputeRequest)}: Processing a AggregationRequest.");
      Debug.Assert(inputs != null);

      var aggregator = new Dictionary<int, int>();

      foreach (var dependency in request.Dependencies)
      {
        var input     = inputs[dependency];
        var inputData = input.Split('.').Select(int.Parse).ToList();
        for (var i = 0; i < inputData.Count; i++)
          if (!aggregator.TryGetValue(i, out var value))
            aggregator[i] = inputData[i];
          else
            aggregator[i] += inputData[i];
      }

      // use this computation to check that the good results were retrieved
      var result = "1." + string.Join(".", aggregator.OrderBy(pair => pair.Key)
                                                     .Select(pair => pair.Value));
      if (logger_.IsEnabled(LogLevel.Information))
        logger_.LogInformation("AggregationRequest {id} computed {result}", request.Id, result);

      return new RequestAnswer(request.Id, result);
    }

    protected RequestAnswer ComputeResult(ComputeRequest request)
    {
      if (logger_.IsEnabled(LogLevel.Information))
        logger_.LogInformation("Start processing request {id} with tree shape {shape}",
                               request.Id,
                               string.Join(".", request.Tree.GetShape()));

      var subtrees = request.Tree.GetSubTrees().ToList();

      if (logger_.IsEnabled(LogLevel.Information) && subtrees.Any())
        logger_.LogInformation("Request {id} generated subtrees with shape {shapes}",
                               request.Id,
                               string.Join(" ", subtrees.Select(tree => tree.GetShapeString())));

      if (subtrees.Count == 0)
        return new RequestAnswer(request.Id, "1");

      var subIds      = new List<string>(subtrees.Count);
      var subRequests = new List<Request>(subtrees.Count + 1);
      for (var i = 0; i < subtrees.Count; i++)
      {
        var id = $"{request.Id}.{i}";
        var r  = new ComputeRequest(id, subtrees[i]);
        subIds.Add(id);
        subRequests.Add(r);
      }

      subRequests.Add(new AggregationRequest($"{request.Id}.{subtrees.Count}", subIds));

      return new RequestAnswer(request.Id, $"{request.Id}.{subtrees.Count}", subRequests);
    }

    protected RequestAnswer ComputeResultDispatch(Request request, IDictionary<string, string> inputs)
    {
      switch (request)
      {
        case AggregationRequest aggregationRequest:
          return ComputeResult(aggregationRequest, inputs);
        case ComputeRequest computeRequest:
          return ComputeResult(computeRequest);
        default:
          throw new ArgumentException($"{typeof(Request)} request cannot be handled.");
      }
    }

    public static string GetResultString(string taskId) => $"{taskId}_result";

    public static uint GetAggregationRes(IEnumerable<string> ids, Func<string, string> resultSelector)
      => GetAggregationRes(ids.Select(resultSelector));

    public static uint GetAggregationRes(IEnumerable<string> results) => results.GetCryptoHashCode();

    public static string GetAggregateString(uint res) => $"Aggregate_{res}";

    private async Task EmulateComputation(Request request)
    {
      var t = Task.Delay(fastCompute_ ? 0 : runConfiguration_.GetTaskDurationMs(request.Id));

      await Task.Run(() =>
                     {
                       var m = useLowMem_ ? new byte[0, 0] : new byte[runConfiguration_.Memory, 1024];
                       // Write all bytes to ensure that the memory is really allocated
                       for (var i = 0; i < m.GetLength(0); ++i)
                       for (var j = 0; j < m.GetLength(1); ++j)
                         m[i, j] = (byte)((m.GetLength(0) * j) - (2019 * i));

                       var output                                        = new byte[smallOutput_ ? 0 : runConfiguration_.Data];
                       for (var i = 0; i < output.Length; ++i) output[i] = (byte)((runConfiguration_.Data & 2019) * i);

                       return t;
                     });
      await t;
    }
  }
}
