// ======================================================================================
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
public class exSpriteAnimState {

    [System.NonSerialized] public exSpriteAnimationClip clip; ///< the referenced sprite animation clip
    [System.NonSerialized] public string name; ///< the name of the sprite animation state
    [System.NonSerialized] public WrapMode wrapMode; ///< the wrap mode
    //[System.NonSerialized] public exSpriteAnimClip.StopAction stopAction; ///< the stop action
    [System.NonSerialized] public float length; ///< the length of the sprite animation in seconds with speed = 1.0f

    [System.NonSerialized] public float speed = 1.0f; ///< the speed to play the sprite animation clip
    [System.NonSerialized] public float time = 0.0f; ///< the current time in seoncds
    // [System.NonSerialized] public float normalizedTime = 0.0f;
    [System.NonSerialized] public List<float> frameTimes; ///< the list of the start time in seconds of each frame in the exSpriteAnimClip

    // ------------------------------------------------------------------ 
    /// \param _animClip the referenced animation clip
    /// Constructor of exSpriteAnimState, it will copy the settings from _animClip. 
    // ------------------------------------------------------------------ 

    public exSpriteAnimState (exSpriteAnimationClip _animClip) :
        this ( _animClip.name, _animClip ) {
    }

    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation state
    /// \param _animClip the referenced animation clip
    /// Constructor of exSpriteAnimState, it will copy the settings from _animClip. 
    // ------------------------------------------------------------------ 

    public exSpriteAnimState (string _name, exSpriteAnimationClip _animClip) {
        clip = _animClip;
        name = _name;
        //wrapMode = _animClip.wrapMode;
        //stopAction = _animClip.stopAction;
        //length = _animClip.length;
        //speed = _animClip.speed;

        frameTimes = new List<float>(_animClip.frameInfos.Count);
        float tmp = 0.0f;
        float timePerFrame = 1.0f / _animClip.frameRate;
        for (int i = 0; i < frameTimes.Count; ++i) {
            tmp += _animClip.frameInfos[i].frames * timePerFrame;
            frameTimes.Add(tmp);
        }
    }
}

