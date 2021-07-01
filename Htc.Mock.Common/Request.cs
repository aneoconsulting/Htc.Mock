/* Request.cs is part of the Htc.Mock.Common solution.
    
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
using System.Diagnostics;

using JetBrains.Annotations;

namespace Htc.Mock.Common
{
  [PublicAPI]
  [Serializable]
  public class Request
  {
    private const string HeadId = "HeadId";

    public Request(int durationMs,
                         int memoryUsageKb,
                         int outputSize,
                         int depth)
      : this(HeadId, 
             durationMs, 
             memoryUsageKb, 
             outputSize, 
             Array.Empty<string>(), 
             string.Empty,
             depth,
             0)
    {
    }

    public Request(string id,
                         int    durationMs,
                         int    memoryUsageKb,
                         int    outputSize,
                         int    depth,
                         int    currentDepth)
      : this(id,
             durationMs, 
             memoryUsageKb, 
             outputSize, 
             Array.Empty<string>(), 
             string.Empty, 
             depth,
             currentDepth)
    {
    }

    public Request(string        id,
                         int           durationMs,
                         int           memoryUsageKb,
                         int           outputSize,
                         IList<string> resultIdsRequired,
                         string        parentId,
                         int           depth,
                         int           currentDepth)
    {
      Debug.Assert(!string.IsNullOrEmpty(id));
      Debug.Assert(durationMs >= 0);
      Debug.Assert(memoryUsageKb >= 0);
      Debug.Assert(outputSize >= 0);
      Debug.Assert(resultIdsRequired is not null);
      Id                = id;
      DurationMs        = durationMs;
      MemoryUsageKb     = memoryUsageKb;
      OutputSize        = outputSize;
      Depth             = depth;
      CurrentDepth      = currentDepth;
      ResultIdsRequired = resultIdsRequired;
      ParentId          = parentId;
      //Console.WriteLine($"{nameof(seed)}={seed}");
    }

    public int Depth { get; }

    public int CurrentDepth { get; }

    public TimeSpan TotalSubtasksCalculationTime { get; }

    [NotNull]
    public IList<string> ResultIdsRequired { get; }

    public string ParentId { get; }

    public bool IsAggregationRequest => !string.IsNullOrEmpty(ParentId);

    [NotNull]
    public string Id { get; }

    public int DurationMs    { get; }
    public int MemoryUsageKb { get; }
    public int OutputSize    { get; }
  }
}
