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


using System;
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
    private readonly ConcurrentDictionary<string, byte[]>                  resultStore = new();
    private readonly ConcurrentDictionary<string, Task>                    statusStore = new();
    private readonly GridWorker                                            gridWorker;
    private          int                                                   taskCount;
    private          int                                                   sessionCount;
    private readonly ConcurrentDictionary<string, CancellationTokenSource> cancelSources = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   subtasksLists = new();
    private readonly ConcurrentDictionary<string, ConcurrentBag<string>>   parentLists   = new();


    public GridClient(IDataClient dataClient)
      => gridWorker = new(new DelegateRequestRunnerFactory((runConfiguration, session)
                                                              => new DistributedRequestRunnerWithAggregation(dataClient,
                                                                                              this,
                                                                                              runConfiguration,
                                                                                              session,
                                                                                              fastCompute: true,
                                                                                              useLowMem: true,
                                                                                              smallOutput: true)));

    /// <inheritdoc />
    public byte[] GetResult(string id) => resultStore[id];

    /// <inheritdoc />
    public void WaitCompletion(string id) => statusStore[id].Wait();

    /// <inheritdoc />
    public void WaitSubtasksCompletion(string id)
    {
      WaitCompletion(id);
      var subtasks = subtasksLists[id];
      int nbSubtasks;
      do
      {
        nbSubtasks = subtasks.Count;
        Task.WhenAll(subtasks.Select(s => statusStore[s])).Wait();
        subtasks = subtasksLists[id];
      } while (nbSubtasks != subtasks.Count);
    }

    /// <inheritdoc />
    public string SubmitTask(string session, byte[] payload)
    {
      cancelSources[session].Token.ThrowIfCancellationRequested();

      var taskId = $"{session}_{Interlocked.Increment(ref taskCount)}";
      cancelSources[taskId] = new CancellationTokenSource();
      parentLists[taskId]   = new ConcurrentBag<string>();
      subtasksLists[taskId] = new ConcurrentBag<string>();

      var cts = CancellationTokenSource.CreateLinkedTokenSource(cancelSources[taskId].Token, cancelSources[session].Token);

      statusStore[taskId] = Task.Factory.StartNew(() => resultStore[taskId] = gridWorker.Execute(session, taskId, payload),
                                                   cts.Token);

      return taskId;
    }

    /// <inheritdoc />
    public IEnumerable<string> SubmitTasks(string session, IEnumerable<byte[]> payloads) => payloads.Select(p=>SubmitTask(session, p));

    /// <inheritdoc />
    public string SubmitSubtask(string session, string parentId, byte[] payload)
    {
      var taskId = SubmitTask(session, payload);
      parentLists[taskId].Add(parentId);

      subtasksLists[parentId].Add(taskId);
      foreach (var parent in parentLists[parentId])
      {
        subtasksLists[parent].Add(taskId);
      }

      return taskId;
    }

    /// <inheritdoc />
    public IEnumerable<string> SubmitSubtask(string session, string parentId, IEnumerable<byte[]> payloads) => payloads.Select(p => SubmitSubtask(session, parentId, p));

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
    public IEnumerable<string> SubmitTaskWithDependencies(string session, IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies) => payloadWithDependencies.Select(p => SubmitTaskWithDependencies(session, p.Item1, p.Item2));

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
    public IEnumerable<string> SubmitSubtaskWithDependencies(string session, string parentId, IEnumerable<Tuple<byte[], IList<string>>> payloadWithDependencies) => payloadWithDependencies.Select(p => SubmitSubtaskWithDependencies(session, parentId, p.Item1, p.Item2));

    /// <inheritdoc />
    public string CreateSession()
    {
      var sessionId = $"Session-{Interlocked.Increment(ref sessionCount)}";
      cancelSources[sessionId] = new CancellationTokenSource();

      return sessionId;
    }

    /// <inheritdoc />
    public void OpenSession(string sessionId)
    {
      cancelSources.TryAdd(sessionId, new CancellationTokenSource());
    }

    /// <inheritdoc />
    public void CancelSession(string session) => cancelSources[session].Cancel();

    /// <inheritdoc />
    public void CancelTask(string taskId)
    {
      cancelSources[taskId].Cancel();
      foreach (var subtask in subtasksLists[taskId])
      {
        CancelTask(subtask);
      }
    }
  }
}
