// BitArray.cs is part of the Htc.Mock solution.
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
using System.Collections.Generic;

namespace Htc.Mock.Core
{
  public class BitArray : ICollection<bool>, ICloneable
  {
    private int length_;

    private byte[] data_ = Array.Empty<byte>();

    public byte[] Data
    {
      get => data_;
      init
      {
        data_   = value;
        length_ = value.Length;
      }
    }

    public int Length
    {
      get => length_;
      set
      {
        if(Data.Length != (value+sizeof(byte)-1)/sizeof(byte))
          Array.Resize(ref data_, (value+sizeof(byte)-1)/sizeof(byte));
        length_ = value;
      }
    }

    public BitArray(){}
    public BitArray(IReadOnlyList<bool> array)
    {
      Data    = new byte[(array.Count+sizeof(byte)-1)/sizeof(byte)];
      length_ = array.Count;
      for (var i = 0; i < length_; i++)
      {
        this[i] = array[i];
      }
    }


    /// <inheritdoc />
    public IEnumerator<bool> GetEnumerator() => new Enumerator(this);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <inheritdoc />
    public void Add(bool item)
    {
      ++Length;
      this[Length - 1] = item;
      throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Clear()
    {
      Length = 0;
    }

    /// <inheritdoc />
    public bool Contains(bool item) => throw new NotImplementedException();

    /// <inheritdoc />
    public void CopyTo(bool[] array, int arrayIndex)
    {
      Data.CopyTo(array,arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(bool item) => throw new NotImplementedException();

    /// <inheritdoc />
    public int Count => Length;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public object Clone() => new BitArray { Data = Data.Clone() as byte[], length_ = length_ };

    public bool this[int index]
    {
      get
      {
        if (index >= length_ || index < 0) throw new IndexOutOfRangeException();
        var byteIdx  = index % sizeof(byte);
        var blockIdx = index / sizeof(byte);
        return (Data[blockIdx] & (1 << byteIdx)) != 0;
      }
      set
      { 
        if (index >= length_ || index < 0) throw new IndexOutOfRangeException();
        var byteIdx  = index % sizeof(byte);
        var blockIdx = index / sizeof(byte);
        if (value)
        {
          Data[blockIdx] = (byte)(Data[blockIdx] | (1 << byteIdx));
        }
        else
        {
          Data[blockIdx] = (byte)(Data[blockIdx] & ~(1 << byteIdx));
        }
      }
    }

    private class Enumerator : IEnumerator<bool>
    {
      private readonly BitArray array_;
      private          int      byteIndex_  ;
      private          int      blockIndex_ ;
      private          int      GlobIndex => blockIndex_ * sizeof(byte)+byteIndex_;



      public Enumerator(BitArray array) => array_ = array;

      /// <inheritdoc />
      public bool MoveNext()
      {
        if (byteIndex_ < sizeof(byte) - 1) ++byteIndex_;
        else
        {
          byteIndex_ = 0;
          ++blockIndex_;
        }

        return GlobIndex < array_.Count;
      }

      /// <inheritdoc />
      public void Reset()
      {
        byteIndex_  = 0;
        blockIndex_ = 0;
      }

      /// <inheritdoc />
      public bool Current => (array_.Data[blockIndex_] & (1 << byteIndex_)) != 0;

      /// <inheritdoc />
      object IEnumerator.Current => Current;

      /// <inheritdoc />
      public void Dispose()
      {
      }
    }

  }
}