/// test
public class exSpriteAnimClipMocker : ScriptableObject {
    public List<FrameInfo> frameInfos = new List<FrameInfo>();
    public float length = 1.0f;
    public float sampleRate = 60.0f;
    public class FrameInfo {
        public exTextureInfo textureInfo;
        public float length = 0.0f;
    }
    public float WrapSeconds ( float _seconds, WrapMode _wrapMode ) {
        float t = Mathf.Abs(_seconds);
        if (_wrapMode == WrapMode.Loop) {
            t %= length;
        }
        else if (_wrapMode == WrapMode.PingPong) {
            int cnt = (int)(t / length);
            t %= length;
            if (cnt % 2 == 1) {
                t = length - t;
            }
        }
        else {
            t = Mathf.Clamp(t, 0.0f, length);
        }
        return t;
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

    private Dictionary<string, exSpriteAnimState> nameToState;
    private exSpriteAnimState curAnimation;
    private exSprite sprite;
    private exTextureInfo defaultTextureInfo;
    //private int lastEventInfoIndex = -1;
    
    private float curWrappedTime = 0.0f;
    private int curIndex = -1;
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////
    
	void Awake () {
        Init();

        if (playAutomatically && defaultAnimation != null) {
            Play(defaultAnimation.name, 0);
        }
        else {
            enabled = false;
        }
	}
	
	void Update () {
        if (curAnimation != null) {
            // advance the time and check if we trigger any animation events
            float delta = Time.deltaTime * curAnimation.speed;
            float curTime = curAnimation.time;

            // advance the time
            curAnimation.time += delta;
            Step(curAnimation);

            // set sprite to current time
            exSpriteAnimationClip.FrameInfo fi = GetCurFrameInfo();
            if (fi != null)
                sprite.textureInfo = fi.textureInfo;

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
        curAnimation = GetAnimation(_name);
        if (curAnimation != null) {
            float unitSeconds = 1.0f / curAnimation.clip.sampleRate;
            float time = _frame * unitSeconds;
            Play(curAnimation, time);
        }
    }

    // ------------------------------------------------------------------ 
    /// Stop the playing animation, take the action that setup in the 
    /// exSpriteAnimState.stopAction 
    // ------------------------------------------------------------------ 

    public void Stop () {
        if ( curAnimation != null ) {
            //
            //exSpriteAnimationClip.StopAction stopAction = curAnimation.stopAction; 

            //
            curAnimation.time = 0.0f;
            curAnimation = null;

            // TODO: add playing
            //switch ( stopAction ) {
            //case exSpriteAnimClip.StopAction.DoNothing:
            //    // Nothing todo;
            //    break;

            //case exSpriteAnimClip.StopAction.DefaultSprite:
            //    sprite.SetSprite( defaultAtlas, defaultIndex );
            //    break;

            //case exSpriteAnimClip.StopAction.Hide:
            //    sprite.enabled = false;
            //    break;

            //case exSpriteAnimClip.StopAction.Destroy:
            //    GameObject.Destroy(gameObject);
            //    break;
            //}
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
        if (defaultAnimation)
            Play(defaultAnimation.name, 0);
    }

    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation
    /// \return the animation state
    /// Get the animation state by _name
    // ------------------------------------------------------------------ 

    public exSpriteAnimState GetAnimation (string _name) {
        Init();
        exSpriteAnimState state = null;
        nameToState.TryGetValue(_name, out state);
        return state;
    }

    // ------------------------------------------------------------------ 
    /// \return the current animation state
    /// Get the current playing animation state
    // ------------------------------------------------------------------ 

    public exSpriteAnimState GetCurrentAnimation () { return curAnimation; }

    // ------------------------------------------------------------------ 
    /// \return the frame info
    /// Get the information of current frame in the playing animation.
    // ------------------------------------------------------------------ 

    public exSpriteAnimationClip.FrameInfo GetCurFrameInfo () {
        if (curAnimation != null) {
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

    public exSpriteAnimState AddAnimation (exSpriteAnimationClip _animClip) {
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

    public exSpriteAnimState AddAnimation (string _name, exSpriteAnimationClip _animClip) {
        Init();
        exSpriteAnimState state = null;

        // if we already have the animation, just return the animation state
        if (animations.IndexOf(_animClip) != -1) {
            state = nameToState[_name];
            if (state.clip != _animClip) {
                state = new exSpriteAnimState(_name, _animClip);
                nameToState[_name] = state;
            }
            return state;
        }

        //
        animations.Add(_animClip);
        state = new exSpriteAnimState(_name, _animClip);
        nameToState[_name] = state;
        return state;
    }

    // ------------------------------------------------------------------ 
    /// \param _animClip the sprite animation clip wants to remove
    /// Remove a sprite animation clip from exSpriteAnimation.animations, 
    // ------------------------------------------------------------------ 

    public void RemoveAnimation (exSpriteAnimationClip _animClip) {
        // if we already have the animation, just return the animation state
        if (animations.IndexOf(_animClip) == -1) {
            return;
        }

        //
        animations.Remove(_animClip);
        nameToState.Remove(_animClip.name);
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

            nameToState = new Dictionary<string, exSpriteAnimState>();
            for (int i = 0; i < animations.Count; ++i) {
                exSpriteAnimationClip clip = animations[i];
                if (clip != null) {
                    exSpriteAnimState state = new exSpriteAnimState(clip);
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

    private void Play (exSpriteAnimState _animState, float _time) {
        curAnimation = _animState;
        if (curAnimation != null) {
            curAnimation.time = _time;
            Step(curAnimation);
            exTextureInfo ti = curAnimation.clip.frameInfos[curIndex].textureInfo;
            if (ti != null) {
                sprite.textureInfo = ti;
            }
            enabled = true;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Step ( exSpriteAnimState _animState ) {
        if ( _animState == null ) {
            curWrappedTime = 0.0f;
            curIndex = -1;
            return;
        }
        curWrappedTime = _animState.clip.WrapSeconds(_animState.time, _animState.wrapMode);
        curIndex = _animState.frameTimes.BinarySearch(curWrappedTime);
        if ( curIndex < 0 ) {
            curIndex = ~curIndex;
        }
    }
}
