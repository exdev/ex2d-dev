// ======================================================================================
// File         : exTextureUtility.cs
// Author       : Wu Jie 
// Last Change  : 06/20/2013 | 17:35:25 PM | Thursday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;

///////////////////////////////////////////////////////////////////////////////
/// 
/// the texture helper class
/// 
///////////////////////////////////////////////////////////////////////////////

public static class exTextureUtility {

    // ------------------------------------------------------------------ 
    /// \param _tex the texture to trim
    /// \return the trimmed rect
    /// get the trimmed texture rect 
    // ------------------------------------------------------------------ 

    public static Rect GetTrimTextureRect ( Texture2D _tex ) {
        Rect rect = new Rect( 0, 0, 0, 0 );
        Color32[] pixels = _tex.GetPixels32(0);

        for ( int x = 0; x < _tex.width; ++x ) {
            for ( int y = 0; y < _tex.height; ++y ) {
                if ( pixels[x+y*_tex.width].a != 0 ) {
                    rect.x = x;
                    x = _tex.width;
                    break;
                }
            }
        }

        for ( int x = _tex.width-1; x >= 0; --x ) {
            for ( int y = 0; y < _tex.height; ++y ) {
                if ( pixels[x+y*_tex.width].a != 0 ) {
                    rect.xMax = x+1;
                    x = 0;
                    break;
                }
            }
        }

        for ( int y = 0; y < _tex.height; ++y ) {
            for ( int x = 0; x < _tex.width; ++x ) {
                if ( pixels[x+y*_tex.width].a != 0 ) {
                    rect.y = y;
                    y = _tex.height;
                    break;
                }
            }
        }

        for ( int y = _tex.height-1; y >= 0; --y ) {
            for ( int x = 0; x < _tex.width; ++x ) {
                if ( pixels[x+y*_tex.width].a != 0 ) {
                    rect.yMax = y+1;
                    y = 0;
                    break;
                }
            }
        }

        return rect;
    }
}
