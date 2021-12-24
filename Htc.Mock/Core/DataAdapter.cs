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
using System.Linq;
using System.Runtime.Serialization;

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

using Htc.Mock.Core.Protos;

namespace Htc.Mock.Core
{
  public static class DataAdapter
  {
    
    private static Protos.Request ToProto(Request request)
    {
      var r = new Protos.Request
              {
                Id           = request.Id,
                Dependencies = {request. Dependencies },
              };
      switch (request)
      {
        case AggregationRequest:
          r.AggregationRequest = new();
          break;
        case ComputeRequest cr:
          r.ComputeRequest = new()
                             {
                               Count    = cr.Tree.Encoding.Count,
                               Encoding = ByteString.CopyFrom(cr.Tree.Encoding.Data),
                             };
          break;
      }
      return r;
    }

    private static Protos.RunConfiguration ToProto(RunConfiguration configuration)
      => new()
         {
           Seed                 = configuration.Seed,
           SubTasksLevels       = configuration.SubTasksLevels,
           TotalNbSubTasks      = configuration.TotalNbSubTasks,
           AvgDurationMs        = configuration.AvgDurationMs,
           Data                 = configuration.Data,
           DurationSamples      = { configuration.DurationSamples },
           MaxDurationMs        = configuration.MaxDurationMs,
           Memory               = configuration.Memory,
           MinDurationMs        = configuration.MinDurationMs,
           TotalCalculationTime = Duration.FromTimeSpan(configuration.TotalCalculationTime),
         };

    private static Request FromProto(Protos.Request r)
    {
      switch (r.SubClassCase)
      {
        case Protos.Request.SubClassOneofCase.AggregationRequest:
          return new AggregationRequest(r.Id, r.Dependencies);
        case Protos.Request.SubClassOneofCase.ComputeRequest:
          return new ComputeRequest(new(new()
                                        {
                                          Data   = r.ComputeRequest.Encoding.ToByteArray(),
                                          Length = r.ComputeRequest.Count,
                                        }),
                                    r.Id,
                                    r.Dependencies
                                   );
        case Protos.Request.SubClassOneofCase.None:
        default:
          throw new SerializationException($"Error while deserializing {nameof(Request)}");
      }
    }

    private static RunConfiguration FromProto(Protos.RunConfiguration configuration)
      => new()
         {
           Seed                 = configuration.Seed,
           SubTasksLevels       = configuration.SubTasksLevels,
           TotalNbSubTasks      = configuration.TotalNbSubTasks,
           AvgDurationMs        = configuration.AvgDurationMs,
           Data                 = configuration.Data,
           DurationSamples      = configuration.DurationSamples.ToArray(),
           MaxDurationMs        = configuration.MaxDurationMs,
           Memory               = configuration.Memory,
           MinDurationMs        = configuration.MinDurationMs,
           TotalCalculationTime = configuration.TotalCalculationTime.ToTimeSpan(),
         };

    public static byte[] BuildPayload(RunConfiguration runConfiguration, Request request)
    {
      var payloadBuilder = new PayloadBuilder
                           {
                             Request = ToProto(request).ToByteString(), 
                             RunConfiguration = runConfiguration.Serialized.Length==0? ToProto(runConfiguration).ToByteString():runConfiguration.Serialized,
                           };

      return payloadBuilder.ToByteArray();
    }

    public static Tuple<RunConfiguration, Request> ReadPayload(byte[] payload)
    {
      var payloadBuilder = PayloadBuilder.Parser.ParseFrom(payload);

      var output = Tuple.Create(FromProto(Protos.RunConfiguration.Parser.ParseFrom(payloadBuilder.RunConfiguration)), 
                                FromProto(Protos.Request.Parser.ParseFrom(payloadBuilder.Request)));

      return output;
    }

    public static byte[] Serialize(RequestResult result)
    {
      if (result is null)
        throw new NullReferenceException("Cannot serialize a null result");
      if (string.IsNullOrEmpty(result.Value))
        throw new("Result contains no data");
      return new Protos.RequestResult
             {
               Value = result.Value,
               Type = result.HasResult
                        ? Protos.RequestResult.Types.ResultType.Value
                        : Protos.RequestResult.Types.ResultType.Forward,
             }.ToByteArray();
    }

    public static RequestResult ReadResult(byte[] input)
    {
      var result = Protos.RequestResult.Parser.ParseFrom(input);
      if (result is null)
        throw new NullReferenceException("Read a null result");
      if (result.Type == Protos.RequestResult.Types.ResultType.Unknown)
        throw new("Data received is not valid: its type cannot be unknown");
      if (string.IsNullOrEmpty(result.Value))
        throw new("Result contains no data");

      return new ()
             {
               HasResult = result.Type == Protos.RequestResult.Types.ResultType.Value,
               Value     = result.Value,
             };
    }
  }
}
