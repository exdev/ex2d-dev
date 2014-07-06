// ======================================================================================
// File         : exEase.cs
// Author       : Wu Jie 
// Last Change  : 08/06/2011 | 21:43:52 PM | Saturday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// class exEase 
// 
// Purpose: 
// 
///////////////////////////////////////////////////////////////////////////////

public class exEase {

    // ------------------------------------------------------------------ 
    // Desc: exEase.Type 
    // ------------------------------------------------------------------ 

    public enum Type {
        Linear,
        QuadIn,
        QuadOut,
        QuadInOut,
        QuadOutIn,
        CubicIn,
        CubicOut,
        CubicInOut,
        CubicOutIn,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuartOutIn,
        QuintIn,
        QuintOut,
        QuintInOut,
        QuintOutIn,
        SineIn,
        SineOut,
        SineInOut,
        SineOutIn,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        ExpoOutIn,
        CircIn,
        CircOut,
        CircInOut,
        CircOutIn,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        ElasticOutIn,
        BackIn,
        BackOut,
        BackInOut,
        BackOutIn,
        BounceIn,
        BounceOut,
        BounceInOut,
        BounceOutIn,
        Smooth,
        Fade,
        Spring,
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    private static Dictionary< exEase.Type, System.Func<float,float>> easeFunctions;
    private static bool initialized = false;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void Init () {
        if ( initialized == false ) {
            initialized = true;
            easeFunctions = new Dictionary<exEase.Type, System.Func<float,float>>();
            easeFunctions[exEase.Type.Linear] = exEase.Linear;
            easeFunctions[exEase.Type.QuadIn] = exEase.QuadIn;
            easeFunctions[exEase.Type.QuadOut] = exEase.QuadOut;
            easeFunctions[exEase.Type.QuadInOut] = exEase.QuadInOut;
            easeFunctions[exEase.Type.QuadOutIn] = exEase.QuadOutIn;
            easeFunctions[exEase.Type.CubicIn] = exEase.CubicIn;
            easeFunctions[exEase.Type.CubicOut] = exEase.CubicOut;
            easeFunctions[exEase.Type.CubicInOut] = exEase.CubicInOut;
            easeFunctions[exEase.Type.CubicOutIn] = exEase.CubicOutIn;
            easeFunctions[exEase.Type.QuartIn] = exEase.QuartIn;
            easeFunctions[exEase.Type.QuartOut] = exEase.QuartOut;
            easeFunctions[exEase.Type.QuartInOut] = exEase.QuartInOut;
            easeFunctions[exEase.Type.QuartOutIn] = exEase.QuartOutIn;
            easeFunctions[exEase.Type.QuintIn] = exEase.QuintIn;
            easeFunctions[exEase.Type.QuintOut] = exEase.QuintOut;
            easeFunctions[exEase.Type.QuintInOut] = exEase.QuintInOut;
            easeFunctions[exEase.Type.QuintOutIn] = exEase.QuintOutIn;
            easeFunctions[exEase.Type.SineIn] = exEase.SineIn;
            easeFunctions[exEase.Type.SineOut] = exEase.SineOut;
            easeFunctions[exEase.Type.SineInOut] = exEase.SineInOut;
            easeFunctions[exEase.Type.SineOutIn] = exEase.SineOutIn;
            easeFunctions[exEase.Type.ExpoIn] = exEase.ExpoIn;
            easeFunctions[exEase.Type.ExpoOut] = exEase.ExpoOut;
            easeFunctions[exEase.Type.ExpoInOut] = exEase.ExpoInOut;
            easeFunctions[exEase.Type.ExpoOutIn] = exEase.ExpoOutIn;
            easeFunctions[exEase.Type.CircIn] = exEase.CircIn;
            easeFunctions[exEase.Type.CircOut] = exEase.CircOut;
            easeFunctions[exEase.Type.CircInOut] = exEase.CircInOut;
            easeFunctions[exEase.Type.CircOutIn] = exEase.CircOutIn;
            easeFunctions[exEase.Type.ElasticIn] = exEase.ElasticIn_Simple;
            easeFunctions[exEase.Type.ElasticOut] = exEase.ElasticOut_Simple;
            easeFunctions[exEase.Type.ElasticInOut] = exEase.ElasticInOut_Simple;
            easeFunctions[exEase.Type.ElasticOutIn] = exEase.ElasticOutIn_Simple;
            easeFunctions[exEase.Type.BackIn] = exEase.BackIn_Simple;
            easeFunctions[exEase.Type.BackOut] = exEase.BackOut_Simple;
            easeFunctions[exEase.Type.BackInOut] = exEase.BackInOut_Simple;
            easeFunctions[exEase.Type.BackOutIn] = exEase.BackOutIn_Simple;
            easeFunctions[exEase.Type.BounceIn] = exEase.BounceIn_Simple;
            easeFunctions[exEase.Type.BounceOut] = exEase.BounceOut_Simple;
            easeFunctions[exEase.Type.BounceInOut] = exEase.BounceInOut_Simple;
            easeFunctions[exEase.Type.BounceOutIn] = exEase.BounceOutIn_Simple;
            easeFunctions[exEase.Type.Smooth] = exEase.Smooth;
            easeFunctions[exEase.Type.Fade] = exEase.Fade;
            easeFunctions[exEase.Type.Spring] = exEase.Spring;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static System.Func<float,float> GetEaseFunc ( exEase.Type _type ) {
        exEase.Init(); // NOTE: this can make sure we initialized the easeFunctions table.
        return easeFunctions[_type];
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static float Linear ( float _t ) { return _t; }

    // ------------------------------------------------------------------ 
    // Desc: quad 
    // ------------------------------------------------------------------ 

    public static float QuadIn ( float _t ) { return _t*_t; }
    public static float QuadOut ( float _t )  { return -_t*(_t-2); }
    public static float QuadInOut ( float _t  ) {
        _t*=2.0f;
        if (_t < 1) {
            return _t*_t/2.0f;
        } else {
            --_t;
            return -0.5f * (_t*(_t-2) - 1);
        }
    }
    public static float QuadOutIn ( float _t ) {
        if (_t < 0.5f) return QuadOut (_t*2)/2;
        return QuadIn((2*_t)-1)/2 + 0.5f;
    }

    // ------------------------------------------------------------------ 
    // Desc: cubic 
    // ------------------------------------------------------------------ 

    public static float CubicIn ( float _t ) { return _t*_t*_t; }
    public static float CubicOut ( float _t ) {
        _t-=1.0f;
        return _t*_t*_t + 1;
    }
    public static float CubicInOut ( float _t ) {
        _t*=2.0f;
        if(_t < 1) {
            return 0.5f*_t*_t*_t;
        } else {
            _t -= 2.0f;
            return 0.5f*(_t*_t*_t + 2);
        }
    }
    public static float CubicOutIn ( float _t ) {
        if (_t < 0.5f) return CubicOut (2*_t)/2;
        return CubicIn (2*_t - 1)/2 + 0.5f;
    }

    // ------------------------------------------------------------------ 
    // Desc: quart
    // ------------------------------------------------------------------ 

    public static float QuartIn ( float _t ) { return _t*_t*_t*_t; }
    public static float QuartOut ( float _t ) {
        _t-= 1.0f;
        return - (_t*_t*_t*_t- 1);
    }
    public static float QuartInOut ( float _t ) {
        _t*=2;
        if (_t < 1) return 0.5f*_t*_t*_t*_t;
        else {
            _t -= 2.0f;
            return -0.5f * (_t*_t*_t*_t- 2);
        }
    }
    public static float QuartOutIn ( float _t ) {
        if (_t < 0.5f) return QuartOut (2*_t)/2;
        return QuartIn (2*_t-1)/2 + 0.5f;
    }

    // ------------------------------------------------------------------ 
    // Desc: quint
    // ------------------------------------------------------------------ 

    public static float QuintIn ( float _t ) { return _t*_t*_t*_t*_t; }
    public static float QuintOut ( float _t ) {
        _t-=1.0f;
        return _t*_t*_t*_t*_t + 1;
    }
    public static float QuintInOut ( float _t ) {
        _t*=2.0f;
        if (_t < 1) return 0.5f*_t*_t*_t*_t*_t;
        else {
            _t -= 2.0f;
            return 0.5f*(_t*_t*_t*_t*_t + 2);
        }
    }
    public static float QuintOutIn ( float _t ) {
        if (_t < 0.5f) return QuintOut (2*_t)/2;
        return QuintIn (2*_t - 1)/2 + 0.5f;
    }

    // ------------------------------------------------------------------ 
    // Desc: sine 
    // ------------------------------------------------------------------ 

    public static float SineIn ( float _t ) {
        return (_t == 1.0f) ? 1.0f : -Mathf.Cos(_t * Mathf.PI/2) + 1.0f;
    }
    public static float SineOut ( float _t ) {
        return Mathf.Sin(_t* Mathf.PI/2);
    }
    public static float SineInOut ( float _t ) {
        return -0.5f * (Mathf.Cos(Mathf.PI*_t) - 1);
    }
    public static float SineOutIn ( float _t ) {
        if (_t < 0.5f) return SineOut (2*_t)/2;
        return SineIn (2*_t - 1)/2 + 0.5f;
    }

    // ------------------------------------------------------------------ 
    // Desc: expo
    // ------------------------------------------------------------------ 

    public static float ExpoIn ( float _t ) {
        return (_t==0 || _t == 1.0f) ? _t : Mathf.Pow(2.0f, 10 * (_t - 1)) - 0.001f;
    }
    public static float ExpoOut ( float _t ) {
        return (_t==1.0f) ? 1.0f : 1.001f * (-Mathf.Pow(2.0f, -10 * _t) + 1);
    }
    public static float ExpoInOut ( float _t ) {
        if (_t==0.0f) return 0.0f;
        if (_t==1.0f) return 1.0f;
        _t*=2.0f;
        if (_t < 1) return 0.5f * Mathf.Pow(2.0f, 10 * (_t - 1)) - 0.005f;
        return 0.5f * 1.005f * (-Mathf.Pow(2.0f, -10 * (_t - 1)) + 2);
    }
    public static float ExpoOutIn ( float _t ) {
        if (_t < 0.5f) return ExpoOut (2*_t)/2;
        return ExpoIn (2*_t - 1)/2 + 0.5f;
    }

    // ------------------------------------------------------------------ 
    // Desc: circ 
    // ------------------------------------------------------------------ 

    public static float CircIn ( float _t ) {
        return -(Mathf.Sqrt(1 - _t*_t) - 1);
    }
    public static float CircOut ( float _t ) {
        _t-= 1.0f;
        return Mathf.Sqrt(1 - _t* _t);
    }
    public static float CircInOut ( float _t ) {
        _t*=2.0f;
        if (_t < 1) {
            return -0.5f * (Mathf.Sqrt(1 - _t*_t) - 1);
        } else {
            _t -= 2.0f;
            return 0.5f * (Mathf.Sqrt(1 - _t*_t) + 1);
        }
    }
    public static float CircOutIn ( float _t ) {
        if (_t < 0.5f) return CircOut (2*_t)/2;
        return CircIn (2*_t - 1)/2 + 0.5f;
    }

    // ------------------------------------------------------------------ 
    // Desc: elastic
    // ------------------------------------------------------------------ 

    private static float ElasticInHelper ( float _t, 
                                           float _b, 
                                           float _c, 
                                           float _d, 
                                           float _a, 
                                           float _p )
    {
        float t_adj;
        float _s;

        if (_t==0) return _b;
        t_adj = _t/_d;
        if (t_adj==1) return _b+_c;

        if ( _a < Mathf.Abs (_c) ) {
            _a = _c;
            _s = _p / 4.0f;
        } else {
            _s = _p / 2*Mathf.PI * Mathf.Asin(_c/_a);
        }

        t_adj -= 1.0f;
        return -(_a*Mathf.Pow(2.0f,10*t_adj) * Mathf.Sin( (t_adj*_d-_s)*2*Mathf.PI/_p )) + _b;

    }
    private static float ElasticOutHelper ( float _t, 
                                            float _b /*dummy*/, 
                                            float _c, 
                                            float _d /*dummy*/, 
                                            float _a, 
                                            float _p )
    {
        float _s;

        if (_t==0) return 0;
        if (_t==1) return _c;

        if(_a < _c) {
            _a = _c;
            _s = _p / 4.0f;
        } else {
            _s = _p / 2*Mathf.PI * Mathf.Asin(_c / _a);
        }

        return (_a*Mathf.Pow(2.0f,-10*_t) * Mathf.Sin( (_t-_s)*2*Mathf.PI/_p ) + _c);
    }

    public static float ElasticIn ( float _t, float _a, float _p ) {
        return ElasticInHelper(_t, 0, 1, 1, _a, _p);
    }
    public static float ElasticOut ( float _t, float _a, float _p ) {
        return ElasticOutHelper(_t, 0, 1, 1, _a, _p);
    }
    public static float ElasticInOut ( float _t, float _a, float _p ) {
        float _s;

        if (_t==0) return 0.0f;
        _t*=2.0f;
        if (_t==2) return 1.0f;

        if(_a < 1.0f) {
            _a = 1.0f;
            _s = _p / 4.0f;
        } else {
            _s = _p / 2*Mathf.PI * Mathf.Asin(1.0f / _a);
        }

        if (_t < 1) return -0.5f*(_a*Mathf.Pow(2.0f,10*(_t-1)) * Mathf.Sin( (_t-1-_s)*2*Mathf.PI/_p ));
        return _a*Mathf.Pow(2.0f,-10*(_t-1)) * Mathf.Sin( (_t-1-_s)*2*Mathf.PI/_p )*0.5f + 1.0f;
    }
    public static float ElasticOutIn ( float _t, float _a, float _p ) {
        if (_t < 0.5f) return ElasticOutHelper(_t*2, 0, 0.5f, 1.0f, _a, _p);
        return ElasticInHelper(2*_t - 1.0f, 0.5f, 0.5f, 1.0f, _a, _p);
    }

    public static float ElasticIn_Simple ( float _t ) { return ElasticIn( _t, 0.1f, 0.05f ); }
    public static float ElasticOut_Simple ( float _t ) { return ElasticOut( _t, 0.1f, 0.05f ); }
    public static float ElasticInOut_Simple ( float _t ) { return ElasticInOut( _t, 0.1f, 0.05f ); }
    public static float ElasticOutIn_Simple ( float _t ) { return ElasticOutIn( _t, 0.1f, 0.05f ); }

    // ------------------------------------------------------------------ 
    // Desc: back 
    // ------------------------------------------------------------------ 

    public static float BackIn ( float _t, float _s ) {
        return _t*_t*((_s+1.0f)*_t - _s);
    }
    public static float BackOut ( float _t, float _s ) {
        _t-= 1.0f;
        return _t*_t*((_s+1.0f)*_t+ _s) + 1.0f;
    }
    public static float BackInOut ( float _t, float _s ) {
        _t *= 2.0f;
        if (_t < 1.0f) {
            _s *= 1.55f;
            return 0.5f*(_t*_t*((_s+1.0f)*_t - _s));
        } else {
            _t -= 2.0f;
            _s *= 1.55f;
            return 0.5f*(_t*_t*((_s+1.0f)*_t+ _s) + 2.0f);
        }
    }
    public static float BackOutIn ( float _t, float _s ) {
        if (_t < 0.5f) return BackOut (2.0f*_t, _s)/2.0f;
        return BackIn(2.0f*_t - 1.0f, _s)/2.0f + 0.5f;
    }

    public static float BackIn_Simple ( float _t ) { return BackIn( _t, 2.0f ); }
    public static float BackOut_Simple ( float _t ) { return BackOut( _t, 2.0f ); }
    public static float BackInOut_Simple ( float _t ) { return BackInOut( _t, 2.0f ); }
    public static float BackOutIn_Simple ( float _t ) { return BackOutIn( _t, 2.0f ); }

    // ------------------------------------------------------------------ 
    // Desc: bounce
    // ------------------------------------------------------------------ 

    private static float BounceOutHelper ( float _t, 
                                           float _c, 
                                           float _a )
    {
        if (_t == 1.0f) return _c;
        if (_t < (4/11.0f)) {
            return _c*(7.565f*_t*_t);
        } else if (_t < (8/11.0f)) {
            _t -= (6/11.0f);
            return -_a * (1.0f - (7.565f*_t*_t + 0.5f)) + _c;
        } else if (_t < (10/11.0f)) {
            _t -= (9/11.0f);
            return -_a * (1.0f - (7.565f*_t*_t + 0.935f)) + _c;
        } else {
            _t -= (21/22.0f);
            return -_a * (1.0f - (7.565f*_t*_t + 0.98435f)) + _c;
        }
    }
    public static float BounceIn ( float _t, float _a ) {
        return 1.0f - BounceOutHelper(1.0f-_t, 1.0f, _a);
    }
    public static float BounceOut ( float _t, float _a ) {
        return BounceOutHelper(_t, 1, _a);
    }
    public static float BounceInOut ( float _t, float _a ) {
        if (_t < 0.5f) return BounceIn (2*_t, _a)/2;
        else return (_t == 1.0f) ? 1.0f : BounceOut (2*_t - 1, _a)/2 + 0.5f;
    }
    public static float BounceOutIn ( float _t, float _a ) {
        if (_t < 0.5f) return BounceOutHelper(_t*2, 0.5f, _a);
        return 1.0f - BounceOutHelper (2.0f-2*_t, 0.5f, _a);
    }

    public static float BounceIn_Simple ( float _t ) { return BounceIn( _t, 2.0f ); }
    public static float BounceOut_Simple ( float _t ) { return BounceOut( _t, 2.0f ); }
    public static float BounceInOut_Simple ( float _t ) { return BounceInOut( _t, 2.0f ); }
    public static float BounceOutIn_Simple ( float _t ) { return BounceOutIn( _t, 2.0f ); }

    // ------------------------------------------------------------------ 
    // Desc: Smooth 
    // ------------------------------------------------------------------ 

    public static float Smooth ( float _t ) {
        if ( _t <= 0.0f ) return 0.0f;
        if ( _t >= 1.0f ) return 1.0f;
        return _t*_t*(3.0f - 2.0f*_t);
    }

    // ------------------------------------------------------------------ 
    // Desc: Fade
    // ------------------------------------------------------------------ 

    public static float Fade ( float _t ) {
        if ( _t <= 0.0f ) return 0.0f;
        if ( _t >= 1.0f ) return 1.0f;
        return _t*_t*_t*(_t*(_t*6.0f-15.0f)+10.0f);
    }

    // ------------------------------------------------------------------ 
    // Desc: Spring
    // ------------------------------------------------------------------ 

    public static float Spring ( float _t ) {
        _t = Mathf.Clamp01(_t);
        _t = (Mathf.Sin(_t * Mathf.PI * (0.2f + 2.5f * _t * _t * _t)) * Mathf.Pow(1f - _t, 2.2f) + _t) * (1f + (1.2f * (1f - _t)));
        return _t;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static float Punch ( float _amplitude, float _t ) {
        float s = 9;
        if (_t == 0) {
            return 0;
        }
        if (_t == 1) {
            return 0;
        }
        float period = 1 * 0.3f;
        s = period / (2 * Mathf.PI) * Mathf.Asin(0);
        return (_amplitude * Mathf.Pow(2, -10 * _t) * Mathf.Sin((_t * 1 - s) * (2 * Mathf.PI) / period));
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static float PingPong ( float _t, System.Func<float,float> _ease ) {
        float ratio = Mathf.PingPong( _t, 0.5f );
        return _ease ( ratio/0.5f );
    }
}