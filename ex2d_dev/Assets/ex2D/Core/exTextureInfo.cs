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
    public int trim_x = 0; ///< the trim offset x of the raw texture in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel
    public int trim_y = 0; ///< the trim offset y of the raw texture in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel
    public int rawWidth = 1;
    public int rawHeight = 1;

    public int x = 0; ///< the x in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel 
    public int y = 0; ///< the y in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel
    public int width = 1;
    public int height = 1;
}
