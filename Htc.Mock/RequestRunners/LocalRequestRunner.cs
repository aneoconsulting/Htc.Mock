/* LocalRequestRunner.cs is part of the Htc.Mock solution.
    
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
using System.Text;
using System.Threading.Tasks;

using Htc.Mock.Core;

using JetBrains.Annotations;

namespace Htc.Mock.RequestRunners
{

  [PublicAPI]
  public class LocalRequestRunner : IRequestRunner
  {

    private readonly RequestProcessor requestProcessor;

    private readonly ConcurrentDictionary<string, string> results = new ConcurrentDictionary<string, string>();

    public event Action<Request> StartRequestProcessingEvent;

    public event Action<int> SpawningRequestEvent;

    public bool ParallelRun { get; set; } = true;

    /// <summary>
    /// Builds a <c>DistributedRequestProcessor</c>
    /// </summary>
    /// <param name="runConfiguration">Defines the properties of the current execution.
    /// It is assumed to be a session data.</param>
    public LocalRequestRunner(RunConfiguration runConfiguration)
      => requestProcessor = new RequestProcessor(true, true, true, runConfiguration);


    public string ProcessRequest(Request request, bool parallelRun)
    {
      switch (request)
      {
        case FinalRequest finalRequest:
        {
          StartRequestProcessingEvent?.Invoke(request);
          var result = requestProcessor.GetResult(finalRequest, Array.Empty<string>());

          if (!results.TryAdd(request.Id, result.Result))
          {
            Console.WriteLine("[Htc.Mock] Result was already written.");
          }

          return result.Result;
        }

        case AggregationRequest aggregationRequest:
        {
          StartRequestProcessingEvent?.Invoke(request);
          var result = requestProcessor.GetResult(aggregationRequest, aggregationRequest.ResultIdsRequired
                                                                                         .Select(id => results[id])
                                                                                         .ToList());
          if (!results.TryAdd(aggregationRequest.Id, result.Result))
          {
            Console.WriteLine("[Htc.Mock]  was already written.");
          }          
          if (!results.TryAdd(aggregationRequest.ParentId, result.Result))
          {
            Console.WriteLine("[Htc.Mock] Result was already written.");
          }

          return result.Result;
        }

        case ComputeRequest computeRequest:
        {
          StartRequestProcessingEvent?.Invoke(request);
          var result = requestProcessor.GetResult(computeRequest, Array.Empty<string>());


          AggregationRequest aggregationRequest = null;

          var dependencyRequests = result.SubRequests.Where(r =>
                                                            {
                                                              if (!(r is AggregationRequest ar))
                                                                return true;

                                                              aggregationRequest = ar;
                                                              return false;
                                                            });

          if (parallelRun)
          {
            Parallel.ForEach(dependencyRequests,
                             leafRequest =>
                             {
                               SpawningRequestEvent?.Invoke(1);
                               ProcessRequest(leafRequest, request.Id);
                             });
          }
          else
          {
            foreach (var leafRequest in dependencyRequests)
            {
              SpawningRequestEvent?.Invoke(1);
              ProcessRequest(leafRequest, ParallelRun);
            }
          }

          SpawningRequestEvent?.Invoke(1);
          ProcessRequest(aggregationRequest, parallelRun);

          return results[request.Id];
        }

        default:
          throw new ArgumentException($"{typeof(Request)} != supported.");

      }
    }

    /// <inheritdoc />
    public byte[] ProcessRequest(Request request, string taskId) => Encoding.ASCII.GetBytes(ProcessRequest(request, ParallelRun));
  }
}
