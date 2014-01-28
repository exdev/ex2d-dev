// ======================================================================================
// File         : exUIPanel.cs
// Author       : Wu Jie 
// Last Change  : 11/01/2013 | 16:02:18 PM | Friday,November
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
/// exUIPanel is used for UI fsm and UI transition
///
///////////////////////////////////////////////////////////////////////////////

public class exUIPanel : exUIControl {

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
        "onEnter",
        "onExit",

        "onStartFadeIn",
        "onFinishFadeIn",
        "onFadeIn",

        "onStartFadeOut",
        "onFinishFadeOut",
        "onFadeOut",
    };

    // events
    public event System.Action<exUIControl> onEnter; // same frame after transition.onEnd invoked
    public event System.Action<exUIControl> onExit;  // same frame before transition.onStart invoked

    public event System.Action<exUIControl> onStartFadeIn;
    public event System.Action<exUIControl> onFinishFadeIn;
    public event System.Action<exUIControl,float> onFadeIn;

    public event System.Action<exUIControl> onStartFadeOut;
    public event System.Action<exUIControl> onFinishFadeOut;
    public event System.Action<exUIControl,float> onFadeOut;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Enter () {
        if ( onEnter != null ) onEnter(this);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Exit () {
        if ( onExit != null ) onExit(this);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void StartFadeIn () {
        if ( onStartFadeIn != null ) onStartFadeIn(this);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FinishFadeIn () {
        if ( onFinishFadeIn != null ) onFinishFadeIn(this);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FadeIn ( float _ratio ) {
        if ( onFadeIn != null ) onFadeIn( this, _ratio );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void StartFadeOut () {
        if ( onStartFadeOut != null ) onStartFadeOut(this);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FinishFadeOut () {
        if ( onFinishFadeOut != null ) onFinishFadeOut(this);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FadeOut ( float _ratio ) {
        if ( onFadeOut != null ) onFadeOut( this, _ratio );
    }
}
