// ======================================================================================
// File         : exSpriteAnimationEditor.cs
// Author       : Wu Jie 
// Last Change  : 07/17/2013 | 10:46:55 AM | Wednesday,July
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

using FrameInfo = exSpriteAnimationClip.FrameInfo;
using EventInfo = exSpriteAnimationClip.EventInfo;

///////////////////////////////////////////////////////////////////////////////
///
/// the sprite animation clip editor
///
///////////////////////////////////////////////////////////////////////////////

partial class exSpriteAnimationEditor : EditorWindow {

	static int exFrameInfoViewHash = "exFrameInfoView".GetHashCode();
	static int exNeedleHandleHash = "exNeedleHandle".GetHashCode();

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exSpriteAnimationClip curEdit = null;
    SerializedObject curSerializedObject = null;

    //
    Vector2 scrollPos = Vector2.zero;
    exRectSelection<FrameInfo> frameRectSelection = null;
    List<FrameInfo> selectedFrameInfos = new List<FrameInfo>(); // NOTE: selected frame info is sorted by animation frame list

    //
    bool isPlaying = false; 
    float previewSpeed = 1.0f;
    bool lockCurEdit = false; 
    float offset = 0.0f;
    float scale_ = 1.0f; ///< the zoom value of the atlas
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
    int previewSize = 256;

    //
    float playingSeconds = 0.0f;
    int curFrame = 0;
    int insertAt = -1;
    double lastTime = 0.0;

    //
    int totalFrames;
    float totalSeconds;
    float totalWidth;

    //
    Rect timelineRect;
    Rect eventInfoViewRect;
    Rect frameInfoViewRect;

    // handles
    bool inDraggingNeedleState = false;
    bool inDraggingFrameInfoState = false;
    bool inResizeFrameInfoState = false;
    List<Object> draggingObjects = new List<Object>();
    int resizeIdx = -1;
    List<int> oldResizeFrames = new List<int>();

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        title = "Sprite Animation Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = false;

