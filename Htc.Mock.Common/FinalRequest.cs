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
