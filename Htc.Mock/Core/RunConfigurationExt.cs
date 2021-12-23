// RunConfigurationExt.cs is part of the Htc.Mock solution.
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
using System.Collections;

using Microsoft.Extensions.Logging;

namespace Htc.Mock.Core
{
  public static class RunConfigurationExt
  {
    public static Tree BuildTree(this RunConfiguration configuration, out int[] shape, ILogger logger)
    {
      var builder = new TreeBuilder(configuration.SubTasksLevels);
      var ran     = new Random(configuration.Seed);
      var bytes   = new byte[100];

      while (builder.TotalNbTasks + (builder.Depth / 2) < configuration.TotalNbSubTasks)
      {
        ran.NextBytes(bytes);
        for (var i = 0; i < bytes.Length && builder.TotalNbTasks + (builder.Depth / 2) < configuration.TotalNbSubTasks; ++i)
        for (var j = 0; j < 8 && builder.TotalNbTasks + (builder.Depth / 2) < configuration.TotalNbSubTasks; ++j)
          builder.TryAdd((bytes[i] & (1 << j)) != 0);
      }

      shape = builder.Shape.ToArray();
      logger.LogInformation("Created encoding for tree with shape {shape}", string.Join(".", shape));
      var subtree = builder.Build();

      subtree.Encoding.Length += 2;
      for (var i = subtree.Encoding.Length - 2; i >= 1; i--) subtree.Encoding[i] = subtree.Encoding[i - 1];
      subtree.Encoding[0]  = true;
      subtree.Encoding[^1] = false;

      return subtree;
    }

    public static Request BuildRequest(this RunConfiguration configuration, out int[] shape, ILogger logger)
    {
      shape = new[] { 1 };
      return configuration.SubTasksLevels == 0
               ? new ComputeRequest("root", new Tree(new BitArray(new[] { true, false })))
               : new ComputeRequest("root", configuration.BuildTree(out shape, logger));
    }
  }
}
