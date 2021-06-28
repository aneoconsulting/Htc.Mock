// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

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
             0,
             depth,
             0)
    {
    }

    public Request(string id,
                         int    durationMs,
                         int    memoryUsageKb,
                         int    outputSize,
                         int    seed,
                         int    depth,
                         int    currentDepth)
      : this(id,
             durationMs, 
             memoryUsageKb, 
             outputSize, 
             Array.Empty<string>(), 
             string.Empty, 
             seed, 
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
                         int           seed,
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
      Seed              = seed;
      Depth             = depth;
      CurrentDepth      = currentDepth;
      ResultIdsRequired = resultIdsRequired;
      ParentId          = parentId;
      //Console.WriteLine($"{nameof(seed)}={seed}");
    }

    public int Depth { get; }

    public int CurrentDepth { get; }

    public int Seed { get; }

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
