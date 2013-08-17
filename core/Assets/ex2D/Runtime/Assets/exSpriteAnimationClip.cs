// ======================================================================================
// File         : exSpriteAnimationClip.cs
// Author       : Wu Jie 
// Last Change  : 07/17/2013 | 10:19:03 AM | Wednesday,July
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite animation clip asset used in exSpriteAnimation component. 
///
///////////////////////////////////////////////////////////////////////////////

public class exSpriteAnimationClip : ScriptableObject {

    // ------------------------------------------------------------------ 
    /// The action type used when animation stpped
    // ------------------------------------------------------------------ 

    public enum StopAction {
        DoNothing,      ///< do nothing
        DefaultSprite,  ///< set to default sprite when the sprite animation stopped
        Hide,           ///< hide the sprite when the sprite animation stopped
        Destroy         ///< destroy the GameObject the sprite belongs to when the sprite animation stopped
    }

    // ------------------------------------------------------------------ 
    /// The structure to descrip a frame in the sprite animation clip
    // ------------------------------------------------------------------ 

    [System.Serializable]
    public class FrameInfo {
        public exTextureInfo textureInfo; ///< the texture info used in this frame
        public int frames = 1;     ///< frame count

        public FrameInfo (exTextureInfo _textureInfo, int _frames) {
            textureInfo = _textureInfo;
            frames = _frames;
        }
    }

    // ------------------------------------------------------------------ 
    /// The structure to descrip an event in the sprite animation clip
    // ------------------------------------------------------------------ 

    [System.Serializable]
    public class EventInfo {

        public class SearchComparer : IComparer<EventInfo> {
            private static SearchComparer instance_;
            private static int frame;
            public static int BinarySearch (List<EventInfo> _list, int _frame) {
                frame = _frame;
                if (instance_ == null) {
                    instance_ = new SearchComparer();
                }
                return _list.BinarySearch(null, instance_);
            }
            public int Compare (EventInfo _x, EventInfo _y) {
                if (_x == null && _y == null) {
                    exDebug.Assert(false, "Failed to trigger current event because event list contains null event.", false);
                    return 0;
                }
                if (_x != null) {
                    if (_x.frame > frame)
                        return 1;
                    else if (_x.frame < frame)
                        return -1;
                    else
                        return 0;
                }
                else {
                    if (frame > _y.frame)
                        return 1;
                    else if (frame < _y.frame)
                        return -1;
                    else
                        return 0;
                }
            }
        }

        // ------------------------------------------------------------------ 
        /// the type of the parameter
        // ------------------------------------------------------------------ 

        public enum ParamType {
            None,   ///< none
            String, ///< string
            Float,  ///< float
            Int,    ///< int
            Bool,   ///< bool
            Object  ///< object
        }

        public int frame = 0; ///< the frame trigger the event
        public string methodName = ""; ///< the name of method to invoke 
        public ParamType paramType = ParamType.None; ///< the first parameter type 
        public string stringParam = ""; ///< the value of the string parameter
        public float floatParam = 0.0f; ///< the value of the float parameter
        public int intParam = -1; ///< the value of the int parameter
        public bool boolParam { ///< the value of the boolean parameter
            get {
                return intParam != 0;
            }
            set {
                intParam = value ? 1 : 0;
            }
        }
        public Object objectParam = null; ///< the value of the object parameter
        public SendMessageOptions msgOptions = SendMessageOptions.RequireReceiver; ///< the SendMessage option
    }

    public WrapMode wrapMode = WrapMode.Once; ///< default wrap mode
    public StopAction stopAction = StopAction.DoNothing; ///< the default type of action used when the animation stopped 

    // ------------------------------------------------------------------ 
    [SerializeField] protected float frameRate_ = 60.0f;
    /// the sample rate used in this animation clip
    // ------------------------------------------------------------------ 

    public float frameRate {
        get { return frameRate_; }
        set {
            if ( value != frameRate_ ) {
                frameRate_ = Mathf.RoundToInt(Mathf.Max(value,1.0f)); 
            }
        }
    }

    public List<FrameInfo> frameInfos = new List<FrameInfo>(); ///< the list of frame info 
    public List<EventInfo> eventInfos = new List<EventInfo>(); ///< the list of event info

    [System.NonSerialized] private int[] frameInfoFrames; ///< the array of the end frame of each frame info
    [System.NonSerialized] private Dictionary<int, List<EventInfo>> frameToEventDict;

