// ======================================================================================
// File         : exAtlasEditor.cs
// Author       : Wu Jie 
// Last Change  : 06/18/2013 | 00:17:05 AM | Tuesday,June
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
/// the atlas editor
///
///////////////////////////////////////////////////////////////////////////////

partial class exAtlasEditor : EditorWindow {

    ///////////////////////////////////////////////////////////////////////////////
    // static 
    ///////////////////////////////////////////////////////////////////////////////

    static int[] sizeList = new int[] { 
        32, 64, 128, 256, 512, 1024, 2048, 4096 
    };
    static string[] sizeTextList = new string[] { 
        "32px", "64px", "128px", "256px", "512px", "1024px", "2048px", "4096px" 
    };
	static int exAtlasEditorHash = "exAtlasEditor".GetHashCode();

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exAtlas curEdit = null;
    SerializedObject curSerializedObject = null;

    Vector2 scrollPos = Vector2.zero;
    List<Object> selectedObjects = new List<Object>();
    Rect atlasRect = new Rect( 0, 0, 1, 1 );
    Rect scrollViewRect = new Rect( 0, 0, 1, 1 );

    // GUI options 
    bool lockCurEdit = false; 
    bool foldoutCanvas = true;
    bool foldoutLayout = true;
    bool foldoutTextureInfo = true;
    bool foldoutBuild = true;

    // GUI states
    // bool inDraggingTextureInfoState = false;
    List<Object> importObjects = new List<Object>();
    Object oldSelActiveObject = null;
    List<Object> oldSelObjects = new List<Object>();

    exRectSelection<Object> rectSelection = null;

    Material quadMaterial = null;
    Mesh quadMesh = null;

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        title = "Atlas Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = false;

        quadMaterial = new Material( Shader.Find("ex2D/Alpha Blended") );
        quadMaterial.hideFlags = HideFlags.DontSave;
        quadMesh = new Mesh();
        quadMesh.hideFlags = HideFlags.DontSave;

        rectSelection = new exRectSelection<Object>( PickObject,
                                                     PickRectObjects,
                                                     ConfirmRectSelection );

