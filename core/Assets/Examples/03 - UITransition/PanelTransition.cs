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
            panel.onStartFadeIn += delegate ( exUIControl _sender ) {
                transform.position = Vector3.zero;
                panel.gameObject.SetActive(true);
            };
            panel.onFinishFadeOut += delegate ( exUIControl _sender ) {
                transform.position = new Vector3(2000,2000,0);
                panel.gameObject.SetActive(false);
            };

            if ( useLeftToRight ) {
                panel.onFadeIn += delegate ( exUIControl _sender, float _ratio ) {
                    transform.position = Vector3.Lerp( new Vector3( -1000, 0, 0 ),
                                                       new Vector3( 0, 0, 0 ),
                                                       _ratio );
                };
                panel.onFadeOut += delegate ( exUIControl _sender, float _ratio ) {
                    transform.position = Vector3.Lerp( new Vector3( 0, 0, 0 ),
                                                       new Vector3( 1000, 0, 0 ),
                                                       _ratio );
                };
            }
        }
    }
}
