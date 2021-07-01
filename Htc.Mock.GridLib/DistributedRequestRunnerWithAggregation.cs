// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

using System;
using System.Collections.Generic;
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
      var inputs = request.ResultIdsRequired
                          .Select(GetResultFromId)
                          .ToList();

      var result = requestProcessor_.GetResult(request, inputs);

      if (result.HasResult)
      {
        StoreResult(request.Id, result.Result, result.Output);
        if(request.IsAggregationRequest)
          StoreResult(request.ParentId, result.Result, result.Output);
        return;
      } 
      // Result contains a set of subrequests

      var subRequestsByDepsRq = result.SubRequests
                                      .Cast<Request>()
                                      .ToLookup(sr => sr.IsAggregationRequest);

      foreach (var subRequest in subRequestsByDepsRq[false])
      {
        SubmitNewRequest(request);
      }

      // TODO: Wait fot the tasks termination

      // When the dependencies request have been processed, 
      // current task will process the aggregation request.
      ProcessRequest(subRequestsByDepsRq[true].Single());
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
