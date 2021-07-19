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
using System.Threading.Tasks;

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
    public RequestResult GetResult(Request request, IEnumerable<string> inputs)
    {
      Debug.Assert(inputs != null);
      var output = EmulateComputation(request);
      var res1   = ComputeResult(request, inputs);
      output.Wait();
      return res1.WithOutput(output.Result);
    }

    protected static RequestResult ComputeResult(AggregationRequest request, IEnumerable<string> inputs)
    {
      Console.WriteLine($"{nameof(ComputeRequest)}: Processing a AggregationRequest.");
      Debug.Assert(inputs != null);

      var nbInputs   = 0;
      var inputsList = inputs.Select(i => 
                                     { 
                                       nbInputs++;
                                       return i;
                                     });

      // use this computation to check that the good results were retrieved
      var result = GetResultString(GetAggregateString(GetAggregationRes(inputsList)));

      if (request.ResultIdsRequired.Count != nbInputs)
        throw new
          ArgumentException($"{nameof(request)} requires {request.ResultIdsRequired.Count} inputs, {nbInputs} provided.",
                            nameof(inputs));

      return new RequestResult(request.Id, result);
    }


    protected static RequestResult ComputeResult(FinalRequest request, IList<string> inputs)
    {
      Console.WriteLine($"{nameof(ComputeRequest)}: Processing a FinalRequest.");
      if (inputs.Any())
        throw new
          ArgumentException($"{nameof(request)} requires no inputs, {inputs.Count} provided.", nameof(inputs));

      return new RequestResult(request.Id, GetResultString(request.Id));
    }

    protected static RequestResult ComputeResult(ComputeRequest request, IList<string> inputs)
    {
      Console.WriteLine($"{nameof(ComputeRequest)}: Processing a ComputeRequest (it will generate subrequests).");
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
        int nbSubReq;
        switch (remainingRequests)
        {
          case 0:
            nbSubReq = 0;
            break;
          case 1:
            throw new InvalidOperationException();
          case 2:
            nbSubReq = 2;
            break;
          default:
            nbSubReq = 2 + (int) (request.Id.GetCryptoHashCode() % (remainingRequests - 2));
            break;
        }

        if (remainingRequests - nbSubReq < 2)
          nbSubReq = remainingRequests;

        subRequests.Add(nbSubReq == 0
                          ? (Request) new FinalRequest($"{request.Id}_{i}")
                          : new ComputeRequest($"{request.Id}_{i}",
                                               request.Depth - 1,
                                               nbSubReq));
        remainingRequests -= nbSubReq;
      }

      subRequests.Add(remainingRequests == 0
                        ? (Request) new FinalRequest($"{request.Id}_{targetNbRq - 2}")
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

      Console.WriteLine($"{nameof(ComputeRequest)}: {subRequests.Count} subrequests generated.");
      return new RequestResult(request.Id, subRequests);

    }

    protected static RequestResult ComputeResult(Request request, IEnumerable<string> inputs)
    {
      switch (request)
      {
        case AggregationRequest aggregationRequest:
          return ComputeResult(aggregationRequest, inputs);
        case ComputeRequest computeRequest:
          return ComputeResult(computeRequest, inputs);
        case FinalRequest finalRequest:
          return ComputeResult(finalRequest, inputs);
        default:
          throw new ArgumentException($"{typeof(Request)} request cannot be handled.");
      }
    }

    public static string GetResultString(string taskId) => $"{taskId}_result";

    public static uint GetAggregationRes(IEnumerable<string> ids, Func<string, string> resultSelector) 
      => GetAggregationRes(ids.Select(resultSelector));

    public static uint GetAggregationRes(IEnumerable<string> results) => results.GetCryptoHashCode();

    public static string GetAggregateString(uint res) => $"Aggregate_{res}";

    private async Task<byte[]> EmulateComputation(Request request)
    {
      var t = Task.Delay(fastCompute_?0: runConfiguration_.GetTaskDurationMs(request.Id));

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

      await t;

      return output;
    }
  }
}
