/* CryptoHashCode.cs is part of the Htc.Mock solution.
    
   Copyright (c) 2021-2021 ANEO. 
     W. Kirschenmann (https://github.com/wkirschenmann)
  
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
   
       http://www.apache.org/licenses/LICENSE-2.0
   
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.

*/ 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using JetBrains.Annotations;

namespace Htc.Mock.Utils
{
  public static class CryptoHashCode
  {
    private static readonly uint[] ChecksumTable;

    private const uint Polynomial = 0xEDB88320;

    static CryptoHashCode()
    {
      ChecksumTable = new uint[0x100];

      for (uint index = 0; index < ChecksumTable.Length; ++index)
      {
        var item = index;
        for (var bit = 0; bit < 8; ++bit)
          item = (item & 1) != 0 ? Polynomial ^ (item >> 1) : item >> 1;
        ChecksumTable[index] = item;
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
    private static uint GetCryptoHashCodeInternals(this string input, uint hash)
    {
      var inputBytes = MemoryMarshal.AsBytes(input.AsSpan());

      // ReSharper disable once ForCanBeConvertedToForeach
      for (var index = 0; index < inputBytes.Length; index++)
      {
        var b = inputBytes[index];
        hash = ChecksumTable[(hash ^ b) & byte.MaxValue] ^ (hash >> 8);
      }

      return hash;
    }

    [PublicAPI]
    public static uint GetCryptoHashCode(this string input) => GetCryptoHashCodeInternals(input, uint.MaxValue);

    [PublicAPI]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static uint GetCryptoHashCode(this IEnumerable<string> enumerable)
    {
      return enumerable.Aggregate(uint.MaxValue, 
                                  (current, s) => GetCryptoHashCodeInternals(s, current));
    }
  }
}