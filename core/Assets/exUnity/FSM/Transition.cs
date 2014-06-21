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
    }

    ///////////////////////////////////////////////////////////////////////////////
    // TimerTransition
    ///////////////////////////////////////////////////////////////////////////////

    public class TimerTransition : Transition {
        public float duration = 1.0f; 
        public System.Action<float> onTick = null; // onTick ( timer/duration )

        float timer = 0.0f; 

        public TimerTransition () {
            onStart += delegate () {
                timer = 0.0f;
            };
            onTransition = delegate () {
                timer += Time.deltaTime;

                if ( onTick != null )
                    onTick ( timer/duration );

                // time up
                if ( timer >= duration ) {
                    return true;
                }
                return false;
            };
        }
    }
}

