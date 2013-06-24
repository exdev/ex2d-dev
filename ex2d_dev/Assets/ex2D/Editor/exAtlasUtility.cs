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

    public static void ImportObjects ( exAtlas _atlas, Object[] _objects, System.Action<float,string> _progress ) {
        // check if create atlas directory
        _progress( 0.1f, "Checking atlas directory" );
        string path = Path.Combine ( ex2DEditor.atlasBuildPath, _atlas.name ); 
        if ( new DirectoryInfo(path).Exists == false ) {
            Directory.CreateDirectory (path);
        }

        // import textures used for atlas
        _progress( 0.2f, "Setup texture for atlas" );
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
        for ( int i = 0; i < _objects.Length; ++i ) {
            Object o = _objects[i];
            _progress( 0.2f + (float)i/(float)_objects.Length * 0.8f, "Add texture " + o.name );

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
                textureInfo.texture = _atlas.texture;
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

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void Build ( exAtlas _atlas, System.Action<float,string> _progress ) {
        TextureImporter importSettings = null;

        // check if create atlas directory
        _progress( 0.1f, "Checking atlas directory" );
        string path = Path.Combine ( ex2DEditor.atlasBuildPath, _atlas.name ); 
        if ( new DirectoryInfo(path).Exists == false ) {
            Directory.CreateDirectory (path);
        }

        // check if create atlas texture
        _progress( 0.2f, "Checking atlas texture" );
        string atlasTexturePath = Path.Combine(path, _atlas.name + ".png");
        Texture2D atlasTexture = AssetDatabase.LoadAssetAtPath( atlasTexturePath, typeof(Texture2D) ) as Texture2D;
        if ( atlasTexture == null ||
             atlasTexture.width != _atlas.width ||
             atlasTexture.height != _atlas.height ) 
        {
            atlasTexture = new Texture2D( _atlas.width, 
                                          _atlas.height, 
                                          TextureFormat.ARGB32, 
                                          false );

            // save texture to png
            File.WriteAllBytes(atlasTexturePath, atlasTexture.EncodeToPNG());
            Object.DestroyImmediate(atlasTexture);
            AssetDatabase.ImportAsset( atlasTexturePath, ImportAssetOptions.ForceSynchronousImport );

            // setup new texture
            importSettings = TextureImporter.GetAtPath(atlasTexturePath) as TextureImporter;
            importSettings.maxTextureSize = Mathf.Max( _atlas.width, _atlas.height );
            importSettings.textureFormat = TextureImporterFormat.AutomaticTruecolor;
            importSettings.isReadable = true;
            importSettings.wrapMode = TextureWrapMode.Clamp;
            importSettings.mipmapEnabled = false;
            importSettings.textureType = TextureImporterType.Advanced;
            importSettings.npotScale = TextureImporterNPOTScale.None;
            AssetDatabase.ImportAsset( atlasTexturePath, ImportAssetOptions.ForceSynchronousImport );

            atlasTexture = (Texture2D)AssetDatabase.LoadAssetAtPath( atlasTexturePath, typeof(Texture2D) );
        }

        // clean the atlas
        _progress( 0.3f, "Cleaning atlas texture" );
        importSettings = TextureImporter.GetAtPath(atlasTexturePath) as TextureImporter;
        if ( importSettings.isReadable == false ) {
            importSettings.isReadable = true;
            AssetDatabase.ImportAsset( atlasTexturePath, ImportAssetOptions.ForceSynchronousImport );
        }
        Color buildColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        if ( _atlas.customBuildColor ) {
            buildColor = _atlas.buildColor;
        }
        for ( int i = 0; i < _atlas.width; ++i ) {
            for ( int j = 0; j < _atlas.height; ++j ) {
                atlasTexture.SetPixel(i, j, buildColor );
            }
        }
        atlasTexture.Apply(false);

        // fill raw texture to atlas
        _progress( 0.4f, "Filling texture-info to atlas" );
        foreach ( exTextureInfo textureInfo in _atlas.textureInfos ) {
            Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D>(textureInfo.rawTextureGUID); 
            if ( exEditorUtility.IsValidForAtlas(rawTexture) == false ) {
                exEditorUtility.ImportTextureForAtlas(rawTexture);
            }
            if ( textureInfo.texture != atlasTexture ) {
                textureInfo.texture = atlasTexture;
                EditorUtility.SetDirty(textureInfo);
            }

            // NOTE: we do this because the texture already been trimmed, and only this way to make texture have better filter
            // apply contour bleed
            if ( _atlas.useContourBleed ) {
                rawTexture = exTextureUtility.ApplyContourBleed( rawTexture );
            }

            // copy raw texture into atlas texture
            exTextureUtility.Fill( atlasTexture
                                 , rawTexture
                                 , new Vector2 ( textureInfo.x, textureInfo.y )
                                 , new Rect ( textureInfo.trim_x, textureInfo.trim_y, textureInfo.width, textureInfo.height )
                                 , textureInfo.rotated
                                 );

            //
            if ( _atlas.useContourBleed ) {
                Object.DestroyImmediate(rawTexture);
            }

            // apply padding bleed
            if ( _atlas.usePaddingBleed ) {
                exTextureUtility.ApplyPaddingBleed( atlasTexture,
                                                    new Rect( textureInfo.x, textureInfo.y, textureInfo.width, textureInfo.height ) );
            }
        }

        // write new atlas texture to disk
        File.WriteAllBytes(atlasTexturePath, atlasTexture.EncodeToPNG());
        AssetDatabase.ImportAsset( atlasTexturePath, ImportAssetOptions.ForceSynchronousImport );

        // turn-off readable to save memory
        if ( _atlas.readable == false ) {
            importSettings = TextureImporter.GetAtPath(atlasTexturePath) as TextureImporter;
            importSettings.isReadable = false;
            AssetDatabase.ImportAsset( atlasTexturePath, ImportAssetOptions.ForceSynchronousImport );
        }

        //
        _atlas.texture = (Texture2D)AssetDatabase.LoadAssetAtPath( atlasTexturePath, typeof(Texture2D) );
        _atlas.needRebuild = false;
    }
}