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

    Camera editCamera;
    Color background = Color.gray;
    Rect sceneViewRect = new Rect( 0, 0, 1, 1 );
    SerializedObject curSerializedObject = null;
    List<Object> draggingObjects = new List<Object>();

    exLayer activeLayer = null;
    // exLayer draggingLayer = null; TODO

    exRectSelection<Object> rectSelection = null;

    // 
    List<exSpriteBase> spriteNodes = new List<exSpriteBase>();

    int firstResolutionIdx = 0;
    int secondResolutionIdx = 0;
    string[] resolutionList = new string[] { 
        "None",
        "320 x 480 (iPhone3 Tall)",  // iPhone3 Tall
        "480 x 320 (iPhone3 Wide)",  // iPhone3 Wide
        "640 x 960 (iPhone4 Tall)",  // iPhone4 Tall
        "960 x 640 (iPhone4 Wide)",  // iPhone4 Wide
        "640 x 1136 (iPhone5 Tall)", // iPhone5 Tall
        "1136 x 640 (iPhone5 Wide)", // iPhone5 Wide
        "768 x 1024 (iPad Tall)",    // iPad Tall
        "1024 x 768 (iPad Wide)",    // iPad Wide
        "Custom",
    };

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        // camera
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

        title = "2D Scene Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = true;

        rectSelection = new exRectSelection<Object>( PickObject,
                                                     PickRectObjects,
                                                     ConfirmRectSelection );
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

    void OnInspectorUpdate () {
        // TODO: this make selection can not select exMeshes, confirm with Jare { 
        // ex2DMng.instance.ForceRenderScene();
        // } TODO end 
        Repaint();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        // pre-check
        PreCheck ();

        //
        if ( ex2DMng.instance == null )
            return;

        if ( curSerializedObject == null || curSerializedObject.targetObject == null )
            curSerializedObject = new SerializedObject(ex2DMng.instance);

        //
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
        ProcessSceneEditorHandles();

        //
        ProcessSceneEditorEvents();

        rectSelection.SetSelection( Selection.objects );
        rectSelection.OnGUI();

        //
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
        // draggingLayer = null; TODO

        if ( ex2DMng.instance ) {
            ex2DMng.instance.ForceRenderScene();
        }
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
                    ex2DMng.instance.CreateLayer();
                }
            EditorGUILayout.EndHorizontal();

            // SerializedObject
            if ( ex2DMng.instance != null ) {
                ex2DMng.instance.ResortLayerDepth();
                ex2DMng.instance.UpdateLayers();
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Toolbar () {
        EditorGUILayout.BeginHorizontal ( EditorStyles.toolbar );

            GUILayout.FlexibleSpace();

            // ======================================================== 
            // Update Scene 
            // ======================================================== 

            if ( GUILayout.Button ("Update Scene", EditorStyles.toolbarButton) ) {
                ex2DMng.instance.ForceRenderScene();
                EditorUtility.SetDirty(ex2DMng.instance);
            }

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

        // ======================================================== 
        // General 
        // ======================================================== 

        EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField ( "General", settingsStyles.boldLabel );
            EditorGUILayout.ObjectField ( ""
                                          , ex2DMng.instance
                                          , typeof(ex2DMng)
                                          , false 
                                        );

            // Color oldContentColor = GUI.contentColor;
                // GUI.contentColor = Color.yellow;
                firstResolutionIdx = EditorGUILayout.Popup ( "1st Resolution", firstResolutionIdx, resolutionList );

                // GUI.contentColor = Color.red;
                secondResolutionIdx = EditorGUILayout.Popup ( "2nd Resolution", secondResolutionIdx, resolutionList );
            // GUI.contentColor = oldContentColor;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // ======================================================== 
        // Layers 
        // ======================================================== 

        EditorGUILayout.BeginVertical( settingsStyles.boxBackground );
            EditorGUILayout.LabelField ( "Layers", settingsStyles.boldLabel );
            GUILayout.Space(2);

            Layout_LayerElementsField();

        EditorGUILayout.EndVertical();

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
                // draggingLayer = null; TODO
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
                    // draggingLayer = _layer; TODO
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
                                               new Rect( (-half_w/scale + editCamera.transform.position.x)/checker.width,
                                                         (-half_h/scale + editCamera.transform.position.y)/checker.height,
                                                         sceneViewRect.width/(checker.width * scale), 
                                                         sceneViewRect.height/(checker.height * scale) ) );
            GUI.color = old;

            // center line
            float center_x = -editCamera.transform.position.x * scale + sceneViewRect.x + half_w;
            float center_y =  editCamera.transform.position.y * scale + sceneViewRect.y + half_h;
            if ( center_y >= sceneViewRect.y && center_y <= sceneViewRect.yMax )
                exEditorUtility.DrawLine ( sceneViewRect.x, center_y, sceneViewRect.xMax, center_y, Color.white, 1 );
            if ( center_x >= sceneViewRect.x && center_x <= sceneViewRect.xMax )
                exEditorUtility.DrawLine ( center_x, sceneViewRect.y, center_x, sceneViewRect.yMax, Color.white, 1 );

            // draw scene
            DoCulling (sceneViewRect);
            DrawScene (sceneViewRect);

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

        case EventType.DragUpdated:
            if ( ex2DMng.instance.layerList.Count > 0 &&  _rect.Contains(e.mousePosition) ) {
                if ( activeLayer == null ) {
                    activeLayer = ex2DMng.instance.layerList[0];
                }

                // Show a copy icon on the drag
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( o is exTextureInfo ||
                         o is exBitmapFont ) 
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        break;
                    }
                }
                draggingObjects.Clear();
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    draggingObjects.Add(o);
                }

                Repaint();
                e.Use();
            }
            break;

        case EventType.DragPerform:
            if ( _rect.Contains(e.mousePosition) ) {
                DragAndDrop.AcceptDrag();

                foreach ( Object o in DragAndDrop.objectReferences ) {
                    GameObject newGO = null; 

                    if ( o is exTextureInfo ) {
                        newGO = new GameObject(o.name);
                        exSprite sprite = newGO.AddComponent<exSprite>();
                        if ( sprite.shader == null )
                            sprite.shader = Shader.Find("ex2D/Alpha Blended");
                        sprite.textureInfo = o as exTextureInfo;
                    }
                    else if ( o is exBitmapFont ) {
                        newGO = new GameObject(o.name);
                        exSpriteFont spriteFont = newGO.AddComponent<exSpriteFont>();
                        if ( spriteFont.shader == null )
                            spriteFont.shader = Shader.Find("ex2D/Alpha Blended");
                        spriteFont.font = o as exBitmapFont;
                    }

                    if ( newGO != null && activeLayer != null ) {
                        newGO.transform.position = SceneField_MapToWorld( _rect, e.mousePosition);
                        newGO.transform.localScale = Vector3.one;
                        newGO.transform.rotation = Quaternion.identity;

                        exSpriteBase sp = newGO.GetComponent<exSpriteBase>();
                        activeLayer.Add(sp);

                        EditorUtility.SetDirty(activeLayer);
                        EditorUtility.SetDirty(sp);

                        Selection.activeObject = newGO;
                    }
                }

                Repaint();
                e.Use();
            }
            break;

        case EventType.DragExited:
            draggingObjects.Clear();
            Repaint();
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
            GL.LoadPixelMatrix( editCamera.transform.position.x - (_rect.width  * 0.5f) / scale, 
                                editCamera.transform.position.x + (_rect.width  * 0.5f) / scale, 
                                editCamera.transform.position.y - (_rect.height * 0.5f) / scale,
                                editCamera.transform.position.y + (_rect.height * 0.5f) / scale );

            // draw culled sprite nodes
            for ( int i = 0; i < spriteNodes.Count; ++i ) {
                if (spriteNodes[i].visible) {
                    DrawNode ( spriteNodes[i] );
                }
            }

            // draw selected objects
            Transform[] selection = Selection.GetTransforms(SelectionMode.Editable);
            for ( int i = 0; i < selection.Length; ++i ) {
                Transform trans = selection[i];
                exSpriteBase spriteBase = trans.GetComponent<exSpriteBase>();
                if ( spriteBase ) {
                    // DrawAABoundingRect (spriteBase);
                    DrawBoundingRect (spriteBase);
                }
            }

            // draw resolution line
            DrawResolutionRect ( secondResolutionIdx, Color.red );
            DrawResolutionRect ( firstResolutionIdx, Color.yellow );

            // Show a copy icon on the drag
            if ( DragAndDrop.visualMode == DragAndDropVisualMode.Copy ) {
                foreach ( Object o in draggingObjects ) {
                    if ( o is exTextureInfo ) {
                        DrawTextureInfoPreview ( o as exTextureInfo, 
                                                 SceneField_MapToWorld( _rect, Event.current.mousePosition) );
                    }
                    else if ( o is exBitmapFont ) {
                        // TODO:
                    }
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

    void DrawTextureInfoPreview ( exTextureInfo _textureInfo, Vector3 _pos ) {
        if (_textureInfo.texture == null) {
            return;
        }

        Vector2 halfSize = new Vector2( _textureInfo.width * 0.5f,
                                        _textureInfo.height * 0.5f );

        float s0 = (float) _textureInfo.x / (float) _textureInfo.texture.width;
        float s1 = (float) (_textureInfo.x+_textureInfo.rotatedWidth)  / (float) _textureInfo.texture.width;
        float t0 = (float) _textureInfo.y / (float) _textureInfo.texture.height;
        float t1 = (float) (_textureInfo.y+_textureInfo.rotatedHeight) / (float) _textureInfo.texture.height;

        exEditorUtility.AlphaBlendedMaterial().mainTexture = _textureInfo.texture;
        exEditorUtility.AlphaBlendedMaterial().SetPass(0);
        GL.Begin(GL.QUADS);
            GL.Color( new Color( 1.0f, 1.0f, 1.0f, 0.5f ) );

            if ( _textureInfo.rotated == false ) {
                GL.TexCoord2 ( s0, t0 );
                GL.Vertex3 ( -halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );

                GL.TexCoord2 ( s0, t1 );
                GL.Vertex3 ( -halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                GL.TexCoord2 ( s1, t1 );
                GL.Vertex3 (  halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                GL.TexCoord2 ( s1, t0 );
                GL.Vertex3 (  halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );
            }
            else {
                GL.TexCoord2 ( s1, t0 );
                GL.Vertex3 ( -halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );

                GL.TexCoord2 ( s0, t0 );
                GL.Vertex3 ( -halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                GL.TexCoord2 ( s0, t1 );
                GL.Vertex3 (  halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                GL.TexCoord2 ( s1, t1 );
                GL.Vertex3 (  halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );
            }

        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawAABoundingRect ( exSpriteBase _node ) {
        Rect boundingRect = _node.GetAABoundingRect();

        exEditorUtility.DrawRectLine ( new Vector3[] {
                                            new Vector3 ( boundingRect.xMin, boundingRect.yMin, 0.0f ),
                                            new Vector3 ( boundingRect.xMin, boundingRect.yMax, 0.0f ),
                                            new Vector3 ( boundingRect.xMax, boundingRect.yMax, 0.0f ),
                                            new Vector3 ( boundingRect.xMax, boundingRect.yMin, 0.0f ),
                                       }, Color.white );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawBoundingRect ( exSpriteBase _node ) {
        Vector3[] vertices = _node.GetWorldVertices();
        if ( _node is exSprite || _node is exSpriteFont ) {
            exEditorUtility.DrawRectLine ( vertices, Color.white );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawResolutionRect ( int _idx, Color _color ) {
        // "320 x 480 (iPhone3 Tall)",  // iPhone3 Tall
        // "480 x 320 (iPhone3 Wide)",  // iPhone3 Wide
        // "640 x 960 (iPhone4 Tall)",  // iPhone4 Tall
        // "960 x 640 (iPhone4 Wide)",  // iPhone4 Wide
        // "640 x 1136 (iPhone5 Tall)", // iPhone5 Tall
        // "1136 x 640 (iPhone5 Wide)", // iPhone5 Wide
        // "768 x 1024 (iPad Tall)",    // iPad Tall
        // "1024 x 768 (iPad Wide)",    // iPad Wide

        switch ( _idx ) {
        case 0:
            break;

        case 1:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -160.0f, -240.0f, 0.0f ),
                                           new Vector3 ( -160.0f,  240.0f, 0.0f ),
                                           new Vector3 (  160.0f,  240.0f, 0.0f ),
                                           new Vector3 (  160.0f, -240.0f, 0.0f ),
                                           }, _color );
            break;

        case 2:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -240.0f, -160.0f, 0.0f ),
                                           new Vector3 ( -240.0f,  160.0f, 0.0f ),
                                           new Vector3 (  240.0f,  160.0f, 0.0f ),
                                           new Vector3 (  240.0f, -160.0f, 0.0f ),
                                           }, _color );
            break;

        case 3:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -320.0f, -480.0f, 0.0f ),
                                           new Vector3 ( -320.0f,  480.0f, 0.0f ),
                                           new Vector3 (  320.0f,  480.0f, 0.0f ),
                                           new Vector3 (  320.0f, -480.0f, 0.0f ),
                                           }, _color );
            break;

        case 4:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -480.0f, -320.0f, 0.0f ),
                                           new Vector3 ( -480.0f,  320.0f, 0.0f ),
                                           new Vector3 (  480.0f,  320.0f, 0.0f ),
                                           new Vector3 (  480.0f, -320.0f, 0.0f ),
                                           }, _color );
            break;

        case 5:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -320.0f, -568.0f, 0.0f ),
                                           new Vector3 ( -320.0f,  568.0f, 0.0f ),
                                           new Vector3 (  320.0f,  568.0f, 0.0f ),
                                           new Vector3 (  320.0f, -568.0f, 0.0f ),
                                           }, _color );
            break;

        case 6:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -568.0f, -320.0f, 0.0f ),
                                           new Vector3 ( -568.0f,  320.0f, 0.0f ),
                                           new Vector3 (  568.0f,  320.0f, 0.0f ),
                                           new Vector3 (  568.0f, -320.0f, 0.0f ),
                                           }, _color );
            break;

        case 7:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -384.0f, -512.0f, 0.0f ),
                                           new Vector3 ( -384.0f,  512.0f, 0.0f ),
                                           new Vector3 (  384.0f,  512.0f, 0.0f ),
                                           new Vector3 (  384.0f, -512.0f, 0.0f ),
                                           }, _color );
            break;

        case 8:
            exEditorUtility.DrawRectLine ( new Vector3[] {
                                           new Vector3 ( -512.0f, -384.0f, 0.0f ),
                                           new Vector3 ( -512.0f,  384.0f, 0.0f ),
                                           new Vector3 (  512.0f,  384.0f, 0.0f ),
                                           new Vector3 (  512.0f, -384.0f, 0.0f ),
                                           }, _color );
            break;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DebugInfos () {
        EditorGUILayout.BeginHorizontal();
            string text = "";
            int width = -1;
            
            text = "Camera Pos: " + editCamera.transform.position;
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

            text = "Mouse: " + SceneField_MapToWorld( sceneViewRect, Event.current.mousePosition);
            width = 300;
            EditorGUILayout.LabelField ( text, GUILayout.Width(width) );
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessSceneEditorHandles () {

        //
        GUI.BeginGroup( sceneViewRect );
        editCamera.enabled = true;
        editCamera.aspect = sceneViewRect.width/sceneViewRect.height;
        editCamera.orthographicSize = (sceneViewRect.height * 0.5f) / scale;
        Rect rect = new Rect( 0, 0, sceneViewRect.width, sceneViewRect.height );
        Handles.ClearCamera( rect, editCamera );
        Handles.SetCamera( rect, editCamera );

        //
        Transform trans = Selection.activeTransform;
        if ( trans ) {
            Vector3 trans_position = trans.position;
            Quaternion trans_rotation = trans.rotation;
            float handleSize = HandleUtility.GetHandleSize(trans_position);

            // resize
            exSpriteBase spriteBase = trans.GetComponent<exSpriteBase>();
            if ( spriteBase && spriteBase.customSize ) {
                Vector3[] vertices = spriteBase.GetLocalVertices();
                Vector3 min = new Vector3 ( float.MaxValue, float.MaxValue, 0.0f );
                Vector3 max = new Vector3 ( float.MinValue, float.MinValue, 0.0f );
                foreach ( Vector3 vertex in vertices ) {
                    if ( vertex.x > max.x )
                        max.x = vertex.x;
                    if ( vertex.x < min.x )
                        min.x = vertex.x;

                    if ( vertex.y > max.y )
                        max.y = vertex.y;
                    if ( vertex.y < min.y )
                        min.y = vertex.y;
                }
                Vector3 center = (max + min) * 0.5f;
                Vector3 size = new Vector3( spriteBase.width, spriteBase.height, 0.0f );

                Vector3 tl = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f, center.y + size.y * 0.5f, 0.0f ) );
                Vector3 tc = trans.TransformPoint ( new Vector3 (                 center.x, center.y + size.y * 0.5f, 0.0f ) );
                Vector3 tr = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f, center.y + size.y * 0.5f, 0.0f ) );
                Vector3 ml = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f,                 center.y, 0.0f ) );
                // Vector3 mc = trans.TransformPoint ( new Vector3 (                     center.x,                 center.y, 0.0f ) );
                Vector3 mr = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f,                 center.y, 0.0f ) );
                Vector3 bl = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f, center.y - size.y * 0.5f, 0.0f ) );
                Vector3 bc = trans.TransformPoint ( new Vector3 (                 center.x, center.y - size.y * 0.5f, 0.0f ) );
                Vector3 br = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f, center.y - size.y * 0.5f, 0.0f ) );

                Vector3 dir_up = trans.up;
                Vector3 dir_right = trans.right;
                Vector3 delta = Vector3.zero;
                bool changed = false;

                EditorGUI.BeginChangeCheck();
                    Vector3 ml2 = Handles.Slider ( ml, dir_right, handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = ml2 - ml;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta = -delta;
                    size += delta;
                    center = (ml2 + mr) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 mr2 = Handles.Slider ( mr, dir_right, handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = mr2 - mr;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    size += delta;
                    center = (mr2 + ml) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 tc2 = Handles.Slider ( tc, dir_up,    handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tc2 - tc;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    size += delta;
                    center = (tc2 + bc) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 bc2 = Handles.Slider ( bc, dir_up,    handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = bc2 - bc;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta = -delta;
                    size += delta;
                    center = (bc2 + tc) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 tr2 = Handles.FreeMoveHandle ( tr, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tr2 - tr;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    size += delta;
                    center = (tr2 + bl) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 tl2 = Handles.FreeMoveHandle ( tl, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tl2 - tl;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x = -delta.x;
                    size += delta;
                    center = (tl2 + br) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 br2 = Handles.FreeMoveHandle ( br, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = br2 - br;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.y = -delta.y;
                    size += delta;
                    center = (br2 + tl) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 bl2 = Handles.FreeMoveHandle ( bl, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = bl2 - bl;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta = -delta;
                    size += delta;
                    center = (bl2 + tr) * 0.5f;
                    changed = true;
                }

                if ( changed ) {
                    spriteBase.width = size.x;
                    spriteBase.height = size.y;

                    switch (spriteBase.anchor) {
                    case Anchor.TopLeft:    trans.position = center + trans.rotation * new Vector3( -size.x*0.5f,  size.y*0.5f, 0.0f ); break;
                    case Anchor.TopCenter:  trans.position = center + trans.rotation * new Vector3(         0.0f,  size.y*0.5f, 0.0f ); break;
                    case Anchor.TopRight:   trans.position = center + trans.rotation * new Vector3(  size.x*0.5f,  size.y*0.5f, 0.0f ); break;
                    case Anchor.MidLeft:    trans.position = center + trans.rotation * new Vector3( -size.x*0.5f,         0.0f, 0.0f ); break;
                    case Anchor.MidCenter:  trans.position = center;                                                                    break;
                    case Anchor.MidRight:   trans.position = center + trans.rotation * new Vector3(  size.x*0.5f,         0.0f, 0.0f ); break;
                    case Anchor.BotLeft:    trans.position = center + trans.rotation * new Vector3( -size.x*0.5f, -size.y*0.5f, 0.0f ); break;
                    case Anchor.BotCenter:  trans.position = center + trans.rotation * new Vector3(         0.0f, -size.y*0.5f, 0.0f ); break;
                    case Anchor.BotRight:   trans.position = center + trans.rotation * new Vector3(  size.x*0.5f, -size.y*0.5f, 0.0f ); break;
                    }
                }
            }

            EditorGUI.BeginChangeCheck();

                // position
                Handles.color = new Color ( 0.858823538f, 0.243137255f, 0.113725491f, 0.93f );
                trans_position = Handles.Slider ( trans_position, trans_rotation * Vector3.right );

                Handles.color = new Color ( 0.6039216f, 0.9529412f, 0.282352954f, 0.93f );
                trans_position = Handles.Slider ( trans_position, trans_rotation * Vector3.up );

                Handles.color = new Color( 0.8f, 0.8f, 0.8f, 0.93f );
                trans_position = Handles.FreeMoveHandle ( trans_position, trans_rotation, handleSize * 0.15f, Vector3.zero, Handles.RectangleCap );

                // rotation
                Handles.color = new Color ( 0.227450982f, 0.478431374f, 0.972549f, 0.93f );
                trans_rotation = Handles.Disc ( trans_rotation, trans_position, Vector3.forward, handleSize * 0.5f, true, 1 );

            if ( EditorGUI.EndChangeCheck() ) {
                if ( Selection.transforms.Length == 1 ) {
                    trans.position = trans_position;
                    trans.rotation = trans_rotation;
                }
                else {
                    Vector3 delta = trans_position - trans.position;
                    // float deltaAngle = Quaternion.Angle( trans_rotation, trans.rotation );

                    float deltaAngle;
                    Vector3 axis;
                    (Quaternion.Inverse(trans.rotation) * trans_rotation).ToAngleAxis(out deltaAngle, out axis);
                    axis = (Vector3) (trans.rotation * axis);

                    foreach ( Transform transObj in Selection.transforms ) {
                        transObj.position += delta;
                        transObj.RotateAround( trans_position, axis, deltaAngle );
                    }

                    trans.position = trans_position;
                    trans.rotation = trans_rotation;
                }
            }

            if ( spriteBase ) {
                spriteBase.UpdateTransform ();
                if ( spriteBase.updateFlags != exUpdateFlags.None ) {
                    ex2DMng.instance.ResortLayerDepth();
                    ex2DMng.instance.UpdateLayers();
                }
            }
        }
        editCamera.enabled = false;
        GUI.EndGroup();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessSceneEditorEvents () {
        int controlID = GUIUtility.GetControlID(sceneEditorHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.KeyDown:
            if ( (e.command || e.control) &&
                 (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete) ) 
            {
                Transform[] selection = Selection.GetTransforms(SelectionMode.Editable);
                for ( int i = selection.Length-1; i >= 0; --i ) {
                    Transform trans = selection[i];
                    if ( trans != null )
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

    Object PickObject ( Vector2 _position ) {
        Object[] objs = PickRectObjects( new Rect(_position.x-1,_position.y-1,2,2) );
        if ( objs.Length > 0 )
            return objs[0];
        return null;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Object[] PickRectObjects ( Rect _rect ) {
        List<Object> objects = new List<Object>();
        for ( int i = spriteNodes.Count-1; i >= 0; --i ) {
            exSpriteBase node = spriteNodes[i];
            Rect boundingRect = MapBoundingRect ( sceneViewRect, node );
            if ( exGeometryUtility.RectRect_Contains( _rect, boundingRect ) != 0 ||
                 exGeometryUtility.RectRect_Intersect( _rect, boundingRect ) )
            {
                objects.Add(node.gameObject);
            }
        }

        return objects.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ConfirmRectSelection ( Object _activeObj, Object[] _selectedObjs ) {
        Selection.activeObject = _activeObj;
        Selection.objects = _selectedObjs;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Vector3 SceneField_MapToWorld ( Rect _rect, Vector2 _mousePosition ) {
        Vector3 screenPos = SceneField_MapToScreen( _rect, _mousePosition );
        return new Vector3 (  (screenPos.x - _rect.width/2.0f)/scale + editCamera.transform.position.x,
                             -(screenPos.y - _rect.height/2.0f)/scale + editCamera.transform.position.y,
                             0.0f );
    }
    Vector3 SceneField_MapToScreen ( Rect _rect, Vector2 _mousePosition ) {
        return new Vector3 ( _mousePosition.x - _rect.x,
                             _mousePosition.y - _rect.y,
                             0.0f );
    }
    Vector2 SceneField_WorldToScreen ( Rect _rect, Vector3 _worldPos ) {
        return new Vector2 ( (_worldPos.x - editCamera.transform.position.x) * scale + _rect.x + _rect.width/2.0f,
                            -(_worldPos.y - editCamera.transform.position.y) * scale + _rect.y + _rect.height/2.0f  );
    }

    Rect MapBoundingRect ( Rect _rect, exSpriteBase _node ) {
        exSpriteBase spriteBase = _node as exSpriteBase;
        Vector2 screenPos = Vector2.zero;

        if ( spriteBase ) {
            Rect boundingRect = spriteBase.GetAABoundingRect();
            screenPos = SceneField_WorldToScreen ( _rect, boundingRect.center );
            boundingRect = new Rect ( screenPos.x - boundingRect.width * scale / 2.0f,
                                      screenPos.y - boundingRect.height * scale / 2.0f,
                                      boundingRect.width * scale,
                                      boundingRect.height * scale );
            boundingRect = exGeometryUtility.Rect_FloorToInt(boundingRect);

            return boundingRect;
        }

        screenPos = SceneField_WorldToScreen ( _rect, _node.transform.position );
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
                System.Array.Sort<exSpriteBase>(spriteList);
                foreach ( exSpriteBase node in spriteList ) {
                    if ( node.enabled ) {
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
}
