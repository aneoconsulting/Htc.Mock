/* RequestResult.cs is part of the Htc.Mock.Common solution.
    
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
