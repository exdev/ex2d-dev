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
public enum exCSS_wrap {
    Normal,
    NoWrap,
    Pre,
    PreWrap,
    Inherit
}

// horizontal-align
public enum exCSS_horizontal_align {
    Left,
    Center,
    Right,
    Inherit
}

// vertical-align
public enum exCSS_vertical_align {
    Top,
    Middle,
    Bottom,
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

// size push
[System.Serializable]
public class exCSS_size_push { 
    public enum Type {
        Length,
        Percentage,
        Auto,
        Push,
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
    public exCSS_size_push ( Type _type, float _val ) { type = _type; val = _val; }
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
    public Object val; 
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
    public Object val;
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
    public exCSS_size_push width         = new exCSS_size_push( exCSS_size_push.Type.Auto, -1.0f );
    public exCSS_size_push height        = new exCSS_size_push( exCSS_size_push.Type.Auto, -1.0f );
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
    public exCSS_color borderColor    = new exCSS_color( exCSS_color.Type.Color, new Color( 255, 255, 255, 255 ) );
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

    // content
    public exCSS_color contentColor = new exCSS_color( exCSS_color.Type.Color, new Color( 255, 255, 255, 255 ) );
    public exCSS_wrap wrap = exCSS_wrap.Normal;
    public exCSS_horizontal_align horizontalAlign = exCSS_horizontal_align.Inherit;
    public exCSS_vertical_align verticalAlign = exCSS_vertical_align.Inherit;
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

