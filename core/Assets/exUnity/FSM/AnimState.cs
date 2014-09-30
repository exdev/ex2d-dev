using UnityEngine;
using System.Collections.Generic;

namespace fsm {

/// <summary>
/// 动画状态机的跳转，实例由AnimState.to方法进行创建。
/// 两个状态机之间可以有任意多个Transition，只要有其中一个被触发，状态就会发生跳转。
/// 当前Transition是否触发的判定方法为：
/// > if (sync != null) then return (sync can trigger now)
/// > if (Trigger == true) then return true
/// > if (when != null and after != null) then 
/// >     return when() is OK AND after() is OK
/// > else
/// >     return (when != null and when() is OK) OR (after != null and after() is OK)
/// 提示：如果希望when或after只要满足一个就触发，需要创建多个AnimTransition实例。
/// </summary>
public class AnimTransition : TimerTransition {

    // We assume that the playing states should not keep the reference of any transition,
    // so we just create a global shared dummy transition for routing.
    // Otherwise we should pass an unique transition everytime we route it
    private static AnimTransition routingTransition = new AnimTransition();

    /// <summary> 触发动画跳转，不论when、after条件是否满足。 </summary>
    public bool syncNormalizedTime = false;
    public float scale = 1.0f;  // scale duration

    System.Nullable<float> exitTime;
    float exitTimeOffset = 0.0f;
    System.Func<bool> onDoCheck;
    AnimTransition syncWith = null;
    string[] fromNameList = null;

    public override float duration {
        get { return base.duration * scale; }
        set { base.duration = value; }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    public AnimTransition ()
        : base () {
        onCheck = OnCheck;
        onStart += PlayNextAnimState;
    }

    /// <summary> 当条件达成就可以触发跳转，和after是与(and)的布尔关系。 </summary>
    /// <param name="_onCheck">
    /// 条件函数，返回是否可以跳转。
    /// 这个方法不能有任何副作用，如果需要修改变量，应该在state的回调里修改！
    /// </param>
    public AnimTransition when (System.Func<bool> _onCheck = null) {
        onDoCheck = _onCheck;
        return this;
    }

    /// <summary> 限制只有动画播放到了一定的时候才能跳转，和when是与(and)的布尔关系。 </summary>
    /// <param name="_normalizedTime">
    /// 和Mecanim里的Exit Time一样，用于限制退出这个状态所需要的时间。
    /// 如果是0.5则代表必须等到动画播放到一半后才能切换到其它状态，2.5则代表要等动画loop 2.5次后才能切换到其它状态。
    /// </param>
    /// <param name="_offset"> 
    /// 为0时，_normalizedTime指定的是动画开始crossfade的时刻；为1.0时，_normalizedTime指定的是动画crossfade完成后的时刻。
    /// </param>
    public AnimTransition after (float _normalizedTime, float _offset = 0.0f) {
        exitTime = _normalizedTime;
        exitTimeOffset = _offset;
        return this;
    }

    public AnimTransition from ( params string[] _names ) {
        fromNameList = _names;
        return this;
    }

    public AnimTransition sync ( AnimTransition _transitionToSync ) {
        syncWith = _transitionToSync;
        return this;
    }

    void PlayNextAnimState () {
        AnimState animState = target as AnimState;
        if ( animState != null ) {
            animState.Play(this);
        }
    }

    public bool TestOwnConditions () {
        //
        if ( fromNameList != null ) {
            string soureName = null;
            try {
                soureName = target.parent.parent.currentTransition.source.name;
            }
            catch (System.NullReferenceException) {
                return false;
            }
            bool found = (System.Array.IndexOf(fromNameList, soureName) != -1);
            if (found == false) {
                return false;
            }
        }

        //
        if ( exitTime.HasValue ) {
            if (source == null) {
                return false;
            }
            AnimState s = (AnimState)source;
            var state = s.anim[s.curAnimName];
            if (state == null) {
                return false;
            }
            float normalizedTime = s.normalizedTime;
#if UNITY_EDITOR
            if ( normalizedTime >= 1.0f && exitTime.HasValue && exitTime.Value > 1.0f ) {
                if (state != null && (state.wrapMode != WrapMode.Loop && state.wrapMode != WrapMode.PingPong && state.wrapMode != WrapMode.ClampForever)) {
                    Debug.LogWarning(string.Format("非循环动画的normalizedTime永远不可能大于1 ({0} to {1})", source.name, target.name));
                }
            }
#endif
            if (normalizedTime * state.length < exitTime.Value * state.length - exitTimeOffset * duration) {
                return false;
            }
        }

        //
        if (onDoCheck != null) {
            return onDoCheck();
        }

        bool defaultTrue = target != null && target.parent.hasRouteIn;
        return defaultTrue;
    }

    bool OnCheck () {
        if ( syncWith != null ) {
            bool isSyncStateActive = (syncWith.source.parent.IsActiveState(syncWith.source, false));
            return isSyncStateActive && syncWith.TestOwnConditions();
        }
        bool result = TestOwnConditions();
        //trigger = false;
        return result;
    }

    public override void OnRouteFrom ( Transition _from ) {
        exDebug.Assert(_from != null, "Previous transition should not be null!");
        base.OnRouteFrom(_from);
        TimerTransition timerTrans = _from as TimerTransition;
        if ( timerTrans != null ) {
            routingTransition.source = _from.source;
            routingTransition.target = target;
            routingTransition.duration = duration + timerTrans.duration;
            AnimState animState = target as AnimState;
            if ( animState != null ) {
                animState.Play(routingTransition);
            }
        }
    }
}

/// <summary>
/// 动画状态机的基本状态，跳转到新状态时将播放对应动作
/// </summary>
public class AnimState : State {

