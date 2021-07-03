// This file is part of the Htc.Mock solution.
// 
// Copyright (c) ANEO. All rights reserved.
// * Wilfried KIRSCHENMANN (ANEO)
// 
// 

using System;
using System.Diagnostics;

using JetBrains.Annotations;

namespace Htc.Mock.Common
{
  [PublicAPI]
  [Serializable]
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

    public int Depth         { get; }
    public int NbSubrequests { get; }
  }
}
