// ======================================================================================
// File         : State.cs
// Author       : Wu Jie 
// Last Change  : 12/20/2011 | 11:51:04 AM | Tuesday,December
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
    using UnityEditor;
#endif

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

namespace fsm {

    ///////////////////////////////////////////////////////////////////////////////
    // State
    ///////////////////////////////////////////////////////////////////////////////

    public class State {

        public enum Mode {
            Exclusive,
            Parallel,
        }

        ///////////////////////////////////////////////////////////////////////////////
        // properties
        ///////////////////////////////////////////////////////////////////////////////

        public string name = "";
        public Mode mode = Mode.Exclusive;

        // group state will check conditions and transfer to 
        // other sub state instead of initState.
        public List<Transition> routeInTransitions = null;
        public bool hasRouteIn {
            get {
                return routeInTransitions != null && routeInTransitions.Count > 0;
            }
        }
        // if hasRouteOut, state can not exit unless its child RouteOutState can be activated
        public bool containRouteOut = false;
        // once its sub transition checked its sub route out state, isEnd become true
        public bool isEnd = false;

        protected State parent_ = null;
        public State parent {
            set {
                if ( parent_ != value ) {
                    State oldParent = parent_;

                    // check if it is parent layer or child
                    while ( parent_ != null ) {
                        if ( parent_ == this ) {
                            Debug.LogWarning("can't add self or child as parent");
                            return;
                        } 
                        parent_ = parent_.parent;
                    }

                    //
                    if ( oldParent != null ) {
                        if ( oldParent.initState == this )
                            oldParent.initState = null;
                        oldParent.children.Remove(this);
                    }

                    //
                    if ( value != null ) {
                        value.children.Add(this);
                        // if this is first child 
                        if ( value.children.Count == 1 )
                            value.initState = this; 
                    }
                    parent_ = value;
                }
            }
            get { return parent_; }
        }

        protected Machine machine_ = null;
        public Machine machine {
            get {
                if ( machine_ != null )
                    return machine_;

                State last = this; 
                State root = parent; 
                while ( root != null ) {
                    last = root;
                    root = root.parent;
                }
                machine_ = last as Machine; // null is possible
                return machine_;
            }
        }

        protected State initState_ = null;
        public State initState {
            get { return initState_; }
            set {
                if ( initState_ != value ) {
                    if ( value != null && children.IndexOf(value) == -1 ) {
                        Debug.LogError ( "FSM error: You must use child state as initial state." );
                        initState_ = null;
                        return;
                    }
                    initState_ = value;
                }
            }
        }
        protected Transition currentTransition_ = null;
        public Transition currentTransition { get { return currentTransition_; } }

        protected List<Transition> transitionList = new List<Transition>();
        protected List<State> children = new List<State>();

        protected bool inTransition = false;
        protected List<State> currentStates = new List<State>();

        public bool trigger = false;

        ///////////////////////////////////////////////////////////////////////////////
        // event handles
        ///////////////////////////////////////////////////////////////////////////////

        public System.Action<State, State> onEnter = null;
        public System.Action<State, State> onExit = null;
        public System.Action<State> onAction = null;
        
