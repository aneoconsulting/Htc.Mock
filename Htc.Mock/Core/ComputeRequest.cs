/* ComputeRequest.cs is part of the Htc.Mock solution.
    
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


using System.Diagnostics;

using JetBrains.Annotations;

using ProtoBuf;

namespace Htc.Mock.Core
{
  [PublicAPI]
  [ProtoContract(SkipConstructor = true)]
  public class ComputeRequest : Request
  {
    private const string HeadId = "HeadId";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="depth">0 means leafRequest</param>
    /// <param name="nbSubrequests"></param>
    public ComputeRequest(int depth,
                          int nbSubrequests)
      : this(HeadId, depth, nbSubrequests)
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="depth">0 means leafRequest</param>
    /// <param name="nbSubrequests"></param>
    public ComputeRequest(string id,
                          int    depth,
                          int    nbSubrequests)
      : base(id)
    {
      Debug.Assert(!string.IsNullOrEmpty(id));
      Debug.Assert(nbSubrequests > 1);
      Debug.Assert(depth > 0);
      Depth         = depth;
      NbSubrequests = nbSubrequests;
    }

    [ProtoMember(1)]
    public int Depth         { get; }
    [ProtoMember(2)]
    public int NbSubrequests { get; }
  }
}
