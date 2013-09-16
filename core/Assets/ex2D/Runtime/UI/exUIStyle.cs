// ======================================================================================
// File         : exUIStyle.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 23:01:11 PM | Friday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

// display
public enum exCSS_display {
    Block,
    InlineBlock,
    Inline,
}

// background-repeat
public enum exCSS_background_repeat {
    Repeat,
    RepeatX,
    RepeatY,
    NoRepeat,
}

// position
public enum exCSS_position {
    Static,
    Relative,
    Absolute,
    Fixed,
}

// white-space
public enum exCSS_white_space {
    Normal,
    Pre,
    NoWrap,
    PreWrap,
    Inherit
}

// text-alignment
public enum exCSS_alignment {
    Left,
    Center,
    Right,
    Inherit
}

// text-decoration
public enum exCSS_decoration {
    None,
    Underline,
    Overline,
    LineThrough
}

// size
[System.Serializable]
public class exCSS_size { 
    public enum Type {
        Length,
        Percentage,
        Auto,
        Inherit
    }
    public Type type; 
    [SerializeField] float val_; 
    public float val {
        set {
            if ( type == Type.Length )
                val_ = Mathf.FloorToInt(value);
            else
                val_ = value;
        }
        get {
            return val_;
        }
    }
    public exCSS_size ( Type _type, float _val ) { type = _type; val = _val; }
}

// size no-percentage
[System.Serializable]
public class exCSS_size_nopercentage { 
    public enum Type {
        Auto,
        Length,
        Inherit
    }
    public Type type; 
    [SerializeField] float val_; 
    public float val {
        set {
            if ( type == Type.Length )
                val_ = Mathf.FloorToInt(value);
            else
                val_ = value;
        }
        get {
            return val_;
        }
    }
    public exCSS_size_nopercentage ( Type _type, float _val ) { type = _type; val = _val; }
}

// size no-auto
[System.Serializable]
public class exCSS_size_noauto { 
    public enum Type {
        Length,
        Percentage,
        Inherit
    }
    public Type type; 
    [SerializeField] float val_; 
    public float val {
        set {
            if ( type == Type.Length )
                val_ = Mathf.FloorToInt(value);
            else
                val_ = value;
        }
        get {
            return val_;
        }
    }
    public exCSS_size_noauto ( Type _type, float _val ) { type = _type; val = _val; }
}

// size length-only
[System.Serializable]
public class exCSS_size_lengthonly { 
    public enum Type {
        Length,
        Inherit
    }
    public Type type; 
    [SerializeField] float val_; 
    public float val {
        set {
            if ( type == Type.Length )
                val_ = Mathf.FloorToInt(value);
            else
                val_ = value;
        }
        get {
            return val_;
        }
    }
    public exCSS_size_lengthonly ( Type _type, float _val ) { type = _type; val = _val; }
}

// min-size
[System.Serializable]
public class exCSS_min_size { 
    public enum Type {
        Length,
        Percentage,
        Inherit
    }
    public Type type; 
    [SerializeField] float val_; 
    public float val {
        set {
            if ( type == Type.Length )
                val_ = Mathf.FloorToInt(value);
            else
                val_ = value;
        }
        get {
            return val_;
        }
    }
    public exCSS_min_size ( Type _type, float _val ) { type = _type; val = _val; }
}

// max-size
[System.Serializable]
public class exCSS_max_size { 
    public enum Type {
        Length,
        Percentage,
        None,
        Inherit
    }
    public Type type; 
    [SerializeField] float val_; 
    public float val {
        set {
            if ( type == Type.Length )
                val_ = Mathf.FloorToInt(value);
            else
                val_ = value;
        }
        get {
            return val_;
        }
    }
    public exCSS_max_size ( Type _type, float _val ) { type = _type; val = _val; }
}

// css color
[System.Serializable]
public class exCSS_color { 
    public enum Type {
        Color,
        Inherit
    }
    public Type type; 
    public Color val; 
    public exCSS_color ( Type _type, Color _val ) { type = _type; val = _val; }
}

