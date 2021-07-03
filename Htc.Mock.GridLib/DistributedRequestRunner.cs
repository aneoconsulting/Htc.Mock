/* DistributedRequestRunner.cs is part of the Htc.Mock.GridLib solution.
    
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
using System.Linq;

using Htc.Mock.Common;

using JetBrains.Annotations;

namespace Htc.Mock.GridLib
{
  [PublicAPI]
  public class DistributedRequestRunner
  {
    private readonly RequestProcessor requestProcessor_;

    /// <summary>
    /// Builds a <c>DistributedRequestRunner</c>
    /// </summary>
    /// <param name="runConfiguration">Defines the properties of the current execution.
    /// It is assumed to be a session data.</param>
    /// <param name="fastCompute">Defines if the execution time should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="useLowMem">Defines if the memory consumption should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="smallOutput">Defines if the output size should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    public DistributedRequestRunner(RunConfiguration runConfiguration,
                                bool                   fastCompute = false,
                                bool                   useLowMem   = false,
                                bool                   smallOutput = false)
      => requestProcessor_ = new RequestProcessor(fastCompute, useLowMem, smallOutput, runConfiguration);

    public void ProcessRequest(Request request)
    {

      switch (request)
      {
        case FinalRequest finalRequest:
        {
          var result = requestProcessor_.GetResult(finalRequest, Array.Empty<string>());
          StoreResult(request.Id, result.Result, result.Output);
          return;
        }

        case AggregationRequest aggregationRequest:
        {
          var result = requestProcessor_.GetResult(aggregationRequest, aggregationRequest.ResultIdsRequired
                                                                                         .Select(GetResultFromId)
                                                                                         .ToList());

          StoreResult(request.Id, result.Result, result.Output);
          StoreResult(aggregationRequest.ParentId, result.Result, result.Output);
          return;
        }

        case ComputeRequest computeRequest:
        {
          var result = requestProcessor_.GetResult(computeRequest, Array.Empty<string>());

          var subRequestsByDepsRq = result.SubRequests
                                          .ToLookup(sr => sr is AggregationRequest);


            foreach (var leafRequest in subRequestsByDepsRq[true])
              SubmitNewRequest(leafRequest);

          var aggregationRequest = subRequestsByDepsRq[false].Cast<AggregationRequest>().Single();

          SubmitNewRequestWithDependencies(aggregationRequest, aggregationRequest.ResultIdsRequired);
          return;
        }

        default:
          throw new ArgumentException($"{typeof(Request)} is not supported.");

      }
    }

    private string GetResultFromId(string id)
    {
      // TODO: IMPLEMENT_ME
      throw new NotImplementedException();
    }

    private void StoreResult(string Id, string result, byte[] output)
    {
      // TODO: IMPLEMENT_ME
      throw new NotImplementedException();
    }

    private void SubmitNewRequest(Request request)
    {
      // TODO: IMPLEMENT_ME
      throw new NotImplementedException();
    }

    private void SubmitNewRequestWithDependencies(Request request, IList<string> dependencies)
    {
      // TODO: IMPLEMENT_ME
      throw new NotImplementedException();
    }
  }
}
