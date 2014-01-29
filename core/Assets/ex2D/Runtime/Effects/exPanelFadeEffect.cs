// ======================================================================================
// File         : exPanelFadeEffect.cs
// Author       : Wu Jie 
// Last Change  : 11/12/2013 | 10:29:59 AM | Tuesday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

public class exPanelFadeEffect : MonoBehaviour {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    exUIPanel panel;
    exSpriteColorController colorCtrl;
    bool inited = false;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        Init();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Init () {
        if ( inited )
            return;

        panel = GetComponent<exUIPanel>();
        colorCtrl = GetComponent<exSpriteColorController>();

        if ( panel ) {
            panel.AddEventListener( "onStartFadeIn",
                                    delegate ( exUIEvent _event ) {
                                        panel.gameObject.SetActive(true);
                                        if ( colorCtrl ) {
                                            colorCtrl.color = new Color( 1.0f, 1.0f, 1.0f, 0.0f );
                                        }
                                    } );
            panel.AddEventListener( "onFinishFadeOut",
                                    delegate ( exUIEvent _event ) {
                                        panel.gameObject.SetActive(false);
                                    } );
            panel.AddEventListener( "onFadeIn",
                                    delegate ( exUIEvent _event ) {
                                        exUIRatioEvent ratioEvent = _event as exUIRatioEvent;
                                        if ( colorCtrl ) {
                                            colorCtrl.color = new Color( 1.0f, 1.0f, 1.0f, ratioEvent.ratio );
                                        }
                                    } );
            panel.AddEventListener( "onFadeOut",
                                    delegate ( exUIEvent _event ) {
                                        exUIRatioEvent ratioEvent = _event as exUIRatioEvent;
                                        if ( colorCtrl ) {
                                            colorCtrl.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f-ratioEvent.ratio );
                                        }
                                    } );
        }

        inited = true;
    }
}
