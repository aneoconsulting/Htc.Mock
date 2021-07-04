/* DistributedRequestRunnerWithAggregation.cs is part of the Htc.Mock solution.
    
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
using System.Linq;
using System.Text;

using Htc.Mock.Core;

using JetBrains.Annotations;

namespace Htc.Mock.RequestRunners
{
  [PublicAPI]
  public class DistributedRequestRunnerWithAggregation : IRequestRunner
  {
    private readonly RunConfiguration runConfiguration_;
    private readonly RequestProcessor requestProcessor_;
    private readonly IDataClient      dataClient_;
    private readonly IGridClient      gridClient_;
    private readonly string           session_;

    /// <summary>
    /// Builds a <c>DistributedRequestRunner</c>
    /// </summary>
    /// <param name="dataClient"></param>
    /// <param name="gridClient"></param>
    /// <param name="runConfiguration">Defines the properties of the current execution.
    /// It is assumed to be a session data.</param>
    /// <param name="session"></param>
    /// <param name="fastCompute">Defines if the execution time should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="useLowMem">Defines if the memory consumption should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="smallOutput">Defines if the output size should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    public DistributedRequestRunnerWithAggregation(IDataClient      dataClient,
                                                   IGridClient      gridClient,
                                                   RunConfiguration runConfiguration,
                                                   string           session,
                                                   bool             fastCompute      = false,
                                                   bool             useLowMem        = false,
                                                   bool             smallOutput      = false)
    {
      runConfiguration_ = runConfiguration;
      requestProcessor_ = new RequestProcessor(fastCompute, useLowMem, smallOutput, runConfiguration);
      dataClient_       = dataClient;
      gridClient_       = gridClient;
      session_          = session;
    }

    public byte[] ProcessRequest(Request request, string taskId)
    {
      switch (request)
      {
        case FinalRequest finalRequest:
        {
          var result = requestProcessor_.GetResult(finalRequest, Array.Empty<string>());
          dataClient_.StoreData(request.Id, Encoding.ASCII.GetBytes(result.Result));

          return result.Output;
        }

        case AggregationRequest aggregationRequest:
        {
          var result = requestProcessor_.GetResult(aggregationRequest, aggregationRequest.ResultIdsRequired
                                                                                         .Select(dataClient_.GetData)
                                                                                         .Select(Encoding.ASCII.GetString)
                                                                                         .ToList());
          var data = Encoding.ASCII.GetBytes(result.Result);
          dataClient_.StoreData(aggregationRequest.Id, data);
          dataClient_.StoreData(aggregationRequest.ParentId, data);

          return result.Output;
        }

        case ComputeRequest computeRequest:
        {
          var result = requestProcessor_.GetResult(computeRequest, Array.Empty<string>());

          var subRequestsByDepsRq = result.SubRequests
                                          .ToLookup(sr => sr is not AggregationRequest);

          var subtaskIds = subRequestsByDepsRq[true]
                          .AsParallel()
                          .Select(leafRequest
                                    => gridClient_.SubmitSubtask(session_, taskId,
                                                                 DataAdapter.BuildPayload(runConfiguration_, leafRequest)))
                          .ToList();

          // We split the waiting in two to ease the scheduling by the parallel runtime
          foreach (var subtaskId in subtaskIds)
          {
            gridClient_.WaitCompletion(taskId);
          }
          foreach (var subtaskId in subtaskIds)
          {
            gridClient_.WaitSubtasksCompletion(taskId);
            gridClient_.GetResult(subtaskId);
          }


          var aggregationRequest = subRequestsByDepsRq[false].Cast<AggregationRequest>().Single();
          return ProcessRequest(aggregationRequest, session_);
        }

        default:
          throw new ArgumentException($"{typeof(Request)} is not supported.");
      }
    }
  }
}
