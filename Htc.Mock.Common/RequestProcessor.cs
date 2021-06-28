﻿// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using JetBrains.Annotations;

namespace Htc.Mock.Common
{
  [PublicAPI]
  public class RequestProcessor
  {
    private readonly RunConfiguration runConfiguration_;
    private readonly bool             fastCompute_;
    private readonly bool             useLowMem_;
    private readonly bool             smallOutput_;

    public RequestProcessor(bool fastCompute, bool useLowMem, bool smallOutput, RunConfiguration runConfiguration) 
    {
      fastCompute_      = fastCompute;
      useLowMem_        = useLowMem;
      smallOutput_      = smallOutput;
      runConfiguration_ = runConfiguration;
    }


    [PublicAPI]
    public RequestResult GetResult(Request request, IList<string> inputs)
      => ComputeResult(request, inputs).WithOutput(EmulateComputation(request));

    protected RequestResult ComputeResult(Request request, IList<string> inputs)
    {
      Debug.Assert(inputs is not null);

      if (request.ResultIdsRequired.Count != inputs.Count)
        throw new
          ArgumentException($"{nameof(request)} requires {request.ResultIdsRequired.Count} inputs, {inputs.Count} provided.",
                            nameof(inputs));

      var leaf = true;

      var ran = new Random(request.Seed);
      if (request.CurrentDepth < request.Depth)
      {
        if (request.CurrentDepth == 0 ||
            !request.IsAggregationRequest && ran.NextDouble() < runConfiguration_.SubTaskFraction)
        {
          leaf = false;
        }
      }

      if (leaf)
      {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (inputs.Count == 0)
          return new RequestResult(request.Id, GetResultString(request.Id));

        // use this computation to check that the good results were retrieved
        var result = GetResultString(GetAggregateString(GetAggregationRes(inputs)));

        Debug.Assert(result == GetResultString(request.Id),
                     $"Error during the computation of task {request.Id} or one of its dependencies.");
        
        return new RequestResult(request.Id, GetResultString(request.ParentId));
      }

      var nbRq = Math.Max(1, runConfiguration_.GetNbSubtasks(ran) - 1);
      var subRequests =
        Enumerable.Range(0, nbRq)
                  .Select(i => new Request($"{request.Id}_{i}",
                                                 runConfiguration_.GetTaskDurationMs(ran),
                                                 runConfiguration_.Memory, runConfiguration_.Data, ran.Next(),
                                                 request.Depth, request.CurrentDepth + 1))
                  .OrderBy(rq => rq.Id)
                  .ToList();
      var subRequestIds = subRequests.Select(sr => sr.Id).ToList();
      var aggregationRequest =
        new Request(GetAggregateString(GetAggregationRes(subRequestIds, GetResultString)),
                          runConfiguration_.GetTaskDurationMs(ran),
                          runConfiguration_.Memory, runConfiguration_.Data, subRequestIds, request.Id, ran.Next(),
                          request.Depth, request.CurrentDepth + 1);
      subRequests.Add(aggregationRequest);
      return new RequestResult(request.Id, subRequests);
    }

    public static string GetResultString(string taskId) => $"{taskId}_result";

    public static int GetAggregationRes(IEnumerable<string> ids, Func<string, string> resultSelector) 
      => GetAggregationRes(ids.Select(resultSelector));

    public static int GetAggregationRes(IEnumerable<string> results) 
      => results.Aggregate(0, HashCode.Combine);

    public static string GetAggregateString(int res) => $"Aggregate_{res}";

    private byte[] EmulateComputation(Request request)
    {
      var t = System.Threading.Tasks.Task.Delay(fastCompute_?0:request.DurationMs);

      var m = useLowMem_ ? new byte[0, 0] : new byte[request.MemoryUsageKb, 1024];
      // Write all bytes to ensure that the memory is really allocated
      for (var i = 0; i < m.GetLength(0); ++i)
      {
        for (var j = 0; j < m.GetLength(1) ; ++j)
        {
          m[i, j] = (byte) (request.MemoryUsageKb * j - 2019 * i);
        }
      }

      var output = new byte[smallOutput_ ? 0 : request.OutputSize];
      for (var i = 0; i < output.Length; ++i)
      {
        output[i] = (byte)((request.OutputSize & 2019) * i);
      }

      t.Wait();

      return output;
    }
  }
}