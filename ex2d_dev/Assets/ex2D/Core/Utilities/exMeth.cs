// // ======================================================================================
// // File         : exMesh.cs
// // Author       : Jare
// // Last Change  : 21 / 07 / 2013
// // Description  : 
// // ======================================================================================

using UnityEngine;
using System.Collections;

public static class exMeth {
	
	// ------------------------------------------------------------------ 
	/// \param _length the length used for wrapping
    /// \param _seconds the in seconds used for wrapping
    /// \param _wrapMode the wrap mode used for wrapping
    /// wrap the seconds of the anim clip by the wrap mode
    // ------------------------------------------------------------------ 

    public static float WrapSeconds ( float _length, float _seconds, WrapMode _wrapMode ) {
        float t = Mathf.Abs(_seconds);
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
