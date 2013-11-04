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
    List<exLayeredSprite> spriteNodes = new List<exLayeredSprite>();

    int firstResolutionIdx = 0;
    int secondResolutionIdx = 0;

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
            = EditorUtility.CreateGameObjectWithHideFlags ( "SceneView_Camera", 
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
        // ex2DRenderer.instance.ForceRenderScene();
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
        if ( ex2DRenderer.instance == null )
            return;

        if ( curSerializedObject == null || curSerializedObject.targetObject == null )
            curSerializedObject = new SerializedObject(ex2DRenderer.instance);

        //
        curSerializedObject.Update ();

        // toolbar
        Toolbar ();

        // layer & scene
        EditorGUILayout.BeginHorizontal();

            //
            int width = 250;
            Settings ( width );

            //
            GUILayout.Space(40);

            // scene filed
            float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
            Layout_SceneViewField ( Mathf.FloorToInt(position.width - width - 40 ),
                                    Mathf.FloorToInt(position.height - toolbarHeight - toolbarHeight ) );
        EditorGUILayout.EndHorizontal();

        GUILayout.FlexibleSpace();

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

        if ( ex2DRenderer.instance ) {
            ex2DRenderer.instance.ForceRenderScene();
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

        // if ex2DRenderer is null
        if ( ex2DRenderer.instance == null ) {
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
                GUILayout.Space(30);

                Color old = GUI.color;
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField ( "Can't find ex2DRenderer in the scene!" );
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
                    ex2DCamera.gameObject.AddComponent<ex2DRenderer>();
                    ex2DRenderer.instance.CreateLayer();
                }
            EditorGUILayout.EndHorizontal();

            // SerializedObject
            if ( ex2DRenderer.instance != null ) {
                ex2DRenderer.instance.ResortLayerDepth();
                ex2DRenderer.instance.UpdateLayers();
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
                ex2DRenderer.instance.ForceRenderScene();
                EditorUtility.SetDirty(ex2DRenderer.instance);
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

            if ( GUILayout.Button( exEditorUtility.textureHelp, EditorStyles.toolbarButton ) ) {
                Help.BrowseURL("http://ex-dev.com/ex2d/docs/");
            }

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Settings ( int _width ) {
        EditorGUILayout.BeginHorizontal( new GUILayoutOption [] {
                                           GUILayout.Width(_width), 
                                           GUILayout.MinWidth(_width), 
                                           GUILayout.MaxWidth(_width),
                                           GUILayout.ExpandWidth(false),
                                       } );
        GUILayout.Space(10);

        // ======================================================== 
        // General 
        // ======================================================== 

        EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField ( "General", settingsStyles.boldLabel );
            EditorGUILayout.ObjectField ( ""
                                          , ex2DRenderer.instance
                                          , typeof(ex2DRenderer)
                                          , false 
                                        );

            // Color oldContentColor = GUI.contentColor;
                // GUI.contentColor = Color.yellow;
                firstResolutionIdx = EditorGUILayout.Popup ( "1st Resolution", firstResolutionIdx, exEditorUtility.resolutionDescList );

                // GUI.contentColor = Color.red;
                secondResolutionIdx = EditorGUILayout.Popup ( "2nd Resolution", secondResolutionIdx, exEditorUtility.resolutionDescList );
            // GUI.contentColor = oldContentColor;

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        
        // ======================================================== 
        // Layers 
        // ======================================================== 

        EditorGUILayout.BeginVertical( settingsStyles.boxBackground );
            EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField ( "Layers", settingsStyles.boldLabel );
                if (ex2DRenderer.instance.customizeLayerZ) {
                    if (GUILayout.Button ("Refresh")) {
                        ex2DRenderer.instance.layerList.Sort((x, y) => x.customZ.CompareTo(y.customZ));    
                    }
                }
            EditorGUILayout.EndHorizontal();
        
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

        Rect rect = GUILayoutUtility.GetRect ( 10f, settingsStyles.elementHeight * ex2DRenderer.instance.layerList.Count );
        LayerElementsField (rect, layerListProp);

        // add layer button
        EditorGUILayout.BeginHorizontal( settingsStyles.toolbar, new GUILayoutOption[0]);

        // custom z
        bool useCustomZ = GUILayout.Toggle( ex2DRenderer.instance.customizeLayerZ, "Customize Z", settingsStyles.toolbarButton );
        if ( useCustomZ != ex2DRenderer.instance.customizeLayerZ ) {
            ex2DRenderer.instance.customizeLayerZ = useCustomZ;
            // resort layer list
            ex2DRenderer.instance.layerList.Sort((x, y) => x.customZ.CompareTo(y.customZ));
            EditorUtility.SetDirty(ex2DRenderer.instance);
        }
        
        GUILayout.FlexibleSpace();
            
            GUI.enabled = (ex2DRenderer.instance.customizeLayerZ == false);
            
            if ( GUILayout.Button( "UP", settingsStyles.toolbarButton ) ) 
            {
                int curIdx = ex2DRenderer.instance.layerList.IndexOf(activeLayer);
                if ( curIdx != -1 ) {
                    int nextIdx = System.Math.Max(curIdx-1,0);
                    layerListProp.MoveArrayElement ( curIdx, nextIdx );
                    // activeLayer = ex2DRenderer.instance.layerList[nextIdx];
                }
            }
            if ( GUILayout.Button( "DOWN", settingsStyles.toolbarButton ) ) 
            {
                int curIdx = ex2DRenderer.instance.layerList.IndexOf(activeLayer);
                if ( curIdx != -1 ) {
                    int nextIdx = System.Math.Min(curIdx+1,ex2DRenderer.instance.layerList.Count-1);
                    layerListProp.MoveArrayElement ( curIdx, nextIdx );
                    // activeLayer = ex2DRenderer.instance.layerList[nextIdx];
                }
            }
            
            GUI.enabled = true;
            
            if ( GUILayout.Button( settingsStyles.iconToolbarPlus, 
                                   settingsStyles.toolbarDropDown ) ) 
            {
                ex2DRenderer.instance.CreateLayer();
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

        // show
        cur_x += 5.0f;
        size = EditorStyles.toggle.CalcSize( GUIContent.none );
        bool newShow = EditorGUI.Toggle ( new Rect ( cur_x, _rect.y + 3f, size.x, size.y ),
                                          _layer.show );
        if ( newShow != _layer.show ) {
            _layer.show = newShow;
            EditorUtility.SetDirty(_layer);
        }
        cur_x += 10.0f;
        
        // layer name
        cur_x += 10.0f;
        string newName = EditorGUI.TextField ( new Rect ( cur_x, _rect.y + 4f, 100f, _rect.height - 8f ),
                                               _layer.gameObject.name ); 
        if ( newName != _layer.gameObject.name ) {
            _layer.gameObject.name = newName;
            EditorUtility.SetDirty(_layer.gameObject);
        }
        cur_x += 100.0f;
        
        if (ex2DRenderer.instance.customizeLayerZ) {
            // custom z
            cur_x += 10.0f;
            string z_text = EditorGUI.TextField (new Rect (cur_x, _rect.y + 4f, 40f, _rect.height - 8f),
                                              _layer.customZ.ToString ()); 
            float z;
            if (float.TryParse (z_text, out z) && _layer.customZ != z) {
                _layer.customZ = z;
                EditorUtility.SetDirty (_layer.gameObject);
            }
            cur_x += 40.0f;
        }
        
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
                ex2DRenderer.instance.DestroyLayer(_layer);
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
            // GUIStyle previewBackground = "AnimationCurveEditorBackground";
            // previewBackground.Draw(_rect, false, false, false, false);

            sceneViewRect = _rect;

            // draw scene
            DoCulling (sceneViewRect);
            DrawScene (sceneViewRect);

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
            if ( ex2DRenderer.instance.layerList.Count > 0 &&  _rect.Contains(e.mousePosition) ) {
                if ( activeLayer == null ) {
                    activeLayer = ex2DRenderer.instance.layerList[0];
                }

                // Show a copy icon on the drag
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( o is exTextureInfo ||
                         o is exBitmapFont ||
                         o is Font ||
                         o is exSpriteAnimationClip ||
                         o is exUILayoutInfo ) 
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
                        sprite.textureInfo = o as exTextureInfo;
                        InitSprite(sprite);
                    }
                    else if ( o is exBitmapFont ) {
                        newGO = new GameObject(o.name);
                        exSpriteFont spriteFont = newGO.AddComponent<exSpriteFont>();
                        spriteFont.shader = Shader.Find("ex2D/Alpha Blended");
                        spriteFont.SetFont(o as exBitmapFont);
                    }
                    else if ( o is Font ) {
                        newGO = new GameObject(o.name);
                        exSpriteFont spriteFont = newGO.AddComponent<exSpriteFont>();
                        spriteFont.shader = Shader.Find("ex2D/Alpha Blended (Use Vertex Color)");
                        spriteFont.SetFont(o as Font);
                    }
                    else if ( o is exSpriteAnimationClip ) {
                        exSpriteAnimationClip clip = o as exSpriteAnimationClip;
                        newGO = new GameObject(o.name);
                        exSprite sprite = newGO.AddComponent<exSprite>();
                        sprite.shader = Shader.Find("ex2D/Alpha Blended");
                        exSpriteAnimation spriteAnim = newGO.AddComponent<exSpriteAnimation>();
                        spriteAnim.defaultAnimation = clip;
                        spriteAnim.animations.Add(clip);

                        if ( clip.frameInfos.Count > 0 ) {
                            sprite.textureInfo = clip.frameInfos[0].textureInfo;
                        }
                        InitSprite(sprite);
                    }
                    else if ( o is exUILayoutInfo ) {
                        newGO = new GameObject(o.name);
                        exUILayout layout = newGO.AddComponent<exUILayout>();
                        layout.layoutInfo = o as exUILayoutInfo;
                        layout.Sync();
                    }

                    if ( newGO != null && activeLayer != null ) {
                        newGO.transform.position = SceneField_MapToWorld( _rect, e.mousePosition);
                        newGO.transform.localScale = Vector3.one;
                        newGO.transform.rotation = Quaternion.identity;

                        activeLayer.Add(newGO);

                        EditorUtility.SetDirty(activeLayer);
                        EditorUtility.SetDirty(newGO);

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

    void InitSprite ( exSprite _sprite ) { 
        if ( _sprite.shader == null )
            _sprite.shader = Shader.Find("ex2D/Alpha Blended");

        if ( _sprite.textureInfo != null ) {
            if ( _sprite.textureInfo.isDiced ) {
                _sprite.spriteType = exSpriteType.Diced;
                _sprite.customSize = false;
            }
            else if ( _sprite.textureInfo.hasBorder ) {
                _sprite.spriteType = exSpriteType.Sliced;
                _sprite.customSize = true;
                _sprite.width = _sprite.textureInfo.width;
                _sprite.height = _sprite.textureInfo.height;
            }
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
        GL.PushMatrix();

            //
            GL.Viewport(viewportRect);
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

            // draw culled sprite nodes
            for ( int i = 0; i < spriteNodes.Count; ++i ) {
                DrawNode ( spriteNodes[i] );
            }

            // draw selected objects
            Transform[] selection = Selection.GetTransforms(SelectionMode.Editable);
            for ( int i = 0; i < selection.Length; ++i ) {
                Transform trans = selection[i];

                PrefabType prefabType = PrefabUtility.GetPrefabType(trans);
                if ( prefabType == PrefabType.Prefab )
                    continue;

                // draw layered sprite first
                exLayeredSprite layeredSprite = trans.GetComponent<exLayeredSprite>();
                if ( layeredSprite ) {
                    exEditorUtility.GL_DrawWireFrame (layeredSprite, Color.white, true);
                }

                // draw clipping
                exClipping clipping = trans.GetComponent<exClipping>();
                if ( clipping ) {
                    exEditorUtility.GL_DrawWireFrame (clipping, new Color( 0.0f, 1.0f, 0.5f, 1.0f ), true);
                }

                // draw ui-control
                exUIControl[] controls = trans.GetComponents<exUIControl>();
                for ( int j = 0; j < controls.Length; ++j ) {
                    exUIControl control = controls[j];
                    DrawControlNode (control);
                }
            }

            // draw resolution line
            DrawResolutionRect ( secondResolutionIdx, Color.red );
            DrawResolutionRect ( firstResolutionIdx, Color.yellow );

            // Show a copy icon on the drag
            if ( DragAndDrop.visualMode == DragAndDropVisualMode.Copy ) {
                foreach ( Object o in draggingObjects ) {
                    if ( o is exTextureInfo ) {
                        exEditorUtility.GL_DrawTextureInfo ( o as exTextureInfo, 
                                                             SceneField_MapToWorld( _rect, Event.current.mousePosition),
                                                             new Color(1.0f, 1.0f, 1.0f, 0.5f) );
                    }
                    else if ( o is exBitmapFont ) {
                        // TODO:
                    }
                    else if ( o is exSpriteAnimationClip ) {
                        exSpriteAnimationClip clip = o as exSpriteAnimationClip;
                        if ( clip.frameInfos.Count > 0 ) {
                            exEditorUtility.GL_DrawTextureInfo ( clip.frameInfos[0].textureInfo, 
                                                                 SceneField_MapToWorld( _rect, Event.current.mousePosition),
                                                                 new Color(1.0f, 1.0f, 1.0f, 0.5f) );
                        } 
                    }
                }
            }
        GL.PopMatrix();

        // pop viewport
        GL.Viewport(oldViewport);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawNode ( exLayeredSprite _node ) {
        Material material = _node.material;
        material.SetPass(0);

        exList<Vector3> vertices = exList<Vector3>.GetTempList();
        exList<Vector2> uvs = exList<Vector2>.GetTempList();
        exList<int> indices = exList<int>.GetTempList();
        exList<Color32> colors = exList<Color32>.GetTempList();
        _node.GetBuffers(vertices, uvs, colors, indices);
        exDebug.Assert(uvs.Count == vertices.Count);

        //GL.PushMatrix();
        //GL.MultMatrix( _node.transform.localToWorldMatrix );
        GL.Begin(GL.TRIANGLES);
        for (int i = 0; i < indices.Count; ++i) {
            int vertexIndex = indices.buffer[i];
            GL.Color ( colors.buffer[vertexIndex] );
            GL.TexCoord2 ( uvs.buffer[vertexIndex].x, uvs.buffer[vertexIndex].y );
            GL.Vertex ( vertices.buffer[vertexIndex] );
        }
        GL.End();
        //GL.PopMatrix();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawControlNode ( exUIControl _ctrl ) {
        Vector3[] vertices = _ctrl.GetLocalVertices();
        if (vertices.Length > 0) {
            Rect aabb = exGeometryUtility.GetAABoundingRect(vertices);
            Matrix4x4 l2w = _ctrl.transform.localToWorldMatrix;

            // draw control rect
            vertices = new Vector3[4] {
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMin, 0)),
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMax, 0)),
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMax, 0)),
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMin, 0)),
            };
            exEditorUtility.GL_DrawRectLine(vertices, new Color( 1.0f, 0.0f, 0.5f, 1.0f ), true);

            // draw scroll-view content
            exUIScrollView scrollView = _ctrl as exUIScrollView;
            if ( scrollView != null ) {
                aabb.width = scrollView.contentSize.x;
                aabb.yMin = aabb.yMax - scrollView.contentSize.y;
                aabb.center += scrollView.GetScrollOffset();
                vertices = new Vector3[4] {
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMin, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMax, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMax, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMin, 0)),
                };
                exEditorUtility.GL_DrawRectLine(vertices, new Color( 0.0f, 0.5f, 1.0f, 1.0f ), true);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawAABoundingRect ( exLayeredSprite _node ) {
        Rect boundingRect = _node.GetWorldAABoundingRect();

        exEditorUtility.GL_DrawRectLine ( new Vector3[] {
                                          new Vector3 ( boundingRect.xMin, boundingRect.yMin, 0.0f ),
                                          new Vector3 ( boundingRect.xMin, boundingRect.yMax, 0.0f ),
                                          new Vector3 ( boundingRect.xMax, boundingRect.yMax, 0.0f ),
                                          new Vector3 ( boundingRect.xMax, boundingRect.yMin, 0.0f ),
                                          }, Color.white );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawResolutionRect ( int _idx, Color _color ) {
        if ( _idx <= 0 || _idx >= exEditorUtility.resolutionList.Length -1 )
            return;

        Vector2 size = exEditorUtility.resolutionList[_idx];
        float half_w = size.x * 0.5f;
        float half_h = size.y * 0.5f;
        exEditorUtility.GL_DrawRectLine ( new Vector3[] {
                                          new Vector3 ( -half_w, -half_h, 0.0f ),
                                          new Vector3 ( -half_w,  half_h, 0.0f ),
                                          new Vector3 (  half_w,  half_h, 0.0f ),
                                          new Vector3 (  half_w, -half_h, 0.0f ),
                                          }, _color );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DebugInfos () {
        EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );
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

            GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessSceneEditorHandles () {

        editCamera.enabled = true;
        editCamera.aspect = sceneViewRect.width/sceneViewRect.height;
        editCamera.orthographicSize = (sceneViewRect.height * 0.5f) / scale;

        //
        GUI.BeginGroup( sceneViewRect );
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
            exPlane resizePlane = null;
            exLayeredSprite layeredSprite = trans.GetComponent<exLayeredSprite>();
            if ( layeredSprite != null ) {
                resizePlane = layeredSprite.customSize ? layeredSprite : null;
            }
            else {
                resizePlane = trans.GetComponent<exPlane>();
            }

            if ( resizePlane != null ) {
                // TODO: limit the size { 
                // float minWidth = float.MinValue;
                // float minHeight = float.MinValue;
                //     exSprite sp = resizePlane as exSprite;
                //     if ( sp != null && sp.spriteType == exSpriteType.Sliced ) {
                //         minWidth = sp.textureInfo.borderLeft + sp.textureInfo.borderRight;
                //         minHeight = sp.textureInfo.borderTop + sp.textureInfo.borderBottom;
                //     }
                // } TODO end 

                Vector3[] vertices = resizePlane.GetLocalVertices();
                Rect aabb = exGeometryUtility.GetAABoundingRect(vertices);
                Vector3 center = aabb.center; // NOTE: this value will become world center after Handles.Slider(s)
                Vector3 size = new Vector3( aabb.width, aabb.height, 0.0f );

                Vector3 tl = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f,
                                                                  center.y + size.y * 0.5f,
                                                                  0.0f ) );
                Vector3 tc = trans.TransformPoint ( new Vector3 ( center.x,
                                                                  center.y + size.y * 0.5f,
                                                                  0.0f ) );
                Vector3 tr = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f,
                                                                  center.y + size.y * 0.5f,
                                                                  0.0f ) );
                Vector3 ml = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f,
                                                                  center.y,
                                                                  0.0f ) );
                Vector3 mr = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f,
                                                                  center.y,
                                                                  0.0f ) );
                Vector3 bl = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f,
                                                                  center.y - size.y * 0.5f,
                                                                  0.0f ) );
                Vector3 bc = trans.TransformPoint ( new Vector3 ( center.x,
                                                                  center.y - size.y * 0.5f,
                                                                  0.0f ) );
                Vector3 br = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f,
                                                                  center.y - size.y * 0.5f,
                                                                  0.0f ) );

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
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (ml2 + mr) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 mr2 = Handles.Slider ( mr, dir_right, handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = mr2 - mr;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (mr2 + ml) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 tc2 = Handles.Slider ( tc, dir_up,    handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tc2 - tc;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
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
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (bc2 + tc) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                    Vector3 tr2 = Handles.FreeMoveHandle ( tr, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tr2 - tr;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
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
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
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
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
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
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (bl2 + tr) * 0.5f;
                    changed = true;
                }

                if ( changed ) {
                    exSprite sprite = resizePlane as exSprite;
                    if (sprite != null) {
                        exSpriteBaseInspector.ApplySpriteScale(sprite, size, center);

                        // also update all planes in the same compnent
                        exPlane[] planes = sprite.GetComponents<exPlane>();
                        for ( int i = 0; i < planes.Length; ++i ) {
                            exPlane plane = planes[i];
                            if ( plane != this ) {
                                plane.width = sprite.width;
                                plane.height = sprite.height;
                                plane.anchor = sprite.anchor;
                                plane.offset = sprite.offset;
                            }

                            exClipping clipping = plane as exClipping;
                            if ( clipping != null ) {
                                clipping.CheckDirty();
                            }
                        }
                    }
                    else {
                        exPlaneInspector.ApplyPlaneScale(resizePlane, size, center);

                        exClipping clipping = resizePlane as exClipping;
                        if ( clipping != null ) {
                            clipping.CheckDirty();
                        }
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
                UnityEditor.Undo.RegisterUndo(Selection.transforms, "Change Transform");

                if ( Selection.transforms.Length == 1 ) {
                    trans.position = trans_position;
                    trans.rotation = trans_rotation;

                    exClipping[] clippings = trans.GetComponentsInChildren<exClipping>();
                    for ( int i = 0; i < clippings.Length; ++i ) {
                        clippings[i].SetDirty();
                        clippings[i].CheckDirty();
                    }
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

                        exClipping[] clippings = transObj.GetComponentsInChildren<exClipping>();
                        for ( int i = 0; i < clippings.Length; ++i ) {
                            clippings[i].SetDirty();
                            clippings[i].CheckDirty();
                        }
                    }

                    trans.position = trans_position;
                    trans.rotation = trans_rotation;
                }
            }

            if ( layeredSprite ) {
                layeredSprite.UpdateTransform ();
                if ( layeredSprite.updateFlags != exUpdateFlags.None ) {
                    ex2DRenderer.instance.ResortLayerDepth();
                    ex2DRenderer.instance.UpdateLayers();
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
            if ( ((e.command || e.control) &&
                 e.keyCode == KeyCode.Backspace) || e.keyCode == KeyCode.Delete ) 
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
            exLayeredSprite node = spriteNodes[i];
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

    Rect MapBoundingRect ( Rect _rect, exLayeredSprite _node ) {
        exLayeredSprite layeredSprite = _node as exLayeredSprite;
        Vector2 screenPos = Vector2.zero;

        if ( layeredSprite ) {
            Rect boundingRect = layeredSprite.GetWorldAABoundingRect();
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
        for ( int i = ex2DRenderer.instance.layerList.Count-1; i >= 0; --i ) {
            exLayer layer = ex2DRenderer.instance.layerList[i];
            if ( layer != null && layer.show ) {
                exLayeredSprite[] spriteList = layer.GetComponentsInChildren<exLayeredSprite>();
                System.Array.Sort<exLayeredSprite>(spriteList);
                foreach ( exLayeredSprite node in spriteList ) {
                    if ( node.visible && node.layer != null ) {
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
