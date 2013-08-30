// ======================================================================================
// File         : exUILayoutEditor.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 16:36:31 PM | Friday,August
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
/// The Texture Info Editor
///
///////////////////////////////////////////////////////////////////////////////

class exUILayoutEditor : EditorWindow {

	static int sceneViewFieldHash = "SceneViewField".GetHashCode();

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exUILayoutInfo curEdit = null;

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

    Camera editCamera;
    Color background = Color.gray;
    Rect sceneViewRect = new Rect( 0, 0, 1, 1 );

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        title = "UI-Layout Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = false;
        minSize = new Vector2(500f, 500f);

        if (editCamera == null) {
            GameObject camGO 
            = EditorUtility.CreateGameObjectWithHideFlags ( "UILayoutView_Camera", 
                                                            HideFlags.HideAndDontSave, 
                                                            new System.Type[] {
                                                                typeof(Camera)
                                                            } );
            editCamera = camGO.camera;
            editCamera.enabled = false;
            editCamera.clearFlags = CameraClearFlags.Depth|CameraClearFlags.SolidColor;
            editCamera.farClipPlane = 10000.0f;
            editCamera.nearClipPlane = -100.0f;
            editCamera.backgroundColor = Color.black;
            editCamera.renderingPath = RenderingPath.Forward;
            editCamera.orthographic = true;
            editCamera.orthographicSize = 100.0f;
            editCamera.transform.position = new Vector3 ( sceneViewRect.width * 0.5f - 20.0f,
                                                          sceneViewRect.height * 0.5f - 20.0f,
                                                          0.0f );
        }

        // rectSelection = new exRectSelection<Object>( PickObject,
        //                                              PickRectObjects,
        //                                              ConfirmRectSelection );

        UpdateEditObject ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    void OnDisable () {
        if (editCamera != null) {
            editCamera.Destroy();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnFocus () {
        UpdateEditObject ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnSelectionChange () {
        UpdateEditObject ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnInspectorUpdate () {
        if ( curEdit == null )
            return;

        Repaint();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {

        if ( curEdit == null ) {
            EditorGUILayout.Space();
            GUILayout.Label ( "Please select a TextureInfo" );
            return;
        }

        // toolbar
        Toolbar ();

        int margin = 20;
        GUILayout.Space(margin);

        // settings & scene
        EditorGUILayout.BeginHorizontal();
            //
            Settings ();

            //
            GUILayout.Space(40);

            // scene filed
            float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
            Layout_SceneViewField ( Mathf.FloorToInt(position.width - 250 - 40 - margin),
                                    Mathf.FloorToInt(position.height - toolbarHeight - margin - margin ) );
        EditorGUILayout.EndHorizontal();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateEditObject () {
        exUILayoutInfo info = Selection.activeObject as exUILayoutInfo;
        if ( info != null && info != curEdit ) {
            Edit (info);
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Reset () {
    }

    // ------------------------------------------------------------------ 
    /// \param _obj
    /// Check if the object is valid atlas and open it in atlas editor.
    // ------------------------------------------------------------------ 

    public void Edit ( exUILayoutInfo _info ) {
        if ( _info == null )
            return;

        curEdit = _info;

        Reset ();
        Repaint ();
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
                editCamera.transform.position = new Vector3 ( sceneViewRect.width * 0.5f - 20.0f,
                                                              sceneViewRect.height * 0.5f - 20.0f,
                                                              0.0f );
            }

            // ======================================================== 
            // zoom in/out button & slider 
            // ======================================================== 

            // button 
            if ( GUILayout.Button( "Zoom", EditorStyles.toolbarButton ) ) {
                scale = 1.0f;
            }

            EditorGUILayout.Space();

            // ======================================================== 
            // Help
            // ======================================================== 

            if ( GUILayout.Button( exEditorUtility.textureHelp, EditorStyles.toolbarButton ) ) {
                Help.BrowseURL("http://ex-dev.com/ex2d/docs/");
            }

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Settings () {

        EditorGUILayout.BeginHorizontal( new GUILayoutOption [] {
                                           GUILayout.Width(250), 
                                           GUILayout.MinWidth(250), 
                                           GUILayout.MaxWidth(250),
                                           GUILayout.ExpandWidth(false),
                                       } );

        GUILayout.Space(10);

        EditorGUILayout.BeginVertical();

        GUILayout.Label ( "Coming Soon..." );

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Layout_SceneViewField ( int _width, int _height ) {
        Rect rect = GUILayoutUtility.GetRect ( _width, _height, 
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

            sceneViewRect = _rect;
            editCamera.aspect = _rect.width/_rect.height;
            editCamera.orthographicSize = (_rect.height * 0.5f) / scale;

            // draw scene view
            DrawSceneView (_rect);

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
                editCamera.transform.position += new Vector3 ( -e.delta.x / scale, -e.delta.y / scale, 0.0f );

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

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawSceneView ( Rect _rect ) {
        Rect oldViewport = new Rect( 0, 0, Screen.width, Screen.height ); 
        Rect viewportRect = new Rect ( _rect.x,
                                       position.height - _rect.yMax,
                                       _rect.width, 
                                       _rect.height );
        GL.PushMatrix();

            //
            GL.Viewport(viewportRect);
            GL.LoadPixelMatrix ( 0.0f, _rect.width, _rect.height, 0.0f );

            // background
            float half_w = _rect.width/2.0f;
            float half_h = _rect.height/2.0f;
            Texture2D checker = exEditorUtility.textureCheckerboard;
            Vector2 center = new Vector2( half_w, half_h );
            Vector2 size = new Vector2 ( _rect.width, _rect.height ); 
            exEditorUtility.GL_DrawTexture ( center, 
                                             size, 
                                             checker, 
                                             new Rect( (-half_w/scale + editCamera.transform.position.x)/checker.width,
                                                       (-half_h/scale + editCamera.transform.position.y)/checker.height,
                                                       _rect.width/(checker.width * scale), 
                                                       _rect.height/(checker.height * scale) ),
                                             background );


            // center line
            float center_x = half_w - editCamera.transform.position.x * scale;
            float center_y = half_h - editCamera.transform.position.y * scale;
            exEditorUtility.GL_DrawLine ( 0.0f,
                                          center_y, 
                                          _rect.width,
                                          center_y, 
                                          new Color( 0.6f, 0.6f, 0.6f ) );
            exEditorUtility.GL_DrawLine ( center_x, 
                                          0.0f,
                                          center_x, 
                                          _rect.height,
                                          new Color( 0.6f, 0.6f, 0.6f ) );

            //
            GL.LoadPixelMatrix( editCamera.transform.position.x - (_rect.width  * 0.5f) / scale, 
                                editCamera.transform.position.x + (_rect.width  * 0.5f) / scale, 
                                editCamera.transform.position.y - (_rect.height * 0.5f) / scale,
                                editCamera.transform.position.y + (_rect.height * 0.5f) / scale );


            // draw layout
            // TODO:

        GL.PopMatrix();

        GL.Viewport(oldViewport);
    }
}
