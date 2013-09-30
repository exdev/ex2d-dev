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
using System.Collections;
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
        public int id = -1;                ///< the char value
        public int trim_x = -1; // TODO: UNITY_EDITOR ///< the trim offset x of the raw texture (used in atlas-font drawing in editor)
        public int trim_y = -1;            ///< the trim offset y of the raw texture (used in atlas-font drawing in editor)
        public int x = -1;                 ///< the x pos
        public int y = -1;                 ///< the y pos
        public int width = -1;             ///< the width
        public int height = -1;            ///< the height
        public int xoffset = -1;           ///< the xoffset
        public int yoffset = -1;           ///< the yoffset
        public int xadvance = -1;          ///< the xadvance
        public bool rotated = false;

        public int rotatedWidth {
            get {
                if ( rotated ) return height;
                return width;
            }
        }
        public int rotatedHeight {
            get {
                if ( rotated ) return width;
                return height;
            }
        }

        public CharInfo () {}
        public CharInfo ( CharInfo _c ) {
            id = _c.id;
            x = _c.x;
            y = _c.y;
            width = _c.width;
            height = _c.height;
            xoffset = _c.xoffset;
            yoffset = _c.yoffset;
            xadvance = _c.xadvance;
            rotated = _c.rotated;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    ///
    /// A structure to descrip the kerning between two character in the bitmap font 
    ///
    ///////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class KerningInfo {
        public int first = -1;  ///< the first character 
        public int second = -1; ///< the second character
        public int amount = -1; ///< the amount of kerning
    }

    ///////////////////////////////////////////////////////////////////////////////
    ///
    /// A structure to descrip the pair of char
    ///
    ///////////////////////////////////////////////////////////////////////////////

    public struct KerningTableKey {
        
        // ------------------------------------------------------------------ 
        /// In Xamarin.iOS, if KerningTableKey is a type as dictionary keys, we should manually implement its IEqualityComparer,
        /// and provide an instance to the Dictionary<TKey, TValue>(IEqualityComparer<TKey>) constructor.
        /// See http://docs.xamarin.com/guides/ios/advanced_topics/limitations for more info.
        // ------------------------------------------------------------------ 
        
        public class Comparer : IEqualityComparer<KerningTableKey> {
            static Comparer instance_;
            public static Comparer instance {
                get {
                    if (instance_ == null) {
                        instance_ = new Comparer();
                    }
                    return instance_;
                }
            }
            public bool Equals (KerningTableKey _lhs, KerningTableKey _rhs) {
                return _lhs.first == _rhs.first && _lhs.second == _rhs.second;
            }
            public int GetHashCode(KerningTableKey _obj) {
                return ((int)_obj.first << 16) ^ _obj.second;
            }
        }
        
        public char first;
        public char second;

        public KerningTableKey (char _first, char _second) {
            first = _first;
            second = _second;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // serialized fileds
    ///////////////////////////////////////////////////////////////////////////////

    public string rawFontGUID = "";
    public string rawTextureGUID = "";
    public string rawAtlasGUID = "";
    public Texture2D texture; ///< the atlas or raw texture

    public List<CharInfo> charInfos = new List<CharInfo>(); ///< the list of the character information
    public List<KerningInfo> kernings = new List<KerningInfo>(); ///< the list of the kerning information 

    public int baseLine;   ///< the base-line of the text when draw
    public int lineHeight; ///< the space of the line
    public int size;       ///< the size in pixel of the font 

    ///////////////////////////////////////////////////////////////////////////////
    // internal fileds
    ///////////////////////////////////////////////////////////////////////////////

    protected Dictionary<int, CharInfo> charInfoTable = null;
    protected Dictionary<KerningTableKey,int> kerningTable = null;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    public void Reset () {
        rawFontGUID = "";
        texture = null;

        charInfos.Clear();
        kernings.Clear();

        baseLine = 0;
        lineHeight = 0;
        size = 0;

        charInfoTable = null;
        kerningTable = null;
    }

    // ------------------------------------------------------------------ 
    /// Rebuild the table to store key exBitmapFont.CharInfo.id to value exBitmapFont.CharInfo
    // ------------------------------------------------------------------ 

    public void RebuildCharInfoTable () {
        if ( charInfoTable == null ) {
            charInfoTable = new Dictionary<int, CharInfo>(charInfos.Count);
        }
        charInfoTable.Clear();
        for ( int i = 0; i < charInfos.Count; ++i ) {
            CharInfo c = charInfos[i];
            charInfoTable[c.id] = c;
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _id the look up key 
    /// \return the expect character info
    /// Get the character information by exBitmapFont.CharInfo.id
    // ------------------------------------------------------------------ 

    public CharInfo GetCharInfo ( char _symbol ) {
        // create and build idToCharInfo table if null
        if ( charInfoTable == null || charInfoTable.Count == 0 ) {
            RebuildCharInfoTable ();
        }

        CharInfo charInfo;
        if ( charInfoTable.TryGetValue (_symbol, out charInfo) )
            return charInfo;
        return null;
    }
    
    // ------------------------------------------------------------------ 
    /// Rebuild the kerningTable to store key <first char, second char> to value kerning amount
    // ------------------------------------------------------------------ 

    public void RebuildKerningTable () {
        // 如果大部分字符的kerning数量都在10个以下，可以直接线性存到CharInfo里。
        if ( kerningTable == null ) {
            kerningTable = new Dictionary<KerningTableKey,int> (kernings.Count, KerningTableKey.Comparer.instance);
        }
        kerningTable.Clear();
        for ( int i = 0; i < kernings.Count; ++i ) {
            KerningInfo k = kernings [i];
            kerningTable[new KerningTableKey((char)k.first, (char)k.second)] = k.amount;
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _first the first character
    /// \param _second the second character
    /// \return the kerning amount
    /// Get the kerning amount between first and sceond character
    // ------------------------------------------------------------------ 

    public int GetKerning ( char _first, char _second ) {
        if ( kerningTable == null ) {
            RebuildKerningTable ();
        }

        int amount;
        if ( kerningTable.TryGetValue (new KerningTableKey (_first, _second), out amount) ) {
            return amount;
        }
        return 0;
    }
}
