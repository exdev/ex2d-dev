// ======================================================================================
// File         : UITransitionCtrl.cs
// Author       : Wu Jie 
// Last Change  : 11/12/2013 | 10:24:54 AM | Tuesday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
// \class GameStageUI 
// 
// \brief 
// 
///////////////////////////////////////////////////////////////////////////////

public class UITransitionCtrl : FSMBase {

    public exUIPanel panelA;
    public exUIPanel panelB;
    public exUIPanel panelC;
    public float fadeDuration = 0.5f;

    int idx = 0;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        base.Init();

        // add root gui (NOTE: exUIMng can not find deactived GameObject)
        exUIMng.inst.AddControl ( panelA );
        exUIMng.inst.AddControl ( panelB );
        exUIMng.inst.AddControl ( panelC );


        //
        InitStateMachine ();
        idx = 0;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Start () {
        panelA.gameObject.SetActive(true);
        panelB.gameObject.SetActive(false);
        panelC.gameObject.SetActive(false);

        StartFSM();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Update () {
        Tick(); // NOTE: you must manually Tick the state-machine
    }

    ///////////////////////////////////////////////////////////////////////////////
    // state machine init
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    bool cond_IsA () { return idx == 0; }
    bool cond_IsB () { return idx == 1; }
    bool cond_IsC () { return idx == 2; }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void InitStateMachine () {

        // ======================================================== 
        // init states 
        // ======================================================== 

        fsm.UIState stateA = new fsm.UIState( panelA, stateMachine );
        fsm.UIState stateB = new fsm.UIState( panelB, stateMachine );
        fsm.UIState stateC = new fsm.UIState( panelC, stateMachine );

        // ======================================================== 
        // init transitions
        // ======================================================== 

        // 
        stateA.to ( stateB, cond_IsB, fadeDuration );

        // 
        stateB.to ( stateA, cond_IsA, fadeDuration );
        stateB.to ( stateC, cond_IsC, fadeDuration );

        // 
        stateC.to ( stateB, cond_IsB, fadeDuration );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnNext ( exUIControl _sender ) {
        ++idx;
        if ( idx > 2 )
            idx = 2;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnBack ( exUIControl _sender ) {
        --idx;
        if ( idx < 0 )
            idx = 0;
    }
}
