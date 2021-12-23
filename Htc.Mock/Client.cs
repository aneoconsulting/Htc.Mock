// Client.cs is part of the Htc.Mock solution.
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

using Htc.Mock.Core;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;

namespace Htc.Mock
{
  [PublicAPI]
  public class Client
  {
    private readonly IGridClient     gridClient_;
    private readonly ILogger<Client> logger_;

    public Client(IGridClient gridClient, ILogger<Client> logger)
    {
      gridClient_ = gridClient;
      logger_     = logger;
    }

    [PublicAPI]
    public void Start(RunConfiguration runConfiguration)
    {
      logger_.LogInformation("Start new run with {configuration}", runConfiguration.ToString());

      var session = gridClient_.CreateSession();

      logger_.LogInformation("Session name is {session}", session);

      var request = runConfiguration.BuildRequest(out var shape, logger_);

      var taskId = gridClient_.SubmitTask(session, DataAdapter.BuildPayload(runConfiguration, request));

      logger_.LogInformation("Submitted root task {taskId}", taskId);

      gridClient_.WaitSubtasksCompletion(taskId);

      var rawResult = RequestResult.FromBytes(gridClient_.GetResult(taskId));
      if (rawResult is null)
      {
        logger_.LogError("Could not read result. Are you sure that WaitSubtasksCompletion waits enough ?");
      }
      else
      {
        while (!rawResult.HasResult) rawResult = RequestResult.FromBytes(gridClient_.GetResult(rawResult.Value));

        logger_.LogError("Final result is {result}", rawResult.Value);
        logger_.LogError("Expected result is 1.{result}", string.Join(".", shape));
      }
    }


    [PublicAPI]
    public void Start() => Start(RunConfiguration.Medium);
  }
}
