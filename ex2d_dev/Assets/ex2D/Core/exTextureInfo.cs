// ======================================================================================
// File         : exTextureInfo.cs
// Author       : Wu Jie 
// Last Change  : 02/17/2013 | 21:39:05 PM | Sunday,February
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;

///////////////////////////////////////////////////////////////////////////////
///
/// The texture-info asset
///
///////////////////////////////////////////////////////////////////////////////

public class exTextureInfo : ScriptableObject {
    // public string rawTextureGUID; // TODO: do we need this??
    public Texture2D texture; ///< the atlas or raw texture

    public int rawWidth = 1;
    public int rawHeight = 1;

    public bool rotated = false; ///< if rotate the texture in atlas 
    public bool trim = false;    ///< if trimmed the texture

    [System.NonSerialized] public float s0 = 0.0f;
    [System.NonSerialized] public float t0 = 0.0f;
    [System.NonSerialized] public float s1 = 1.0f;
    [System.NonSerialized] public float t1 = 1.0f;
}
