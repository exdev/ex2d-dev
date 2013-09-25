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

    public static Rect GetTrimTextureRect ( Texture2D _tex, int _trimThreshold, Rect _rect ) {
        Rect rect = new Rect( 0, 0, 0, 0 );
        Color32[] pixels = _tex.GetPixels32(0);

        int xmin = _tex.width;
        int xmax = 0;
        int ymin = _tex.height;
        int ymax = 0;

        int x_start = (int)_rect.x;
        int x_end = (int)(_rect.x + _rect.width);
        int y_start = (int)_rect.y;
        int y_end = (int)(_rect.y + _rect.height);

        for ( int x = x_start; x < x_end; ++x ) {
            for ( int y = y_start; y < y_end; ++y ) {
                if ( pixels[x+y*_tex.width].a >= _trimThreshold ) {
                    xmin = x;
                    x = _tex.width;
                    break;
                }
            }
        }

        for ( int x = x_end-1; x >= x_start; --x ) {
            for ( int y = y_start; y < y_end; ++y ) {
                if ( pixels[x+y*_tex.width].a >= _trimThreshold ) {
                    xmax = x;
                    x = -1;
                    break;
                }
            }
        }

        for ( int y = y_start; y < y_end; ++y ) {
            for ( int x = x_start; x < x_end; ++x ) {
                if ( pixels[x+y*_tex.width].a >= _trimThreshold ) {
                    ymin = y;
                    y = _tex.height;
                    break;
                }
            }
        }

        for ( int y = y_end-1; y >= y_start; --y ) {
            for ( int x = x_start; x < x_end; ++x ) {
                if ( pixels[x+y*_tex.width].a >= _trimThreshold ) {
                    ymax = y;
                    y = -1;
                    break;
                }
            }
        }

        // DISABLE { 
        // for ( int y = 0, yw = _tex.height; y < yw; ++y ) {
        //     for ( int x = 0, xw = _tex.width; x < xw; ++x ) {
        //         Color32 c = pixels[y * xw + x];

        //         if ( c.a >= _trimThreshold ) {
        //             if (y < ymin) ymin = y;
        //             if (y > ymax) ymax = y;
        //             if (x < xmin) xmin = x;
        //             if (x > xmax) xmax = x;
        //         }
        //     }
        // }
        // } DISABLE end 

        int newWidth  = (xmax - xmin) + 1;
        int newHeight = (ymax - ymin) + 1;
        rect = new Rect( xmin, ymin, newWidth, newHeight );

        return rect;
    }

    // ------------------------------------------------------------------ 
    /// \param _dest the target texture
    /// \param _src the src texture
    /// \param _pos the fill start position in target texture
    /// \param _srcRect the rect to fill
    /// \param _rotated if rotated
    /// fill the source texture to target texture
    // ------------------------------------------------------------------ 

    public static void Fill ( ref Color32[] _destPixels, 
                              int _destWidth,
                              Texture2D _src, 
                              string _name,
                              int _destX,
                              int _destY,
                              int _srcX,
                              int _srcY,
                              int _srcWidth,
                              int _srcHeight,
                              bool _rotated ) {
        Color32[] srcPixels = _src.GetPixels32(0);
        if ( _rotated == false ) {
            for ( int j = 0; j < _srcHeight; ++j ) {
                for ( int i = 0; i < _srcWidth; ++i ) {
                    int src_x = _srcX + i;
                    int src_y = _srcY + j;
                    int dest_x = _destX + i;
                    int dest_y = _destY + j;
                    Color32 pixel = srcPixels[src_x + src_y*_src.width];
                    _destPixels[dest_x + dest_y*_destWidth] = pixel;
                }
            }
        }
        else {
            int destWidth = _srcHeight;
            int destHeight = _srcWidth;
            for ( int j = 0; j < destHeight; ++j ) {
                for ( int i = 0; i < destWidth; ++i ) {
                    int src_x = _srcX + j;
                    int src_y = _srcY + _srcHeight - 1 - i;
                    int dest_x = _destX + i;
                    int dest_y = _destY + j;
                    Color32 pixel = srcPixels[src_x + src_y*_src.width];
                    _destPixels[dest_x + dest_y*_destWidth] = pixel;

                    // DEBUG { 
                    // try {
                    //     Color32 pixel = srcPixels[src_x + src_y*_src.width];
                    //     _dest.SetPixel( dest_x, dest_y, pixel );
                    // }
                    // catch {
                    //     Debug.Log( string.Format( "src_x = {0}, src_y = {1}, name = {2}", src_x, src_y, _name ) );
                    // }
                    // } DEBUG end 
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _tex the texture in which to apply contour bleed
    /// prevents edge artifacts due to bilinear filtering
    /// Note: Some image editors like Photoshop tend to fill purely transparent pixel with
    /// white color (R=1, G=1, B=1, A=0). This is generally OK, because these white pixels
    /// are impossible to see in normal circumstances.  However, when such textures are
    /// used in 3D with bilinear filtering, the shader will sometimes sample beyond visible
    /// edges into purely transparent pixels and the white color stored there will bleed
    /// into the visible edge.  This method scans the texture to find all purely transparent
    /// pixels that have a visible neighbor pixel, and copy the color data from that neighbor
    /// into the transparent pixel, while preserving its 0 alpha value.  In order to
    /// optimize the algorithm for speed of execution, a compromise is made to use any
    /// arbitrary neighboring pixel, as this should generally lead to correct results.
    /// It also limits itself to the immediate neighbors around the edge, resulting in a
    /// a bleed of a single pixel border around the edges, which should be fine, as bilinear
    /// filtering should generally not sample beyond that one pixel range.
    // ------------------------------------------------------------------ 

    // X and Y offsets used in contour bleed for sampling all around each purely transparent pixel
    private static readonly int[] bleedXOffsets = new [] { -1,  0,  1, -1,  1, -1,  0,  1 };
    private static readonly int[] bleedYOffsets = new [] { -1, -1, -1,  0,  0,  1,  1,  1 };

    public static void ApplyContourBleed ( ref Color32[] _result, Color32[] _srcPixels, int _textureWidth, Rect _rect ) {
        if ( _rect.width == 0 || _rect.height == 0 )
            return;

        int start_x = (int)_rect.x;
        int end_x = (int)(_rect.x + _rect.width);
        int start_y = (int)_rect.y;
        int end_y = (int)(_rect.y + _rect.height);

        for ( int x = start_x; x < end_x; ++x ) {
            for ( int y = start_y; y < end_y; ++y ) {

                // only try to bleed into purely transparent pixels
                if ( _srcPixels[x + y * _textureWidth].a == 0 ) {
                    // sample all around to find any non-purely transparent pixels
                    for ( int i = 0; i < bleedXOffsets.Length; i++ ) {
                        int sampleX = x + bleedXOffsets[i];
                        int sampleY = y + bleedYOffsets[i];

                        // check to stay within texture bounds
                        if (sampleX >= start_x && sampleX < end_x && sampleY >= start_y && sampleY < end_y) {
                            Color32 color = _srcPixels[sampleX + sampleY * _textureWidth];

                            if (color.a != 0) {
                                // Copy RGB color channels to purely transparent pixel, but preserving its 0 alpha
                                _result[x + y * _textureWidth] = new Color32(color.r, color.g, color.b, 0);
                                // _result[x + y * _textureWidth] = new Color32( 255, 255, 255, 255 );
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// \param _tex the texture in which to apply padding bleed
    /// \param _rect the bounds of the element around which to apply bleed
    /// prevents border artifacts due to bilinear filtering
    /// Note: Shaders with bilinear filtering will sometimes sample outside the bounds
    /// of the element, in the padding area, resulting in the padding color to bleed
    /// around the rectangular borders of the element.  This is true even when padding is
    /// purely transparent, because in that case, it is the 0 alpha that bleeds into the
    /// alpha of the outer pixels.  Such alpha bleed is especially problematic when
    /// trying to seamlessly tile multiple rectangular textures, as semi-transparent seams
    /// will sometimes appear at different scales.  This method duplicates a single row of
    /// pixels from the inner border of an element into the padding area.  This technique
    /// can be used with all kinds of textures without risk, even textures with uneven
    /// transparent edges, as it only allows the shader to sample more of the same (opaque
    /// or transparent) values when it exceeds the bounds of the element.
    // ------------------------------------------------------------------ 

    public static void ApplyPaddingBleed ( ref Color32[] _result, Color32[] _srcPixels, int _textureWidth, int _textureHeight, Rect _rect ) {
        // exAtlas allows bitmap font put in into atlas texture. Some font character didn't have
        // width and height (), they still need one element represent otherwise sprite will not index
        // on it and get null
        if ( _rect.width == 0 || _rect.height == 0 )
            return;
        
        // NOTE: Possible optimization: If Get/SetPixels32() make a copy of the data (instead
        // of just returning a reference to it, this method call might be very intensive on
        // CPU, as the *entire* atlas would be copied twice for *every* element.  A simple way
        // to optimize that would be to externalize the call to GetPixels32() out of this method
        // and out of the foreach, then call ApplyPaddingBleed() for every element, and finally
        // call SetPixel32() to copy data back into atlas.  It would require two foreach instead
        // of one, but the performance could be greatly improved.  That stands *only* if
        // Get/SetPixels32() make a copy of the data, otherwise there is no performance
        // cost to the current algorithm.  It might be worth investigating that...

        int start_x = (int)_rect.x;
        int end_x = (int)(_rect.x + _rect.width);
        int start_y = (int)_rect.y;
        int end_y = (int)(_rect.y + _rect.height);

        int yMin = start_y;
        int yMax = end_y-1;
        int xMin = start_x;
        int xMax = end_x-1;
        
        // Copy top and bottom rows of pixels
        for ( int x = start_x; x < end_x; ++x ) {
            // ignore clamp
            if ( yMin - 1 >= 0 ) {
                Color32 color = _srcPixels[x + yMin * _textureWidth];
                _result[x + (yMin - 1) * _textureWidth] = color;
            }

            // ignore clamp
            if ( yMax + 1 < _textureHeight ) {
                Color32 color = _srcPixels[x + yMax * _textureWidth];
                _result[x + (yMax + 1) * _textureWidth] = color;
            }
        }

        // Copy left and right columns of pixels (plus 2 extra pixels for corners)
        for ( int y = start_y; y < end_y; ++y ) {
            // ignore clamp
            if ( xMin - 1 >= 0 ) {
                Color32 color = _srcPixels[xMin + y * _textureWidth];
                _result[xMin - 1 + y * _textureWidth] = color;
            }

            // ignore clamp
            if ( xMax + 1 < _textureWidth ) {
                Color32 color = _srcPixels[xMax + y * _textureWidth];
                // color.a = 0;
                _result[xMax + 1 + y * _textureWidth] = color;
            }
        }

        // corners
        if ( xMin-1 >= 0 && yMin-1 >= 0 ) {
            Color32 color = _srcPixels[xMin + yMin * _textureWidth];
            _result[xMin - 1 + (yMin - 1) * _textureWidth] = color;
        }
        if ( xMin-1 >= 0 && yMax+1 < _textureHeight ) {
            Color32 color = _srcPixels[xMin + yMax * _textureWidth];
            _result[xMin - 1 + (yMax + 1) * _textureWidth] = color;
        }
        if ( xMax+1 < _textureWidth && yMax+1 < _textureHeight ) {
            Color32 color = _srcPixels[xMax + yMax * _textureWidth];
            _result[xMax + 1 + (yMax + 1) * _textureWidth] = color;
        }
        if ( xMax+1 < _textureWidth && yMin-1 >= 0 ) {
            Color32 color = _srcPixels[xMax + yMin * _textureWidth];
            _result[xMax + 1 + (yMin - 1) * _textureWidth] = color;
        }
    }
}
