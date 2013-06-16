// ======================================================================================
// File         : exEditorUtility.cs
// Author       : Wu Jie 
// Last Change  : 06/16/2013 | 14:37:44 PM | Sunday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

///////////////////////////////////////////////////////////////////////////////
///
/// editor helper function
///
///////////////////////////////////////////////////////////////////////////////

public static class exEditorUtility {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    static Texture2D texWhite;
    static Texture2D texCheckerboard;
    static Texture2D texHelp;

    ///////////////////////////////////////////////////////////////////////////////
    // special texture
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \return the helper texture
    /// return a help texture
    // ------------------------------------------------------------------ 

    public static Texture2D HelpTexture () {
        // NOTE: hack from "unity editor resources" in Unity3D Contents
        if ( texHelp == null ) {
            texHelp = EditorGUIUtility.FindTexture("_help");
            if ( texHelp == null ) {
                Debug.LogError ( "can't find help texture" );
                return null;
            }
        }
        return texHelp;
    }
}
