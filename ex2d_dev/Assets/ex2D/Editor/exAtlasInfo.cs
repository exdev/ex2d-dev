// ======================================================================================
// File         : exAtlasInfo.cs
// Author       : Wu Jie 
// Last Change  : 06/15/2013 | 10:06:53 AM | Saturday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
///
/// The atlas editor information asset
///
///////////////////////////////////////////////////////////////////////////////

public partial class exAtlasInfo : ScriptableObject {

    // ------------------------------------------------------------------ 
    /// the algorithm type of texture packer
    // ------------------------------------------------------------------ 

    public enum Algorithm {
        Basic, ///< basic algorithm, pack texture by the sort order
        // Shelf, // TODO
        Tree,  ///< Pack the textures by binary space, find the most fitable texture in it
        // MaxRect, // TODO
    }

    // ------------------------------------------------------------------ 
    /// sorting type for sort elements
    // ------------------------------------------------------------------ 

    public enum SortBy {
        UseBest, ///< use the best sorting result depends on exAtlasInfo.algorithm
        Width,   ///< sort by texture width
        Height,  ///< sort by texture height
        Area,    ///< sort by texture area 
        Name     ///< sort by texture name
    }

    // ------------------------------------------------------------------ 
    /// sorting the elements in Ascending or Descending order
    // ------------------------------------------------------------------ 

    public enum SortOrder {
        UseBest,   ///< use the best order depends on the exAtlasInfo.algorithm
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
    public Texture2D texture; ///< the referenced atlas texture
    public Material material; ///< the default material we used
    public bool useBuildColor = false; ///< use buildColor as background color for transparent pixels
    public Color buildColor = new Color(1.0f, 1.0f, 1.0f, 0.0f); ///< the color of transparent pixels in atlas texture
    public bool useContourBleed = false; ///< extends the color of pixels at the edge of transparent pixels to prevent bilinear filtering artifacts
    public bool usePaddingBleed = false; ///< extends the color and alpha of pixels on border of each element into the surrounding padding area
    public bool trimElements = true; ///< trim all element when importing
    public bool readable = false; ///< enabled Read/Write option for atlas texture after build

    // canvas settings
    public Color bgColor = Color.white; ///< the canvas background color
    public bool showCheckerboard = true; ///< if show the checkerboard

    // layout settings
    public Algorithm algorithm = Algorithm.Tree; ///< the algorithm used for texture packer
    public SortBy sortBy = SortBy.UseBest; ///< the method to sort the elements in atlas editor info
    public SortOrder sortOrder = SortOrder.UseBest; ///< the order to sort the elements in atlas editor info
    public PaddingMode paddingMode = PaddingMode.Auto; ///< the padding mode used to determine the actualPadding value
    public int customPadding = 1; ///< the user-specified padding size between each element, used when paddingMode is Custom
    public bool allowRotate = false; ///< if allow texture rotated, disabled in current version 

    // element settings
    public Color elementBgColor = new Color( 1.0f, 1.0f, 1.0f, 0.0f ); ///< the background color of each element
    public Color elementSelectColor = new Color( 0.0f, 0.0f, 1.0f, 1.0f ); ///< the select rect color of each element

    //
    public float scale = 1.0f; ///< the zoom value of the atlas

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

