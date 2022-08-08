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

namespace Htc.Mock.Core
{
  public record RequestResult(bool HasResult, string Value)
  {
    public RequestResult() : this(false, string.Empty){}

    public RequestResult(string value):this(true, value){}
  }

  public record RequestAnswer
  {

    public RequestAnswer(string requestId, string result)
      : this(requestId, result, true, Array.Empty<Request>())
    {
    }

    public RequestAnswer(string requestId, string resultRedirection, IEnumerable<Request> subRequests)
      : this(requestId, resultRedirection, false, subRequests)
    {
    }

    private RequestAnswer(string requestId, string result, bool hasResult, IEnumerable<Request> subRequests)
    {
      Debug.Assert(!string.IsNullOrEmpty(requestId));
      Debug.Assert(!string.IsNullOrEmpty(result));
      Debug.Assert(subRequests.Any() != hasResult);

      RequestId   = requestId;
      Result = new()
               {
                 HasResult = hasResult, 
                 Value = result,
               };
      SubRequests = subRequests;
    }

    public string RequestId { get; }

    public RequestResult Result { get; }

    public IEnumerable<Request> SubRequests { get; }
  }
}