        frameRectSelection = new exRectSelection<FrameInfo>( PickObject_FrameInfo,
                                                             PickRectObjects_FrameInfo,
                                                             ConfirmRectSelection_FrameInfo );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnSelectionChange () {
        if ( lockCurEdit == false ) {
            exSpriteAnimationClip clip = Selection.activeObject as exSpriteAnimationClip;
            if ( clip != null && clip != curEdit ) {
                Edit (clip);
                return;
            }
        }
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

    void Update () {
        if ( isPlaying ) {
            float delta = (float)(EditorApplication.timeSinceStartup - lastTime);
            playingSeconds += delta * previewSpeed;
            float curSeconds = exMath.Wrap( playingSeconds, 
                                            totalSeconds, 
                                            curEdit.wrapMode );
            curFrame = Mathf.FloorToInt(curSeconds * curEdit.frameRate);

            //
            if ( curEdit.wrapMode == WrapMode.Once &&
                 curFrame == totalFrames ) 
            {
                isPlaying = false;
                curFrame = 0;
                playingSeconds = 0.0f;
            }

            Repaint();
        }
        lastTime = EditorApplication.timeSinceStartup; 
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        if ( curEdit == null ) {
            EditorGUILayout.Space();
            GUILayout.Label ( "Please select a SpriteAnimationClip" );
            return;
        }

        //
        if ( curSerializedObject == null )
            curSerializedObject = new SerializedObject(curEdit);

        //
        curSerializedObject.Update ();

        // initalize common data
        totalFrames = curEdit.GetTotalFrames();
        totalSeconds = curEdit.GetLength();

        // toolbar
        Toolbar ();

        scrollPos = EditorGUILayout.BeginScrollView ( scrollPos );

            // settings
            Settings ();

            GUILayout.Space(30);

            // timeline field
            EditorGUILayout.BeginHorizontal();
                GUILayoutUtility.GetRect ( 40, 200, 
                                           new GUILayoutOption[] {
                                               GUILayout.ExpandWidth(false),
                                               GUILayout.ExpandHeight(false)
                                           });
                Layout_TimelineField ( (int)position.width - 80, 200 );
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // preview field
            GUILayout.BeginHorizontal();
            GUILayout.Space(40);
            GUILayout.BeginVertical( new GUILayoutOption[] {
                                        GUILayout.Width(300),
                                        GUILayout.ExpandWidth(false),
                                        GUILayout.ExpandHeight(false),
                                     } );

                // preview Size 
                EditorGUI.BeginChangeCheck();
                int newPreviewSize = (int)EditorGUILayout.Slider( "Preview Size", 
                                                                   previewSize, 
                                                                   128.0f, 
                                                                   512.0f,
                                                                   GUILayout.Width(300) );
                if ( EditorGUI.EndChangeCheck() ) {
                    previewSize = newPreviewSize;
                    // TODO { 
                    // CalculatePreviewScale();
                    // } TODO end 
                }
                EditorGUILayout.Space();

                //
                Layout_PreviewField ( previewSize, previewSize );
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            //
            ProcessEvents();
            frameRectSelection.SetSelection(selectedFrameInfos.ToArray());
            frameRectSelection.OnGUI();

        EditorGUILayout.EndScrollView();

        //
        curSerializedObject.ApplyModifiedProperties ();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Reset () {
        curSerializedObject = null;
        curFrame = 0;
        isPlaying = false;
        playingSeconds = 0.0f;
        inDraggingNeedleState = false;
        inDraggingFrameInfoState = false;
        inResizeFrameInfoState = false;
        resizeIdx = -1;

        selectedFrameInfos.Clear();
    }

    // ------------------------------------------------------------------ 
    /// \param _obj
    /// Check if the object is valid atlas and open it in atlas editor.
    // ------------------------------------------------------------------ 

    public void Edit ( exSpriteAnimationClip _clip ) {
        if ( _clip == null )
            return;

        curEdit = _clip;

        Reset ();
        Repaint ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    FrameInfo PickObject_FrameInfo ( Vector2 _position ) {
        FrameInfo[] objs = PickRectObjects_FrameInfo( new Rect(_position.x-1,_position.y-1,2,2) );
        if ( objs.Length > 0 )
            return objs[0];
        return null;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    FrameInfo[] PickRectObjects_FrameInfo ( Rect _rect ) {
        List<FrameInfo> objects = new List<FrameInfo>();

        float curX = frameInfoViewRect.x;

        for ( int i = 0; i < curEdit.frameInfos.Count; ++i ) {
            FrameInfo fi = curEdit.frameInfos[i];
            float frameWidth = ((float)fi.frames/(float)totalFrames) * totalWidth;
            Rect frameRect = new Rect ( curX + timelineRect.x,
                                        frameInfoViewRect.y + timelineRect.y + 10.0f,
                                        frameWidth,
                                        frameInfoViewRect.height - 20.0f );

            if ( exGeometryUtility.RectRect_Contains( _rect, frameRect ) != 0 ||
                 exGeometryUtility.RectRect_Intersect( _rect, frameRect ) )
            {
                objects.Add(fi);
            }

            curX += frameWidth;
        }

        return objects.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ConfirmRectSelection_FrameInfo ( FrameInfo _activeObj, FrameInfo[] _selectedObjs ) {
        selectedFrameInfos.Clear();

        // NOTE: use this way to make sure selectedFrameInfos is sorted by frame.
        foreach ( FrameInfo info in curEdit.frameInfos ) {
            foreach ( FrameInfo obj in _selectedObjs ) {
                if ( info == obj ) {
                    selectedFrameInfos.Add (obj);
                    break;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Toolbar () {
        EditorGUILayout.BeginHorizontal ( EditorStyles.toolbar );

            // ======================================================== 
            // Play 
            // ======================================================== 

            EditorGUI.BeginChangeCheck();
            isPlaying = GUILayout.Toggle ( isPlaying, 
                                           exEditorUtility.AnimationPlayTexture(),
                                           EditorStyles.toolbarButton );
            //
            if ( EditorGUI.EndChangeCheck() ) {
                if ( isPlaying == false ) {
                    curFrame = 0;
                    playingSeconds = 0.0f;
                }
            }

            // ======================================================== 
            // prev frame 
            // ======================================================== 

            if ( GUILayout.Button ( exEditorUtility.AnimationPrevTexture(), EditorStyles.toolbarButton ) ) {
                // TODO { 
                // FrameInfo fi = curEdit.GetFrameInfoBySeconds ( curSeconds, WrapMode.Once );
                // int i = curEdit.frameInfos.IndexOf(fi) - 1;
                // if ( i >= 0  ) {
                //     curSeconds = 0.0f;
                //     for ( int j = 0; j < i; ++j ) {
                //         curSeconds += curEdit.frameInfos[j].length; 
                //     } 
                //     curSeconds += 0.1f/totalWidth * curEdit.length;
                // }
                // } TODO end 
            }

            // ======================================================== 
            // next frame 
            // ======================================================== 

            if ( GUILayout.Button ( exEditorUtility.AnimationNextTexture(), EditorStyles.toolbarButton ) ) {
                // TODO { 
                // FrameInfo fi = curEdit.GetFrameInfoBySeconds ( curSeconds, WrapMode.Once );
                // int i = curEdit.frameInfos.IndexOf(fi) + 1;
                // if ( i < curEdit.frameInfos.Count ) {
                //     curSeconds = 0.0f;
                //     for ( int j = 0; j < i; ++j ) {
                //         curSeconds += curEdit.frameInfos[j].length; 
                //     } 
                //     curSeconds += 0.1f/totalWidth * curEdit.length;
                // }
                // } TODO end 
            }

            // ======================================================== 
            // Frames & Seconds
            // ======================================================== 

            GUILayout.Space(5);
            EditorGUILayout.SelectableLabel( totalFrames + " frames | " + 
                                             totalSeconds.ToString("f3") + " secs",
                                             new GUILayoutOption [] {
                                                GUILayout.Width(150), 
                                                GUILayout.Height(18)
                                             } );

            // ======================================================== 
            // Preview Speed
            // ======================================================== 

            GUILayout.Space(10);
            previewSpeed = EditorGUILayout.FloatField( new GUIContent("Preview Speed"),
                                                       previewSpeed,
                                                       EditorStyles.toolbarTextField,
                                                       new GUILayoutOption [] {
                                                           GUILayout.ExpandWidth(false),
                                                           GUILayout.Width(200),
                                                       } );

            // ======================================================== 
            // Preview Length
            // ======================================================== 

            GUILayout.Space(5);
            EditorGUILayout.SelectableLabel( (totalSeconds / previewSpeed).ToString("f3") + " secs", 
                                             new GUILayoutOption [] {
                                                GUILayout.Width(80),
                                                GUILayout.Height(18)
                                             } );


            GUILayout.FlexibleSpace();

            // ======================================================== 
            // Reset 
            // ======================================================== 

            if ( GUILayout.Button( "Reset", EditorStyles.toolbarButton ) ) {
                offset = 0.0f;
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
                Help.BrowseURL("http://www.ex-dev.com/ex2d/wiki/doku.php?id=manual:sprite_animation_editor");
            }

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Settings () {
        // sprite animation clip
        Object newClip = EditorGUILayout.ObjectField( "Sprite Animation Clip"
                                                      , curEdit
                                                      , typeof(exSpriteAnimationClip)
                                                      , false 
                                                      , new GUILayoutOption[] {
                                                        GUILayout.Width(400), 
                                                      }
                                                    );
        if ( newClip != curEdit ) 
            Selection.activeObject = newClip;

        // speed and length
        GUILayout.BeginHorizontal();
            // speed
            EditorGUI.BeginChangeCheck();
            float newSpeed = EditorGUILayout.FloatField( "Speed", 
                                                         curEdit.speed, 
                                                         new GUILayoutOption [] {
                                                            GUILayout.MaxWidth(250)
                                                         } );
            if ( EditorGUI.EndChangeCheck() ) {
                curEdit.speed = newSpeed;
                EditorUtility.SetDirty(curEdit);
            }
            GUILayout.Space(10);

            // length
            float curLength = totalSeconds/curEdit.speed;
            float newLength = EditorGUILayout.FloatField( "Length", 
                                                          curLength, 
                                                          new GUILayoutOption [] {
                                                            GUILayout.MaxWidth(250)
                                                          } );
            if ( curLength != newLength ) {
                curEdit.speed = totalSeconds/newLength;
                EditorUtility.SetDirty(curEdit);
            }
            GUILayout.Label( "secs" );
        GUILayout.EndHorizontal();

        // Frame Rate
        EditorGUI.BeginChangeCheck();
        float newFrameRate = EditorGUILayout.FloatField( "Frame Rate", 
                                                         curEdit.frameRate, 
                                                         new GUILayoutOption [] {
                                                            GUILayout.MaxWidth(250)
                                                         } );
        if ( EditorGUI.EndChangeCheck() ) {
            curEdit.frameRate = newFrameRate;
            EditorUtility.SetDirty(curEdit);
        }

        // Wrap Mode enum popup
        EditorGUI.BeginChangeCheck();
        WrapMode newWrapMode = (WrapMode)EditorGUILayout.EnumPopup ( "Wrap Mode", 
                                                                     curEdit.wrapMode, 
                                                                     new GUILayoutOption [] {
                                                                        GUILayout.MaxWidth(250)
                                                                     } );
        if ( EditorGUI.EndChangeCheck() ) {
            curEdit.wrapMode = newWrapMode;
        }

        // Anim Stop Action 
        EditorGUI.BeginChangeCheck();
        exSpriteAnimationClip.StopAction newStopAction 
            = (exSpriteAnimationClip.StopAction)EditorGUILayout.EnumPopup ( "Stop Action", 
                                                                            curEdit.stopAction, 
                                                                            new GUILayoutOption [] {
                                                                                GUILayout.MaxWidth(250)
                                                                            } );
        if ( EditorGUI.EndChangeCheck() ) {
            curEdit.stopAction = newStopAction;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Layout_TimelineField ( int _width, int _height ) {
        Rect rect = GUILayoutUtility.GetRect ( _width, _height, 
                                               new GUILayoutOption[] {
                                                   GUILayout.ExpandWidth(false),
                                                   GUILayout.ExpandHeight(false)
                                               });
        TimelineField (rect);
    }
    void TimelineField ( Rect _rect ) {

        // ======================================================== 
        // init varaible
        // ======================================================== 

        int topHeight = 20;
        int botHeight = 20;
        int eventViewHeight = 25;
        int scalarHeight = 14; // 20 for scalar + label, 14 for scalar

        float boxWidth = _rect.width;
        float boxHeight = _rect.height - topHeight - botHeight;

        // constant
        float widthToShowLabel = 60.0f;
        float minWidth = 10.0f;
        float maxWidth = 80.0f;
        float minUnitSecond = 1.0f/curEdit.frameRate;

        // variable
        // int[] lodScales = new int[] { 5, 2, 3, 2 };
        List<int> lodScales = new List<int>();
        int tmpFrameRate = (int)curEdit.frameRate;
        while ( true ) {
            int div = 0;
            if ( tmpFrameRate == 30 ) {
                div = 3;
            }
            else if ( tmpFrameRate % 2 == 0 ) {
                div = 2;
            }
            else if ( tmpFrameRate % 5 == 0 ) {
                div = 5;
            }
            else if ( tmpFrameRate % 3 == 0 ) {
                div = 3;
            }
            else {
                break;
            }
            tmpFrameRate /= div;
            lodScales.Insert(0,div);
        }
        int curIdx = lodScales.Count;
        lodScales.AddRange( new int[] { 
                            5, 2, 3, 2,
                            5, 2, 3, 2,
                            } );

        //
        float unitWidth = 1000.0f; // width for 1 second
        float curUnitSecond = 1.0f;
        float curCellWidth = unitWidth * scale;

        // get curUnitSecond and curIdx
        if ( curCellWidth < minWidth ) {
            while ( curCellWidth < minWidth ) {
                curUnitSecond = curUnitSecond * lodScales[curIdx];
                curCellWidth = curCellWidth * lodScales[curIdx];

                curIdx += 1;
                if ( curIdx >= lodScales.Count ) {
                    curIdx = lodScales.Count - 1;
                    break;
                }
            }
        }
        else if ( curCellWidth > maxWidth ) {
            while ( (curCellWidth > maxWidth) && 
                    (curUnitSecond > minUnitSecond) ) {
                curIdx -= 1;
                if ( curIdx < 0 ) {
                    curIdx = 0;
                    break;
                }

                curUnitSecond = curUnitSecond / lodScales[curIdx];
                curCellWidth = curCellWidth / lodScales[curIdx];
            }
        }

        // check if prev width is good to show
        if ( curUnitSecond > minUnitSecond ) {
            int prev = curIdx - 1;
            if ( prev < 0 )
                prev = 0;
            float prevCellWidth = curCellWidth / lodScales[prev];
            float prevUnitSecond = curUnitSecond / lodScales[prev];
            if ( prevCellWidth >= minWidth ) {
                curIdx = prev;
                curUnitSecond = prevUnitSecond;
                curCellWidth = prevCellWidth;
            }
        }

        // init total width and cell-count
        totalWidth = scale * totalSeconds * unitWidth;
        if ( totalWidth > boxWidth/2.0f ) {
            offset = Mathf.Clamp( offset, boxWidth - totalWidth - boxWidth/2.0f, 0 );
        }
        else {
            offset = 0;
        }

        // get lod interval list
        int[] lodIntervalList = new int[lodScales.Count+1];
        lodIntervalList[curIdx] = 1;
        for ( int i = curIdx-1; i >= 0; --i ) {
            lodIntervalList[i] = lodIntervalList[i+1] / lodScales[i];
        }
        for ( int i = curIdx+1; i < lodScales.Count+1; ++i ) {
            lodIntervalList[i] = lodIntervalList[i-1] * lodScales[i-1];
        }

        // get lod width list
        float[] lodWidthList = new float[lodScales.Count+1];
        lodWidthList[curIdx] = curCellWidth;
        for ( int i = curIdx-1; i >= 0; --i ) {
            lodWidthList[i] = lodWidthList[i+1] / lodScales[i];
        }
        for ( int i = curIdx+1; i < lodScales.Count+1; ++i ) {
            lodWidthList[i] = lodWidthList[i-1] * lodScales[i-1];
        }

        // get idx from
        int idxFrom = curIdx;
        for ( int i = 0; i < lodScales.Count+1; ++i ) {
            if ( lodWidthList[i] > maxWidth ) {
                idxFrom = i;
                break;
            }
        }

        // calc event info view and frame info view rect
        timelineRect = _rect;
        eventInfoViewRect = new Rect ( offset, topHeight, totalWidth, eventViewHeight );
        frameInfoViewRect = new Rect ( offset, topHeight + eventViewHeight, totalWidth, boxHeight - eventViewHeight );

        // ======================================================== 
        GUI.BeginGroup(_rect);
        // ======================================================== 

        Event e = Event.current;
        switch ( e.type ) {
        case EventType.Repaint:

            float xStart = 0;
            float yStart = topHeight;

            // draw the scalar
            // NOTE: +50 here can avoid us clip text so early 
            int iStartFrom = Mathf.CeilToInt( -(offset + 50.0f)/curCellWidth );
            int cellCount = Mathf.CeilToInt( (boxWidth - offset)/curCellWidth );
            for ( int i = iStartFrom; i < cellCount; ++i ) {
                float x = xStart + offset + i * curCellWidth + 1;
                int idx = idxFrom;

                while ( idx >= 0 ) {
                    if ( i % lodIntervalList[idx] == 0 ) {
                        float heightRatio = lodWidthList[idx] / maxWidth;

                        // draw scalar
                        if ( heightRatio >= 1.0f ) {
                            exEditorUtility.DrawLine ( x, yStart,
                                                       x, yStart - scalarHeight, 
                                                       Color.gray, 
                                                       1 );
                            exEditorUtility.DrawLine ( x, yStart,
                                                       x+1, yStart - scalarHeight,
                                                       Color.gray, 
                                                       1 );
                        }
                        else if ( heightRatio >= 0.5f ) {
                            exEditorUtility.DrawLine ( x, yStart,
                                                       x, yStart - scalarHeight * heightRatio,
                                                       Color.gray, 
                                                       1 );
                        }
                        else {
                            exEditorUtility.DrawLine ( x, yStart,
                                                       x, yStart - scalarHeight * heightRatio,
                                                       Color.gray, 
                                                       1 );
                        }

                        // draw lable
                        if ( lodWidthList[idx] >= widthToShowLabel ) {
                            GUI.Label ( new Rect( x + 4.0f, yStart - 22, 50, 20 ), 
                                        ToString_FramesInSeconds(i*curUnitSecond,curEdit.frameRate) );
                        }

                        //
                        break;
                    }
                    --idx;
                }
            }

            // draw background
            Color old = GUI.color;
            GUI.color = Color.gray;
                GUI.DrawTexture( new Rect ( xStart, yStart, boxWidth, boxHeight ), EditorGUIUtility.whiteTexture );
            GUI.color = old;

            // draw event info view background (before in-box scalar) 
            old = GUI.color;
            GUI.color = new Color ( 0.65f, 0.65f, 0.65f, 1.0f );
                GUI.DrawTexture( eventInfoViewRect, EditorGUIUtility.whiteTexture );
            GUI.color = old;

            // draw in-box scalar
            for ( int i = iStartFrom; i < cellCount; ++i ) {
                float x = offset + i * curCellWidth + 1;
                int idx = idxFrom;

                while ( idx >= 0) {
                    if ( i % lodIntervalList[idx] == 0 ) {
                        float ratio = lodWidthList[idx] / maxWidth;
                        exEditorUtility.DrawLine ( x, yStart,
                                                   x, yStart + boxHeight, 
                                                   new Color( 0.4f, 0.4f, 0.4f, ratio - 0.3f ),
                                                   1 );
                        break;
                    }
                    --idx;
                }
            }

            // draw frame infos
            float curX = offset;
            for ( int i = 0; i < curEdit.frameInfos.Count; ++i ) {
                FrameInfo fi = curEdit.frameInfos[i];
                float frameWidth = ((float)fi.frames/(float)totalFrames) * totalWidth;
                Rect frameRect = new Rect ( curX-1, yStart + eventViewHeight + 10.0f, frameWidth+1, (boxHeight - eventViewHeight) - 20.0f );

                Color borderColor = Color.black;
                Color solidColor = new Color( 1.0f, 0.0f, 0.85f, 0.2f );
                if ( selectedFrameInfos.IndexOf(fi) != -1 ) {
                    solidColor = new Color( 0.2f, 0.85f, 0.0f, 0.2f );
                }

                exEditorUtility.DrawRect ( frameRect, solidColor, borderColor );
                DrawTextureInfo ( new Rect ( frameRect.x + 5.0f,
                                             frameRect.y + 5.0f,
                                             frameRect.width - 10.0f,
                                             frameRect.height - 10.0f ), 
                                  fi.textureInfo, 
                                  Color.white );
                curX += frameWidth;
            }

            // draw unused block
            exEditorUtility.DrawLine ( 0, yStart + eventViewHeight,
                                       boxWidth, yStart + eventViewHeight,
                                       new Color( 0.8f, 0.8f, 0.8f, 1.0f ),
                                       1 );

            if ( boxWidth > offset + totalWidth ) {
                exEditorUtility.DrawRect( new Rect ( offset + totalWidth,
                                                     yStart,
                                                     boxWidth - (offset + totalWidth),
                                                     boxHeight ),
                                          new Color(0.7f, 0.7f, 0.7f, 1.0f),
                                          new Color(0.8f, 0.8f, 0.8f, 0.0f) );
            }
            break;

        case EventType.ScrollWheel:
            if ( new Rect(0, 0, _rect.width, _rect.height).Contains(e.mousePosition) ) {
                float s = 1000.0f;
                while ( (scale/s) < 1.0f || (scale/s) > 10.0f ) {
                    s /= 10.0f;
                }
                scale -= e.delta.y * s * 0.05f;
                Repaint();

                e.Use();
            }
            break;

        case EventType.MouseDrag:
            if ( new Rect(0, 0, _rect.width, _rect.height).Contains(e.mousePosition) &&
                 e.button == 1 ) 
            {
                offset += e.delta.x;
                Repaint();

                e.Use();
            }
            break;

        case EventType.DragUpdated:
            // Show a copy icon on the drag
            foreach ( Object o in DragAndDrop.objectReferences ) {
                if ( o is exTextureInfo /* TODO: || exEditorUtility.IsDirectory(o)*/ ) {
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
            break;

        case EventType.DragExited:
            draggingObjects.Clear();
            Repaint();
            break;

        case EventType.DragPerform:
            DragAndDrop.AcceptDrag();

            draggingObjects.Sort ( delegate ( Object _a, Object _b ) { return string.Compare ( _a.name, _b.name ); } );
            foreach ( Object o in draggingObjects ) {
                exTextureInfo info = o as exTextureInfo;
                if ( info ) {
                    curEdit.AddFrame(info);
                }
            }
            EditorUtility.SetDirty(curEdit);

            Repaint();
            e.Use();
            break;
        }

        // ======================================================== 
        GUI.EndGroup();
        // ======================================================== 

        // draw border
        switch ( e.type ) {
        case EventType.Repaint:
            exEditorUtility.DrawRectBorder ( new Rect( _rect.x-1, _rect.y + topHeight - 1, boxWidth+2, boxHeight+2 ), Color.black );

            // show preview textures
            if ( DragAndDrop.visualMode == DragAndDropVisualMode.Copy ) {
                foreach ( Object o in draggingObjects ) {
                    exTextureInfo textureInfo = o as exTextureInfo;
                    if ( textureInfo ) {
                        float size = 100.0f;
                        DrawTextureInfo ( new Rect ( e.mousePosition.x - size * 0.5f, 
                                                     e.mousePosition.y + 30.0f,
                                                     size, 
                                                     size ),
                                          textureInfo,
                                          new Color( 1.0f, 1.0f, 1.0f, 0.7f )
                                        );
                    }
                }
            }

            break;
        }

        // process handles
        FrameInfoHandle (_rect);
        NeedleHandle (_rect);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Layout_PreviewField ( int _width, int _height ) {
        Rect rect = GUILayoutUtility.GetRect ( _width, _height, 
                                               new GUILayoutOption[] {
                                                   GUILayout.ExpandWidth(false),
                                                   GUILayout.ExpandHeight(false)
                                               });
        PreviewField (rect);
    }
    void PreviewField ( Rect _rect ) {
        Event e = Event.current;

        switch ( e.type ) {
        case EventType.Repaint:
            // checker box
            Texture2D checker = exEditorUtility.CheckerboardTexture();
            GUI.DrawTextureWithTexCoords ( _rect, checker, 
                                           new Rect( 0.0f, 0.0f, _rect.width/checker.width, _rect.height/checker.height) );

            // draw preview texture
            curFrame = System.Math.Min ( curFrame, totalFrames );
            int idx = GetFrameInfoIndexByFrame(curFrame);
            if ( idx != -1 ) {
                FrameInfo fi = curEdit.frameInfos[idx];
                DrawTextureInfo ( _rect, fi.textureInfo, Color.white );
            }

            // border
            exEditorUtility.DrawRect( new Rect(_rect.x-2, _rect.y-2, _rect.width+4, _rect.height+4),
                                      new Color( 1,1,1,0 ), 
                                      EditorStyles.label.normal.textColor );

            break;
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _seconds input seoncds
    /// \param _sampleRate input sample rate
    /// \return the frame in string
    /// get seconds in string from input seconds
    // ------------------------------------------------------------------ 

    public string ToString_FramesInSeconds ( float _seconds, float _sampleRate ) {
        int sec1 = Mathf.FloorToInt(_seconds);
        int sec2 = Mathf.FloorToInt((_seconds - sec1) * _sampleRate % _sampleRate);

        int d = 1;
        int dd = 10;
        while ( _sampleRate / dd >= 1.0f ) {
            d += 1;
            dd *= 10;
        }
        return sec1 + ":" + sec2.ToString("d"+d);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void DrawTextureInfo ( Rect _rect, exTextureInfo _textureInfo, Color _color ) {
        if ( _textureInfo == null )
            return;

        float scale = 1.0f;
        float width = _textureInfo.width;
        float height = _textureInfo.height;

        // confirm the scale, width and height
        if ( width > _rect.width && height > _rect.height ) {
            scale = Mathf.Min( _rect.width / width, 
                               _rect.height / height );
        }
        else if ( width > _rect.width ) {
            scale = _rect.width / width;
        }
        else if ( height > _rect.height ) {
            scale = _rect.height / height;
        }
        width = width * scale;
        height = height * scale;

        //
        Rect pos = new Rect( _rect.center.x - width * 0.5f,
                              _rect.center.y - height * 0.5f, 
                              width, 
                              height );

        // draw the texture
        Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D>( _textureInfo.rawTextureGUID );
        if ( rawTexture ) {
            Color old = GUI.color;
            GUI.color = _color;
            GUI.DrawTextureWithTexCoords( pos, rawTexture,
                                          new Rect( (float)_textureInfo.trim_x /(float)rawTexture.width,
                                                    (float)_textureInfo.trim_y /(float)rawTexture.height,
                                                    (float)_textureInfo.width  /(float)rawTexture.width,
                                                    (float)_textureInfo.height /(float)rawTexture.height ) );
            GUI.color = old;
        }

        // DEBUG { 
        // exEditorUtility.DrawRectBorder ( _rect, Color.white );
        // } DEBUG end 
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FrameInfoHandle ( Rect _rect ) {

        // get selected frameinfo rects
        List<Rect> selectedFrameRects = new List<Rect>();
        List<Rect> resizeFrameRects = new List<Rect>();
        List<List<int>> resizeFrameIdxList = new List<List<int>>();

        float curX = _rect.x + offset;
        float yStart = _rect.y + 20.0f + 25.0f + 10.0f;
        int lastSelectFrameIdx = -1;
        List<int> curResizeFrames = new List<int>();

        // intialize selectedFrameRects, resizeFrameRects and resizeFrameIdxList
        for ( int i = 0; i < curEdit.frameInfos.Count; ++i ) {
            FrameInfo fi = curEdit.frameInfos[i];
            float frameWidth = ((float)fi.frames/(float)totalFrames) * totalWidth;
            Rect frameRect = new Rect ( curX-1, yStart, frameWidth+1, frameInfoViewRect.height - 20.0f );
            bool addResizeRect = false;

            if ( selectedFrameInfos.IndexOf(fi) != -1 ) {
                selectedFrameRects.Add(frameRect);
                lastSelectFrameIdx = i;
                curResizeFrames.Add(i);

                if ( i == curEdit.frameInfos.Count-1 ) {
                    addResizeRect = true;
                }
            }
            else {
                if ( (lastSelectFrameIdx != -1 && i - lastSelectFrameIdx == 1) ) {
                    addResizeRect = true;
                }
            }

            if ( addResizeRect ) {
                Rect lastFrameRect = selectedFrameRects[selectedFrameRects.Count-1];
                resizeFrameRects.Add ( new Rect( lastFrameRect.xMax-4.0f, lastFrameRect.y + 5.0f, 8.0f, lastFrameRect.height-10.0f ) );

                //
                List<int> copy = new List<int>();
                foreach ( int idx in curResizeFrames ) {
                    copy.Add(idx);
                }
                resizeFrameIdxList.Add(copy);
                curResizeFrames.Clear();
            }

            curX += frameWidth;
        }

        //
        int controlID = GUIUtility.GetControlID(exFrameInfoViewHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.Repaint:
            if ( inDraggingFrameInfoState ) {
                for ( int i = 0; i < selectedFrameRects.Count; ++i ) {
                    exEditorUtility.DrawRectBorder ( selectedFrameRects[i], Color.yellow );
                }

                if ( insertAt != -1 ) {
                    int frames = GetFrames(0,insertAt);
                    float playOffset = (float)frames/(float)totalFrames * totalWidth;
                    float xPos = _rect.x + offset + playOffset;

                    exEditorUtility.DrawRect ( new Rect ( xPos-3.0f, _rect.y + 20.0f + 25.0f, 6.0f, frameInfoViewRect.height ), 
                                               new Color( 0.0f, 0.2f, 1.0f, 0.8f ),
                                               new Color( 1.0f, 1.0f, 1.0f, 1.0f ) );
                }
            }
            else {
                for ( int i = 0; i < selectedFrameRects.Count; ++i ) {
                    exEditorUtility.DrawRectBorder ( selectedFrameRects[i], Color.white );
                }

                for ( int i = 0; i < resizeFrameRects.Count; ++i ) {
                    Color borderColor = Color.white;
                    if ( resizeIdx == i ) {
                        continue;
                    }
                    exEditorUtility.DrawRect ( resizeFrameRects[i], 
                                               new Color( 1.0f, 0.0f, 0.0f, 1.0f ),
                                               borderColor );
                }

                if ( inResizeFrameInfoState ) {
                    int frameIdx = resizeFrameIdxList[resizeIdx][0];
                    int frames = GetFrames(0,frameIdx);
                    float playOffset = (float)frames/(float)totalFrames * totalWidth;
                    float xPos = _rect.x + offset + playOffset;

                    exEditorUtility.DrawRect ( new Rect( xPos, yStart,
                                                         e.mousePosition.x - xPos,
                                                         frameInfoViewRect.height-20.0f ), 
                                               new Color( 1.0f, 1.0f, 0.0f, 0.2f ),
                                               Color.yellow );

                    Rect resizeFrameRect = resizeFrameRects[resizeIdx];
                    resizeFrameRect.x = e.mousePosition.x-3.0f;
                    exEditorUtility.DrawRect ( resizeFrameRect, 
                                               new Color( 1.0f, 0.0f, 0.0f, 1.0f ),
                                               Color.yellow );
                }
            }
            break;

        case EventType.MouseDown:
            if ( EditorGUI.actionKey == false && 
                 e.shift == false && 
                 e.button == 0 && 
                 selectedFrameRects.Count > 0 ) 
            {
                GUIUtility.hotControl = controlID;
                GUIUtility.keyboardControl = controlID;

                for ( int i = 0; i < resizeFrameRects.Count; ++i ) {
                    if ( resizeFrameRects[i].Contains(e.mousePosition) ) {
                        oldResizeFrames.Clear();
                        inResizeFrameInfoState = true;
                        resizeIdx = i;
                        foreach ( int idx in resizeFrameIdxList[resizeIdx] ) {
                            oldResizeFrames.Add(curEdit.frameInfos[idx].frames);
                        }

                        break;
                    }
                }

                if ( inResizeFrameInfoState == false ) {
                    for ( int i = 0; i < selectedFrameRects.Count; ++i ) {
                        if ( selectedFrameRects[i].Contains(e.mousePosition) ) {
                            inDraggingFrameInfoState = true;
                            break;
                        }
                    }
                }

                if ( inDraggingFrameInfoState || inResizeFrameInfoState ) {
                    e.Use();
                }
            }
            break;

        case EventType.MouseUp:
            if ( e.button == 0 ) {
                if ( inDraggingFrameInfoState ) {
                    GUIUtility.hotControl = 0;

                    if ( insertAt != -1 ) {
                        //
                        int insertStart = 0;
                        for ( int i = 0; i < insertAt; ++i ) {
                            if ( selectedFrameInfos.IndexOf( curEdit.frameInfos[i] ) == -1 ) {
                                ++insertStart;
                            }
                        }
                        foreach ( FrameInfo fi in selectedFrameInfos ) {
                            curEdit.frameInfos.Remove(fi);
                        }

                        //
                        foreach ( FrameInfo fi in selectedFrameInfos ) {
                            curEdit.InsertFrameInfo( insertStart, fi );
                            ++insertStart;
                        }
                    }

                    inDraggingFrameInfoState = false;
                    insertAt = -1;
                    EditorUtility.SetDirty(curEdit);

                    Repaint();
                    e.Use();
                }
                else if ( inResizeFrameInfoState ) {
                    GUIUtility.hotControl = 0;

                    inResizeFrameInfoState = false;
                    resizeIdx = -1;
                    EditorUtility.SetDirty(curEdit);

                    Repaint();
                    e.Use();
                }
            }
            break;

        case EventType.MouseDrag:
            if ( inDraggingFrameInfoState ) {
                float pos = Mathf.Clamp( e.mousePosition.x - _rect.x, 0.0f, totalWidth + offset );
                int insertStart = Mathf.FloorToInt( (float)totalFrames * (pos - offset)/totalWidth );
                if ( insertStart < totalFrames ) {
                    for ( int i = insertStart; i >= 0; --i ) {
                        int frameIdx = GetFrameInfoIndexByFrame(i);
                        FrameInfo fi = curEdit.frameInfos[frameIdx];
                        if ( selectedFrameInfos.IndexOf(fi) == -1 ) {
                            break;
                        }
                        insertStart = i;
                    }

                    insertAt = GetFrameInfoIndexByFrame(insertStart);
                }
                else {
                    insertAt = insertStart;
                }
                insertAt = Mathf.Min ( insertAt, curEdit.frameInfos.Count );

                Repaint();
                e.Use();
            }
            else if ( inResizeFrameInfoState ) {
                float unitFrameWidth = totalWidth/(float)totalFrames;
                Rect resizeFrameRect = resizeFrameRects[resizeIdx];
                float minX = resizeFrameRect.center.x;
                float maxX = resizeFrameRect.center.x;
                
                foreach ( int idx in resizeFrameIdxList[resizeIdx] ) {
                    if ( curEdit.frameInfos[idx].frames > 1 )
                        minX -= unitFrameWidth;
                } 

                foreach ( int idx in resizeFrameIdxList[resizeIdx] ) {
                    maxX += unitFrameWidth;
                }

                //
                if ( e.mousePosition.x >= maxX ) {
                    foreach ( int idx in resizeFrameIdxList[resizeIdx] ) {
                        curEdit.frameInfos[idx].frames += 1;
                    }
                }
                else if ( e.mousePosition.x <= minX ) {
                    foreach ( int idx in resizeFrameIdxList[resizeIdx] ) {
                        if ( curEdit.frameInfos[idx].frames > 1 )
                            curEdit.frameInfos[idx].frames -= 1;
                    }
                }

                Repaint();
                e.Use();
            }
            break;

        case EventType.KeyDown:
            if ( e.keyCode == KeyCode.Escape ) {
                if ( inDraggingFrameInfoState ) {
                    inDraggingFrameInfoState = false;
                    insertAt = -1;

                    Repaint();
                    e.Use();
                }
                else if ( inResizeFrameInfoState ) {
                    // recover to original size
                    for ( int i = 0; i < resizeFrameIdxList[resizeIdx].Count; ++i ) {
                        int idx = resizeFrameIdxList[resizeIdx][i];
                        curEdit.frameInfos[idx].frames = oldResizeFrames[i];
                    } 

                    inResizeFrameInfoState = false;
                    resizeIdx = -1;

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

    public void NeedleHandle ( Rect _rect ) {
        curFrame = Mathf.Clamp ( curFrame, 0, totalFrames );

        float playOffset = (float)curFrame/(float)totalFrames * totalWidth;
        float xPos = _rect.x + offset + playOffset;
        float yMax = _rect.yMax - 20.0f;
        Rect needleRect = new Rect ( xPos-3.0f, _rect.y - 20.0f, 6.0f, 30.0f );
        Rect needleValidRect = new Rect ( _rect.x-3.0f, _rect.y - 20.0f, totalWidth + 6.0f, 40.0f );

        int controlID = GUIUtility.GetControlID(exNeedleHandleHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.Repaint:
            if ( xPos >= _rect.x && xPos <= _rect.xMax ) {
                Color lineColor = Color.red;
                if ( inDraggingNeedleState ) {
                    lineColor = Color.yellow;
                }

                exEditorUtility.DrawRect( needleRect,
                                          new Color( 1.0f, 0.0f, 0.0f, 0.5f ),
                                          lineColor );

                exEditorUtility.DrawLine ( xPos, needleRect.yMax,
                                           xPos, yMax,
                                          lineColor,
                                          1 );
                // show label
                GUI.Label ( new Rect( xPos - 15.0f, yMax, 30.0f, 20.0f ),
                            ToString_FramesInSeconds( (float)curFrame/(float)curEdit.frameRate, curEdit.frameRate ) );
            }
            break;

        case EventType.MouseDown:
            if ( needleValidRect.Contains( e.mousePosition ) && e.button == 0 ) {
                GUIUtility.hotControl = controlID;
                GUIUtility.keyboardControl = controlID;

                inDraggingNeedleState = true;
                float pos = Mathf.Clamp( e.mousePosition.x - _rect.x, 0.0f, totalWidth + offset );
                curFrame = Mathf.RoundToInt( (float)totalFrames * (pos - offset)/totalWidth );

                Repaint();
                e.Use();
            }
            break;

        case EventType.MouseUp:
            if ( inDraggingNeedleState && e.button == 0 ) {
                GUIUtility.hotControl = 0;
                inDraggingNeedleState = false;

                Repaint();
                e.Use();
            }
            break;

        case EventType.MouseDrag:
            if ( inDraggingNeedleState ) {
                float pos = Mathf.Clamp( e.mousePosition.x - _rect.x, 0.0f, totalWidth + offset );
                curFrame = Mathf.RoundToInt( (float)totalFrames * (pos - offset)/totalWidth );

                Repaint();
                e.Use();
            }
            break;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessEvents () {
        Event e = Event.current;
        switch ( e.type ) {
        case EventType.KeyDown:
            if ( e.keyCode == KeyCode.Backspace ||
                 e.keyCode == KeyCode.Delete ) 
            {
                if ( selectedFrameInfos.Count > 0 ) {
                    foreach ( FrameInfo frameInfo in selectedFrameInfos ) {
                        curEdit.RemoveFrame(frameInfo);
                    }
                    selectedFrameInfos.Clear();
                    EditorUtility.SetDirty(curEdit);

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

    int GetFrameInfoIndexByFrame ( int _frame ) {
        int tmp = 0;
        for ( int i = 0; i < curEdit.frameInfos.Count; ++i ) {
            FrameInfo fi = curEdit.frameInfos[i];
            tmp += fi.frames;
            if ( _frame < tmp )
                return i;
        }
        return curEdit.frameInfos.Count-1; 
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    int GetFrames ( int _start, int _end ) {
        int tmp = 0;
        for ( int i = _start; i < _end; ++i ) {
            FrameInfo fi = curEdit.frameInfos[i];
            tmp += fi.frames;
        }
        return tmp;
    }
}
