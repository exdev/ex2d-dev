// ======================================================================================
// File         : exUIEffect.cs
// Author       : Wu Jie 
// Last Change  : 10/21/2013 | 16:22:57 PM | Monday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

// float
public class EffectState_Float {
    public bool inverse;
    public bool start;
    public bool useCurve;
    public float timer;
    public float duration;
    public System.Func<float,float> func;
    public AnimationCurve curve;
    public float src;
    public float dest;

    public float Step ( float _delta ) {
        if ( start ) {
            if ( inverse )
                timer -= _delta;
            else
                timer += _delta;

            float ratio = 0.0f;
            if ( useCurve )
                ratio = curve.Evaluate(timer/duration);
            else
                ratio = func(timer/duration);

            float result = Mathf.Lerp( src, dest, ratio );
            if ( inverse ) {
                if ( timer <= 0.0f ) {
                    timer = 0.0f;
                    start = false;
                    result = src;
                }
            }
            else {
                if ( timer >= duration ) {
                    timer = duration;
                    start = false;
                    result = dest;
                }
            }

            return result;
        }
        return src;
    }
}

// Vector2
public class EffectState_Vector2 {
    public bool inverse;
    public bool start;
    public bool useCurve;
    public float timer;
    public float duration;
    public System.Func<float,float> func;
    public AnimationCurve curve;
    public Vector2 src;
    public Vector2 dest;

    public Vector2 Step ( float _delta ) {
        if ( start ) {
            if ( inverse )
                timer -= _delta;
            else
                timer += _delta;

            float ratio = 0.0f;
            if ( useCurve )
                ratio = curve.Evaluate(timer/duration);
            else
                ratio = func(timer/duration);

            Vector2 result = Vector2.Lerp( src, dest, ratio );
            if ( inverse ) {
                if ( timer <= 0.0f ) {
                    timer = 0.0f;
                    start = false;
                    result = src;
                }
            }
            else {
                if ( timer >= duration ) {
                    timer = duration;
                    start = false;
                    result = dest;
                }
            }

            return result;
        }
        return src;
    }
}

// Vector3
public class EffectState_Vector3 {
    public bool inverse;
    public bool start;
    public bool useCurve;
    public float timer;
    public float duration;
    public System.Func<float,float> func;
    public AnimationCurve curve;
    public Vector3 src;
    public Vector3 dest;

    public Vector3 Step ( float _delta ) {
        if ( start ) {
            if ( inverse )
                timer -= _delta;
            else
                timer += _delta;

            float ratio = 0.0f;
            if ( useCurve )
                ratio = curve.Evaluate(timer/duration);
            else
                ratio = func(timer/duration);

            Vector3 result = Vector3.Lerp( src, dest, ratio );
            if ( inverse ) {
                if ( timer <= 0.0f ) {
                    timer = 0.0f;
                    start = false;
                    result = src;
                }
            }
            else {
                if ( timer >= duration ) {
                    timer = duration;
                    start = false;
                    result = dest;
                }
            }

            return result;
        }
        return src;
    }
}

// Color
public class EffectState_Color {
    public bool inverse;
    public bool start;
    public bool useCurve;
    public float timer;
    public float duration;
    public System.Func<float,float> func;
    public AnimationCurve curve;
    public Color src;
    public Color dest;

    public Color Step ( float _delta ) {
        if ( start ) {
            if ( inverse )
                timer -= _delta;
            else
                timer += _delta;

            float ratio = 0.0f;
            if ( useCurve )
                ratio = curve.Evaluate(timer/duration);
            else
                ratio = func(timer/duration);

            Color result = Color.Lerp( src, dest, ratio );
            if ( inverse ) {
                if ( timer <= 0.0f ) {
                    timer = 0.0f;
                    start = false;
                    result = src;
                }
            }
            else {
                if ( timer >= duration ) {
                    timer = duration;
                    start = false;
                    result = dest;
                }
            }

            return result;
        }
        return src;
    }
}

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIEffect : MonoBehaviour {

    // DELME { 
    bool inverse = false;
    bool start = false;
    float timer = 0.0f;
    float duration = 1.0f;
    Vector3 src = Vector3.one;
    Vector3 dest = new Vector3( 1.2f, 1.2f, 1.0f );
    // } DELME end 

    public enum EffectType {
        Scale,
        Color,
        Offset,
        Custom,
    }

    public enum EffectOp {
        Func,
        Curve,
        Animation
    }

    public class EffectInfo {
        public float duration = 1.0f;
        public EffectOp op = EffectOp.Func;

        // if op is Func, Curve
        public MonoBehaviour target; 
        public EffectType type = EffectType.Scale;
        public System.Func<float,float> func = exEase.Linear;
        public AnimationCurve curve;

        // if op is Animation
        public Animation anim; 
        public string animName;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        exUIControl ctrl = GetComponent<exUIControl>();
        if ( ctrl != null ) {
            ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                start = true;
                inverse = false;
                timer = 0.0f;
                duration = 0.2f;
            };
            ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                start = true;
                inverse = true;
            };
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Update () {
        // DELME { 
        if ( start ) {
            if ( inverse )
                timer -= Time.deltaTime;
            else
                timer += Time.deltaTime;

            // float ratio = exEase.PingPong ( timer/duration, exEase.ExpoOut );
            float ratio = exEase.ExpoInOut(timer/duration);
            Vector3 result = Vector3.Lerp( src, dest, ratio );
            transform.localScale = result;

            if ( inverse ) {
                if ( timer <= 0.0f ) {
                    timer = 0.0f;
                    start = false;
                    transform.localScale = src;
                }
            }
            else {
                if ( timer >= duration ) {
                    timer = duration;
                    start = false;
                    transform.localScale = dest;
                }
            }
        }
        // } DELME end 
    }
}
