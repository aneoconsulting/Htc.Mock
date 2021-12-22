// GridClient.cs is part of the Htc.Mock solution.
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
using System.Threading;
using System.Threading.Tasks;

using Htc.Mock.RequestRunners;

using Microsoft.Extensions.Logging;

namespace Htc.Mock.LocalGridSample
{
  internal class GridClient : IGridClient
  {
    private readonly ConcurrentDictionary<string, CancellationTokenSource> cancelSources_ = new();
    private readonly GridWorker                                            gridWorker_;
    private readonly ILogger<GridClient>                                   logger_;
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   parentLists_   = new();
    private readonly ConcurrentDictionary<string, byte[]>                  resultStore_   = new();
    private readonly ConcurrentDictionary<string, Task>                    statusStore_   = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   subtasksLists_ = new();
    private          int                                                   sessionCount_;
    private          int                                                   taskCount_;


    public GridClient(ILoggerFactory loggerFactory)
    {
      logger_ = loggerFactory.CreateLogger<GridClient>();
      gridWorker_ = new(new DelegateRequestRunnerFactory((runConfiguration, session)
                                                           => new DistributedRequestRunner(this,
                                                                                           runConfiguration,
                                                                                           session,
                                                                                           loggerFactory
                                                                                            .CreateLogger<DistributedRequestRunner>(),
                                                                                           true,
                                                                                           true,
                                                                                           true)),
                        loggerFactory.CreateLogger<GridWorker>());
    }

    /// <inheritdoc />
    public byte[] GetResult(string id)
    {
      logger_.LogTrace("Getting result for task {id}", id);
      return resultStore_[id];
    }

    /// <inheritdoc />
    public void WaitCompletion(string id)
    {
      logger_.LogTrace("Will wait for task {id}", id);
      statusStore_[id].Wait();
    }

    /// <inheritdoc />
    public void WaitSubtasksCompletion(string id)
    {
      WaitCompletion(id);
      var subtasks = subtasksLists_[id];
      int nbSubtasks;
      do
      {
        nbSubtasks = subtasks.Count;
        Task.WhenAll(subtasks.Select(s =>
                                     {
                                       logger_.LogTrace("Will wait for task {id}", s);
                                       return statusStore_[s];
                                     })).Wait();
        subtasks = subtasksLists_[id];
      } while (nbSubtasks != subtasks.Count);
    }

    /// <inheritdoc />
    public IEnumerable<string> SubmitTasks(string session, IEnumerable<byte[]> payloads)
      => payloads.Select(p => SubmitTask(session, p));

    /// <inheritdoc />
    public IEnumerable<string> SubmitSubtasks(string session, string parentId, IEnumerable<byte[]> payloads)
      => payloads.AsParallel().Select(p => SubmitSubtask(session, parentId, p));

    /// <inheritdoc />
    public string SubmitTaskWithDependencies(string session, byte[] payload, IList<string> dependencies)
    {
      foreach (var dependency in dependencies) WaitSubtasksCompletion(dependency);

      return SubmitTask(session, payload);
    }

    /// <inheritdoc />
    public IEnumerable<string> SubmitTaskWithDependencies(string                                    session,
                                                          IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies)
      => payloadWithDependencies.Select(p => SubmitTaskWithDependencies(session, p.Item1, p.Item2));

    /// <inheritdoc />
    public string SubmitSubtaskWithDependencies(string session, string parentId, byte[] payload, IList<string> dependencies)
    {
      foreach (var dependency in dependencies) WaitSubtasksCompletion(dependency);

      return SubmitSubtask(session, parentId, payload);
    }

    /// <inheritdoc />
    public IEnumerable<string> SubmitSubtaskWithDependencies(string                                    session,
                                                             string                                    parentId,
                                                             IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies)
      => payloadWithDependencies.Select(p => SubmitSubtaskWithDependencies(session, parentId, p.Item1, p.Item2));

    /// <inheritdoc />
    public string CreateSession()
    {
      var sessionId = $"Session-{Interlocked.Increment(ref sessionCount_)}";
      cancelSources_[sessionId] = new();

      return sessionId;
    }

    /// <inheritdoc />
    public IDisposable OpenSession(string sessionId)
    {
      cancelSources_.TryAdd(sessionId, new());
      return null;
    }

    /// <inheritdoc />
    public void CancelSession(string session) => cancelSources_[session].Cancel();

    /// <inheritdoc />
    public void CancelTask(string taskId)
    {
      cancelSources_[taskId].Cancel();
      foreach (var subtask in subtasksLists_[taskId]) CancelTask(subtask);
    }

    private string SubmitTask(string session, byte[] payload)
    {
      cancelSources_[session].Token.ThrowIfCancellationRequested();

      var taskId = $"{session}_{Interlocked.Increment(ref taskCount_)}";
      logger_.LogTrace("Submit task with Id {id}", taskId);
      cancelSources_[taskId] = new();
      parentLists_[taskId]   = new();
      subtasksLists_[taskId] = new();

      var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelSources_[taskId].Token, cancelSources_[session].Token);

      statusStore_[taskId] = Task.Factory.StartNew(() =>
                                                   {
                                                     logger_.LogTrace("Launch a new System.Task to process task {id}", taskId);
                                                     var result = gridWorker_.Execute(session, taskId, payload);
                                                     logger_.LogTrace("Store result for task {id}", taskId);
                                                     return resultStore_[taskId] = result;
                                                   },
                                                   cts.Token);
      return taskId;
    }

    private string SubmitSubtask(string session, string parentId, byte[] payload)
    {
      var taskId = SubmitTask(session, payload);
      logger_.LogInformation("Task with Id {id} is a child of {parentId}", taskId, parentId);
      parentLists_[taskId].Add(parentId);

      subtasksLists_[parentId].Add(taskId);
      foreach (var parent in parentLists_[parentId]) subtasksLists_[parent].Add(taskId);

      return taskId;
    }
  }
}
