// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Htc.Mock.Common
{
  [Serializable]
  public class RequestResult
  {
    public RequestResult(string requestId, string result)
      : this(requestId, true, result, Array.Empty<Request>(), Array.Empty<byte>())
    {
      Debug.Assert(!string.IsNullOrEmpty(requestId));
      Debug.Assert(!string.IsNullOrEmpty(result));
    }

    public RequestResult(string requestId, IList<Request> subRequests)
      : this(requestId, false, string.Empty, subRequests, Array.Empty<byte>())
    {
      Debug.Assert(!string.IsNullOrEmpty(requestId));
      Debug.Assert(subRequests is not null);
    }

    private RequestResult(string requestId, bool hasResult, string result, IList<Request> subRequests, byte[] output)
    {
      Debug.Assert(!string.IsNullOrEmpty(requestId));
      Debug.Assert(subRequests is not null);
      Debug.Assert(output is not null);
      if (hasResult)
      {
        Debug.Assert(!subRequests.Any());
        Debug.Assert(!string.IsNullOrEmpty(result));
      }
      else
      {
        Debug.Assert(subRequests.Any());
        Debug.Assert(string.IsNullOrEmpty(result));
      }
      RequestId    = requestId;
      HasResult = hasResult;
      Result    = result;
      SubRequests  = subRequests;
      Output    = output;
    }

    [Pure]
    public RequestResult WithOutput(byte[] output)
    {
      Debug.Assert(output is not null);
      return new RequestResult(RequestId, HasResult, Result, SubRequests, output);
    }

    [NotNull]
    public string RequestId { get; }

    [NotNull]
    public bool HasResult { get; }

    [NotNull]
    public string Result { get; }

    /// <summary>
    /// 
    /// </summary>
    [NotNull]
    public IList<Request> SubRequests { get; }

    /// <summary>
    /// Dummy data only used to force some data transfers
    /// </summary>
    [NotNull]
    public byte[] Output { get; }
  }
}
