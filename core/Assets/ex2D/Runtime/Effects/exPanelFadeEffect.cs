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
            panel.onStartFadeIn += delegate ( exUIControl _sender ) {
                panel.gameObject.SetActive(true);
                if ( colorCtrl ) {
                    colorCtrl.color = new Color( 1.0f, 1.0f, 1.0f, 0.0f );
                }
            };
            panel.onFinishFadeOut += delegate ( exUIControl _sender ) {
                panel.gameObject.SetActive(false);
            };
            panel.onFadeIn += delegate ( exUIControl _sender, float _ratio ) {
                if ( colorCtrl ) {
                    colorCtrl.color = new Color( 1.0f, 1.0f, 1.0f, _ratio );
                }
            };
            panel.onFadeOut += delegate ( exUIControl _sender, float _ratio ) {
                if ( colorCtrl ) {
                    colorCtrl.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f-_ratio );
                }
            };
        }

        inited = true;
    }
}