    public Animation anim = null;
    private string curAnimName_ = null;

    public string curAnimName {
        get { return curAnimName_; }
        set {
            curAnimName_ = value;
            var group = parent_ as AnimState;
            if (group != null) {
                group.curAnimName = syncGroupNTime ? value : null;
	        }
        }
    }

    /// <summary>
    /// Sync group's normalized time with this animation, so that its parent group's transition can support after() condition.
    /// </summary>
    public bool syncGroupNTime = false;

    ///////////////////////////////////////////////////////////////////////////////
    // Properties
    ///////////////////////////////////////////////////////////////////////////////

    public float normalizedTime {
        get {
            //AnimationState animState = anim.GetPlayingAnimation();
            AnimationState animState = anim[curAnimName_];
            //Debug.Log(string.Format("animState: {0} " + animState.normalizedTime, animState.name));

            if ( animState != null ) {
                if ( animState.wrapMode == WrapMode.Once ||
                     animState.wrapMode == WrapMode.Default ) 
                {
                    if ( animState.normalizedTime == 0.0f )
                        return 1.0f;
                }

                return animState.normalizedTime;
            }
            else {
                return 0.0f;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    /// <param name="_name"> 状态名，用于调试，同时作为当前状态所播放的动作名(也就是AnimationClip.name) </param>
    public AnimState (string _name, Animation _anim, State _parent = null) 
        : base(_name, _parent) 
    {
        anim = _anim;
        exDebug.Assert(anim != null);
        InitWrapMode(_name);
    }

    /// <summary> 声明状态的跳转 </summary>
    /// <param name="_targetState"> 目标状态 </param>
    /// <param name="_duration"> 动画过渡所需时间 </param>
    /// <param name="_syncNTime"> 跳转开始时，将目标动画的normalized time设置成和当前动画一致 </param>
    public AnimTransition to (State _targetState, float _duration = 0.3f, bool _syncNTime = false) {
        AnimTransition transition = new AnimTransition() {
            source = this,
            target = _targetState,
            duration = _duration,
            syncNormalizedTime = _syncNTime,
        };
        transitionList.Add (transition);
        return transition;
    }

    /// <summary> 播放本状态的动作 </summary>
    public virtual void Play (Transition _transition) {
        DoPlay(_transition, name);
    }

    public virtual AnimState Clone ( State _parent ) {
        string cloneName = name + "_clone";
        anim.AddClip( anim[name].clip, cloneName );

        return new AnimState(cloneName, anim, _parent);
    }

    public virtual void SyncSpeed ( float _speed ) {
        AnimationState curState = anim[name];
        curState.speed = _speed;
    }

    protected void DoPlay (Transition _transition, string _animName) {
        if (string.IsNullOrEmpty(_animName)) {
            anim.Stop();
            curAnimName = null;
            return;
        }

        // NOTE: we will change transition's scale here
        AnimTransition animTrans = _transition as AnimTransition;
        AnimationState nextAnimationState = anim[_animName];
        if ( nextAnimationState &&
             animTrans != null &&
             nextAnimationState.speed != 0.0f ) 
        {
            animTrans.scale = 1.0f/nextAnimationState.speed;
        }

        // check if rewind
        if (_animName == curAnimName) {
            anim.Rewind(_animName);
        }

        //
        if (animTrans == null || animTrans.duration == 0.0f) {
            anim.Play(_animName);
        }
        else {
            // sync n-time
            if ( animTrans.syncNormalizedTime ) {
                AnimState lastState = _transition.source as AnimState;
                if ( lastState != null ) {
                    nextAnimationState.normalizedTime = lastState.normalizedTime;
                }

                anim.Blend(lastState.curAnimName, 0.0f, animTrans.duration);
                anim.Blend(_animName, 1.0f, animTrans.duration);
            }
            else {
                anim.CrossFade(_animName, animTrans.duration);
            }
        }

        curAnimName = _animName;
    }

    /// <summary> 
    /// By default, our animation are Loop, ClampForever or PingPong. Only upperBody animation needs 
    /// Once. The Once animation will be blend to stop automatically when it finished. 
    /// </summary>
    protected void InitWrapMode (string _animName) {
        AnimationState s = anim[_animName];
        if (s != null && (s.wrapMode == WrapMode.Default || s.wrapMode == WrapMode.Once)) {
            s.wrapMode = WrapMode.ClampForever;
        }
    }
}

/// <summary>
/// 做上半身动画时，可以用NullAnimState来表示什么都不播的状态。
/// </summary>

public class NullAnimState : AnimState {
    public NullAnimState (string _name, Animation _anim, State _parent = null)
        : base(_name, _anim, _parent)
    {
    }

    public override void Play (Transition _transition) {
    }
}

/// <summary>
/// 用于动画从ClampForever到结束。
/// NOTE: 正常情况下我们不应该使用这个状态，ClampForver 是一个需要和其他状态CrossFade配合使用的动画模式
/// 而如果我们在layer1里，应该使用WrapMode.Once。
/// 目前仅有一个上下半身sync的状态，由于WrapMode.Once比WrapMode.ClampForever早一帧结束的Bug的极端情况需要使用这个类
/// </summary>

public class NullBlendToStopAnimState : AnimState {
    public NullBlendToStopAnimState (string _name, Animation _anim, State _parent = null)
        : base(_name, _anim, _parent)
    {
        AnimState from = null;
        onEnter += delegate ( State _from, State _to ) {
            from = _from as AnimState;
        };
        onAction += delegate ( State _cur ) {
            if ( from != null ) {
                Stop(from,true); // stop when weight is zero;
            }
        };
        onExit += delegate ( State _from, State _to ) {
            // in case we are doing state transition, but the animation still not blend to zero
            if ( from != null ) {
                Stop(from); // stop any way
            }
        };
    }

    public override void Play (Transition _transition) {
        AnimState src = _transition.source as AnimState;
        if ( src != null ) {
            Blend( src, 0.0f, _transition );
        }
    }

    public void Blend ( AnimState _s, float _to, Transition _transition) {
        if ( _s.curAnimName != null ) {
            _s.anim.Blend(_s.curAnimName, _to, ((TimerTransition)_transition).duration);
        }
    }

    public void Stop ( AnimState _s, bool _checkWeight = false ) {
        if ( _s.curAnimName != null ) {
            if ( _checkWeight && _s.anim[_s.curAnimName].weight == 0.0f ) {
                _s.anim.Stop(_s.curAnimName);
            }
            else {
                _s.anim.Stop(_s.curAnimName);
            }
        }
    }
}

/// <summary>
/// GroupAnimState
/// </summary>

public class GroupAnimState : AnimState {

    //public override float normalizedTime {
    //    get {
    //        return isEnd ? 1.0f : 0.0f;
    //    }
    //}

    public GroupAnimState (string _name, Animation _anim, State _parent = null)
        : base(_name, _anim, _parent)
    {
    }

    public override void Play (Transition _transition) {
        // if routed, animation was played by transition
        bool playRouteAnim = _transition != null && hasRouteIn;
        AnimState animState = initState as AnimState;
        if ( animState != null && playRouteAnim == false ) {
            animState.Play(_transition);
        }
    }

    /// <summary> 声明一个路由，用于选择进入Group后触发哪个子状态，用法请参考AnimState.to </summary>
    public AnimTransition routeIn (State _targetState, float _duration = 0.3f, bool _syncNTime = false) {
        exDebug.Assert(_targetState.parent == this, "Can not route into other state");
        if (routeInTransitions == null) {
            routeInTransitions = new List<Transition>();
	    }
        AnimTransition transition = new AnimTransition() {
            source = null,
            target = _targetState,
            duration = _duration,
            syncNormalizedTime = _syncNTime,
        };
        routeInTransitions.Add (transition);
        return transition;
    }
}

/// <summary>
/// BodypartAnimState
/// </summary>

public class BodypartAnimState : AnimState {
    Transform mixing;
    bool mixingAdded = false;

    public BodypartAnimState (string _name, Animation _anim, Transform _mixing, State _parent = null)
        : base(_name, _anim, _parent ) 
    {
        // init raw state
        AnimationState state = _anim[_name];
        state.layer = 1;
        state.wrapMode = WrapMode.ClampForever; // NOTE: WrapMode.Once have bug with .Clone() state. It will be one-frame earlier than cloned state to stop
        mixing = _mixing;
        AddMixingTransform();
    }

    public override AnimState Clone ( State _parent ) {
        AnimState newAnimState = base.Clone(_parent); 

        string cloneName = newAnimState.name;
        AnimationState cloneState = anim[cloneName];
        cloneState.layer = 0;
        cloneState.wrapMode = WrapMode.ClampForever;

        return newAnimState;
    }

    public void AddMixingTransform () {
        if ( mixingAdded == false ) {
            mixingAdded = true;
            anim[name].AddMixingTransform(mixing);
            anim[name].blendMode = AnimationBlendMode.Blend;
        }
    }

    public void RemoveMixingTransform () {
        if ( mixingAdded ) {
            anim[name].RemoveMixingTransform(mixing);
            mixingAdded = false;
        }
    }
}

namespace Detail {

    /// <summary>
    /// 包含多个动作的状态
    /// </summary>
    public abstract class MultiAnimStateBase : AnimState {
        public string[] animList = null;

        /// <param name="_animList"> 以";"分割的动画名称列表 </param>
        /// <param name="_name"> 状态名，用于调试 </param>
        public MultiAnimStateBase (string _name, Animation _anim, string _animList, State _parent = null)
            : base(_name, _anim, _parent) {
            animList = _animList.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (animList.Length == 0) {
                Debug.LogError("no animation is specified");
                return;
            }

            for ( int i = 0; i < animList.Length; ++i ) {
                string animName = animList[i];
                InitWrapMode(animName);
            }
        }
    }

}

/// <summary>
/// 自动切换动作的状态，用于播放不同动作
/// </summary>
public class MultiAnimState : Detail.MultiAnimStateBase {

    public enum PlayMode {
        Loop,       // 依次循环播放一系列动作
        Random,     // 随机播放
    }

    PlayMode playMode;
    int lastAnimIndex = -1;

    /// <param name="_animList"> 以";"分割的动画名称列表 </param>
    /// <param name="_name"> 状态名，用于调试 </param>
    public MultiAnimState (string _name, Animation _anim, string _animList, PlayMode _playMode, State _parent = null)
        : base(_name, _anim, _animList, _parent) {
        playMode = _playMode;
    }

    public override void Play (Transition _transition) {
        switch (playMode) {
        case PlayMode.Loop:
            lastAnimIndex = (lastAnimIndex+1) % animList.Length;
            DoPlay(_transition, animList[lastAnimIndex]);
            break;
        case PlayMode.Random:
            DoPlay(_transition, animList[Random.Range(0, animList.Length)]);
            break;
        }
    }

    public override void SyncSpeed ( float _speed ) {
        for ( int i = 0; i < animList.Length; ++i ) {
            AnimationState curState = anim[animList[i]];
            curState.speed = _speed;
        }
    }

    /// <summary> 重置播放索引 </summary>
    public void Reset() {
        //exDebug.Assert(selectType == PlayMode.Loop);
        lastAnimIndex = -1;
    }
}

//public class BlendTree : Detail.MultiAnimStateBase

}
