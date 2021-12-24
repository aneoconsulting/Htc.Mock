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
  public class GridData
  {
    public readonly ConcurrentDictionary<string, CancellationTokenSource> cancelSources_ = new();
    public readonly ConcurrentDictionary<string, ConcurrentBag<string>>   parentLists_        = new();
    public readonly ConcurrentDictionary<string, byte[]>                  resultStore_        = new();
    public readonly ConcurrentDictionary<string, Task>                    statusStore_        = new();
    public readonly ConcurrentDictionary<string, ConcurrentQueue<string>> subtasksLists_      = new();
    public readonly ConcurrentDictionary<string, Task>                    subTasksCompletion_ = new();
    public          int                                                   taskCount_;
  }

  internal class SessionClient : ISessionClient
  {
    private readonly GridWorker             gridWorker_;
    private readonly ILogger<SessionClient> logger_;
    private readonly GridData               gridData_;
    private readonly string                 parentId_;


    public SessionClient(ILoggerFactory loggerFactory, GridWorker gridWorker, GridData gridData, string parentId)
    {
      gridData_   = gridData;
      parentId_   = parentId;
      logger_     = loggerFactory.CreateLogger<SessionClient>();
      gridWorker_ = gridWorker;
    }

    /// <inheritdoc />
    public void Dispose() { }
    
    /// <inheritdoc />
    public byte[] GetResult(string id)
    {
      logger_.LogTrace("Getting result for task {id}", id);
      return gridData_.resultStore_[id];
    }

    /// <inheritdoc />
    public Task WaitSubtasksCompletion(string id) => gridData_.subTasksCompletion_[id];

    /// <inheritdoc />
    public IEnumerable<string> SubmitTasksWithDependencies(IEnumerable<Tuple<byte[], IList<string>>> payloadsWithDependencies)
      => payloadsWithDependencies.Select(p =>
                                        {
                                          var taskId = $"task_{Interlocked.Increment(ref gridData_.taskCount_)}";
                                          logger_.LogTrace("Submit task with Id {id}", taskId);
                                          gridData_.cancelSources_[taskId] = new();
                                          gridData_.parentLists_[taskId]   = new();
                                          gridData_.subtasksLists_[taskId] = new();
                                          logger_.LogTrace("Launch a new System.Task to process task {id}",
                                                           taskId);

                                          if (!string.IsNullOrEmpty(parentId_))
                                          {
                                            logger_.LogDebug("Task with Id {id} is a child of {parentId}",
                                                             taskId, parentId_);
                                            gridData_.parentLists_[taskId].Add(parentId_);

                                            gridData_.subtasksLists_[parentId_].Enqueue(taskId);
                                          }

                                          gridData_.statusStore_[taskId] = Task.Factory
                                                                               .StartNew(async () =>
                                                                                         {
                                                                                           await p.Item2
                                                                                                  .Select(WaitSubtasksCompletion)
                                                                                                  .WhenAll();

                                                                                           var result = gridWorker_.Execute(taskId, p.Item1);
                                                                                           logger_.LogTrace("Store result for task {id}",
                                                                                                            taskId);
                                                                                           gridData_.resultStore_[taskId] = result;
                                                                                         },
                                                                                         gridData_.cancelSources_[taskId].Token)
                                                                               .Unwrap();
                                          gridData_.subTasksCompletion_[taskId] = WaitForSubTaskCompletionAsync(taskId);

                                          return taskId;
                                        });

    private Task WaitForSubTaskCompletionAsync(string taskId)
    {
      return Task.Factory
                 .StartNew(async () =>
                           {
                             await gridData_.statusStore_[taskId];
                             while (gridData_.subtasksLists_[taskId].TryDequeue(out var childId))
                             {
                               await gridData_.subTasksCompletion_[childId];
                             }
                           }
                          ).Unwrap();
    }

    

  }
  
}
