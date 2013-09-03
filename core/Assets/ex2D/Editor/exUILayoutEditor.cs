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
    Vector2 styleScrollPos = Vector2.zero;
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

        //
        if ( activeElement == null || curEdit.root.Exists(activeElement) == false ) {
            activeElement = curEdit.root;
        }

        int margin = 20;
        int hierarchy_width = 300;
        int style_width = 250;
        float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );

        // hierarchy & scene
        EditorGUILayout.BeginHorizontal();

            // hierarchy
            EditorGUILayout.BeginVertical();
                // toolbar
                Hierarchy_Toolbar ();

                // hierarchy elements
                Hierarchy ( hierarchy_width, (int)(position.height - toolbarHeight) );

            EditorGUILayout.EndVertical();

            // scene filed
            EditorGUILayout.BeginVertical();
                // toolbar
                SceneView_Toolbar ();

                GUILayout.Space(margin);

                // view
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(5);
                Layout_SceneViewField ( Mathf.FloorToInt(position.width - hierarchy_width - style_width - margin - 5),
                                        Mathf.FloorToInt(position.height - toolbarHeight - margin - margin) );
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            // style settings
            EditorGUILayout.BeginVertical();
                // toolbar
                Style_Toolbar ();

                // hierarchy elements
                Style ( style_width, (int)(position.height - toolbarHeight) );

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
        if ( curEdit != null )
            activeElement = curEdit.root;
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

            EditorGUI.BeginChangeCheck ();
            // resolution
            curEdit.resolutionIdx = EditorGUILayout.Popup ( "",
                                                            curEdit.resolutionIdx, 
                                                            exEditorUtility.resolutionDescList,
                                                            hierarchyStyles.toolbarDropDown,
                                                            new GUILayoutOption [] {
                                                            GUILayout.Width(150), 
                                                            } );
            if ( curEdit.resolutionIdx != exEditorUtility.resolutionList.Length-1 ) {
                Vector2 size = exEditorUtility.resolutionList[curEdit.resolutionIdx];
                curEdit.width = (int)size.x;
                curEdit.height = (int)size.y;
            }
            EditorGUILayout.Space();

            // if customSize
            GUI.enabled = (curEdit.resolutionIdx == exEditorUtility.resolutionList.Length-1);
                curEdit.width = EditorGUILayout.IntField( "", curEdit.width, EditorStyles.toolbarTextField,
                                                          new GUILayoutOption [] {
                                                          GUILayout.Width(40)
                                                          } ); 
                GUILayout.Label( "x" );
                curEdit.height = EditorGUILayout.IntField( "", curEdit.height, EditorStyles.toolbarTextField,
                                                           new GUILayoutOption [] {
                                                           GUILayout.Width(40)
                                                           } ); 
            GUI.enabled = true;
            if ( EditorGUI.EndChangeCheck () ) {
                EditorUtility.SetDirty(curEdit);
            }

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
                if ( activeElement != curEdit.root && activeElement.parent != null ) {
                    int curIdx = activeElement.parent.GetElementIndex(activeElement);
                    int nextIdx = System.Math.Max(curIdx-1,0);
                    activeElement.parent.InsertAt ( nextIdx, activeElement );

                    EditorUtility.SetDirty(curEdit);
                }
            }
            if ( GUILayout.Button( "DOWN", hierarchyStyles.toolbarButton ) ) {
                if ( activeElement != curEdit.root && activeElement.parent != null ) {
                    int curIdx = activeElement.parent.GetElementIndex(activeElement);
                    int nextIdx = System.Math.Min(curIdx+1, activeElement.parent.children.Count-1);
                    activeElement.parent.InsertAt ( nextIdx, activeElement );

                    EditorUtility.SetDirty(curEdit);
                }
            }
            if ( GUILayout.Button( hierarchyStyles.iconToolbarPlus, 
                                   hierarchyStyles.toolbarDropDown ) ) 
            {
                exUIElement newEL = new exUIElement(); 
                newEL.parent = activeElement;

                EditorUtility.SetDirty(curEdit);
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
                bool isDeleted = false;
                ElementField ( controlID, 10.0f, 0.0f, 0, curEdit.root, ref isDeleted );


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

    float ElementField ( int _controlID, float _x, float _y, int _indentLevel, exUIElement _el, ref bool _deleted ) {
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
                // DISABLE { 
                // if ( EditorUtility.DisplayDialog ( "Delete Element?", 
                //                                    string.Format("Are you sure you want to delete element: {0}?", _el.name),
                //                                    "Yes",
                //                                    "No" ) )
                // {
                //     deleted = true;
                // }
                // } DISABLE end 
                deleted = true;
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

        if ( deleted ) {
            _deleted = true;
            return 0.0f;
        }

        // children
        cur_y += height;
        for ( int i = 0; i < _el.children.Count; ++i ) {
            bool isDeleted = false;
            float totalHeight = ElementField ( _controlID, _x, cur_y, _indentLevel + 1, _el.children[i], ref isDeleted );
            if ( isDeleted ) {
                _el.children.RemoveAt(i);
                --i;
                EditorUtility.SetDirty ( curEdit );
            }
            cur_y += totalHeight;
        }

        return (cur_y - _y);
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

    void Style_Toolbar () {
        // add layer button
        EditorGUILayout.BeginHorizontal( hierarchyStyles.toolbar, new GUILayoutOption[0]);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Style ( int _width, int _height ) {
        EditorGUILayout.BeginVertical ( new GUILayoutOption [] {
                                        GUILayout.Width(_width), 
                                        GUILayout.MinWidth(_width), 
                                        GUILayout.MaxWidth(_width),
                                        GUILayout.ExpandWidth(false),
                                        } );

            EditorGUILayout.LabelField ( "Style", hierarchyStyles.boldLabel );
            EditorGUILayout.Space ();
            EditorGUILayout.Space ();

            if ( activeElement != null ) {
                styleScrollPos = EditorGUILayout.BeginScrollView ( styleScrollPos, 
                                                                   new GUILayoutOption [] {
                                                                   GUILayout.Width(_width), 
                                                                   GUILayout.MinWidth(_width), 
                                                                   GUILayout.MaxWidth(_width),
                                                                   GUILayout.ExpandWidth(false),
                                                                   } );


                    EditorGUI.BeginChangeCheck();

                    exUIStyle style = activeElement.style;
                    int indentLevel = 0;

                    // size
                    GUILayout.Label ( "size", new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                    ++indentLevel;
                        exCSSUI.SizeField ( indentLevel, activeElement, "width", style.width, false );
                        exCSSUI.SizeField ( indentLevel, activeElement, "height", style.height, false );
                        exCSSUI.MinSizeField ( indentLevel, activeElement, "minWidth", style.minWidth, false );
                        exCSSUI.MinSizeField ( indentLevel, activeElement, "minHeight", style.minHeight, false );
                        exCSSUI.MaxSizeField ( indentLevel, activeElement, "maxWidth", style.maxWidth, false );
                        exCSSUI.MaxSizeField ( indentLevel, activeElement, "maxHeight", style.maxHeight, false );
                    --indentLevel;

                    EditorGUILayout.Space();

                    // position
                    GUILayout.Label ( "position", new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                    ++indentLevel;
                        exCSSUI.PositionField ( indentLevel, activeElement, "position", ref style.position );
                        exCSSUI.SizeField ( indentLevel, activeElement, "top", style.top, false );
                        exCSSUI.SizeField ( indentLevel, activeElement, "right", style.right, false );
                        exCSSUI.SizeField ( indentLevel, activeElement, "bottom", style.bottom, false );
                        exCSSUI.SizeField ( indentLevel, activeElement, "left", style.left, false );
                    --indentLevel;

                    EditorGUILayout.Space();

                    // margin
                    GUILayout.Label ( "margin", new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                    ++indentLevel;
                        exCSSUI.SizeField ( indentLevel, activeElement, "top", style.marginTop, false );
                        exCSSUI.LockableSizeField ( indentLevel, activeElement, "right", style.marginRight, false, ref style.lockMarginRight );
                        exCSSUI.LockableSizeField ( indentLevel, activeElement, "bottom", style.marginBottom, false, ref style.lockMarginBottom );
                        exCSSUI.LockableSizeField ( indentLevel, activeElement, "left", style.marginLeft, false, ref style.lockMarginLeft );
                        if ( style.lockMarginRight ) {
                            style.marginRight.type = style.marginTop.type;
                            style.marginRight.val = style.marginTop.val;
                            style.marginBottom.type = style.marginTop.type;
                            style.marginBottom.val = style.marginTop.val;
                            style.marginLeft.type = style.marginRight.type;
                            style.marginLeft.val = style.marginRight.val;

                            style.lockMarginRight = true;
                            style.lockMarginBottom = true;
                            style.lockMarginLeft = true;
                        }
                        else if ( style.lockMarginBottom ) {
                            style.marginBottom.type = style.marginTop.type;
                            style.marginBottom.val = style.marginTop.val;
                            style.marginLeft.type = style.marginRight.type;
                            style.marginLeft.val = style.marginRight.val;

                            style.lockMarginBottom = true;
                            style.lockMarginLeft = true;
                        }
                        else if ( style.lockMarginLeft ) {
                            style.marginLeft.type = style.marginRight.type;
                            style.marginLeft.val = style.marginRight.val;

                            style.lockMarginLeft = true;
                        }
                    --indentLevel;

                    EditorGUILayout.Space();

                    // padding
                    GUILayout.Label ( "padding", new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                    ++indentLevel;
                        exCSSUI.SizeNoAutoField ( indentLevel, activeElement, "top", style.paddingTop, false );
                        exCSSUI.LockableSizeNoAutoField ( indentLevel, activeElement, "right", style.paddingRight, false, ref style.lockPaddingRight );
                        exCSSUI.LockableSizeNoAutoField ( indentLevel, activeElement, "bottom", style.paddingBottom, false, ref style.lockPaddingBottom );
                        exCSSUI.LockableSizeNoAutoField ( indentLevel, activeElement, "left", style.paddingLeft, false, ref style.lockPaddingLeft );
                        if ( style.lockPaddingRight ) {
                            style.paddingRight.type = style.paddingTop.type;
                            style.paddingRight.val = style.paddingTop.val;
                            style.paddingBottom.type = style.paddingTop.type;
                            style.paddingBottom.val = style.paddingTop.val;
                            style.paddingLeft.type = style.paddingRight.type;
                            style.paddingLeft.val = style.paddingRight.val;

                            style.lockPaddingRight = true;
                            style.lockPaddingBottom = true;
                            style.lockPaddingLeft = true;
                        }
                        else if ( style.lockPaddingBottom ) {
                            style.paddingBottom.type = style.paddingTop.type;
                            style.paddingBottom.val = style.paddingTop.val;
                            style.paddingLeft.type = style.paddingRight.type;
                            style.paddingLeft.val = style.paddingRight.val;

                            style.lockPaddingBottom = true;
                            style.lockPaddingLeft = true;
                        }
                        else if ( style.lockPaddingLeft ) {
                            style.paddingLeft.type = style.paddingRight.type;
                            style.paddingLeft.val = style.paddingRight.val;

                            style.lockPaddingLeft = true;
                        }
                    --indentLevel;

                    EditorGUILayout.Space();

                    // border
                    GUILayout.Label ( "border", new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                    ++indentLevel;
                        exCSSUI.ImageField ( indentLevel, activeElement, "src", style.borderSrc, false );
                        exTextureInfo borderTextureInfo = style.borderSrc.val as exTextureInfo;
                        if ( borderTextureInfo && borderTextureInfo.hasBorder ) {
                            style.borderSizeTop.type    = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeTop.val     = borderTextureInfo.borderTop;
                            style.borderSizeRight.type  = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeRight.val   = borderTextureInfo.borderRight;
                            style.borderSizeBottom.type = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeBottom.val  = borderTextureInfo.borderBottom;
                            style.borderSizeLeft.type   = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeLeft.val    = borderTextureInfo.borderRight;
                        }

                        exCSSUI.ColorField ( indentLevel, activeElement, "color", style.borderColor, false );
                        exCSSUI.SizeLengthOnlyField ( indentLevel, activeElement, "top", style.borderSizeTop, false );
                        exCSSUI.LockableSizeLengthOnlyField ( indentLevel, activeElement, "right", style.borderSizeRight, false, ref style.lockBorderSizeRight );
                        exCSSUI.LockableSizeLengthOnlyField ( indentLevel, activeElement, "bottom", style.borderSizeBottom, false, ref style.lockBorderSizeBottom );
                        exCSSUI.LockableSizeLengthOnlyField ( indentLevel, activeElement, "left", style.borderSizeLeft, false, ref style.lockBorderSizeLeft );
                        if ( style.lockBorderSizeRight ) {
                            style.borderSizeRight.type = style.borderSizeTop.type;
                            style.borderSizeRight.val = style.borderSizeTop.val;
                            style.borderSizeBottom.type = style.borderSizeTop.type;
                            style.borderSizeBottom.val = style.borderSizeTop.val;
                            style.borderSizeLeft.type = style.borderSizeRight.type;
                            style.borderSizeLeft.val = style.borderSizeRight.val;

                            style.lockBorderSizeRight = true;
                            style.lockBorderSizeBottom = true;
                            style.lockBorderSizeLeft = true;
                        }
                        else if ( style.lockBorderSizeBottom ) {
                            style.borderSizeBottom.type = style.borderSizeTop.type;
                            style.borderSizeBottom.val = style.borderSizeTop.val;
                            style.borderSizeLeft.type = style.borderSizeRight.type;
                            style.borderSizeLeft.val = style.borderSizeRight.val;

                            style.lockBorderSizeBottom = true;
                            style.lockBorderSizeLeft = true;
                        }
                        else if ( style.lockBorderSizeLeft ) {
                            style.borderSizeLeft.type = style.borderSizeRight.type;
                            style.borderSizeLeft.val = style.borderSizeRight.val;

                            style.lockBorderSizeLeft = true;
                        }
                    --indentLevel;


                    if ( EditorGUI.EndChangeCheck() ) {
                        // apply layout
                        curEdit.Apply();
                        EditorUtility.SetDirty (curEdit);
                    }

                EditorGUILayout.EndScrollView();
            }

        EditorGUILayout.EndVertical();
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
                                editCamera.transform.position.y + (_rect.height * 0.5f) / scale,
                                editCamera.transform.position.y - (_rect.height * 0.5f) / scale );


            // draw resolution border
            int width = (curEdit.width == -1) ? int.MaxValue : curEdit.width; 
            int height = (curEdit.height == -1) ? int.MaxValue : curEdit.height; 
            exEditorUtility.GL_DrawRectLine( new Vector3[] {
                                                new Vector3( 0.0f, 0.0f, 0.0f ),
                                                new Vector3( width, 0.0f, 0.0f ),
                                                new Vector3( width, height, 0.0f ),
                                                new Vector3( 0.0f, height, 0.0f ),
                                             }, Color.yellow );

            // draw layout
            DrawElement( curEdit.root );

        GL.PopMatrix();

        GL.Viewport(oldViewport);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawElement ( exUIElement _el ) {
        exEditorUtility.GL_DrawRectLine( new Vector3[] {
                                         new Vector3( _el.x, _el.y, 0.0f ),
                                         new Vector3( _el.x + _el.width, _el.y, 0.0f ),
                                         new Vector3( _el.x + _el.width, _el.y + _el.height, 0.0f ),
                                         new Vector3( _el.x, _el.y + _el.height, 0.0f ),
                                         }, Color.white );
    }
}
