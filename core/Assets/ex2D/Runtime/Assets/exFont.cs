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

    // ------------------------------------------------------------------ 
    [SerializeField] private Font dynamicFont_;
    /// The referenced dynamic font asset
    // ------------------------------------------------------------------ 
    public Font dynamicFont {
        get {
            return dynamicFont_;
        }
        private set {
            if (ReferenceEquals(dynamicFont_, value)) {
                return;
            }
            if (dynamicFont_ != null) {
                dynamicFont_.textureRebuildCallback -= textureRebuildCallback_;
            }
            if (value != null) {
                value.textureRebuildCallback += textureRebuildCallback_;
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
            if (bitmapFont_ != null && bitmapFont_.texture == null) {
                Debug.LogWarning("invalid font texture");
            }
        }
    }

#if UNITY_EDITOR
    
    public enum TypeForEditor {
        Bitmap,
        Dynamic,
    }

    [SerializeField] private TypeForEditor type_ = TypeForEditor.Bitmap;
    /// 该属性仅供编辑器使用，用户直接调用exFont.Set方法即可，无需设置类型。
    public TypeForEditor type {
        get {
            return type_;
        }
        set {
            type_ = value;
            if (type_ == TypeForEditor.Bitmap) {
                dynamicFont = null;
            }
            else {
                bitmapFont = null;
            }
        }
    }

#endif
    
    public void Set (exBitmapFont _bitmapFont) {
        bitmapFont = _bitmapFont;
#if UNITY_EDITOR
        type = TypeForEditor.Bitmap;
#endif
    }

    public void Set (Font _dynamicFont) {
        dynamicFont = _dynamicFont;
#if UNITY_EDITOR
        type = TypeForEditor.Dynamic;
#endif
    }

    public void Clear () {
        bitmapFont = null;
        dynamicFont = null;
        textureRebuildCallback_ = null;
    }
    
/*    // ------------------------------------------------------------------ 
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
    }*/

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
        exDebug.Assert(textureRebuildCallback_ != null);
        if (textureRebuildCallback_ != null) {
            textureRebuildCallback_();
        }
    }

    //[System.NonSerialized] public Font.FontTextureRebuildCallback textureRebuildCallback;

    [System.NonSerialized] private Font.FontTextureRebuildCallback textureRebuildCallback_;
    /// 这个事件的add和remove方法必须保证成对调用。
    public event Font.FontTextureRebuildCallback textureRebuildCallback {
        add {
#if UNITY_EDITOR
            exDebug.Assert( textureRebuildCallback_ == null || UnityEditor.ArrayUtility.Contains(textureRebuildCallback_.GetInvocationList(), 
                new Font.FontTextureRebuildCallback(value)) == false );
#endif
            textureRebuildCallback_ = value;    // 直接限制不能多个sprite font共享一个exFont
            if (dynamicFont_ != null) {
                dynamicFont_.textureRebuildCallback += textureRebuildCallback_;
            }
            //Debug.Log("[|exFont] add");
        }
        remove {
#if UNITY_EDITOR
            exDebug.Assert( textureRebuildCallback_ == value );
#endif
            textureRebuildCallback_ = null;    // 直接限制不能多个sprite font共享一个exFont
            if (dynamicFont_ != null) {
                dynamicFont_.textureRebuildCallback -= textureRebuildCallback_;
            }
            //Debug.Log("[|exFont] remove");
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
                    return true;
                }
            }
            return false;
        }
    }

    private float jOffsetY;
    
    ///////////////////////////////////////////////////////////////////////////////
    // Functions
    ///////////////////////////////////////////////////////////////////////////////

    /*public CharInfo GetCharInfo ( char _symbol ) {
        if (bitmapFont_ != null) {
            return bitmapFont_.GetCharInfo(_symbol);
        }
        if (dynamicFont_ != null) {
            //// yes, Unity's GetCharacterInfo have y problem, you should get lowest character j's y-offset adjust it.
            CharacterInfo jCharInfo;
            dynamicFont_.RequestCharactersInTexture("j", dynamicFontSize_, dynamicFontStyle_);
            dynamicFont_.GetCharacterInfo('j', out jCharInfo, dynamicFontSize_, dynamicFontStyle_);
            int ttf_offset = (int)(dynamicFontSize_ + jCharInfo.vert.yMax);

            CharacterInfo dynamicCharInfo;
            dynamicFont_.GetCharacterInfo(_symbol, out dynamicCharInfo, dynamicFontSize_, dynamicFontStyle_);

            Texture texture = dynamicFont_.material.mainTexture;
            CharInfo charInfo = new CharInfo(); // TODO: use static char info
            charInfo.id = _symbol;
            charInfo.trim_x = 0;
            charInfo.trim_y = 0;
            charInfo.x = (int)(dynamicCharInfo.uv.x * texture.width);
            charInfo.y = (int)(dynamicCharInfo.uv.yMax * texture.height);
            charInfo.width = (int)dynamicCharInfo.vert.width;
            charInfo.height = - (int)dynamicCharInfo.vert.height;
            charInfo.xoffset = (int)dynamicCharInfo.vert.x;
            charInfo.yoffset = - (int)dynamicCharInfo.vert.y;
            charInfo.xadvance = (int)dynamicCharInfo.width;
            charInfo.rotated = dynamicCharInfo.flipped;
            return charInfo;
        }
        return null;
    }*/

    public bool GetCharInfo ( char _symbol, out CharacterInfo _charInfo ) {
        if (bitmapFont_ != null) {
            exBitmapFont.CharInfo bitmapCharInfo = bitmapFont_.GetCharInfo(_symbol);
            _charInfo.flipped = bitmapCharInfo.rotated;
            _charInfo.index = bitmapCharInfo.id;
            _charInfo.size = 0;
            _charInfo.style = FontStyle.Normal;
            if (bitmapFont_.texture != null) {
                Vector2 texelSize = bitmapFont_.texture.texelSize;
                if (bitmapCharInfo.rotated) {
                    _charInfo.uv = new Rect ((bitmapCharInfo.x + bitmapCharInfo.rotatedWidth) * texelSize.x,
                                             bitmapCharInfo.y * texelSize.y,
                                             - bitmapCharInfo.rotatedWidth * texelSize.x,
                                             bitmapCharInfo.rotatedHeight * texelSize.y);
                }
                else {
                    _charInfo.uv = new Rect (bitmapCharInfo.x * texelSize.x,
                                             bitmapCharInfo.y * texelSize.y,
                                             bitmapCharInfo.rotatedWidth * texelSize.x,
                                             bitmapCharInfo.rotatedHeight * texelSize.y);
                }
            }
            else {
                _charInfo.uv = new Rect();
            }
            _charInfo.vert = new Rect(bitmapCharInfo.xoffset, - bitmapCharInfo.yoffset, bitmapCharInfo.width, - bitmapCharInfo.height);
            _charInfo.width = bitmapCharInfo.xadvance;
            return true;
        }
        else if (dynamicFont_ != null) {
            dynamicFont_.GetCharacterInfo(_symbol, out _charInfo, dynamicFontSize_, dynamicFontStyle_);
            _charInfo.vert.y -= jOffsetY;
            return true;
        }
        else {
            _charInfo = new CharacterInfo();
            return false;
        }
    }

    public int GetKerning ( char _first, char _second ) {
        if (bitmapFont_ != null) {
            return bitmapFont_.GetKerning(_first, _second);
        }
        return 0;
    }
    
    public void RequestCharactersInTexture ( string _text ) {
        if (dynamicFont_ != null) {
            /// yes, Unity's GetCharacterInfo have y problem, you should get lowest character j's y-offset adjust it.
            dynamicFont_.RequestCharactersInTexture("j", dynamicFontSize_, dynamicFontStyle_);
            CharacterInfo jCharInfo;
            dynamicFont_.GetCharacterInfo('j', out jCharInfo, dynamicFontSize_, dynamicFontStyle_);
            jOffsetY = jCharInfo.vert.yMin;
            //
            dynamicFont_.RequestCharactersInTexture (_text, dynamicFontSize_, dynamicFontStyle_);
        }
    }
}
