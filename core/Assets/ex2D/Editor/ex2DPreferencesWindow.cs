// ======================================================================================
// File         : ex2DPreferencesWindow.cs
// Author       : Wu Jie 
// Last Change  : 06/19/2013 | 22:49:43 PM | Wednesday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.IO;
using System;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

class ex2DPreferencesWindow : ScriptableWizard {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = EditorStyles.boldLabel.normal.textColor;

        EditorGUILayout.LabelField( "General", style );

        GUILayout.Space (10);

        EditorGUI.indentLevel++;

        // DISABLE { 
        // // ======================================================== 
        // // atlas build path
        // // ======================================================== 

        // string newAtlasBuildPath = EditorGUILayout.TextField ( "Atlas Build Path", EditorPrefs.GetString( ex2DEditor.atlasBuildPathKey, ex2DEditor.atlasBuildPath ) );
        // if ( newAtlasBuildPath != ex2DEditor.atlasBuildPath ) {
        //     ex2DEditor.atlasBuildPath = newAtlasBuildPath;
        //     EditorPrefs.SetString( ex2DEditor.atlasBuildPathKey, ex2DEditor.atlasBuildPath );
        // }
        // } DISABLE end 

        // ======================================================== 
        // EX_DEBUG macro
        // ======================================================== 

        if (!EditorApplication.isPlaying) {
            bool isDebug = exDebug.enabled;
            bool newIsDebug = EditorGUILayout.Toggle("Debugging", isDebug);
            if (newIsDebug != isDebug) {
                exDebug.enabled = newIsDebug;
            }
        }

        EditorGUI.indentLevel--;
    }

}
