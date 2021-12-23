// IRequestRunner.cs is part of the Htc.Mock solution.
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

using System.Threading.Tasks;

using Htc.Mock.Core;

namespace Htc.Mock.RequestRunners
{
  /// <summary>
  ///   This interface defines the method used to process a request.
  ///   The classes implementing this interface are intended to have a lifecycle
  ///   corresponding to a session (i.e., the same <c>IRequestRunner</c> object
  ///   can be reused for different items belonging to the same session
  /// </summary>
  public interface IRequestRunner
  {
    /// <summary>
    ///   Process a <c>Request</c>.
    /// </summary>
    /// <param name="request">The request to process</param>
    /// <returns>The result ot the request</returns>
    /// <param name="taskId"></param>
    byte[] ProcessRequest(Request request, string taskId);
  }
}