    // a > b = 1, a < b = -1, a = b = 0
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static int CompareByWidth ( exTextureInfo _a, exTextureInfo _b ) {
        int ret = (int)_a.width - (int)_b.width;
        // if ( ret == 0 ) {
        //     ret = string.Compare( exEditorHelper.AssetToGUID(_a.texture), exEditorHelper.AssetToGUID(_b.texture) );
        // }
        // if ( _a.isFontElement && _b.isFontElement && ret == 0 ) {
        //     ret = _a.charInfo.id - _b.charInfo.id;
        // }
        return ret;
    }
    public static int CompareByHeight ( exTextureInfo _a, exTextureInfo _b ) {
        int ret = (int)_a.height - (int)_b.height;
        // if ( ret == 0 ) {
        //     ret = string.Compare( exEditorHelper.AssetToGUID(_a.texture), exEditorHelper.AssetToGUID(_b.texture) );
        // }
        // if ( _a.isFontElement && _b.isFontElement && ret == 0 ) {
        //     ret = _a.charInfo.id - _b.charInfo.id;
        // }
        return ret;
    }
    public static int CompareByArea ( exTextureInfo _a, exTextureInfo _b ) {
        int ret = (int)_a.width * (int)_a.height - (int)_b.width * (int)_b.height;
        // if ( ret == 0 ) {
        //     ret = string.Compare( exEditorHelper.AssetToGUID(_a.texture), exEditorHelper.AssetToGUID(_b.texture) );
        // }
        // if ( _a.isFontElement && _b.isFontElement && ret == 0 ) {
        //     ret = _a.charInfo.id - _b.charInfo.id;
        // }
        return ret;
    }
    // public static int CompareByName ( exTextureInfo _a, exTextureInfo _b ) {
    //     int ret = string.Compare( _a.texture.name, _b.texture.name );
    //     if ( ret == 0 ) {
    //         ret = string.Compare( exEditorHelper.AssetToGUID(_a.texture), exEditorHelper.AssetToGUID(_b.texture) );
    //     }
    //     if ( _a.isFontElement && _b.isFontElement && ret == 0 ) {
    //         ret = _a.charInfo.id - _b.charInfo.id;
    //     }
    //     return ret;
    // }

    // public static int CompareByWidthRotate ( exTextureInfo _a, exTextureInfo _b ) {
    //     int a_size = (int)_a.trimRect.height;
    //     if ( (int)_a.trimRect.height > (int)_a.trimRect.width ) {
    //         a_size = (int)_a.trimRect.height;
    //         _a.rotated = true;
    //     }
    //     int b_size = (int)_b.trimRect.height;
    //     if ( (int)_b.trimRect.height > (int)_b.trimRect.width ) {
    //         b_size = (int)_b.trimRect.height;
    //         _b.rotated = true;
    //     }
    //     int ret = a_size - b_size;
    //     if ( ret == 0 ) {
    //         ret = string.Compare( exEditorHelper.AssetToGUID(_a.texture), exEditorHelper.AssetToGUID(_b.texture) );
    //     }
    //     if ( _a.isFontElement && _b.isFontElement && ret == 0 ) {
    //         ret = _a.charInfo.id - _b.charInfo.id;
    //     }
    //     return ret;
    // }
    // public static int CompareByHeightRotate ( exTextureInfo _a, exTextureInfo _b ) {
    //     int a_size = (int)_a.trimRect.height;
    //     if ( (int)_a.trimRect.width > (int)_a.trimRect.height ) {
    //         a_size = (int)_a.trimRect.width;
    //         _a.rotated = true;
    //     }
    //     int b_size = (int)_b.trimRect.height;
    //     if ( (int)_b.trimRect.width > (int)_b.trimRect.height ) {
    //         b_size = (int)_b.trimRect.width;
    //         _b.rotated = true;
    //     }
    //     int ret = a_size - b_size;
    //     if ( ret == 0 ) {
    //         ret = string.Compare( exEditorHelper.AssetToGUID(_a.texture), exEditorHelper.AssetToGUID(_b.texture) );
    //     }
    //     if ( _a.isFontElement && _b.isFontElement && ret == 0 ) {
    //         ret = _a.charInfo.id - _b.charInfo.id;
    //     }
    //     return ret;
    // }

    // ///////////////////////////////////////////////////////////////////////////////
    // // functions
    // ///////////////////////////////////////////////////////////////////////////////

    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // public void UpdateElement ( Texture2D _tex, bool _trim, bool _noImport = true ) {
    //     //
    //     if ( exTextureHelper.IsValidForAtlas (_tex) == false ) {
    //         if ( _noImport ) {
    //             Debug.LogError("Invalid texture settings for atlas, texture name " + _tex.name );
    //             return;
    //         }
    //         exTextureHelper.ImportTextureForAtlas(_tex);
    //     }

    //     //
    //     exAtlasInfo.exTextureInfo el = null;
    //     for ( int i = 0; i < elements.Count; ++i ) {
    //         el = elements[i];
    //         if ( el.texture == _tex )
    //             break;
    //     }

