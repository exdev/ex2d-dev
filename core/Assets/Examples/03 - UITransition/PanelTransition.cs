// ======================================================================================
// File         : PanelTransition.cs
// Author       : Wu Jie 
// Last Change  : 11/12/2013 | 11:01:46 AM | Tuesday,November
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

public class PanelTransition : MonoBehaviour {

    public bool useLeftToRight = false;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Awake () {
        exUIPanel panel = GetComponent<exUIPanel>();
        if ( panel ) {
            panel.AddEventListener( "onStartFadeIn",
                                    delegate ( exUIEvent _event ) {
                                        transform.position = Vector3.zero;
                                        panel.gameObject.SetActive(true);
                                    } );
            panel.AddEventListener( "onFinishFadeOut",
                                    delegate ( exUIEvent _event ) {
                                        transform.position = new Vector3(2000,2000,0);
                                        panel.gameObject.SetActive(false);
                                    } );

            if ( useLeftToRight ) {
                panel.AddEventListener( "onFadeIn",
                                        delegate ( exUIEvent _event ) {
                                            exUIRatioEvent ratioEvent = _event as exUIRatioEvent;
                                            transform.position = Vector3.Lerp( new Vector3( -1000, 0, 0 ),
                                                                               new Vector3( 0, 0, 0 ),
                                                                               ratioEvent.ratio );
                                        } );
                panel.AddEventListener( "onFadeOut",
                                        delegate ( exUIEvent _event ) {
                                            exUIRatioEvent ratioEvent = _event as exUIRatioEvent;
                                            transform.position = Vector3.Lerp( new Vector3( 0, 0, 0 ),
                                                                               new Vector3( 1000, 0, 0 ),
                                                                               ratioEvent.ratio );
                                        } );
            }
        }
    }
}
