// ======================================================================================
// File         : exSceneEditor.cs
// Author       : Wu Jie 
// Last Change  : 06/18/2013 | 17:11:09 PM | Tuesday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// the scene editor
///
///////////////////////////////////////////////////////////////////////////////

class exSceneEditor : EditorWindow {

    float scale = 1.0f;
    Color background = Color.gray;
    Vector2 editCameraPos = Vector2.zero;

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        title = "2D Scene Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = true;
        // position = new Rect ( 50, 50, 800, 600 );

        Reset();
        Repaint();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        // toolbar
        Toolbar ();

        // scene filed
        GUILayout.Space(40);
        int margin = 40; 
        SceneField ( new Rect( margin, 
                               margin, 
                               position.width - margin*2,
                               position.height - margin*2 ) );
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Reset () {
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Toolbar () {
        EditorGUILayout.BeginHorizontal ( EditorStyles.toolbar );

            GUILayout.FlexibleSpace();

            // ======================================================== 
            // zoom in/out slider 
            // ======================================================== 

            GUILayout.Label ("Zoom");
            GUILayout.Space(5);
            scale = GUILayout.HorizontalSlider ( scale, 
                                                 0.1f, 
                                                 2.0f, 
                                                 GUILayout.MinWidth(50),
                                                 GUILayout.MaxWidth(150) );
            GUILayout.Space(5);
            scale = EditorGUILayout.FloatField( scale,
                                                EditorStyles.toolbarTextField,
                                                GUILayout.Width(30) );
            scale = Mathf.Clamp( scale, 0.1f, 10.0f );
            scale = Mathf.Round( scale * 100.0f ) / 100.0f;

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void SceneField ( Rect _rect ) {
        GUILayoutUtility.GetRect ( _rect.width+2, _rect.height+2, GUI.skin.box );

        Event e = Event.current;
        switch ( e.type ) {
        case EventType.Repaint:
            Color old = GUI.color;
            GUI.color = background;
                Texture2D checker = exEditorUtility.CheckerboardTexture();
                // background
                GUI.DrawTextureWithTexCoords ( _rect, checker, 
                                               new Rect( 0.0f, 0.0f, _rect.width/checker.width, _rect.height/checker.height) );

                // center line
                int half_w = _rect.width/2;
                int half_h = _rect.height/2;

                // border
                exEditorUtility.DrawRect( new Rect ( _rect.x-2, _rect.y-2, _rect.width+4, _rect.height+4 ),
                                          new Color( 1,1,1,0 ), 
                                          Color.white );
            GUI.color = old;
            break;
        }
    }
}
