// ======================================================================================
// File         : exUIEffect.cs
// Author       : Wu Jie 
// Last Change  : 10/21/2013 | 16:22:57 PM | Monday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIEffect : MonoBehaviour {

    bool inverse = false;
    bool start = false;
    float timer = 0.0f;
    float duration = 1.0f;
    Vector3 src = Vector3.one;
    Vector3 dest = new Vector3( 1.5f, 1.5f, 1.0f );

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        exUIControl ctrl = GetComponent<exUIControl>();
        if ( ctrl != null ) {
            ctrl.onHoverIn += delegate ( exUIControl _sender, exHotPoint _point ) {
                start = true;
                inverse = false;
                timer = 0.0f;
                duration = 0.3f;
            };
            ctrl.onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
                start = true;
                inverse = true;
            };
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Update () {
        if ( start ) {
            if ( inverse )
                timer -= Time.deltaTime;
            else
                timer += Time.deltaTime;

            // float ratio = exEase.PingPong ( timer/duration, exEase.ExpoOut );
            float ratio = exEase.QuadOut(timer/duration);
            Vector3 result = Vector3.Lerp( src, dest, ratio );
            transform.localScale = result;

            if ( inverse ) {
                if ( timer <= 0.0f ) {
                    timer = 0.0f;
                    start = false;
                    transform.localScale = src;
                }
            }
            else {
                if ( timer >= duration ) {
                    timer = duration;
                    start = false;
                    transform.localScale = dest;
                }
            }
        }
    }
}
