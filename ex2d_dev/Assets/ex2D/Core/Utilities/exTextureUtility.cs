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

    public static Rect GetTrimTextureRect ( Texture2D _tex, int _trimThreshold = 1 ) {
        Rect rect = new Rect( 0, 0, 0, 0 );
        Color32[] pixels = _tex.GetPixels32(0);

        int xmin = _tex.width;
        int xmax = 0;
        int ymin = _tex.height;
        int ymax = 0;

        for ( int x = 0; x < _tex.width; ++x ) {
            for ( int y = 0; y < _tex.height; ++y ) {
                if ( pixels[x+y*_tex.width].a >= _trimThreshold ) {
                    xmin = x;
                    x = _tex.width;
                    break;
                }
            }
        }

        for ( int x = _tex.width-1; x >= 0; --x ) {
            for ( int y = 0; y < _tex.height; ++y ) {
                if ( pixels[x+y*_tex.width].a >= _trimThreshold ) {
                    xmax = x;
                    x = -1;
                    break;
                }
            }
        }

        for ( int y = 0; y < _tex.height; ++y ) {
            for ( int x = 0; x < _tex.width; ++x ) {
                if ( pixels[x+y*_tex.width].a >= _trimThreshold ) {
                    ymin = y;
                    y = _tex.height;
                    break;
                }
            }
        }

        for ( int y = _tex.height-1; y >= 0; --y ) {
            for ( int x = 0; x < _tex.width; ++x ) {
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

    public static void Fill ( Texture2D _dest, 
                              Texture2D _src, 
                              Vector2 _destPos, 
                              Rect _srcRect, 
                              bool _rotated ) {
        int xDest = (int)_destPos.x;
        int yDest = (int)_destPos.y;
        int xSrc = (int)_srcRect.x;
        int ySrc = (int)_srcRect.y;
        int srcWidth = (int)_srcRect.width;
        int srcHeight = (int)_srcRect.height;

        if ( _rotated == false ) {
            _dest.SetPixels( xDest, yDest, srcWidth, srcHeight, 
                             _src.GetPixels( xSrc, ySrc, srcWidth, srcHeight ) );
        }
        else {
            int destWidth = srcHeight;
            int destHeight = srcWidth;

            for ( int j = 0; j < destHeight; ++j ) {
                for ( int i = 0; i < destWidth; ++i ) {
                    // Color c = _src.GetPixel( xSrc + srcWidth - j, ySrc + _src.height + i );
                    // _dest.SetPixel( xDest + i, yDest + j, c ); 

                    Color c = _src.GetPixel( xSrc + j, ySrc + _src.height + i );
                    _dest.SetPixel( xDest + destWidth - i, yDest + j, c ); 
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

    public static Texture2D ApplyContourBleed ( Texture2D _tex ) {
        // Extract pixel buffer to be modified
        Color32[] pixels = _tex.GetPixels32(0);

        for ( int x = 0; x < _tex.width; ++x ) {
            for ( int y = 0; y < _tex.height; ++y ) {
                // only try to bleed into purely transparent pixels
                if ( pixels[x + y * _tex.width].a == 0 ) {
                    // sample all around to find any non-purely transparent pixels
                    for ( int i = 0; i < bleedXOffsets.Length; i++ ) {
                        int sampleX = x + bleedXOffsets[i];
                        int sampleY = y + bleedYOffsets[i];
						// check to stay within texture bounds
                        if (sampleX >= 0 && sampleX < _tex.width && sampleY >= 0 && sampleY < _tex.height) {
                            Color32 color = pixels[sampleX + sampleY * _tex.width];
                            if (color.a != 0) {
                                // Copy RGB color channels to purely transparent pixel, but preserving its 0 alpha
                                pixels[x + y * _tex.width] = new Color32(color.r, color.g, color.b, 0);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // Copy modified pixel buffer to new texture (to preserve original element texture and allow user to uncheck the option)
        Texture2D tex = new Texture2D(_tex.width, _tex.height, _tex.format, false);
        tex.SetPixels32(pixels);
        return tex;
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

    public static void ApplyPaddingBleed( Texture2D _tex, Rect _rect ) {
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
        
        // Extract pixel buffer to be modified
        Color32[] pixels = _tex.GetPixels32(0);
        
        // Copy top and bottom rows of pixels
        for ( int x = (int)_rect.xMin; x < (int)_rect.xMax; ++x ) {
            int yMin = (int)_rect.yMin;
            if (yMin - 1 >= 0) // Clamp
                pixels[x + (yMin - 1) * _tex.width] = pixels[x + yMin * _tex.width];

            int yMax = (int)_rect.yMax - 1;
            if (yMax + 1 < _tex.height) // Clamp
                pixels[x + (yMax + 1) * _tex.width] = pixels[x + yMax * _tex.width];
        }

        // Copy left and right columns of pixels (plus 2 extra pixels for corners)
        int startY = System.Math.Max((int)_rect.yMin - 1, 0); // Clamp
        int endY = System.Math.Min((int)_rect.yMax + 1, _tex.height); // Clamp
        for ( int y = startY; y < endY; ++y ) {
            int xMin = (int)_rect.xMin;
            if (xMin - 1 >= 0) // Clamp
                pixels[xMin - 1 + y * _tex.width] = pixels[xMin + y * _tex.width];

            int xMax = (int)_rect.xMax - 1;
            if (xMax + 1 < _tex.width) // Clamp
                pixels[xMax + 1 + y * _tex.width] = pixels[xMax + y * _tex.width];
        }

        // Copy modified pixel buffer back to same texture (we are modifying the destination atlas anyway)
        _tex.SetPixels32(pixels);
    }
}
