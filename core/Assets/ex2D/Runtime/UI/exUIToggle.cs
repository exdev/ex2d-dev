// ======================================================================================
// File         : exUIToggle.cs
// Author       : Wu Jie 
// Last Change  : 10/24/2013 | 11:48:36 AM | Thursday,October
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

public class exUIToggle : exUIButton {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // event-defs
    public static new string[] eventNames = new string[] {
        "onChecked",
        "onUnchecked",
    };

    // events
    List<exUIEventListener> onChecked;
    List<exUIEventListener> onUnchecked;

    public void OnChecked   ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onChecked", onChecked, _event ); }
    public void OnUnchecked ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onUnchecked", onUnchecked, _event ); }
    
    public override void CacheEventListeners () {
        base.CacheEventListeners();

        onChecked = eventListenerTable["onChecked"];
        onUnchecked = eventListenerTable["onUnchecked"];
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

    //
    [SerializeField] protected bool isChecked_ = false;
    public bool isChecked {
        get { return isChecked_; }
        set {
            if ( isChecked_ != value ) {
                isChecked_ = value;

                if ( isChecked_ == false ) {
                    exUIEvent uiEvent = new exUIEvent();
                    uiEvent.bubbles = false;
                    OnUnchecked (uiEvent);
                }
                else {
                    exUIEvent uiEvent = new exUIEvent();
                    uiEvent.bubbles = false;
                    OnChecked (uiEvent);
                }
            }
        }
    }

    public bool isRadio = false;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();

        AddEventListener( "onClick",
                          delegate ( exUIEvent _event ) {
                              if ( isRadio ) {
                                  isChecked = true;
                              }
                              else {
                                  isChecked = !isChecked;
                              }
                          } );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Internal_SetChecked ( bool _checked ) {
        isChecked_ = _checked;
    }
}
