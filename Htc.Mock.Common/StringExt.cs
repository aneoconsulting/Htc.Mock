/* StringExt.cs is part of the Htc.Mock.Common solution.
    
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
using System.IO;
using System.Text;

using JetBrains.Annotations;

namespace Htc.Mock.Common
{
  internal static class StringExt
  {
    private static readonly uint[] ChecksumTable;

    private const uint Polynomial = 0xEDB88320;

    static StringExt()
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

    private static int GetCryptoHashCode(this Stream stream)
    {
      var result = 0xFFFFFFFF;

      int current;
      while ((current = stream.ReadByte()) != -1)
        result = ChecksumTable[(result & 0xFF) ^ (byte) current] ^ (result >> 8);

      var hash = BitConverter.GetBytes(~result);
      Array.Reverse(hash);
      return BitConverter.ToInt32(hash);
    }

    [PublicAPI]
    public static int GetCryptoHashCode(this byte[] data)
    {
      using var stream = new MemoryStream(data);
      return GetCryptoHashCode(stream);
    }

    [PublicAPI]
    public static int GetCryptoHashCode(this string input) => Encoding.ASCII.GetBytes(input).GetCryptoHashCode();

    [PublicAPI]
    public static int GetCryptoHashCode(this IEnumerable<string> enumerable)
    {
      using var stream = new MemoryStream();

      var writer = new StreamWriter(stream);
      foreach (var s in enumerable)
      {
        writer.Write(s);
      }

      writer.Flush();
      stream.Position = 0;

      return GetCryptoHashCode(stream);
    }
  }
}