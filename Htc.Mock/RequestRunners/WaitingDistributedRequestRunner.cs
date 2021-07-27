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
    private readonly RunConfiguration runConfiguration;
    private readonly RequestProcessor requestProcessor;
    private readonly IDataClient      dataClient;
    private readonly IGridClient      gridClient;
    private readonly string           session;
    private readonly bool             waitDependencies;

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

      this.runConfiguration = runConfiguration;
      requestProcessor      = new RequestProcessor(fastCompute, useLowMem, smallOutput, runConfiguration);
      this.dataClient       = dataClient;
      this.gridClient       = gridClient;
      this.session          = session;
      this.waitDependencies = waitDependencies;
    }

    public byte[] ProcessRequest(Request request, string taskId)
    {
      using (gridClient.OpenSession(session))
      {
        switch (request)
        {
          case FinalRequest finalRequest:
          {
            var result = requestProcessor.GetResult(finalRequest, Array.Empty<string>());
            dataClient.StoreData(request.Id, Encoding.ASCII.GetBytes(result.Result));

            return result.Output;
          }

          case AggregationRequest aggregationRequest:
          {
            var result = requestProcessor.GetResult(aggregationRequest, aggregationRequest.ResultIdsRequired
                                                                                          .Select(dataClient.GetData)
                                                                                          .Select(Encoding.ASCII.GetString)
                                                                                          .ToList());
            var data = Encoding.ASCII.GetBytes(result.Result);
            dataClient.StoreData(aggregationRequest.Id, data);
            dataClient.StoreData(aggregationRequest.ParentId, data);
            
            return result.Output;
          }

          case ComputeRequest computeRequest:
          {
            gridClient.WaitSubtasksCompletion(taskId);
            var result = requestProcessor.GetResult(computeRequest, Array.Empty<string>());

            var subRequestsByDepsRq = result.SubRequests
                                            .ToLookup(sr => !(sr is AggregationRequest));


            var subtasksPayload = subRequestsByDepsRq[true].Select(lr => DataAdapter.BuildPayload(runConfiguration, lr));
            var subtaskIds      = gridClient.SubmitTasks(session, subtasksPayload);

            var aggregationRequest = subRequestsByDepsRq[false].Cast<AggregationRequest>()
                                                              .Single();

            if (waitDependencies)
              gridClient.WaitDependenciesAndSubmitSubtask(session,
                                                          taskId,
                                                          DataAdapter.BuildPayload(runConfiguration,
                                                                                    aggregationRequest),
                                                          subtaskIds);
            else
              gridClient.SubmitSubtaskWithDependencies(session,
                                                        taskId,
                                                        DataAdapter.BuildPayload(runConfiguration,
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
}
