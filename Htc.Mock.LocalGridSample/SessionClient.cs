// SessionClient.cs is part of the Htc.Mock solution.
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

using Htc.Mock.Utils;

using Microsoft.Extensions.Logging;

namespace Htc.Mock.LocalGridSample
{
  internal class SessionClient : ISessionClient
  {
    private readonly ConcurrentDictionary<string, CancellationTokenSource> cancelSources_ = new();
    private readonly GridWorker                                            gridWorker_;
    private readonly ILogger<SessionClient>                                logger_;
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   parentLists_        = new();
    private readonly ConcurrentDictionary<string, byte[]>                  resultStore_        = new();
    private readonly ConcurrentDictionary<string, Task>                    statusStore_        = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<string>> subtasksLists_      = new();
    private readonly ConcurrentDictionary<string, Task>                    subTasksCompletion_ = new();
    private          int                                                   taskCount_;


    public SessionClient(ILoggerFactory loggerFactory, GridWorker gridWorker)
    {
      logger_     = loggerFactory.CreateLogger<SessionClient>();
      gridWorker_ = gridWorker;
    }

    /// <inheritdoc />
    public void Dispose() { }
    
    /// <inheritdoc />
    public byte[] GetResult(string id)
    {
      logger_.LogTrace("Getting result for task {id}", id);
      return resultStore_[id];
    }

    /// <inheritdoc />
    public Task WaitSubtasksCompletion(string id) => subTasksCompletion_[id];

    /// <inheritdoc />
    public IEnumerable<string> SubmitTasksWithDependencies(IEnumerable<Tuple<byte[], IList<string>>> payloadsWithDependencies)
      => payloadsWithDependencies.Select(p =>
                                        {
                                          var taskId = $"task_{Interlocked.Increment(ref taskCount_)}";
                                          logger_.LogTrace("Submit task with Id {id}", taskId);
                                          cancelSources_[taskId] = new();
                                          parentLists_[taskId]   = new();
                                          subtasksLists_[taskId] = new();
                                          logger_.LogTrace("Launch a new System.Task to process task {id}",
                                                           taskId);
                                          statusStore_[taskId] = Task.Factory
                                                                     .StartNew(async () =>
                                                                               {
                                                                                 await p.Item2
                                                                                        .Select(WaitSubtasksCompletion)
                                                                                        .WhenAll();

                                                                                 var result = gridWorker_.Execute(taskId, p.Item1);
                                                                                 logger_.LogTrace("Store result for task {id}",
                                                                                                  taskId);
                                                                                 resultStore_[taskId] = result;
                                                                               },
                                                                               cancelSources_[taskId].Token)
                                                                     .Unwrap();
                                          subTasksCompletion_[taskId] = WaitForSubTaskCompletionAsync(taskId);

                                          return taskId;
                                        });

    private Task WaitForSubTaskCompletionAsync(string taskId)
    {
      return Task.Factory
                 .StartNew(async () =>
                           {
                             await statusStore_[taskId];
                             while (subtasksLists_[taskId].TryDequeue(out var childId))
                             {
                               await subTasksCompletion_[childId];
                             }
                           }
                          ).Unwrap();
    }


    /// <inheritdoc />
    public IEnumerable<string> SubmitSubtasksWithDependencies(string                                    parentId,
                                                              IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies)
      => payloadWithDependencies.Select(p =>
                                        {
                                          var taskId = $"task_{Interlocked.Increment(ref taskCount_)}";
                                          logger_.LogTrace("Submit task with Id {id}", taskId);
                                          cancelSources_[taskId] = new();
                                          parentLists_[taskId]   = new();
                                          subtasksLists_[taskId] = new();

                                          logger_.LogDebug("Task with Id {id} is a child of {parentId}",
                                                           taskId, parentId);
                                          parentLists_[taskId].Add(parentId);

                                          subtasksLists_[parentId].Enqueue(taskId);
                                          //foreach (var parent in parentLists_[parentId])
                                          //  subtasksLists_[parent].Enqueue(taskId);

                                          logger_.LogTrace("Launch a new System.Task to process task {id}", taskId);
                                          statusStore_[taskId] = Task.Factory
                                                                     .StartNew(async () =>
                                                                               {
                                                                                 await p.Item2.Select(WaitSubtasksCompletion)
                                                                                        .WhenAll();


                                                                                 var result = gridWorker_.Execute(taskId, p.Item1);
                                                                                 logger_.LogTrace("Store result for task {id}",
                                                                                                  taskId);
                                                                                 resultStore_[taskId] = result;
                                                                               }).Unwrap();

                                          subTasksCompletion_[taskId] = WaitForSubTaskCompletionAsync(taskId);
                                          return taskId;
                                        });

  }
  
}