    public float speed = 1.0f; ///< the default speed of the animation clip

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetTotalFrames () {
        int frames = 0;
        for ( int i = 0; i < frameInfos.Count; ++i ) {
            frames += frameInfos[i].frames;
        }
        return frames;
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int[] GetFrameInfoFrames () {
        if (frameInfoFrames == null) {
            frameInfoFrames = new int[frameInfos.Count];
            int totalFrames = 0;
            for (int i = 0; i < frameInfos.Count; ++i) {
                totalFrames += frameInfos[i].frames;
                frameInfoFrames[i] = totalFrames;
            }
        }
        return frameInfoFrames;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public float GetLength () {
        return (float)GetTotalFrames() / frameRate_;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddFrame ( exTextureInfo _info, int _frames = 1 ) {
        InsertFrameInfo ( frameInfos.Count, new FrameInfo(_info,_frames) );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddFrameAt ( int _idx, exTextureInfo _info, int _frames = 1 ) {
        InsertFrameInfo ( _idx, new FrameInfo(_info,_frames) );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RemoveFrame ( FrameInfo _frameInfo ) {
        frameInfos.Remove (_frameInfo);
        // TODO: should move all events behind this frame
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void InsertFrameInfo ( int _idx, FrameInfo _frameInfo ) {
        frameInfos.Insert (_idx,_frameInfo);
        // TODO: should move all events behind this frame
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddEmptyEvent ( int _frame ) {
        EventInfo ei = new EventInfo();
        ei.frame = _frame;
        AddEvent(ei);
    }

    // ------------------------------------------------------------------ 
    /// \param _e the event information wants to add
    /// add an event information
    // ------------------------------------------------------------------ 

    public void AddEvent ( EventInfo _eventInfo ) {
        if ( eventInfos.Count == 0 ) {
            eventInfos.Insert( 0, _eventInfo );
        }
        else if ( eventInfos.Count == 1 ) {
            if ( _eventInfo.frame >= eventInfos[0].frame )
                eventInfos.Insert( 1, _eventInfo );
            else
                eventInfos.Insert( 0, _eventInfo );
        }
        else {
            bool inserted = false;
            EventInfo lastEventInfo = eventInfos[0];

            for ( int i = 1; i < eventInfos.Count; ++i ) {
                EventInfo ei = eventInfos[i];
                if ( _eventInfo.frame >= lastEventInfo.frame && _eventInfo.frame < ei.frame ) {
                    eventInfos.Insert( i, _eventInfo );
                    inserted = true;
                    break;
                }

                lastEventInfo = ei; 
            }

            if ( inserted == false ) {
                eventInfos.Insert( eventInfos.Count, _eventInfo );
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _e the event information wants to remove
    /// remove an event information
    // ------------------------------------------------------------------ 

    public void RemoveEvent ( EventInfo _eventInfo ) {
        eventInfos.Remove( _eventInfo );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

#if UNITY_EDITOR
    // NOTE: I only allow this used in editor, the stable sort is slow and cost heap alloc, it is used in editor to make sure move 
    //       same frame event never flip which is caused if we use unstable-sort algorithm List.Sort
    public void StableSortEvents () {
        List<EventInfo> newList = new List<EventInfo>(); 
        for ( int i = 0; i < eventInfos.Count; ++i ) {
            EventInfo ei = eventInfos[i];
            int insertPos = newList.Count;
            for ( int j = 0; j < newList.Count; ++j ) {
                if ( ei.frame < newList[j].frame ) {
                    insertPos = j;
                    break;
                }
            }
            newList.Insert(insertPos,ei);
        }
        eventInfos = newList;
    }
#endif
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Dictionary<int, List<EventInfo>> GetFrameToEventDict () {
        if (frameToEventDict == null) {
            frameToEventDict = new Dictionary<int, List<EventInfo>>();
            int sameEventFrame = -1;
            List<EventInfo> sameFrameEventList = null;
            for (int i = 0; i < eventInfos.Count; ++i) {
                EventInfo e = eventInfos[i];
                if (e.frame != sameEventFrame) {
                    if (sameFrameEventList != null) {
                        frameToEventDict.Add(sameEventFrame, sameFrameEventList);
                    }
                    sameFrameEventList = new List<EventInfo>();
                    sameEventFrame = e.frame;
                }
                sameFrameEventList.Add(e);
            }
            if (sameFrameEventList != null) {
                frameToEventDict.Add(sameEventFrame, sameFrameEventList);
            }
        }
        return frameToEventDict;
    }
}
