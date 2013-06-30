// ======================================================================================
// File         : ex2DAboutWindow.cs
// Author       : Wu Jie 
// Last Change  : 06/19/2013 | 22:46:46 PM | Wednesday,June
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

        string version = "ex2D v2.0.1 (beta)";
        GUILayout.Space (10);
        GUILayout.BeginHorizontal();
            GUILayout.Space (10);
            GUILayout.Label(version);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
            GUILayout.Space (10);
            GUILayout.Label("Develop by: exDev Studio (www.ex-dev.com)");
        GUILayout.EndHorizontal();
    }
}
