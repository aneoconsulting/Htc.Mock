/* IGridClient.cs is part of the Htc.Mock solution.
    
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


using System.Collections.Generic;

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
    /// Waits for the completion of a task processing and fetches the corresponding results.
    /// </summary>
    /// <param name="id">Id of the task to wait for.</param>
    /// <returns>The result of the task</returns>
    void WaitCompletion(string id);

    /// <summary>
    /// Waits for the completion of a task processing and fetches the corresponding results.
    /// </summary>
    /// <param name="id">Id of the task to wait for.</param>
    /// <returns>The result of the task</returns>
    void WaitSubtasksCompletion(string id);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <returns>The id of the task corresponding to the <c>Task</c></returns>
    string SubmitTask(string session, byte[] payload);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="parentId"></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <returns>The id of the task corresponding to the <c>Task</c></returns>
    string SubmitSubtask(string session, string parentId, byte[] payload);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed after completion of its dependencies
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <param name="dependencies">The list of dependencies that have to be processed before start processing <c>task</c></param>
    string SubmitTaskWithDependencies(string session, byte[] payload, IList<string> dependencies);

    /// <summary>
    /// Submit a new <c>Task</c> to be processed after completion of its dependencies
    /// </summary>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="parentId"></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <param name="dependencies">The list of dependencies that have to be processed before start processing <c>task</c></param>
    string SubmitSubtaskWithDependencies(string session, string parentId, byte[] payload, IList<string> dependencies);

    string CreateSession();

    void CancelSession(string session);

    void CancelTask(string taskId);
  }

  public static class GridClientExt
  {
    /// <summary>
    /// Wait for the dependencies to be completed and then submits a new task
    /// </summary>
    /// <param name="client">The <c>IGridClient</c> implementation used to communicate with the grid</param>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <param name="dependencies">The list of dependencies that have to be processed before start processing <c>task</c></param>
    public static string WaitDependenciesAndSubmitTask(this IGridClient    client,
                                                       string              session,
                                                       byte[]              payload,
                                                       IEnumerable<string> dependencies)
    {
      foreach (var dependency in dependencies)
      {
        client.WaitCompletion(dependency);
      }

      return client.SubmitTask(session, payload);
    }

    /// <summary>
    /// Wait for the dependencies to be completed and then submits a new task
    /// </summary>
    /// <param name="client">The <c>IGridClient</c> implementation used to communicate with the grid</param>
    /// <param name="session">The session to which submit the new <c>task</c></param>
    /// <param name="parentId"></param>
    /// <param name="payload">The payload of the task to process</param>
    /// <param name="dependencies">The list of dependencies that have to be processed before start processing <c>task</c></param>
    public static string WaitDependenciesAndSubmitSubtask(this IGridClient    client,
                                                          string              session,
                                                          string              parentId,
                                                          byte[]              payload,
                                                          IEnumerable<string> dependencies)
    {
      foreach (var dependency in dependencies)
      {
        client.WaitSubtasksCompletion(dependency);
      }

      return client.SubmitSubtask(session, parentId, payload);
    }
  }
}
