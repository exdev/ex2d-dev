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

public enum EffectEventType {
    Deactive,
    Press,
    Hover,
    Unchecked,
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
    [System.Serializable]
    public class PropInfo {
        public EffectEventType type;
        public Vector3 val;
    }

    public Transform target = null; 
    public Vector3 normal = Vector3.one;
    public List<PropInfo> propInfos = new List<PropInfo>();

    public Vector3 GetValue ( EffectEventType _type ) {
        for ( int i = 0; i < propInfos.Count; ++i ) {
            PropInfo propInfo = propInfos[i];
            if ( propInfo.type == _type )
                return propInfo.val;
        }
        return normal;
    }
}

// Color
[System.Serializable]
public class EffectInfo_Color : EffectInfo_Base {
    [System.Serializable]
    public class PropInfo {
        public EffectEventType type;
        public Color val;
    }

    public exSpriteBase target = null; 
    public Color normal = Color.white;
    public List<PropInfo> propInfos = new List<PropInfo>();

    public Color GetValue ( EffectEventType _type ) {
        for ( int i = 0; i < propInfos.Count; ++i ) {
            PropInfo propInfo = propInfos[i];
            if ( propInfo.type == _type )
                return propInfo.val;
        }
        return normal;
    }
}

// Offset
[System.Serializable]
public class EffectInfo_Offset : EffectInfo_Base {
    [System.Serializable]
    public class PropInfo {
        public EffectEventType type;
        public Vector2 val;
    }

    public exSpriteBase target = null; 
    public Vector2 normal = Vector2.one;
    public List<PropInfo> propInfos = new List<PropInfo>();

    public Vector2 GetValue ( EffectEventType _type ) {
        for ( int i = 0; i < propInfos.Count; ++i ) {
            PropInfo propInfo = propInfos[i];
            if ( propInfo.type == _type )
                return propInfo.val;
        }
        return normal;
    }
}

///////////////////////////////////////////////////////////////////////////////
// EffectState
///////////////////////////////////////////////////////////////////////////////

public class EffectState_Base {
    public System.Func<float,float> func;
    protected float timer;
    protected bool start;

    public virtual bool Tick ( float _delta ) {
        return true;
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

        return !start;
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

        return !start;
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

        return !start;
    }
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

