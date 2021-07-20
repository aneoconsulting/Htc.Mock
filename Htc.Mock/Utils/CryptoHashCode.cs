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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Htc.Mock.Utils
{
  public class CryptoHashCode
  {
    public uint Hash { get; private set; } = uint.MaxValue;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(byte input)
    {
      Hash = ChecksumTable[(Hash ^ input) & byte.MaxValue] ^ (Hash >> 8);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(byte[] input)
    {
      // ReSharper disable once ForCanBeConvertedToForeach
      for (var i = 0; i < input.Length; i++)
      {
        Add(input[i]);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(ReadOnlySpan<byte> input)
    {
      // ReSharper disable once ForCanBeConvertedToForeach
      for (var i = 0; i < input.Length; i++)
      {
        Add(input[i]);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>(T input) where T : struct
      => Add(Unsafe.As<byte[]>(input));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>(T[] input) where T : struct
      => Add(Unsafe.As<byte[]>(input));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add<T>(IEnumerable<T> input) where T : struct
    {
      foreach (var element in input)
      {
        Add(element);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(string input) => Add(MemoryMarshal.AsBytes(input.AsSpan()));


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(IEnumerable<string> input)
    {
      foreach (var element in input)
      {
        Add(element);
      }
    }
    




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
  }

  public static class CryptoHashCodeExt
  {

    public static uint GetCryptoHashCode(this string input)
    {
      var coder = new CryptoHashCode();
      coder.Add(input);
      return coder.Hash;
    }

    public static uint GetCryptoHashCode(this IEnumerable<string> enumerable)
    {
      var coder = new CryptoHashCode();
      coder.Add(enumerable);
      return coder.Hash;
    }
  }
}