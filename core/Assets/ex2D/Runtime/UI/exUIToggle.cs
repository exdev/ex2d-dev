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
        "onChecked",
        "onUnchecked",
    };

    // events
    public event System.Action<exUIControl> onChecked;
    public event System.Action<exUIControl> onUnchecked;

    //
    [SerializeField] protected bool isChecked_ = false;
    public bool isChecked {
        get { return isChecked_; }
        set {
            if ( isChecked_ != value ) {
                isChecked_ = value;

                if ( isChecked_ == false ) {
                    if ( onUnchecked != null )
                        onUnchecked (this);
                }
                else {
                    if ( onChecked != null )
                        onChecked (this);
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

        onClick += delegate ( exUIControl _sender ) {
            if ( isRadio ) {
                isChecked = true;
            }
            else {
                isChecked = !isChecked;
            }
        };
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Internal_SetChecked ( bool _checked ) {
        isChecked_ = _checked;
    }
}