                    EffectState_Scale state = new EffectState_Scale();
                    state.info = info;
                    state.func = info.GetCurveFunction();
                    AddState_Scale (ctrl, state);
                }
            }

            if ( offsetInfos != null ) {
                for ( int j = 0; j < offsetInfos.Count; ++j ) {
                    EffectInfo_Offset info = offsetInfos[j];

                    EffectState_Offset state = new EffectState_Offset();
                    state.info = info;
                    state.func = info.GetCurveFunction();
                    AddState_Offset (ctrl, state);
                }
            }

            if ( colorInfos != null ) {
                for ( int j = 0; j < colorInfos.Count; ++j ) {
                    EffectInfo_Color info = colorInfos[j];

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
        bool allFinished = true;
        for ( int i = 0; i < states.Count; ++i ) {
            bool finished = states[i].Tick( Time.deltaTime );
            if ( finished == false )
                allFinished = false;
        }
        if ( allFinished )
            enabled = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddEffect_Scale ( Transform _target, EffectEventType _type, exEase.Type _curveType, Vector3 _to, float _duration ) {
        exUIControl ctrl = GetComponent<exUIControl>();
        if ( ctrl ) {
            EffectInfo_Scale info = new EffectInfo_Scale();
            info.duration = _duration;
            info.target = _target;
            info.normal = _target.localScale;
            info.curveType = _curveType;

            EffectInfo_Scale.PropInfo propInfo = new EffectInfo_Scale.PropInfo();
            propInfo.type = _type;
            propInfo.val = _to;
            info.propInfos.Add(propInfo);

            EffectState_Scale state = new EffectState_Scale();
            state.info = info;
            state.func = info.GetCurveFunction();
            AddState_Scale( ctrl, state );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddEffect_Color ( exSpriteBase _target, EffectEventType _type, exEase.Type _curveType, Color _to, float _duration ) {
        exUIControl ctrl = GetComponent<exUIControl>();
        if ( ctrl ) {
            EffectInfo_Color info = new EffectInfo_Color();
            info.duration = _duration;
            info.target = _target;
            info.normal = _target.color;
            info.curveType = _curveType;

            EffectInfo_Color.PropInfo propInfo = new EffectInfo_Color.PropInfo();
            propInfo.type = _type;
            propInfo.val = _to;
            info.propInfos.Add(propInfo);

            EffectState_Color state = new EffectState_Color();
            state.info = info;
            state.func = info.GetCurveFunction();
            AddState_Color( ctrl, state );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddEffect_Offset ( exSpriteBase _target, EffectEventType _type, exEase.Type _curveType, Vector2 _to, float _duration ) {
        exUIControl ctrl = GetComponent<exUIControl>();
        if ( ctrl ) {
            EffectInfo_Offset info = new EffectInfo_Offset();
            info.duration = _duration;
            info.target = _target;
            info.normal = _target.offset;
            info.curveType = _curveType;

            EffectInfo_Offset.PropInfo propInfo = new EffectInfo_Offset.PropInfo();
            propInfo.type = _type;
            propInfo.val = _to;
            info.propInfos.Add(propInfo);

            EffectState_Offset state = new EffectState_Offset();
            state.info = info;
            state.func = info.GetCurveFunction();
            AddState_Offset( ctrl, state );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddState_Scale ( exUIControl _ctrl, EffectState_Scale _state ) {
        for ( int i = 0; i < _state.info.propInfos.Count; ++i ) {
            EffectInfo_Scale.PropInfo propInfo = _state.info.propInfos[i];
            switch ( propInfo.type ) {
            case EffectEventType.Deactive:
                _ctrl.onDeactive += delegate ( exUIControl _sender ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onActive += delegate ( exUIControl _sender ) {
                    enabled = true;
                    _state.Begin( _state.info.normal );
                };
                break;

            case EffectEventType.Press:
                _ctrl.onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( _state.info.GetValue( EffectEventType.Hover ) );
                };
                break;

            case EffectEventType.Hover:
                _ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( _state.info.normal );
                };
                break;

            case EffectEventType.Unchecked:
                exUIToggle toggle = _ctrl as exUIToggle;
                if ( toggle != null ) {
                    toggle.onUnchecked += delegate ( exUIControl _sender ) {
                        enabled = true;
                        _state.Begin( propInfo.val );
                    };
                    toggle.onChecked += delegate ( exUIControl _sender ) {
                        enabled = true;
                        _state.Begin( _state.info.GetValue( EffectEventType.Hover ) );
                    };
                }
                break;
            }
        }

        states.Add(_state);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddState_Offset ( exUIControl _ctrl, EffectState_Offset _state ) {
        for ( int i = 0; i < _state.info.propInfos.Count; ++i ) {
            EffectInfo_Offset.PropInfo propInfo = _state.info.propInfos[i];
            switch ( propInfo.type ) {
            case EffectEventType.Deactive:
                _ctrl.onDeactive += delegate ( exUIControl _sender ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onActive += delegate ( exUIControl _sender ) {
                    enabled = true;
                    _state.Begin( _state.info.normal );
                };
                break;

            case EffectEventType.Press:
                _ctrl.onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( _state.info.GetValue( EffectEventType.Hover ) );
                };
                break;

            case EffectEventType.Hover:
                _ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( _state.info.normal );
                };
                break;

            case EffectEventType.Unchecked:
                exUIToggle toggle = _ctrl as exUIToggle;
                if ( toggle != null ) {
                    toggle.onUnchecked += delegate ( exUIControl _sender ) {
                        enabled = true;
                        _state.Begin( propInfo.val );
                    };
                    toggle.onChecked += delegate ( exUIControl _sender ) {
                        enabled = true;
                        _state.Begin( _state.info.GetValue( EffectEventType.Hover ) );
                    };
                }
                break;
            }
        }

        states.Add(_state);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddState_Color ( exUIControl _ctrl, EffectState_Color _state ) {
        for ( int i = 0; i < _state.info.propInfos.Count; ++i ) {
            EffectInfo_Color.PropInfo propInfo = _state.info.propInfos[i];
            switch ( propInfo.type ) {
            case EffectEventType.Deactive:
                _ctrl.onDeactive += delegate ( exUIControl _sender ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onActive += delegate ( exUIControl _sender ) {
                    enabled = true;
                    _state.Begin( _state.info.normal );
                };
                break;

            case EffectEventType.Press:
                _ctrl.onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( _state.info.GetValue( EffectEventType.Hover ) );
                };
                break;

            case EffectEventType.Hover:
                _ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( propInfo.val );
                };
                _ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                    enabled = true;
                    _state.Begin( _state.info.normal );
                };
                break;

            case EffectEventType.Unchecked:
                exUIToggle toggle = _ctrl as exUIToggle;
                if ( toggle != null ) {
                    toggle.onUnchecked += delegate ( exUIControl _sender ) {
                        enabled = true;
                        _state.Begin( propInfo.val );
                    };
                    toggle.onChecked += delegate ( exUIControl _sender ) {
                        enabled = true;
                        _state.Begin( _state.info.GetValue( EffectEventType.Hover ) );
                    };
                }
                break;
            }
        }

        states.Add(_state);
    }
}
