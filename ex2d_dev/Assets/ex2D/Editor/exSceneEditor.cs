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

    exRectSelection rectSelection = null;

    // 
    List<exSpriteBase> spriteNodes = new List<exSpriteBase>();

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
        // position = new Rect ( 50, 50, 800, 600 );

        rectSelection = new exRectSelection( PickObject,
                                             PickRectObjects,
                                             ConfirmRectSelection );

        Reset();
        Repaint();
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
                }
            EditorGUILayout.EndHorizontal();
        }

        // SerializedObject
        if ( ex2DMng.instance != null ) {
            curSerializedObject = new SerializedObject(ex2DMng.instance);
            ex2DMng.instance.ResortLayerDepth();
            ex2DMng.instance.UpdateLayers();
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
                    if ( o is exTextureInfo ) {
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

                        Selection.activeObject = gameObject;
                    }
                }

                Repaint();
                e.Use();
            }
            break;

        case EventType.DragExited:
            draggingObjects.Clear();
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
                DrawNode ( spriteNodes[i] );
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

            // Show a copy icon on the drag
            if ( DragAndDrop.visualMode == DragAndDropVisualMode.Copy ) {
                foreach ( Object o in draggingObjects ) {
                    if ( o is exTextureInfo ) {
                        DrawTextureInfoPreview ( o as exTextureInfo, 
                                                 SceneField_MapToWorld( _rect, Event.current.mousePosition) );
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

        Vector2 halfSize = new Vector2( _textureInfo.width * 0.5f,
                                        _textureInfo.height * 0.5f );

        float s0 = (float) _textureInfo.x / (float) _textureInfo.texture.width;
        float s1 = (float) (_textureInfo.x+_textureInfo.width)  / (float) _textureInfo.texture.width;
        float t0 = (float) _textureInfo.y / (float) _textureInfo.texture.height;
        float t1 = (float) (_textureInfo.y+_textureInfo.height) / (float) _textureInfo.texture.height;

        exEditorUtility.AlphaBlendedMaterial().mainTexture = _textureInfo.texture;
        exEditorUtility.AlphaBlendedMaterial().SetPass(0);
        GL.Begin(GL.QUADS);
            GL.Color( new Color( 1.0f, 1.0f, 1.0f, 0.5f ) );

            GL.TexCoord2 ( s0, t0 );
            GL.Vertex3 ( -halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );

            GL.TexCoord2 ( s0, t1 );
            GL.Vertex3 ( -halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

            GL.TexCoord2 ( s1, t1 );
            GL.Vertex3 (  halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

            GL.TexCoord2 ( s1, t0 );
            GL.Vertex3 (  halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );
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
        Vector3[] vertices = _node.GetVertices();
        if ( _node is exSprite ) {
            exEditorUtility.DrawRectLine ( vertices, Color.white );
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
                Vector3 center = new Vector3( 0.0f, 0.0f, 0.0f );
                Vector3 size = new Vector3( spriteBase.width, spriteBase.height, 0.0f );
                Vector3 min = center - size * 0.5f;
                Vector3 max = center + size * 0.5f;

                Vector3 dir = size.normalized;
                Vector3 dir_a = new Vector3( dir.x, dir.y, 0.0f );
                Vector3 dir_b = new Vector3( -dir.x, dir.y, 0.0f );
                Vector3 dir_up = Vector3.up;
                Vector3 dir_right = Vector3.right;

                Vector3 result = Vector3.zero; 

                Matrix4x4 oldMatrix = Handles.matrix;
                Handles.matrix = trans.localToWorldMatrix;
                Handles.color = Color.white;

                EditorGUI.BeginChangeCheck();

                    result = Handles.Slider ( new Vector3( max.x, 0.0f, 0.0f ), dir_right, handleSize * 0.05f, Handles.DotCap, -1 );
                    max.x = result.x;

                    result = Handles.Slider ( new Vector3( min.x, 0.0f, 0.0f ), dir_right, handleSize * 0.05f, Handles.DotCap, -1 );
                    min.x = result.x;

                    result = Handles.Slider ( new Vector3( 0.0f, max.y, 0.0f ), dir_up, handleSize * 0.05f, Handles.DotCap, -1 );
                    max.y = result.y;

                    result = Handles.Slider ( new Vector3( 0.0f, min.y, 0.0f ), dir_up, handleSize * 0.05f, Handles.DotCap, -1 );
                    min.y = result.y;

                    result = Handles.Slider ( new Vector3( max.x, max.y, 0.0f ), dir_a, handleSize * 0.05f, Handles.DotCap, -1 );
                    max = result;

                    result = Handles.Slider ( new Vector3( min.x, max.y, 0.0f ), dir_b, handleSize * 0.05f, Handles.DotCap, -1 );
                    min.x = result.x;
                    max.y = result.y;

                    result = Handles.Slider ( new Vector3( max.x, min.y, 0.0f ), dir_b, handleSize * 0.05f, Handles.DotCap, -1 );
                    max.x = result.x;
                    min.y = result.y;

                    result = Handles.Slider ( new Vector3( min.x, min.y, 0.0f ), dir_a, handleSize * 0.05f, Handles.DotCap, -1 );
                    min = result;

                if ( EditorGUI.EndChangeCheck() ) {
                    center = (max + min) * 0.5f;
                    size = (max - min);

                    Vector3 pos = trans.TransformPoint(center);
                    trans_position.x = pos.x;
                    trans_position.y = pos.y;
                    trans.position = trans_position;
                    spriteBase.width = size.x;
                    spriteBase.height = size.y;
                }
                Handles.matrix = oldMatrix;
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
                trans.position = trans_position;
                trans.rotation = trans_rotation;
            }

            if ( spriteBase ) {
                spriteBase.UpdateTransform ();
                if ( spriteBase.updateFlags != UpdateFlags.None ) {
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
        exSprite sprite = _node as exSprite;
        Vector2 screenPos = Vector2.zero;

        if ( sprite ) {
            // Rect boundingRect = new Rect ( screenPos.x - sprite.textureInfo.rotatedWidth/2.0f * scale,
            //                                screenPos.y - sprite.textureInfo.rotatedHeight/2.0f * scale,
            //                                sprite.textureInfo.rotatedWidth * scale,
            //                                sprite.textureInfo.rotatedHeight * scale );
            Rect boundingRect = sprite.GetAABoundingRect();
            screenPos = SceneField_WorldToScreen ( _rect, boundingRect.center );
            boundingRect = new Rect ( screenPos.x - boundingRect.width * scale / 2.0f,
                                      screenPos.y - boundingRect.height * scale / 2.0f,
                                      boundingRect.width * scale,
                                      boundingRect.height * scale );
            boundingRect = exGeometryUtility.Rect_FloorToInt(boundingRect);

            return boundingRect;
        }

        screenPos = SceneField_WorldToScreen ( _rect, sprite.transform.position );
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
