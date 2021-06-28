// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 


using System;
using System.Collections.Generic;
using System.Linq;

using Htc.Mock.Common;

using JetBrains.Annotations;

namespace Htc.Mock.GridLib
{
  [PublicAPI]
  public class DistributedRequestProcessor
  {
    private readonly RequestProcessor requestProcessor_;

    /// <summary>
    /// Builds a <c>DistributedRequestProcessor</c>
    /// </summary>
    /// <param name="runConfiguration">Defines the properties of the current execution.
    /// It is assumed to be a session data.</param>
    /// <param name="fastCompute">Defines if the execution time should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="useLowMem">Defines if the memory consumption should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="smallOutput">Defines if the output size should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    public DistributedRequestProcessor(RunConfiguration runConfiguration,
                                bool                   fastCompute = false,
                                bool                   useLowMem   = false,
                                bool                   smallOutput = false)
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
        return;
      }

      foreach (var subRequest in result.SubRequests)
      {
        // In a real implementation, the two methods could be merged
        // the separation is made to emphasize the two cases to handle
        if (subRequest.ResultIdsRequired.Any())
          SubmitNewRequestWithDependencies(request, subRequest.ResultIdsRequired);
        else
          SubmitNewRequest(request);
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
