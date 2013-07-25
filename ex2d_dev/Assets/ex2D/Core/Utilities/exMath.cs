// ======================================================================================
// File         : exMesh.cs
// Author       : Jare
// Last Change  : 21 / 07 / 2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

public static class exMath {
    
    // ------------------------------------------------------------------ 
    /// \param _length the length used for wrapping
    /// \param _value the in value to wrap
    /// \param _wrapMode the wrap mode used for wrapping
    /// wrap the value by the wrap mode
    // ------------------------------------------------------------------ 

    public static float Wrap ( float _value, float _length, WrapMode _wrapMode ) {
        float t = Mathf.Abs(_value);
        if ( _wrapMode == WrapMode.Loop ) {
            t %= _length;
        }
        else if ( _wrapMode == WrapMode.PingPong ) {
            int cnt = (int)(t/_length);
            t %= _length;
            if ( cnt % 2 == 1 ) {
                t = _length - t;
            }
        }
        else {
            t = Mathf.Clamp( t, 0, _length );
        }
        return t;
    }

    // ------------------------------------------------------------------ 
    /// \param _maxValue the max value used for wrapping
    /// \param _value the in value to wrap
    /// \param _wrapMode the wrap mode used for wrapping
    /// wrap the value by the wrap mode
    // ------------------------------------------------------------------ 

    public static int Wrap ( int _value, int _maxValue, WrapMode _wrapMode ) {
        if (_maxValue == 0) {
            return 0;
        }
        if (_value < 0) {
            _value = -_value;
        }
        if ( _wrapMode == WrapMode.Loop ) {
            return _value % (_maxValue + 1);
        }
        else if ( _wrapMode == WrapMode.PingPong ) {
            int cnt = _value / _maxValue;
            _value %= (_maxValue);
            if ( cnt % 2 == 1 ) {
                return _maxValue - _value;
            }
        }
        else {
            if (_value < 0)
            {
                return 0;
            }
            if (_value > _maxValue)
            {
                return _maxValue;
            }
        }
        return _value;
    }
}
