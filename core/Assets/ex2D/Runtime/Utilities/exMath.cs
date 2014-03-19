// ======================================================================================
// File         : exMesh.cs
// Author       : 
// Last Change  : 08/17/2013 | 15:27:56
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
        if (_wrapMode == WrapMode.Loop) {
            return _value % (_maxValue + 1);
        }
        else if (_wrapMode == WrapMode.PingPong) {
            int cnt = _value / _maxValue;
            _value %= _maxValue;
            if ((cnt & 0x1) == 1) {
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

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public static float SpringLerp ( float _strength, float _deltaTime ) {
		if ( _deltaTime > 1.0f ) 
            _deltaTime = 1.0f;

		int ms = Mathf.RoundToInt(_deltaTime * 1000.0f);
		_deltaTime = 0.001f * _strength;

		float r = 0.0f;
		for ( int i = 0; i < ms; ++i ) 
            r = Mathf.Lerp(r, 1.0f, _deltaTime);
		return r;
	}
	public static float SpringLerp ( float _from, float _to, float _strength, float _deltaTime ) {
		if ( _deltaTime > 1.0f ) 
            _deltaTime = 1.0f;

		int ms = Mathf.RoundToInt(_deltaTime * 1000.0f);
		_deltaTime = 0.001f * _strength;

        float r = _from;
		for ( int i = 0; i < ms; ++i ) 
            r = Mathf.Lerp(r, _to, _deltaTime);
		return r;
	}
	public static Vector2 SpringLerp ( Vector2 _from, Vector2 _to, float _strength, float _deltaTime ) {
		return Vector2.Lerp ( _from, _to, SpringLerp(_strength, _deltaTime) );
	}
	public static Vector3 SpringLerp ( Vector3 _from, Vector3 _to, float _strength, float _deltaTime ) {
		return Vector3.Lerp ( _from, _to, SpringLerp(_strength, _deltaTime) );
	}

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static float Lerp ( float _from, float _to, float _v ) {
        return _from * ( 1.0f - _v ) + _to * _v;
    }
    public static Vector2 Lerp ( Vector2 _from, Vector2 _to, float _v ) {
        return _from * ( 1.0f - _v ) + _to * _v;
    }
    public static Vector3 Lerp ( Vector3 _from, Vector3 _to, float _v ) {
        return _from * ( 1.0f - _v ) + _to * _v;
    }

    // ------------------------------------------------------------------ 
    // No clamp version of Color.Lerp
    // ------------------------------------------------------------------ 

    public static Color Lerp ( Color _from, Color _to, float _v ) {
        return new Color(_from.r + ((_to.r - _from.r) * _v), _from.g + ((_to.g - _from.g) * _v), _from.b + ((_to.b - _from.b) * _v), _from.a + ((_to.a - _from.a) * _v));
    }
    public static Rect Lerp ( Rect _from, Rect _to, float _v ) {
        Rect rect = new Rect();
        rect.x = _from.x * (1.0f - _v) + _to.x * _v;
        rect.y = _from.y * (1.0f - _v) + _to.y * _v;
        rect.width = _from.width * (1.0f - _v) + _to.width * _v;
        rect.height = _from.height * (1.0f - _v) + _to.height * _v;
        return rect;
    }
}
