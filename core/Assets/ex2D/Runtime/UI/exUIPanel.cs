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

    public override EventDef GetEventDef ( string _name ) {
        EventDef eventDef = exUIControl.FindEventDef ( eventDefs, _name );
        if ( eventDef == null )
            eventDef = base.GetEventDef(_name);
        return eventDef;
    }

    public override string[] GetEventDefNames () {
        string[] baseNames = base.GetEventDefNames();
        string[] names = new string[baseNames.Length + eventDefs.Length];

        for ( int i = 0; i < baseNames.Length; ++i ) {
            names[i] = baseNames[i];
        }

        for ( int i = 0; i < eventDefs.Length; ++i ) {
            names[i+baseNames.Length] = eventDefs[i].name;
        }

        return names;
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // event-defs
    public static new EventDef[] eventDefs = new EventDef[] {
        new EventDef ( "onEnter",         new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onExit",          new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),

        new EventDef ( "onStartFadeIn",   new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onFinishFadeIn",  new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onFadeIn",        new Type[] { typeof(exUIControl), typeof(float) }, typeof(Action<exUIControl,float>) ),

        new EventDef ( "onStartFadeOut",  new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onFinishFadeOut", new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onFadeOut",       new Type[] { typeof(exUIControl), typeof(float) }, typeof(Action<exUIControl,float>) ),
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
