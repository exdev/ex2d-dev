// ======================================================================================
// File         : exAtlasInfoEditor.cs
// Author       : Wu Jie 
// Last Change  : 06/15/2013 | 11:40:19 AM | Saturday,June
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
/// the atlas info editor
///
///////////////////////////////////////////////////////////////////////////////

partial class exAtlasInfoEditor : EditorWindow {

    ///////////////////////////////////////////////////////////////////////////////
    // static 
    ///////////////////////////////////////////////////////////////////////////////

    static int[] sizeList = new int[] { 
        32, 64, 128, 256, 512, 1024, 2048, 4096 
    };
    static string[] sizeTextList = new string[] { 
        "32px", "64px", "128px", "256px", "512px", "1024px", "2048px", "4096px" 
    };

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exAtlasInfo curEdit = null;
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

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        name = "Atlas Info Editor";
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
            GUILayout.Label ( "Please select an Atlas Info" );
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

            // atlas info
            Object newAtlasInfo = EditorGUILayout.ObjectField( "Atlas Info"
                                                               , curEdit
                                                               , typeof(exAtlasInfo)
                                                               , false 
                                                               , GUILayout.Width(300)
                                                               , GUILayout.MaxWidth(300)
                                                             );
            if ( newAtlasInfo != curEdit ) 
                Selection.activeObject = newAtlasInfo;
            GUILayout.Space(10);

            //
            EditorGUILayout.BeginHorizontal();
                //
                Settings ();

                //
                GUILayout.Space(40);

                //
                Rect lastRect = GUILayoutUtility.GetLastRect ();  
                AtlasInfo ( new Rect( lastRect.xMax, lastRect.yMax, curEdit.width * curEdit.scale, curEdit.height * curEdit.scale ) );

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
    }

    // ------------------------------------------------------------------ 
    /// \param _obj
    /// Check if the object is valid atlas and open it in atlas editor.
    // ------------------------------------------------------------------ 

    public void Edit ( Object _obj ) {
        if ( _obj is exAtlasInfo && curEdit != _obj ) {
            curEdit = _obj as exAtlasInfo;

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

                curEdit.algorithm = (exAtlasInfo.Algorithm)EditorGUILayout.EnumPopup ( "Algorithm", curEdit.algorithm );
                curEdit.sortBy = (exAtlasInfo.SortBy)EditorGUILayout.EnumPopup ( "Sort By", curEdit.sortBy );
                curEdit.sortOrder = (exAtlasInfo.SortOrder)EditorGUILayout.EnumPopup ( "Sort Order", curEdit.sortOrder );

                // padding
                curEdit.paddingMode = (exAtlasInfo.PaddingMode)EditorGUILayout.EnumPopup("Padding", curEdit.paddingMode);
                EditorGUI.indentLevel++;
                    GUI.enabled = (curEdit.paddingMode == exAtlasInfo.PaddingMode.Custom);
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

                GUI.enabled = (curEdit.paddingMode == exAtlasInfo.PaddingMode.Auto) || (curEdit.actualPadding >= 2);
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
                    // foreach ( exAtlasInfo.Element el in curEdit.elements ) {
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

    void AtlasInfo ( Rect _rect ) {
        if ( Event.current.type == EventType.Repaint ) {
            Texture2D checker = exEditorUtility.CheckerboardTexture();
            GUI.DrawTextureWithTexCoords ( _rect, 
                                           checker, 
                                           new Rect( 0.0f, 0.0f, _rect.width/checker.width, _rect.height/checker.height) );
            exEditorUtility.DrawRect( new Rect ( _rect.x-2, _rect.y-2, _rect.width+4, _rect.height+4 ),
                                      new Color( 1,1,1,0 ), 
                                      Color.white );
        }

        // TODO:

        GUILayoutUtility.GetRect ( _rect.width+2, _rect.height+2, GUI.skin.box );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessEvents () {
        Event e = Event.current;

        // repaint
        if ( e.type == EventType.Repaint ) {
            // draw select rect 
            if ( inRectSelectState && (selectRect.width != 0.0f || selectRect.height != 0.0f) ) {
                exEditorUtility.DrawRect( selectRect, new Color( 0.0f, 0.5f, 1.0f, 0.2f ), new Color( 0.0f, 0.5f, 1.0f, 1.0f ) );
            }
        }

        // mouse down
        if ( e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 1 ) {
            GUIUtility.keyboardControl = -1; // remove any keyboard control

            mouseDownPos = e.mousePosition;
            inRectSelectState = true;
            UpdateSelectRect ();
            // ConfirmRectSelection(); // TODO
            Repaint();

            e.Use();
        }

        // rect select
        if ( inRectSelectState ) {
            if ( e.type == EventType.MouseDrag ) {
                UpdateSelectRect ();
                // ConfirmRectSelection(); // TODO
                Repaint();

                e.Use();
            }
            else if ( e.type == EventType.MouseUp && e.button == 0 ) {
                inRectSelectState = false;
                // ConfirmRectSelection(); // TODO
                Repaint();

                e.Use();
            }
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

