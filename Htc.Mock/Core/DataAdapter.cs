/* DataAdapter.cs is part of the Htc.Mock solution.
    
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
using System.IO;

using ProtoBuf;

namespace Htc.Mock.Core
{
  public static class DataAdapter
  {
    [ProtoContract]
    private class PayloadBuilder
    {
      [ProtoMember(1)]
      public RunConfiguration RunConfiguration { get; set; }

      [ProtoMember(2)]
      public Request Request { get; set; }
    }

    public static byte[] BuildPayload(RunConfiguration runConfiguration, Request request)
    {
      var payloadBuilder = new PayloadBuilder {Request = request, RunConfiguration = runConfiguration};

      using (var stream = new MemoryStream())
      {
        Serializer.Serialize(stream, payloadBuilder);

        stream.Position = 0;

        return stream.ToArray();
      }
    }

    public static Tuple<RunConfiguration, Request> ReadPayload(byte[] payload)
    {
      var payloadBuilder = Serializer.Deserialize<PayloadBuilder>(payload.AsSpan());

      return Tuple.Create(payloadBuilder.RunConfiguration, payloadBuilder.Request);
    }
  }
}
