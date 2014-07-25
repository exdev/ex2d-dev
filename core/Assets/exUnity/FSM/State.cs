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

        // route state will check condition and transfer to 
        // the next state immediately before transition.onStart
        // it will then reset the initState of its parent.
        public bool isRoute = false; 

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

        protected List<Transition> transitionList = new List<Transition>();
        protected List<State> children = new List<State>();

        protected bool inTransition = false;
        protected Transition currentTransition = null;
        protected List<State> currentStates = new List<State>();

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

        public void CheckConditions () {
            // if we are in transtion, don't do anything
            if ( inTransition )
                return;

            //
            for ( int i = 0; i < currentStates.Count; ++i ) {
                State activeChild = currentStates[i];

                // NOTE: if parent transition triggerred, the child should always execute onExit transition
                for ( int j = 0; j < activeChild.transitionList.Count; ++j ) {
                    Transition transition = activeChild.transitionList[j];
                    if ( transition.onCheck() ) {

                        // exit states
                        transition.source.parent.ExitStates ( transition.target, transition.source );   // TODO: 这里应该不进行++i

                        // route happends here
                        if ( transition.target.mode == Mode.Exclusive &&
                             transition.target.children.Count > 0 ) 
                        {
                            State firstChild = transition.target.children[0];
                            if ( firstChild.isRoute ) {
                                firstChild.CheckRoute(transition);
                            }
                        }

                        // transition on start
                        if ( transition.onStart != null ) transition.onStart();
                        
                        // set current transition
                        currentTransition = transition;
                        inTransition = true;

                        break;
                    }
                }

                if ( inTransition == false ) {
                    activeChild.CheckConditions ();
                }
            }
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void CheckRoute ( Transition _parentTransition ) {
            bool hasTransition = false;
            for ( int i = 0; i < transitionList.Count; ++i ) {
                Transition transition = transitionList[i];
                transition.Bridge(_parentTransition);

                if ( transition.onCheck() ) {
                    parent.initState = transition.target;
                    hasTransition = true;
                    break;
                }
            }
            if ( hasTransition == false ) {
                Debug.LogError( "FSM error: route state must have transition" );
            }
        }


        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public void UpdateTransitions () {
            if ( inTransition ) {
                // update transition
                if ( currentTransition.onTransition() ) {

                    // transition on end
                    if ( currentTransition.onEnd != null ) currentTransition.onEnd();

                    // enter states
                    State targetState = currentTransition.target;
                    if ( targetState == null )
                        targetState = currentTransition.source;

                    if ( targetState.parent != null )
                        targetState.parent.EnterStates ( targetState, currentTransition.source );
                    else {
                        Debug.Log( "targetState = " + targetState.name + ", " + name );
                    }

                    //
                    currentTransition = null;
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
                if (ReferenceEquals(currentTransition.target, _state)) {
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
            if ( currentTransition != null ) {
                return currentTransition;
            }

            State p = parent_;
            while ( p != null ) {
                if ( p.inTransition && p.currentTransition != null )
                    return p.currentTransition;
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
                        suffix = " >>>";
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
                s.ShowDebugInfo ( _level + 1, currentStates.IndexOf(s) != -1, _textStyle, inTransition ? currentTransition : _t );
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
}

