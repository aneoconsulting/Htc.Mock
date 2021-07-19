// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

using System;
using System.Linq;
using System.Text;

using Htc.Mock.Core;

using JetBrains.Annotations;

namespace Htc.Mock.RequestRunners
{
  [PublicAPI]
  public class WaitingDistributedRequestRunner : IRequestRunner
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
    public WaitingDistributedRequestRunner(IDataClient      dataClient,
                                           IGridClient      gridClient,
                                           RunConfiguration runConfiguration,
                                           string           session,
                                           bool             waitDependencies = false,
                                           bool             fastCompute      = false,
                                           bool             useLowMem        = false,
                                           bool             smallOutput      = false)
    {

      runConfiguration_ = runConfiguration;
      requestProcessor_ = new RequestProcessor(fastCompute, useLowMem, smallOutput, runConfiguration);
      dataClient_       = dataClient;
      gridClient_       = gridClient;
      session_          = session;
      waitDependencies_ = waitDependencies;

      gridClient_.OpenSession(session_);
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
          gridClient_.WaitSubtasksCompletion(taskId);
          var result = requestProcessor_.GetResult(computeRequest, Array.Empty<string>());

          var subRequestsByDepsRq = result.SubRequests
                                          .ToLookup(sr => !(sr is AggregationRequest));


          var subtasksPayload = subRequestsByDepsRq[true].Select(lr => DataAdapter.BuildPayload(runConfiguration_, lr));
          var subtaskIds      = gridClient_.SubmitTasks(session_, subtasksPayload);

          var aggregationRequest = subRequestsByDepsRq[false].Cast<AggregationRequest>()
                                                             .Single();

          if (waitDependencies_)
            gridClient_.WaitDependenciesAndSubmitSubtask(session_,
                                                         taskId,
                                                         DataAdapter.BuildPayload(runConfiguration_,
                                                                                  aggregationRequest),
                                                         subtaskIds);
          else
            gridClient_.SubmitSubtaskWithDependencies(session_,
                                                      taskId,
                                                      DataAdapter.BuildPayload(runConfiguration_,
                                                                               aggregationRequest),
                                                      subtaskIds.ToList());

          return result.Output;
        }

        default:
          throw new ArgumentException($"{typeof(Request)} != supported.");

      }
    }
  }
}
