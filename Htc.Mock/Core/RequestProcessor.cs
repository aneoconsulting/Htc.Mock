/* RequestProcessor.cs is part of the Htc.Mock solution.
    
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Htc.Mock.Utils;

using JetBrains.Annotations;

namespace Htc.Mock.Core
{
  [PublicAPI]
  public class RequestProcessor
  {
    private readonly RunConfiguration runConfiguration_;
    private readonly bool             fastCompute_;
    private readonly bool             useLowMem_;
    private readonly bool             smallOutput_;

    public RequestProcessor(bool fastCompute, bool useLowMem, bool smallOutput, RunConfiguration runConfiguration) 
    {
      fastCompute_      = fastCompute;
      useLowMem_        = useLowMem;
      smallOutput_      = smallOutput;
      runConfiguration_ = runConfiguration;
    }


    [PublicAPI]
    public RequestResult GetResult(Request request, IList<string> inputs)
    {
      Debug.Assert(inputs is not null);
      return ComputeResult(request, inputs).WithOutput(EmulateComputation(request));
    }

    protected static RequestResult ComputeResult(AggregationRequest request, IList<string> inputs)
    {
      Debug.Assert(inputs is not null);

      if (request.ResultIdsRequired.Count != inputs.Count)
        throw new
          ArgumentException($"{nameof(request)} requires {request.ResultIdsRequired.Count} inputs, {inputs.Count} provided.",
                            nameof(inputs));

      // use this computation to check that the good results were retrieved
      var result = GetResultString(GetAggregateString(GetAggregationRes(inputs)));

      return new RequestResult(request.Id, result);
    }


    protected static RequestResult ComputeResult(FinalRequest request, IList<string> inputs)
    {
      if (inputs.Any())
        throw new
          ArgumentException($"{nameof(request)} requires no inputs, {inputs.Count} provided.", nameof(inputs));

      return new RequestResult(request.Id, GetResultString(request.Id));
    }

    protected static RequestResult ComputeResult(ComputeRequest request, IList<string> inputs)
    {
      if (inputs.Any())
        throw new
          ArgumentException($"{nameof(request)} requires no inputs, {inputs.Count} provided.", nameof(inputs));

      var targetNbRq        = Math.Max(2, (int)Math.Pow(request.NbSubrequests, 1.0 / request.Depth));
      if (request.NbSubrequests - targetNbRq < 2)
        targetNbRq = request.NbSubrequests;
      var remainingRequests = request.NbSubrequests - targetNbRq;
      var subRequests       = new List<Request>(targetNbRq);

      for (var i = 0; i < targetNbRq - 2; ++i)
      {
        var nbSubReq = remainingRequests switch
                       {
                         0 => 0,
                         1 => throw new InvalidOperationException(),
                         2 => 2,
                         _ => 2 + (int) (request.Id.GetCryptoHashCode() % (remainingRequests -2)),
                       };
        if (remainingRequests - nbSubReq < 2)
          nbSubReq = remainingRequests;

        subRequests.Add(nbSubReq == 0
                          ? new FinalRequest($"{request.Id}_{i}")
                          : new ComputeRequest($"{request.Id}_{i}",
                                               request.Depth - 1,
                                               nbSubReq));
        remainingRequests -= nbSubReq;
      }

      subRequests.Add(remainingRequests == 0
                        ? new FinalRequest($"{request.Id}_{targetNbRq - 2}")
                        : new ComputeRequest($"{request.Id}_{targetNbRq - 2}",
                                             request.Depth - 1,
                                             remainingRequests));

      var subRequestIds = subRequests.Select(sr => sr.Id).ToList();

      Debug.Assert(1 +
                   subRequests.Count +
                   subRequests.Where(r => r is ComputeRequest)
                              .Cast<ComputeRequest>()
                              .Sum(cr => cr.NbSubrequests) ==
                   request.NbSubrequests);

      var aggregateString = GetAggregateString(GetAggregationRes(subRequestIds, GetResultString));

      var aggregationRequest =
        new AggregationRequest(aggregateString,
                               request.Id,
                               request.Depth-1,
                               subRequestIds);
      subRequests.Add(aggregationRequest);
      return new RequestResult(request.Id, subRequests);

    }

    protected static RequestResult ComputeResult(Request request, IList<string> inputs)
    {
      return request switch
             {
               AggregationRequest aggregationRequest => ComputeResult(aggregationRequest, inputs),
               ComputeRequest computeRequest         => ComputeResult(computeRequest, inputs),
               FinalRequest finalRequest             => ComputeResult(finalRequest, inputs),
               _                                     => throw new ArgumentException($"{typeof(Request)} request cannot be handled.")
             };
    }

    public static string GetResultString(string taskId) => $"{taskId}_result";

    public static uint GetAggregationRes(IEnumerable<string> ids, Func<string, string> resultSelector) 
      => GetAggregationRes(ids.Select(resultSelector));

    public static uint GetAggregationRes(IEnumerable<string> results) => results.GetCryptoHashCode();

    public static string GetAggregateString(uint res) => $"Aggregate_{res}";

    private byte[] EmulateComputation(Request request)
    {
      var t = System.Threading.Tasks.Task.Delay(fastCompute_?0: runConfiguration_.GetTaskDurationMs(request.Id));

      var m = useLowMem_ ? new byte[0, 0] : new byte[runConfiguration_.Memory, 1024];
      // Write all bytes to ensure that the memory is really allocated
      for (var i = 0; i < m.GetLength(0); ++i)
      {
        for (var j = 0; j < m.GetLength(1) ; ++j)
        {
          m[i, j] = (byte)(m.GetLength(0) * j - 2019 * i);
        }
      }

      var output = new byte[smallOutput_ ? 0 : runConfiguration_.Data];
      for (var i = 0; i < output.Length; ++i)
      {
        output[i] = (byte)((runConfiguration_.Data & 2019) * i);
      }

      t.Wait();

      return output;
    }
  }
}
