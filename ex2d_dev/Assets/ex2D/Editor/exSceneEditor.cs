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

	static int sceneFieldHash = "SceneField".GetHashCode();

    float scale_ = 1.0f;
    float scale {
        get { return scale_; }
        set {
            if ( scale_ != value ) {
                scale_ = value;
                scale_ = Mathf.Clamp( scale_, 0.1f, 10.0f );
                scale_ = Mathf.Round( scale_ * 100.0f ) / 100.0f;
            }
        }
    }

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

        EditorGUILayout.BeginHorizontal();
            //
            LayerField ();

            //
            GUILayout.Space(40);

            // scene filed
            Rect lastRect = GUILayoutUtility.GetLastRect ();  
            int margin = 40; 
            Rect sceneRect = new Rect( lastRect.xMax + margin, 
                                       lastRect.yMax + margin, 
                                       position.width - lastRect.xMax - margin*2,
                                       position.height - lastRect.yMax - margin*2 );
            sceneRect = exGeometryUtility.Rect_FloorToInt(sceneRect);
            SceneField(sceneRect);
        EditorGUILayout.EndHorizontal();
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
                                                 10.0f, 
                                                 GUILayout.MinWidth(50),
                                                 GUILayout.MaxWidth(150) );
            GUILayout.Space(5);
            scale = EditorGUILayout.FloatField( scale,
                                                EditorStyles.toolbarTextField,
                                                GUILayout.Width(30) );

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LayerField () {
        EditorGUILayout.BeginVertical( GUILayout.Width(200), GUILayout.MinWidth(200), GUILayout.MaxWidth(200) );
            EditorGUILayout.LabelField ( "Layers" );
            EditorGUILayout.LabelField ( "editCameraPos = " + editCameraPos );
        EditorGUILayout.EndVertical();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void SceneField ( Rect _rect ) {
        int controlID = GUIUtility.GetControlID(sceneFieldHash, FocusType.Passive);

        GUILayoutUtility.GetRect ( _rect.width+4, _rect.height+4, GUI.skin.box );

        float half_w = _rect.width/2.0f;
        float half_h = _rect.height/2.0f;

        Event e = Event.current;
        switch ( e.type ) {
        case EventType.Repaint:
            Color old = GUI.color;
            GUI.color = background;
                Texture2D checker = exEditorUtility.CheckerboardTexture();
                // background
                GUI.DrawTextureWithTexCoords ( _rect, checker, 
                                               new Rect( (-half_w + editCameraPos.x)/(checker.width * scale), 
                                                         (-half_h + editCameraPos.y)/(checker.height * scale), 
                                                         _rect.width/(checker.width * scale), 
                                                         _rect.height/(checker.height * scale) ) );

                // center line
                float center_x = -editCameraPos.x + _rect.x + half_w;
                float center_y =  editCameraPos.y + _rect.y + half_h;
                if ( center_y >= _rect.y && center_y <= _rect.yMax )
                    exEditorUtility.DrawLine ( _rect.x, center_y, _rect.xMax, center_y, Color.white, 1 );
                if ( center_x >= _rect.x && center_x <= _rect.xMax ) {
                    exEditorUtility.DrawLine ( center_x, _rect.y, center_x, _rect.yMax, Color.white, 1 );
                }

                // border
                exEditorUtility.DrawRect( new Rect ( _rect.x-2, _rect.y-2, _rect.width+4, _rect.height+4 ),
                                          new Color( 1,1,1,0 ), 
                                          Color.white );
            GUI.color = old;
            break;

        case EventType.ScrollWheel:
            if ( _rect.Contains(e.mousePosition) ) {
                scale += -e.delta.y * 0.01f;

                Repaint();
                e.Use();
            }
            break;

        case EventType.MouseDown:
            if ( _rect.Contains(e.mousePosition) ) {
                if ( e.button == 1 && e.clickCount == 1 ) {
                    GUIUtility.hotControl = controlID;
                    GUIUtility.keyboardControl = controlID;

                    Repaint();
                    e.Use();
                }
            }
            break;

        case EventType.MouseDrag:
            if ( GUIUtility.hotControl == controlID ) {
                editCameraPos.x -= e.delta.x;
                editCameraPos.y += e.delta.y;

                Repaint();
                e.Use();
            }
            break;

        case EventType.MouseUp:
			if ( GUIUtility.hotControl == controlID ) {
				GUIUtility.hotControl = 0;

                Repaint();
                e.Use();
			}
            break;
        }
    }
}
