﻿/* DistributedRequestRunner.cs is part of the Htc.Mock solution.
    
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
  public class DistributedRequestRunner : IRequestRunner
  {
    private readonly RunConfiguration runConfiguration_;
    private readonly RequestProcessor requestProcessor_;
    private readonly IDataClient      dataClient_;
    private readonly IGridClient      gridClient_;
    private readonly string           session_;
    private readonly bool             waitDependencies_;

    /// <summary>
    /// Builds a <c>DistributedRequestRunner</c>. The lifecycle of the object is meant to
    /// corresponds to the life cycle of sessions (<see cref="GridWorker"/>).
    /// </summary>
    /// <param name="gridClient"></param>
    /// <param name="runConfiguration">Defines the properties of the current execution.
    /// It is assumed to be a session data.</param>
    /// <param name="waitDependencies"></param>
    /// <param name="fastCompute">Defines if the execution time should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="useLowMem">Defines if the memory consumption should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="smallOutput">Defines if the output size should be emulated.
    /// It is assumed to be a deployment configuration.</param>
    /// <param name="dataClient"></param>
    /// <param name="session"></param>
    public DistributedRequestRunner(IDataClient dataClient,
                                    IGridClient gridClient,
                                    RunConfiguration runConfiguration,
                                    string session,
                                    bool waitDependencies = false,
                                    bool fastCompute = false,
                                    bool useLowMem = false,
                                    bool smallOutput = false)
    {

      runConfiguration_      = runConfiguration;
      requestProcessor_      = new RequestProcessor(fastCompute, useLowMem, smallOutput, runConfiguration);
      dataClient_       = dataClient;
      gridClient_       = gridClient;
      session_          = session;
      waitDependencies_ = waitDependencies;
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

          var taskIds = subRequestsByDepsRq[true]
                       .AsParallel()
                       .Select(leafRequest
                                 => gridClient_.SubmitSubtask(session_, taskId, DataAdapter.BuildPayload(runConfiguration_, leafRequest)))
                       .ToList();

          var aggregationRequest = subRequestsByDepsRq[false].Cast<AggregationRequest>()
                                                             .Single();

          if (waitDependencies_)
            gridClient_.WaitDependenciesAndSubmitSubtask(session_, taskId, DataAdapter.BuildPayload(runConfiguration_, aggregationRequest),
                                                      taskIds);
          else
            gridClient_.SubmitSubtaskWithDependencies(session_, taskId, DataAdapter.BuildPayload(runConfiguration_, aggregationRequest), taskIds);

          return result.Output;
        }

        default:
          throw new ArgumentException($"{typeof(Request)} is not supported.");

      }
    }
  }
}