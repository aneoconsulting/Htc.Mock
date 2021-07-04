/* FinalRequest.cs is part of the Htc.Mock solution.
    
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


using JetBrains.Annotations;

using ProtoBuf;

namespace Htc.Mock.Core
{
  [PublicAPI]
  [ProtoContract]
  public class FinalRequest : Request
  {
    private const string HeadId = "HeadId";
    

    public FinalRequest()
      : this(HeadId)
    {
    }

    public FinalRequest(string id)
      : base(id)
    {
    }
  }
}
