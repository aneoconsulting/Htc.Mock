﻿/* IGridClient.cs is part of the Htc.Mock solution.
    
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
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

namespace Htc.Mock
{
  /// <summary>
  /// Interface to implement to allow the Htc.Mock components to connect to a grid scheduler
  /// </summary>
  [PublicAPI]
  public interface IGridClient
  {
    /// <summary>
    /// Waits for the completion of a task processing and fetches the corresponding results.
    /// </summary>
    /// <param name="id">Id of the task to wait for.</param>
    /// <returns>The result of the task</returns>
    byte[] GetResult(string id);

    /// <summary>
    /// Waits for the completion of a task processing .
    /// </summary>
    /// <param name="id">Id of the task to wait for.</param>
    /// <returns>The result of the task</returns>
    void WaitCompletion(string id);

    /// <summary>
    /// Waits for the completion of a task and all of its subtasks.
    /// </summary>
    /// <param name="id">Id of the task to wait for.</param>
    /// <returns>The result of the task</returns>
    void WaitSubtasksCompletion(string id);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="payloads">The payloads of the tasks to process</param>
    /// <returns>The ids of the tasks corresponding to the <c>Tasks</c></returns>
    IEnumerable<string> SubmitTasks(string session, IEnumerable<byte[]> payloads);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="parentId"></param>
    /// <param name="payloads">The payloads of the tasks to process</param>
    /// <returns>The ids of the tasks corresponding to the <c>Tasks</c></returns>
    IEnumerable<string> SubmitSubtasks(string session, string parentId, IEnumerable<byte[]> payloads);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed after completion of its dependencies
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <param name="dependencies">The list of dependencies that have to be processed before start processing <c>task</c></param>
    string SubmitTaskWithDependencies(string session, byte[] payload, IList<string> dependencies);

    IEnumerable<string> SubmitTaskWithDependencies(string session, IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed after completion of its dependencies
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="parentId"></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <param name="dependencies">The list of dependencies that have to be processed before start processing <c>task</c></param>
    string SubmitSubtaskWithDependencies(string session, string parentId, byte[] payload, IList<string> dependencies);
    IEnumerable<string> SubmitSubtaskWithDependencies(string session, string parentId, IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies);

    string CreateSession();

    IDisposable OpenSession(string sessionId);

    void CancelSession(string session);

    void CancelTask(string taskId);
  }

  [PublicAPI]
  public static class GridClientExt
  {
    /// <summary>
    /// Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <returns>The id of the task corresponding to the <c>Task</c></returns>
    public static string SubmitTask(this IGridClient client, string session, byte[] payload) 
      => client.SubmitTasks(session, new[] {payload}).Single();

    /// <summary>
    /// Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="client"></param>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="parentId"></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <returns>The id of the task corresponding to the <c>Task</c></returns>
    public static string SubmitSubtask(this IGridClient client, string session, string parentId, byte[] payload) 
      => client.SubmitSubtasks(session, parentId, new[] {payload}).Single();
    
  }
}
