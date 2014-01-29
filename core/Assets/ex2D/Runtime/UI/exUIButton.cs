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

    public void OnClick      ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onClick,      _event ); }
    public void OnButtonDown ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onButtonDown, _event ); }
    public void OnButtonUp   ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onButtonUp,   _event ); }
    
    public override void CacheEventListeners () {
        base.CacheEventListeners();

        onClick = eventListenerTable["onClick"];
        onButtonDown = eventListenerTable["onButtonDown"];
        onButtonUp = eventListenerTable["onButtonUp"];
    }

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

    public bool allowDrag = true;
    public float dragThreshold = 40.0f;

    //
    bool pressing = false;
    int pressingID = -1;
    Vector2 pressDownAt = Vector2.zero; 

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
                                  pressDownAt = pointEvent.mainPoint.pos;
                                  pressingID = pointEvent.mainPoint.id;

                                  exUIMng.inst.SetFocus(this);

                                  exUIEvent evtButtonDown = new exUIEvent();
                                  evtButtonDown.bubbles = false;
                                  OnButtonDown(evtButtonDown);

                                  _event.StopPropagation();
                              }
                          } );

        AddEventListener( "onPressUp", 
                          delegate ( exUIEvent _event ) {
                              exUIPointEvent pointEvent = _event as exUIPointEvent;
                              if ( pointEvent.isTouch || pointEvent.GetMouseButton(0) ) {
                                  exUIEvent evtButtonUp = new exUIEvent();
                                  evtButtonUp.bubbles = false;
                                  OnButtonUp(evtButtonUp);

                                  if ( pressing ) {
                                      pressing = false;

                                      exUIEvent evtClick = new exUIEvent();
                                      evtClick.bubbles = false;
                                      OnClick (evtClick);

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

        AddEventListener( "onHoverMove", 
                          delegate ( exUIEvent _event ) {
                              if ( pressing ) {
                                  exUIPointEvent pointEvent = _event as exUIPointEvent;

                                   for ( int i = 0; i < pointEvent.pointInfos.Length; ++i ) {
                                       exUIPointInfo point = pointEvent.pointInfos[i];
                                       if ( point.id == pressingID ) {
                                          Vector2 delta = pointEvent.mainPoint.pos - pressDownAt; 
                                          if ( allowDrag == false &&
                                               delta.sqrMagnitude >= dragThreshold * dragThreshold )
                                          {
                                              pressing = false;


                                              exUIPointEvent pointEvent2 = new exUIPointEvent();
                                              pointEvent2.isMouse = pointEvent.isMouse;
                                              pointEvent2.pointInfos = new exUIPointInfo[] {
                                                point
                                              };
                                              OnHoverOut (pointEvent2);

                                              pointEvent2.Reset();
                                              if ( parent != null )
                                                  exUIMng.inst.PressDown( pressingID, parent, pointEvent2 );
                                          }
                                          else {
                                              _event.StopPropagation();
                                          }
                                       }
                                  }
                              }
                          } );

    }
}
