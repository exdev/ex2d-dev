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

    class HierarchyStyles {
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

        public HierarchyStyles() {
            // NOTE: if we don't new GUIStyle, it will reference the original style. 
            boxBackground = new GUIStyle(boxBackground);
            boxBackground.margin = new RectOffset( 0, 0, 0, 0 );
            boxBackground.padding = new RectOffset( 0, 0, 0, 0 );

            elementBackground = new GUIStyle(elementBackground);
            elementBackground.overflow = new RectOffset(0, 0, 1, 0);

            elementSelectionRect = new GUIStyle(elementSelectionRect);
            elementSelectionRect.overflow = new RectOffset(0, 0, 1, -1);

            boldLabel = new GUIStyle(boldLabel);
            boldLabel.fontSize = 20;
            boldLabel.fontStyle = FontStyle.Bold;
            boldLabel.normal.textColor = EditorStyles.boldLabel.normal.textColor;
        }
    }

	static int sceneViewFieldHash = "SceneViewField".GetHashCode();
	static int elementsFieldHash = "ElementsField".GetHashCode();
    static HierarchyStyles hierarchyStyles = null;

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exUILayoutInfo curEdit = null;
    SerializedObject curSerializedObject = null;

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
    exRectSelection<Object> rectSelection = null;

    Vector2 hierarchyScrollPos = Vector2.zero;
    exUIElement activeElement = null;

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

        rectSelection = new exRectSelection<Object>( PickObject,
                                                     PickRectObjects,
                                                     ConfirmRectSelection );

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

        //
        if ( curSerializedObject == null || 
             curSerializedObject.targetObject == null ||
             curSerializedObject.targetObject != curEdit )
        {
            curSerializedObject = new SerializedObject(curEdit);
        }

        // if hierarchyStyles is null
        if ( hierarchyStyles == null ) {
            hierarchyStyles = new HierarchyStyles();
        }

        int margin = 20;
        int width = 300;
        float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );

        // hierarchy & scene
        EditorGUILayout.BeginHorizontal();

            // hierarchy
            EditorGUILayout.BeginVertical();
                // toolbar
                Hierarchy_Toolbar ();

                // hierarchy elements
                Hierarchy ( width, (int)(position.height - toolbarHeight ) );

            EditorGUILayout.EndVertical();

            // scene filed
            EditorGUILayout.BeginVertical();
                // toolbar
                SceneView_Toolbar ();

                GUILayout.Space(margin);

                // view
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                Layout_SceneViewField ( Mathf.FloorToInt(position.width - width - margin - 5),
                                        Mathf.FloorToInt(position.height - toolbarHeight - margin - margin) );
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        // TODO { 
        //
        // ProcessSceneEditorHandles();

        //
        // ProcessSceneEditorEvents();
        // } TODO end 

        rectSelection.SetSelection( Selection.objects );
        rectSelection.OnGUI();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

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
        // TODO { 
        // for ( int i = spriteNodes.Count-1; i >= 0; --i ) {
        //     exLayeredSprite node = spriteNodes[i];
        //     Rect boundingRect = MapBoundingRect ( sceneViewRect, node );
        //     if ( exGeometryUtility.RectRect_Contains( _rect, boundingRect ) != 0 ||
        //          exGeometryUtility.RectRect_Intersect( _rect, boundingRect ) )
        //     {
        //         objects.Add(node.gameObject);
        //     }
        // }
        // } TODO end 

        return objects.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ConfirmRectSelection ( Object _activeObj, Object[] _selectedObjs ) {
        // TODO { 
        // Selection.activeObject = _activeObj;
        // Selection.objects = _selectedObjs;
        // } TODO end 
    }

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
        activeElement = null;
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

    void SceneView_Toolbar () {
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

    void Hierarchy_Toolbar () {
        // add layer button
        EditorGUILayout.BeginHorizontal( hierarchyStyles.toolbar, new GUILayoutOption[0]);
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button( "UP", hierarchyStyles.toolbarButton ) ) {
                // int curIdx = ex2DRenderer.instance.layerList.IndexOf(activeLayer);
                // if ( curIdx != -1 ) {
                //     int nextIdx = System.Math.Max(curIdx-1,0);
                //     layerListProp.MoveArrayElement ( curIdx, nextIdx );
                //     // activeLayer = ex2DRenderer.instance.layerList[nextIdx];
                // }
            }
            if ( GUILayout.Button( "DOWN", hierarchyStyles.toolbarButton ) ) {
                // int curIdx = ex2DRenderer.instance.layerList.IndexOf(activeLayer);
                // if ( curIdx != -1 ) {
                //     int nextIdx = System.Math.Min(curIdx+1,ex2DRenderer.instance.layerList.Count-1);
                //     layerListProp.MoveArrayElement ( curIdx, nextIdx );
                //     // activeLayer = ex2DRenderer.instance.layerList[nextIdx];
                // }
            }
            if ( GUILayout.Button( hierarchyStyles.iconToolbarPlus, 
                                   hierarchyStyles.toolbarDropDown ) ) 
            {
                // ex2DRenderer.instance.CreateLayer();
            }
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Hierarchy ( int _width, int _height ) {
        EditorGUILayout.BeginVertical ( new GUILayoutOption [] {
                                        GUILayout.Width(_width), 
                                        GUILayout.MinWidth(_width), 
                                        GUILayout.MaxWidth(_width),
                                        GUILayout.ExpandWidth(false),
                                        } );

            EditorGUILayout.LabelField ( "Hierarchy", hierarchyStyles.boldLabel );
            EditorGUILayout.Space ();
            EditorGUILayout.Space ();

            hierarchyScrollPos = EditorGUILayout.BeginScrollView ( hierarchyScrollPos, 
                                                                   new GUILayoutOption [] {
                                                                   GUILayout.Width(_width), 
                                                                   GUILayout.MinWidth(_width), 
                                                                   GUILayout.MaxWidth(_width),
                                                                   GUILayout.ExpandWidth(false),
                                                                   } );


                // TODO:
                int controlID = GUIUtility.GetControlID(elementsFieldHash, FocusType.Passive);
                ElementField ( controlID, 10.0f, 0.0f, 0, curEdit.root );


                // event process for layers
                Event e = Event.current;
                switch ( e.GetTypeForControl(controlID) ) {
                case EventType.MouseUp:
                    if ( GUIUtility.hotControl == controlID ) {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
                }

            EditorGUILayout.EndScrollView();

        EditorGUILayout.EndVertical();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    bool ElementField ( int _controlID, float _x, float _y, int _indentLevel, exUIElement _el ) {
        Event e = Event.current;

        float height = 25.0f;
        Vector2 size = Vector2.zero;
        float cur_x = _x + _indentLevel * 15.0f + 5.0f;
        float cur_y = _y;
        Rect rect = new Rect ( _x, _y, 290.0f, height );
        bool deleted = false;

        // background
        Rect draggingHandleRect = new Rect(cur_x, cur_y + 10f, 10f, rect.height);
        if ( Event.current.type == EventType.Repaint ) {
            // draw background
            if ( activeElement == _el ) {
                hierarchyStyles.elementSelectionRect.Draw(rect, false, false, false, false);
            }
            else {
                hierarchyStyles.elementBackground.Draw(rect, false, false, false, false);
            }

            hierarchyStyles.draggingHandle.Draw( draggingHandleRect, false, false, false, false );
            EditorGUIUtility.AddCursorRect ( draggingHandleRect, MouseCursor.Pan );
        }

        // name
        cur_x += 10.0f;
        cur_x += 5.0f;
        EditorGUI.BeginChangeCheck ();
            _el.name = EditorGUI.TextField ( new Rect ( cur_x, cur_y + 4f, 100.0f, height - 8f ),
                                             _el.name ); 
        if ( EditorGUI.EndChangeCheck () ) {
            EditorUtility.SetDirty ( curEdit );
        }

        // #
        cur_x += 100.0f;
        cur_x += 5.0f;
        EditorGUI.LabelField ( new Rect ( cur_x, cur_y + 4f, 10.0f, height - 8f ), "#" );

        // id
        cur_x += 10.0f;
        cur_x += 2.0f;
        EditorGUI.BeginChangeCheck ();
            _el.id = EditorGUI.TextField ( new Rect ( cur_x, cur_y + 4f, 80.0f, height - 8f ),
                                             _el.id ); 
        if ( EditorGUI.EndChangeCheck () ) {
            EditorUtility.SetDirty ( curEdit );
        }

        // delete
        size = hierarchyStyles.removeButton.CalcSize( new GUIContent(hierarchyStyles.iconToolbarMinus) );
        cur_x = rect.xMax - 5.0f - size.x;
        if ( GUI.Button( new Rect( cur_x, rect.y + 2f, size.x, size.y ), 
                         hierarchyStyles.iconToolbarMinus, 
                         hierarchyStyles.removeButton) )
        {
            if ( _indentLevel == 0 ) {
                EditorUtility.DisplayDialog ( "Can not delete root element?", 
                                              string.Format("You can not delete root element"),
                                              "OK" );
            }
            else {
                if ( EditorUtility.DisplayDialog ( "Delete Element?", 
                                                   string.Format("Are you sure you want to delete element: {0}?", _el.name),
                                                   "Yes",
                                                   "No" ) )
                {
                    deleted = true;
                }
            }
        }


        // event process for _layer
        switch ( e.GetTypeForControl(_controlID) ) {
        case EventType.MouseDown:
            if ( e.button == 0 && e.clickCount == 1 && rect.Contains(e.mousePosition) ) {
                GUIUtility.hotControl = _controlID;
                GUIUtility.keyboardControl = _controlID;
                activeElement = _el;

                if ( draggingHandleRect.Contains(e.mousePosition) ) {
                    // draggingLayer = _layer; TODO
                }

                e.Use();
            }
            break;
        }

        if ( deleted )
            return true;

        // children
        cur_y += height;
        for ( int i = 0; i < _el.children.Count; ++i ) {
            bool isDeleted = ElementField ( _controlID, _x, cur_y, _indentLevel + 1, _el.children[i] );
            if ( isDeleted ) {
                _el.children.RemoveAt(i);
                --i;
                EditorUtility.SetDirty ( curEdit );
            }
            cur_y += height;
        }

        return false;
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
