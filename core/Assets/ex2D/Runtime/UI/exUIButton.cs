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

    // events
    public event System.Action<exUIControl> onClick;
    public event System.Action<exUIControl> onButtonDown;
    public event System.Action<exUIControl> onButtonUp;

    // event slots
    public EventSlot[] exUIButton_events = new EventSlot[] {
        new EventSlot ( "onClick",      new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventSlot ( "onButtonDown", new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventSlot ( "onButtonUp",   new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
    };

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
        InitEvents (exUIButton_events);

        onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
            // only accept on hot-point
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
            if ( onButtonUp != null ) onButtonUp (this);

            if ( pressing ) {
                pressing = false;
                if ( onClick != null ) onClick (this);
            }
        };

        onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
            pressing = false;
        };
    }
}
