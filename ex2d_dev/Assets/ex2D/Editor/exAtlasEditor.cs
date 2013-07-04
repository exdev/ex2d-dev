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
    int selectIdx = 0;
    List<exTextureInfo> selectedTextureInfos = new List<exTextureInfo>();
    Rect atlasRect = new Rect( 0, 0, 1, 1 );

    // GUI options 
    bool lockCurEdit = false; 
    bool foldoutCanvas = true;
    bool foldoutLayout = true;
    bool foldoutTextureInfo = true;
    bool foldoutBuild = true;

    // GUI states
    Vector2 mouseDownPos = Vector2.zero;
    Rect selectRect = new Rect( 0, 0, 1, 1 );
    bool inRectSelectState = false;
    // bool inDraggingTextureInfoState = false;
    List<Object> importObjects = new List<Object>();
    Object oldSelActiveObject;
    List<Object> oldSelObjects = new List<Object>();

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
        // position = new Rect ( 50, 50, 800, 600 );

        Reset();
        Repaint();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnSelectionChange () {
        if ( lockCurEdit == false ) {
            Edit ( Selection.activeObject );
        }
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
        for ( int i = selectedTextureInfos.Count-1; i >= 0; --i ) {
            exTextureInfo textureInfo = selectedTextureInfos[i];
            if ( textureInfo == null ) {
                selectedTextureInfos.RemoveAt(i);
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
                                                               GUILayout.Width(300), 
                                                               GUILayout.MaxWidth(300)
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
            ProcessEvents();

        EditorGUILayout.EndScrollView();

        curSerializedObject.ApplyModifiedProperties ();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Reset () {
        if ( curEdit != null )
            curSerializedObject = new SerializedObject(curEdit);

        scrollPos = Vector2.zero;
        selectIdx = 0;
        selectedTextureInfos.Clear();
        importObjects.Clear();
        oldSelActiveObject = null;
        oldSelObjects.Clear();

        inRectSelectState = false;
        // inDraggingTextureInfoState = false;
    }

    // ------------------------------------------------------------------ 
    /// \param _obj
    /// Check if the object is valid atlas and open it in atlas editor.
    // ------------------------------------------------------------------ 

    public void Edit ( Object _obj ) {
        if ( _obj is exAtlas && curEdit != _obj ) {
            curEdit = _obj as exAtlas;

            Reset ();
            Repaint ();

            return;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Toolbar () {
        EditorGUILayout.BeginHorizontal ( EditorStyles.toolbar );

            GUILayout.FlexibleSpace();

            // ======================================================== 
            // Select 
            // ======================================================== 

            GUI.enabled = selectedTextureInfos.Count != 0;
            if ( GUILayout.Button( "Select In Project...", EditorStyles.toolbarButton ) ) {
                selectIdx = (selectIdx + 1) % selectedTextureInfos.Count;  
                Selection.objects = selectedTextureInfos.ToArray();
                EditorGUIUtility.PingObject(Selection.objects[selectIdx]);
            }
            GUI.enabled = true;
            EditorGUILayout.Space();

            // ======================================================== 
            // zoom in/out slider 
            // ======================================================== 

            GUILayout.Label ("Zoom");
            EditorGUILayout.Space();
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
            if ( GUI.changed ) {
                GUI.changed = false;
                EditorUtility.SetDirty(curEdit);
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
                GUI.enabled = false; // TODO: have bug in view rect clipping, so temporary disable it
                curEdit.allowRotate = false;
                curEdit.allowRotate = EditorGUILayout.Toggle ( "Allow Rotate", curEdit.allowRotate );
                GUI.enabled = true;

                EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    if ( GUILayout.Button ( "Apply", 
                                            new GUILayoutOption [] {
                                                GUILayout.Width(80)
                                            } ) ) {
                        curEdit.needRebuild = true;
                        LayoutTextureInfos();
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

                if ( curEdit.trimElements != EditorGUILayout.Toggle ( "Trimmed Elements", curEdit.trimElements ) ) {
                    curEdit.trimElements = !curEdit.trimElements;

                    // TODO
                    // foreach ( exAtlas.Element el in curEdit.elements ) {
                    //     curEdit.UpdateElement( el.texture, newTrimElements );
                    // }
                    curEdit.needRebuild = true;
                }

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

            // TODO { 
            // // ======================================================== 
            // // bitmap fonts 
            // // ======================================================== 

            // GUILayout.Space(20);
            // GUILayout.Label ( "Atlas Fonts" );
            // for ( int i = 0; i < curEdit.bitmapFonts.Count; ++i ) {
            //     EditorGUILayout.BeginHorizontal();
            //         exBitmapFont bmfont = curEdit.bitmapFonts[i];
            //         EditorGUILayout.ObjectField( bmfont 
            //                                      , typeof(exBitmapFont) 
            //                                      , false 
            //                                    );
            //         if ( GUILayout.Button("Delete", GUILayout.MaxWidth(80) ) ) {
            //             curEdit.RemoveBitmapFont(bmfont);
            //             AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bmfont));
            //             --i;
            //         }
            //     EditorGUILayout.EndHorizontal();
            // }
            // } TODO end 

        EditorGUILayout.EndVertical();

        // ======================================================== 
        // check gui changes 
        // ======================================================== 

        if ( GUI.changed ) {
            GUI.changed = false;
            EditorUtility.SetDirty(curEdit);
        }
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

                DrawTextureInfo ( MapTextureInfo( new Rect ( 0, 0, atlasRect.width, atlasRect.height ), textureInfo ), textureInfo );
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

                    if ( MapTextureInfo( _rect, textureInfo ).Contains (e.mousePosition) ) {
                        if ( e.command || e.control ) {
                            ToggleSelected(textureInfo);
                        }
                        else {
                            // inDraggingTextureInfoState = true; 
                            if ( selectedTextureInfos.IndexOf(textureInfo) == -1 ) {
                                selectedTextureInfos.Clear();
                                selectedTextureInfos.Add(textureInfo);
                            }
                        }
                        e.Use();
                    }
                }
            }
            break;

        case EventType.DragUpdated:
            if ( _rect.Contains(e.mousePosition) ) {
                // Show a copy icon on the drag
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( o is Texture2D 
                         // || (o is exBitmapFont && (o as exBitmapFont).inAtlas == false) 
                         || exEditorUtility.IsDirectory(o) 
                       ) 
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
                foreach ( Object o in DragAndDrop.objectReferences ) {
                    if ( exEditorUtility.IsDirectory(o) ) {
                        Selection.activeObject = o;

                        // add Texture2D objects
                        Object[] objs = Selection.GetFiltered( typeof(Texture2D), SelectionMode.DeepAssets);
                        importObjects.AddRange(objs);

                        // TODO { 
                        // // add exBitmapFont objects
                        // objs = Selection.GetFiltered( typeof(exBitmapFont), SelectionMode.DeepAssets);
                        // importObjects.AddRange(objs);
                        // } TODO end 
                    }
                    else if ( o is Texture2D /* TODO || o is exBitmapFont*/ ) {
                        importObjects.Add(o);
                    }
                }
                Selection.activeObject = null;


                try {
                    exAtlasUtility.ImportObjects ( curEdit, importObjects.ToArray(), (_progress, _info) => {
                                                      EditorUtility.DisplayProgressBar( "Adding Textures...", _info, _progress );
                                                   } );
                }
                finally {
                    importObjects.Clear();
                    EditorUtility.ClearProgressBar();    
                }
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

    void DrawTextureInfo ( Rect _rect, exTextureInfo _textureInfo ) {
        Color old = GUI.color;
        GUI.color = curEdit.elementBgColor;
            GUI.DrawTexture( _rect, EditorGUIUtility.whiteTexture );
        GUI.color = old;

        Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D>( _textureInfo.rawTextureGUID );
        if ( rawTexture ) {
            GUI.DrawTextureWithTexCoords( _rect, rawTexture,
                                          new Rect( (float)_textureInfo.trim_x/(float)rawTexture.width,
                                                    (float)_textureInfo.trim_y/(float)rawTexture.height,
                                                    (float)_textureInfo.width/(float)rawTexture.width,
                                                    (float)_textureInfo.height/(float)rawTexture.height ) );
        }

        if ( selectedTextureInfos.IndexOf(_textureInfo) != -1 ) {
            exEditorUtility.DrawRectBorder( _rect, curEdit.elementSelectColor );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessEvents () {
        int controlID = GUIUtility.GetControlID(exAtlasEditorHash, FocusType.Passive);
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

                mouseDownPos = e.mousePosition;
                inRectSelectState = true;
                UpdateSelectRect ();
                ConfirmRectSelection();
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

                AssetDatabase.StartAssetEditing();
                    foreach ( exTextureInfo textureInfo in selectedTextureInfos ) {
                        int i = curEdit.textureInfos.IndexOf(textureInfo);
                        if ( i != -1 ) {
                            curEdit.textureInfos.RemoveAt(i);
                            curEdit.needRebuild = true;
                            AssetDatabase.DeleteAsset( AssetDatabase.GetAssetPath(textureInfo) );
                        }
                    }
                AssetDatabase.StopAssetEditing();
                selectedTextureInfos.Clear();
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

    void ToggleSelected ( exTextureInfo _textureInfo ) {
        int i = selectedTextureInfos.IndexOf(_textureInfo);
        if ( i != -1 ) {
            selectedTextureInfos.RemoveAt(i);
        }
        else {
            selectedTextureInfos.Add(_textureInfo);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ConfirmRectSelection () {
        selectedTextureInfos.Clear();

        foreach ( exTextureInfo textureInfo in curEdit.textureInfos ) {
            if ( textureInfo == null )
                continue;

            Rect textureInfoRect = MapTextureInfo ( atlasRect, textureInfo );
            if ( exGeometryUtility.RectRect_Contains( selectRect, textureInfoRect ) != 0 ||
                 exGeometryUtility.RectRect_Intersect( selectRect, textureInfoRect ) )
            {
                selectedTextureInfos.Add (textureInfo);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Rect MapTextureInfo ( Rect _atlasRect, exTextureInfo _textureInfo ) {
        Rect textureInfoRect = new Rect ( _textureInfo.x * curEdit.scale,
                                          _textureInfo.y * curEdit.scale,
                                          _textureInfo.rotatedWidth * curEdit.scale,
                                          _textureInfo.rotatedHeight * curEdit.scale );

        textureInfoRect.x = _atlasRect.x + textureInfoRect.x;
        textureInfoRect.y = _atlasRect.y + _atlasRect.height - textureInfoRect.y - textureInfoRect.height;
        textureInfoRect = exGeometryUtility.Rect_FloorToInt(textureInfoRect);

        return textureInfoRect;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LayoutTextureInfos () {
        try {
            EditorUtility.DisplayProgressBar( "Layout Elements...", "Layout Elements...", 0.5f  );    

            // sort texture info
            curEdit.SortTextureInfos();

            // pack texture
            if ( curEdit.algorithm == exAtlas.Algorithm.Basic ) {
                exAtlasUtility.BasicPack (curEdit);
            }
            else if ( curEdit.algorithm == exAtlas.Algorithm.Tree ) {
                exAtlasUtility.TreePack (curEdit);
            }

            //
            foreach ( exTextureInfo info in curEdit.textureInfos ) {
                EditorUtility.SetDirty(info);
            }
        }
        finally {
            EditorUtility.ClearProgressBar();
        }
    }
}

