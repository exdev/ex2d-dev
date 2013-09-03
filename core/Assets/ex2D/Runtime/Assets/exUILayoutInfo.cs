// ======================================================================================
// File         : exUILayoutInfo.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 16:43:39 PM | Friday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
///
/// The ui-layout information
///
///////////////////////////////////////////////////////////////////////////////

public class exUILayoutInfo : ScriptableObject {
    public int resolutionIdx = 0;
    public int width = 0;
    public int height = 0;

    public exUIElement root;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        LinkElement ( root );
        Apply();
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Apply () {
        if ( width < 0 )
            width = int.MaxValue;

        if ( height < 0 )
            height = int.MaxValue;

        root.Layout( 0, 0, width, height );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LinkElement ( exUIElement _el ) {
        for ( int i = 0; i < _el.children.Count; ++i ) {
            exUIElement child = _el.children[i];
            child.parent_ = _el;

            LinkElement ( child );
        }
    }
}

