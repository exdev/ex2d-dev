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
    Vector2 contentScrollPos = Vector2.zero;

    exUIElement activeElement = null;
    exUIElement hoverElement = null;
    exUIElement dropElement = null;
    bool debugElement = false;

    List<exUIElement> selectedElements = new List<exUIElement>();
    bool draggingElements = false;

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

        selectedElements.Clear();
        draggingElements = false;
        dropElement = null;

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
        int hierarchy_width = 250;
        int style_width = 300;
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

            // style settings
            EditorGUILayout.BeginVertical();
                // toolbar
                Style_Toolbar ();

                // hierarchy elements
                Style ( style_width, (int)(position.height - toolbarHeight) );

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
        EditorGUILayout.EndHorizontal();

        // TODO { 
        // ProcessSceneEditorHandles();
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
        debugElement = false;
        hoverElement = null;
        activeElement = null;
        dropElement = null;

        draggingElements = false;
        selectedElements.Clear();

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
                curEdit.Apply();
                EditorUtility.SetDirty(curEdit);
                Repaint();
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
            debugElement = GUILayout.Toggle ( debugElement, "Debug", hierarchyStyles.toolbarButton );
            if ( GUILayout.Button( "Clone", hierarchyStyles.toolbarButton ) ) {
                if ( activeElement != null && activeElement != curEdit.root ) {
                    exUIElement newEL = activeElement.Clone();
                    activeElement.parent.AddElement(newEL);

                    curEdit.Apply();
                    EditorUtility.SetDirty(curEdit);
                    Repaint();
                }
            }

            GUILayout.FlexibleSpace();

            if ( GUILayout.Button( "UP", hierarchyStyles.toolbarButton ) ) {
                if ( activeElement != curEdit.root && activeElement.parent != null ) {
                    int curIdx = activeElement.parent.GetElementIndex(activeElement);
                    int nextIdx = System.Math.Max(curIdx-1,0);
                    activeElement.parent.InsertAt ( nextIdx, activeElement );

                    curEdit.Apply();
                    EditorUtility.SetDirty(curEdit);
                    Repaint();
                }
            }
            if ( GUILayout.Button( "DOWN", hierarchyStyles.toolbarButton ) ) {
                if ( activeElement != curEdit.root && activeElement.parent != null ) {
                    int curIdx = activeElement.parent.GetElementIndex(activeElement);
                    int nextIdx = System.Math.Min(curIdx+1, activeElement.parent.children.Count-1);
                    activeElement.parent.InsertAt ( nextIdx, activeElement );

                    curEdit.Apply();
                    EditorUtility.SetDirty(curEdit);
                    Repaint();
                }
            }
            if ( GUILayout.Button( hierarchyStyles.iconToolbarPlus, 
                                   hierarchyStyles.toolbarDropDown ) ) 
            {
                exUIElement newEL = new exUIElement(); 
                activeElement.AddElement(newEL);

                curEdit.Apply();
                EditorUtility.SetDirty(curEdit);
                Repaint();
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
                                                                   GUILayout.Width(_width + 20), 
                                                                   GUILayout.MinWidth(_width + 20), 
                                                                   GUILayout.MaxWidth(_width + 20),
                                                                   GUILayout.ExpandWidth(false),
                                                                   } );


                // TODO:
                int controlID = GUIUtility.GetControlID(elementsFieldHash, FocusType.Passive);
                bool isDeleted = false;
                float totalHeight = ElementField ( controlID, 10.0f, 0.0f, _width, 0, curEdit.root, ref isDeleted );
                totalHeight = Mathf.Min( _height, totalHeight );
                GUILayoutUtility.GetRect ( _width, totalHeight );


                // event process for layers
                Event e = Event.current;
                switch ( e.GetTypeForControl(controlID) ) {
                case EventType.MouseMove:
                    if ( new Rect( 0.0f, 0.0f, _width, totalHeight).Contains(e.mousePosition) == false ) {
                        hoverElement = null;
                        Repaint();
                    }
                    break;

                case EventType.MouseUp:
                    if ( GUIUtility.hotControl == controlID ) {
                        GUIUtility.hotControl = 0;
                        e.Use();
                    }
                    break;
                }

            EditorGUILayout.EndScrollView();

            // content
            GUILayout.FlexibleSpace();
            GUILayout.Space(20);

            EditorGUILayout.BeginHorizontal(new GUILayoutOption [] {
                                            GUILayout.Width(_width), 
                                            GUILayout.MinWidth(_width), 
                                            GUILayout.MaxWidth(_width),
                                            GUILayout.ExpandWidth(false),
                                            });
                GUILayout.Space(10);
                EditorGUI.BeginChangeCheck();
                    EditorGUILayout.LabelField ( "Content: ", new GUILayoutOption [] { GUILayout.Width(80) } );
                    GUILayout.FlexibleSpace();
                    activeElement.contentType = (exUIElement.ContentType)EditorGUILayout.EnumPopup ( activeElement.contentType, new GUILayoutOption [] { GUILayout.Width(80) } );
                if ( EditorGUI.EndChangeCheck() ) {
                    curEdit.Apply();
                    EditorUtility.SetDirty(curEdit);
                    Repaint();
                }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if ( activeElement != null ) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                switch ( activeElement.contentType ) {
                case exUIElement.ContentType.Text:
                case exUIElement.ContentType.Markdown:
                    contentScrollPos = EditorGUILayout.BeginScrollView(contentScrollPos, 
                                                                       new GUILayoutOption [] {
                                                                       GUILayout.Height(200),
                                                                       GUILayout.MinHeight(200),
                                                                       GUILayout.MaxHeight(200),
                                                                       GUILayout.Width(_width),
                                                                       GUILayout.MinWidth(_width),
                                                                       GUILayout.MaxWidth(_width),
                                                                       GUILayout.ExpandHeight(false), 
                                                                       GUILayout.ExpandWidth(false), 
                                                                       }
                                                                      );        
                        EditorGUI.BeginChangeCheck();
                        activeElement.text = EditorGUILayout.TextArea( activeElement.text
                                                                       , new GUILayoutOption [] {
                                                                       GUILayout.Width(_width - 30),
                                                                       GUILayout.MinWidth(_width - 30),
                                                                       GUILayout.MaxWidth(_width - 30),
                                                                       GUILayout.ExpandHeight(true), 
                                                                       } 
                                                                     );        
                        if ( EditorGUI.EndChangeCheck() ) {
                            curEdit.Apply();
                            EditorUtility.SetDirty(curEdit);
                            Repaint();
                        }
                    EditorGUILayout.EndScrollView();
                    break;

                case exUIElement.ContentType.Texture2D:
                    EditorGUILayout.BeginVertical(new GUILayoutOption [] {
                                                  GUILayout.Height(200),
                                                  GUILayout.MinHeight(200),
                                                  GUILayout.MaxHeight(200),
                                                  GUILayout.Width(_width),
                                                  GUILayout.MinWidth(_width),
                                                  GUILayout.MaxWidth(_width),
                                                  GUILayout.ExpandHeight(false), 
                                                  GUILayout.ExpandWidth(false), 
                                                  });
                        EditorGUI.BeginChangeCheck();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            activeElement.image = EditorGUILayout.ObjectField ( activeElement.image, typeof(Texture2D), false, 
                                                                                new GUILayoutOption[] { 
                                                                                GUILayout.Width(100), 
                                                                                GUILayout.Height(100) 
                                                                                } ) as Texture2D;
                            GUILayout.Space(10);
                            EditorGUILayout.EndHorizontal();
                        if ( EditorGUI.EndChangeCheck() ) {
                            curEdit.Apply();
                            EditorUtility.SetDirty(curEdit);
                            Repaint();
                        }
                    EditorGUILayout.EndVertical();
                    break;

                case exUIElement.ContentType.TextureInfo:
                    EditorGUILayout.BeginVertical(new GUILayoutOption [] {
                                                  GUILayout.Height(200),
                                                  GUILayout.MinHeight(200),
                                                  GUILayout.MaxHeight(200),
                                                  GUILayout.Width(_width),
                                                  GUILayout.MinWidth(_width),
                                                  GUILayout.MaxWidth(_width),
                                                  GUILayout.ExpandHeight(false), 
                                                  GUILayout.ExpandWidth(false), 
                                                  });
                        EditorGUI.BeginChangeCheck();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            activeElement.image = EditorGUILayout.ObjectField ( activeElement.image, typeof(exTextureInfo), false, new GUILayoutOption[] { 
                                                                                GUILayout.Width(200) 
                                                                                } ) as exTextureInfo;
                            GUILayout.Space(10);
                            EditorGUILayout.EndHorizontal();
                        if ( EditorGUI.EndChangeCheck() ) {
                            curEdit.Apply();
                            EditorUtility.SetDirty(curEdit);
                            Repaint();
                        }
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.Space(20);

        EditorGUILayout.EndVertical();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    float ElementField ( int _controlID, float _x, float _y, int _width, int _indentLevel, exUIElement _el, ref bool _deleted ) {
        Event e = Event.current;

        float height = 25.0f;
        Vector2 size = Vector2.zero;
        float cur_x = _x + _indentLevel * 10.0f + 5.0f;
        float cur_y = _y;
        Rect rect = new Rect ( _x, _y, _width - 10.0f, height );
        bool deleted = false;

        // background
        Rect draggingHandleRect = new Rect(cur_x, cur_y + 10f, 10f, rect.height);
        if ( Event.current.type == EventType.Repaint ) {
            // draw background
            if ( activeElement == _el ) {
                hierarchyStyles.elementSelectionRect.Draw(rect, false, false, false, false);
            }
            else if ( hoverElement == _el ) {
                exEditorUtility.GUI_DrawRect( rect, new Color( 0.0f, 0.5f, 1.0f, 0.5f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
            }
            else if ( selectedElements.IndexOf(_el) != -1 ) {
                exEditorUtility.GUI_DrawRect( rect, new Color( 0.0f, 1.0f, 0.5f, 0.5f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
            }
            else if ( dropElement == _el ) {
                exEditorUtility.GUI_DrawRect( rect, new Color( 1.0f, 1.0f, 1.0f, 0.5f ), new Color( 0.0f, 0.0f, 0.0f, 0.0f ) );
            }
            else {
                hierarchyStyles.elementBackground.Draw(rect, false, false, false, false);
            }

            hierarchyStyles.draggingHandle.Draw( draggingHandleRect, false, false, false, false );
            EditorGUIUtility.AddCursorRect ( draggingHandleRect, MouseCursor.Pan );
        }

        //
        Vector2 minuIconSize = hierarchyStyles.removeButton.CalcSize( new GUIContent(hierarchyStyles.iconToolbarMinus) );
        float idWidth = 40.0f;
        float nameWidth = (_width - (int)cur_x - idWidth - (int)minuIconSize.x - 40.0f);

        // name
        cur_x += 10.0f;
        cur_x += 5.0f;
        EditorGUI.BeginChangeCheck ();
            _el.name = EditorGUI.TextField ( new Rect ( cur_x, cur_y + 4f, nameWidth, height - 8f ),
                                             _el.name ); 
        if ( EditorGUI.EndChangeCheck () ) {
            EditorUtility.SetDirty ( curEdit );
        }

        // #
        cur_x += nameWidth;
        cur_x += 5.0f;
        size = EditorStyles.label.CalcSize(new GUIContent("#") );
        EditorGUI.LabelField ( new Rect ( cur_x, cur_y + 4f, size.y, height - 8f ), "#" );

        // id
        cur_x += 10.0f;
        cur_x += 2.0f;
        EditorGUI.BeginChangeCheck ();
            _el.id = EditorGUI.TextField ( new Rect ( cur_x, cur_y + 4f, idWidth, height - 8f ),
                                             _el.id ); 
        if ( EditorGUI.EndChangeCheck () ) {
            EditorUtility.SetDirty ( curEdit );
        }

        // delete
        cur_x = rect.xMax - 5.0f - minuIconSize.x;
        if ( GUI.Button( new Rect( cur_x, rect.y + 2f, minuIconSize.x, minuIconSize.y ), 
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
        case EventType.MouseDrag:
            if ( draggingElements && rect.Contains(e.mousePosition) ) {
                if ( selectedElements.IndexOf(activeElement) != -1 ) {

                    bool canDrop = true;
                    for ( int i = 0; i < selectedElements.Count; ++i ) {
                        if ( selectedElements[i].IsSelfOrAncestorOf(_el) ) {
                            canDrop = false;
                            break;
                        } 
                    }

                    if ( canDrop ) {
                        dropElement = _el;
                        Repaint();
                    }
                }
            }
            break;

        case EventType.MouseMove:
            if ( rect.Contains(e.mousePosition) ) {
                hoverElement = _el;
                Repaint();
            }
            break;

        case EventType.MouseDown:
            dropElement = null;
            if ( e.button == 0 && e.clickCount == 1 && rect.Contains(e.mousePosition) ) {
                GUIUtility.hotControl = _controlID;
                GUIUtility.keyboardControl = _controlID;
                activeElement = _el;

                if ( draggingHandleRect.Contains(e.mousePosition) ) {
                    // draggingLayer = _layer; TODO
                }

                if ( selectedElements.IndexOf(activeElement) != -1 ) {
                    draggingElements = true;
                }
                else {
                    if ( e.control || e.command ) {
                        if ( selectedElements.IndexOf(activeElement) == -1 ) {
                            selectedElements.Add(activeElement);
                        }
                    }
                    else {
                        selectedElements.Clear();
                        selectedElements.Add(activeElement);
                        draggingElements = true;
                    }
                }

                Repaint();
                e.Use();
            }
            break;

        case EventType.MouseUp:
            if ( draggingElements ) {
                if ( dropElement != null ) {
                    for ( int i = 0; i < selectedElements.Count; ++i ) {
                        dropElement.AddElement(selectedElements[i]);
                    }

                    curEdit.Apply();
                    EditorUtility.SetDirty(curEdit);
                    Repaint();
                }
                
                draggingElements = false;
                dropElement = null;
                selectedElements.Clear();
                selectedElements.Add(activeElement);
            }
            break;
        }

        if ( deleted ) {
            _deleted = true;
            return 0.0f;
        }

        bool hasDirty = false;

        // children
        cur_y += height;
        for ( int i = 0; i < _el.children.Count; ++i ) {
            bool isDeleted = false;
            float totalHeight = ElementField ( _controlID, _x, cur_y, _width, _indentLevel + 1, _el.children[i], ref isDeleted );
            if ( isDeleted ) {
                _el.children.RemoveAt(i);
                --i;

                hasDirty = true;
            }
            cur_y += totalHeight;
        }

        if ( hasDirty ) {
            curEdit.Apply();
            EditorUtility.SetDirty ( curEdit );
            Repaint();
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
        // button 
        if ( GUILayout.Button( "Apply", hierarchyStyles.toolbarButton ) ) {
            curEdit.Apply();
            Repaint();
        }
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
                    GUILayout.Label ( "size", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        exCSSUI.SizePushField ( indentLevel, activeElement, "width", style.width, false );
                        exCSSUI.SizePushField ( indentLevel, activeElement, "height", style.height, false );
                        exCSSUI.MinSizeField ( indentLevel, activeElement, "min-width", style.minWidth, false );
                        exCSSUI.MinSizeField ( indentLevel, activeElement, "min-height", style.minHeight, false );
                        exCSSUI.MaxSizeField ( indentLevel, activeElement, "max-width", style.maxWidth, false );
                        exCSSUI.MaxSizeField ( indentLevel, activeElement, "max-height", style.maxHeight, false );
                    --indentLevel;

                    EditorGUILayout.Space();

                    // position
                    GUILayout.Label ( "position", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        exCSSUI.DisplayField ( indentLevel, activeElement, "display", ref style.display );
                        exCSSUI.PositionField ( indentLevel, activeElement, "position", ref style.position );
                        exCSSUI.SizeField ( indentLevel, activeElement, "top", style.top, false );
                        exCSSUI.SizeField ( indentLevel, activeElement, "right", style.right, false );
                        exCSSUI.SizeField ( indentLevel, activeElement, "bottom", style.bottom, false );
                        exCSSUI.SizeField ( indentLevel, activeElement, "left", style.left, false );
                    --indentLevel;

                    EditorGUILayout.Space();

                    // margin
                    GUILayout.Label ( "margin", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        exCSSUI.SizeField ( indentLevel, activeElement, "top", style.marginTop, false );
                        if ( exCSSUI.LockableSizeField ( indentLevel, 
                                                         activeElement, 
                                                         "right", 
                                                         style.marginRight, 
                                                         false, 
                                                         ref style.lockMarginRight ) ) 
                        {
                            if ( style.lockMarginRight ) {
                                style.lockMarginRight = true;
                                style.lockMarginBottom = true;
                                style.lockMarginLeft = true;
                            }
                        }
                        if ( exCSSUI.LockableSizeField ( indentLevel, 
                                                         activeElement, 
                                                         "bottom", 
                                                         style.marginBottom, 
                                                         false, 
                                                         ref style.lockMarginBottom ) ) 
                        {
                            if ( style.lockMarginBottom ) {
                                style.lockMarginBottom = true;
                                style.lockMarginLeft = true;
                            }
                            else {
                                style.lockMarginRight = false;
                            }
                        }
                        if ( exCSSUI.LockableSizeField ( indentLevel, 
                                                         activeElement, 
                                                         "left", 
                                                         style.marginLeft, 
                                                         false, 
                                                         ref style.lockMarginLeft ) ) 
                        {
                            if ( style.lockMarginLeft ) {
                                style.lockMarginLeft = true;
                            }
                            else {
                                style.lockMarginRight = false;
                                style.lockMarginBottom = false;
                            }
                        }

                        if ( style.lockMarginRight ) {
                            style.marginRight.type = style.marginTop.type;
                            style.marginRight.val = style.marginTop.val;
                            style.marginBottom.type = style.marginTop.type;
                            style.marginBottom.val = style.marginTop.val;
                            style.marginLeft.type = style.marginRight.type;
                            style.marginLeft.val = style.marginRight.val;
                        }
                        else if ( style.lockMarginBottom ) {
                            style.marginBottom.type = style.marginTop.type;
                            style.marginBottom.val = style.marginTop.val;
                            style.marginLeft.type = style.marginRight.type;
                            style.marginLeft.val = style.marginRight.val;
                        }
                        else if ( style.lockMarginLeft ) {
                            style.marginLeft.type = style.marginRight.type;
                            style.marginLeft.val = style.marginRight.val;
                        }

                    --indentLevel;

                    EditorGUILayout.Space();

                    // padding
                    GUILayout.Label ( "padding", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        exCSSUI.SizeNoAutoField ( indentLevel, activeElement, "top", style.paddingTop, false );
                        if ( exCSSUI.LockableSizeNoAutoField ( indentLevel, 
                                                               activeElement, 
                                                               "right", 
                                                               style.paddingRight, 
                                                               false, 
                                                               ref style.lockPaddingRight ) ) 
                        {
                            if ( style.lockPaddingRight ) {
                                style.lockPaddingRight = true;
                                style.lockPaddingBottom = true;
                                style.lockPaddingLeft = true;
                            }
                        }
                        if ( exCSSUI.LockableSizeNoAutoField ( indentLevel, 
                                                               activeElement, 
                                                               "bottom", 
                                                               style.paddingBottom, 
                                                               false, 
                                                               ref style.lockPaddingBottom ) ) 
                        {
                            if ( style.lockPaddingBottom ) {
                                style.lockPaddingBottom = true;
                                style.lockPaddingLeft = true;
                            }
                            else {
                                style.lockPaddingRight = false;
                            }
                        }
                        if ( exCSSUI.LockableSizeNoAutoField ( indentLevel, 
                                                               activeElement, 
                                                               "left", 
                                                               style.paddingLeft, 
                                                               false, 
                                                               ref style.lockPaddingLeft ) ) 
                        {
                            if ( style.lockPaddingLeft ) {
                                style.lockPaddingLeft = true;
                            }
                            else {
                                style.lockPaddingRight = false;
                                style.lockPaddingBottom = false;
                            }
                        }

                        if ( style.lockPaddingRight ) {
                            style.paddingRight.type = style.paddingTop.type;
                            style.paddingRight.val = style.paddingTop.val;
                            style.paddingBottom.type = style.paddingTop.type;
                            style.paddingBottom.val = style.paddingTop.val;
                            style.paddingLeft.type = style.paddingRight.type;
                            style.paddingLeft.val = style.paddingRight.val;
                        }
                        else if ( style.lockPaddingBottom ) {
                            style.paddingBottom.type = style.paddingTop.type;
                            style.paddingBottom.val = style.paddingTop.val;
                            style.paddingLeft.type = style.paddingRight.type;
                            style.paddingLeft.val = style.paddingRight.val;
                        }
                        else if ( style.lockPaddingLeft ) {
                            style.paddingLeft.type = style.paddingRight.type;
                            style.paddingLeft.val = style.paddingRight.val;
                        }

                    --indentLevel;

                    EditorGUILayout.Space();

                    // border
                    GUILayout.Label ( "border", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        GUILayout.BeginHorizontal ();
                        exCSSUI.ImageField ( indentLevel, activeElement, "image", style.borderImage, false );

                        exTextureInfo borderTextureInfo = style.borderImage.val as exTextureInfo;
                        GUI.enabled = (borderTextureInfo && borderTextureInfo.hasBorder);
                        if ( GUILayout.Button("Reset") ) {
                            style.borderSizeTop.type    = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeTop.val     = borderTextureInfo.borderTop;
                            style.borderSizeRight.type  = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeRight.val   = borderTextureInfo.borderRight;
                            style.borderSizeBottom.type = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeBottom.val  = borderTextureInfo.borderBottom;
                            style.borderSizeLeft.type   = exCSS_size_lengthonly.Type.Length;
                            style.borderSizeLeft.val    = borderTextureInfo.borderRight;

                            style.lockBorderSizeRight  = false;
                            style.lockBorderSizeBottom = false;
                            style.lockBorderSizeLeft   = false;
                        }
                        GUI.enabled = true;
                        GUILayout.EndHorizontal ();

                        exCSSUI.ColorField ( indentLevel, activeElement, "color", style.borderColor, false );
                        exCSSUI.SizeLengthOnlyField ( indentLevel, activeElement, "top", style.borderSizeTop, false );
                        if ( exCSSUI.LockableSizeLengthOnlyField ( indentLevel, 
                                                                   activeElement, 
                                                                   "right", 
                                                                   style.borderSizeRight, 
                                                                   false, 
                                                                   ref style.lockBorderSizeRight ) ) 
                        {
                            if ( style.lockBorderSizeRight ) {
                                style.lockBorderSizeRight = true;
                                style.lockBorderSizeBottom = true;
                                style.lockBorderSizeLeft = true;
                            }

                        }
                        if ( exCSSUI.LockableSizeLengthOnlyField ( indentLevel, 
                                                                   activeElement, 
                                                                   "bottom", 
                                                                   style.borderSizeBottom, 
                                                                   false, 
                                                                   ref style.lockBorderSizeBottom ) ) 
                        {
                            if ( style.lockBorderSizeBottom ) {
                                style.lockBorderSizeBottom = true;
                                style.lockBorderSizeLeft = true;
                            }
                            else {
                                style.lockBorderSizeRight = false;
                            }
                        }
                        if ( exCSSUI.LockableSizeLengthOnlyField ( indentLevel, 
                                                                   activeElement, 
                                                                   "left", 
                                                                   style.borderSizeLeft, 
                                                                   false, 
                                                                   ref style.lockBorderSizeLeft ) ) 
                        {
                            if ( style.lockBorderSizeLeft ) {
                                style.lockBorderSizeLeft = true;
                            }
                            else {
                                style.lockBorderSizeBottom = false;
                                style.lockBorderSizeRight = false;
                            }
                        }

                        if ( style.lockBorderSizeRight ) {
                            style.borderSizeRight.type = style.borderSizeTop.type;
                            style.borderSizeRight.val = style.borderSizeTop.val;
                            style.borderSizeBottom.type = style.borderSizeTop.type;
                            style.borderSizeBottom.val = style.borderSizeTop.val;
                            style.borderSizeLeft.type = style.borderSizeRight.type;
                            style.borderSizeLeft.val = style.borderSizeRight.val;
                        }
                        else if ( style.lockBorderSizeBottom ) {
                            style.borderSizeBottom.type = style.borderSizeTop.type;
                            style.borderSizeBottom.val = style.borderSizeTop.val;
                            style.borderSizeLeft.type = style.borderSizeRight.type;
                            style.borderSizeLeft.val = style.borderSizeRight.val;
                        }
                        else if ( style.lockBorderSizeLeft ) {
                            style.borderSizeLeft.type = style.borderSizeRight.type;
                            style.borderSizeLeft.val = style.borderSizeRight.val;
                        }
                    --indentLevel;

                    EditorGUILayout.Space();

                    // font
                    GUILayout.Label ( "font", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        exCSSUI.FontField ( indentLevel, activeElement, "font", style.font, true );
                        GUI.enabled = (style.font.type != exCSS_font.Type.BitmapFont) ? true : false;
                        if ( style.font.type == exCSS_font.Type.BitmapFont ) {
                            if ( style.fontSize.type != exCSS_size_noauto.Type.Length ) {
                                style.fontSize.type = exCSS_size_noauto.Type.Length;
                                GUI.changed = true;
                            }

                            exBitmapFont bitmapFont = style.font.val as exBitmapFont;
                            int bitmapFontSize = (bitmapFont == null) ? 0 : bitmapFont.size;
                            if ( style.fontSize.val != bitmapFontSize ) {
                                style.fontSize.val = bitmapFontSize;
                                GUI.changed = true;
                            }
                        }
                        exCSSUI.SizeNoAutoField ( indentLevel, activeElement, "size", style.fontSize, true );
                        GUI.enabled = true;
                    --indentLevel;

                    EditorGUILayout.Space();

                    // text
                    GUILayout.Label ( "content", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        exCSSUI.ColorField ( indentLevel, activeElement, "color", style.contentColor, true );
                        exCSSUI.WrapField ( indentLevel, activeElement, "wrap", ref style.wrap );
                        exCSSUI.HorizontalAlignField ( indentLevel, activeElement, "horizontal-align", ref style.horizontalAlign );
                        exCSSUI.VerticalAlignField ( indentLevel, activeElement, "vertical-align", ref style.verticalAlign );
                        exCSSUI.DecorationField ( indentLevel, activeElement, "decoration", ref style.textDecoration );
                        exCSSUI.SizeNoPercentageField ( indentLevel, activeElement, "letter-spacing", style.letterSpacing, true );
                        exCSSUI.SizeNoPercentageField ( indentLevel, activeElement, "word-spacing", style.wordSpacing, true );
                        exCSSUI.SizeField ( indentLevel, activeElement, "line-height", style.lineHeight, true );
                    --indentLevel;

                    EditorGUILayout.Space();

                    // background
                    GUILayout.Label ( "background", new GUILayoutOption[] { GUILayout.Width(200.0f) } );
                    ++indentLevel;
                        exCSSUI.ImageField ( indentLevel, activeElement, "image", style.backgroundImage, false );
                        exCSSUI.ColorField ( indentLevel, activeElement, "color", style.backgroundColor, false );
                        // TODO: exCSSUI.BackgroundRepeatField ( indentLevel, activeElement, "repeat", style.backgroundColor, false );
                    --indentLevel;


                    if ( EditorGUI.EndChangeCheck() ) {
                        // apply layout
                        curEdit.Apply();
                        EditorUtility.SetDirty (curEdit);
                        Repaint();
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
            DrawElements ( 0, 0, curEdit.root );

            // draw hover element
            if ( debugElement && hoverElement != null ) {
                float alpha = 0.78f;
                DrawHoverElement ( hoverElement, 
                                   new Color( 0.98f, 0.8f,  0.62f, alpha ),
                                   new Color( 0.5f,  0.5f,  0.5f,  alpha ),
                                   new Color( 0.76f, 0.87f, 0.71f, alpha ),
                                   new Color( 0.6f,  0.75f, 0.89f, alpha ) );
            }

            // draw active element border-line again
            if ( activeElement != null ) {
                // DrawElementBorder ( activeElement, new Color( 0.0f, 1.0f, 0.2f )  );
                DrawElementBorder ( activeElement, Color.white  );
            }

        GL.PopMatrix();

        GL.Viewport(oldViewport);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawElements ( int _x, int _y, exUIElement _el ) {
        // this is a dummy element, skip it.
        if ( _el.display == exCSS_display.Inline && _el.isContent == false ) 
            return;

        int element_x = _x + _el.x;
        int element_y = _y + _el.y;

        // draw border
        if ( _el.borderColor.a > 0.0f &&
             ( _el.borderSizeLeft > 0 || _el.borderSizeRight > 0 || _el.borderSizeTop > 0 || _el.borderSizeBottom > 0 ) ) 
        {
            int x = element_x - _el.paddingLeft - _el.borderSizeLeft;
            int y = element_y - _el.paddingTop - _el.borderSizeTop;
            int width = _el.width 
                + _el.paddingLeft + _el.paddingRight 
                + _el.borderSizeLeft + _el.borderSizeRight;
            int height = _el.height 
                + _el.paddingTop + _el.paddingBottom 
                + _el.borderSizeTop + _el.borderSizeBottom;

            if ( _el.borderImage == null ) {
                exEditorUtility.GL_UI_DrawBorderRectangle ( x, y, width, height, 
                                                            _el.borderSizeTop, _el.borderSizeRight, _el.borderSizeBottom, _el.borderSizeLeft,
                                                            _el.borderColor );
            }
            else {
                float s0 = 0.0f; 
                float t0 = 0.0f;
                float s1 = 1.0f;
                float t1 = 1.0f;
                Texture2D texture = _el.borderImage as Texture2D; 
                bool rotated = false;

                float uv_top = _el.borderSizeTop;
                float uv_bottom = _el.borderSizeBottom;
                float uv_left = _el.borderSizeLeft;
                float uv_right = _el.borderSizeRight;

                if ( texture == null ) {
                    exTextureInfo textureInfo = _el.borderImage as exTextureInfo;
                    if ( textureInfo != null ) {
                        texture = textureInfo.texture;

                        s0 = textureInfo.x * texture.texelSize.x;
                        t0 = textureInfo.y * texture.texelSize.y;
                        s1 = (textureInfo.x + textureInfo.rotatedWidth) * texture.texelSize.x;
                        t1 = (textureInfo.y + textureInfo.rotatedHeight) * texture.texelSize.y;

                        rotated = textureInfo.rotated;

                        uv_top = textureInfo.borderTop;
                        uv_bottom = textureInfo.borderBottom;
                        uv_left = textureInfo.borderLeft;
                        uv_right = textureInfo.borderRight;
                    }
                }

                exEditorUtility.GL_UI_DrawBorderTexture ( x, y, width, height, 
                                                          _el.borderSizeTop, _el.borderSizeRight, _el.borderSizeBottom, _el.borderSizeLeft,
                                                          uv_top, uv_right, uv_bottom, uv_left,
                                                          s0, t0, s1, t1,
                                                          texture,
                                                          _el.borderColor,
                                                          rotated );
            }
        }

        // draw background
        if ( _el.backgroundImage == null ) {
            if ( _el.backgroundColor.a > 0.0f ) {
                int x = element_x - _el.paddingLeft;
                int y = element_y - _el.paddingTop;
                int width = _el.width + _el.paddingLeft + _el.paddingRight; 
                int height = _el.height + _el.paddingTop + _el.paddingBottom; 
                exEditorUtility.GL_UI_DrawRectangle ( x, y, width, height, 
                                                      _el.backgroundColor );
            }
        }
        else {
            // TODO:
        }

        // draw content or child (NOTE: content-element will not have child) 
        if ( _el.isContent ) {
            switch ( _el.contentType ) {
            case exUIElement.ContentType.Text:
                DrawText ( element_x, element_y, _el, _el.text );
                break;

            case exUIElement.ContentType.Texture2D:
                // exEditorUtility.GUI_DrawTextureInfo ( new Rect( element_x, element_y, _el.width, _el.height ),
                //                                       textureInfo,
                //                                       _el.contentColor );
                break;

            case exUIElement.ContentType.TextureInfo:
                exEditorUtility.GUI_DrawTextureInfo ( new Rect( element_x, element_y, _el.width, _el.height ),
                                                      _el.image as exTextureInfo,
                                                      _el.contentColor );
                break;
            }
        }
        else {
            // DrawElementBorder ( _el, Color.white );
            for ( int i = 0; i < _el.normalFlows.Count; ++i ) {
                DrawElements( element_x, element_y, _el.normalFlows[i] );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawText ( int _x, int _y, exUIElement _el, string _text ) {
        if ( _el.font == null )
            return;

        Vector3[] vertices = new Vector3[_text.Length * 4];
        Vector2[] uvs = new Vector2[_text.Length * 4];
        Texture2D texture = null;

        if ( _el.font is Font ) {
            Font ttfFont = _el.font as Font;
            exTextUtility.BuildTextLine ( vertices,
                                          uvs,
                                          _text,
                                          ttfFont,
                                          _el.lineHeight,
                                          _el.fontSize,
                                          _el.wordSpacing,
                                          _el.letterSpacing );
            texture = ttfFont.material.mainTexture as Texture2D;

            exEditorUtility.materialAlphaBlendedVertColor.mainTexture = texture;
            exEditorUtility.materialAlphaBlendedVertColor.SetPass(0);
        }
        else if ( _el.font is exBitmapFont ) {
            exBitmapFont bitmapFont = _el.font as exBitmapFont;
            exTextUtility.BuildTextLine ( vertices,
                                          uvs,
                                          _text,
                                          bitmapFont,
                                          _el.lineHeight,
                                          _el.fontSize,
                                          _el.wordSpacing,
                                          _el.letterSpacing );
            texture = bitmapFont.texture;

            exEditorUtility.materialAlphaBlended.mainTexture = texture;
            exEditorUtility.materialAlphaBlended.SetPass(0);
        }

        GL.Begin(GL.QUADS);
        GL.Color(_el.contentColor);
        for ( int i = 0; i < _text.Length; ++i ) {
            int idx = 4*i;
            GL.TexCoord2 ( uvs[idx].x, uvs[idx].y );
            GL.Vertex3 ( vertices[idx].x + _x, vertices[idx].y + _y, 0.0f );

            GL.TexCoord2 ( uvs[idx+1].x, uvs[idx+1].y );
            GL.Vertex3 ( vertices[idx+1].x + _x, vertices[idx+1].y + _y, 0.0f );

            GL.TexCoord2 ( uvs[idx+2].x, uvs[idx+2].y );
            GL.Vertex3 ( vertices[idx+2].x + _x, vertices[idx+2].y + _y, 0.0f );

            GL.TexCoord2 ( uvs[idx+3].x, uvs[idx+3].y );
            GL.Vertex3 ( vertices[idx+3].x + _x, vertices[idx+3].y + _y, 0.0f );
        }
        GL.End();

        // DEBUG { 
        // for ( int i = 0; i < _text.Length; ++i ) {
        //     int idx = 4*i;
        //     exEditorUtility.GL_DrawRectLine ( new Vector3[] {
        //                                       new Vector3 ( vertices[idx].x   + _x, vertices[idx].y   + _y, 0.0f ),
        //                                       new Vector3 ( vertices[idx+1].x + _x, vertices[idx+1].y + _y, 0.0f ),
        //                                       new Vector3 ( vertices[idx+2].x + _x, vertices[idx+2].y + _y, 0.0f ),
        //                                       new Vector3 ( vertices[idx+3].x + _x, vertices[idx+3].y + _y, 0.0f ),
        //                                       },
        //                                       Color.white );
        // }
        // } DEBUG end 
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawElementBorder ( exUIElement _el, Color _color ) {
        int x;
        int y;
        _el.GetPosition ( out x, out y );

        exEditorUtility.GL_DrawRectLine( new Vector3[] {
                                         new Vector3( x,             y,              0.0f ),
                                         new Vector3( x + _el.width, y,              0.0f ),
                                         new Vector3( x + _el.width, y + _el.height, 0.0f ),
                                         new Vector3( x,             y + _el.height, 0.0f ),
                                         }, _color );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawHoverElement ( exUIElement _el, 
                            Color _marginColor,
                            Color _borderColor,
                            Color _paddingColor,
                            Color _contentColor ) 
    {
        int x;
        int y;
        _el.GetPosition ( out x, out y );

        float cur_x = x;
        float cur_y = y;
        float width = _el.width;
        float height = _el.height;

        // content
        exEditorUtility.GL_UI_DrawRectangle ( cur_x, cur_y, width, height, 
                                              _contentColor );

        // padding
        cur_x -= _el.paddingLeft;
        cur_y -= _el.paddingTop;
        width += (_el.paddingLeft + _el.paddingRight);
        height += (_el.paddingTop + _el.paddingBottom);
        exEditorUtility.GL_UI_DrawBorderRectangle ( cur_x, cur_y, width, height, 
                                                    _el.paddingTop, _el.paddingRight, _el.paddingBottom, _el.paddingLeft, 
                                                    _paddingColor );

        // border
        cur_x -= _el.borderSizeLeft;
        cur_y -= _el.borderSizeTop;
        width += (_el.borderSizeLeft + _el.borderSizeRight);
        height += (_el.borderSizeTop + _el.borderSizeBottom);
        exEditorUtility.GL_UI_DrawBorderRectangle ( cur_x, cur_y, width, height, 
                                                    _el.borderSizeTop, _el.borderSizeRight, _el.borderSizeBottom, _el.borderSizeLeft, 
                                                    _borderColor );

        // margin
        cur_x -= _el.marginLeft;
        cur_y -= _el.marginTop;
        width += (_el.marginLeft + _el.marginRight);
        height += (_el.marginTop + _el.marginBottom);
        exEditorUtility.GL_UI_DrawBorderRectangle ( cur_x, cur_y, width, height, 
                                                    _el.marginTop, _el.marginRight, _el.marginBottom, _el.marginLeft, 
                                                    _marginColor );
    }

    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // void ProcessSceneEditorHandles () {

    //     editCamera.enabled = true;
    //     editCamera.aspect = sceneViewRect.width/sceneViewRect.height;
    //     editCamera.orthographicSize = (sceneViewRect.height * 0.5f) / scale;

    //     //
    //     GUI.BeginGroup( sceneViewRect );
    //     Rect rect = new Rect( 0, 0, sceneViewRect.width, sceneViewRect.height );
    //     Handles.ClearCamera( rect, editCamera );
    //     Handles.SetCamera( rect, editCamera );

    //     if ( debugElement && hoverElement != null ) {
    //         Handles.BeginGUI();
    //         GUI.Label( new Rect ( hoverElement.x, hoverElement.y, 200.0f, 200.0f ), "Hello World" );
    //         Handles.EndGUI();
    //     }


    //     editCamera.enabled = false;
    //     GUI.EndGroup();
    // }
}
