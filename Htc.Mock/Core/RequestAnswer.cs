// RequestAnswer.cs is part of the Htc.Mock solution.
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Htc.Mock.Core.Protos;

using JetBrains.Annotations;

namespace Htc.Mock.Core
{
  public class RequestAnswer
  {
    public RequestAnswer(string requestId, string result)
      : this(requestId, result, true, Array.Empty<Request>())
    {
      Debug.Assert(!string.IsNullOrEmpty(requestId));
      Debug.Assert(!string.IsNullOrEmpty(result));
    }

    public RequestAnswer(string requestId, string resultRedirection, IEnumerable<Request> subRequests)
      : this(requestId, resultRedirection, false, subRequests)
    {
      Debug.Assert(!string.IsNullOrEmpty(requestId));
      Debug.Assert(!string.IsNullOrEmpty(resultRedirection));
      Debug.Assert(subRequests != null);
      Debug.Assert(subRequests.Any());
    }

    private RequestAnswer(string requestId, string result, bool hasResult, IEnumerable<Request> subRequests)
    {
      Debug.Assert(!string.IsNullOrEmpty(requestId));
      Debug.Assert(subRequests != null);
      if (hasResult)
      {
        Debug.Assert(!subRequests.Any());
        Debug.Assert(!string.IsNullOrEmpty(result));
      }
      else
      {
        Debug.Assert(subRequests.Any());
        Debug.Assert(!string.IsNullOrEmpty(result));
      }

      RequestId   = requestId;
      Result      = new (){HasResult = hasResult, Value = result};
      SubRequests = subRequests;
    }

    [NotNull]
    public string RequestId { get; }

    [NotNull]
    public RequestResult Result { get; }

    /// <summary>
    /// </summary>
    [NotNull]
    public IEnumerable<Request> SubRequests { get; }
  }
}
