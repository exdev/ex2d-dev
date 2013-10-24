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
// EffectState
///////////////////////////////////////////////////////////////////////////////

public class EffectState_Base {
    public System.Func<float,float> func;
    protected float timer;
    protected bool start;

    public virtual bool Tick ( float _delta ) {
        return false;
    }
}

// Scale
public class EffectState_Scale : EffectState_Base {
    public EffectInfo_Scale info;

    Vector3 from;
    Vector3 to;

    public void Begin ( Vector3 _to ) {
        timer = 0.0f;
        start = true;
        from = info.target.localScale;
        to = _to;
    }

    public override bool Tick ( float _delta ) {
        if ( start ) {
            timer += _delta;

            float ratio = func ( timer/info.duration );
            Vector3 result = Vector3.Lerp( from, to, ratio );
            if ( timer >= info.duration ) {
                timer = 0.0f;
                start = false;
                result = to;
            }

            info.target.localScale = result;
        }

        return start;
    }
}

// Offset
public class EffectState_Offset : EffectState_Base {
    public EffectInfo_Offset info;

    Vector2 from;
    Vector2 to;

    public void Begin ( Vector2 _to ) {
        timer = 0.0f;
        start = true;
        from = info.target.offset;
        to = _to;
    }

    public override bool Tick ( float _delta ) {
        if ( start ) {
            timer += _delta;

            float ratio = func ( timer/info.duration );
            Vector2 result = Vector2.Lerp( from, to, ratio );
            if ( timer >= info.duration ) {
                timer = 0.0f;
                start = false;
                result = to;
            }

            info.target.offset = result;
        }

        return start;
    }
}

// Color
public class EffectState_Color : EffectState_Base {
    public EffectInfo_Color info;

    Color from;
    Color to;

    public void Begin ( Color _to ) {
        timer = 0.0f;
        start = true;
        from = info.target.color;
        to = _to;
    }

    public override bool Tick ( float _delta ) {
        if ( start ) {
            timer += _delta;

            float ratio = func ( timer/info.duration );
            Color result = Color.Lerp( from, to, ratio );
            if ( timer >= info.duration ) {
                timer = 0.0f;
                start = false;
                result = to;
            }

            info.target.color = result;
        }

        return start;
    }
}

///////////////////////////////////////////////////////////////////////////////
// EffectInfo
///////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public class EffectInfo_Base {
    public float duration = 1.0f;
    public exEase.Type curveType = exEase.Type.Linear;
    public bool customCurve = false; 
    public AnimationCurve curve;

    public System.Func<float,float> GetCurveFunction () {
        if ( customCurve ) {
            return delegate ( float _t ) {
                return curve.Evaluate(_t);
            };
        }
        return exEase.GetEaseFunc(curveType);
    }
}

// Scale
[System.Serializable]
public class EffectInfo_Scale : EffectInfo_Base {
    public Transform target = null; 

    public bool hasDeactive = false;
    public Vector3 deactive = Vector3.one;

    public bool hasPress = false;
    public Vector3 press = Vector3.one;

    public bool hasHover = false;
    public Vector3 hover = Vector3.one;

    [System.NonSerialized] public Vector3 normal;
}

// Color
[System.Serializable]
public class EffectInfo_Color : EffectInfo_Base {
    public exSpriteBase target = null; 

    public bool hasDeactive = false;
    public Color deactive = Color.white;

    public bool hasPress = false;
    public Color press = Color.white;

    public bool hasHover = false;
    public Color hover = Color.white;

    [System.NonSerialized] public Color normal;
}

// Offset
[System.Serializable]
public class EffectInfo_Offset : EffectInfo_Base {
    public exSpriteBase target = null; 

    public bool hasDeactive = false;
    public Vector2 deactive = Vector2.one;

    public bool hasPress = false;
    public Vector2 press = Vector2.one;

    public bool hasHover = false;
    public Vector2 hover = Vector2.one;

