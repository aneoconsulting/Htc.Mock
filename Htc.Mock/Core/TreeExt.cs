// TreeExt.cs is part of the Htc.Mock solution.
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
using System.Linq;

namespace Htc.Mock.Core
{
  internal static class TreeExt
  {
    public static int[] GetShape(this Tree tree)
    {
      var shape = new Dictionary<int, int>();
      var depth = 0;
      for (var i = 0; i < tree.Encoding.Count; i++)
      {
        var b = tree.Encoding[i];

        if (!b)
        {
          --depth;
        }
        else
        {
          ++depth;
          if (!shape.ContainsKey(depth))
            shape[depth] = 1;
          else
            shape[depth] += 1;
        }
      }

      return shape.OrderBy(pair => pair.Key)
                  .Select(pair => pair.Value)
                  .ToArray();
    }

    public static string GetShapeString(this Tree tree) => string.Join(".", tree.GetShape());

    public static IEnumerable<Tree> GetSubTrees(this Tree tree)
    {
      var builder = new TreeBuilder();
      if (!tree.Encoding[0] || tree.Encoding[^1])
        throw new ArgumentException("Tree coding must start with a 1 and finish with a 0", nameof(tree));

      if (tree.Encoding.Length > 2)
      {
        for (var i = 1; i < tree.Encoding.Length - 1; i++)
        {
          var b = tree.Encoding[i];

          if (!builder.TryAdd(b))
            throw new ArgumentException("The Tree is not well formed", nameof(tree));

          if (builder.Depth == 0)
          {
            yield return builder.Build();
            builder = new TreeBuilder();
            if (i + 1 < tree.Encoding.Length - 1)
              if (!tree.Encoding[i + 1])
                throw new ArgumentException("The Tree is not well formed; tree already contains several root trees", nameof(tree));
          }
        }

        if (builder.TotalNbTasks > 0)
          throw new ArgumentException("The Tree is not well formed; tree contains a remainder piece of tree at the end", nameof(tree));
      }
    }
  }
}
