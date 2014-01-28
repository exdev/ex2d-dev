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

public enum exUIEventPhase {
    Capture,
    Target,
    Bubble,
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class exUIEvent {
    // public bool bubbles; 
    // public bool cancelable; 

    public exUIControl target; // the target trigger this event
    public exUIControl currentTarget; // current target during event phase
    public exUIEventPhase eventPhase;

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

    public bool altKey;
    public bool ctrlKey;
    public bool metaKey;
    public bool isMouse;
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

public class exUIEventListener {
    public bool capturePhase = false;
    public System.Action<exUIEvent> func = null;
}
