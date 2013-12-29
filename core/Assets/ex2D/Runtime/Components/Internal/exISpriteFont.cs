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
    exFont font { get; }
    exTextUtility.WrapMode wrapMode { get; set; }
    int lineHeight { get; set; }
    int letterSpacing { get; set; }
    int wordSpacing { get; set; }
    TextAlignment textAlign { get; set; }
    bool useKerning { get; set; }
    Color topColor { get; set; }
    Color botColor { get; set; }
}
