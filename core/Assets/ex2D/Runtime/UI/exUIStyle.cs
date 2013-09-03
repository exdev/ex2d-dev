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

// position
public enum exCSS_position {
    Static,
    Relative,
    Absolute,
    Fixed,
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
    float val_; 
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

// size no-auto
[System.Serializable]
public class exCSS_size_noauto { 
    public enum Type {
        Length,
        Percentage,
        Inherit
    }
    public Type type; 
    float val_; 
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
    float val_; 
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
    float val_; 
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
    float val_; 
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
    public exCSS_image borderSrc      = new exCSS_image( exCSS_image.Type.TextureInfo, null );
    public exCSS_color borderColor    = new exCSS_color( exCSS_color.Type.Color, new Color( 0, 0, 0, 255 ) );
    public exCSS_size_lengthonly borderSizeTop    = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public exCSS_size_lengthonly borderSizeRight  = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public exCSS_size_lengthonly borderSizeBottom = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public exCSS_size_lengthonly borderSizeLeft   = new exCSS_size_lengthonly( exCSS_size_lengthonly.Type.Length, 0.0f );
    public bool lockBorderSizeRight   = true;
    public bool lockBorderSizeBottom  = true;
    public bool lockBorderSizeLeft    = true;

    // background
    public exCSS_color backgroundColor = new exCSS_color( exCSS_color.Type.Color, new Color( 0, 0, 0, 255 ) );

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetMarginLeft ( int _contentWidth ) {
        float val = marginLeft.val;
        if ( marginLeft.type == exCSS_size.Type.Percentage ) 
            val = marginLeft.val/100.0f * (float)_contentWidth;
        return Mathf.FloorToInt(val); 
    } 
    public int GetMarginRight ( int _contentWidth ) {
        float val = marginRight.val;
        if ( marginRight.type == exCSS_size.Type.Percentage ) 
            val = marginRight.val/100.0f * (float)_contentWidth;
        return Mathf.FloorToInt(val); 
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetPaddingLeft ( int _contentWidth ) {
        float val = paddingLeft.val;
        if ( paddingLeft.type == exCSS_size_noauto.Type.Percentage ) 
            val = paddingLeft.val/100.0f * (float)_contentWidth;
        return Mathf.FloorToInt(val); 
    } 
    public int GetPaddingRight ( int _contentWidth ) {
        float val = paddingRight.val;
        if ( paddingRight.type == exCSS_size_noauto.Type.Percentage ) 
            val = paddingRight.val/100.0f * (float)_contentWidth;
        return Mathf.FloorToInt(val); 
    } 
}


