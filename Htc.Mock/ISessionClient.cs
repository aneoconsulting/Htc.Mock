// ISessionClient.cs is part of the Htc.Mock solution.
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
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Htc.Mock
{
  /// <summary>
  ///   Interface to implement to allow the Htc.Mock components to connect to a grid scheduler
  /// </summary>
  [PublicAPI]
  public interface ISessionClient : IDisposable
  {
    /// <summary>
    ///   Waits for the completion of a task processing and fetches the corresponding results.
    /// </summary>
    /// <param name="id">Id of the task to wait for.</param>
    /// <returns>The result of the task</returns>
    byte[] GetResult(string id);

    /// <summary>
    ///   Waits for the completion of a task and all of its subtasks.
    /// </summary>
    /// <param name="id">Id of the task to wait for.</param>
    /// <returns>The result of the task</returns>
    Task WaitSubtasksCompletion(string id);

    /// <summary>
    /// </summary>
    /// <param name="payloadsWithDependencies"></param>
    /// <returns></returns>
    IEnumerable<string> SubmitTasksWithDependencies(IEnumerable<Tuple<byte[], IList<string>>> payloadsWithDependencies);
  }
}
