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

using System.Diagnostics;
using System.Threading.Tasks;

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
      var watch = Stopwatch.StartNew();

      var sessionClient = gridClient_.CreateSession();

      var request = runConfiguration.BuildRequest(out var shape, logger_);

      var taskId = sessionClient.SubmitTask(DataAdapter.BuildPayload(runConfiguration, request));

      logger_.LogInformation("Submitted root task {taskId}", taskId);

      sessionClient.WaitSubtasksCompletion(taskId).Wait();

      var rawResult = RequestResult.FromBytes(sessionClient.GetResult(taskId));
      if (rawResult is null)
      {
        logger_.LogError("Could not read result. Are you sure that WaitSubtasksCompletion waits enough ?");
      }
      else
      {
        while (!rawResult.HasResult) rawResult = RequestResult.FromBytes(sessionClient.GetResult(rawResult.Value));

        logger_.LogWarning("Final result is {result}", rawResult.Value);
        logger_.LogWarning("Expected result is 1.{result}", string.Join(".", shape));
      }
      watch.Stop();
      logger_.LogWarning("Client was executed in {time}s", watch.Elapsed.TotalSeconds);
    }


    [PublicAPI]
    public void Start() => Start(RunConfiguration.Medium);
  }
}
