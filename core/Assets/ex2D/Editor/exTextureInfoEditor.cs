// ======================================================================================
// File         : exTextureInfoEditor.cs
// Author       : Wu Jie 
// Last Change  : 08/21/2013 | 14:27:34 PM | Wednesday,August
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

class exTextureInfoEditor : EditorWindow {

	static int sceneViewFieldHash = "SceneViewField".GetHashCode();

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exTextureInfo curEdit = null;

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

    int editModeIndex = 0;

    bool showTrimRect = true;
    Color trimRectColor = new Color ( 0.8f, 0.8f, 0.0f );

    bool showRawRect = false;
    Color rawRectColor = new Color ( 0.8f, 0.0f, 0.0f );

    bool showPixelGrid = false;
    Color pixelGridColor = new Color ( 1.0f, 1.0f, 1.0f, 0.3f );

    // sliced
    Color slicedColor = new Color ( 0.0f, 0.8f, 0.0f );

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        title = "TextureInfo Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = false;
        minSize = new Vector2(500f, 500f);

        if (editCamera == null) {
            GameObject camGO 
            = EditorUtility.CreateGameObjectWithHideFlags ( "SceneViewCamera", 
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

        GUILayout.Space(20);

        // settings & scene
        EditorGUILayout.BeginHorizontal();
            //
            Settings ();

            //
            GUILayout.Space(40);

            // scene filed
            int margin = 20; 
            float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
            Layout_SceneViewField ( Mathf.FloorToInt(position.width - 250 - 40 - 10 - margin),
                                    Mathf.FloorToInt(position.height - toolbarHeight - 40 - margin) );
        EditorGUILayout.EndHorizontal();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateEditObject () {
        exTextureInfo info = Selection.activeObject as exTextureInfo;
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

    public void Edit ( exTextureInfo _info ) {
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

            string[] toolbarStrings = new string[] {"Attach Points", "Collision", "Sliced", "Diced" };
            editModeIndex = GUILayout.Toolbar( editModeIndex, toolbarStrings, EditorStyles.toolbarButton );

            GUILayout.FlexibleSpace();

            // ======================================================== 
            // Reset 
            // ======================================================== 

            if ( GUILayout.Button( "Reset", EditorStyles.toolbarButton ) ) {
                editCamera.transform.position = Vector3.zero;
            }

            // ======================================================== 
            // zoom in/out button & slider 
            // ======================================================== 

            // button 
            if ( GUILayout.Button( "Zoom", EditorStyles.toolbarButton ) ) {
                scale = 1.0f;
            }

            EditorGUILayout.Space();

            // slider
            scale = GUILayout.HorizontalSlider ( scale, 
                                                 0.1f, 
                                                 10.0f, 
                                                 new GUILayoutOption[] {
                                                 GUILayout.MinWidth(50),
                                                 GUILayout.MaxWidth(150)
                                                 } );
            EditorGUILayout.Space();
            scale = EditorGUILayout.FloatField( scale,
                                                EditorStyles.toolbarTextField,
                                                new GUILayoutOption[] {
                                                GUILayout.Width(30)
                                                } );

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

        showTrimRect = EditorGUILayout.Toggle ( "Show Trimed Rect", showTrimRect );
        trimRectColor = EditorGUILayout.ColorField ( "Trimed Rect Color", trimRectColor );

        showRawRect = EditorGUILayout.Toggle ( "Show Raw Rect", showRawRect );
        rawRectColor = EditorGUILayout.ColorField ( "Raw Rect Color", rawRectColor );
        
        showPixelGrid = EditorGUILayout.Toggle ( "Show Pixel Grid", showPixelGrid );
        pixelGridColor = EditorGUILayout.ColorField ( "Pixel Grid Color", pixelGridColor );

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        switch ( editModeIndex ) {
            // attach pionts
        case 0:
            if ( GUILayout.Button("Add...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                // TODO:
            }

            break;

            // collision
        case 1:
            GUILayout.Label ( "Coming Soon..." );
            break;

            // sliced
        case 2:
            slicedColor = EditorGUILayout.ColorField ( "Color", slicedColor );
            EditorGUI.BeginChangeCheck();
                int result = EditorGUILayout.IntField ( "Left", curEdit.borderLeft );
                curEdit.borderLeft = System.Math.Min ( System.Math.Max ( 0, result ), curEdit.width - curEdit.borderRight );

                result = EditorGUILayout.IntField ( "Right", curEdit.borderRight );
                curEdit.borderRight = System.Math.Min ( System.Math.Max ( 0, result ), curEdit.width - curEdit.borderLeft );

                result = EditorGUILayout.IntField ( "Top", curEdit.borderTop );
                curEdit.borderTop = System.Math.Min ( System.Math.Max ( 0, result ), curEdit.height - curEdit.borderBottom );

                result = EditorGUILayout.IntField ( "Bottom", curEdit.borderBottom );
                curEdit.borderBottom = System.Math.Min ( System.Math.Max ( 0, result ), curEdit.height - curEdit.borderTop );

            if ( EditorGUI.EndChangeCheck() ) {
                EditorUtility.SetDirty(curEdit);
            }
            break;

            // diced
        case 3:
            GUILayout.Label ( "Coming Soon..." );
            break;
        }

        EditorGUILayout.EndVertical();

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
                editCamera.transform.position += new Vector3 ( -e.delta.x / scale, e.delta.y / scale, 0.0f );

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
        GL.Viewport(viewportRect);

        GL.PushMatrix();

            //
            GL.LoadPixelMatrix ( 0.0f, _rect.width, 0.0f, _rect.height );

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

            // draw texture info
            exEditorUtility.GL_DrawTextureInfo ( curEdit, Vector2.zero, Color.white );
            
            float half_width = curEdit.width * 0.5f;
            float half_height = curEdit.height * 0.5f;
            int trim_left  = curEdit.trim_x;
            int trim_right = curEdit.rawWidth - curEdit.trim_x - curEdit.width;
            int trim_top   = curEdit.rawHeight - curEdit.trim_y - curEdit.height;
            int trim_bot   = curEdit.trim_y;

            // draw pixel grid
            if ( showPixelGrid ) {
                Vector2[] points = new Vector2[(curEdit.width + curEdit.height) * 2];
                int idx = 0;
                for ( int x = 0; x < curEdit.width; ++x ) {
                    points[idx]   = new Vector2( -half_width + x, -half_height );
                    points[idx+1] = new Vector2( -half_width + x,  half_height );
                    idx += 2;
                }
                for ( int y = 0; y < curEdit.height; ++y ) {
                    points[idx]   = new Vector2( -half_width, -half_height + y );
                    points[idx+1] = new Vector2(  half_width, -half_height + y );
                    idx += 2;
                }

                exEditorUtility.GL_DrawLines ( points, pixelGridColor );
            }

            // draw raw-texture bounding 
            if ( showRawRect ) {
                exEditorUtility.GL_DrawRectLine ( new Vector3[] {
                                                  new Vector3 ( -half_width - trim_left,  -half_height - trim_bot, 0.0f ),
                                                  new Vector3 ( -half_width - trim_left,   half_height + trim_top, 0.0f ),
                                                  new Vector3 (  half_width + trim_right,  half_height + trim_top, 0.0f ),
                                                  new Vector3 (  half_width + trim_right, -half_height - trim_bot, 0.0f ),
                                                  }, 
                                                  rawRectColor );
            }

            // draw trimed bounding 
            if ( showTrimRect ) {
                exEditorUtility.GL_DrawRectLine ( new Vector3[] {
                                                  new Vector3 ( -half_width, -half_height, 0.0f ),
                                                  new Vector3 ( -half_width,  half_height, 0.0f ),
                                                  new Vector3 (  half_width,  half_height, 0.0f ),
                                                  new Vector3 (  half_width, -half_height, 0.0f ),
                                                  }, 
                                                  trimRectColor );
            }

            // draw sliced line
            if ( editModeIndex == 2 ) {
                float left   = -half_width  + curEdit.borderLeft;
                float right  =  half_width  - curEdit.borderRight;
                float top    =  half_height - curEdit.borderTop;
                float bottom = -half_height + curEdit.borderBottom;

                exEditorUtility.GL_DrawLine ( left, -half_height, left, half_height, slicedColor );
                exEditorUtility.GL_DrawLine ( right, -half_height, right, half_height, slicedColor );
                exEditorUtility.GL_DrawLine ( -half_width, top, half_width, top, slicedColor );
                exEditorUtility.GL_DrawLine ( -half_width, bottom, half_width, bottom, slicedColor );
            }

            // draw border line
            GL.LoadPixelMatrix ( 0.0f, _rect.width, _rect.height, 0.0f );
            exEditorUtility.GL_DrawRectLine ( new Vector3[] {
                                              new Vector3 ( 1.0f,        1.0f,         0.0f ),
                                              new Vector3 ( _rect.width, 1.0f,         0.0f ),
                                              new Vector3 ( _rect.width, _rect.height, 0.0f ),
                                              new Vector3 ( 1.0f,        _rect.height, 0.0f ),
                                              },
                                              new Color( 0.7f, 0.7f, 0.7f ) );

        GL.PopMatrix();
        GL.Viewport(oldViewport);
    }
}
