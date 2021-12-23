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
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   parentLists_   = new();
    private readonly ConcurrentDictionary<string, byte[]>                  resultStore_   = new();
    private readonly ConcurrentDictionary<string, Task>                    statusStore_   = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   subtasksLists_ = new();
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
    public async Task WaitSubtasksCompletion(string id)
    {
      await statusStore_[id];
      var subtasks = subtasksLists_[id];
      int nbSubtasks;
      do
      {
        nbSubtasks = subtasks.Count;
        await Task.WhenAll(subtasks.Select(s => statusStore_[s]));
        subtasks = subtasksLists_[id];
      } while (nbSubtasks != subtasks.Count);
    }

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
                                                                                 await Task.WhenAll(p.Item2
                                                                                                     .Select(WaitSubtasksCompletion));
                                                                                 var result = gridWorker_.Execute(taskId, p.Item1);
                                                                                 logger_.LogTrace("Store result for task {id}",
                                                                                                  taskId);
                                                                                 resultStore_[taskId] = result;
                                                                               },
                                                                               cancelSources_[taskId].Token)
                                                                     .Unwrap();

                                          return taskId;
                                        });


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

                                          subtasksLists_[parentId].Add(taskId);
                                          foreach (var parent in parentLists_[parentId])
                                            subtasksLists_[parent].Add(taskId);

                                          logger_.LogTrace("Launch a new System.Task to process task {id}", taskId);
                                          statusStore_[taskId] = Task.Factory.StartNew(async () =>
                                                                                       {
                                                                                         await p.Item2.Select(WaitSubtasksCompletion)
                                                                                                .WhenAll();


                                                                                         var result = gridWorker_.Execute(taskId, p.Item1);
                                                                                         logger_.LogTrace("Store result for task {id}",
                                                                                                          taskId);
                                                                                         resultStore_[taskId] = result;
                                                                                       }).Unwrap();
                                          return taskId;
                                        });

  }
  
}