    //     //
    //     if ( el == null ) {
    //         Debug.LogError("can't find element by texture " + _tex.name );
    //         return;
    //     }

    //     //
    //     if ( _trim ) {
    //         el.trimRect = exTextureHelper.GetTrimTextureRect(_tex);
    //     }
    //     else {
    //         el.trimRect = new Rect( 0, 0, _tex.width, _tex.height );
    //     }
    //     el.trim = _trim;
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _tex the raw texture you want to add
    // /// \param _trim if trim the texture
    // /// \return the new element
    // /// add the element by raw texture 
    // // ------------------------------------------------------------------ 

    // public exTextureInfo AddElement ( Texture2D _tex, bool _trim ) {
    //     if ( exTextureHelper.IsValidForAtlas (_tex) == false )
    //         exTextureHelper.ImportTextureForAtlas(_tex);

    //     //
    //     exAtlasInfo.exTextureInfo el = new exAtlasInfo.exTextureInfo();
    //     if ( _trim ) {
    //         el.trimRect = exTextureHelper.GetTrimTextureRect(_tex);
    //     }
    //     else {
    //         el.trimRect = new Rect( 0, 0, _tex.width, _tex.height );
    //     }

    //     el.rotated = false;
    //     el.trim = _trim;
    //     el.atlasInfo = this;
    //     el.texture = _tex;
    //     el.coord[0] = 0;
    //     el.coord[1] = 0;
    //     elements.Add(el);

    //     // get sprite animation clip by textureGUID, add them to rebuildAnimClipGUIDs
    //     AddSpriteAnimClipForRebuilding(el);

    //     //
    //     needRebuild = true;
    //     EditorUtility.SetDirty(this);

    //     return el;
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _fontInfo the font info you want to remove
    // /// Find and remove the font info from the atlas
    // // ------------------------------------------------------------------ 

    // public void RemoveBitmapFont ( exBitmapFont _fontInfo ) {
    //     for ( int i = 0; i < elements.Count; ++i ) {
    //         exAtlasInfo.exTextureInfo el = elements[i];
    //         if ( el.isFontElement == false )
    //             continue;

    //         if ( el.destFontInfo == _fontInfo ) {
    //             RemoveElement (el);
    //             --i;
    //         }
    //     }
    //     bitmapFonts.Remove(_fontInfo);
    //     EditorUtility.SetDirty(this);
    // }

    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // protected exTextureInfo AddFontElement ( exBitmapFont _srcFontInfo, exBitmapFont _destFontInfo, exBitmapFont.CharInfo _charInfo ) {
    //     exAtlasInfo.exTextureInfo el = new exAtlasInfo.exTextureInfo();
    //     el.isFontElement = true;

    //     el.srcFontInfo = _srcFontInfo;
    //     el.destFontInfo = _destFontInfo;
    //     el.charInfo = _charInfo;

    //     el.trimRect = new Rect( _charInfo.x, _charInfo.y, _charInfo.width, _charInfo.height );
    //     el.rotated = false;
    //     el.trim = true;
    //     el.atlasInfo = this;
    //     el.texture = _srcFontInfo.pageInfos[0].texture;
    //     el.coord[0] = 0;
    //     el.coord[1] = 0;

    //     exBitmapFont.CharInfo destCharInfo = el.destFontInfo.GetCharInfo(el.charInfo.id);
    //     if ( destCharInfo != null ) {
    //         destCharInfo.id = el.charInfo.id;
    //         destCharInfo.x = el.charInfo.x;
    //         destCharInfo.y = el.charInfo.y;
    //         destCharInfo.width = el.charInfo.width;
    //         destCharInfo.height = el.charInfo.height;
    //         destCharInfo.xoffset = el.charInfo.xoffset;
    //         destCharInfo.yoffset = el.charInfo.yoffset;
    //         destCharInfo.xadvance = el.charInfo.xadvance;
    //         destCharInfo.page = el.charInfo.page;
    //         destCharInfo.uv0 = el.charInfo.uv0;
    //     }
    //     else {
    //         Debug.LogError ( "can't not find char info with ID " + el.charInfo.id );
    //     }

