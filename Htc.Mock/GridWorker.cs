// GridWorker.cs is part of the Htc.Mock solution.
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

using Htc.Mock.Core;
using Htc.Mock.RequestRunners;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace Htc.Mock
{
  [PublicAPI]
  public class GridWorker
  {
    private readonly ILogger<GridWorker>   logger_;
    private readonly IRequestRunnerFactory requestRunnerFactory_;

    private IRequestRunner requestRunner_;

    public GridWorker(IRequestRunnerFactory requestRunnerFactory, ILogger<GridWorker> logger)
    {
      requestRunnerFactory_ = requestRunnerFactory;
      logger_               = logger;
    }

    public byte[] Execute(string taskId, byte[] payload)
    {
      logger_.LogDebug("Start task {id}", taskId);
      var (runConfiguration, request) = DataAdapter.ReadPayload(payload);

      requestRunner_ = requestRunnerFactory_.Create(runConfiguration);

      var output = requestRunner_.ProcessRequest(request, taskId);
      logger_.LogInformation("Completed task {id}", taskId);
      return output;
    }
  }
}
