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
        style.normal.textColor = Color.gray;
        EditorGUILayout.LabelField( "General", style );

        GUILayout.Space (10);

        EditorGUI.indentLevel++;

        // ======================================================== 
        // atlas build path
        // ======================================================== 

        string newAtlasBuildPath = EditorGUILayout.TextField ( "Atlas Build Path", EditorPrefs.GetString( ex2DEditor.atlasBuildPathKey, ex2DEditor.atlasBuildPath ) );
        if ( newAtlasBuildPath != ex2DEditor.atlasBuildPath ) {
            ex2DEditor.atlasBuildPath = newAtlasBuildPath;
            EditorPrefs.SetString( ex2DEditor.atlasBuildPathKey, ex2DEditor.atlasBuildPath );
        }

        EditorGUI.indentLevel--;
    }
}