// css image
[System.Serializable]
public class exCSS_image { 
    public enum Type {
        TextureInfo,
        Texture2D,
        Inherit
    }
    public Type type;
    public exTextureInfo src1; 
    public Texture2D src2; 
    public Object val {
        set {
            if ( value is Texture2D ) {
                src1 = null; 
                src2 = value as Texture2D; 
            }
            else if ( value is exTextureInfo ) {
                src1 = value as exTextureInfo; 
                src2 = null;
            }
            else {
                src1 = null;
                src2 = null;
            }
        }
        get { 
            if ( src1 != null )
                return src1;
            else if ( src2 != null )
                return src2;
            return null;
        }
    }
    public exCSS_image ( Type _type, Object _val ) { 
        type = _type; 
        val = _val;
    }
}

// css font
[System.Serializable]
public class exCSS_font { 
    public enum Type {
        TTF,
        BitmapFont,
        Inherit
    }
    public Type type;
    public Font src1; 
    public exBitmapFont src2; 
    public Object val {
        set {
            if ( value is Font ) {
                src1 = value as Font; 
                src2 = null; 
            }
            else if ( value is exBitmapFont ) {
                src1 = null;
                src2 = value as exBitmapFont; 
            }
            else {
                src1 = null;
                src2 = null;
            }
        }
        get { 
            if ( src1 != null )
                return src1;
            else if ( src2 != null )
                return src2;
            return null;
        }
    }
    public exCSS_font ( Type _type, Object _val ) { 
        type = _type; 
        val = _val;
    }
}

