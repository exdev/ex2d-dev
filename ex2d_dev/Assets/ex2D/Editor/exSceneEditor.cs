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
        public GUIStyle toolbarButton = "TE ToolbarButton";
        public GUIStyle toolbarDropDown = "TE ToolbarDropDown";
        public GUIStyle boxBackground = "TE NodeBackground";
        public GUIStyle elementBackground = "OL Box";
        public GUIStyle draggingHandle = "WindowBottomResize";
        public GUIStyle removeButton = "InvisibleButton";
        public GUIStyle elementSelectionRect = "SelectionRect";

        public Texture iconToolbarPlus = EditorGUIUtility.FindTexture ("Toolbar Plus");
        public Texture iconToolbarMinus = EditorGUIUtility.FindTexture("Toolbar Minus");

        public int elementHeight = 25;

        public SettingsStyles() {
            // NOTE: if we don't new GUIStyle, it will reference the original style. 
            boxBackground = new GUIStyle(boxBackground);
            boxBackground.margin = new RectOffset( 0, 0, 0, 0 );
            boxBackground.padding = new RectOffset( 0, 0, 0, 0 );

            elementBackground = new GUIStyle(elementBackground);
            elementBackground.overflow = new RectOffset(0, 0, 1, 0);

            elementSelectionRect = new GUIStyle(elementSelectionRect);
            elementSelectionRect.overflow = new RectOffset(0, 0, 1, -1);

            boldLabel = new GUIStyle(boldLabel);
            boldLabel.fontSize = 15;
            boldLabel.fontStyle = FontStyle.Bold;
            boldLabel.normal.textColor = EditorStyles.boldLabel.normal.textColor;
        }
    }

    static SettingsStyles settingsStyles = null;
	static int sceneViewFieldHash = "SceneViewField".GetHashCode();
	static int sceneEditorHash = "SceneEditor".GetHashCode();
	static int layerElementsFieldHash = "LayerElementsField".GetHashCode();

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
    SerializedObject curSerializedObject = null;

    exLayer activeLayer = null;
    exLayer draggingLayer = null;

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
        // pre-check
        PreCheck ();
        if ( ex2DMng.instance == null )
            return;

        curSerializedObject.Update ();

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
            int margin = 40; 
            float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
            Layout_SceneViewField ( Mathf.FloorToInt(position.width - 250 - 40 - 10 - margin),
                                    Mathf.FloorToInt(position.height - toolbarHeight - 40 - margin) );
        EditorGUILayout.EndHorizontal();

        // debug info
        DebugInfos ();

        //
        ProcessSceneEditorEvents();

        curSerializedObject.ApplyModifiedProperties ();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Reset () {
        curSerializedObject = null;
        activeLayer = null;
        draggingLayer = null;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void PreCheck () {
        // if settingsStyles is null
        if ( settingsStyles == null ) {
            settingsStyles = new SettingsStyles();
        }

        // if ex2DMng is null
        if ( ex2DMng.instance == null ) {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30);

                Color old = GUI.color;
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField ( "Can't find ex2DMng in the scene!" );
                GUI.color = old;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30);
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

        // if SerializedObject is null
        if ( ex2DMng.instance != null && curSerializedObject == null ) {
            curSerializedObject = new SerializedObject(ex2DMng.instance);
        }
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
                                           GUILayout.Width(250), 
                                           GUILayout.MinWidth(250), 
                                           GUILayout.MaxWidth(250),
                                           GUILayout.ExpandWidth(false),
                                       } );
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical( settingsStyles.boxBackground );

            // ======================================================== 
            // General 
            // ======================================================== 

            EditorGUILayout.LabelField ( "General", settingsStyles.boldLabel );
            EditorGUILayout.ObjectField( ""
                                         , ex2DMng.instance
                                         , typeof(ex2DMng)
                                         , false 
                                       );

            // ======================================================== 
            // Layers 
            // ======================================================== 

            EditorGUILayout.Space();
            EditorGUILayout.LabelField ( "Layers", settingsStyles.boldLabel );
            GUILayout.Space(2);

            Layout_LayerElementsField();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Layout_LayerElementsField () {
        SerializedProperty layerListProp = curSerializedObject.FindProperty ("layerList");

        Rect rect = GUILayoutUtility.GetRect ( 10f, settingsStyles.elementHeight * ex2DMng.instance.layerList.Count );
        LayerElementsField (rect, layerListProp);

        // add layer button
        EditorGUILayout.BeginHorizontal( settingsStyles.toolbar, new GUILayoutOption[0]);
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button( "UP", settingsStyles.toolbarButton ) ) 
            {
                int curIdx = ex2DMng.instance.layerList.IndexOf(activeLayer);
                if ( curIdx != -1 ) {
                    int nextIdx = System.Math.Max(curIdx-1,0);
                    layerListProp.MoveArrayElement ( curIdx, nextIdx );
                    activeLayer = ex2DMng.instance.layerList[nextIdx];
                }
            }
            if ( GUILayout.Button( "DOWN", settingsStyles.toolbarButton ) ) 
            {
                int curIdx = ex2DMng.instance.layerList.IndexOf(activeLayer);
                if ( curIdx != -1 ) {
                    int nextIdx = System.Math.Min(curIdx+1,ex2DMng.instance.layerList.Count-1);
                    layerListProp.MoveArrayElement ( curIdx, nextIdx );
                    activeLayer = ex2DMng.instance.layerList[nextIdx];
                }
            }
            if ( GUILayout.Button( settingsStyles.iconToolbarPlus, 
                                   settingsStyles.toolbarDropDown ) ) 
            {
                ex2DMng.instance.CreateLayer();
            }
        EditorGUILayout.EndHorizontal();
    }

    void LayerElementsField ( Rect _rect, SerializedProperty _layerListProp ) {
        int controlID = GUIUtility.GetControlID(layerElementsFieldHash, FocusType.Passive);
        float cx = _rect.x;
        float cy = _rect.y;
        Event e = Event.current;

        for ( int i = 0; i < _layerListProp.arraySize; ++i ) {
            SerializedProperty layerProp = _layerListProp.GetArrayElementAtIndex(i);
            LayerElementField ( new Rect( cx, cy, _rect.width, settingsStyles.elementHeight ), 
                                layerProp,
                                i,
                                controlID ); 
            cy += settingsStyles.elementHeight;
        }

        // event process for layers
        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.MouseUp:
			if ( GUIUtility.hotControl == controlID ) {
				GUIUtility.hotControl = 0;
                draggingLayer = null;
                e.Use();
			}
            break;
        }
    }

    void LayerElementField ( Rect _rect, SerializedProperty _prop, int _idx, int _controlID ) {
        Vector2 size = Vector2.zero;
        float cur_x = _rect.x;
        Event e = Event.current;
        exLayer layer = ex2DMng.instance.layerList[_idx];

        cur_x += 5.0f;
        Rect draggingHandleRect = new Rect(cur_x, _rect.y + 10f, 10f, _rect.height);
        if ( Event.current.type == EventType.Repaint ) {
            // draw background
            if ( activeLayer == layer ) {
                settingsStyles.elementSelectionRect.Draw(_rect, false, false, false, false);
            }
            else {
                settingsStyles.elementBackground.Draw(_rect, false, false, false, false);
            }

            settingsStyles.draggingHandle.Draw( draggingHandleRect, false, false, false, false );
            EditorGUIUtility.AddCursorRect ( draggingHandleRect, MouseCursor.Pan );
        }
        cur_x += 10.0f;

        cur_x += 5.0f;
        size = EditorStyles.toggle.CalcSize( GUIContent.none );
        EditorGUI.PropertyField ( new Rect ( cur_x, _rect.y + 3f, size.x, size.y ),
                                  _prop.FindPropertyRelative("show"), 
                                  GUIContent.none );
        cur_x += 10.0f;

        cur_x += 10.0f;
        EditorGUI.PropertyField ( new Rect ( cur_x, _rect.y + 4f, 100f, _rect.height - 8f ),
                                  _prop.FindPropertyRelative("name"), 
                                  GUIContent.none );
        cur_x += 100.0f;


        //
        size = settingsStyles.removeButton.CalcSize( new GUIContent(settingsStyles.iconToolbarMinus) );
        cur_x = _rect.xMax - 5.0f - size.x;
        if ( GUI.Button( new Rect( cur_x, _rect.y + 2f, size.x, size.y ), 
                         settingsStyles.iconToolbarMinus, 
                         settingsStyles.removeButton) )
        {
            string layerName = _prop.FindPropertyRelative("name").stringValue;
            if ( EditorUtility.DisplayDialog ( "Delete Layer?", 
                                               string.Format("Are you sure you want to delete layer: {0}?", layerName),
                                               "Yes",
                                               "No" ) )
            {
                ex2DMng.instance.DestroyLayer(_idx);
                _prop.DeleteCommand();
                if ( activeLayer == layer )
                    activeLayer = null;
            }
        }

        // event process for layer
        switch ( e.GetTypeForControl(_controlID) ) {
        case EventType.MouseDown:
            if ( e.button == 0 && e.clickCount == 1 && _rect.Contains(e.mousePosition) ) {
                GUIUtility.hotControl = _controlID;
                GUIUtility.keyboardControl = _controlID;
                activeLayer = layer;

                if ( draggingHandleRect.Contains(e.mousePosition) ) {
                    draggingLayer = layer;
                }

                e.Use();
            }
            break;
        }
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
            if ( ex2DMng.instance.layerList.Count > 0 &&  _rect.Contains(e.mousePosition) ) {
                if ( activeLayer == null ) {
                    activeLayer = ex2DMng.instance.layerList[0];
                }

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
                        GameObject gameObject = new GameObject("New Sprite");
                        exSprite sprite = gameObject.AddComponent<exSprite>();
                        if ( sprite.shader == null )
                            sprite.shader = Shader.Find("ex2D/Alpha Blended");
                        sprite.textureInfo = o as exTextureInfo;
                        gameObject.transform.position = Vector3.zero;
                        gameObject.transform.localScale = Vector3.one;
                        gameObject.transform.rotation = Quaternion.identity;

                        if ( activeLayer != null )
                            activeLayer.Add(sprite);
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
        Rect viewportRect = new Rect ( _rect.x,
                                       position.height - _rect.yMax,
                                       _rect.width, 
                                       _rect.height );
        GL.Viewport(viewportRect);

        GL.PushMatrix();
            // GL.LoadPixelMatrix( Mathf.FloorToInt((editCameraPos.x - _rect.width * 0.5f) / scale), 
            //                     Mathf.FloorToInt((editCameraPos.x + _rect.width * 0.5f) / scale), 
            //                     Mathf.FloorToInt((editCameraPos.y - _rect.height * 0.5f) / scale),
            //                     Mathf.FloorToInt((editCameraPos.y + _rect.height * 0.5f) / scale) );

            // TODO { 
            // GL.LoadPixelMatrix( 0.0f, _rect.width, 0.0f, _rect.height );
            // GL.LoadOrtho();
            GL.LoadProjectionMatrix( Matrix4x4.Ortho( -_rect.width * 0.5f, 
                                                       _rect.width * 0.5f, 
                                                      -_rect.height * 0.5f, 
                                                       _rect.height * 0.5f, 
                                                      -1.0f, 
                                                       1.0f ) );
            GL.LoadIdentity();
            GL.MultMatrix( Matrix4x4.TRS( new Vector3(editCameraPos.x, editCameraPos.y, 0.0f),
                                          Quaternion.identity,
                                          new Vector3(1.0f, 1.0f, 1.0f) ).inverse );
            // } TODO end 

            // draw all nodes in the scene
            // foreach ( exLayer layer in ex2DMng.instance.layerList ) {
            //     if ( layer.show ) {
            //         foreach ( exSpriteBase node in layer ) {
            //             DrawNode ( node );
            //         }
            //     }
            // }

            GL.PushMatrix();
            GL.Begin(GL.QUADS);
            GL.Color( new Color( 1.0f, 1.0f, 1.0f, 0.5f ) );
                GL.Vertex3( -10.0f, -10.0f, 0.0f );
                GL.Vertex3( -10.0f,  20.0f, 0.0f );
                GL.Vertex3(  30.0f,  20.0f, 0.0f );
                GL.Vertex3(  30.0f, -10.0f, 0.0f );
            GL.End();
            GL.PopMatrix();

        GL.PopMatrix();
        GL.Viewport(oldViewport);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawNode ( exSpriteBase _node ) {
        Material mat = _node.material;
        mat.SetPass(0);

        Vector3 pos = _node.transform.position;
        Vector2 size = Vector2.one;

        GL.Begin(GL.QUADS);
        GL.Color( new Color( 1.0f, 1.0f, 1.0f, 0.5f ) );
            GL.Vertex3( -size.x + pos.x, -size.y + pos.y, 0.0f );
            GL.Vertex3( -size.x + pos.x,  size.y + pos.y, 0.0f );
            GL.Vertex3(  size.x + pos.x,  size.y + pos.y, 0.0f );
            GL.Vertex3(  size.x + pos.x, -size.y + pos.y, 0.0f );
        GL.End();
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

    void ProcessSceneEditorEvents () {
        int controlID = GUIUtility.GetControlID(sceneEditorHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.MouseDown:
            if ( e.button == 0 && e.clickCount == 1 ) {
                GUIUtility.hotControl = controlID;
                GUIUtility.keyboardControl = controlID;

                e.Use();
            }
            break;

        case EventType.MouseUp:
			if ( GUIUtility.hotControl == controlID ) {
				GUIUtility.hotControl = 0;

                e.Use();
			}
            break;
        }
    }
}
