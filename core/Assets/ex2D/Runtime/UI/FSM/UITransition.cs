// ======================================================================================
// File         : UITransition.cs
// Author       : Wu Jie 
// Last Change  : 11/04/2013 | 11:52:42 AM | Monday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
// UITransition
///////////////////////////////////////////////////////////////////////////////

namespace fsm {

    ///////////////////////////////////////////////////////////////////////////////
    // UITransition
    ///////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class UITransition : TimerTransition {
        public exUIPanel from = null; 
        public exUIPanel to = null; 

        public UITransition ()
            : base ()
        {
            onStart += delegate () {
                exUIMng.inst.enabled = false;
                from.StartFadeOut();
                to.StartFadeIn();
            };

            onEnd += delegate () {
                exUIMng.inst.enabled = true;
                from.FinishFadeOut();
                to.FinishFadeIn();
            };

            onTick += delegate ( float _ratio ) {
                from.FadeOut(_ratio);
                to.FadeIn(_ratio);
            };
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // UIState
    ///////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class UIState : State {
        public exUIPanel panel = null;

        public UIState ( exUIPanel _panel, State _parent = null ) 
            : base(_panel.name,_parent) 
        {
            panel = _panel;
            onEnter += delegate ( State _from, State _to ) {
                panel.Enter();
            };
            onExit += delegate ( State _from, State _to ) {
                panel.Exit();
            };
        }

        public void to ( UIState _targetState, System.Func<bool> _onCheck, float _duration ) {
            UITransition newTranstion = new UITransition ();

            newTranstion.source = this;
            newTranstion.target = _targetState;

            if ( _onCheck != null ) newTranstion.onCheck = _onCheck;

            newTranstion.duration = _duration;
            newTranstion.from = panel;
            newTranstion.to = _targetState.panel;

            transitionList.Add ( newTranstion );
        }
    }
}
