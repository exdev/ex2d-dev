// ======================================================================================
// File         : exAtlasUtility.cs
// Author       : Wu Jie 
// Last Change  : 06/18/2013 | 01:01:25 AM | Tuesday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// exAtlas editor helper function
///
///////////////////////////////////////////////////////////////////////////////

public static class exAtlasUtility {

    class PackNode {
        public Rect rect;
        public PackNode right = null;
        public PackNode bottom = null;

        public PackNode ( Rect _rect ) { rect = _rect; }
        public Vector2? Insert ( exTextureInfo _info, int _padding ) {
            // when this node is already occupied (when it has children),
            // forward to child nodes recursively
            if (right != null) {
                Vector2? pos = right.Insert(_info, _padding);
                if (pos != null)
                    return pos;
                return bottom.Insert(_info, _padding);
            }

            // determine trimmed and padded sizes
            float trimmedWidth = _info.width;
            float trimmedHeight = _info.height;
            float paddedWidth = trimmedWidth + _padding;
            float paddedHeight = trimmedHeight + _padding;

            // trimmed element size must fit within current node rect
            if (trimmedWidth > rect.width || trimmedHeight > rect.height)
                return null;

            // create first child node in remaining space to the right, using trimmedHeight
            // so that only other elements with the same height or less can be added there
            // (we do not use paddedHeight, because the padding area is reserved and should
            // never be occupied)
            right = new PackNode( new Rect ( rect.x + paddedWidth, 
                                         rect.y,
                                         rect.width - paddedWidth, 
                                         trimmedHeight ) );

            // create second child node in remaining space at the bottom, occupying the entire width
            bottom = new PackNode( new Rect ( rect.x,
                                          rect.y + paddedHeight,
                                          rect.width, 
                                          rect.height - paddedHeight ) );

            // return position where to put element
            return new Vector2( rect.x, rect.y );
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void TreePack ( exAtlas _atlas ) {
        PackNode root = new PackNode( new Rect( 0,
                                                0,
                                                _atlas.width,
                                                _atlas.height ) );
        foreach ( exTextureInfo info in _atlas.textureInfos ) {
            Vector2? pos = root.Insert (info, _atlas.actualPadding);
            if (pos != null) {
                info.x = (int)pos.Value.x;
                info.y = (int)pos.Value.y;
            }
            else {
                // log warning but continue processing other elements
                Debug.LogWarning("Failed to layout texture info " + info.name);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void BasicPack ( exAtlas _atlas ) {
        int curX = 0;
        int curY = 0;
        int maxY = 0; 
        int i = 0; 

        foreach ( exTextureInfo info in _atlas.textureInfos ) {
            if ( (curX + info.rotatedWidth) > _atlas.width ) {
                curX = 0;
                curY = curY + maxY + _atlas.actualPadding;
                maxY = 0;
            }
            if ( (curY + info.rotatedHeight) > _atlas.height ) {
                Debug.LogWarning( "Failed to layout element " + info.name );
                break;
            }
            info.x = curX;
            info.y = curY;

            curX = curX + info.rotatedWidth + _atlas.actualPadding;
            if (info.rotatedHeight > maxY) {
                maxY = info.rotatedHeight;
            }
            ++i;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ImportObjects ( exAtlas _atlas, Object[] _objects ) {
        string path = Path.Combine ( ex2DEditor.atlasBuildPath, _atlas.name ); 
        if ( new DirectoryInfo(path).Exists == false ) {
            Directory.CreateDirectory (path);
        }

        // import textures used for atlas
        try {
            AssetDatabase.StartAssetEditing();
            foreach ( Object o in _objects ) {
                if ( o is Texture2D ) {
                    exEditorUtility.ImportTextureForAtlas (o as Texture2D);
                }
            }
        }
        finally {
            AssetDatabase.StopAssetEditing();
        }

        //
        foreach ( Object o in _objects ) {
            if ( o is Texture2D ) {
                Texture2D rawTexture = o as Texture2D;

                // if the texture already in the atlas, warning and skip it.
                if ( exAtlasUtility.Exists(_atlas,rawTexture) ) {
                    Debug.LogWarning ( "The texture " + o.name + " already exists in the atlas" );
                    continue;
                }

                Rect trimRect = new Rect ( 0, 0, rawTexture.width, rawTexture.height );
                if ( _atlas.trimElements ) {
                    trimRect = exTextureUtility.GetTrimTextureRect(rawTexture);
                }

                //
                exTextureInfo textureInfo = exGenericAssetUtility<exTextureInfo>.LoadExistsOrCreate( path, rawTexture.name );
                textureInfo.rawTextureGUID = exEditorUtility.AssetToGUID(rawTexture);
                textureInfo.name = rawTexture.name;
                textureInfo.texture = rawTexture;
                textureInfo.rawWidth = rawTexture.width;
                textureInfo.rawHeight = rawTexture.height;
                textureInfo.x = 0;
                textureInfo.y = 0;
                textureInfo.trim_x = (int)trimRect.x;
                textureInfo.trim_y = (int)trimRect.y;
                textureInfo.width = (int)trimRect.width;
                textureInfo.height = (int)trimRect.height;
                textureInfo.trim = _atlas.trimElements;

                _atlas.textureInfos.Add(textureInfo);
            }
        }

        EditorUtility.SetDirty(_atlas);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static bool Exists ( exAtlas _atlas, Texture2D _texture ) {
        string guid = exEditorUtility.AssetToGUID(_texture);
        foreach ( exTextureInfo info in _atlas.textureInfos ) {
            if ( info.rawTextureGUID == guid ) {
                return true;
            }
        }
        return false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static bool Exists ( exAtlas _atlas, exTextureInfo _textureInfo ) {
        return _atlas.textureInfos.IndexOf(_textureInfo) != -1;
    }
} 