    public exUIStyle InlineContent ( int _width, int _height ) {
        exUIStyle newStyle = new exUIStyle ();

        newStyle.display = exCSS_display.Inline;

        //
        newStyle.width     = new exCSS_size_push( width.type, _width );
        newStyle.height    = new exCSS_size_push( height.type, _height );
        newStyle.minWidth  = new exCSS_min_size( minWidth.type, minWidth.val );
        newStyle.minHeight = new exCSS_min_size( minHeight.type, minHeight.val );
        newStyle.maxWidth  = new exCSS_max_size( maxWidth.type, maxWidth.val );
        newStyle.maxHeight = new exCSS_max_size( maxHeight.type, maxHeight.val );

        // font
        newStyle.font = new exCSS_font( font.type, font.val );
        newStyle.fontSize = new exCSS_size_noauto( fontSize.type, fontSize.val );

        // text
        newStyle.contentColor = new exCSS_color( contentColor.type, contentColor.val );
        newStyle.wrap = wrap;
        newStyle.horizontalAlign = horizontalAlign;
        newStyle.verticalAlign = verticalAlign;
        newStyle.textDecoration = textDecoration;
        newStyle.letterSpacing = new exCSS_size_nopercentage( letterSpacing.type, letterSpacing.val );
        newStyle.wordSpacing = new exCSS_size_nopercentage( wordSpacing.type, wordSpacing.val );
        newStyle.lineHeight = new exCSS_size( lineHeight.type, lineHeight.val );

        return newStyle;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public exUIStyle Clone () {
        exUIStyle newStyle = new exUIStyle ();
        newStyle.width = new exCSS_size_push ( width.type, width.val ); 
        newStyle.height = new exCSS_size_push ( height.type, height.val ); 
        newStyle.minWidth  = new exCSS_min_size( minWidth.type, minWidth.val );
        newStyle.minHeight = new exCSS_min_size( minHeight.type, minHeight.val );
        newStyle.maxWidth  = new exCSS_max_size( maxWidth.type, maxWidth.val );
        newStyle.maxHeight = new exCSS_max_size( maxHeight.type, maxHeight.val );

        // position
        newStyle.display = display;
        newStyle.position = position;
        newStyle.top    = new exCSS_size( top.type, top.val );
        newStyle.right  = new exCSS_size( right.type, right.val );
        newStyle.bottom = new exCSS_size( bottom.type, bottom.val );
        newStyle.left   = new exCSS_size( left.type, left.val );

        // margin
        newStyle.marginTop    = new exCSS_size( marginTop.type, marginTop.val );
        newStyle.marginRight  = new exCSS_size( marginRight.type, marginRight.val );
        newStyle.marginBottom = new exCSS_size( marginBottom.type, marginBottom.val );
        newStyle.marginLeft   = new exCSS_size( marginLeft.type, marginLeft.val );
        newStyle.lockMarginRight   = lockMarginRight;
        newStyle.lockMarginBottom  = lockMarginBottom;
        newStyle.lockMarginLeft    = lockMarginLeft;

        // padding
        newStyle.paddingTop    = new exCSS_size_noauto( paddingTop.type, paddingTop.val );
        newStyle.paddingRight  = new exCSS_size_noauto( paddingRight.type, paddingRight.val );
        newStyle.paddingBottom = new exCSS_size_noauto( paddingBottom.type, paddingBottom.val );
        newStyle.paddingLeft   = new exCSS_size_noauto( paddingLeft.type, paddingLeft.val );
        newStyle.lockPaddingRight   = lockPaddingRight;
        newStyle.lockPaddingBottom  = lockPaddingBottom;
        newStyle.lockPaddingLeft    = lockPaddingLeft;

        // border
        newStyle.borderImage    = new exCSS_image( borderImage.type, borderImage.val );
        newStyle.borderColor    = new exCSS_color( borderColor.type, borderColor.val );
        newStyle.borderSizeTop    = new exCSS_size_lengthonly( borderSizeTop.type, borderSizeTop.val );
        newStyle.borderSizeRight  = new exCSS_size_lengthonly( borderSizeRight.type, borderSizeRight.val );
        newStyle.borderSizeBottom = new exCSS_size_lengthonly( borderSizeBottom.type, borderSizeBottom.val );
        newStyle.borderSizeLeft   = new exCSS_size_lengthonly( borderSizeLeft.type, borderSizeLeft.val );
        newStyle.lockBorderSizeRight   = lockBorderSizeRight;
        newStyle.lockBorderSizeBottom  = lockBorderSizeBottom;
        newStyle.lockBorderSizeLeft    = lockBorderSizeLeft;

        // background
        newStyle.backgroundColor = new exCSS_color( backgroundColor.type, backgroundColor.val );
        newStyle.backgroundImage = new exCSS_image( backgroundImage.type, backgroundImage.val );
        newStyle.backgroundRepeat = backgroundRepeat;

        // font
        newStyle.font = new exCSS_font( font.type, font.val );
        newStyle.fontSize = new exCSS_size_noauto( fontSize.type, fontSize.val );

        // text
        newStyle.contentColor = new exCSS_color( contentColor.type, contentColor.val );
        newStyle.wrap = wrap;
        newStyle.horizontalAlign = horizontalAlign;
        newStyle.verticalAlign = verticalAlign;
        newStyle.textDecoration = textDecoration;
        newStyle.letterSpacing = new exCSS_size_nopercentage( letterSpacing.type, letterSpacing.val );
        newStyle.wordSpacing = new exCSS_size_nopercentage( wordSpacing.type, wordSpacing.val );
        newStyle.lineHeight = new exCSS_size( lineHeight.type, lineHeight.val );

        return newStyle;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Compute ( exUIElement _el, int _x, int _y, int _width, int _height ) {
        float val = 0.0f;

        // ======================================================== 
        // min, max width & height 
        // ======================================================== 

        val = minWidth.val;
        if ( minWidth.type == exCSS_min_size.Type.Percentage ) 
            val = minWidth.val/100.0f * (float)_width;
        _el.minWidth = Mathf.FloorToInt(val); 

        val = minHeight.val;
        if ( minHeight.type == exCSS_min_size.Type.Percentage ) 
            val = minHeight.val/100.0f * (float)_height;
        _el.minHeight = Mathf.FloorToInt(val); 

        val = maxWidth.val;
        if ( maxWidth.type == exCSS_max_size.Type.Percentage ) 
            val = maxWidth.val/100.0f * (float)_width;

        if ( maxWidth.type == exCSS_max_size.Type.None )
            _el.maxWidth = int.MaxValue;
        else 
            _el.maxWidth = Mathf.FloorToInt(val); 

        val = maxHeight.val;
        if ( maxHeight.type == exCSS_max_size.Type.Percentage ) 
            val = maxHeight.val/100.0f * (float)_height;

        if ( maxHeight.type == exCSS_max_size.Type.None )
            _el.maxHeight = int.MaxValue;
        else 
            _el.maxHeight = Mathf.FloorToInt(val); 

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
        if ( contentColor.type == exCSS_color.Type.Inherit ) {
            _el.contentColor = (_el.parent != null) ? _el.parent.contentColor : Color.white;
        }
        else {
            _el.contentColor = contentColor.val;
        }

        // wrap
        if ( wrap == exCSS_wrap.Inherit ) {
            _el.wrap = (_el.parent != null) ? _el.parent.wrap : exCSS_wrap.Normal;
        }
        else {
            _el.wrap = wrap;
        }

        // horizontal-align
        if ( horizontalAlign == exCSS_horizontal_align.Inherit ) {
            _el.horizontalAlign = (_el.parent != null) ? _el.parent.horizontalAlign : exCSS_horizontal_align.Left;
        }
        else {
            _el.horizontalAlign = horizontalAlign;
        }

        // vertical-align
        if ( verticalAlign == exCSS_vertical_align.Inherit ) {
            _el.verticalAlign = (_el.parent != null) ? _el.parent.verticalAlign : exCSS_vertical_align.Top;
        }
        else {
            _el.verticalAlign = verticalAlign;
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
                exBitmapFont bitmapFont = _el.font as exBitmapFont;
                if ( bitmapFont != null ) {
                    val = bitmapFont.lineHeight;
                } 
                else {
                    val = _el.fontSize;
                }
            }
            _el.lineHeight = Mathf.FloorToInt(val);
        }
    }
}


