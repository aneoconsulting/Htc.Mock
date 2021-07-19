/* GridWorker.cs is part of the Htc.Mock solution.
    
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

using Htc.Mock.Core;
using Htc.Mock.RequestRunners;

using JetBrains.Annotations;

namespace Htc.Mock
{
  [PublicAPI]
  public class GridWorker
  {
    private readonly IRequestRunnerFactory requestRunnerFactory_;

    private IRequestRunner requestRunner_  = null;
    private string         currentSession_ = string.Empty;
    
    public GridWorker(IRequestRunnerFactory requestRunnerFactory) => requestRunnerFactory_ = requestRunnerFactory;

    public byte[] Execute(string session, string taskId, byte[] payload)
    {
      var readPayload = DataAdapter.ReadPayload(payload);
      if (session != currentSession_)
      {
        requestRunner_  = requestRunnerFactory_.Create(readPayload.Item1, session);
        currentSession_ = session;
      }

      return requestRunner_.ProcessRequest(readPayload.Item2, taskId);
    }
  }
}