        UpdateEditObject ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        Object.DestroyImmediate(quadMaterial);
        Object.DestroyImmediate(quadMesh);
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnFocus () {
        UpdateEditObject ();
        UpdateSelection();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnSelectionChange () {
        UpdateEditObject ();
        UpdateSelection();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnInspectorUpdate () {
        if ( curEdit == null )
            return;

        for ( int i = curEdit.textureInfos.Count-1; i >= 0; --i ) {
            exTextureInfo textureInfo = curEdit.textureInfos[i];
            if ( textureInfo == null ) {
                curEdit.textureInfos.RemoveAt(i);
            }
        }
        for ( int i = curEdit.bitmapFonts.Count-1; i >= 0; --i ) {
            exBitmapFont bitmapFont = curEdit.bitmapFonts[i];
            if ( bitmapFont == null ) {
                curEdit.bitmapFonts.RemoveAt(i);
            }
        }
        for ( int i = selectedObjects.Count-1; i >= 0; --i ) {
            Object obj = selectedObjects[i];
            if ( obj == null ) {
                selectedObjects.RemoveAt(i);
            }
        }

        Repaint();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        if ( curEdit == null ) {
            EditorGUILayout.Space();
            GUILayout.Label ( "Please select an Atlas" );
            return;
        }
        if ( curSerializedObject == null )
            curSerializedObject = new SerializedObject(curEdit);


        curSerializedObject.Update ();

        // toolbar
        Toolbar ();

        // DISABLE { 
        // NOTE: we can't use GUILayoutUtility.GetLastRect() here, 
        //       because GetLastRect() will return wrong value when Event.current.type is EventType.Layout
        // Rect rect = GUILayoutUtility.GetLastRect ();
        // float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
        // scrollPos = EditorGUILayout.BeginScrollView ( scrollPos, 
        //                                               GUILayout.Width(position.width),
        //                                               GUILayout.Height(position.height-toolbarHeight) );
        // } DISABLE end 
        scrollPos = EditorGUILayout.BeginScrollView ( scrollPos );
            // atlas
            Object newAtlas = EditorGUILayout.ObjectField( "Atlas"
                                                           , curEdit
                                                           , typeof(exAtlas)
                                                           , false 
                                                           , new GUILayoutOption[] {
                                                               GUILayout.Width(400), 
                                                           }
                                                         );
            if ( newAtlas != curEdit ) 
                Selection.activeObject = newAtlas;

            EditorGUILayout.Space();

            //
            EditorGUILayout.BeginHorizontal();
                //
                Settings ();

                //
                GUILayout.Space(40);

                //
                Layout_AtlasField ( Mathf.FloorToInt(curEdit.width * curEdit.scale), 
                                    Mathf.FloorToInt (curEdit.height * curEdit.scale) );

            EditorGUILayout.EndHorizontal();

            //
            Rect lastRect = GUILayoutUtility.GetLastRect();
            CalculateScrollViewRect( new Rect( 0, 0, lastRect.xMax, lastRect.yMax ),
                                     false, 
                                     false );
            ProcessEvents();
            rectSelection.SetSelection(selectedObjects.ToArray());
            rectSelection.OnGUI();

        EditorGUILayout.EndScrollView();

        curSerializedObject.ApplyModifiedProperties ();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateEditObject () {
        if ( lockCurEdit == false || curEdit == null ) {
            exAtlas atlas = Selection.activeObject as exAtlas;
            if ( atlas != null && atlas != curEdit ) {
                Edit (atlas);
            }
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Reset () {
        curSerializedObject = null;
        scrollPos = Vector2.zero;
        selectedObjects.Clear();
        importObjects.Clear();
        oldSelActiveObject = null;
        oldSelObjects.Clear();

        UpdateSelection();

        // inDraggingTextureInfoState = false;
    }

    // ------------------------------------------------------------------ 
    /// \param _obj
    /// Check if the object is valid atlas and open it in atlas editor.
    // ------------------------------------------------------------------ 

    public void Edit ( exAtlas _atlas ) {
        if ( _atlas == null )
            return;

        curEdit = _atlas;

        Reset ();
        Repaint ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateSelection () {
        if ( curEdit != null ) {
            bool needRepaint = false;
            selectedObjects.Clear();
            foreach ( Object obj in Selection.objects ) {
                exTextureInfo textureInfo = obj as exTextureInfo;
                if ( textureInfo != null && curEdit.textureInfos.IndexOf(textureInfo) != -1 ) {
                    selectedObjects.Add(textureInfo);
                    needRepaint = true;
                }

                exBitmapFont bitmapFont = obj as exBitmapFont;
                if ( bitmapFont != null && curEdit.bitmapFonts.IndexOf(bitmapFont) != -1 ) {
                    selectedObjects.Add(bitmapFont);
                    needRepaint = true;
                }
            }
            if ( needRepaint ) {
                Repaint();
            }
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Toolbar () {
        EditorGUILayout.BeginHorizontal ( EditorStyles.toolbar );

            GUILayout.FlexibleSpace();

            // DISABLE { 
            // // ======================================================== 
            // // Select 
            // // ======================================================== 

            // GUI.enabled = selectedObjects.Count != 0;
            // if ( GUILayout.Button( "Select In Project...", EditorStyles.toolbarButton ) ) {
            //     selectIdx = (selectIdx + 1) % selectedObjects.Count;  
            //     Selection.objects = selectedObjects.ToArray();
            //     EditorGUIUtility.PingObject(Selection.objects[selectIdx]);
            // }
            // GUI.enabled = true;
            // EditorGUILayout.Space();
            // } DISABLE end 

            // ======================================================== 
            // zoom in/out button & slider 
            // ======================================================== 

            EditorGUI.BeginChangeCheck();
                // button 
                if ( GUILayout.Button( "Zoom", EditorStyles.toolbarButton ) ) {
                    curEdit.scale = 1.0f;
                }

                EditorGUILayout.Space();

                // slider
                curEdit.scale = GUILayout.HorizontalSlider ( curEdit.scale, 
                                                             0.1f, 
                                                             2.0f, 
                                                             new GUILayoutOption[] {
                                                                 GUILayout.MinWidth(50),
                                                                 GUILayout.MaxWidth(150)
                                                             } );
                EditorGUILayout.Space();
                curEdit.scale = EditorGUILayout.FloatField( curEdit.scale,
                                                            EditorStyles.toolbarTextField,
                                                            new GUILayoutOption[] {
                                                                GUILayout.Width(30)
                                                            } );
            if ( EditorGUI.EndChangeCheck() ) {
                EditorUtility.SetDirty(curEdit);
            }

            // ======================================================== 
            // Sync 
            // ======================================================== 

            EditorGUILayout.Space();
            if ( GUILayout.Button( "Sync", EditorStyles.toolbarButton ) ) {
                try {
                    exAtlasUtility.Sync( curEdit, (_progress, _info) => {
                                            EditorUtility.DisplayProgressBar( "Syncing Atlas...", _info, _progress );
                                          } );
                }
                finally {
                    EditorUtility.ClearProgressBar();    
                }
            }

            // ======================================================== 
            // Build 
            // ======================================================== 

            GUI.enabled = curEdit.needRebuild;
            EditorGUILayout.Space();
            if ( GUILayout.Button( "Build", EditorStyles.toolbarButton ) ) {
                try {
                    exAtlasUtility.Build( curEdit, (_progress, _info) => {
                                            EditorUtility.DisplayProgressBar( "Building Atlas...", _info, _progress );
                                          } );
                }
                finally {
                    curEdit.needRebuild = false;
                    EditorUtility.ClearProgressBar();    
                }
            }
            GUI.enabled = true;

            // ======================================================== 
            // Lock 
            // ======================================================== 

            EditorGUILayout.Space();
			GUILayout.BeginVertical();
            GUILayout.Space(3f);
            lockCurEdit = GUILayout.Toggle ( lockCurEdit, GUIContent.none, "IN LockButton", new GUILayoutOption [] {
                                                GUILayout.Width(20),
                                                GUILayout.Height(20),
                                             } );
			GUILayout.EndVertical();

            // ======================================================== 
            // Help
            // ======================================================== 

            if ( GUILayout.Button( exEditorUtility.HelpTexture(), EditorStyles.toolbarButton ) ) {
                Help.BrowseURL("http://www.ex-dev.com/ex2d/wiki/doku.php?id=manual:atlas_editor");
            }

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Settings () {
        GUIContent content = null;

        EditorGUILayout.BeginVertical( new GUILayoutOption [] {
                                           GUILayout.Width(250), 
                                           GUILayout.MinWidth(250), 
                                           GUILayout.MaxWidth(250),
                                           GUILayout.ExpandWidth(false)
                                       } );
        EditorGUI.BeginChangeCheck();

            // ======================================================== 
            // canvas
            // ======================================================== 

            foldoutCanvas = EditorGUILayout.Foldout(foldoutCanvas, "Canvas");
            if ( foldoutCanvas ) {
                EditorGUI.indentLevel++;

                // width and height
                int width = EditorGUILayout.IntPopup ( "Width", curEdit.width, sizeTextList, sizeList );
                int height = EditorGUILayout.IntPopup ( "Height", curEdit.height, sizeTextList, sizeList );

                // Check if we need to Reset width & height
                if ( width != curEdit.width || height != curEdit.height ) {
                    curEdit.width = width;
                    curEdit.height = height;
                    curEdit.needRebuild = true;
                }

                // EditorGUILayout.PropertyField (curSerializedObject.FindProperty ("bgColor"), new GUIContent("Background"));
                // EditorGUILayout.PropertyField (curSerializedObject.FindProperty ("showCheckerboard"), new GUIContent("Checkerboard"));
                curEdit.bgColor = EditorGUILayout.ColorField( "Background", curEdit.bgColor );
                curEdit.showCheckerboard = EditorGUILayout.Toggle ( "Checkerboard", curEdit.showCheckerboard );

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            // ======================================================== 
            // layout
            // ======================================================== 

            foldoutLayout = EditorGUILayout.Foldout(foldoutLayout, "Layout");
            if ( foldoutLayout ) {
                EditorGUI.indentLevel++;

                curEdit.algorithm = (exAtlas.Algorithm)EditorGUILayout.EnumPopup ( "Algorithm", curEdit.algorithm );
                curEdit.sortBy = (exAtlas.SortBy)EditorGUILayout.EnumPopup ( "Sort By", curEdit.sortBy );
                curEdit.sortOrder = (exAtlas.SortOrder)EditorGUILayout.EnumPopup ( "Sort Order", curEdit.sortOrder );

                // padding
                curEdit.paddingMode = (exAtlas.PaddingMode)EditorGUILayout.EnumPopup("Padding", curEdit.paddingMode);
                EditorGUI.indentLevel++;
                    GUI.enabled = (curEdit.paddingMode == exAtlas.PaddingMode.Custom);
                    curEdit.customPadding = System.Math.Max( EditorGUILayout.IntField("Pixels", curEdit.actualPadding), 0 ); // Clamp to 0
                    GUI.enabled = true;
                EditorGUI.indentLevel--;

                // allow rotate
                curEdit.allowRotate = EditorGUILayout.Toggle ( "Allow Rotate", curEdit.allowRotate );

                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if ( GUILayout.Button ( "Apply", 
                                            new GUILayoutOption [] {
                                                GUILayout.Width(80)
                                            } ) ) {
                        LayoutAtlasElements();
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            // ======================================================== 
            // TextureInfo
            // ======================================================== 

            foldoutTextureInfo = EditorGUILayout.Foldout(foldoutTextureInfo, "TextureInfo");
            if ( foldoutTextureInfo ) {
                EditorGUI.indentLevel++;

                // sprite background color
                curEdit.elementBgColor = EditorGUILayout.ColorField( "Background", curEdit.elementBgColor );
                curEdit.elementSelectColor = EditorGUILayout.ColorField( "Select", curEdit.elementSelectColor );

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            // ======================================================== 
            // Build 
            // ======================================================== 

            foldoutBuild = EditorGUILayout.Foldout(foldoutBuild, "Build");
            if ( foldoutBuild ) {
                EditorGUI.indentLevel++;

                // ======================================================== 
                // build color 
                // ======================================================== 

                EditorGUILayout.BeginHorizontal();
                    bool newCustomBuildColor = EditorGUILayout.Toggle ( "Custom Build Color", curEdit.customBuildColor ); 
                    if ( newCustomBuildColor != curEdit.customBuildColor ) {
                        curEdit.customBuildColor = newCustomBuildColor;
                        curEdit.needRebuild = true;
                    }
                    if ( curEdit.customBuildColor ) {
                        Color newBuildColor = EditorGUILayout.ColorField( curEdit.buildColor );
                        if ( newBuildColor != curEdit.buildColor ) {
                            curEdit.buildColor = newBuildColor;
                            curEdit.needRebuild = true;
                        }
                    }
                EditorGUILayout.EndHorizontal();

                // ======================================================== 
                // contour bleed
                // ======================================================== 

                GUI.enabled = !curEdit.customBuildColor;
                EditorGUILayout.BeginHorizontal();
                    content = new GUIContent( "Use Contour Bleed", 
                                              "Prevents artifacts around the silhouette of artwork due to bilinear filtering (requires Build Color to be turned off)" );
                    if ( curEdit.useContourBleed != EditorGUILayout.Toggle ( content, curEdit.useContourBleed ) ) {
                        curEdit.useContourBleed = !curEdit.useContourBleed;
                        curEdit.needRebuild = true;
                    }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;

                // ======================================================== 
                // padding bleed
                // ======================================================== 

                GUI.enabled = (curEdit.paddingMode == exAtlas.PaddingMode.Auto) || (curEdit.actualPadding >= 2);
                EditorGUILayout.BeginHorizontal();
                    content = new GUIContent( "Use Padding Bleed", 
                                              "Prevents artifacts and seams around the outer bounds of a texture due to bilinear filtering (requires at least Padding of 2)" );
                    if ( curEdit.usePaddingBleed != EditorGUILayout.Toggle ( content, curEdit.usePaddingBleed ) ) {
                        curEdit.usePaddingBleed = !curEdit.usePaddingBleed;
                        curEdit.needRebuild = true;
                    }
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;

                // ======================================================== 
                // trim elements 
                // ======================================================== 

                curEdit.trimElements = EditorGUILayout.Toggle ( "Trimmed Elements", curEdit.trimElements );

                // ======================================================== 
                // trim threshold 
                // ======================================================== 

                curEdit.trimThreshold = EditorGUILayout.IntField ( "Trimmed Threshold", curEdit.trimThreshold );

                // ======================================================== 
                // readable
                // ======================================================== 

                if ( EditorGUILayout.Toggle ( "Read/Write Enabled", curEdit.readable ) != curEdit.readable ) {
                    curEdit.readable = !curEdit.readable;

                    if ( curEdit.texture != null ) {
                        string atlasTexturePath = AssetDatabase.GetAssetPath(curEdit.texture);
                        TextureImporter importSettings = TextureImporter.GetAtPath(atlasTexturePath) as TextureImporter;
                        importSettings.isReadable = curEdit.readable;
                        AssetDatabase.ImportAsset( atlasTexturePath, ImportAssetOptions.ForceSynchronousImport );
                    }
                }

                // ======================================================== 
                // texture 
                // ======================================================== 

                EditorGUILayout.ObjectField( "Texture"
                                             , curEdit.texture
                                             , typeof(Texture2D)
                                             , false
                                           );

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

        // check gui changes 
        if ( EditorGUI.EndChangeCheck() ) {
            EditorUtility.SetDirty(curEdit);
        }

        EditorGUI.indentLevel++;
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.yellow;
        EditorGUILayout.LabelField( "Zoom in/out: Ctrl + MouseWheel", style );
        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Layout_AtlasField ( int _width, int _height ) {
        Rect rect = GUILayoutUtility.GetRect ( _width+4, _height+4, 
                                               new GUILayoutOption[] {
                                                   GUILayout.ExpandWidth(false),
                                                   GUILayout.ExpandHeight(false)
                                               });
        AtlasField (rect);
    }
    void AtlasField ( Rect _rect ) {
        Event e = Event.current;

        switch ( e.type ) {
        case EventType.Repaint:
            atlasRect = new Rect( _rect.x + 2, _rect.y + 2, _rect.width - 4, _rect.height - 4 );

            // checker box
            Color old = GUI.color;
            GUI.color = curEdit.bgColor;
                if ( curEdit.showCheckerboard ) {
                    Texture2D checker = exEditorUtility.CheckerboardTexture();
                    GUI.DrawTextureWithTexCoords ( atlasRect, checker, 
                                                   new Rect( 0.0f, 0.0f, atlasRect.width/(checker.width * curEdit.scale), atlasRect.height/(checker.height * curEdit.scale)) );
                }
                else {
                    GUI.DrawTexture( atlasRect, EditorGUIUtility.whiteTexture );
                }
            GUI.color = old;


            // texture info list 
            GUI.BeginGroup( atlasRect );
            foreach ( exTextureInfo textureInfo in curEdit.textureInfos ) {
                if ( textureInfo == null )
                    continue;

                Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D>( textureInfo.rawTextureGUID );
                bool selected = selectedObjects.IndexOf(textureInfo) != -1;
                DrawAtlasElement ( MapTextureInfo( new Rect ( 0, 0, atlasRect.width, atlasRect.height ), textureInfo ),
                                   rawTexture, 
                                   textureInfo.trim_x,
                                   textureInfo.trim_y,
                                   textureInfo.width,
                                   textureInfo.height,
                                   textureInfo.rotated,
                                   selected );
            }
            foreach ( exBitmapFont bitmapFont in curEdit.bitmapFonts ) {
                if ( bitmapFont == null )
                    continue;
                
                Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D>( bitmapFont.rawTextureGUID );
                bool selected = selectedObjects.IndexOf(bitmapFont) != -1;
                foreach ( exBitmapFont.CharInfo charInfo in bitmapFont.charInfos ) {
                    DrawAtlasElement ( MapCharInfo ( new Rect ( 0, 0, atlasRect.width, atlasRect.height ), charInfo ), 
                                       rawTexture,
                                       charInfo.trim_x,
                                       charInfo.trim_y,
                                       charInfo.width,
                                       charInfo.height,
                                       charInfo.rotated,
                                       selected );
                }
            }
            GUI.EndGroup();

            // border
            exEditorUtility.DrawRect( _rect,
                                      new Color( 1,1,1,0 ), 
                                      EditorStyles.label.normal.textColor );
            break;

        case EventType.MouseDown:
            if ( e.button == 0 && e.clickCount == 1 && _rect.Contains(e.mousePosition) ) {
                foreach ( exTextureInfo textureInfo in curEdit.textureInfos ) {
                    if ( textureInfo == null )
                        continue;
                }
            }
            break;

        case EventType.DragUpdated:
            if ( _rect.Contains(e.mousePosition) ) {
                // Show a copy icon on the drag
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( o is Texture2D 
                      || o is exBitmapFont
                      || exEditorUtility.IsDirectory(o) 
                      || exBitmapFontUtility.IsFontInfo(o) ) 
                    {
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

                // NOTE: Unity3D have a problem in ImportTextureForAtlas, when a texture is an active selection, 
                //       no matter how you change your import settings, finally it will apply changes that in Inspector (shows when object selected)
                oldSelActiveObject = null;
                oldSelObjects.Clear();
                foreach ( Object o in Selection.objects ) {
                    oldSelObjects.Add(o);
                }
                oldSelActiveObject = Selection.activeObject;

                // NOTE: Selection.GetFiltered only affect on activeObject, but we may proceed non-active selections sometimes
                importObjects.Clear();
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( exEditorUtility.IsDirectory(o) ) {
                        Selection.activeObject = o;

                        // add Texture2D objects
                        Object[] objs = Selection.GetFiltered( typeof(Object), SelectionMode.DeepAssets);
                        foreach ( Object obj in objs ) {
                            if ( importObjects.IndexOf(obj) == -1 ) {
                                importObjects.Add(obj);
                            }
                        }
                    }
                    else {
                        if ( importObjects.IndexOf(o) == -1 ) {
                            importObjects.Add(o);
                        }
                    }
                }

                Selection.activeObject = null;


                try {
                    exAtlasUtility.ImportObjects ( curEdit, importObjects.ToArray(), (_progress, _info) => {
                                                      EditorUtility.DisplayProgressBar( "Adding zbjects to Atlas...", _info, _progress );
                                                   } );
                }
                finally {
                    importObjects.Clear();
                    EditorUtility.ClearProgressBar();    
                }

                LayoutAtlasElements();

                Repaint();

                // recover selections
                Selection.activeObject = oldSelActiveObject;
                Selection.objects = oldSelObjects.ToArray();

                e.Use();
            }
            break;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawAtlasElement ( Rect _rect, 
                            Texture2D _rawTexture, 
                            int _trim_x,
                            int _trim_y,
                            int _trim_width,
                            int _trim_height,
                            bool _rotated,
                            bool _selected ) 
    {
        // calculate scroll view rect in altas space plus atlas clipped rect
        Rect scrollViewRectInAtlasSpace = scrollViewRect;
        scrollViewRectInAtlasSpace.xMin = Mathf.Max( scrollViewRectInAtlasSpace.xMin, atlasRect.xMin ); 
        scrollViewRectInAtlasSpace.xMax = Mathf.Min( scrollViewRectInAtlasSpace.xMax, atlasRect.xMax ); 
        scrollViewRectInAtlasSpace.yMin = Mathf.Max( scrollViewRectInAtlasSpace.yMin, atlasRect.yMin ); 
        scrollViewRectInAtlasSpace.yMax = Mathf.Min( scrollViewRectInAtlasSpace.yMax, atlasRect.yMax ); 
        scrollViewRectInAtlasSpace = new Rect( scrollViewRectInAtlasSpace.x - atlasRect.x,
                                               scrollViewRectInAtlasSpace.y - atlasRect.y,
                                               scrollViewRectInAtlasSpace.width,
                                               scrollViewRectInAtlasSpace.height ); 

        // check if we've been clipped
        if ( _rect.xMin >= scrollViewRectInAtlasSpace.xMax )
            return;
        if ( _rect.xMax <= scrollViewRectInAtlasSpace.xMin )
            return;
        if ( _rect.yMin >= scrollViewRectInAtlasSpace.yMax )
            return;
        if ( _rect.yMax <= scrollViewRectInAtlasSpace.yMin )
            return;

        Color old = GUI.color;
        GUI.color = curEdit.elementBgColor;
            GUI.DrawTexture( _rect, EditorGUIUtility.whiteTexture );
        GUI.color = old;

        if ( _rawTexture ) {
            if ( _rotated ) {
                Rect clippedRect = _rect;
                float trim_x = (float)_trim_x;
                float trim_y = (float)_trim_y;
                float trim_width = (float)_trim_width;
                float trim_height = (float)_trim_height;

                if ( _rect.xMax > scrollViewRectInAtlasSpace.xMax ) {
                    float delta = (_rect.xMax - scrollViewRectInAtlasSpace.xMax)/_rect.width * (float)_trim_height;
                    trim_height -= delta;
                    trim_y += delta;
                    clippedRect.width -= (_rect.xMax - scrollViewRectInAtlasSpace.xMax);
                }
                if ( _rect.yMax > scrollViewRectInAtlasSpace.yMax ) {
                    float delta = (_rect.yMax - scrollViewRectInAtlasSpace.yMax)/_rect.height * (float)_trim_width;
                    trim_width -= delta;
                    trim_x += delta;
                    clippedRect.height -= (_rect.yMax - scrollViewRectInAtlasSpace.yMax);
                }

                if ( _rect.xMin < scrollViewRectInAtlasSpace.xMin ) {
                    float delta = (scrollViewRectInAtlasSpace.xMin - _rect.xMin)/_rect.width * (float)_trim_height;
                    trim_height -= delta;
                    clippedRect.width -= (scrollViewRectInAtlasSpace.xMin - _rect.xMin);
                    clippedRect.x += (scrollViewRectInAtlasSpace.xMin - _rect.xMin);
                }
                if ( _rect.yMin < scrollViewRectInAtlasSpace.yMin ) {
                    float delta = (scrollViewRectInAtlasSpace.yMin - _rect.yMin)/_rect.height * (float)_trim_width;
                    trim_width -= delta;
                    clippedRect.height -= (scrollViewRectInAtlasSpace.yMin - _rect.yMin);
                    clippedRect.y += (scrollViewRectInAtlasSpace.yMin - _rect.yMin);
                }

                float xStart = trim_x/(float)_rawTexture.width;
                float xEnd = xStart + trim_width/(float)_rawTexture.width;
                float yStart = trim_y/(float)_rawTexture.height;
                float yEnd = yStart + trim_height/(float)_rawTexture.height;

                quadMaterial.mainTexture = _rawTexture;
                quadMaterial.SetPass(0);

                quadMesh.hideFlags = HideFlags.DontSave;
                quadMesh.vertices = new Vector3[] {
                    new Vector3 ( clippedRect.x, clippedRect.y, 0.0f ),
                    new Vector3 ( clippedRect.x, clippedRect.y + clippedRect.height, 0.0f ),
                    new Vector3 ( clippedRect.x + clippedRect.width, clippedRect.y + clippedRect.height, 0.0f ),
                    new Vector3 ( clippedRect.x + clippedRect.width, clippedRect.y, 0.0f ),
                };
                quadMesh.uv = new Vector2[] {
                    new Vector2 ( xEnd, yEnd ),
                    new Vector2 ( xStart, yEnd ),
                    new Vector2 ( xStart, yStart ),
                    new Vector2 ( xEnd, yStart ),
                };
                quadMesh.colors32 = new Color32[] {
                    new Color32 ( 255, 255, 255, 255 ),
                    new Color32 ( 255, 255, 255, 255 ),
                    new Color32 ( 255, 255, 255, 255 ),
                    new Color32 ( 255, 255, 255, 255 ),
                };
                quadMesh.triangles = new int[] {
                    0, 1, 2,
                    0, 2, 3
                };

                Graphics.DrawMeshNow ( quadMesh, Vector3.zero, Quaternion.identity );
            }
            else {
                GUI.DrawTextureWithTexCoords( _rect, 
                                              _rawTexture,
                                              new Rect( (float)_trim_x/(float)_rawTexture.width,
                                                        (float)_trim_y/(float)_rawTexture.height,
                                                        (float)_trim_width/(float)_rawTexture.width,
                                                        (float)_trim_height/(float)_rawTexture.height ) );
            }
        }

        if ( _selected ) {
            exEditorUtility.DrawRectBorder( _rect, curEdit.elementSelectColor );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void CalculateScrollViewRect ( Rect _viewRect, bool _alwaysShowHorizontal, bool _alwaysShowVertical ) {
        Event e = Event.current;
        if ( e.type != EventType.Layout && e.type != EventType.Used ) {
            bool flag = _alwaysShowVertical;
            bool flag2 = _alwaysShowHorizontal;
            GUIStyle horizontalScrollbar = GUI.skin.horizontalScrollbar;
            GUIStyle verticalScrollbar = GUI.skin.verticalScrollbar;
			Rect screenRect = new Rect( 0, 0, position.width, position.height );

            if ( flag2 || _viewRect.width > screenRect.width ) {
                screenRect.height -= horizontalScrollbar.fixedHeight + (float)horizontalScrollbar.margin.top;
                flag2 = true;
            }
            if ( flag || _viewRect.height > screenRect.height ) {
                screenRect.width -= verticalScrollbar.fixedWidth + (float)verticalScrollbar.margin.left;
                flag = true;
                if ( !flag2 && _viewRect.width > screenRect.width ) {
                    screenRect.height -= horizontalScrollbar.fixedHeight + (float)horizontalScrollbar.margin.top;
                    flag2 = true;
                }
            }

            float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
            scrollViewRect = screenRect;
            scrollViewRect.x += scrollPos.x;
            scrollViewRect.y += scrollPos.y;
            scrollViewRect.height -= toolbarHeight;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessEvents () {
        int controlID = GUIUtility.GetControlID(exAtlasEditorHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.ScrollWheel:
            if ( e.control ) {
                curEdit.scale += -e.delta.y * 0.1f;

                Repaint();
                e.Use();
            }
            break;

        case EventType.KeyDown:
            if ( e.keyCode == KeyCode.Backspace ||
                 e.keyCode == KeyCode.Delete ) 
            {
                if ( selectedObjects.Count > 0 ) {
                    AssetDatabase.StartAssetEditing();
                        foreach ( Object obj in selectedObjects ) {
                            exTextureInfo textureInfo = obj as exTextureInfo;
                            if ( textureInfo ) {
                                int i = curEdit.textureInfos.IndexOf(textureInfo);
                                if ( i != -1 ) {
                                    curEdit.textureInfos.RemoveAt(i);
                                    curEdit.needRebuild = true;
                                    AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath(textureInfo) );
                                }
                            }

                            exBitmapFont bitmapFont = obj as exBitmapFont;
                            if ( bitmapFont ) {
                                int i = curEdit.bitmapFonts.IndexOf(bitmapFont);
                                if ( i != -1 ) {
                                    curEdit.bitmapFonts.RemoveAt(i);
                                    curEdit.needRebuild = true;
                                    AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath(bitmapFont) );
                                }
                            }
                        }
                        AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath(curEdit) );
                    AssetDatabase.StopAssetEditing();
                    selectedObjects.Clear();
                    Repaint();
                    e.Use();
                }
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

        foreach ( exTextureInfo textureInfo in curEdit.textureInfos ) {
            if ( textureInfo == null )
                continue;

            Rect textureInfoRect = MapTextureInfo ( atlasRect, textureInfo );
            if ( exGeometryUtility.RectRect_Contains( _rect, textureInfoRect ) != 0 ||
                 exGeometryUtility.RectRect_Intersect( _rect, textureInfoRect ) )
            {
                objects.Add (textureInfo);
            }
        }
        foreach ( exBitmapFont bitmapFont in curEdit.bitmapFonts ) {
            if ( bitmapFont == null )
                continue;

            foreach ( exBitmapFont.CharInfo charInfo in bitmapFont.charInfos ) {
                Rect charInfoRect = MapCharInfo ( atlasRect, charInfo );
                if ( exGeometryUtility.RectRect_Contains( _rect, charInfoRect ) != 0 ||
                     exGeometryUtility.RectRect_Intersect( _rect, charInfoRect ) )
                {
                    objects.Add (bitmapFont);
                    break;
                }
            }
        }

        return objects.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ConfirmRectSelection ( Object _activeObj, Object[] _selectedObjs ) {
        selectedObjects.Clear();
        foreach ( Object obj in _selectedObjs ) {
            if ( obj is exTextureInfo ||
                 obj is exBitmapFont )
            {
                selectedObjects.Add (obj);
            }
        }
        Selection.activeObject = _activeObj;
        Selection.objects = _selectedObjs;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Rect MapTextureInfo ( Rect _atlasRect, exTextureInfo _textureInfo ) {
        Rect rect = new Rect ( _textureInfo.x * curEdit.scale,
                               _textureInfo.y * curEdit.scale,
                               _textureInfo.rotatedWidth * curEdit.scale,
                               _textureInfo.rotatedHeight * curEdit.scale );

        rect.x = _atlasRect.x + rect.x;
        rect.y = _atlasRect.y + _atlasRect.height - rect.y - rect.height;
        rect = exGeometryUtility.Rect_FloorToInt(rect);

        return rect;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Rect MapCharInfo ( Rect _atlasRect, exBitmapFont.CharInfo _charInfo ) {
        Rect rect = new Rect ( _charInfo.x * curEdit.scale,
                               _charInfo.y * curEdit.scale,
                               _charInfo.rotatedWidth * curEdit.scale,
                               _charInfo.rotatedHeight * curEdit.scale );

        rect.x = _atlasRect.x + rect.x;
        rect.y = _atlasRect.y + _atlasRect.height - rect.y - rect.height;
        rect = exGeometryUtility.Rect_FloorToInt(rect);

        return rect;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LayoutAtlasElements () {
        try {
            EditorUtility.DisplayProgressBar( "Layout Elements...", "Layout Elements...", 0.5f  );    

            // sort texture info
            List<exAtlasUtility.Element> elements = exAtlasUtility.GetElementList(curEdit);
            exAtlasUtility.Sort( elements, 
                                 curEdit.sortBy, 
                                 curEdit.sortOrder, 
                                 curEdit.algorithm,
                                 curEdit.allowRotate );

            // pack texture
            exAtlasUtility.Pack ( elements, 
                                  curEdit.algorithm,
                                  curEdit.width,
                                  curEdit.height,
                                  curEdit.actualPadding,
                                  curEdit.allowRotate );
            
            // apply back element to atlas texture info, char info or others
            foreach ( exAtlasUtility.Element el in elements ) {
                el.Apply();
            }
            curEdit.needLayout = false;
            curEdit.needRebuild = true;
            EditorUtility.SetDirty(curEdit);
        }
        finally {
            EditorUtility.ClearProgressBar();
        }
    }
}

