// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;

using JetBrains.Annotations;

namespace Htc.Mock.Common
{
  [PublicAPI]
  [Serializable]
  public class AggregationRequest : Request
  {
    public AggregationRequest(string        id,
                              string        parentId,
                              int           depth,
                              IList<string> resultIdsRequired)
      : base(id)
    {
      Debug.Assert(resultIdsRequired is not null);
      Debug.Assert(depth >= 0);

      ResultIdsRequired = resultIdsRequired;

      ParentId = parentId;

      Depth = depth;
    }

    [NotNull]
    public IList<string> ResultIdsRequired { get; }

    [NotNull]
    public string ParentId { get; }

    public int Depth { get; }
  }
}
