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
        public System.Func<bool> onCheck = delegate () { return false; };   // 该函数会多次调用，不应改变内部状态

        public System.Action onStart = null;
        public System.Func<bool> onTransition = delegate () { return true; }; 
        public System.Action onEnd = null;

        public virtual void OnRouteFrom ( Transition _from ) {}
    }

    ///////////////////////////////////////////////////////////////////////////////
    // TimerTransition
    ///////////////////////////////////////////////////////////////////////////////

    public class TimerTransition : Transition {
        public virtual float duration { get; set; }
        public System.Action<float> onTick = null; // onTick ( timer/duration )

        float timer = 0.0f;

        public TimerTransition () {
            onStart += delegate () {
                timer = 0.0f;
            };
            onTransition = delegate () {
                timer += Time.deltaTime;

                if ( onTick != null )
                    onTick ( duration != 0.0f ? timer / duration : 0.0f );

                // time up
                if ( timer >= duration ) {
                    return true;
                }
                return false;
            };
        }
    }
}

