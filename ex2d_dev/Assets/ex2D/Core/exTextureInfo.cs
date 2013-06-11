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
    // public string rawTextureGUID;
    public Texture2D texture; ///< the atlas or raw texture

    public bool rotated = false; ///< if rotate the texture in atlas 
    public bool trim = false;    ///< if trimmed the texture

    // for texture offset
    public int trim_x = 0;
    public int trim_y = 0;
    public int rawWidth = 1;
    public int rawHeight = 1;

    public int x = 0;
    public int y = 0;
    public int width = 1;
    public int height = 1;

    [System.NonSerialized] public float s0 = 0.0f;
    [System.NonSerialized] public float t0 = 0.0f;
    [System.NonSerialized] public float s1 = 1.0f;
    [System.NonSerialized] public float t1 = 1.0f;
}
