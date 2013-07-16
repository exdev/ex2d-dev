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

    [System.NonSerialized] public exTextureInfo[] clip; ///< the referenced sprite animation clip
    [System.NonSerialized] public string name; ///< the name of the sprite animation state
    //[System.NonSerialized] public WrapMode wrapMode; ///< the wrap mode
    //[System.NonSerialized] public exSpriteAnimClip.StopAction stopAction; ///< the stop action
    [System.NonSerialized] public float length; ///< the length of the sprite animation in seconds with speed = 1.0f

    [System.NonSerialized] public float speed = 1.0f; ///< the speed to play the sprite animation clip
    [System.NonSerialized] public float time = 0.0f; ///< the current time in seoncds
    // [System.NonSerialized] public float normalizedTime = 0.0f;
    //[System.NonSerialized] public List<float> frameTimes; ///< the list of the start time in seconds of each frame in the exSpriteAnimClip

    // ------------------------------------------------------------------ 
    /// \param _animClip the referenced animation clip
    /// Constructor of exSpriteAnimState, it will copy the settings from _animClip. 
    // ------------------------------------------------------------------ 

    public exSpriteAnimState ( exTextureInfo[] _animClip ) :
        this ( "", _animClip ) {
    }

    // ------------------------------------------------------------------ 
    /// \param _name the name of the animation state
    /// \param _animClip the referenced animation clip
    /// Constructor of exSpriteAnimState, it will copy the settings from _animClip. 
    // ------------------------------------------------------------------ 

    public exSpriteAnimState ( string _name, exTextureInfo[] _animClip ) {
        clip = _animClip;
        name = _name;
        //wrapMode = _animClip.wrapMode;
        //stopAction = _animClip.stopAction;
        //length = _animClip.length;
        //speed = _animClip.speed;

        //frameTimes = new List<float>(_animClip.frameInfos.Count);
        //float tmp = 0.0f;
        //for ( int i = 0; i < _animClip.frameInfos.Count; ++i ) {
        //    tmp += _animClip.frameInfos[i].length;
        //    frameTimes.Add(tmp);
        //}
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

    public exTextureInfo[] defaultAnimation;    // TODO: use exSpriteAnimClip

    // ------------------------------------------------------------------ 
    /// the list of sprite animation clips used in the component
    // ------------------------------------------------------------------ 

    public List<exTextureInfo[]> animations = new List<exTextureInfo[]>();

    // ------------------------------------------------------------------ 
    /// When playAutomatically set to true, it will play the 
    /// exSpriteAnimation.defaultAnimation at the start
    // ------------------------------------------------------------------ 

    public bool playAutomatically = false;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    //private bool initialized = false;
    //private Dictionary<string,exSpriteAnimState> nameToState;
    private exSpriteAnimState curAnimation;
    private exSprite sprite;
    private bool playing = false;
    //private int lastEventInfoIndex = -1;
    
    //private float curWrappedTime = 0.0f;
    private int curIndex = -1;
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////
    
	void Awake () {
        sprite = GetComponent<exSprite>();
        if ( playAutomatically && defaultAnimation != null ) {
            Play (new exSpriteAnimState(defaultAnimation), 0);
        }
        else {
            enabled = false;
        }
	}
	
	void Update () {
        if (playing && (curAnimation != null)) {
            // advance the time and check if we trigger any animation events
            float delta = Time.deltaTime * curAnimation.speed;
            //float curTime = curAnimation.time;

            // advance the time
            curAnimation.time += delta;
            if (Time.frameCount % 10 == 0) {
                Step(curAnimation);
            }

            // set sprite to current time
            if (curIndex < curAnimation.clip.Length){
                sprite.textureInfo = curAnimation.clip[curIndex];
            }
        }
	}

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    public void Play (exSpriteAnimState _animState, float _time) {
        curAnimation = _animState;
        if (curAnimation != null) {
            curAnimation.time = _time;
            playing = true;
            Step (curAnimation);
            exTextureInfo ti = curAnimation.clip[curIndex];
            if (ti != null) {
                sprite.textureInfo = ti;
            }
            enabled = true;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
        
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Step ( exSpriteAnimState _animState ) {
        if ( _animState == null ) {
            //curWrappedTime = 0.0f;
            curIndex = -1;
            return;
        }
        //curWrappedTime = _animState.clip.WrapSeconds(_animState.time, _animState.wrapMode);
        //curIndex = _animState.frameTimes.BinarySearch(curWrappedTime);
        if ( curIndex < 0 ) {
            curIndex = ~curIndex;
        }
        // Test
        ++curIndex;
        curIndex %= _animState.clip.Length;
    }
}
