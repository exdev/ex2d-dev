// ======================================================================================
// File         : exISpriteFont.cs
// Author       : Jare
// Last Change  : 12/29/2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
//
// Interface for exSpriteFont and ex3DSpriteFont
//
///////////////////////////////////////////////////////////////////////////////

public interface exISpriteFont : exISpriteBase {
    string text { get; set; }
    int fontSize { get; set; }
    FontStyle fontStyle { get; set; }
    exFont font { get; }
    exBitmapFont bitmapFont { get; }
    Font dynamicFont { get; }
    exTextUtility.WrapMode wrapMode { get; set; }
    int lineHeight { get; set; }
    bool customLineHeight { get; set; }
    int letterSpacing { get; set; }
    int wordSpacing { get; set; }
    TextAlignment textAlign { get; set; }
    bool useKerning { get; set; }
    Color topColor { get; set; }
    Color botColor { get; set; }
#if UNITY_EDITOR
    exFont.TypeForEditor fontType { get; set; }
#endif

    void SetFont (exBitmapFont _bitmapFont);
    void SetFont (Font _dynamicFont);
}
