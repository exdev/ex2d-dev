// ======================================================================================
// File         : exFont.cs
// Author       : 
// Last Change  : 09/17/2013
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
/// The facade for both exBitmapFont and unity's dynamic font.
///
///////////////////////////////////////////////////////////////////////////////

[System.Serializable] public class exFont {

    ///////////////////////////////////////////////////////////////////////////////
    ///
    /// A structure to descrip the character in the bitmap font 
    ///
    ///////////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public class CharInfo {
        public int id = -1;                ///< the char value
        public int trim_x = -1;            ///< the trim offset x of the raw texture (used in atlas-font drawing in editor)
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

    // ------------------------------------------------------------------ 
    [SerializeField] private Font dynamicFont_;
    /// The referenced dynamic font asset
    // ------------------------------------------------------------------ 
    /*public */Font dynamicFont {
        get {
            return dynamicFont_;
        }
        /*private */set {
            if (dynamicFont_ != null && dynamicFontRegistered) {
                dynamicFontRegistered = false;
                dynamicFont_.textureRebuildCallback -= OnFontTextureRebuilt;
            }
            dynamicFont_ = value;
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] private exBitmapFont bitmapFont_;
    /// The referenced bitmap font asset
    // ------------------------------------------------------------------ 
    /*public */exBitmapFont bitmapFont {
        get {
            return bitmapFont_;
        }
        /*private */set {
            bitmapFont_ = value;
        }
    }

    void OnFontTextureRebuilt () {
        exDebug.Assert(dynamicFontRegistered);
        if (textureRebuildCallback != null) {
            textureRebuildCallback();
        }
    }

    public Texture2D texture {
        get {
            if (bitmapFont_ != null) {
                return bitmapFont_.texture;
            }
            else if (dynamicFont_ != null) {
                return dynamicFont_.material.mainTexture as Texture2D;
            }
            return null;
        }
    }

    public bool isValid {
        get {
            if (texture != null) {
                if (bitmapFont_ != null) {
                    return bitmapFont_.charInfos.Count > 0;
                }
                else if (dynamicFont_ != null) {
                    return dynamicFont_.characterInfo.Length > 0;
                }
            }
            return false;
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] private int dynamicLineHeight;
    /// the space of the line
    // ------------------------------------------------------------------ 
    public int lineHeight {
        get {
            if (bitmapFont_ != null) {
                return bitmapFont_.lineHeight;
            }
            return dynamicLineHeight;
        }
        set {
            if (bitmapFont_ != null) {
                bitmapFont_.lineHeight = value;
            }
            else {
                dynamicLineHeight = value;
            }
        }
    }
    
    public void Set (exBitmapFont _bitmapFont) {
        dynamicFont = null;
        bitmapFont = _bitmapFont;
    }

    public void Set (Font _dynamicFont) {
        bitmapFont = null;
        dynamicFont = _dynamicFont;
    }

    public void Clear () {
        bitmapFont = null;
        dynamicFont = null;
        textureRebuildCallback = null;
    }

    [System.NonSerialized] public Font.FontTextureRebuildCallback textureRebuildCallback;
    private bool dynamicFontRegistered = false;

    /*
    private Font.FontTextureRebuildCallback textureRebuildCallback_;
    public event Font.FontTextureRebuildCallback textureRebuildCallback {
        add {
#if UNITY_EDITOR
            exDebug.Assert( UnityEditor.ArrayUtility.Contains(textureRebuildCallback_.GetInvocationList(), 
                new Font.FontTextureRebuildCallback(value)) == false );
#endif
            textureRebuildCallback_ += value;
            if (dynamicFontRegistered == false && dynamicFont_ != null) {
                dynamicFontRegistered = true;
                dynamicFont_.textureRebuildCallback += textureRebuildCallback_;
            }
        }
        remove {
#if UNITY_EDITOR
            exDebug.Assert( UnityEditor.ArrayUtility.Contains(textureRebuildCallback_.GetInvocationList(), 
                new Font.FontTextureRebuildCallback(value)) == true );
#endif
            textureRebuildCallback_ -= value;
            if (dynamicFontRegistered && textureRebuildCallback_ == null && dynamicFont_ != null) {
                dynamicFontRegistered = false;
                dynamicFont_.textureRebuildCallback -= textureRebuildCallback_;
            }
        }
    }*/

    public CharInfo GetCharInfo ( char _symbol ) {
        if (bitmapFont_ != null) {
            return bitmapFont_.GetCharInfo(_symbol);
        }
        
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
