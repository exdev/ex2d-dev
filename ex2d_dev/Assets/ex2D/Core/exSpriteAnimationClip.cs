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

    public class EventInfoComparer: IComparer<EventInfo> {
        public int Compare( EventInfo _x, EventInfo _y ) {
            if ( _x.frame > _y.frame )
                return 1;
            else if ( _x.frame == _y.frame )
                return 0;
            else
                return -1;
        }
    }

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

        public FrameInfo ( exTextureInfo _textureInfo, int _frames ) {
            textureInfo = _textureInfo;
            frames = _frames;
        }
    }

    // ------------------------------------------------------------------ 
    /// The structure to descrip an event in the sprite animation clip
    // ------------------------------------------------------------------ 

    [System.Serializable]
    public class EventInfo {

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
        public bool boolParam = false; ///< the value of the boolean parameter
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

    void InsertFrameInfo ( int _idx, FrameInfo _frameInfo ) {
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
    // /// \param _seconds the in seconds used for wrapping
    // /// \param _wrapMode the wrap mode used for wrapping
    // /// wrap the seconds of the anim clip by the wrap mode
    // // ------------------------------------------------------------------ 

    // public float WrapSeconds ( float _seconds, WrapMode _wrapMode ) {
    //     float t = Mathf.Abs(_seconds);
    //     if ( _wrapMode == WrapMode.Loop ) {
    //         t %= length;
    //     }
    //     else if ( _wrapMode == WrapMode.PingPong ) {
    //         int cnt = (int)(t/length);
    //         t %= length;
    //         if ( cnt % 2 == 1 ) {
    //             t = length - t;
    //         }
    //     }
    //     else {
    //         t = Mathf.Clamp( t, 0.0f, length );
    //     }
    //     return t;
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

    // // ------------------------------------------------------------------ 
    // /// \param _spAnim send message to target _spAnim.gameObject
    // /// \param _lastAnim last animation state
    // /// \param _lastIndex last triggered event info index (-1 means from start)
    // /// \param _start the start time in seconds 
    // /// \param _delta the delta time in seconds
    // /// \param _wrapMode  the wrap mode
    // /// \return return the last triggered event index
    // /// Trigger events locate between the start and start+_delta time span
    // // ------------------------------------------------------------------ 

    // public int TriggerEvents ( exSpriteAnimation _spAnim, 
    //                            exSpriteAnimState _lastAnim,
    //                            int _lastIndex,
    //                            float _start, 
    //                            float _delta, 
    //                            WrapMode _wrapMode ) 
    // {
    //     if ( eventInfos.Count == 0 )
    //         return -1;
    //     if ( _delta == 0.0f )
    //         return -1;

    //     // WrapSeconds
    //     float t = WrapSeconds(_start,_wrapMode); 

    //     // if we are the just start playing
    //     if ( _lastIndex == -1 ) {
    //         return ForwardTriggerEvents ( _spAnim, _lastAnim, -1, t, t + _delta, true );
    //     }

    //     //
    //     if ( _wrapMode == WrapMode.PingPong ) {
    //         int cnt = (int)(_start/length);
    //         if ( cnt % 2 == 1 )
    //             _delta = -_delta; 
    //     }

    //     // if we are play forward
    //     if ( _delta > 0.0f ) {
    //         if ( t + _delta > length ) {
    //             if ( _wrapMode == WrapMode.Loop ) {
    //                 float rest = t + _delta - length;
    //                 ForwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, length, false );
    //                 exSpriteAnimState curAnim = _spAnim.GetCurrentAnimation();
    //                 if ( curAnim == null || _lastAnim != curAnim )
    //                     return -1;
    //                 return ForwardTriggerEvents ( _spAnim, _lastAnim, -1, 0.0f, rest, true );
    //             }
    //             else if ( _wrapMode == WrapMode.PingPong ) {
    //                 float rest = t + _delta - length;
    //                 ForwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, length, false );
    //                 exSpriteAnimState curAnim = _spAnim.GetCurrentAnimation();
    //                 if ( curAnim == null || _lastAnim != curAnim )
    //                     return -1;
    //                 return BackwardTriggerEvents ( _spAnim, _lastAnim, eventInfos.Count, length, length - rest, false );
    //             }
    //             else {
    //                 return ForwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, t + _delta, false );
    //             }
    //         }
    //         else {
    //             return ForwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, t + _delta, false );
    //         }
    //     }
    //     else {
    //         if ( t + _delta < 0.0f ) {
    //             if ( _wrapMode == WrapMode.Loop ) {
    //                 float rest = 0.0f - (t + _delta);
    //                 BackwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, 0.0f, false );
    //                 exSpriteAnimState curAnim = _spAnim.GetCurrentAnimation();
    //                 if ( curAnim == null || _lastAnim != curAnim )
    //                     return -1;
    //                 return BackwardTriggerEvents ( _spAnim, _lastAnim, eventInfos.Count, length, length - rest, true );
    //             }
    //             else if ( _wrapMode == WrapMode.PingPong ) {
    //                 float rest = 0.0f - (t + _delta);
    //                 BackwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, 0.0f, false );
    //                 exSpriteAnimState curAnim = _spAnim.GetCurrentAnimation();
    //                 if ( curAnim == null || _lastAnim != curAnim )
    //                     return -1;
    //                 return ForwardTriggerEvents ( _spAnim, _lastAnim, -1, 0.0f, rest, false );
    //             }
    //             else {
    //                 return BackwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, t + _delta, false );
    //             }
    //         }
    //         else {
    //             return BackwardTriggerEvents ( _spAnim, _lastAnim, _lastIndex, t, t + _delta, false );
    //         }
    //     }
    // }

    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // int ForwardTriggerEvents ( exSpriteAnimation _spAnim, 
    //                            exSpriteAnimState _lastAnim,
    //                            int _index, 
    //                            float _start, 
    //                            float _end, 
    //                            bool _includeStart ) 
    // {
    //     int idx = _index;
    //     exSpriteAnimState curAnim = _spAnim.GetCurrentAnimation();
    //     for ( int i = _index+1; i < eventInfos.Count; ++i ) {
    //         EventInfo ei = eventInfos[i];

    //         if ( ei.frame == _start && _includeStart == false ) {
    //             idx = i;
    //             continue;
    //         }

    //         if ( ei.frame <= _end ) {
    //             Trigger ( _spAnim, ei );
    //             if ( curAnim == null || _lastAnim != curAnim )
    //                 return -1;
    //             idx = i;
    //         }
    //         else {
    //             break;
    //         }
    //     }
    //     return idx;
    // }

    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // int BackwardTriggerEvents ( exSpriteAnimation _spAnim, 
    //                             exSpriteAnimState _lastAnim,
    //                             int _index, 
    //                             float _start, 
    //                             float _end,
    //                             bool _includeStart )
    // {
    //     int idx = _index;
    //     exSpriteAnimState curAnim = _spAnim.GetCurrentAnimation();
    //     for ( int i = _index-1; i >= 0; --i ) {
    //         EventInfo ei = eventInfos[i];

    //         if ( ei.frame == _start && _includeStart == false ) {
    //             idx = i;
    //             continue;
    //         }

    //         if ( ei.frame >= _end ) {
    //             Trigger ( _spAnim, ei );
    //             if ( curAnim == null || _lastAnim != curAnim )
    //                 return -1;
    //             idx = i;
    //         }
    //         else {
    //             break;
    //         }
    //     }
    //     return idx;
    // }

    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // void Trigger ( exSpriteAnimation _spAnim, EventInfo _eventInfo ) {

    //     if ( _eventInfo.methodName == "" )
    //         return;

    //     switch ( _eventInfo.paramType ) {
    //     case EventInfo.ParamType.NONE:
    //         _spAnim.SendMessage ( _eventInfo.methodName, _eventInfo.msgOptions );
    //         break;

    //     case EventInfo.ParamType.STRING:
    //         _spAnim.SendMessage ( _eventInfo.methodName, _eventInfo.stringParam, _eventInfo.msgOptions );
    //         break;

    //     case EventInfo.ParamType.FLOAT:
    //         _spAnim.SendMessage ( _eventInfo.methodName, _eventInfo.floatParam, _eventInfo.msgOptions );
    //         break;

    //     case EventInfo.ParamType.INT:
    //         _spAnim.SendMessage ( _eventInfo.methodName, _eventInfo.intParam, _eventInfo.msgOptions );
    //         break;

    //     case EventInfo.ParamType.BOOL:
    //         _spAnim.SendMessage ( _eventInfo.methodName, _eventInfo.boolParam, _eventInfo.msgOptions );
    //         break;

    //     case EventInfo.ParamType.OBJECT:
    //         _spAnim.SendMessage ( _eventInfo.methodName, _eventInfo.objectParam, _eventInfo.msgOptions );
    //         break;
    //     }
    // }
}