        ///////////////////////////////////////////////////////////////////////////////
        // functions
        ///////////////////////////////////////////////////////////////////////////////

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public State ( string _name, State _parent = null ) {
            name = _name;
            parent = _parent;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void ClearCurrentStatesRecursively () {
            currentStates.Clear();
            for ( int i = 0; i < children.Count; ++i ) {
                children[i].ClearCurrentStatesRecursively ();
            }
        }

        // ------------------------------------------------------------------ 
        // Desc: add transition
        // ------------------------------------------------------------------ 

        public T Add<T> ( State _targetState, 
                          System.Func<bool> _onCheck = null, 
                          System.Action _onStart = null,
                          System.Func<bool> _onTransition = null,
                          System.Action _onEnd = null ) where T : Transition, new() {
            T newTranstion = new T ();
            newTranstion.source = this;
            newTranstion.target = _targetState;
            if ( _onCheck != null ) newTranstion.onCheck = _onCheck;

            if ( _onStart != null ) newTranstion.onStart = _onStart;
            if ( _onTransition != null ) newTranstion.onTransition = _onTransition;
            if ( _onEnd != null ) newTranstion.onEnd = _onEnd;

            transitionList.Add ( newTranstion );
            return newTranstion;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void OnAction () { 
            if ( onAction != null ) onAction ( this );         

            for ( int i = 0; i < currentStates.Count; ++i ) {
                currentStates[i].OnAction ();

                // DISABLE { 
                // if ( machine != null && machine.logDebugInfo ) 
                //     Debug.Log( "FSM Debug: On Action - " + currentStates[i].name + " at " + Time.time );
                // } DISABLE end 
            }
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        private Transition SelectTransition () {
            for (int i = 0; i < transitionList.Count; ++i) {
                Transition transition = transitionList[i];
                if (transition.onCheck() || (transition.target != null && transition.target.trigger)) {
                    if (transition.target is RouteOutState) {
                        parent.isEnd = true;
                        // check parent's transtion
                        var parentTransition = parent.SelectTransition(); // TODO: cache result
                        if (parentTransition != null) {
                            return transition;
                        }
                    }
                    else {
                        return transition;
                    }
                }
            }
            return null;
        }

        // ------------------------------------------------------------------ 
        // Transition to another child state
        // ------------------------------------------------------------------ 

        private void Transition (State sourceSubState, Transition transition) {
            exDebug.Assert(sourceSubState.parent == this);
            exDebug.Assert(transition.source == sourceSubState);
            // NOTE: if parent transition triggerred, the child should always execute onExit transition
            // exit states
            ExitStates(transition.target, sourceSubState);
            // route out
            if (transition.target is RouteOutState) {
                exDebug.Assert(transition.target.hasRouteIn == false, "RouteOutState not allowed to have route in");

                var parentTransition = SelectTransition();
                exDebug.Assert(parentTransition != null, "No valid transition to route oute!");
                parentTransition.OnRouteFrom(transition);
                parent.Transition(this, parentTransition);
                // if routing out, this state will be exit immediately by calling parent.Transition(),
                // so we dont need to mark its transition here.
            }
            else {
                // update state
                currentTransition_ = transition;
                inTransition = true;
                // route in
            	if (transition.target.mode == Mode.Exclusive) {
                    transition.target.CheckRoute(transition);
                }
            }
            // transition on start
            if (transition.onStart != null) transition.onStart();
            // reset trigger
            transition.target.trigger = false;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void CheckConditions () {
            // if we are in transtion, don't do anything
            if ( inTransition )
                return;

            //
            for ( int i = 0; i < currentStates.Count; ++i ) {
                State activeChild = currentStates[i];
#if UN_INTERRUPTABLE
                if (activeChild.containRouteOut) {
                    // we dont need to check activeChild's transition here, because its child states will check its route out transitions
                    activeChild.CheckConditions ();
                }
                else {
#endif
                    var nextTransition = activeChild.SelectTransition();
                    if (nextTransition != null) {
                        // TODO: 这里应该不进行++i，因为currentStates已经改变
                        Transition(activeChild, nextTransition);
	                }
                    else {
                        activeChild.CheckConditions ();
                    }
#if UN_INTERRUPTABLE
                }
#endif
            }
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void CheckRoute ( Transition _parentTransition ) {
            if (hasRouteIn) {
                exDebug.Assert(_parentTransition == null || 
                               _parentTransition.source.parent.currentTransition_ == _parentTransition,
                               "Routing rely on currentTransition");

                bool hasTransition = false;
                for ( int i = 0; i < routeInTransitions.Count; ++i ) {
                    Transition transition = routeInTransitions[i];
                    if ( transition.onCheck() || (transition.target != null && transition.target.trigger) ) {
                        transition.target.trigger = false;
                        initState = transition.target;
                        if (_parentTransition != null) {
                            transition.OnRouteFrom(_parentTransition);
                        }
                        hasTransition = true;
                        break;
                    }
                }
                if ( hasTransition == false ) {
                    Debug.LogError( "FSM error: route state must have transition" );
                }
            }
        }


        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void UpdateTransitions () {
            if ( inTransition ) {
                // update transition
                if ( currentTransition_.onTransition() ) {

                    // transition on end
                    if ( currentTransition_.onEnd != null ) currentTransition_.onEnd();

                    // enter states
                    State targetState = currentTransition_.target;
                    if ( targetState == null )
                        targetState = currentTransition_.source;

                    if ( targetState.parent != null )
                        targetState.parent.EnterStates ( targetState, currentTransition_.source );
                    else {
                        Debug.Log( "targetState = " + targetState.name + ", " + name );
                    }

                    //
                    currentTransition_ = null;
                    inTransition = false;
                }
            }
            else {
                for ( int i = 0; i < currentStates.Count; ++i ) {
                    State activeChild = currentStates[i];
                    activeChild.UpdateTransitions();
                }
            }
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void EnterStates ( State _toEnter, State _toExit ) {
            _toEnter.isEnd = false;
            currentStates.Add (_toEnter);
            if ( machine != null && machine.logDebugInfo ) 
                Debug.Log( "FSM Debug: Enter State - " + _toEnter.name + " at " + Time.time );

            if ( _toEnter.onEnter != null )
                _toEnter.onEnter ( _toExit, _toEnter );

            if ( _toEnter.children.Count != 0 ) {
                if ( _toEnter.mode == State.Mode.Exclusive ) {
                    if ( _toEnter.initState != null ) {
                        _toEnter.EnterStates( _toEnter.initState, _toExit );
                    }
                    else {
                        Debug.LogError( "FSM error: can't find initial state in " + _toEnter.name );
                    }
                }
                else { // if ( _toEnter.mode == State.Mode.Parallel )
                    for ( int i = 0; i < _toEnter.children.Count; ++i ) {
                        _toEnter.EnterStates( _toEnter.children[i], _toExit );
                    }
                }
            }
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void ExitStates ( State _toEnter, State _toExit ) {
            _toExit.ExitAllStates ( _toEnter );

            if ( machine != null && machine.logDebugInfo ) 
                Debug.Log( "FSM Debug: Exit State - " + _toExit.name + " at " + Time.time );

            if ( _toExit.onExit != null )
                _toExit.onExit ( _toExit, _toEnter );

            currentStates.Remove (_toExit);
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public bool IsActiveState (State _state, bool _containsTransTarget = true) {
            if (inTransition && _containsTransTarget) {
                if (ReferenceEquals(currentTransition_.target, _state)) {
                    return true;
                }
            }
            for ( int i = 0; i < currentStates.Count; ++i ) {
                State child = currentStates[i];
                if (ReferenceEquals(child, _state)) {
                    return true;
                }
                if (child.IsActiveState(_state, _containsTransTarget)) {
                    return true;
                }
            }
            return false;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public bool IsAncientOf ( State _state ) {
            State p = _state.parent_;
            while ( p != null ) {
                if ( p == this )
                    return true;
                p = p.parent_;
            }
            return false;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        protected void ExitAllStates ( State _toEnter ) {
            for ( int i = 0; i < currentStates.Count; ++i ) {

                State activeChild = currentStates[i];
                activeChild.ExitAllStates ( _toEnter );

                if ( activeChild.onExit != null )
                    activeChild.onExit ( activeChild, _toEnter );

                if ( machine != null && machine.logDebugInfo ) 
                    Debug.Log( "FSM Debug: Exit State - " + activeChild.name + " at " + Time.time );
            }
            currentStates.Clear();
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public int TotalStates () {
            int count = 1;
            for ( int i = 0; i < children.Count; ++i ) {
                count += children[i].TotalStates();
            }
            return count;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        protected Transition GetCurrentTransition () {
            if ( currentTransition_ != null ) {
                return currentTransition_;
            }

            State p = parent_;
            while ( p != null ) {
                if ( p.inTransition && p.currentTransition_ != null )
                    return p.currentTransition_;
                p = p.parent_;
            }

            return null;
        }

#if UNITY_EDITOR

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void ShowDebugInfo ( int _level, bool _active, GUIStyle _textStyle, Transition _t ) {

            Color colorEnter = Color.green;
            Color colorExit = Color.red;
            Color colorActive = Color.blue;
            Color colorDeactive = new Color( 0.5f, 0.5f, 0.5f );

            if ( EditorGUIUtility.isProSkin ) {
                colorEnter = Color.yellow;
                colorExit = Color.red;
                colorActive = Color.green;
                colorDeactive = new Color( 0.4f, 0.4f, 0.4f );
            }

            string suffix = "";
            if ( _active ) {
                if ( inTransition ) {
                    _textStyle.fontStyle = FontStyle.Bold;
                    suffix = " *";
                }
                else {
                    _textStyle.fontStyle = FontStyle.Normal;
                }
                _textStyle.normal.textColor = colorActive;
            }
            else {
                _textStyle.fontStyle = FontStyle.Normal;
                if ( _t != null ) {
                    if ( _t.source == this ) {
                        suffix = (_t.target != this) ? " >>>" : " <->";
                        _textStyle.normal.textColor = colorExit;
                    }
                    else if ( _t.target == this ) {
                        suffix = " <<<";
                        _textStyle.normal.textColor = colorEnter;
                    }
                    else {
                        _textStyle.normal.textColor = colorDeactive;
                    }
                }
                else {
                    _textStyle.normal.textColor = colorDeactive;
                }
            }
            GUILayout.BeginHorizontal ();
                GUILayout.Space(5);
                GUILayout.Label ( new string('\t',_level) + name + suffix, _textStyle, new GUILayoutOption[] {} );
            GUILayout.EndHorizontal ();

            for ( int i = 0; i < children.Count; ++i ) {
                State s = children[i];
                s.ShowDebugInfo ( _level + 1, currentStates.IndexOf(s) != -1, _textStyle, inTransition ? currentTransition_ : _t );
            }
        }

#endif

    }

    ///////////////////////////////////////////////////////////////////////////////
    // FinalState
    ///////////////////////////////////////////////////////////////////////////////

    public class FinalState : State {

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public FinalState ( string _name, State _parent = null ) 
            : base ( _name, _parent )
        {
            onEnter += OnFinished;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        void OnFinished ( State _from, State _to ) {
            // TODO { 
            // Machine stateMachine = machine;
            // if ( stateMachine != null ) {
            //     stateMachine.Send ( Event.FINISHED );
            // }
            // } TODO end 
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // RouteOutState
    ///////////////////////////////////////////////////////////////////////////////

    public class RouteOutState : State {
        public RouteOutState (string _name, State _parent = null)
            : base (_name, _parent) {
            exDebug.Assert(_parent != null);
            _parent.containRouteOut = true;
        }
    }
}

