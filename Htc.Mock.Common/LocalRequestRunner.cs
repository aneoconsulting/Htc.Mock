/* LocalRequestRunner.cs is part of the Htc.Mock.Common solution.
    
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
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Htc.Mock.Common
{

  [PublicAPI]
  public class LocalRequestRunner
  {

    private readonly RequestProcessor requestProcessor_;

    private readonly ConcurrentDictionary<string, string> results_ = new();

    public event Action<Request> StartRequestProcessingEvent;

    public event Action<int> SpawningRequestEvent;

    /// <summary>
    /// Builds a <c>DistributedRequestProcessor</c>
    /// </summary>
    /// <param name="runConfiguration">Defines the properties of the current execution.
    /// It is assumed to be a session data.</param>
    public LocalRequestRunner(RunConfiguration runConfiguration)
      => requestProcessor_ = new RequestProcessor(true, true, true, runConfiguration);


    public string ProcessRequest(Request request, bool parallelRun = true)
    {
      switch (request)
      {
        case FinalRequest finalRequest:
        {
          StartRequestProcessingEvent?.Invoke(request);
          var result = requestProcessor_.GetResult(finalRequest, Array.Empty<string>());

          if (!results_.TryAdd(request.Id, result.Result))
          {
            Console.WriteLine("Result was already written.");
          }

          return result.Result;
        }

        case AggregationRequest aggregationRequest:
        {
          StartRequestProcessingEvent?.Invoke(request);
          var result = requestProcessor_.GetResult(aggregationRequest, aggregationRequest.ResultIdsRequired
                                                                                         .Select(id => results_[id])
                                                                                         .ToList());
          if (!results_.TryAdd(aggregationRequest.Id, result.Result))
          {
            Console.WriteLine("Result was already written.");
          }          
          if (!results_.TryAdd(aggregationRequest.ParentId, result.Result))
          {
            Console.WriteLine("Result was already written.");
          }

          return result.Result;
        }

        case ComputeRequest computeRequest:
        {
          StartRequestProcessingEvent?.Invoke(request);
          var result = requestProcessor_.GetResult(computeRequest, Array.Empty<string>());

          var subRequestsByDepsRq = result.SubRequests
                                          .ToLookup(sr => sr is not AggregationRequest);

          SpawningRequestEvent?.Invoke(subRequestsByDepsRq[true].Count());

          if (parallelRun)
          {
            Parallel.ForEach(subRequestsByDepsRq[true], leafRequest => ProcessRequest(leafRequest));
          }
          else
          {
            foreach (var leafRequest in subRequestsByDepsRq[true])
              ProcessRequest(leafRequest);
          }

          var aggregationRequest = subRequestsByDepsRq[false].Single();

          SpawningRequestEvent?.Invoke(1);

          ProcessRequest(aggregationRequest);

          return results_[request.Id];
        }

        default:
          throw new ArgumentException($"{typeof(Request)} is not supported.");

      }
    }
  }
}
