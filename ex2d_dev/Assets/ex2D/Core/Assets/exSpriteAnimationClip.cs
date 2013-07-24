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
            public static EventInfo BinarySearch (List<EventInfo> _list, int _frame) {
                frame = _frame;
                if (instance_ == null) {
                    instance_ = new SearchComparer();
                }
                int index = _list.BinarySearch(null, instance_);
                if (index >= 0) {
                    return _list[index];
                }
                else {
                    return null;
                }
            }
            public int Compare (EventInfo _x, EventInfo _y) {
                if (_x == null && _y == null) {
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
            NONE,   ///< none
            STRING, ///< string
            FLOAT,  ///< float
            INT,    ///< int
            BOOL,   ///< bool
            OBJECT  ///< object
        }

        public int frame = 0; ///< the frame trigger the event
        public string methodName = ""; ///< the name of method to invoke 
        public ParamType paramType = ParamType.NONE; ///< the first parameter type 
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

        // ------------------------------------------------------------------ 
        /// Calls the method named methodName on every Component target game object.
        // ------------------------------------------------------------------ 

        public void Trigger (Component _target) {
            if (methodName == "")
                return;
            switch (paramType) {
            case ParamType.NONE:
                _target.SendMessage(methodName, msgOptions);
                break;
            case ParamType.STRING:
                _target.SendMessage(methodName, stringParam, msgOptions);
                break;
            case ParamType.FLOAT:
                _target.SendMessage(methodName, floatParam, msgOptions);
                break;
            case ParamType.INT:
                _target.SendMessage(methodName, intParam, msgOptions);
                break;
            case ParamType.BOOL:
                _target.SendMessage(methodName, boolParam, msgOptions);
                break;
            case ParamType.OBJECT:
                _target.SendMessage(methodName, objectParam, msgOptions);
                break;
            }
        }
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

    // ------------------------------------------------------------------ 
    [System.NonSerialized]
    private int totalFrames_ = -1;
    /// cached total frame count
    // ------------------------------------------------------------------ 

    public int totalFrames {
        get {
            if (totalFrames_ == -1) {
                totalFrames_ = 0;
                for (int i = 0; i < frameInfos.Count; ++i) {
                    totalFrames_ += frameInfos[i].frames;
                }
            }
            return totalFrames_;
        }
    }

    public List<FrameInfo> frameInfos = new List<FrameInfo>(); ///< the list of frame info 
    public List<EventInfo> eventInfos = new List<EventInfo>(); ///< the list of event info
    public float speed = 1.0f; ///< the default speed of the animation clip

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public float GetLength () {
        return (float)totalFrames / frameRate_;
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

    // // ------------------------------------------------------------------ 
    // /// \param _e the event information wants to add
    // /// add an event information
    // // ------------------------------------------------------------------ 

    // public void AddEvent ( EventInfo _e ) {
    //     //
    //     int index = eventInfos.BinarySearch( _e, eventInfoComparer );
    //     if ( index < 0 ) {
    //         index = ~index;
    //     }

    //     eventInfos.Insert( index, _e );
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _e the event information wants to remove
    // /// remove an event information
    // // ------------------------------------------------------------------ 

    // public void RemoveEvent ( EventInfo _e ) {
    //     eventInfos.Remove( _e );
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _time the time of the current animation
    // /// Get the event index play forward by time 
    // // ------------------------------------------------------------------ 

    // public int GetForwardEventIndex ( float _time ) {
    //     for ( int i = eventInfos.Count-1; i >= 0; --i ) {
    //         EventInfo ei = eventInfos[i];

    //         if ( _time > ei.frame ) {
    //             return i;
    //         }
    //     }
    //     return -1;
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _time the time of the current animation
    // /// Get the event index play backward by time 
    // // ------------------------------------------------------------------ 

    // public int GetBackwardEventIndex ( float _time ) {
    //     for ( int i = 0; i < eventInfos.Count; ++i ) {
    //         EventInfo ei = eventInfos[i];

    //         if ( _time < ei.frame ) {
    //             return i;
    //         }
    //     }
    //     return eventInfos.Count;
    // }

    // ------------------------------------------------------------------ 
    /// \param _target send message to target
    /// \param _start the start frame index
    /// \param _end the end frame index
    /// \param _wrapMode  the wrap mode
    /// Trigger events locate between the start and end frame
    // ------------------------------------------------------------------ 

    public void TriggerEvents (Component _target, int _start, float _end, WrapMode _wrapMode) {
        if (eventInfos.Count == 0)
            return;

        //get frame count
        if (totalFrames_ == -1) {
            totalFrames_ = totalFrames;
        }

        //
        bool lastFrameIsEnd;
        if (_start > 0) {
            if (totalFrames_ == 0) {
                lastFrameIsEnd = false;
            }
            else {
                int lastFrameWrappedIndex = exMath.Wrap(_start - 1, totalFrames_ - 1, _wrapMode);
                lastFrameIsEnd = (lastFrameWrappedIndex == totalFrames_ - 1);
            }
        }
        else {
            lastFrameIsEnd = false;
        }
        //Debug.Log(string.Format("[TriggerEvents|exSpriteAnimationClip] lastFrameIsEnd: {0}", lastFrameIsEnd));
        for (int i = _start; i <= _end; ++i) {
            EventInfo eventInfo;
            if (lastFrameIsEnd) {
                // TODO: check wrap dir
                eventInfo = EventInfo.SearchComparer.BinarySearch(eventInfos, totalFrames_);    // TODO: search same frame event
                if (eventInfo != null) {
                    eventInfo.Trigger(_target);
                }
                eventInfo = EventInfo.SearchComparer.BinarySearch(eventInfos, totalFrames_ - 1);
                if (eventInfo != null) {
                    eventInfo.Trigger(_target);
                }
            }
            int wrappedIndex;
            if (totalFrames_ == 0) {
                wrappedIndex = 0;
            }
            else {
                wrappedIndex =  exMath.Wrap(i, totalFrames_ - 1, _wrapMode);
            }
            eventInfo = EventInfo.SearchComparer.BinarySearch(eventInfos, wrappedIndex);
            if (eventInfo != null) {
                eventInfo.Trigger(_target);
            }
            lastFrameIsEnd = (wrappedIndex == totalFrames_ - 1);
        }
    }
}