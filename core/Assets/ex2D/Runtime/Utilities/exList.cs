// ======================================================================================
// File         : exList.cs
// Author       : 
// Last Change  : 08/17/2013 | 15:27:56
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
// 
/// 自定义的list，只能用于值类型，仅供内部使用，在大多数情况下，推荐使用List<T>。
/// 相比起List<T>，能够调用FastToArray直接拿到里面的array，节省了某些情况下的GC。
/// 并且直接访问buffer可以简化对struct的修改。此外某些平台上，直接访问buffer可能会有一定的性能优势。
// 
///////////////////////////////////////////////////////////////////////////////

public class exList<T> where T : struct {

    static readonly T[] emptyArray = new T[0];

    static exList<T> tempList_;
    public static exList<T> GetTempList () {
        if (tempList_ == null) {
            tempList_ = new exList<T>();
        }
        tempList_.Clear();
        // TODO: trim
        return tempList_;
    }

    public T[] buffer;
    public int Count = 0;

    ///////////////////////////////////////////////////////////////////////////////
    // Properties
    ///////////////////////////////////////////////////////////////////////////////

    public int Capacity {
        get {
            return buffer.Length;
        }
        set {
            if (value < Count) {
                throw new ArgumentOutOfRangeException();
            }
            Array.Resize<T>(ref buffer, value);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    public exList () {
        buffer = emptyArray;
    }

    public exList (int capacity) {
        if (capacity < 0) {
            throw new ArgumentOutOfRangeException("capacity");
        }
        buffer = new T[capacity];
    }

    public void Add (T _item) {
        if (Count == buffer.Length) {
            GrowIfNeeded(1);
        }
        buffer[Count++] = _item;
    }
    
    public void AddRange (int _count) {
        int num = Count + _count;
        if (num > buffer.Length) {
            Capacity = Math.Max(Math.Max(Capacity * 2, 4), num);
        }
        Count = num;
    }

    public void RemoveRange (int _index, int _count) {
        if (_count > 0) {
            Shift(_index, -_count);
        }
    }
    
    public void Clear () {
        Count = 0;  // 如果用的是引用类型，这边需要调用Array.Clear()
    }
    
    public void TrimExcess () {
        if (Count > 0) {
            if (Count < buffer.Length) {
                T[] newBuffer = new T[Count];
                System.Array.Copy (buffer, newBuffer, Count);
                buffer = newBuffer;
            }
        }
        else {
            buffer = emptyArray;
        }
    }

    /// 使用此方法会导致list变长后需要重新分配buffer
    public T[] FastToArray () {
        TrimExcess ();
        return buffer;
    }

    public T[] ToArray () {
        T[] array = new T[Count];
        Array.Copy(buffer, array, Count);
        return array;
    }

    /// 调用完该方法后，外部不能再对传入的array进行操作
    public void FromArray (ref T[] _array) {
        buffer = _array;
        Count = _array.Length;
        _array = null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////

    void GrowIfNeeded (int _newCount) {
        int num = Count + _newCount;
        if (num > buffer.Length) {
            Capacity = Math.Max(Math.Max(Capacity * 2, 4), num);    // TODO: 测试只分配目标长度的数组，避免ToArray时重新new Array的性能
        }
    }

    void Shift (int start, int delta) {
        if (delta < 0) {
            start -= delta;
        }
        if (start < Count) {
            Array.Copy(buffer, start, buffer, start + delta, Count - start);
        }
        Count += delta;
        if (delta < 0) {
            Array.Clear(buffer, Count, -delta);
        }
    }
}