    //     elements.Add(el);

    //     needRebuild = true;
    //     EditorUtility.SetDirty(this);

    //     return el;
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _el the element you expect to remove
    // /// remove an element from the atlas info
    // // ------------------------------------------------------------------ 

    // public void RemoveElement ( exTextureInfo _el ) {
    //     int idx = elements.IndexOf(_el);
    //     if ( idx != -1 ) {
    //         RemoveElementAt (idx);
    //     }
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _idx the index of the element 
    // /// remove an element from the atlas info by index
    // // ------------------------------------------------------------------ 

    // public void RemoveElementAt ( int _idx ) {
    //     exTextureInfo el = elements[_idx];

    //     // get sprite animation clip by textureGUID, add them to rebuildAnimClipGUIDs
    //     AddSpriteAnimClipForRebuilding(el);
    //     needRebuild = true;

    //     // register undo
    //     Undo.RegisterUndo ( new Object[] {
    //                         exAtlasDB.DB(),
    //                         this
    //                         }, "RemoveElementAt" );

    //     // remove element in atlas DB
    //     exAtlasDB.RemoveElementInfo(exEditorHelper.AssetToGUID(el.texture));

    //     //
    //     elements.RemoveAt(_idx);

    //     //
    //     EditorUtility.SetDirty(this);
    // }

    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // public void ResetElements () {
    //     foreach ( exTextureInfo el in elements ) {
    //         el.rotated = false;
    //     }
    //     needRebuild = true;
    //     EditorUtility.SetDirty(this);
    // }

    // // ------------------------------------------------------------------ 
    // /// Sort the elemtns in atlas info by the exAtlasInfo.SortBy and exAtlasInfo.SortOrder 
    // // ------------------------------------------------------------------ 

    // public void SortElements () {
    //     //
    //     SortBy mySortBy = sortBy;
    //     SortOrder mySortOrder = sortOrder;
    //     if ( sortBy == SortBy.UseBest ) {
    //         switch ( algorithm ) {
    //         case Algorithm.Basic:
    //             mySortBy = SortBy.Height;
    //             break;
    //         case Algorithm.Tree:
    //             mySortBy = SortBy.Height;
    //             break;
    //         default:
    //             mySortBy = SortBy.Height;
    //             break;
    //         }
    //     }
    //     if ( sortOrder == SortOrder.UseBest ) {
    //         mySortOrder = SortOrder.Descending;
    //     }

    //     // sort by
    //     switch ( mySortBy ) {
    //     case SortBy.Width:
    //         if ( allowRotate )
    //             elements.Sort( CompareByWidthRotate );
    //         else
    //             elements.Sort( CompareByWidth );
    //         break;
    //     case SortBy.Height:
    //         if ( allowRotate )
    //             elements.Sort( CompareByHeightRotate );
    //         else
    //             elements.Sort( CompareByHeight );
    //         break;
    //     case SortBy.Area:
    //         elements.Sort( CompareByArea );
    //         break;
    //     case SortBy.Name:
    //         elements.Sort( CompareByName );
    //         break;
    //     }

    //     // sort order
    //     if ( mySortOrder == SortOrder.Descending ) {
    //         elements.Reverse();
    //     }
    //     needRebuild = true;
    //     EditorUtility.SetDirty(this);
    // }

    // // ------------------------------------------------------------------ 
    // /// Clear the all pixels in atlas texture, and fill with white color
    // // ------------------------------------------------------------------ 

    // public void ClearAtlasTexture () {
    //     for ( int j = 0; j < texture.height; ++j )
    //         for ( int i = 0; i < texture.width; ++i )
    //             texture.SetPixel( i, j, new Color(1.0f, 1.0f, 1.0f, 0.0f) );
    //     texture.Apply(false);
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _el
    // /// Add the sprite animation clip for rebuild by checking if clip contains the in element's exAtlasInfo.exTextureInfo.texture
    // // ------------------------------------------------------------------ 

    // public void AddSpriteAnimClipForRebuilding ( exTextureInfo _el ) {
    //     List<string> spAnimClipGUIDs 
    //         = exSpriteAnimationDB.GetSpriteAnimClipGUIDs ( exEditorHelper.AssetToGUID(_el.texture) );

