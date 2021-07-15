/* GridClient.cs is part of the Htc.Mock.LocalGridSample solution.
    
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


using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Htc.Mock.RequestRunners;

namespace Htc.Mock.LocalGridSample
{
  class GridClient : IGridClient
  {
    private readonly ConcurrentDictionary<string, byte[]>                  resultStore_ = new();
    private readonly ConcurrentDictionary<string, Task>                    statusStore_ = new();
    private readonly GridWorker                                            gridWorker_;
    private          int                                                   taskCount_;
    private          int                                                   sessionCount_;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> cancelSources_ = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   subtasksLists_ = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   parentLists_   = new();


    public GridClient(IDataClient dataClient)
      => gridWorker_ = new(new DelegateRequestRunnerFactory((runConfiguration, session)
                                                              => new DistributedRequestRunnerWithAggregation(dataClient,
                                                                                              this,
                                                                                              runConfiguration,
                                                                                              session,
                                                                                              fastCompute: true,
                                                                                              useLowMem: true,
                                                                                              smallOutput: true)));

    /// <inheritdoc />
    public byte[] GetResult(string id) => resultStore_[id];

    /// <inheritdoc />
    public void WaitCompletion(string id) => statusStore_[id].Wait();

    /// <inheritdoc />
    public void WaitSubtasksCompletion(string id)
    {
      WaitCompletion(id);
      var subtasks = subtasksLists_[id];
      int nbSubtasks;
      do
      {
        nbSubtasks = subtasks.Count;
        Task.WhenAll(subtasks.Select(s => statusStore_[s])).Wait();
        subtasks = subtasksLists_[id];
      } while (nbSubtasks != subtasks.Count);
    }

    /// <inheritdoc />
    public string SubmitTask(string session, byte[] payload)
    {
      cancelSources_[session].Token.ThrowIfCancellationRequested();

      var taskId = $"{session}_{Interlocked.Increment(ref taskCount_)}";
      cancelSources_[taskId] = new CancellationTokenSource();
      parentLists_[taskId]   = new ConcurrentBag<string>();
      subtasksLists_[taskId] = new ConcurrentBag<string>();

      var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelSources_[taskId].Token, cancelSources_[session].Token);

      statusStore_[taskId] = Task.Factory.StartNew(() => resultStore_[taskId] = gridWorker_.Execute(session, taskId, payload),
                                                   cts.Token);

      return taskId;
    }

    /// <inheritdoc />
    public string SubmitSubtask(string session, string parentId, byte[] payload)
    {
      var taskId = SubmitTask(session, payload);
      parentLists_[taskId].Add(parentId);

      subtasksLists_[parentId].Add(taskId);
      foreach (var parent in parentLists_[parentId])
      {
        subtasksLists_[parent].Add(taskId);
      }

      return taskId;
    }

    /// <inheritdoc />
    public string SubmitTaskWithDependencies(string session, byte[] payload, IList<string> dependencies)
    {
      foreach (var dependency in dependencies)
      {
        WaitSubtasksCompletion(dependency);
      }

      return SubmitTask(session, payload);
    }

    /// <inheritdoc />
    public string SubmitSubtaskWithDependencies(string session, string parentId, byte[] payload, IList<string> dependencies)
    {
      foreach (var dependency in dependencies)
      {
        WaitSubtasksCompletion(dependency);
      }

      return SubmitSubtask(session, parentId, payload);
    }

    /// <inheritdoc />
    public string CreateSession()
    {
      var sessionId = $"Session-{Interlocked.Increment(ref sessionCount_)}";
      cancelSources_[sessionId] = new CancellationTokenSource();

      return sessionId;
    }

    /// <inheritdoc />
    public void OpenSession(string sessionId)
    {
      cancelSources_.TryAdd(sessionId, new CancellationTokenSource());
    }

    /// <inheritdoc />
    public void CancelSession(string session) => cancelSources_[session].Cancel();

    /// <inheritdoc />
    public void CancelTask(string taskId)
    {
      cancelSources_[taskId].Cancel();
      foreach (var subtask in subtasksLists_[taskId])
      {
        CancelTask(subtask);
      }
    }
  }
}
