// ======================================================================================
// File         : exUIToggleGroup.cs
// Author       : Wu Jie 
// Last Change  : 10/24/2013 | 14:26:11 PM | Thursday,October
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

public class exUIToggleGroup : exUIToggle {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // event-defs
    public static new string[] eventNames = new string[] {
        "onCheckChanged",
    };

    // events
    List<exUIEventListener> onCheckChanged;

    public void OnCheckChanged   ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onCheckChanged, _event ); }
    
    public override void CacheEventListeners () {
        base.CacheEventListeners();

        onCheckChanged = eventListenerTable["onCheckChanged"];
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
    [SerializeField] protected int index_ = 0;
    public int index {
        get { return index_; }
        set {
            if ( index_ != value ) {
                index_ = value;
                if ( index_ < 0 )
                    index_ = 0;
                if ( index_ > toggles.Count-1 )
                    index_ = toggles.Count-1;

                for ( int i = 0; i < toggles.Count; ++i ) {
                    exUIToggle toggle = toggles[i];
                    if ( i == index_ ) {
                        toggle.isChecked = true;
                    }
                    else {
                        toggle.isChecked = false;
                    }
                }

                exUIEvent uiEvent = new exUIEvent();
                uiEvent.bubbles = false;
                OnCheckChanged (uiEvent);
            }
        }
    }

    public List<exUIToggle> toggles = new List<exUIToggle>();

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();

        // register events
        for ( int i = 0; i < toggles.Count; ++i ) {
            RegisterEvent(toggles[i]);
        }

        // make sure active is correct for those button
        for ( int i = 0; i < toggles.Count; ++i ) {
            exUIToggle toggle = toggles[i];
            toggle.isRadio = true;
            if ( i == index_ ) {
                toggle.Internal_SetChecked(true);
            }
            else {
                toggle.Internal_SetChecked(false);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddToggle ( exUIToggle _toggle ) {
        toggles.Add(_toggle);
        RegisterEvent(_toggle);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void RegisterEvent ( exUIToggle _toggle ) {
        _toggle.AddEventListener( "onChecked",
                                  delegate ( exUIEvent _event ) {
                                      index = toggles.IndexOf(_toggle);
                                  } ); 
    }
}
