// // ======================================================================================
// // File         : exMesh.cs
// // Author       : Jare
// // Last Change  : 21 / 07 / 2013
// // Description  : 
// // ======================================================================================

using UnityEngine;
using System.Collections;

public static class exMath {
    
    // ------------------------------------------------------------------ 
    /// \param _length the length used for wrapping
    /// \param _value the in value to wrap
    /// \param _wrapMode the wrap mode used for wrapping
    /// wrap the seconds of the anim clip by the wrap mode
    // ------------------------------------------------------------------ 

    public static float Wrap ( float _value, float _length, WrapMode _wrapMode ) {
        float t = Mathf.Abs(_value);
        if ( _wrapMode == WrapMode.Loop ) {
            t %= _length;
        }
        else if ( _wrapMode == WrapMode.PingPong ) {
            int cnt = (int)(t/_length);
            t %= _length;
            if ( cnt % 2 == 1 ) {
                t = _length - t;
            }
        }
        else {
            t = Mathf.Clamp( t, 0.0f, _length );
        }
        return t;
    }
}
