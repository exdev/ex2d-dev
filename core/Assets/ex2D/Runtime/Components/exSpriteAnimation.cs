// ======================================================================================
// File         : exSpriteAnimation.cs
// Author       : 
// Last Change  : 08/17/2013 | 15:27:56
// Description  : The exSpriteAnimation component
// ======================================================================================

#define DUPLICATE_WHEN_PINGPONE

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// The state of the exSpriteAnimClip, a state can be treat as an instance. In 
/// ex2D, when you play the sprite animation, it will load and reference the data
/// in exSpriteAnimClip. But you can't just save your state in the clip, because 
/// it is possible the other gameObject play the same animation in the same time. 
/// 
/// The exSpriteAnimState is designed to solve the problems, it initialized self by  
/// copy the settings in exSpriteAnimClip, and provide identity state for user in 
/// exSpriteAnimation component.
///
///////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public class exSpriteAnimationState {

    [System.NonSerialized] public exSpriteAnimationClip clip; ///< the referenced sprite sprite animation clip
    [System.NonSerialized] public string name; ///< the name of the sprite animation state
    [System.NonSerialized] public WrapMode wrapMode; ///< the wrap mode
    [System.NonSerialized] public exSpriteAnimationClip.StopAction stopAction; ///< the stop action
    [System.NonSerialized] public float length; ///< the length of the sprite animation in seconds with speed = 1.0f
    [System.NonSerialized] public int totalFrames; ///< the total frame count of the sprite animation clip
    [System.NonSerialized] public float speed = 1.0f; ///< the speed to play the sprite animation clip
    [System.NonSerialized] public float time = 0.0f; ///< the current time in seoncds
    
    /// The current index of frame. The value can be larger than totalFrames.
    /// If the frame is larger than totalFrames it will be wrapped according to wrapMode. 
    [System.NonSerialized] public int frame = -1;

    [System.NonSerialized] private int[] frameInfoFrames; ///< the array of the end frame of each frame info in the sprite animation clip
    [System.NonSerialized] private int cachedIndex = -1;    ///< cache result of GetCurrentIndex
    [System.NonSerialized] private Dictionary<int, List<exSpriteAnimationClip.EventInfo>> frameToEventDict = null;

    // ------------------------------------------------------------------ 
    /// \param _animClip the referenced animation clip
    /// Constructor of exSpriteAnimState, it will copy the settings from _animClip. 
    // ------------------------------------------------------------------ 

    public exSpriteAnimationState (exSpriteAnimationClip _animClip) :
        this ( _animClip.name, _animClip ) {
    }

    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation state
    /// \param _animClip the referenced animation clip
    /// Constructor of exSpriteAnimState, it will copy the settings from _animClip. 
    // ------------------------------------------------------------------ 

    public exSpriteAnimationState (string _name, exSpriteAnimationClip _animClip) {
        name = _name;
        clip = _animClip;
        wrapMode = clip.wrapMode;
        stopAction = clip.stopAction;
        speed = clip.speed;
        frameInfoFrames = clip.GetFrameInfoFrames();
        if (frameInfoFrames.Length > 0) {
            totalFrames = frameInfoFrames[frameInfoFrames.Length - 1];
        }
        else {
            totalFrames = 0;
        }
        length = totalFrames / clip.frameRate;
        const int MIN_HASH_COUNT = 9;
        if (clip.eventInfos.Count >= MIN_HASH_COUNT) {
            frameToEventDict = clip.GetFrameToEventDict();
        }
    }
    
    // ------------------------------------------------------------------ 
    /// \return Get current frame info index.
    // ------------------------------------------------------------------ 

    public int GetCurrentIndex() {
        if (totalFrames > 1) {
            //int oldFrame = frame;
            frame = (int) (time * clip.frameRate);
            if (frame < 0) {
                frame = -frame;
            }
            //// use cache to optimize
            //if (frame == oldFrame && cachedIndex != -1) {
            //    return cachedIndex;
            //}
            int wrappedIndex;
#if DUPLICATE_WHEN_PINGPONE
            if (wrapMode != WrapMode.PingPong) {
                wrappedIndex = exMath.Wrap(frame, totalFrames - 1, wrapMode);
            }
            else {
                wrappedIndex = frame;
                int cnt = wrappedIndex / totalFrames;
                wrappedIndex %= totalFrames;
                if ((cnt & 0x1) == 1) {
                    wrappedIndex = totalFrames - 1 - wrappedIndex;
                }
            }
#else
            wrappedIndex = exMath.Wrap(frame, totalFrames - 1, wrapMode);
#endif
            // try to use cached frame info index
            if (cachedIndex - 1 >= 0 && 
                wrappedIndex >= frameInfoFrames[cachedIndex - 1] &&
                wrappedIndex < frameInfoFrames[cachedIndex]) {
                return cachedIndex;
            }
            // search frame info
            int frameInfoIndex = System.Array.BinarySearch(frameInfoFrames, wrappedIndex + 1);
            if (frameInfoIndex < 0) {
                frameInfoIndex = ~frameInfoIndex;
                exDebug.Assert(frameInfoIndex < frameInfoFrames.Length);
            }
            cachedIndex = frameInfoIndex;
            return frameInfoIndex;
        }
        else if (totalFrames == 1) {
            return 0;
        }
        else {
            return -1;
        }
    }
    
    // ------------------------------------------------------------------ 
    /// \param _target send event messages to the target
    /// \param _start the unwrapped start frame index
    /// \param _end the unwrapped end frame index
    /// Trigger events locate between the start and end frame
    // ------------------------------------------------------------------ 

    public void TriggerEvents (Component _target, int _start, float _end) {
        if (clip.eventInfos.Count == 0)
            return;
        for (int i = _start; i <= _end; ++i) {
            if (totalFrames == 0) {
                TriggerEvents(_target, 0, false);
                continue;
            }
            int wrappedIndex;
            bool reversed = false;
            if (wrapMode == WrapMode.PingPong) {
#if DUPLICATE_WHEN_PINGPONE
                //wrappedIndex = exMath.Wrap(i, totalFrame, wrapMode);
                int cnt = i / totalFrames;
                wrappedIndex = i % totalFrames;
                reversed = ((cnt & 0x1) == 1);
                if (reversed) {
                    wrappedIndex = totalFrames - wrappedIndex;
                }
#else
                int cnt = (i - 1) / (totalFrames - 1);
                wrappedIndex = (i - 1) % (totalFrames - 1) + 1;
                bool skippedEndsEvent = (i > 1 && wrappedIndex == 1);
                reversed = ((cnt & 0x1) == 1);
                if (reversed) {
                    if (skippedEndsEvent) {
                        TriggerEvents(_target, totalFrames, true);
                    }
                    wrappedIndex = totalFrames - wrappedIndex;
                }
                else {
                    if (skippedEndsEvent) {
                        TriggerEvents(_target, 0, false);
                    }
                }
#endif
            }
            else if (wrapMode == WrapMode.Loop) {
                wrappedIndex = exMath.Wrap(i, totalFrames - 1, wrapMode);
                bool skippedFinalEvent = (i > 0 && wrappedIndex == 0);
                if (skippedFinalEvent) {
                    TriggerEvents(_target, totalFrames, false);
                }
            }
            else {
                exDebug.Assert(i <= totalFrames);
                wrappedIndex = i;
            }
            TriggerEvents(_target, wrappedIndex, reversed);
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _target send event messages to the target
    /// \param _wrappedFrame the wrapped frame to trigger
    /// \param _reversed reversed trigger order between same frame events
    /// Trigger all events at the frame
    // ------------------------------------------------------------------ 

    private void TriggerEvents (Component _target, int _wrappedIndex, bool _reversed) {
        if (clip.eventInfos.Count == 0) {
            return;
        }
        List<exSpriteAnimationClip.EventInfo> eventInfoList;
        if (frameToEventDict == null) {
            eventInfoList = clip.eventInfos;
        }
        else if (frameToEventDict.TryGetValue(_wrappedIndex, out eventInfoList) == false) {
            return;
        }
        if (_reversed) {
            for (int i = eventInfoList.Count - 1; i >= 0; --i) {
                if (eventInfoList[i].frame == _wrappedIndex) {
                    Trigger(_target, eventInfoList[i]);
                }
            }
        }
        else {
            for (int i = 0; i < eventInfoList.Count; ++i) {
                if (eventInfoList[i].frame == _wrappedIndex) {
                    Trigger(_target, eventInfoList[i]);
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// Calls the method named methodName on every Component target game object.
    // ------------------------------------------------------------------ 

    public void Trigger (Component _target, exSpriteAnimationClip.EventInfo _event) {
        if (_event.methodName == "")
            return;
        switch (_event.paramType) {
        case exSpriteAnimationClip.EventInfo.ParamType.None:
            _target.SendMessage(_event.methodName, _event.msgOptions);
            break;
        case exSpriteAnimationClip.EventInfo.ParamType.String:
            _target.SendMessage(_event.methodName, _event.stringParam, _event.msgOptions);
            break;
        case exSpriteAnimationClip.EventInfo.ParamType.Float:
            _target.SendMessage(_event.methodName, _event.floatParam, _event.msgOptions);
            break;
        case exSpriteAnimationClip.EventInfo.ParamType.Int:
            _target.SendMessage(_event.methodName, _event.intParam, _event.msgOptions);
            break;
        case exSpriteAnimationClip.EventInfo.ParamType.Bool:
            _target.SendMessage(_event.methodName, _event.boolParam, _event.msgOptions);
            break;
        case exSpriteAnimationClip.EventInfo.ParamType.Object:
            _target.SendMessage(_event.methodName, _event.objectParam, _event.msgOptions);
            break;
        }
    }
}

///////////////////////////////////////////////////////////////////////////////
//
/// The sprite animation component
//
///////////////////////////////////////////////////////////////////////////////

[RequireComponent (typeof(exSprite))]
[AddComponentMenu("ex2D/Sprite Animation")]
public class exSpriteAnimation : MonoBehaviour {
        
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
        
    // ------------------------------------------------------------------ 
    /// the default sprite animation clip
    // ------------------------------------------------------------------ 

    public exSpriteAnimationClip defaultAnimation;

    // ------------------------------------------------------------------ 
    /// the list of sprite animation clips used in the component
    // ------------------------------------------------------------------ 

    public List<exSpriteAnimationClip> animations = new List<exSpriteAnimationClip>();

    // ------------------------------------------------------------------ 
    /// When playAutomatically set to true, it will play the 
    /// exSpriteAnimation.defaultAnimation at the start
    // ------------------------------------------------------------------ 

    public bool playAutomatically = true;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    private Dictionary<string, exSpriteAnimationState> nameToState;
    private exSpriteAnimationState curAnimation;
    private exSprite sprite_;
    public exSprite sprite { 
        get {
            return sprite_;
        }
    }
    private exTextureInfo defaultTextureInfo;
    private int lastFrameIndex = -1;
    
    //private float curWrappedTime = 0.0f;
    private int curIndex = -1;

    ///////////////////////////////////////////////////////////////////////////////
    // other properties
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation
    /// \return the animation state
    /// Get the animation state by _name
    // ------------------------------------------------------------------ 

    public exSpriteAnimationState this[string _name] {
        get {
            Init();
            exSpriteAnimationState state;
            if (nameToState.TryGetValue(_name, out state)) {
                return state;
            }
            else {
                return null;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////
    
	void Awake () {
        Init();
        if (enabled) {  // 和Unity自带的Animation保持一致，未激活时不播放
            if (playAutomatically && defaultAnimation != null) {
                Play(defaultAnimation.name, 0);
            }
            else {
                enabled = false;
            }
        }
	}
	
    // Unity自带的Animation在Update和LateUpdate之间执行。
    // 这里我们采用LateUpdate，用户如果有需要在帧切换之后执行的操作，可使用事件或自行修改优先级。
	void LateUpdate () {
        if (curAnimation != null) {
            float delta = Time.deltaTime * curAnimation.speed;
            Step(delta);
        }
	}

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation to play
    /// Play the animation by _name 
    // ------------------------------------------------------------------ 

    public void Play (string _name) {
        Play(_name, 0);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Play (string _name, float _time) {
        exSpriteAnimationState anim = GetAnimation(_name);
        if (anim != null) {
            Play(anim, _time);
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation to play
    /// \param _frame the frame count
    /// Play the animation by _name, start from the _frame
    // ------------------------------------------------------------------ 

    public void PlayByFrame (string _name, int _frame) {
        exSpriteAnimationState anim = GetAnimation(_name);
        if (anim != null) {
            float unitSeconds = 1.0f / anim.clip.frameRate;
            float time = _frame * unitSeconds;
            Play(anim, time);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Stop () {
        Stop (curAnimation);
    }

    // ------------------------------------------------------------------ 
    /// Stop the playing animation, take the action that setup in the 
    /// exSpriteAnimState.stopAction 
    // ------------------------------------------------------------------ 

    public void Stop (exSpriteAnimationState _animState) {
        if ( _animState != null ) {
            exSpriteAnimationClip.StopAction stopAction = _animState.stopAction;

            _animState.time = 0.0f;
            _animState = null;

            switch ( stopAction ) {
            case exSpriteAnimationClip.StopAction.DoNothing:
                break;

            case exSpriteAnimationClip.StopAction.DefaultSprite:
                sprite_.textureInfo = defaultTextureInfo;
                break;

            case exSpriteAnimationClip.StopAction.Hide:
                sprite_.enabled = false;
                break;

            case exSpriteAnimationClip.StopAction.Destroy:
                Object.Destroy(gameObject);
                break;
            }
        }
        enabled = false;
    }

    // ------------------------------------------------------------------ 
    /// reset to default texture info
    // ------------------------------------------------------------------ 

    public void SetDefaultSprite () {
        sprite_.textureInfo = defaultTextureInfo;
    }

    // ------------------------------------------------------------------ 
    /// update the default texture info if we dynamically change it in the game
    // ------------------------------------------------------------------ 

    public void UpdateDefaultSprite (exTextureInfo _textureInfo) {
        defaultTextureInfo = _textureInfo;
    }

    // NOTE: the reason I design to Play instead of using default parameter is because in 
    // Unity Animation Editor, it can send message to function that only have one parameter.

    // ------------------------------------------------------------------ 
    /// Play the default animation by _name 
    // ------------------------------------------------------------------ 

    public void PlayDefault () {
        if (defaultAnimation != null)
            Play(defaultAnimation.name, 0);
    }

    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation
    /// \return the animation state
    /// Get the animation state by _name
    // ------------------------------------------------------------------ 

    public exSpriteAnimationState GetAnimation (string _name) {
        return this[_name];
    }

    // ------------------------------------------------------------------ 
    /// \return the current animation state
    /// Get the current playing animation state
    // ------------------------------------------------------------------ 

    public exSpriteAnimationState GetCurrentAnimation () { return curAnimation; }
    
    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation
    /// \return the boolean result
    /// Check if the _name of the animation is the current playing animation.
    /// If the _name is empty, it will check if there is animation playing now.
    // ------------------------------------------------------------------ 

    public bool IsPlaying ( string _name = "" ) {
        if ( string.IsNullOrEmpty(_name) )
            return enabled && curAnimation != null;
        else
            return ( enabled && curAnimation != null && curAnimation.name == _name );
    }

    // ------------------------------------------------------------------ 
    /// \return the frame info
    /// Get the information of current frame in the playing animation.
    // ------------------------------------------------------------------ 

    public exSpriteAnimationClip.FrameInfo GetCurFrameInfo () {
        if (curAnimation != null) {
            exDebug.Assert(curIndex < curAnimation.clip.frameInfos.Count);
            if (curIndex < curAnimation.clip.frameInfos.Count)
                return curAnimation.clip.frameInfos[curIndex];
        }
        return null;
    }

    // ------------------------------------------------------------------ 
    /// \return the frame index
    /// Get the index of current frame in the playing animation.
    // ------------------------------------------------------------------ 

    public int GetCurFrameIndex () {
        return curIndex;
    }

    // ------------------------------------------------------------------ 
    /// \param _animClip the sprite animation clip wants to add
    /// \return the instantiate animation state of the added _animClip 
    /// Add a sprite animation clip, create a new animation state and saves 
    /// it to the lookup table by the name of the clip
    /// 
    /// \note if the animation already in the exSpriteAnimation.animations, 
    /// it will override the old clip and return a new animation state.
    // ------------------------------------------------------------------ 

    public exSpriteAnimationState AddAnimation (exSpriteAnimationClip _animClip) {
        return AddAnimation(_animClip.name, _animClip);
    }

    // ------------------------------------------------------------------ 
    /// \param _name the name of animation state you want to add
    /// \param _animClip the sprite animation clip wants to add
    /// \return the instantiate animation state of the added _animClip 
    /// Add a sprite animation clip, create a new animation state and saves 
    /// it to the lookup table by the name of the clip
    /// 
    /// \note if the animation already in the exSpriteAnimation.animations, 
    /// it will override the old clip and return a new animation state.
    // ------------------------------------------------------------------ 

    public exSpriteAnimationState AddAnimation (string _name, exSpriteAnimationClip _animClip) {
        Init();
        exSpriteAnimationState state = null;

        // if we already have the animation, just return the animation state
        if (animations.IndexOf(_animClip) != -1) {
            state = nameToState[_name];
            if (state.clip != _animClip) {
                state = new exSpriteAnimationState(_name, _animClip);
                nameToState[_name] = state;
            }
            return state;
        }

        //
        animations.Add(_animClip);
        state = new exSpriteAnimationState(_name, _animClip);
        nameToState[_name] = state;
        return state;
    }

    // ------------------------------------------------------------------ 
    /// \param _animClip the sprite animation clip wants to remove
    /// Remove a sprite animation clip from exSpriteAnimation.animations, 
    // ------------------------------------------------------------------ 

    public void RemoveAnimation (exSpriteAnimationClip _animClip) {
        if (animations.IndexOf(_animClip) == -1) {
            return;
        }

        //
        Init();
        animations.Remove(_animClip);
        nameToState.Remove(_animClip.name);
    }
    
    // ------------------------------------------------------------------ 
    /// \param _animState the animation state to sample
    /// Samples animations at the current state.
    /// This is useful when you explicitly want to set up some animation state, and sample it once.
    // ------------------------------------------------------------------ 

    public void Sample (exSpriteAnimationState _animState) {
        if (_animState != null) {
            if (curAnimation != _animState) {
                curAnimation = _animState;
                lastFrameIndex = -1;
            }
            Sample();
        }
    }
    
    // ------------------------------------------------------------------ 
    /// advance the time and check if we trigger any animation events
    // ------------------------------------------------------------------ 

    public void Step (exSpriteAnimationState _animState, float _deltaTime) {
        if (_animState != null) {
            if (curAnimation != _animState) {
                curAnimation = _animState;
                lastFrameIndex = -1;
            }
            Step(_deltaTime);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
        
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Init () {
        bool initialized = (nameToState != null);
        if (initialized == false) {
            sprite_ = GetComponent<exSprite>();
            defaultTextureInfo = sprite_.textureInfo;

            nameToState = new Dictionary<string, exSpriteAnimationState>();
            for (int i = 0; i < animations.Count; ++i) {
                exSpriteAnimationClip clip = animations[i];
                if (clip != null) {
                    exSpriteAnimationState state = new exSpriteAnimationState(clip);
                    nameToState[state.name] = state;
                    if (ReferenceEquals(defaultAnimation, clip)) {
                        curAnimation = state;
                        lastFrameIndex = -1;
                    }
                }
            }
            exDebug.Assert(defaultAnimation == null || defaultAnimation == nameToState[defaultAnimation.name].clip);
        }
    }
    
    // ------------------------------------------------------------------ 
    /// \param _animState the animation state to play
    /// \param _time the time to play
    /// Play the animation by _animState, start from the _index of frame  
    // ------------------------------------------------------------------ 

    private void Play (exSpriteAnimationState _animState, float _time) {
        curAnimation = _animState;
        if (curAnimation != null) {
            curIndex = -1;
            curAnimation.time = _time;
            Sample();
            enabled = true;
        }
    }

    // ------------------------------------------------------------------ 
    // Do step
    // ------------------------------------------------------------------ 

    void Step (float _deltaTime) {
        if (curAnimation != null) {
            int eventStartIndex = curAnimation.frame;
            if (lastFrameIndex == eventStartIndex) {
                ++eventStartIndex;
            }

            curAnimation.time += _deltaTime;
            Sample();

            // check if stop
            bool stop = false;
            if (curAnimation.wrapMode == WrapMode.Once ||
                curAnimation.wrapMode == WrapMode.Default ||
                curAnimation.wrapMode == WrapMode.ClampForever)
            {
                if (curAnimation.speed > 0.0f && curAnimation.frame >= curAnimation.totalFrames) {
                    stop = true;
                    curAnimation.frame = curAnimation.totalFrames;
                }
                else if (curAnimation.speed < 0.0f && curAnimation.frame < 0) {
                    stop = true;
                    curAnimation.frame = 0;
                }
            }

            exSpriteAnimationState backupAnimBeforeEvent = curAnimation;

            // trigger events
            if (eventStartIndex <= curAnimation.frame) {
                curAnimation.TriggerEvents(this, eventStartIndex, curAnimation.frame);
                lastFrameIndex = backupAnimBeforeEvent.frame;
            }
            
            // do stop
            if (stop) {
                Stop(backupAnimBeforeEvent);
            }
        }
        else {
            curIndex = -1;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Do sample
    // ------------------------------------------------------------------ 

    void Sample () {
        if (curAnimation != null) {
            int newIndex = curAnimation.GetCurrentIndex();
            if (newIndex >= 0 && newIndex != curIndex) {
                sprite_.textureInfo = curAnimation.clip.frameInfos[newIndex].textureInfo;
            }
            curIndex = newIndex;
        }
        else {
            curIndex = -1;
        }
    }
}