    [System.NonSerialized] public Vector2 normal;
}

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIEffect : MonoBehaviour {

    public List<EffectInfo_Scale> scaleInfos;
    public List<EffectInfo_Offset> offsetInfos;
    public List<EffectInfo_Color> colorInfos;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    List<EffectState_Base> states = new List<EffectState_Base>();

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        exUIControl ctrl = GetComponent<exUIControl>();
        if ( ctrl ) {
            if ( scaleInfos != null ) {
                for ( int j = 0; j < scaleInfos.Count; ++j ) {
                    EffectInfo_Scale info = scaleInfos[j];
                    info.normal = info.target.localScale;

                    EffectState_Scale state = new EffectState_Scale();
                    state.info = info;
                    state.func = info.GetCurveFunction();
                    AddState_Scale (ctrl, state);
                }
            }

            if ( offsetInfos != null ) {
                for ( int j = 0; j < offsetInfos.Count; ++j ) {
                    EffectInfo_Offset info = offsetInfos[j];
                    info.normal = info.target.offset;

                    EffectState_Offset state = new EffectState_Offset();
                    state.info = info;
                    state.func = info.GetCurveFunction();
                    AddState_Offset (ctrl, state);
                }
            }

            if ( colorInfos != null ) {
                for ( int j = 0; j < colorInfos.Count; ++j ) {
                    EffectInfo_Color info = colorInfos[j];
                    info.normal = info.target.color;

                    EffectState_Color state = new EffectState_Color();
                    state.info = info;
                    state.func = info.GetCurveFunction();
                    AddState_Color (ctrl, state);
                }
            }
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Update () {
        bool allFinished = false;
        for ( int i = 0; i < states.Count; ++i ) {
            bool finished = states[i].Tick( Time.deltaTime );
            if ( finished == false )
                allFinished = true;
        }
        if ( allFinished )
            enabled = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddState_Scale ( exUIControl _ctrl, EffectState_Scale _state ) {
        if ( _state.info.hasDeactive ) {
            _ctrl.onDeactive += delegate ( exUIControl _sender ) {
                enabled = true;
                _state.Begin( _state.info.deactive );
            };
            _ctrl.onActive += delegate ( exUIControl _sender ) {
                enabled = true;
                _state.Begin( _state.info.normal );
            };
        }
        else {
            _state.info.deactive = _state.info.normal;
        }

        if ( _state.info.hasPress ) {
            _ctrl.onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.press );
            };
            _ctrl.onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.hover );
            };
        }
        else {
            _state.info.press = _state.info.normal;
        }

        if ( _state.info.hasHover ) {
            _ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.hover );
            };
            _ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.normal );
            };
        }
        else {
            _state.info.hover = _state.info.normal;
        }

        states.Add(_state);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddState_Offset ( exUIControl _ctrl, EffectState_Offset _state ) {
        if ( _state.info.hasDeactive ) {
            _ctrl.onDeactive += delegate ( exUIControl _sender ) {
                enabled = true;
                _state.Begin( _state.info.deactive );
            };
            _ctrl.onActive += delegate ( exUIControl _sender ) {
                enabled = true;
                _state.Begin( _state.info.normal );
            };
        }
        else {
            _state.info.deactive = _state.info.normal;
        }

        if ( _state.info.hasPress ) {
            _ctrl.onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.press );
            };
            _ctrl.onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.hover );
            };
        }
        else {
            _state.info.press = _state.info.normal;
        }

        if ( _state.info.hasHover ) {
            _ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.hover );
            };
            _ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.normal );
            };
        }
        else {
            _state.info.hover = _state.info.normal;
        }

        states.Add(_state);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddState_Color ( exUIControl _ctrl, EffectState_Color _state ) {
        if ( _state.info.hasDeactive ) {
            _ctrl.onDeactive += delegate ( exUIControl _sender ) {
                enabled = true;
                _state.Begin( _state.info.deactive );
            };
            _ctrl.onActive += delegate ( exUIControl _sender ) {
                enabled = true;
                _state.Begin( _state.info.normal );
            };
        }
        else {
            _state.info.deactive = _state.info.normal;
        }

        if ( _state.info.hasPress ) {
            _ctrl.onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.press );
            };
            _ctrl.onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.hover );
            };
        }
        else {
            _state.info.press = _state.info.normal;
        }

        if ( _state.info.hasHover ) {
            _ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.hover );
            };
            _ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                enabled = true;
                _state.Begin( _state.info.normal );
            };
        }
        else {
            _state.info.hover = _state.info.normal;
        }

        states.Add(_state);
    }
}
