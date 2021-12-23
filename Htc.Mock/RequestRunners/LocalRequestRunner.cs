// LocalRequestRunner.cs is part of the Htc.Mock solution.
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Htc.Mock.Core;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace Htc.Mock.RequestRunners
{
  [PublicAPI]
  public class LocalRequestRunner : IRequestRunner
  {
    private readonly ILogger<LocalRequestRunner> logger_;

    private readonly RequestProcessor requestProcessor_;

    private readonly ConcurrentDictionary<string, RequestResult> results_ = new ConcurrentDictionary<string, RequestResult>();

    /// <summary>
    ///   Builds a <c>DistributedRequestProcessor</c>
    /// </summary>
    /// <param name="runConfiguration">
    ///   Defines the properties of the current execution.
    ///   It is assumed to be a session data.
    /// </param>
    /// <param name="logger"></param>
    public LocalRequestRunner(RunConfiguration runConfiguration, ILogger<LocalRequestRunner> logger)
    {
      logger_           = logger;
      requestProcessor_ = new RequestProcessor(true, true, true, runConfiguration, logger);
    }

    public bool ParallelRun { get; set; } = true;

    /// <inheritdoc />
    public byte[] ProcessRequest(Request request, string taskId) => ProcessRequest(request, ParallelRun).ToBytes();

    public event Action<int> SpawningRequestEvent;


    public RequestResult ProcessRequest(Request request, bool parallelRun)
    {
      using (var _ = logger_.BeginScope(new Dictionary<string, string>
                                        {
                                          ["requestId"] = request.Id,
                                        }))
      {
        var inputs = request.Dependencies.ToDictionary(s => s, s =>
                                                               {
                                                                 var rr                   = results_[s];
                                                                 while (!rr.HasResult) rr = results_[rr.Value];

                                                                 return rr.Value;
                                                               });

        var result = requestProcessor_.GetResult(request, inputs);

        if (parallelRun)
          Parallel.ForEach(result.SubRequests.Where(r => r.Dependencies.Count == 0), r =>
                                                                                     {
                                                                                       SpawningRequestEvent?.Invoke(1);
                                                                                       var res = ProcessRequest(r, r.Id);
                                                                                       results_[r.Id] = RequestResult.FromBytes(res);
                                                                                     });
        else
          foreach (var r in result.SubRequests.Where(r => r.Dependencies.Count == 0))
          {
            SpawningRequestEvent?.Invoke(1);
            var res = ProcessRequest(r, r.Id);
            results_[r.Id] = RequestResult.FromBytes(res);
          }

        foreach (var r in result.SubRequests.Where(r => r.Dependencies.Count == 0))
        {
          SpawningRequestEvent?.Invoke(1);
          var res = ProcessRequest(r, r.Id);
          results_[r.Id] = RequestResult.FromBytes(res);
        }

        return result.Result;
      }
    }
  }
}
