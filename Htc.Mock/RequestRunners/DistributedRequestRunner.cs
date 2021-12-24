// DistributedRequestRunner.cs is part of the Htc.Mock solution.
// 
// Copyright (c) 2021-2021 ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (https://github.com/wkirschenmann)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Google.Protobuf;

using Htc.Mock.Core;
using Htc.Mock.Core.Protos;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

using Request = Htc.Mock.Core.Request;
using RunConfiguration = Htc.Mock.Core.RunConfiguration;

// ReSharper disable All

namespace Htc.Mock.RequestRunners
{
  [PublicAPI]
  public class DistributedRequestRunner : IRequestRunner
  {
    private readonly IGridClient                       gridClient_;
    private readonly ILogger<DistributedRequestRunner> logger_;
    private readonly RequestProcessor                  requestProcessor_;
    private readonly RunConfiguration                  runConfiguration_;

    /// <summary>
    ///   Builds a <c>DistributedRequestRunner</c>. The lifecycle of the object is meant to
    ///   corresponds to the life cycle of sessions (<see cref="GridWorker" />).
    /// </summary>
    /// <param name="gridClient"></param>
    /// <param name="runConfiguration">
    ///   Defines the properties of the current execution.
    ///   It is assumed to be a session data.
    /// </param>
    /// <param name="logger"></param>
    /// <param name="fastCompute">
    ///   Defines if the execution time should be emulated.
    ///   It is assumed to be a deployment configuration.
    /// </param>
    /// <param name="useLowMem">
    ///   Defines if the memory consumption should be emulated.
    ///   It is assumed to be a deployment configuration.
    /// </param>
    /// <param name="smallOutput">
    ///   Defines if the output size should be emulated.
    ///   It is assumed to be a deployment configuration.
    /// </param>
    public DistributedRequestRunner([NotNull] IGridClient                       gridClient,
                                    [NotNull] RunConfiguration                  runConfiguration,
                                    [NotNull] ILogger<DistributedRequestRunner> logger,
                                    bool                                        fastCompute = false,
                                    bool                                        useLowMem   = false,
                                    bool                                        smallOutput = false)
    {
      runConfiguration_ = runConfiguration ?? throw new ArgumentNullException(nameof(runConfiguration));
      requestProcessor_ = new RequestProcessor(fastCompute, useLowMem, smallOutput, runConfiguration, logger);
      gridClient_       = gridClient ?? throw new ArgumentNullException(nameof(gridClient));
      logger_           = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public byte[] ProcessRequest(Request request, string taskId)
    {
      using (var sessionClient = gridClient_.CreateSubSession(taskId))
      {
        logger_.BeginScope(new Dictionary<string, string>
                           {
                             ["requestId"] = request.Id,
                             ["taskId"]    = taskId,
                           });
        logger_.LogDebug("Start to process request");

        var inputs = request.Dependencies is null
                       ? new Dictionary<string, string>()
                       : request.Dependencies
                                .ToDictionary(id => id,
                                              id =>
                                              {                                                  
                                                logger_.LogInformation("Looking for result for Id {id}",id);
                                                var rr         = RequestResult.Parser.ParseFrom(sessionClient.GetResult(id));
                                                var previousId = id;
                                                while (!rr.HasResult)
                                                {
                                                  logger_.LogInformation("Result for Id {id} was inconclusive. Looking for nested Id {id2}",
                                                                         previousId, 
                                                                         rr.Value);
                                                  previousId = rr.Value;
                                                  rr         = RequestResult.Parser.ParseFrom(sessionClient.GetResult(rr.Value));
                                                }

                                                return rr.Value;
                                              }
                                             );

        var res = requestProcessor_.GetResult(request, inputs);

        var requests = res.SubRequests.GroupBy(r => r.Dependencies is null || r.Dependencies.Count == 0)
                          .ToDictionary(g => g.Key, g => g);


        Dictionary<string, string> idTranslation = new Dictionary<string, string>();
        if (requests.ContainsKey(true))
        {
          logger_.LogDebug("Will submit {count} new tasks", requests[true].Count());
          var readyRequests = requests[true];

          var newIds = sessionClient.SubmitTasks(readyRequests.Select(r => DataAdapter.BuildPayload(runConfiguration_, r)));


          idTranslation = new Dictionary<string, string>(readyRequests.Zip(newIds, (r, s) => new { Key = r.Id, Value = s })
                                                                      .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        if (requests.ContainsKey(false))
        {
          var requestsToSubmit = new Queue<Request>(requests[false]);
          while (requestsToSubmit.Any())
          {
            var req = requestsToSubmit.Dequeue();
            if (!req.Dependencies.All(dep => idTranslation.ContainsKey(dep)))
            {
              requestsToSubmit.Enqueue(req);
              continue;
            }

            var newDeps = req.Dependencies.Select(id => idTranslation[id]).ToList();
            req.Dependencies.Clear();

            foreach (var newDep in newDeps)
            {
              req.Dependencies.Add(newDep);
            }

            idTranslation[req.Id] =
              sessionClient.SubmitTaskWithDependencies(DataAdapter.BuildPayload(runConfiguration_, req), newDeps);
          }
        }

        var output = res.Result;

        if (!res.Result.HasResult)
        {
          output = new RequestResult
                   {
                     HasResult = false,
                     Value     = idTranslation[res.Result.Value],
                   };
          logger_.LogInformation("translated internal id {internal} to external id {external}", res.Result.Value, output.Value);
        }

        return output.ToByteArray();
      }
    }
  }
}
