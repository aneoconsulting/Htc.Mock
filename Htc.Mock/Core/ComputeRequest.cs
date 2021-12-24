// ComputeRequest.cs is part of the Htc.Mock solution.
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


namespace Htc.Mock.Core
{
  [PublicAPI]
  public class ComputeRequest : Request
  {
    public ComputeRequest(TreeWrapper treeWrapper, string id, IList<string> dependencies)
      : base(id, dependencies)
      => TreeWrapper = treeWrapper;

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="treeWrapper"></param>
    public ComputeRequest(string      id,
                          TreeWrapper treeWrapper)
      : base(id)
      => TreeWrapper = treeWrapper ?? throw new ArgumentNullException(nameof(treeWrapper));

    public TreeWrapper TreeWrapper { get; }
    
    private Tree tree_;

    public Tree Tree
    {
      get

      {
        if (tree_ is not null) return tree_;
        tree_ = (Tree)TreeWrapper;
        return tree_;
      }
    }
  }
}
