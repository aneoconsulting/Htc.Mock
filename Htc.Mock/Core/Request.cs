// Request.cs is part of the Htc.Mock solution.
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

using JetBrains.Annotations;

using MessagePack;

namespace Htc.Mock.Core
{
  [PublicAPI]
  [MessagePackObject]
  [Union(0, typeof(ComputeRequest))]
  [Union(1, typeof(AggregationRequest))]
  public abstract class Request
  {
    protected Request(string id, IList<string> dependencies)
    {
      Id           = id;
      Dependencies = dependencies;
    }

    protected Request(string id) : this(id, Array.Empty<string>())
    {
      Id = id ?? throw new ArgumentNullException(nameof(id));
      if (string.IsNullOrEmpty(id)) throw new ArgumentException("id cannot be null or empty", nameof(id));
    }

    [NotNull]
    [Key(2)]
    public string Id { get; }

    [NotNull]
    [Key(3)]
    public IList<string> Dependencies { get; }
  }
}
