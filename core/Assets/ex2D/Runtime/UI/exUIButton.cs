// ======================================================================================
// File         : exUIButton.cs
// Author       : Wu Jie 
// Last Change  : 10/11/2013 | 15:58:38 PM | Friday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIButton : exUIControl {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    public override string[] GetEventNames () {
        string[] baseNames = base.GetEventNames();
        string[] names = new string[baseNames.Length + eventNames.Length];

        for ( int i = 0; i < baseNames.Length; ++i ) {
            names[i] = baseNames[i];
        }

        for ( int i = 0; i < eventNames.Length; ++i ) {
            names[i+baseNames.Length] = eventNames[i];
        }

        return names;
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // event-defs
    public static new string[] eventNames = new string[] {
        "onClick",
        "onButtonDown",
        "onButtonUp",
    };

    // events
    public event System.Action<exUIControl> onClick;
    public event System.Action<exUIControl> onButtonDown;
    public event System.Action<exUIControl> onButtonUp;

    //
    bool pressing = false;
    // Vector2 pressDownAt = Vector2.zero; 

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();

        onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
            if ( pressing )
                return;

            if ( _point.isTouch || _point.GetMouseButton(0) ) {
                pressing = true;
                // pressDownAt = _point.pos;

                exUIMng.inst.SetFocus(this);
                if ( onButtonDown != null ) onButtonDown (this);
            }
        };

        onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
            if ( _point.isTouch || _point.GetMouseButton(0) ) {
                if ( onButtonUp != null ) onButtonUp (this);

                if ( pressing ) {
                    pressing = false;
                    if ( onClick != null ) onClick (this);
                }
            }
        };

        onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
            pressing = false;
        };
    }
}
