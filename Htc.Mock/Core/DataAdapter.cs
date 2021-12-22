// DataAdapter.cs is part of the Htc.Mock solution.
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

using MessagePack;
using MessagePack.Resolvers;

namespace Htc.Mock.Core
{
  public static class DataAdapter
  {
    public static byte[] BuildPayload(RunConfiguration runConfiguration, Request request)
    {
      var payloadBuilder = new PayloadBuilder { Request = request, RunConfiguration = runConfiguration };

      return MessagePackSerializer.Serialize(payloadBuilder, StandardResolverAllowPrivate.Options);
    }

    public static Tuple<RunConfiguration, Request> ReadPayload(byte[] payload)
    {
      var payloadBuilder = MessagePackSerializer.Deserialize<PayloadBuilder>(payload);

      var output = Tuple.Create(payloadBuilder.RunConfiguration, payloadBuilder.Request);

      return output;
    }

    [MessagePackObject]
    public class PayloadBuilder
    {
      [Key(1)]
      public RunConfiguration RunConfiguration { get; set; }

      [Key(2)]
      public Request Request { get; set; }
    }
  }
}
