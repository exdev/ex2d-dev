// ======================================================================================
// File         : exBitmapFont.cs
// Author       : Wu Jie 
// Last Change  : 07/26/2013 | 17:18:41 PM | Friday,July
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// The texture-info asset
///
///////////////////////////////////////////////////////////////////////////////

public class exBitmapFont : ScriptableObject {

    ///////////////////////////////////////////////////////////////////////////////
    ///
    /// A structure to descrip the character in the bitmap font 
    ///
    ///////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class CharInfo {
        public exTextureInfo textureInfo = null;
        public int id = -1;                ///< the character id 
        public int xoffset {               ///< the xoffset
            get {
                return textureInfo.trim_x;
            }
        }
        public int yoffset {               ///< the yoffset
            get {
                return textureInfo.trim_y;
            }
        }
        public int xadvance {              ///< the xadvance
            get {
                return textureInfo.rawWidth - textureInfo.trim_x - textureInfo.width;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    ///
    /// A structure to descrip the kerning between two character in the bitmap font 
    ///
    ///////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class KerningInfo {
        public int first = -1;  ///< the id of first character 
        public int second = -1; ///< the id of second character
        public int amount = -1; ///< the amount of kerning
    }

    ///////////////////////////////////////////////////////////////////////////////
    // serialized fileds
    ///////////////////////////////////////////////////////////////////////////////

    public List<CharInfo> charInfos = new List<CharInfo>(); ///< the list of the character information
    public List<KerningInfo> kernings = new List<KerningInfo>(); ///< the list of the kerning information 

    public int lineHeight; ///< the space of the line
    public int size;       ///< the size in pixel of the font 

    ///////////////////////////////////////////////////////////////////////////////
    // internal fileds
    ///////////////////////////////////////////////////////////////////////////////

    protected Dictionary<int,CharInfo> idToCharInfo = null;
}