    //     if ( spAnimClipGUIDs != null ) {
    //         foreach ( string animClipGUID in spAnimClipGUIDs ) {
    //             if ( rebuildAnimClipGUIDs.IndexOf(animClipGUID) == -1 ) {
    //                 rebuildAnimClipGUIDs.Add(animClipGUID);
    //             }
    //         }
    //     }
    // }

    // // ------------------------------------------------------------------ 
    // /// \param _objects 
    // /// get the Texture2D and exBitmapFont from a list of objects, import them into atlas
    // // ------------------------------------------------------------------ 

    // public void ImportObjects ( Object[] _objects ) {
    //     bool dirty = false;
    //     foreach ( Object o in _objects ) {
    //         if ( o is Texture2D ) {
    //             Texture2D t = o as Texture2D;
    //             exAtlasDB.ElementInfo elInfo = exAtlasDB.GetElementInfo(t);
    //             if ( elInfo == null ) {
    //                 AddElement( t, trimElements );
    //                 dirty = true;
    //             }
    //             else {
    //                 Debug.LogError( "The texture [" + t.name + "]" + 
    //                                 " has already been added in atlas: " +
    //                                 AssetDatabase.GUIDToAssetPath(elInfo.guidAtlasInfo) );
    //             }
    //         }
    //         else if ( o is exBitmapFont ) {
    //             exBitmapFont f = o as exBitmapFont;
    //             if ( f.inAtlas ) {
    //                 // NOTE: it is still possible we have atlas font in the obj list since we use Selection.GetFiltered().
    //                 continue;
    //             }

    //             // multi-page atlas font is forbit
    //             if ( f.pageInfos.Count > 1 ) {
    //                 Debug.LogError("Can't not create atlas font from " + f.name + ", it has multiple page info.");
    //                 continue;
    //             }

    //             // check if we have resource in the project
    //             string assetPath = AssetDatabase.GetAssetPath(texture);
    //             string dirname = Path.GetDirectoryName(assetPath);
    //             string filename = Path.GetFileNameWithoutExtension(assetPath);
    //             string bitmapFontPath = Path.Combine( dirname, filename + " - " + f.name + ".asset" );
    //             exBitmapFont f2 = (exBitmapFont)AssetDatabase.LoadAssetAtPath( bitmapFontPath,
    //                                                                            typeof(exBitmapFont) );
    //             if ( f2 == null ) {
    //                 f2 = (exBitmapFont)ScriptableObject.CreateInstance(typeof(exBitmapFont));
    //                 f2.inAtlas = true;
    //                 f2.name = f.name;
    //                 f2.lineHeight = f.lineHeight;

    //                 // add page info
    //                 exBitmapFont.PageInfo pageInfo = new exBitmapFont.PageInfo();
    //                 pageInfo.texture = texture;
    //                 pageInfo.material = material;
    //                 f2.pageInfos.Add(pageInfo);

    //                 // add char info
    //                 foreach ( exBitmapFont.CharInfo c in f.charInfos ) {
    //                     exBitmapFont.CharInfo c2 = new exBitmapFont.CharInfo(c);
    //                     f2.charInfos.Add(c2);
    //                 }

    //                 // add kerning info
    //                 foreach ( exBitmapFont.KerningInfo k in f.kernings ) {
    //                     f2.kernings.Add(k);
    //                 }

    //                 AssetDatabase.CreateAsset ( f2, bitmapFontPath );

    //                 //
    //                 foreach ( exBitmapFont.CharInfo c in f2.charInfos ) {
    //                     if ( c.id == -1 )
    //                         continue;
    //                     AddFontElement( f, f2, c );
    //                 }
    //             }
    //             else {
    //                 Debug.LogError("You already add the BitmapFont in this Atlas");
    //             }

    //             //
    //             if ( bitmapFonts.IndexOf(f2) == -1 ) {
    //                 bitmapFonts.Add(f2);
    //             }

    //             dirty = true;
    //         }
    //         if ( dirty ) {
    //             EditorUtility.SetDirty(this);
    //         }
    //     }
    // }
}


