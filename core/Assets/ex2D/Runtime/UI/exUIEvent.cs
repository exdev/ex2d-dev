// ======================================================================================
// File         : exUIEvent.cs
// Author       : Wu Jie 
// Last Change  : 01/28/2014 | 09:44:54 AM | Tuesday,January
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class exUIEventListener {
    public bool capturePhase = false;
    public System.Action<exUIEvent> func = null;
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public enum exUIEventPhase {
    Capture,
    Target,
    Bubble,
}

public class exUIEvent {
    public bool bubbles = true; 
    public bool cancelable = true; 

    public exUIControl target = null; // the target trigger this event
    public exUIControl currentTarget = null; // current target during event phase
    public exUIEventPhase eventPhase = exUIEventPhase.Target;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    bool isPropagationStopped_ = false;
    public bool isPropagationStopped {
        get { return isPropagationStopped_; }
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    public void StopPropagation () { isPropagationStopped_ = true; }
    public void Reset () { 
        isPropagationStopped_ = false; 
        target = null;
        currentTarget = null;
    }
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class exUIFocusEvent : exUIEvent {
    public exUIControl relatedTarget = null;
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public struct exUIPointInfo {
    public int id; 
    public Vector2 pos;
    public Vector2 delta;
    public Vector3 worldPos;
    public Vector3 worldDelta;
}

public class exUIPointEvent : exUIEvent {

    public bool altKey = false;
    public bool ctrlKey = false;
    public bool metaKey = false;
    public bool isMouse = false;
    public bool isTouch { get { return isMouse == false; } }  
    public exUIPointInfo[] pointInfos;

    // 0: left, 1: right, 2: middle
    public bool GetMouseButton ( int _id ) { 
        if ( isMouse ) {
            for ( int i = 0; i < pointInfos.Length; ++i ) {
                if ( pointInfos[i].id == _id )
                    return true;
            }
        }
        return false;
    }
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class exUIWheelEvent : exUIEvent {
    public float delta = 0.0f;
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class exUIRatioEvent : exUIEvent {
    public float ratio = 0.0f;
}
