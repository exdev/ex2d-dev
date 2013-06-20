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
        Repaint();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        if ( curEdit == null ) {
            GUILayout.Space(10);
            GUILayout.Label ( "Please select an Atlas" );
            return;
        }

        curSerializedObject.Update ();

        // toolbar
        Toolbar ();


        // NOTE: we can't use GUILayoutUtility.GetLastRect() here, 
        //       because GetLastRect() will return wrong value when Event.current.type is EventType.Layout
        // Rect rect = GUILayoutUtility.GetLastRect ();
        float toolbarHeight = EditorStyles.toolbar.CalcHeight( GUIContent.none, 0 );
        scrollPos = EditorGUILayout.BeginScrollView ( scrollPos, 
                                                      GUILayout.Width(position.width),
                                                      GUILayout.Height(position.height-toolbarHeight) );

            // atlas
            Object newAtlas = EditorGUILayout.ObjectField( "Atlas"
                                                           , curEdit
                                                           , typeof(exAtlas)
                                                           , false 
                                                           , GUILayout.Width(300)
                                                           , GUILayout.MaxWidth(300)
                                                         );
            if ( newAtlas != curEdit ) 
                Selection.activeObject = newAtlas;
            GUILayout.Space(10);

            //
            EditorGUILayout.BeginHorizontal();
                //
                Settings ();

                //
                GUILayout.Space(40);

                //
                Rect lastRect = GUILayoutUtility.GetLastRect ();  
                AtlasField ( new Rect( lastRect.xMax, lastRect.yMax, curEdit.width * curEdit.scale, curEdit.height * curEdit.scale ) );

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
            GUILayout.Space(5);

            // ======================================================== 
            // zoom in/out slider 
            // ======================================================== 

            float scale = curEdit.scale;
            GUILayout.Label ("Zoom");
            GUILayout.Space(5);
            scale = GUILayout.HorizontalSlider ( scale, 
                                                 0.1f, 
                                                 2.0f, 
                                                 GUILayout.MinWidth(50),
                                                 GUILayout.MaxWidth(150) );
            GUILayout.Space(5);
            scale = EditorGUILayout.FloatField( scale,
                                                EditorStyles.toolbarTextField,
                                                GUILayout.Width(30) );
            scale = Mathf.Clamp( scale, 0.1f, 2.0f );
            scale = Mathf.Round( scale * 100.0f ) / 100.0f;
            if ( GUI.changed ) {
                GUI.changed = false;
                curEdit.scale = scale;
                EditorUtility.SetDirty(curEdit);
            }

            // ======================================================== 
            // Build 
            // ======================================================== 

            GUI.enabled = curEdit.needRebuild;
            GUILayout.Space(5);
            if ( GUILayout.Button( "Build", EditorStyles.toolbarButton ) ) {
                // TODO: build atlas
            }
            GUI.enabled = true;

            // ======================================================== 
            // Lock 
            // ======================================================== 

            lockCurEdit = GUILayout.Toggle ( lockCurEdit, "Lock", EditorStyles.toolbarButton );

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
        int width = -1;
        int height = -1;
        GUIContent content = null;

        EditorGUILayout.BeginVertical( GUILayout.Width(200), GUILayout.MinWidth(200), GUILayout.MaxWidth(200) );

            // ======================================================== 
            // canvas
            // ======================================================== 

            foldoutCanvas = EditorGUILayout.Foldout(foldoutCanvas, "Canvas");
            if ( foldoutCanvas ) {
                EditorGUI.indentLevel++;

                // width and height
                width = EditorGUILayout.IntPopup ( "Width", curEdit.width, sizeTextList, sizeList );
                height = EditorGUILayout.IntPopup ( "Height", curEdit.height, sizeTextList, sizeList );

                // Check if we need to Reset width & height
                if ( width != curEdit.width || height != curEdit.height ) {
                    // TODO:
                }

                // EditorGUILayout.PropertyField (curSerializedObject.FindProperty ("bgColor"), new GUIContent("Background"));
                // EditorGUILayout.PropertyField (curSerializedObject.FindProperty ("showCheckerboard"), new GUIContent("Checkerboard"));
                curEdit.bgColor = EditorGUILayout.ColorField( "Background", curEdit.bgColor );
                curEdit.showCheckerboard = EditorGUILayout.Toggle ( "Checkerboard", curEdit.showCheckerboard );

                EditorGUI.indentLevel--;
            }
            GUILayout.Space(20);

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
                    if ( GUILayout.Button ( "Apply", GUILayout.Width(80) ) ) {
                        curEdit.needRebuild = true;

                        // this is very basic algorithm
                        if ( curEdit.algorithm == exAtlas.Algorithm.Basic ) {
                            exAtlasUtility.BasicPack (curEdit);
                        }
                        else if ( curEdit.algorithm == exAtlas.Algorithm.Tree ) {
                            exAtlasUtility.TreePack (curEdit);
                        }

                        // TODO:
                        // try {
                        //     EditorUtility.DisplayProgressBar( "Layout Elements...", "Layout Elements...", 0.5f  );    
                        //     // register undo
                        //     Undo.RegisterUndo ( curEdit, "Apply.LayoutElements" );
                        //     curEdit.LayoutElements ();
                        //     EditorUtility.ClearProgressBar();
                        // }
                        // catch ( System.Exception ) {
                        //     EditorUtility.ClearProgressBar();
                        //     throw;
                        // }
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUI.indentLevel--;
            }
            GUILayout.Space(20);

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
            GUILayout.Space(20);

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
                    // TODO:
                    // exTextureHelper.SetReadable ( curEdit.texture, curEdit.readable );
                }

                EditorGUI.indentLevel--;
            }
            GUILayout.Space(20);

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

    void AtlasField ( Rect _rect ) {
        GUILayoutUtility.GetRect ( _rect.width+2, _rect.height+2, GUI.skin.box );

        Event e = Event.current;
        switch ( e.type ) {
        case EventType.Repaint:
            Color old = GUI.color;
            GUI.color = curEdit.bgColor;
                // checker box
                if ( curEdit.showCheckerboard ) {
                    Texture2D checker = exEditorUtility.CheckerboardTexture();
                    GUI.DrawTextureWithTexCoords ( _rect, checker, 
                                                   new Rect( 0.0f, 0.0f, _rect.width/(checker.width * curEdit.scale), _rect.height/(checker.height * curEdit.scale)) );
                }
                else {
                    GUI.DrawTexture( _rect, EditorGUIUtility.whiteTexture );
                }

                // border
                exEditorUtility.DrawRect( new Rect ( _rect.x-2, _rect.y-2, _rect.width+4, _rect.height+4 ),
                                          new Color( 1,1,1,0 ), 
                                          Color.white );

                // texture info list 
                foreach ( exTextureInfo textureInfo in curEdit.textureInfos ) {
                    Rect textureInfoRect 
                        = new Rect ( _rect.x + textureInfo.x * curEdit.scale,
                                     _rect.y + textureInfo.y * curEdit.scale,
                                     textureInfo.rotatedWidth * curEdit.scale,
                                     textureInfo.rotatedHeight * curEdit.scale );

                    DrawTextureInfo ( textureInfoRect, textureInfo );
                }
            GUI.color = old;
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

        case  EventType.DragPerform:
            if ( _rect.Contains(e.mousePosition) ) {
                DragAndDrop.AcceptDrag();

                // TODO { 
                // // NOTE: Unity3D have a problem in ImportTextureForAtlas, when a texture is an active selection, 
                // //       no matter how you change your import settings, finally it will apply changes that in Inspector (shows when object selected)
                // oldSelActiveObject = null;
                // oldSelObjects.Clear();
                // foreach ( Object o in Selection.objects ) {
                //     oldSelObjects.Add(o);
                // }
                // oldSelActiveObject = Selection.activeObject;
                // } TODO end 

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

                // TODO { 
                // Selection.activeObject = null;
                // } TODO end 

                exAtlasUtility.ImportObjects ( curEdit, importObjects.ToArray() );
                importObjects.Clear();

                e.Use();
                Repaint();
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
                // ConfirmRectSelection(); // TODO
                Repaint();

                e.Use();
            }
            break;

        case EventType.MouseDrag:
            if ( GUIUtility.hotControl == controlID && inRectSelectState ) {
                UpdateSelectRect ();
                // ConfirmRectSelection(); // TODO
                Repaint();

                e.Use();
            }
            break;

        case EventType.MouseUp:
			if ( GUIUtility.hotControl == controlID ) {
				GUIUtility.hotControl = 0;

                if ( inRectSelectState && e.button == 0 ) {
                    inRectSelectState = false;
                    // ConfirmRectSelection(); // TODO
                    Repaint();
                }

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
}

