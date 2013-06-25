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

    class SettingsStyles {
        public GUIStyle boldLabel = new GUIStyle();
        public GUIStyle toolbar = "TE Toolbar";
        public GUIStyle toolbarDropDown = "TE ToolbarDropDown";
        public GUIStyle boxBackground = "TE NodeBackground";
        public Texture iconToolbarPlus = EditorGUIUtility.FindTexture ("Toolbar Plus");

        public SettingsStyles() {
            boxBackground.margin = new RectOffset( 0, 0, 0, 0 );
            boxBackground.padding = new RectOffset( 0, 0, 0, 0 );
            boldLabel.fontSize = 15;
            boldLabel.fontStyle = FontStyle.Bold;
            boldLabel.normal.textColor = EditorStyles.boldLabel.normal.textColor;
        }
    }

    static SettingsStyles settingsStyles = null;
	static int sceneViewFieldHash = "SceneViewField".GetHashCode();

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
    Rect sceneViewRect = new Rect( 0, 0, 1, 1 );

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
        if ( settingsStyles == null ) {
            settingsStyles = new SettingsStyles();
        }

        // toolbar
        Toolbar ();

        GUILayout.Space(40);

        // layer & scene
        EditorGUILayout.BeginHorizontal();
            //
            Settings ();

            //
            GUILayout.Space(40);

            // scene filed
            // int margin = 40; 
            // float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
            // Layout_SceneViewField ( Mathf.FloorToInt(position.width - 210 - 40 - 10 - margin),
            //                         Mathf.FloorToInt(position.height - toolbarHeight - 40 - margin) );
        EditorGUILayout.EndHorizontal();

        // debug info
        DebugInfos ();
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
            // Reset 
            // ======================================================== 

            if ( GUILayout.Button( "Reset", EditorStyles.toolbarButton ) ) {
                editCameraPos = Vector2.zero;
            }

            // ======================================================== 
            // zoom in/out slider 
            // ======================================================== 

            GUILayout.Label ("Zoom");
            EditorGUILayout.Space();
            scale = GUILayout.HorizontalSlider ( scale, 
                                                 0.1f, 
                                                 10.0f, 
                                                 new GUILayoutOption [] {
                                                    GUILayout.MinWidth(50),
                                                    GUILayout.MaxWidth(150)
                                                 } );
            EditorGUILayout.Space();
            scale = EditorGUILayout.FloatField( scale,
                                                EditorStyles.toolbarTextField,
                                                new GUILayoutOption [] {
                                                    GUILayout.Width(30)
                                                } );

            // ======================================================== 
            // Help
            // ======================================================== 

            if ( GUILayout.Button( exEditorUtility.HelpTexture(), EditorStyles.toolbarButton ) ) {
                Help.BrowseURL("http://www.ex-dev.com/ex2d/wiki/doku.php?id=manual:scene_editor");
            }

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Settings () {
        EditorGUILayout.BeginHorizontal( new GUILayoutOption [] {
                                           GUILayout.Width(200), 
                                           GUILayout.MinWidth(200), 
                                           GUILayout.MaxWidth(200),
                                           GUILayout.ExpandWidth(false),
                                       } );
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical( settingsStyles.boxBackground );

            // ======================================================== 
            // General 
            // ======================================================== 

            EditorGUILayout.LabelField ( "General", settingsStyles.boldLabel );
            if ( ex2DMng.instance == null ) {
                Color old = GUI.color;
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField ( "Can't find ex2DMng in the scene!" );
                GUI.color = old;

                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if ( GUILayout.Button("Create...", GUILayout.Width(80) ) ) {
                        Camera ex2DCamera = Camera.main;
                        if ( ex2DCamera == null ) {
                            GameObject go = new GameObject("Main Camera");
                            ex2DCamera = go.AddComponent<Camera>();
                        }
                        ex2DCamera.gameObject.AddComponent<ex2DMng>();
                    }
                EditorGUILayout.EndHorizontal();
            }
            else {
                EditorGUILayout.ObjectField( ""
                                             , ex2DMng.instance
                                             , typeof(ex2DMng)
                                             , false 
                                           );
            }

            // ======================================================== 
            // Layers 
            // ======================================================== 

            EditorGUILayout.Space();
            EditorGUILayout.LabelField ( "Layers", settingsStyles.boldLabel );

            // TODO { 
			Rect rect = GUILayoutUtility.GetRect ( 10f, (float)(21 * ex2DMng.instance.layerList.Count) );
            // } TODO end 

            EditorGUILayout.BeginHorizontal( settingsStyles.toolbar, new GUILayoutOption[0]);
            GUILayout.FlexibleSpace();
                if ( GUILayout.Button( settingsStyles.iconToolbarPlus, 
                                       settingsStyles.toolbarDropDown ) ) 
                {
                    ex2DMng.instance.CreateLayer();
                }
            EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DebugInfos () {
        EditorGUILayout.BeginHorizontal();
            string text = "";
            int width = -1;
            
            text = "Camera Pos: " + editCameraPos;
            // width = (int)EditorStyles.label.CalcSize(new GUIContent(text)).x;
            width = 180;
            EditorGUILayout.LabelField ( text, GUILayout.Width(width) );

            text = "Viewport: " + new Vector2(sceneViewRect.width, sceneViewRect.height).ToString();
            // width = (int)EditorStyles.label.CalcSize(new GUIContent(text)).x;
            width = 180;
            EditorGUILayout.LabelField ( text, GUILayout.Width(width) );
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Layout_SceneViewField ( int _width, int _height ) {
        Rect rect = GUILayoutUtility.GetRect ( _width+4, _height+4, 
                                               new GUILayoutOption[] {
                                                   GUILayout.ExpandWidth(false),
                                                   GUILayout.ExpandHeight(false)
                                               });
        SceneViewField (rect);
    }
    void SceneViewField ( Rect _rect ) {
        int controlID = GUIUtility.GetControlID(sceneViewFieldHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.type ) {
        case EventType.Repaint:
            sceneViewRect = new Rect( _rect.x + 2, _rect.y + 2, _rect.width - 4, _rect.height - 4 );
            float half_w = sceneViewRect.width/2.0f;
            float half_h = sceneViewRect.height/2.0f;

            Color old = GUI.color;
            GUI.color = background;
                Texture2D checker = exEditorUtility.CheckerboardTexture();
                // background
                GUI.DrawTextureWithTexCoords ( sceneViewRect, checker, 
                                               new Rect( (-half_w + editCameraPos.x)/(checker.width * scale), 
                                                         (-half_h + editCameraPos.y)/(checker.height * scale), 
                                                         sceneViewRect.width/(checker.width * scale), 
                                                         sceneViewRect.height/(checker.height * scale) ) );
            GUI.color = old;

            // center line
            float center_x = -editCameraPos.x + sceneViewRect.x + half_w;
            float center_y =  editCameraPos.y + sceneViewRect.y + half_h;
            if ( center_y >= sceneViewRect.y && center_y <= sceneViewRect.yMax )
                exEditorUtility.DrawLine ( sceneViewRect.x, center_y, sceneViewRect.xMax, center_y, Color.white, 1 );
            if ( center_x >= sceneViewRect.x && center_x <= sceneViewRect.xMax )
                exEditorUtility.DrawLine ( center_x, sceneViewRect.y, center_x, sceneViewRect.yMax, Color.white, 1 );

            // draw scene
            DrawScene ( sceneViewRect );

            // border
            exEditorUtility.DrawRect( _rect,
                                      new Color( 1,1,1,0 ), 
                                      EditorStyles.label.normal.textColor );
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

        case EventType.DragUpdated:
            if ( _rect.Contains(e.mousePosition) ) {
                // Show a copy icon on the drag
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( o is exTextureInfo ) {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        break;
                    }
                }
                e.Use();
            }
            break;

        case EventType.DragPerform:
            if ( _rect.Contains(e.mousePosition) ) {
                DragAndDrop.AcceptDrag();

                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( o is exTextureInfo ) {
                    }
                }

                Repaint();
                e.Use();
            }
            break;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawScene ( Rect _rect ) {
        Rect oldViewport = new Rect( 0, 0, Screen.width, Screen.height ); 
        GL.PushMatrix();
            GL.LoadPixelMatrix( Mathf.FloorToInt((editCameraPos.x - _rect.width * 0.5f) / scale), 
                                Mathf.FloorToInt((editCameraPos.x - _rect.width * 0.5f + _rect.width) / scale), 
                                Mathf.FloorToInt((editCameraPos.y - _rect.height * 0.5f) / scale),
                                Mathf.FloorToInt((editCameraPos.y - _rect.height * 0.5f + _rect.height) / scale) );
            Rect viewportRect = new Rect ( _rect.x,
                                           position.height - _rect.yMax,
                                           _rect.width, 
                                           _rect.height );
            GL.Viewport(viewportRect);

            // TODO { 
            Material mat = (Material)EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
            mat.SetPass(0);
            GL.Begin(GL.QUADS);
            GL.Color( new Color( 1.0f, 0.0f, 0.0f, 0.5f ) );
                GL.Vertex3(200,   100,   0);
                GL.Vertex3(200,   300,   0);
                GL.Vertex3(300,   300,   0);
                GL.Vertex3(300,   100,   0);
            GL.End();
            // } TODO end 
        GL.PopMatrix();
        GL.Viewport(oldViewport);
    }
}
