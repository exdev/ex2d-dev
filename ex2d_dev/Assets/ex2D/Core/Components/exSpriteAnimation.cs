﻿// ======================================================================================
// File         : exSpriteAnimation.cs
// Author       : Jare
// Last Change  : 07/16/2013 | 22:50:36
// Description  : The exSpriteAnimation component
// ======================================================================================

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

    [System.NonSerialized] public exSpriteAnimationClip clip; ///< the referenced sprite animation clip
    [System.NonSerialized] public string name; ///< the name of the sprite animation state
    [System.NonSerialized] public WrapMode wrapMode; ///< the wrap mode
    [System.NonSerialized] public exSpriteAnimationClip.StopAction stopAction; ///< the stop action
    [System.NonSerialized] public float length; ///< the length of the sprite animation in seconds with speed = 1.0f

    [System.NonSerialized] public float speed = 1.0f; ///< the speed to play the sprite animation clip
    [System.NonSerialized] public float time = 0.0f; ///< the current time in seoncds
    // [System.NonSerialized] public float normalizedTime = 0.0f;
    [System.NonSerialized] public List<float> frameTimes; ///< the list of the start time in seconds of each frame in the exSpriteAnimClip

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
        clip = _animClip;
        name = _name;
        wrapMode = _animClip.wrapMode;
        stopAction = _animClip.stopAction;
        length = _animClip.GetLength();
        speed = _animClip.speed;

        frameTimes = new List<float> (_animClip.frameInfos.Count);
        float unitSeconds = 1.0f / _animClip.frameRate;
        int startFrame = 0;
        for (int i = 0; i < _animClip.frameInfos.Count; ++i) {
            startFrame += _animClip.frameInfos[i].frames;
            frameTimes.Add(startFrame * unitSeconds);
        }
    }
    
    public int GetCurrentIndex() {
        int index = frameTimes.BinarySearch(exMath.Wrap(time, length, wrapMode));
        if (index < 0) {
            index = ~index;
        }
        return index;
    }
}

///////////////////////////////////////////////////////////////////////////////
//
/// The sprite animation component
//
///////////////////////////////////////////////////////////////////////////////

[RequireComponent (typeof(exSprite))]
[AddComponentMenu("ex2D Sprite/Sprite Animation")]
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

    public bool playAutomatically = false;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    private Dictionary<string, exSpriteAnimationState> nameToState;
    private exSpriteAnimationState curAnimation;
    private exSprite sprite;
    private exTextureInfo defaultTextureInfo;
    private int lastEventInfoIndex = -1;
    
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
    /// \param _name the name of the animation to play
    /// \param _frame the frame count
    /// Play the animation by _name, start from the _frame
    // ------------------------------------------------------------------ 

    public void Play (string _name, int _frame) {
        exSpriteAnimationState anim = GetAnimation(_name);
        if (anim != null) {
            float unitSeconds = 1.0f / anim.clip.frameRate;
            float time = _frame * unitSeconds;
            Play(anim, time);
        }
    }

    // ------------------------------------------------------------------ 
    /// Stop the playing animation, take the action that setup in the 
    /// exSpriteAnimState.stopAction 
    // ------------------------------------------------------------------ 

    public void Stop () {
        if ( curAnimation != null ) {
            exSpriteAnimationClip.StopAction stopAction = curAnimation.stopAction;

            curAnimation.time = 0.0f;
            curAnimation = null;

            switch ( stopAction ) {
            case exSpriteAnimationClip.StopAction.DoNothing:
                // Nothing todo;
                break;

            case exSpriteAnimationClip.StopAction.DefaultSprite:
                sprite.textureInfo = defaultTextureInfo;
                break;

            case exSpriteAnimationClip.StopAction.Hide:
                sprite.enabled = false;
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
        sprite.textureInfo = defaultTextureInfo;
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
    /// \return the frame info
    /// Get the information of current frame in the playing animation.
    // ------------------------------------------------------------------ 

    public exSpriteAnimationClip.FrameInfo GetCurFrameInfo () {
        if (curAnimation != null) {
            exDebug.Assert(0 <= curIndex && curIndex < curAnimation.clip.frameInfos.Count);
            if (0 <= curIndex && curIndex < curAnimation.clip.frameInfos.Count)
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
        animations.Remove(_animClip);
        nameToState.Remove(_animClip.name);
    }
    
    // ------------------------------------------------------------------ 
    /// \param _animState the animation state to sample
    /// Samples animations at the current state.
    /// This is useful when you explicitly want to set up some animation state, and sample it once.
    // ------------------------------------------------------------------ 

    public void Sample (exSpriteAnimationState _animState) {
        curAnimation = _animState;
        Sample();
    }
    
    // ------------------------------------------------------------------ 
    /// advance the time and check if we trigger any animation events
    // ------------------------------------------------------------------ 

    public void Step (exSpriteAnimationState _animState, float _deltaTime) {
        curAnimation = _animState;
        Step(_deltaTime);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
        
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Init () {
        bool unInited = (nameToState == null);
        if (unInited) {
            sprite = GetComponent<exSprite>();
            defaultTextureInfo = sprite.textureInfo;

            nameToState = new Dictionary<string, exSpriteAnimationState>();
            for (int i = 0; i < animations.Count; ++i) {
                exSpriteAnimationClip clip = animations[i];
                if (clip != null) {
                    exSpriteAnimationState state = new exSpriteAnimationState(clip);
                    nameToState[state.name] = state;
                    if (ReferenceEquals(defaultAnimation, clip)) {
                        curAnimation = state;
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
            curAnimation.time += _deltaTime;

            Sample();

            // save the last state
            //exSpriteAnimState lastAnimation = curAnimation;

            int newIdx = curAnimation.clip.TriggerEvents( this, 
                                                          lastAnimation,
                                                          lastEventInfoIndex,
                                                          curTime,
                                                          delta,
                                                          curAnimation.wrapMode );

            // check if stop
            if (curAnimation.wrapMode == WrapMode.Once ||
                curAnimation.wrapMode == WrapMode.Default)
            {
                if ((curAnimation.speed > 0.0f && curAnimation.time >= curAnimation.length) ||
                    (curAnimation.speed < 0.0f && curAnimation.time <= 0.0f))
                {
                    Stop();
                }
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
            curIndex = curAnimation.GetCurrentIndex();
            sprite.textureInfo = curAnimation.clip.frameInfos[curIndex].textureInfo;
        }
        else {
            curIndex = -1;
        }
    }
}
