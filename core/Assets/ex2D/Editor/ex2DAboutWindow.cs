// ======================================================================================
// File         : ex2DAboutWindow.cs
// Author       : Wu Jie 
// Last Change  : 10/02/2013 | 10:43:10 AM | Wednesday,October
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

class ex2DAboutWindow : ScriptableWizard {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        string logoPath = "Assets/ex2D/Editor/Res/Textures/ex2d_logo.png";
        float logoWidth = 150.0f;  
        float logoHeight = 150.0f;  

        float x = position.width * 0.5f - logoWidth * 0.5f;
        GUI.DrawTexture( new Rect( x, 10.0f, logoWidth, logoHeight ), 
                         (Texture2D)AssetDatabase.LoadAssetAtPath( logoPath, typeof(Texture2D) ) );
        GUILayoutUtility.GetRect ( logoWidth, logoHeight );

        //
        EditorGUILayout.Space ();
        GUILayout.Label("Build:");
        string version = "v2.0.1 (beta 4)";
        string date = "10/02/2013";
        string commit = "cc3869512b5dbcc31085c8e620a45881e492389b";
        string text = version 
            + '\n' + date 
            + '\n' + commit;

        GUILayout.BeginHorizontal();
            GUILayout.Space (10);
            // EditorGUILayout.SelectableLabel(version);
            EditorGUILayout.TextArea(text);
        GUILayout.EndHorizontal();

        //
        EditorGUILayout.Space ();
        GUILayout.Label("Develop by:");
        GUILayout.BeginHorizontal();
            GUILayout.Space (10);
            EditorGUILayout.SelectableLabel("exDev Studio (ex-dev.com)");
        GUILayout.EndHorizontal();
    }
}
