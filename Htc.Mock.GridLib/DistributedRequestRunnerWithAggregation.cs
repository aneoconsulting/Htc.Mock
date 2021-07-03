// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

using System;
using System.Linq;

using Htc.Mock.Common;

using JetBrains.Annotations;

namespace Htc.Mock.GridLib
{
  [PublicAPI]
  public class DistributedRequestRunnerWithAggregation
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
    public DistributedRequestRunnerWithAggregation(RunConfiguration runConfiguration,
                                                   bool             fastCompute = false,
                                                   bool             useLowMem   = false,
                                                   bool             smallOutput = false)
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

          foreach (var leafRequest in subRequestsByDepsRq[true])
            WaitForResult(leafRequest);

          ProcessRequest(aggregationRequest);
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

    private void StoreResult(string id, string result, byte[] output)
    {
      // TODO: IMPLEMENT_ME
      throw new NotImplementedException();
    }

    private void SubmitNewRequest(Request request)
    {
      // TODO: IMPLEMENT_ME
      throw new NotImplementedException();
    }

    private void WaitForResult(Request request)
    {
      // TODO: IMPLEMENT_ME
      throw new NotImplementedException();
    }
  }
}
