// GridClientExt.cs is part of the Htc.Mock solution.
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

using JetBrains.Annotations;

namespace Htc.Mock
{
  [PublicAPI]
  public static class GridClientExt
  {
    /// <summary>
    ///   Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <returns>The id of the task corresponding to the <c>Task</c></returns>
    public static string SubmitTask(this ISessionClient client, byte[] payload)
      => client.SubmitTasks(new[] { payload }).Single();

    /// <summary>
    ///   Submit a new <c>Task</c> to be processed after completion of its dependencies
    /// </summary>
    /// <param name="client"></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <param name="dependencies">
    ///   The list of dependencies that have to be processed before start processing <c>task</c>
    /// </param>
    public static string SubmitTaskWithDependencies(this ISessionClient client, byte[] payload, IList<string> dependencies)
      => client.SubmitTasksWithDependencies(new[] { Tuple.Create(payload, dependencies) }).Single();


    /// <summary>
    ///   Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="payloads">The payloads of the tasks to process</param>
    /// <returns>The ids of the tasks corresponding to the <c>Tasks</c></returns>
    public static IEnumerable<string> SubmitTasks(this ISessionClient client, IEnumerable<byte[]> payloads)
      => client.SubmitTasksWithDependencies(payloads.Select(bytes => Tuple.Create(bytes, Array.Empty<string>() as IList<string>)));
  }
}
