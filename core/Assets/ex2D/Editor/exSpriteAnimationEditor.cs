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

	static int exEventInfoViewHash = "exEventInfoView".GetHashCode();
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
    exRectSelection<EventInfo> eventRectSelection = null;
    List<FrameInfo> selectedFrameInfos = new List<FrameInfo>(); // NOTE: selected frame info is sorted by animation frame list
    List<EventInfo> selectedEventInfos = new List<EventInfo>();
    bool activeEventSelection = false;

    //
    bool isPlaying = false; 
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
    float previewScale = 1.0f;

    //
    float playingSeconds = 0.0f;
    int curFrame = 0;
    int insertAt = -1;
    double lastTime = 0.0;

    //
    int eachFrames = 1;
    int totalFrames;
    float totalSeconds;
    float totalWidth;

    //
    Rect timelineRect;
    Rect eventInfoViewRect;
    Rect frameInfoViewRect;

    // handles
    List<Object> draggingObjects = new List<Object>();
    bool inDraggingFrameInfoState = false;
    bool inDraggingNeedleState = false;
    bool inDraggingEventInfoState = false;
    bool inResizeFrameInfoState = false;
    int resizeIdx = -1;
    List<int> oldResizeFrames = new List<int>();
    List<int> oldSelectedEventFrames = new List<int>();
    int draggingEventInfoOldFrame = 0;
    List<Rect> eventInfoRects = new List<Rect>();

    // 
    int infoEditorIndex = 0;

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
        eventRectSelection = new exRectSelection<EventInfo>( PickObject_EventInfo,
                                                             PickRectObjects_EventInfo,
                                                             ConfirmRectSelection_EventInfo,
                                                             UpdateRect_EventInfo );

        UpdateEditObject ();
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

    void Update () {
        if ( isPlaying ) {
            float delta = (float)(EditorApplication.timeSinceStartup - lastTime);
            playingSeconds += delta * curEdit.speed;
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
                }
                previewScale = CalculatePreviewScale( previewSize, previewSize );
                EditorGUILayout.Space();

                //
                Layout_PreviewField ( previewSize, previewSize );
            GUILayout.EndVertical();

            GUILayout.Space(30);

            // frame info edit field or event info edit field
            GUILayout.BeginVertical();
                if ( selectedFrameInfos.Count > 0 )
                    infoEditorIndex = 0;
                else if ( selectedEventInfos.Count > 0 )
                    infoEditorIndex = 1;

                string[] toolbarStrings = new string[] {"FrameInfo List", "EventInfo List"};
                infoEditorIndex = GUILayout.Toolbar( infoEditorIndex, toolbarStrings, new GUILayoutOption[] { 
                                                        GUILayout.Width(200),
                                                     } );

                if ( infoEditorIndex == 0 )
                    FrameInfoEditField();
                else
                    EventInfoEditField();
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

    void UpdateEditObject () {
        if ( lockCurEdit == false || curEdit == null ) {
            exSpriteAnimationClip clip = Selection.activeObject as exSpriteAnimationClip;
            if ( clip != null && clip != curEdit ) {
                Edit (clip);
            }
        }
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Reset () {
        curSerializedObject = null;
        eachFrames = 1;
        curFrame = 0;
        isPlaying = false;
        playingSeconds = 0.0f;

        inDraggingNeedleState = false;
        inDraggingEventInfoState = false;
        inDraggingFrameInfoState = false;
        inResizeFrameInfoState = false;
        resizeIdx = -1;

        selectedFrameInfos.Clear();
        selectedEventInfos.Clear();
        eventInfoRects.Clear();
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
        selectedEventInfos.Clear();
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

    EventInfo PickObject_EventInfo ( Vector2 _position ) {
        EventInfo[] objs = PickRectObjects_EventInfo( new Rect(_position.x-1,_position.y-1,2,2) );
        if ( objs.Length > 0 )
            return objs[0];
        return null;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    EventInfo[] PickRectObjects_EventInfo ( Rect _rect ) {
        List<EventInfo> objects = new List<EventInfo>();

        for ( int i = 0; i < eventInfoRects.Count; ++i ) {
            Rect eventRect = eventInfoRects[i];
            if ( exGeometryUtility.RectRect_Contains( _rect, eventRect ) != 0 ||
                 exGeometryUtility.RectRect_Intersect( _rect, eventRect ) )
            {
                objects.Add(curEdit.eventInfos[i]);
            }
        }

        return objects.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ConfirmRectSelection_EventInfo ( EventInfo _activeObj, EventInfo[] _selectedObjs ) {
        selectedEventInfos.Clear();
        selectedFrameInfos.Clear();

        // NOTE: use this way to make sure selectedEventInfos is sorted by frame.
        foreach ( EventInfo obj in _selectedObjs ) {
            selectedEventInfos.Add (obj);
        }
        selectedEventInfos.Sort ( delegate ( EventInfo _x, EventInfo _y ) {
                                    if ( _x.frame > _y.frame )
                                        return 1;
                                    else if ( _x.frame == _y.frame )
                                        return 0;
                                    else
                                        return -1;
                                  } );
        oldSelectedEventFrames.Clear();
        foreach ( EventInfo ei in selectedEventInfos ) {
            oldSelectedEventFrames.Add(ei.frame);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Rect UpdateRect_EventInfo ( Vector2 _start, Vector2 _end ) {
        Rect result = new Rect(_start.x, _start.y, _end.x - _start.x, _end.y - _start.y);
        if (( result.width < 0f )) {
            result.x += result.width;
            result.width = -result.width;
        }
        result.y = timelineRect.y + eventInfoViewRect.y;
        result.height = eventInfoViewRect.height;
        return result;
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
                                           exEditorUtility.textureAnimationPlay,
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

            if ( GUILayout.Button ( exEditorUtility.textureAnimationPrev, EditorStyles.toolbarButton ) ) {
                curFrame = System.Math.Max ( curFrame - 1, 0 );
            }

            // ======================================================== 
            // next frame 
            // ======================================================== 

            if ( GUILayout.Button ( exEditorUtility.textureAnimationNext, EditorStyles.toolbarButton ) ) {
                curFrame = System.Math.Min ( curFrame + 1, totalFrames );
            }

            GUILayout.Space(30);

            // ======================================================== 
            // add event 
            // ======================================================== 

            if ( GUILayout.Button ( exEditorUtility.textureAddEvent, EditorStyles.toolbarButton ) ) {
                curEdit.AddEmptyEvent( curFrame );
                curEdit.StableSortEvents();
                EditorUtility.SetDirty(curEdit);
            }

            // ======================================================== 
            // Frames & Seconds
            // ======================================================== 

            GUILayout.Space(30);
            EditorGUILayout.SelectableLabel( totalFrames + " frames | " + 
                                             totalSeconds.ToString("f3") + " secs",
                                             new GUILayoutOption [] {
                                                GUILayout.Width(150), 
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

            if ( GUILayout.Button( exEditorUtility.textureHelp, EditorStyles.toolbarButton ) ) {
                Help.BrowseURL("http://ex-dev.com/ex2d/docs/");
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
                            exEditorUtility.GL_DrawLineAA ( x, yStart,
                                                            x, yStart - scalarHeight, 
                                                            Color.gray, 
                                                            1 );
                            exEditorUtility.GL_DrawLineAA ( x, yStart,
                                                            x+1, yStart - scalarHeight,
                                                            Color.gray, 
                                                            1 );
                        }
                        else if ( heightRatio >= 0.5f ) {
                            exEditorUtility.GL_DrawLineAA ( x, yStart,
                                                            x, yStart - scalarHeight * heightRatio,
                                                            Color.gray, 
                                                            1 );
                        }
                        else {
                            exEditorUtility.GL_DrawLineAA ( x, yStart,
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
                        exEditorUtility.GL_DrawLineAA ( x, yStart,
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

                exEditorUtility.GUI_DrawRect ( frameRect, solidColor, borderColor );
                Rect rect = new Rect ( frameRect.x + 5.0f,
                                       frameRect.y + 5.0f,
                                       frameRect.width - 10.0f,
                                       frameRect.height - 10.0f );
                exEditorUtility.GUI_DrawRawTextureInfo ( rect, 
                                                         fi.textureInfo, 
                                                         Color.white,
                                                         exEditorUtility.CalculateTextureInfoScale(rect,fi.textureInfo) );
                curX += frameWidth;
            }

            // draw unused block
            exEditorUtility.GL_DrawLineAA ( 0, yStart + eventViewHeight,
                                            boxWidth, yStart + eventViewHeight,
                                            new Color( 0.8f, 0.8f, 0.8f, 1.0f ),
                                            1 );

            if ( boxWidth > offset + totalWidth ) {
                exEditorUtility.GUI_DrawRect( new Rect ( offset + totalWidth,
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
            exEditorUtility.GUI_DrawRectBorder ( new Rect( _rect.x-1, _rect.y + topHeight - 1, boxWidth+2, boxHeight+2 ), Color.black );
            break;
        }

        // process handles
        EventInfoHandle (_rect);
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
            Texture2D checker = exEditorUtility.textureCheckerboard;
            GUI.DrawTextureWithTexCoords ( _rect, checker, 
                                           new Rect( 0.0f, 0.0f, _rect.width/checker.width, _rect.height/checker.height) );

            // draw preview texture
            curFrame = System.Math.Min ( curFrame, totalFrames );
            int idx = GetFrameInfoIndexByFrame(curFrame);
            if ( idx != -1 ) {
                FrameInfo fi = curEdit.frameInfos[idx];
                exEditorUtility.GUI_DrawRawTextureInfo ( _rect, fi.textureInfo, Color.white, previewScale, true );
            }

            // border
            exEditorUtility.GUI_DrawRect( new Rect(_rect.x-2, _rect.y-2, _rect.width+4, _rect.height+4),
                                          new Color( 1,1,1,0 ), 
                                          EditorStyles.label.normal.textColor );

            break;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    float CalculatePreviewScale ( int _width, int _height ) {
        // get the max width and max height
        float maxWidth = float.MinValue;
        float maxHeight = float.MinValue;
        foreach ( FrameInfo frameInfo in curEdit.frameInfos ) {
            //
            exTextureInfo textureInfo = frameInfo.textureInfo;
            if ( textureInfo == null )
                continue;

            float fiWidth = (float)textureInfo.rawWidth;
            float fiHeight = (float)textureInfo.rawHeight;

            //
            if ( maxWidth < fiWidth ) {
                maxWidth = fiWidth;
            }
            if ( maxHeight < fiHeight ) {
                maxHeight = fiHeight;
            }
        }

        // get the preview scale
        float scale = 1.0f;
        float viewWidth = (float)_width;
        float viewHeight = (float)_height;
        if ( maxWidth > viewWidth && maxHeight > viewHeight ) {
            scale = Mathf.Min( viewWidth / maxWidth, viewHeight / maxHeight );
        }
        else if ( maxWidth > viewWidth ) {
            scale = viewWidth / maxWidth;
        }
        else if ( maxHeight > viewHeight ) {
            scale = viewHeight / maxHeight;
        }
        return scale;
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void FrameInfoEditField () {
        GUILayout.BeginVertical();

        EditorGUILayout.Space();

        // total frames
        EditorGUILayout.LabelField( "Total Frames", totalFrames.ToString() );

        // each frames
        GUILayout.BeginHorizontal();
        GUI.enabled = selectedFrameInfos.Count > 0;
        eachFrames = EditorGUILayout.IntField( "Each Frames", eachFrames, GUILayout.Width(200) );
        eachFrames = System.Math.Max ( eachFrames, 1 );
        if ( GUILayout.Button("Apply", new GUILayoutOption[] { GUILayout.Width(80) }) ) 
        {
            foreach ( FrameInfo fi in selectedFrameInfos ) {
                fi.frames = eachFrames;
            }
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // ======================================================== 
        // frame info each 
        // ======================================================== 

        for ( int i = 0; i < curEdit.frameInfos.Count; ++i ) {
            FrameInfo fi = curEdit.frameInfos[i];

            GUILayout.BeginHorizontal();
                Color old = GUI.backgroundColor;
                if ( selectedFrameInfos.IndexOf(fi) != -1 )
                    GUI.backgroundColor = Color.yellow;

                // texture info
                EditorGUI.BeginChangeCheck();
                exTextureInfo newTextureInfo = EditorGUILayout.ObjectField( "Frame ["+i+"]"
                                                                            , fi.textureInfo
                                                                            , typeof(exTextureInfo)
                                                                            , false 
                                                                            , new GUILayoutOption[] {
                                                                                GUILayout.Width(400)
                                                                            } ) as exTextureInfo;
                if ( EditorGUI.EndChangeCheck() ) {
                    fi.textureInfo = newTextureInfo;
                    EditorUtility.SetDirty(curEdit);
                }

                // frames
                EditorGUI.BeginChangeCheck();
                int newFrame = EditorGUILayout.IntField( "Frames", 
                                                         fi.frames, 
                                                         new GUILayoutOption[] {
                                                            GUILayout.Width(200)
                                                         } );
                if ( EditorGUI.EndChangeCheck() ) {
                    fi.frames = System.Math.Max( newFrame, 1 );
                    EditorUtility.SetDirty(curEdit);
                }

                GUI.backgroundColor = old;
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        GUILayout.EndVertical();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void EventInfoEditField () {
        GUILayout.BeginVertical();

        EditorGUILayout.Space();

        for ( int i = 0; i < curEdit.eventInfos.Count; ++i ) {
            EventInfo ei = curEdit.eventInfos[i];

            GUILayout.BeginHorizontal();
                Color old = GUI.backgroundColor;
                if ( selectedEventInfos.IndexOf(ei) != -1 )
                    GUI.backgroundColor = Color.yellow;

                // lable
                GUILayout.Label( "Event ["+i+"]", new GUILayoutOption[] { GUILayout.Width(100) } );

                // Frame
                EditorGUI.BeginChangeCheck();
                int newFrame = EditorGUILayout.IntField( GUIContent.none, 
                                                         ei.frame, 
                                                         new GUILayoutOption[] {
                                                            GUILayout.Width(80)
                                                         } );
                if ( EditorGUI.EndChangeCheck() ) {
                    ei.frame = System.Math.Min( System.Math.Max( newFrame, 0 ), totalFrames );
                    EditorUtility.SetDirty(curEdit);
                }

                GUILayout.Space(30);

                // Method Name
                GUILayout.Label( "Method", new GUILayoutOption[] { GUILayout.Width(50) } );
                EditorGUI.BeginChangeCheck();
                string newMethodName = EditorGUILayout.TextField ( GUIContent.none, ei.methodName, GUILayout.Width(200) );
                if ( EditorGUI.EndChangeCheck() ) {
                    ei.methodName = newMethodName;
                    EditorUtility.SetDirty(curEdit);
                }


                // param type
                EditorGUI.BeginChangeCheck();
                EventInfo.ParamType newParamType = (EventInfo.ParamType)EditorGUILayout.EnumPopup ( GUIContent.none, 
                                                                                                    ei.paramType,
                                                                                                    GUILayout.Width(60) );
                if ( EditorGUI.EndChangeCheck() ) {
                    ei.paramType = newParamType;
                    EditorUtility.SetDirty(curEdit);
                }

                // parameter
                EditorGUI.BeginChangeCheck();
                switch ( ei.paramType ) {
                case EventInfo.ParamType.None: 
                    GUILayout.Label( "none", new GUILayoutOption[] { GUILayout.Width(100) } );
                    break;
                case EventInfo.ParamType.String: 
                    ei.stringParam = EditorGUILayout.TextField ( GUIContent.none, ei.stringParam, GUILayout.Width(100) );
                    break;
                case EventInfo.ParamType.Float: 
                    ei.floatParam = EditorGUILayout.FloatField ( GUIContent.none, ei.floatParam, GUILayout.Width(100) );
                    break;
                case EventInfo.ParamType.Int: 
                    ei.intParam = EditorGUILayout.IntField ( GUIContent.none, ei.intParam, GUILayout.Width(100)  );
                    break;
                case EventInfo.ParamType.Bool: 
                    ei.boolParam = EditorGUILayout.Toggle ( GUIContent.none, ei.boolParam, GUILayout.Width(100)  );
                    break;
                case EventInfo.ParamType.Object: 
                    ei.objectParam = EditorGUILayout.ObjectField ( GUIContent.none
                                                                   , ei.objectParam
                                                                   , typeof(Object)
                                                                   , true
                                                                   , GUILayout.Width(100) 
                                                                 );
                    break;
                }
                if ( EditorGUI.EndChangeCheck() ) {
                    EditorUtility.SetDirty(curEdit);
                }

                // Send Message Options
                EditorGUI.BeginChangeCheck();
                SendMessageOptions newMsgOptions = (SendMessageOptions)EditorGUILayout.EnumPopup ( GUIContent.none, 
                                                                                                   ei.msgOptions,
                                                                                                   GUILayout.Width(100) );
                if ( EditorGUI.EndChangeCheck() ) {
                    ei.msgOptions = newMsgOptions;
                    EditorUtility.SetDirty(curEdit);
                }

                GUI.backgroundColor = old;
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        GUILayout.EndVertical();
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

    public void EventInfoHandle ( Rect _rect ) {
        // GUI.BeginGroup(_rect);
        eventInfoRects.Clear();
        Rect myEventInfoViewRect = new Rect( _rect.x,
                                             _rect.y + eventInfoViewRect.y,
                                             _rect.width,
                                             eventInfoViewRect.height );

        List<int> selectedIdxList = new List<int>();

        float curX = _rect.x + offset;
        float yStart = _rect.y + eventInfoViewRect.y;
        float markerWidth = exEditorUtility.textureEventMarker.width; 
        float markerHeight = exEditorUtility.textureEventMarker.height + 2.0f; 
        int lastFrame = -1;
        int sameFrameCount = 0;
        float unitFrameWidth = totalWidth/(float)totalFrames;

        for ( int i = 0; i < curEdit.eventInfos.Count; ++i ) {
            EventInfo ei = curEdit.eventInfos[i];
            float x = ((float)ei.frame/(float)totalFrames) * totalWidth;

            if ( selectedEventInfos.IndexOf(ei) != -1 ) {
                selectedIdxList.Add(i);
            }

            // process same frame event drawing
            bool resetToZero = false;
            if ( lastFrame == ei.frame ) {
                ++sameFrameCount;
            }
            else {
                resetToZero = true;
            }
            if ( resetToZero && sameFrameCount > 0 ) {
                float delta = Mathf.Min ( unitFrameWidth/sameFrameCount, markerWidth );
                float eventInfoOffset = delta;
                for ( int j = eventInfoRects.Count-sameFrameCount; j < eventInfoRects.Count; ++j ) {
                    eventInfoRects[j] = new Rect( eventInfoRects[j].x + eventInfoOffset,
                                                  eventInfoRects[j].y,
                                                  eventInfoRects[j].width,
                                                  eventInfoRects[j].height );
                    eventInfoOffset += delta;
                }
                sameFrameCount = 0;
            }
            lastFrame = ei.frame;

            //
            Rect eventRect = new Rect ( curX + x - markerWidth*0.5f, yStart, markerWidth, markerHeight );
            eventInfoRects.Add(eventRect);
        }

        // process same frame event drawing ( after all )
        if ( sameFrameCount > 0 ) {
            float delta = Mathf.Min ( unitFrameWidth/sameFrameCount, markerWidth );
            float eventInfoOffset = delta;
            for ( int j = eventInfoRects.Count-sameFrameCount; j < eventInfoRects.Count; ++j ) {
                eventInfoRects[j] = new Rect( eventInfoRects[j].x + eventInfoOffset,
                                              eventInfoRects[j].y,
                                              eventInfoRects[j].width,
                                              eventInfoRects[j].height );
                eventInfoOffset += delta;
            }
            sameFrameCount = 0;
        }

        //
        int controlID = GUIUtility.GetControlID(exEventInfoViewHash, FocusType.Passive);
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.Repaint:

            Color old = GUI.color; 
            for ( int i = 0; i < eventInfoRects.Count; ++i ) {
                Rect eventInfoRect = eventInfoRects[i];
                if ( eventInfoRect.center.x+0.2f < _rect.xMin ||
                     eventInfoRect.center.x-0.2f > _rect.xMax )
                {
                    continue;
                }

                if ( selectedIdxList.IndexOf(i) != -1 ) {
                    if ( inDraggingEventInfoState )
                        GUI.color = Color.red;
                    else
                        GUI.color = new Color(0.3f, 0.55f, 0.95f, 1f);
                }
                else {
                    GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                }
                GUI.DrawTexture ( eventInfoRect, exEditorUtility.textureEventMarker ); 
            }
            GUI.color = old;

            break;

        case EventType.MouseDown:
            if ( activeEventSelection == false && myEventInfoViewRect.Contains(e.mousePosition) ) {
                if ( e.button == 0 && e.clickCount == 2 ) {
                    float xPos = e.mousePosition.x - _rect.x - offset;
                    int frame = Mathf.RoundToInt( (float)totalFrames * xPos/totalWidth );
                    curEdit.AddEmptyEvent(frame);
                    curEdit.StableSortEvents();
                    EditorUtility.SetDirty (curEdit);

                    Repaint();
                    e.Use();
                }
                else if ( e.button == 0 ) {
                    if ( EditorGUI.actionKey == false && 
                         e.shift == false && 
                         e.button == 0 && 
                         selectedEventInfos.Count > 0 ) 
                    {
                        for ( int i = 0; i < eventInfoRects.Count; ++i ) {
                            Rect eventRect = eventInfoRects[i];
                            eventRect.width = eventRect.width + eventRect.width * 0.5f;
                            if ( selectedIdxList.IndexOf(i) != -1 &&
                                 eventRect.Contains(e.mousePosition) ) 
                            {
                                inDraggingEventInfoState = true;
                                draggingEventInfoOldFrame = curEdit.eventInfos[i].frame;
                                break;
                            }
                        }
                    }

                    //
                    if ( inDraggingEventInfoState ) {
                        GUIUtility.hotControl = controlID;
                        GUIUtility.keyboardControl = controlID;

                        Repaint();
                        e.Use();
                    }
                    else {
                        activeEventSelection = true;
                    }
                }
            } 
            break;

        case EventType.MouseUp:
            if ( e.button == 0 && inDraggingEventInfoState ) {
                GUIUtility.hotControl = 0;
                inDraggingEventInfoState = false;

                curEdit.StableSortEvents();
                EditorUtility.SetDirty(curEdit); 

                for ( int i = 0; i < selectedEventInfos.Count; ++i ) {
                    oldSelectedEventFrames[i] = selectedEventInfos[i].frame;
                }

                Repaint();
                e.Use();
            }
            break;

        case EventType.MouseDrag:
            if ( inDraggingEventInfoState ) {
                float pos = e.mousePosition.x - _rect.x - offset;
                int frame = Mathf.RoundToInt( (float)totalFrames * pos/totalWidth );
                int deltaFrame = frame - draggingEventInfoOldFrame;

                for ( int i = 0; i < selectedEventInfos.Count; ++i ) {
                    EventInfo ei = selectedEventInfos[i];

                    ei.frame = oldSelectedEventFrames[i] + deltaFrame;
                    ei.frame = System.Math.Min( System.Math.Max( ei.frame, 0 ), totalFrames );
                }
                curEdit.StableSortEvents();

                Repaint();
                e.Use();
            }
            break;
        }

        // GUI.EndGroup();

        if ( activeEventSelection ) {
            eventRectSelection.SetSelection(selectedEventInfos.ToArray());
            eventRectSelection.OnGUI();

            if ( GUIUtility.hotControl == 0 ) {
                if ( activeEventSelection ) {
                    activeEventSelection = false;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void FrameInfoHandle ( Rect _rect ) {
        GUI.BeginGroup(_rect);

        // get selected frameinfo rects
        List<Rect> selectedFrameRects = new List<Rect>();
        List<Rect> resizeFrameRects = new List<Rect>();
        List<List<int>> resizeFrameIdxList = new List<List<int>>();

        float curX = offset;
        float yStart = 20.0f + 25.0f + 10.0f;
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
                    exEditorUtility.GUI_DrawRectBorder ( selectedFrameRects[i], Color.yellow );
                }

                if ( insertAt != -1 ) {
                    int frames = GetFrames(0,insertAt);
                    float playOffset = (float)frames/(float)totalFrames * totalWidth;
                    float xPos = offset + playOffset;

                    exEditorUtility.GUI_DrawRect ( new Rect ( xPos-3.0f, 20.0f + 25.0f, 6.0f, frameInfoViewRect.height ), 
                                                   new Color( 0.0f, 0.2f, 1.0f, 0.8f ),
                                                   new Color( 1.0f, 1.0f, 1.0f, 1.0f ) );
                }
            }
            else {
                for ( int i = 0; i < selectedFrameRects.Count; ++i ) {
                    exEditorUtility.GUI_DrawRectBorder ( selectedFrameRects[i], Color.white );
                }

                for ( int i = 0; i < resizeFrameRects.Count; ++i ) {
                    Color borderColor = Color.white;
                    if ( resizeIdx == i ) {
                        continue;
                    }
                    exEditorUtility.GUI_DrawRect ( resizeFrameRects[i], 
                                                   new Color( 1.0f, 0.0f, 0.0f, 1.0f ),
                                                   borderColor );
                }

                if ( inResizeFrameInfoState ) {
                    int frameIdx = resizeFrameIdxList[resizeIdx][0];
                    int frames = GetFrames(0,frameIdx);
                    float playOffset = (float)frames/(float)totalFrames * totalWidth;
                    float xPos = offset + playOffset;

                    exEditorUtility.GUI_DrawRect ( new Rect( xPos, yStart,
                                                             e.mousePosition.x - xPos,
                                                             frameInfoViewRect.height-20.0f ), 
                                                   new Color( 1.0f, 1.0f, 0.0f, 0.2f ),
                                                   Color.yellow );

                    Rect resizeFrameRect = resizeFrameRects[resizeIdx];
                    resizeFrameRect.x = e.mousePosition.x-3.0f;
                    exEditorUtility.GUI_DrawRect ( resizeFrameRect, 
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
                    GUIUtility.hotControl = controlID;
                    GUIUtility.keyboardControl = controlID;

                    Repaint();
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
                float pos = Mathf.Clamp( e.mousePosition.x, 0.0f, totalWidth + offset ) - offset;
                int insertStart = Mathf.FloorToInt( (float)totalFrames * pos/totalWidth );
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
                maxX = maxX + unitFrameWidth * resizeFrameIdxList[resizeIdx].Count;

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

        GUI.EndGroup();
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

                exEditorUtility.GUI_DrawRect( needleRect,
                                              new Color( 1.0f, 0.0f, 0.0f, 0.5f ),
                                              lineColor );

                exEditorUtility.GL_DrawLineAA ( xPos, needleRect.yMax,
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
                float pos = Mathf.Clamp( e.mousePosition.x - _rect.x, 0.0f, totalWidth + offset ) - offset;
                curFrame = Mathf.RoundToInt( (float)totalFrames * pos/totalWidth );

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
                float pos = Mathf.Clamp( e.mousePosition.x - _rect.x, 0.0f, totalWidth + offset ) - offset;
                curFrame = Mathf.RoundToInt( (float)totalFrames * pos/totalWidth );

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
        case EventType.Repaint:
            // show preview textures
            if ( DragAndDrop.visualMode == DragAndDropVisualMode.Copy ) {
                foreach ( Object o in draggingObjects ) {
                    exTextureInfo textureInfo = o as exTextureInfo;
                    if ( textureInfo ) {
                        float size = 100.0f;
                        Rect rect = new Rect ( e.mousePosition.x - size * 0.5f, 
                                               e.mousePosition.y + 30.0f,
                                               size, 
                                               size );
                        exEditorUtility.GUI_DrawRawTextureInfo ( rect,
                                                                 textureInfo,
                                                                 new Color( 1.0f, 1.0f, 1.0f, 0.7f ),
                                                                 exEditorUtility.CalculateTextureInfoScale(rect,textureInfo) );
                    }
                }
            }
            break;

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

                if ( selectedEventInfos.Count > 0 ) {
                    foreach ( EventInfo eventInfo in selectedEventInfos ) {
                        curEdit.RemoveEvent(eventInfo);
                    }
                    selectedEventInfos.Clear();
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
