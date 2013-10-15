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

    public static new EventTrigger[] eventDefs = new EventTrigger[] {
        new EventTrigger ( "onClick",      new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventTrigger ( "onButtonDown", new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventTrigger ( "onButtonUp",   new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
    };

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

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

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override EventTrigger GetEvent ( string _name ) {
        EventTrigger eventTrigger = exUIControl.FindEvent ( eventDefs, _name );
        if ( eventTrigger == null )
            eventTrigger = base.GetEvent(_name);
        return eventTrigger;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override string[] GetEventNames () {
        string[] baseNames = base.GetEventNames();
        string[] names = new string[baseNames.Length + eventDefs.Length];

        for ( int i = 0; i < baseNames.Length; ++i ) {
            names[i] = baseNames[i];
        }

        for ( int i = 0; i < eventDefs.Length; ++i ) {
            names[i+baseNames.Length] = eventDefs[i].name;
        }

        return names;
    }
}
