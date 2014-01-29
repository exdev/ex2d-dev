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
    List<exUIEventListener> onClick;
    List<exUIEventListener> onButtonDown;
    List<exUIEventListener> onButtonUp;

    public void OnClick      ( exUIEvent _event )  { if ( onClick      != null ) exUIMng.inst.DispatchEvent( this, onClick,      _event ); }
    public void OnButtonDown ( exUIEvent _event )  { if ( onButtonDown != null ) exUIMng.inst.DispatchEvent( this, onButtonDown,    _event ); }
    public void OnButtonUp   ( exUIEvent _event )  { if ( onButtonUp   != null ) exUIMng.inst.DispatchEvent( this, onButtonUp,     _event ); }

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

        AddEventListener( "onPressDown", 
                          delegate ( exUIEvent _event ) {
                              if ( pressing )
                                  return;

                              exUIPointEvent pointEvent = _event as exUIPointEvent;

                              if ( pointEvent.isTouch || pointEvent.GetMouseButton(0) ) {
                                  pressing = true;
                                  // pressDownAt = _point.pos;

                                  exUIMng.inst.SetFocus(this);
                                  OnButtonDown( new exUIEvent() );

                                  _event.StopPropagation();
                              }
                          } );

        AddEventListener( "onPressUp", 
                          delegate ( exUIEvent _event ) {
                              exUIPointEvent pointEvent = _event as exUIPointEvent;
                              if ( pointEvent.isTouch || pointEvent.GetMouseButton(0) ) {
                                  OnButtonUp( new exUIEvent() );

                                  if ( pressing ) {
                                      pressing = false;
                                      OnClick ( new exUIEvent() );

                                      _event.StopPropagation();
                                  }
                              }
                          } );

        AddEventListener( "onHoverOut", 
                          delegate ( exUIEvent _event ) {
                              if ( pressing ) {
                                  pressing = false;
                                  _event.StopPropagation();
                              }
                          } );
    }
}
