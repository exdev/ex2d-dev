// ======================================================================================
// File         : Transition.cs
// Author       : Wu Jie 
// Last Change  : 12/20/2011 | 12:02:07 PM | Tuesday,December
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

namespace fsm {

    ///////////////////////////////////////////////////////////////////////////////
    // Transition
    ///////////////////////////////////////////////////////////////////////////////

    public class Transition {

        ///////////////////////////////////////////////////////////////////////////////
        // properties
        ///////////////////////////////////////////////////////////////////////////////

        public Machine machine {
            get {
                if ( source != null )
                    return source.machine;
                return null;
            }
        }

        public State source = null;
        public State target = null;
        public System.Func<bool> onCheck = delegate () { return false; }; 

        public System.Action onStart = null;
        public System.Func<bool> onTransition = delegate () { return true; }; 
        public System.Action onEnd = null;

        public virtual void Bridge ( Transition _transition ) {
            if ( _transition == null )
                source = null;
            else
                source = _transition.source;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // TimerTransition
    ///////////////////////////////////////////////////////////////////////////////

    public class TimerTransition : Transition {
        public float duration {
            get { return duration_ * scale; }
            set { duration_ = value; }
        }
        public float scale = 1.0f; 
        public System.Action<float> onTick = null; // onTick ( timer/duration )

        float timer = 0.0f; 
        float duration_ = 1.0f;

        public TimerTransition () {
            onStart += delegate () {
                timer = 0.0f;
            };
            onTransition = delegate () {
                timer += Time.deltaTime;
                float finalDuration = duration_ * scale;

                if ( onTick != null )
                    onTick ( finalDuration != 0.0f ? timer / finalDuration : 0.0f );

                // time up
                if ( timer >= finalDuration ) {
                    return true;
                }
                return false;
            };
        }

        public override void Bridge ( Transition _transition ) {
            base.Bridge(_transition);
            TimerTransition timerTrans = _transition as TimerTransition;
            if ( timerTrans != null ) {
                timerTrans.duration_ = duration_;
            }
        }
    }
}

