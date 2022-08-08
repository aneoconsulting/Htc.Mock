// EnumerableExt.cs is part of the Htc.Mock solution.
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
using System.Threading.Tasks;

namespace Htc.Mock.Utils
{
  public static class EnumerableExt
  {
    public static void EvaluateAll<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
      foreach (var element in enumerable) action(element);
    }

    public static Task WhenAll(this IEnumerable<Task> tasks) => Task.WhenAll(tasks);

    public static Task<TResult[]> WhenAll<TResult>(this IEnumerable<Task<TResult>> tasks) => Task.WhenAll(tasks);

    public static async IAsyncEnumerable<TResult> AsAsyncEnumerable<TResult>(this IEnumerable<Task<TResult>> tasks)
    {
      foreach (var task in tasks) yield return await task;
    }
  }
}
