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

    // 
    Vector2 mouseDownPos = Vector2.zero;
    Rect selectRect = new Rect( 0, 0, 1, 1 );
    bool inRectSelectState = false;
    List<exSpriteBase> spriteNodes = new List<exSpriteBase>();

    //
    Material matLine = null;

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

    void OnInspectorUpdate () {
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

        // SerializedObject
        if ( ex2DMng.instance != null )
            curSerializedObject = new SerializedObject(ex2DMng.instance);
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
                    // activeLayer = ex2DMng.instance.layerList[nextIdx];
                }
            }
            if ( GUILayout.Button( "DOWN", settingsStyles.toolbarButton ) ) 
            {
                int curIdx = ex2DMng.instance.layerList.IndexOf(activeLayer);
                if ( curIdx != -1 ) {
                    int nextIdx = System.Math.Min(curIdx+1,ex2DMng.instance.layerList.Count-1);
                    layerListProp.MoveArrayElement ( curIdx, nextIdx );
                    // activeLayer = ex2DMng.instance.layerList[nextIdx];
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
                                layerProp.objectReferenceValue as exLayer,
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

    void LayerElementField ( Rect _rect, exLayer _layer, int _controlID ) {
        Vector2 size = Vector2.zero;
        float cur_x = _rect.x;
        Event e = Event.current;

        if ( _layer == null )
            return;

        cur_x += 5.0f;
        Rect draggingHandleRect = new Rect(cur_x, _rect.y + 10f, 10f, _rect.height);
        if ( Event.current.type == EventType.Repaint ) {
            // draw background
            if ( activeLayer == _layer ) {
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
        bool newShow = EditorGUI.Toggle ( new Rect ( cur_x, _rect.y + 3f, size.x, size.y ),
                                          _layer.show );
        if ( newShow != _layer.show ) {
            _layer.show = newShow;
            EditorUtility.SetDirty(_layer);
        }
        cur_x += 10.0f;

        cur_x += 10.0f;
        string newName = EditorGUI.TextField ( new Rect ( cur_x, _rect.y + 4f, 100f, _rect.height - 8f ),
                                               _layer.gameObject.name ); 
        if ( newName != _layer.gameObject.name ) {
            _layer.gameObject.name = newName;
            EditorUtility.SetDirty(_layer.gameObject);
        }
        cur_x += 100.0f;


        //
        size = settingsStyles.removeButton.CalcSize( new GUIContent(settingsStyles.iconToolbarMinus) );
        cur_x = _rect.xMax - 5.0f - size.x;
        if ( GUI.Button( new Rect( cur_x, _rect.y + 2f, size.x, size.y ), 
                         settingsStyles.iconToolbarMinus, 
                         settingsStyles.removeButton) )
        {
            if ( EditorUtility.DisplayDialog ( "Delete Layer?", 
                                               string.Format("Are you sure you want to delete _layer: {0}?", _layer.gameObject.name),
                                               "Yes",
                                               "No" ) )
            {
                ex2DMng.instance.DestroyLayer(_layer);
            }
        }

        // event process for _layer
        switch ( e.GetTypeForControl(_controlID) ) {
        case EventType.MouseDown:
            if ( e.button == 0 && e.clickCount == 1 && _rect.Contains(e.mousePosition) ) {
                GUIUtility.hotControl = _controlID;
                GUIUtility.keyboardControl = _controlID;
                activeLayer = _layer;

                if ( draggingHandleRect.Contains(e.mousePosition) ) {
                    draggingLayer = _layer;
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
            DoCulling (sceneViewRect);
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
                        GameObject gameObject = new GameObject(o.name);
                        exSprite sprite = gameObject.AddComponent<exSprite>();
                        if ( sprite.shader == null )
                            sprite.shader = Shader.Find("ex2D/Alpha Blended");
                        sprite.textureInfo = o as exTextureInfo;
                        gameObject.transform.position = SceneField_MapToWorld( _rect, e.mousePosition);
                        gameObject.transform.localScale = Vector3.one;
                        gameObject.transform.rotation = Quaternion.identity;

                        if ( activeLayer != null ) {
                            activeLayer.Add(sprite);
                            EditorUtility.SetDirty(activeLayer);
                            EditorUtility.SetDirty(sprite);
                        }
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
            GL.LoadPixelMatrix( (editCameraPos.x - _rect.width  * 0.5f) / scale, 
                                (editCameraPos.x + _rect.width  * 0.5f) / scale, 
                                (editCameraPos.y - _rect.height * 0.5f) / scale,
                                (editCameraPos.y + _rect.height * 0.5f) / scale );

            // DELME { 
            // // draw all nodes in the scene
            // for ( int i = ex2DMng.instance.layerList.Count-1; i >= 0; --i ) {
            //     exLayer layer = ex2DMng.instance.layerList[i];
            //     if ( layer != null && layer.show ) {
            //         exSpriteBase[] spriteList = layer.GetComponentsInChildren<exSpriteBase>();
            //         // TODO: sort the spriteList
            //         foreach ( exSpriteBase node in spriteList ) {
            //             DrawNode ( node );
            //         }
            //     }
            // }
            // } DELME end 

            // draw culled sprite nodes
            for ( int i = 0; i < spriteNodes.Count; ++i ) {
                DrawNode ( spriteNodes[i] );
            }

            // draw selected objects
            if ( matLine == null ) {
                // matLine = new Material( Shader.Find("ex2D/Alpha Blended") );
                matLine = new Material( "Shader \"Lines/Colored Blended\" {" +
                                        "SubShader { Pass { " +
                                        "    Blend SrcAlpha OneMinusSrcAlpha " +
                                        "    ZWrite Off Cull Off Fog { Mode Off } " +
                                        "    BindChannels {" +
                                        "      Bind \"vertex\", vertex Bind \"color\", color }" +
                                        "} } }" );
                matLine.hideFlags = HideFlags.HideAndDontSave;
                matLine.shader.hideFlags = HideFlags.HideAndDontSave;
            }
            matLine.SetPass(0);
            for ( int i = 0; i < Selection.transforms.Length; ++i ) {
                Transform trans = Selection.transforms[i];
                exSpriteBase spriteBase = trans.GetComponent<exSpriteBase>();
                if ( spriteBase ) {
                    Rect boundingRect = spriteBase.GetBoundingRect();
                    GL.Begin( GL.LINES );
                    GL.Color( Color.white );
                    GL.Vertex3( boundingRect.xMin, boundingRect.yMin, 0.0f );
                    GL.Vertex3( boundingRect.xMin, boundingRect.yMax, 0.0f );

                    GL.Vertex3( boundingRect.xMin, boundingRect.yMax, 0.0f );
                    GL.Vertex3( boundingRect.xMax, boundingRect.yMax, 0.0f );

                    GL.Vertex3( boundingRect.xMax, boundingRect.yMax, 0.0f );
                    GL.Vertex3( boundingRect.xMax, boundingRect.yMin, 0.0f );

                    GL.Vertex3( boundingRect.xMax, boundingRect.yMin, 0.0f );
                    GL.Vertex3( boundingRect.xMin, boundingRect.yMin, 0.0f );

                    GL.End();
                }
            }

        GL.PopMatrix();
        GL.Viewport(oldViewport);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawNode ( exSpriteBase _node ) {
        Material material = _node.material;
        material.SetPass(0);

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        _node.GetBuffers(vertices, uvs);
        exDebug.Assert(uvs.Count == vertices.Count);

        //GL.PushMatrix();
        //GL.MultMatrix( _node.transform.localToWorldMatrix );
        GL.Begin(GL.QUADS);
            GL.Color( new Color( 1.0f, 1.0f, 1.0f, 1.0f ) );
            for (int i = 0; i < vertices.Count; ++i) {
                GL.TexCoord2 ( uvs[i].x, uvs[i].y );
                GL.Vertex ( vertices[i] );
            }
        GL.End();
        //GL.PopMatrix();
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

            text = "Sprites: " + spriteNodes.Count;
            width = 100;
            EditorGUILayout.LabelField ( text, GUILayout.Width(width) );

            EditorGUILayout.LabelField ( "selection count = " + Selection.objects.Length );
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessSceneEditorEvents () {
        int controlID = GUIUtility.GetControlID(sceneEditorHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.Repaint:
            // draw select rect 
            if ( inRectSelectState && (selectRect.width != 0.0f || selectRect.height != 0.0f) ) {
                exEditorUtility.DrawRect( selectRect, new Color( 0.0f, 0.5f, 1.0f, 0.2f ), new Color( 0.0f, 0.5f, 1.0f, 1.0f ) );
            }
            break;

        case EventType.MouseDown:
            if ( e.button == 0 && e.clickCount == 1 ) {
                GUIUtility.hotControl = controlID;
                GUIUtility.keyboardControl = controlID;

                bool toggle = (e.command||e.control);
                inRectSelectState = (toggle==false);

                mouseDownPos = e.mousePosition;
                UpdateSelectRect ();
                ConfirmRectSelection(toggle);
                Repaint();

                e.Use();
            }
            break;

        case EventType.MouseDrag:
            if ( GUIUtility.hotControl == controlID && inRectSelectState ) {
                UpdateSelectRect ();
                ConfirmRectSelection();
                Repaint();

                e.Use();
            }
            break;

        case EventType.MouseUp:
			if ( GUIUtility.hotControl == controlID ) {
				GUIUtility.hotControl = 0;

                if ( inRectSelectState && e.button == 0 ) {
                    inRectSelectState = false;
                    ConfirmRectSelection();
                    Repaint();
                }

                e.Use();
			}
            break;

        case EventType.KeyDown:
            if ( (e.command || e.control) &&
                 (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete) ) 
            {
                for ( int i = Selection.transforms.Length-1; i >= 0; --i ) {
                    Transform trans = Selection.transforms[i];
                    if ( trans.gameObject != null )
                        Object.DestroyImmediate(trans.gameObject);
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

    void UpdateSelectRect () {
        float x = 0;
        float y = 0;
        float width = 0;
        float height = 0;
        Vector2 curMousePos = Event.current.mousePosition;

        if ( mouseDownPos.x < curMousePos.x ) {
            x = mouseDownPos.x;
            width = curMousePos.x - mouseDownPos.x;
        }
        else {
            x = curMousePos.x;
            width = mouseDownPos.x - curMousePos.x;
        }
        if ( mouseDownPos.y < curMousePos.y ) {
            y = mouseDownPos.y;
            height = curMousePos.y - mouseDownPos.y;
        }
        else {
            y = curMousePos.y;
            height = mouseDownPos.y - curMousePos.y;
        }

        selectRect = new Rect( x, y, width, height );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ConfirmRectSelection ( bool _toggle = false ) {
        List<GameObject> selectGOs = new List<GameObject>();
        foreach ( exSpriteBase node in spriteNodes ) {
            Rect boundingRect = MapBoundingRect ( sceneViewRect, node );
            if ( exGeometryUtility.RectRect_Contains( selectRect, boundingRect ) != 0 ||
                 exGeometryUtility.RectRect_Intersect( selectRect, boundingRect ) )
            {
                selectGOs.Add(node.gameObject);
            }
        }

        if ( _toggle ) {
            Transform[] oldSelection = Selection.transforms;
            foreach ( Transform trans in oldSelection ) {
                int idx = selectGOs.IndexOf (trans.gameObject);
                if ( idx  == -1 ) {
                    selectGOs.Add(trans.gameObject);
                }
                else {
                    selectGOs.RemoveAt(idx);
                }
            }
        }

        Selection.objects = selectGOs.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Vector3 SceneField_MapToWorld ( Rect _rect, Vector2 _mousePosition ) {
        return new Vector3 (  (_mousePosition.x - _rect.x - _rect.width/2.0f + editCameraPos.x)/scale,
                             -(_mousePosition.y - _rect.y - _rect.height/2.0f - editCameraPos.y)/scale,
                             0.0f );
    }
    Vector3 SceneField_MapToScreen ( Rect _rect, Vector2 _mousePosition ) {
        return new Vector3 ( _mousePosition.x - _rect.x,
                             _mousePosition.y - _rect.y,
                             0.0f );
    }
    Vector2 SceneField_WorldToScreen ( Rect _rect, Vector3 _worldPos ) {
        return new Vector2 ( _worldPos.x * scale + _rect.x + _rect.width/2.0f - editCameraPos.x,
                            -_worldPos.y * scale + _rect.y + _rect.height/2.0f + editCameraPos.y );
    }

    Rect MapBoundingRect ( Rect _rect, exSpriteBase _node ) {
        exSprite sprite = _node as exSprite;
        Vector2 screenPos = SceneField_WorldToScreen ( _rect, sprite.transform.position );

        if ( sprite ) {
            // Rect boundingRect = new Rect ( screenPos.x - sprite.textureInfo.rotatedWidth/2.0f * scale,
            //                                screenPos.y - sprite.textureInfo.rotatedHeight/2.0f * scale,
            //                                sprite.textureInfo.rotatedWidth * scale,
            //                                sprite.textureInfo.rotatedHeight * scale );
            Rect boundingRect = sprite.GetBoundingRect();
            boundingRect = new Rect ( screenPos.x - boundingRect.width/2.0f * scale,
                                      screenPos.y - boundingRect.height/2.0f * scale,
                                      boundingRect.width * scale,
                                      boundingRect.height * scale );
            boundingRect = exGeometryUtility.Rect_FloorToInt(boundingRect);

            return boundingRect;
        }

        return new Rect (  screenPos.x * scale,
                           screenPos.y * scale,
                           1.0f * scale,
                           1.0f * scale );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DoCulling ( Rect _rect ) {
        spriteNodes.Clear();

        // draw all nodes in the scene
        for ( int i = ex2DMng.instance.layerList.Count-1; i >= 0; --i ) {
            exLayer layer = ex2DMng.instance.layerList[i];
            if ( layer != null && layer.show ) {
                exSpriteBase[] spriteList = layer.GetComponentsInChildren<exSpriteBase>();
                foreach ( exSpriteBase node in spriteList ) {
                    Rect boundingRect = MapBoundingRect ( _rect, node );
                    if ( exGeometryUtility.RectRect_Contains( _rect, boundingRect ) != 0 ||
                         exGeometryUtility.RectRect_Intersect( _rect, boundingRect ) )
                    {
                        spriteNodes.Add(node);
                    }
                }
            }
        }
    }
}
