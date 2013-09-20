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

    // ------------------------------------------------------------------ 
    [SerializeField] private Font dynamicFont_;
    /// The referenced dynamic font asset
    // ------------------------------------------------------------------ 
    public Font dynamicFont {
        get {
            return dynamicFont_;
        }
        private set {
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
    public exBitmapFont bitmapFont {
        get {
            return bitmapFont_;
        }
        private set {
            bitmapFont_ = value;
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

    // ------------------------------------------------------------------ 
    [SerializeField] private int dynamicFontSize_;
    /// the space of the line
    // ------------------------------------------------------------------ 
    public int fontSize {
        get {
            if (bitmapFont_ != null) {
                return bitmapFont_.size;
            }
            return dynamicFontSize_;
        }
        set {
            if (bitmapFont_ != null) {
                bitmapFont_.size = value;
            }
            else {
                dynamicFontSize_ = value;
            }
        }
    }

    [SerializeField] private FontStyle dynamicFontStyle_ = FontStyle.Normal;
    public FontStyle fontStyle {
        get {
            return dynamicFontStyle_;
        }
        set {
            dynamicFontStyle_ = value;
        }
    }

    void OnFontTextureRebuilt () {
        exDebug.Assert(dynamicFontRegistered);
        if (textureRebuildCallback != null) {
            textureRebuildCallback();
        }
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

    public CharInfo GetCharInfo ( char _symbol ) {
        if (bitmapFont_ != null) {
            return bitmapFont_.GetCharInfo(_symbol);
        }
        if (dynamicFont_ != null) {
            //// yes, Unity's GetCharacterInfo have y problem, you should get lowest character j's y-offset adjust it.
            //CharacterInfo jCharInfo;
            //dynamicFont_.RequestCharactersInTexture("j", dynamicFontSize_, dynamicFontStyle_);
            //dynamicFont_.GetCharacterInfo('j', out jCharInfo, dynamicFontSize_, dynamicFontStyle_);
            //float ttf_offset = (dynamicFontSize_ + jCharInfo.vert.yMax);

            CharacterInfo dynamicCharInfo;
            dynamicFont_.GetCharacterInfo(_symbol, out dynamicCharInfo, dynamicFontSize_, dynamicFontStyle_);

            Texture texture = dynamicFont_.material.mainTexture;
            CharInfo charInfo = new CharInfo(); // TODO: use static char info
            charInfo.id = _symbol;
            charInfo.trim_x = 0;
            charInfo.trim_y = 0;
            charInfo.x = (int)dynamicCharInfo.uv.x * texture.width;  // TODO: save uv in char info
            charInfo.y = (int)dynamicCharInfo.uv.y * texture.height;
            charInfo.width = (int)dynamicCharInfo.vert.width;
            charInfo.height = (int)dynamicCharInfo.vert.height;
            charInfo.xoffset = (int)dynamicCharInfo.vert.x;
            charInfo.yoffset = (int)dynamicCharInfo.vert.y;
            charInfo.xadvance = (int)dynamicCharInfo.width;
            charInfo.rotated = dynamicCharInfo.flipped;
            return charInfo;
        }
        return null;
    }

    public int GetKerning ( char _first, char _second ) {
        if (bitmapFont_ != null) {
            return bitmapFont_.GetKerning(_first, _second);
        }
        return 0;
    }
}
