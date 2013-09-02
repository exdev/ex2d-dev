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

// css type
public enum exCSS_type {
    Local,      // css: none
    Inherit,    // css: inherit        
    Auto,       // css: auto
    Percentage, // css: %
    Pixel,      // css: px
}

// css position
public enum exCSS_position {
    Static,
    Relative,
    Absolute,
    Fixed,
    Inherit
}

// css int
[System.Serializable]
public class exCSS_int { 
    public exCSS_type type; 
    public int val; 
    public exCSS_int ( exCSS_type _type, int _val ) { type = _type; val = _val; }
}

// css float
[System.Serializable]
public class exCSS_float { 
    public exCSS_type type; 
    public float val; 
    public exCSS_float ( exCSS_type _type, float _val ) { type = _type; val = _val; }
}

// css color
[System.Serializable]
public class exCSS_color { 
    public exCSS_type type; 
    public Color val; 
    public exCSS_color ( exCSS_type _type, Color _val ) { type = _type; val = _val; }
}

// css image
[System.Serializable]
public class exCSS_image { 
    public exCSS_type type; 
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
    public exCSS_image ( exCSS_type _type, Object _val ) { 
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
    // margin
    public exCSS_int marginTop    = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int marginRight  = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int marginBottom = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int marginLeft   = new exCSS_int( exCSS_type.Auto, -1 );
    public bool lockMarginRight   = true;
    public bool lockMarginBottom  = true;
    public bool lockMarginLeft    = true;

    // padding
    public exCSS_int paddingTop    = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int paddingRight  = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int paddingBottom = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int paddingLeft   = new exCSS_int( exCSS_type.Auto, -1 );
    public bool lockPaddingRight   = true;
    public bool lockPaddingBottom  = true;
    public bool lockPaddingLeft    = true;

    // border
    public exCSS_image borderSrc      = new exCSS_image( exCSS_type.Inherit, null );
    public exCSS_color borderColor    = new exCSS_color( exCSS_type.Inherit, new Color( 0, 0, 0, 255 ) );
    public exCSS_int borderSizeTop    = new exCSS_int( exCSS_type.Inherit, 0 );
    public exCSS_int borderSizeRight  = new exCSS_int( exCSS_type.Inherit, 0 );
    public exCSS_int borderSizeBottom = new exCSS_int( exCSS_type.Inherit, 0 );
    public exCSS_int borderSizeLeft   = new exCSS_int( exCSS_type.Inherit, 0 );
    public bool lockBorderSizeRight   = true;
    public bool lockBorderSizeBottom  = true;
    public bool lockBorderSizeLeft    = true;

    // size
    public exCSS_int width     = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int height    = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int minWidth  = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int minHeight = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int maxWidth  = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int maxHeight = new exCSS_int( exCSS_type.Auto, -1 );

    // position
    public exCSS_position position = exCSS_position.Static;
    public exCSS_int top    = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int right  = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int bottom = new exCSS_int( exCSS_type.Auto, -1 );
    public exCSS_int left   = new exCSS_int( exCSS_type.Auto, -1 );

    // background
    public exCSS_color backgroundColor = new exCSS_color( exCSS_type.Inherit, new Color( 0, 0, 0, 255 )  );
}