///////////////////////////////////////////////////////////////////////////////
///
/// The ui style
///
///////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public class exUIStyle {

    // size
    public exCSS_size width         = new exCSS_size( exCSS_size.Type.Auto, -1.0f );
    public exCSS_size height        = new exCSS_size( exCSS_size.Type.Auto, -1.0f );
    public exCSS_min_size minWidth  = new exCSS_min_size( exCSS_min_size.Type.Length, 0.0f );
    public exCSS_min_size minHeight = new exCSS_min_size( exCSS_min_size.Type.Length, 0.0f );
    public exCSS_max_size maxWidth  = new exCSS_max_size( exCSS_max_size.Type.None, -1.0f );
    public exCSS_max_size maxHeight = new exCSS_max_size( exCSS_max_size.Type.None, -1.0f );

    // position
    public exCSS_display display = exCSS_display.Block;
    public exCSS_position position = exCSS_position.Static;
    public exCSS_size top    = new exCSS_size( exCSS_size.Type.Auto, -1.0f );
    public exCSS_size right  = new exCSS_size( exCSS_size.Type.Auto, -1.0f );
    public exCSS_size bottom = new exCSS_size( exCSS_size.Type.Auto, -1.0f );
    public exCSS_size left   = new exCSS_size( exCSS_size.Type.Auto, -1.0f );

    // margin
    public exCSS_size marginTop    = new exCSS_size( exCSS_size.Type.Length, 0.0f );
    public exCSS_size marginRight  = new exCSS_size( exCSS_size.Type.Length, 0.0f );
    public exCSS_size marginBottom = new exCSS_size( exCSS_size.Type.Length, 0.0f );
    public exCSS_size marginLeft   = new exCSS_size( exCSS_size.Type.Length, 0.0f );
    public bool lockMarginRight   = true;
    public bool lockMarginBottom  = true;
    public bool lockMarginLeft    = true;

    // padding
    public exCSS_size_noauto paddingTop    = new exCSS_size_noauto( exCSS_size_noauto.Type.Length, 0.0f );
    public exCSS_size_noauto paddingRight  = new exCSS_size_noauto( exCSS_size_noauto.Type.Length, 0.0f );
    public exCSS_size_noauto paddingBottom = new exCSS_size_noauto( exCSS_size_noauto.Type.Length, 0.0f );
    public exCSS_size_noauto paddingLeft   = new exCSS_size_noauto( exCSS_size_noauto.Type.Length, 0.0f );
    public bool lockPaddingRight   = true;
    public bool lockPaddingBottom  = true;
    public bool lockPaddingLeft    = true;

    // border
    public exCSS_image borderImage    = new exCSS_image( exCSS_image.Type.TextureInfo, null );
    public exCSS_color borderColor    = new exCSS_color( exCSS_color.Type.Color, new Color( 0, 0, 0, 255 ) );
    public exCSS_size_lengthonly borderSizeTop    = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public exCSS_size_lengthonly borderSizeRight  = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public exCSS_size_lengthonly borderSizeBottom = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public exCSS_size_lengthonly borderSizeLeft   = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public bool lockBorderSizeRight   = true;
    public bool lockBorderSizeBottom  = true;
    public bool lockBorderSizeLeft    = true;

    // background
    public exCSS_color backgroundColor = new exCSS_color( exCSS_color.Type.Color, new Color( 0, 0, 0, 0 ) );
    public exCSS_image backgroundImage = new exCSS_image( exCSS_image.Type.TextureInfo, null );
    public exCSS_background_repeat backgroundRepeat = exCSS_background_repeat.NoRepeat;

    // font
    public exCSS_font font = new exCSS_font( exCSS_font.Type.Inherit, null );
    public exCSS_size_noauto fontSize = new exCSS_size_noauto( exCSS_size_noauto.Type.Inherit, 16.0f );

    // text
    public exCSS_color textColor = new exCSS_color( exCSS_color.Type.Color, new Color( 0, 0, 0, 255 ) );
    public exCSS_white_space whitespace = exCSS_white_space.Normal;
    public exCSS_alignment textAlign = exCSS_alignment.Left;
    public exCSS_decoration textDecoration = exCSS_decoration.None;
    public exCSS_size_nopercentage letterSpacing = new exCSS_size_nopercentage( exCSS_size_nopercentage.Type.Auto, 0.0f );
    public exCSS_size_nopercentage wordSpacing = new exCSS_size_nopercentage( exCSS_size_nopercentage.Type.Auto, 0.0f );
    public exCSS_size lineHeight = new exCSS_size( exCSS_size.Type.Auto, 0.0f );

    // clipping
    // text-overflow: ellipsis;
    // overflow-x: visible;
    // overflow-y: visible;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Compute ( exUIElement _el, int _x, int _y, int _width, int _height ) {
        float val = 0.0f;

        // ======================================================== 
        // margin 
        // ======================================================== 

        // margin-left
        val = marginLeft.val;
        if ( marginLeft.type == exCSS_size.Type.Percentage ) 
            val = marginLeft.val/100.0f * (float)_width;
        else if ( marginLeft.type == exCSS_size.Type.Auto )
            val = 0.0f;
        _el.marginLeft = Mathf.FloorToInt(val); 

        // margin-right
        val = marginRight.val;
        if ( marginRight.type == exCSS_size.Type.Percentage ) 
            val = marginRight.val/100.0f * (float)_width;
        else if ( marginRight.type == exCSS_size.Type.Auto )
            val = 0.0f;
        _el.marginRight = Mathf.FloorToInt(val); 

        // margin-top
        val = marginTop.val;
        if ( marginTop.type == exCSS_size.Type.Percentage ) 
            val = marginTop.val/100.0f * (float)_height;
        else if ( marginTop.type == exCSS_size.Type.Auto )
            val = 0.0f;
        _el.marginTop = Mathf.FloorToInt(val); 

        // margin-bottom
        val = marginBottom.val;
        if ( marginBottom.type == exCSS_size.Type.Percentage ) 
            val = marginBottom.val/100.0f * (float)_height;
        else if ( marginBottom.type == exCSS_size.Type.Auto )
            val = 0.0f;
        _el.marginBottom = Mathf.FloorToInt(val); 

        // ======================================================== 
        // border
        // ======================================================== 

        // border-color
        _el.borderColor = borderColor.val;

        // border-image
        _el.borderImage = borderImage.val;

        // border-size-left
        _el.borderSizeLeft = Mathf.FloorToInt(borderSizeLeft.val);

        // border-size-right
        _el.borderSizeRight = Mathf.FloorToInt(borderSizeRight.val);

        // border-size-top
        _el.borderSizeTop = Mathf.FloorToInt(borderSizeTop.val);

        // border-size-bottom
        _el.borderSizeBottom = Mathf.FloorToInt(borderSizeBottom.val);

        // ======================================================== 
        // padding 
        // ======================================================== 

        // padding-left
        val = paddingLeft.val;
        if ( paddingLeft.type == exCSS_size_noauto.Type.Percentage ) 
            val = paddingLeft.val/100.0f * (float)_width;
        _el.paddingLeft = Mathf.FloorToInt(val); 

        // padding-right 
        val = paddingRight.val;
        if ( paddingRight.type == exCSS_size_noauto.Type.Percentage ) 
            val = paddingRight.val/100.0f * (float)_width;
        _el.paddingRight = Mathf.FloorToInt(val); 

        // padding-top
        val = paddingTop.val;
        if ( paddingTop.type == exCSS_size_noauto.Type.Percentage ) 
            val = paddingTop.val/100.0f * (float)_height;
        _el.paddingTop = Mathf.FloorToInt(val); 

        // padding-bottom
        val = paddingBottom.val;
        if ( paddingBottom.type == exCSS_size_noauto.Type.Percentage ) 
            val = paddingBottom.val/100.0f * (float)_height;
        _el.paddingBottom = Mathf.FloorToInt(val); 

        // ======================================================== 
        // background
        // ======================================================== 

        // background-color
        _el.backgroundColor = backgroundColor.val;

        // background-image
        _el.backgroundImage = backgroundImage.val;

        // ======================================================== 
        // calculate inherit elements 
        // ======================================================== 

        // font
        if ( font.type == exCSS_font.Type.Inherit )
            _el.font = (_el.parent != null) ? _el.parent.font : null;
        else 
            _el.font = font.val;

        // font-size
        if ( fontSize.type == exCSS_size_noauto.Type.Inherit ) {
            _el.fontSize = (_el.parent != null) ? _el.parent.fontSize : 16;
        }
        else {
            val = fontSize.val;
            if ( fontSize.type == exCSS_size_noauto.Type.Percentage ) {
                float parent_val = (_el.parent != null) ? (float)_el.parent.fontSize : 16;
                val = fontSize.val/100.0f * parent_val;
            }
            _el.fontSize = Mathf.FloorToInt(val);
        }

        // text-color
        if ( textColor.type == exCSS_color.Type.Inherit ) {
            _el.textColor = (_el.parent != null) ? _el.parent.textColor : Color.white;
        }
        else {
            _el.textColor = textColor.val;
        }

        // white-space
        if ( whitespace == exCSS_white_space.Inherit ) {
            _el.whitespace = (_el.parent != null) ? _el.parent.whitespace : exCSS_white_space.Normal;
        }
        else {
            _el.whitespace = whitespace;
        }

        // letter-spacing
        if ( letterSpacing.type == exCSS_size_nopercentage.Type.Inherit ) {
            _el.letterSpacing = (_el.parent != null) ? _el.parent.letterSpacing : 0;
        }
        else {
            _el.letterSpacing = Mathf.FloorToInt(letterSpacing.val);
        }

        // word-spacing
        if ( wordSpacing.type == exCSS_size_nopercentage.Type.Inherit ) {
            _el.wordSpacing = (_el.parent != null) ? _el.parent.wordSpacing : 0;
        }
        else {
            _el.wordSpacing = Mathf.FloorToInt(wordSpacing.val);
        }

        // line-height
        if ( lineHeight.type == exCSS_size.Type.Inherit ) {
            _el.lineHeight = (_el.parent != null) ? _el.parent.lineHeight : 0;
        }
        else {
            val = lineHeight.val;
            if ( lineHeight.type == exCSS_size.Type.Percentage ) {
                float parent_val = (_el.parent != null) ? (float)_el.parent.lineHeight : 0;
                val = lineHeight.val/100.0f * parent_val;
            }
            else if ( lineHeight.type == exCSS_size.Type.Auto ) {
                val = _el.fontSize;
            }
            _el.lineHeight = Mathf.FloorToInt(val);
        }
    }
}


