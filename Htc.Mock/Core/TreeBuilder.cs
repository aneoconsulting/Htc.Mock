// TreeBuilder.cs is part of the Htc.Mock solution.
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Htc.Mock.Core
{
  internal class TreeBuilder
  {
    private readonly BitArray encoding_ = new BitArray(0);

    private readonly Dictionary<int, int> shape_ = new Dictionary<int, int>();

    public TreeBuilder(int maxDepth = -1) => MaxDepth = maxDepth;
    private int       MaxDepth     { get; }
    public  int       TotalNbTasks => encoding_.Count / 2;
    public  List<int> Shape        => shape_.OrderBy(pair => pair.Key).Select(pair => pair.Value).ToList();

    public int Depth { get; private set; }

    public bool TryAdd(bool a)
    {
      if (Depth == 0 && !a) return false;
      if (Depth == MaxDepth && a) return false;

      if (a)
      {
        ++Depth;

        if (!shape_.ContainsKey(Depth)) shape_.Add(Depth, 0);

        shape_[Depth] += 1;
      }
      else
      {
        --Depth;
      }

      encoding_.Length++;

      encoding_[^1] = a;
      return true;
    }

    public Tree Build()
    {
      while (Depth > 0) TryAdd(false);

      return new Tree(encoding_);
    }
  }
}
