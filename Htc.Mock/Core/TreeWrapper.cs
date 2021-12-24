// TreeWrapper.cs is part of the Htc.Mock solution.
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

namespace Htc.Mock.Core
{
  public class TreeWrapper
  {
    public byte[] Encoding { get; set; }
    public int Count { get; set; }

    public static implicit operator TreeWrapper(Tree tree)
    {
      var output = new TreeWrapper()
                   {
                     Count    = tree.Encoding.Count,
                     Encoding = new byte[tree.Encoding.Count / sizeof(byte) + 1],
                   };
      tree.Encoding.CopyTo(output.Encoding, 0);
      return output;
    }

    public static explicit operator Tree(TreeWrapper wrapper)
    {
      var encoding = new BitArray(wrapper.Encoding)
                     {
                       Length = wrapper.Count,
                     };
      return new(encoding);
    }

  }
}
