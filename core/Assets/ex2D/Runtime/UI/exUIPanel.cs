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
    List<exUIEventListener> onEnter; // same frame after transition.onEnd invoked
    List<exUIEventListener> onExit;  // same frame before transition.onStart invoked

    List<exUIEventListener> onStartFadeIn;
    List<exUIEventListener> onFinishFadeIn;
    List<exUIEventListener> onFadeIn;

    List<exUIEventListener> onStartFadeOut;
    List<exUIEventListener> onFinishFadeOut;
    List<exUIEventListener> onFadeOut;

    public void OnEnter ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onEnter",    onEnter, _event ); }
    public void OnExit  ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onExit",     onExit,  _event ); }

    public void OnStartFadeIn   ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onStartFadeIn",      onStartFadeIn,  _event ); }
    public void OnFinishFadeIn  ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onFinishFadeIn",     onFinishFadeIn,  _event ); }
    public void OnFadeIn        ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onFadeIn",           onFadeIn,  _event ); }

    public void OnStartFadeOut  ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onStartFadeOut",     onStartFadeOut,  _event ); }
    public void OnFinishFadeOut ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onFinishFadeOut",    onFinishFadeOut,  _event ); }
    public void OnFadeOut       ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onFadeOut",          onFadeOut,  _event ); }

    public override void CacheEventListeners () {
        base.CacheEventListeners();

        onEnter = eventListenerTable["onEnter"];
        onExit = eventListenerTable["onExit"];
        
        onStartFadeIn = eventListenerTable["onStartFadeIn"];
        onFinishFadeIn = eventListenerTable["onFinishFadeIn"];
        onFadeIn = eventListenerTable["onFadeIn"];

        onStartFadeOut = eventListenerTable["onStartFadeOut"];
        onFinishFadeOut = eventListenerTable["onFinishFadeOut"];
        onFadeOut = eventListenerTable["onFadeOut"];
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
        exUIEvent uiEvent = new exUIEvent();
        uiEvent.bubbles = false;
        OnEnter (uiEvent);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Exit () {
        exUIEvent uiEvent = new exUIEvent();
        uiEvent.bubbles = false;
        OnExit (uiEvent);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void StartFadeIn () {
        exUIEvent uiEvent = new exUIEvent();
        uiEvent.bubbles = false;
        OnStartFadeIn (uiEvent);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FinishFadeIn () {
        exUIEvent uiEvent = new exUIEvent();
        uiEvent.bubbles = false;
        OnFinishFadeIn (uiEvent);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FadeIn ( float _ratio ) {
        exUIRatioEvent ratioEvent = new exUIRatioEvent();
        ratioEvent.bubbles = false;
        ratioEvent.ratio = _ratio;
        OnFadeIn (ratioEvent);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void StartFadeOut () {
        exUIEvent uiEvent = new exUIEvent();
        uiEvent.bubbles = false;
        OnStartFadeOut(uiEvent);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FinishFadeOut () {
        exUIEvent uiEvent = new exUIEvent();
        uiEvent.bubbles = false;
        OnFinishFadeOut(uiEvent);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FadeOut ( float _ratio ) {
        exUIRatioEvent ratioEvent = new exUIRatioEvent();
        ratioEvent.bubbles = false;
        ratioEvent.ratio = _ratio;
        OnFadeOut (ratioEvent);
    }
}
