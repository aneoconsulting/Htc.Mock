using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Htc.Mock.Common
{

  [PublicAPI]
  public class LocalRequestRunner
  {
    public bool ParallelRun { get; set; } = false;

    private readonly RequestProcessor requestProcessor_;

    private readonly ConcurrentDictionary<string, string> results_ = new();

    public event Action<Request> StartRequestProcessingEvent;

    public event Action<int, int> SpawningRequestEvent;

    /// <summary>
    /// Builds a <c>DistributedRequestProcessor</c>
    /// </summary>
    /// <param name="runConfiguration">Defines the properties of the current execution.
    /// It is assumed to be a session data.</param>
    public LocalRequestRunner(RunConfiguration runConfiguration)
      => requestProcessor_ = new RequestProcessor(true, true, true, runConfiguration);


    public string ProcessRequest(Request request)
    {
      StartRequestProcessingEvent?.Invoke(request);
      var inputs = request.ResultIdsRequired
                          .Select(id => results_[id])
                          .ToList();

      var result = requestProcessor_.GetResult(request, inputs);

      if (result.HasResult)
      {
        if (request.IsAggregationRequest)
        {
          if (!results_.TryAdd(request.ParentId, result.Result))
          {
            Console.WriteLine("Result was already written.");
          }
        }
        else
        {
          if (!results_.TryAdd(request.Id, result.Result))
          {
            Console.WriteLine("Result was already written.");
          }
        }

        return result.Result;
      }

      var subRequestsByDepsRq = result.SubRequests
                                      .Cast<Request>()
                                      .ToLookup(sr => sr.ResultIdsRequired.Count == 0);


      SpawningRequestEvent?.Invoke(subRequestsByDepsRq[true].Count(), request.CurrentDepth);

      if (ParallelRun)
      {
        Parallel.ForEach(subRequestsByDepsRq[true], leafRequest => ProcessRequest(leafRequest));
      }
      else
      {
        foreach (var leafRequest in subRequestsByDepsRq[true])
            ProcessRequest(leafRequest);
      }

      var aggregationRequest = subRequestsByDepsRq[false].Single();

      ProcessRequest(aggregationRequest);

      return results_[request.Id];
    }
  }
}
