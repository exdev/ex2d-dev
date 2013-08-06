// ======================================================================================
// File         : exAtlas.cs
// Author       : Wu Jie 
// Last Change  : 06/18/2013 | 00:16:02 AM | Tuesday,June
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
///
/// The atlas editor information asset
///
///////////////////////////////////////////////////////////////////////////////

public class exAtlas : ScriptableObject {

    // ------------------------------------------------------------------ 
    /// the algorithm type of texture packer
    // ------------------------------------------------------------------ 

    public enum Algorithm {
        Basic, ///< basic algorithm, pack texture by the sort order
        Tree,  ///< Pack the textures by binary space, find the most fitable texture in it
        MaxRect,
    }

    // ------------------------------------------------------------------ 
    /// sorting type for sort textureInfos
    // ------------------------------------------------------------------ 

    public enum SortBy {
        UseBest, ///< use the best sorting result depends on exAtlas.algorithm
        Width,   ///< sort by texture width
        Height,  ///< sort by texture height
        Area,    ///< sort by texture area 
        Name     ///< sort by texture name
    }

    // ------------------------------------------------------------------ 
    /// sorting the textureInfos in Ascending or Descending order
    // ------------------------------------------------------------------ 

    public enum SortOrder {
        UseBest,   ///< use the best order depends on the exAtlas.algorithm
        Ascending, ///< use ascending order 
        Descending ///< use descending order
    }

    // ------------------------------------------------------------------ 
    /// the padding mode used to determine the actualPadding value
    // ------------------------------------------------------------------ 

    public enum PaddingMode {
        None,   ///< 0 pixel padding
        Auto,   ///< 2 pixels padding when usePaddingBleed is true, otherwise 1 pixel padding
        Custom  ///< custom padding value is specified by user
    }

    //
    public int width = 512; ///< the width of the atlas texture 
    public int height = 512; ///< the height of the atlas texture
    public List<exTextureInfo> textureInfos = new List<exTextureInfo>(); ///< the list of exTextureInfo
    public List<exBitmapFont> bitmapFonts = new List<exBitmapFont>(); ///< the list of exBitmapFont
    public Texture2D texture; ///< the referenced atlas texture
    public bool customBuildColor = false; ///< use buildColor as background color for transparent pixels
    public Color buildColor = new Color(1.0f, 1.0f, 1.0f, 0.0f); ///< the color of transparent pixels in atlas texture
    public bool useContourBleed = true; ///< extends the color of pixels at the edge of transparent pixels to prevent bilinear filtering artifacts
    public bool usePaddingBleed = true; ///< extends the color and alpha of pixels on border of each element into the surrounding padding area
    public bool trimElements = true; ///< trim all element when importing
    [SerializeField] public int trimThreshold_ = 1; ///< 
    public int trimThreshold {
        get { return trimThreshold_; }
        set {
            trimThreshold_ = System.Math.Min ( System.Math.Max( value, 1 ), 255 );
        }
    }
    public bool readable = false; ///< enabled Read/Write option for atlas texture after build

    // canvas settings
    public Color bgColor = Color.white; ///< the canvas background color
    public bool showCheckerboard = true; ///< if show the checkerboard

    // layout settings
    public Algorithm algorithm = Algorithm.MaxRect; ///< the algorithm used for texture packer
    public SortBy sortBy = SortBy.UseBest; ///< the method to sort the textureInfos in atlas editor info
    public SortOrder sortOrder = SortOrder.UseBest; ///< the order to sort the textureInfos in atlas editor info
    public PaddingMode paddingMode = PaddingMode.Auto; ///< the padding mode used to determine the actualPadding value
    public int customPadding = 1; ///< the user-specified padding size between each element, used when paddingMode is Custom
    public bool allowRotate = true; ///< if allow texture rotated, disabled in current version 

    // element settings
    public Color elementBgColor = new Color( 1.0f, 1.0f, 1.0f, 0.0f ); ///< the background color of each element
    public Color elementSelectColor = new Color( 0.0f, 0.0f, 1.0f, 1.0f ); ///< the select rect color of each element

    //
    [SerializeField] float scale_ = 1.0f; ///< the zoom value of the atlas
    public float scale {
        get { return scale_; }
        set {
            if ( scale_ != value ) {
                scale_ = value;
                scale_ = Mathf.Clamp( scale_, 0.1f, 2.0f );
                scale_ = Mathf.Round( scale_ * 100.0f ) / 100.0f;
            }
        }
    }

    // bitmap fonts
    // public List<exBitmapFont> bitmapFonts = new List<exBitmapFont>(); ///< the list of bitmap fonts in the atlas

    //
    public bool needRebuild = false; ///< if need rebuild the atlas
    public bool needLayout = false; ///< if need layout the atlas

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \return the dynamically computed padding value to use for building the atlas
    // ------------------------------------------------------------------ 

    public int actualPadding {
        get {
            if (paddingMode == PaddingMode.None)
                return 0;

            if (paddingMode == PaddingMode.Custom)
                return customPadding;

            // PaddingMode.Auto : padding bleed requires 2 pixels, otherwise 1 pixel is enough
            return usePaddingBleed ? 2 : 1;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // static
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public exTextureInfo GetTextureInfoByName ( string _name ) {
        for ( int i = 0; i < textureInfos.Count; ++i ) {
            exTextureInfo textureInfo = textureInfos[i];
            if ( textureInfo.name == _name )
                return textureInfo;
        }
        return null;
    }
}
