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

    public class Element {
        public string id = ""; // texture-info is its name, charinfo is its "bitmapfont.name@id"
        public int x = 0;
        public int y = 0;
        public int width = 1;
        public int height = 1;
        public bool rotated = false;

        // raw referenced
        public exTextureInfo textureInfo = null; 
        public exBitmapFont.CharInfo charInfo = null;

        public int rotatedWidth {
            get {
                if ( rotated ) return height;
                return width;
            }
        }

        public int rotatedHeight {
            get {
                if ( rotated ) return width;
                return height;
            }
        }

        public void Apply () {
            if ( textureInfo != null ) {
                textureInfo.x = x;
                textureInfo.y = y;
                textureInfo.width = width;
                textureInfo.height = height;
                textureInfo.rotated = rotated;

                EditorUtility.SetDirty(textureInfo);
            }
            else if ( charInfo != null ) {
                charInfo.x = x;
                charInfo.y = y;
                charInfo.width = width;
                charInfo.height = height;
                charInfo.rotated = rotated;

                // TODO: EditorUtility.SetDirty(el.charInfo.bitmapFont); ????
            }
            else {
                Debug.LogWarning( "Can't find the raw reference of atlas element " + id );
            }
        }
    }

    public class PackNode {
        public Rect rect;
        public PackNode right = null;
        public PackNode bottom = null;

        public PackNode ( Rect _rect ) { rect = _rect; }
        public Vector2? Insert ( Element _el, int _padding ) {
            // when this node is already occupied (when it has children),
            // forward to child nodes recursively
            if (right != null) {
                Vector2? pos = right.Insert(_el, _padding);
                if (pos != null)
                    return pos;
                return bottom.Insert(_el, _padding);
            }

            // determine trimmed and padded sizes
            float elWidth = _el.rotatedWidth;
            float elHeight = _el.rotatedHeight;
            float paddedWidth = elWidth + _padding;
            float paddedHeight = elHeight + _padding;

            // trimmed element size must fit within current node rect
            if (elWidth > rect.width || elHeight > rect.height)
                return null;

            // create first child node in remaining space to the right, using elHeight
            // so that only other elements with the same height or less can be added there
            // (we do not use paddedHeight, because the padding area is reserved and should
            // never be occupied)
            right = new PackNode( new Rect ( rect.x + paddedWidth, 
                                             rect.y,
                                             rect.width - paddedWidth, 
                                             elHeight ) );

            // create second child node in remaining space at the bottom, occupying the entire width
            bottom = new PackNode( new Rect ( rect.x,
                                              rect.y + paddedHeight,
                                              rect.width, 
                                              rect.height - paddedHeight ) );

            // return position where to put element
            return new Vector2( rect.x, rect.y );
        }
    }

    // a > b = 1, a < b = -1, a = b = 0
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static int CompareByWidth ( Element _a, Element _b ) {
        int ret = (int)_a.width - (int)_b.width;
        if ( ret == 0 ) {
            ret = string.Compare( _a.id, _b.id );
        }
        return ret;
    }
    public static int CompareByHeight ( Element _a, Element _b ) {
        int ret = (int)_a.height - (int)_b.height;
        if ( ret == 0 ) {
            ret = string.Compare( _a.id, _b.id );
        }
        return ret;
    }
    public static int CompareByArea ( Element _a, Element _b ) {
        int ret = (int)_a.width * (int)_a.height - (int)_b.width * (int)_b.height;
        if ( ret == 0 ) {
            ret = string.Compare( _a.id, _b.id );
        }
        return ret;
    }
    public static int CompareByName ( Element _a, Element _b ) {
        int ret = string.Compare( _a.id, _b.id );
        return ret;
    }
    public static int CompareByRotateWidth ( Element _a, Element _b ) {
        int a_size = (int)_a.width;
        if ( (int)_a.height > (int)_a.width ) {
            a_size = (int)_a.height;
            _a.rotated = true;
        }
        int b_size = (int)_b.width;
        if ( (int)_b.height > (int)_b.width ) {
            b_size = (int)_b.height;
            _b.rotated = true;
        }
        int ret = a_size - b_size;
        if ( ret == 0 ) {
            ret = string.Compare( _a.id, _b.id );
        }
        return ret;
    }
    public static int CompareByRotateHeight ( Element _a, Element _b ) {
        int a_size = (int)_a.height;
        if ( (int)_a.width > (int)_a.height ) {
            a_size = (int)_a.width;
            _a.rotated = true;
        }
        int b_size = (int)_b.height;
        if ( (int)_b.width > (int)_b.height ) {
            b_size = (int)_b.width;
            _b.rotated = true;
        }
        int ret = a_size - b_size;
        if ( ret == 0 ) {
            ret = string.Compare( _a.id, _b.id );
        }
        return ret;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static List<Element> GetElementList ( exAtlas _atlas ) {
        List<Element> elements = new List<Element>();
        foreach ( exTextureInfo info in _atlas.textureInfos ) {
            Element el = new Element();
            el.x = 0;
            el.y = 0;
            el.rotated = false;
            el.textureInfo = info;
            el.id = info.name;
            el.width = info.width;
            el.height = info.height;
            elements.Add(el);
        }
        return elements;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void Sort ( List<Element> _elements, 
                              exAtlas.SortBy _sortBy, 
                              exAtlas.SortOrder _sortOrder, 
                              exAtlas.Algorithm _algorithm,
                              bool _allowRotate ) {
        //
        exAtlas.SortBy mySortBy = _sortBy;
        exAtlas.SortOrder mySortOrder = _sortOrder;
        if ( mySortBy == exAtlas.SortBy.UseBest ) {
            switch ( _algorithm ) {
            case exAtlas.Algorithm.Basic:
                mySortBy = exAtlas.SortBy.Height;
                break;
            case exAtlas.Algorithm.Tree:
                mySortBy = exAtlas.SortBy.Height;
                break;
            default:
                mySortBy = exAtlas.SortBy.Height;
                break;
            }
        }
        if ( mySortOrder == exAtlas.SortOrder.UseBest ) {
            mySortOrder = exAtlas.SortOrder.Descending;
        }

        // sort by
        switch ( mySortBy ) {
        case exAtlas.SortBy.Width:
            if ( _allowRotate )
                _elements.Sort( CompareByRotateWidth );
            else
                _elements.Sort( CompareByWidth );
            break;
        case exAtlas.SortBy.Height:
            if ( _allowRotate )
                _elements.Sort( CompareByRotateHeight );
            else
                _elements.Sort( CompareByHeight );
            break;
        case exAtlas.SortBy.Area:
            _elements.Sort( CompareByArea );
            break;
        case exAtlas.SortBy.Name:
            _elements.Sort( CompareByName );
            break;
        }

        // sort order
        if ( mySortOrder == exAtlas.SortOrder.Descending ) {
            _elements.Reverse();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void Pack ( List<Element> _elements, exAtlas.Algorithm _algorithm, int _atlasWidth, int _atlasHeight, int _padding ) {
        if ( _algorithm == exAtlas.Algorithm.Basic ) {
            BasicPack ( _elements, _atlasWidth, _atlasHeight, _padding );
        }
        else if ( _algorithm == exAtlas.Algorithm.Tree ) {
            TreePack ( _elements, _atlasWidth, _atlasHeight, _padding );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void TreePack ( List<Element> _elements, int _atlasWidth, int _atlasHeight, int _padding ) {
        PackNode root = new PackNode( new Rect( 0,
                                                0,
                                                _atlasWidth,
                                                _atlasHeight ) );
        foreach ( Element el in _elements ) {
            Vector2? pos = root.Insert (el, _padding);
            if (pos != null) {
                el.x = (int)pos.Value.x;
                el.y = (int)pos.Value.y;
            }
            else {
                // log warning but continue processing other elements
                Debug.LogWarning("Failed to layout atlas element " + el.id);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void BasicPack ( List<Element> _elements, int _atlasWidth, int _atlasHeight, int _padding ) {
        int curX = 0;
        int curY = 0;
        int maxY = 0; 
        int i = 0; 

        foreach ( Element el in _elements ) {
            if ( (curX + el.rotatedWidth) > _atlasWidth ) {
                curX = 0;
                curY = curY + maxY + _padding;
                maxY = 0;
            }
            if ( (curY + el.rotatedHeight) > _atlasHeight ) {
                Debug.LogWarning( "Failed to layout element " + el.id );
                break;
            }
            el.x = curX;
            el.y = curY;

            curX = curX + el.rotatedWidth + _padding;
            if (el.rotatedHeight > maxY) {
                maxY = el.rotatedHeight;
            }
            ++i;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ImportObjects ( exAtlas _atlas, Object[] _objects, System.Action<float,string> _progress ) {

        string atlasAssetsDir = Path.Combine( Path.GetDirectoryName (AssetDatabase.GetAssetPath(_atlas)), _atlas.name  );

        // check if create atlas directory
        _progress( 0.1f, "Checking atlas directory" );
        if ( new DirectoryInfo(atlasAssetsDir).Exists == false ) {
            Directory.CreateDirectory (atlasAssetsDir);
            AssetDatabase.ImportAsset (atlasAssetsDir);
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
        bool noToAll = false;
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
                exTextureInfo textureInfo = exGenericAssetUtility<exTextureInfo>.LoadExistsOrCreate( atlasAssetsDir, rawTexture.name );
                // DISABLE: AddObjectToAsset { 
                // exTextureInfo textureInfo = _atlas.GetTextureInfoByName (rawTexture.name);
                // if ( textureInfo == null ) {
                //     textureInfo = ScriptableObject.CreateInstance<exTextureInfo>();
                //     textureInfo.name = rawTexture.name;
                //     AssetDatabase.AddObjectToAsset( textureInfo, _atlas );
                //     AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath(textureInfo) );
                // }
                // } DISABLE end 

                int result = 1;
                if ( noToAll == false 
                  && string.IsNullOrEmpty(textureInfo.rawTextureGUID) == false 
                  && textureInfo.rawTextureGUID != exEditorUtility.AssetToGUID(rawTexture) ) 
                {
                    result = EditorUtility.DisplayDialogComplex( "Texture Info " + textureInfo.name + " already exists, it is bind to texture " + AssetDatabase.GUIDToAssetPath(textureInfo.rawTextureGUID),
                                                                 "Do you want to bind it with new texture " + AssetDatabase.GetAssetPath(rawTexture),
                                                                 "No",          // 0 
                                                                 "Yes",         // 1
                                                                 "No to all"    // 2
                                                                 );
                    if ( result == 2 )
                        noToAll = true;
                }

                if ( noToAll == false && result == 1 ) {
                    textureInfo.rawAtlasGUID = exEditorUtility.AssetToGUID(_atlas);
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
                    EditorUtility.SetDirty(textureInfo);
                }

                if ( _atlas.textureInfos.IndexOf(textureInfo) == -1 )
                    _atlas.textureInfos.Add(textureInfo);
            }
        }

        if ( _atlas.textureInfos.Count > 0 ) {
            _atlas.name = ".atlas";
            AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath(_atlas) );
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

    public static void Sync ( exAtlas _atlas, System.Action<float,string> _progress ) {
        string atlasAssetsDir = Path.Combine( Path.GetDirectoryName (AssetDatabase.GetAssetPath(_atlas)), _atlas.name  );

        // check if create atlas directory
        _progress( 0.1f, "Checking atlas directory" );
        if ( new DirectoryInfo(atlasAssetsDir).Exists == false ) {
            Directory.CreateDirectory (atlasAssetsDir);
            AssetDatabase.ImportAsset (atlasAssetsDir);
        }

        // texture info
        _progress( 0.4f, "Syncing texture infos" );
        AssetDatabase.StartAssetEditing();
        for ( int i = 0; i < _atlas.textureInfos.Count; ++i ) {
            exTextureInfo textureInfo = _atlas.textureInfos[i]; 
            string textureInfoPath = AssetDatabase.GetAssetPath(textureInfo);
            // string textureInfoDir = Path.GetDirectoryName (textureInfoPath);

            string rawTexturePath = AssetDatabase.GUIDToAssetPath(textureInfo.rawTextureGUID);
            string rawTextureName = Path.GetFileNameWithoutExtension(rawTexturePath);

            string expectPath = Path.Combine( atlasAssetsDir, rawTextureName + ".asset" );
            if ( textureInfoPath != expectPath ) {
                bool doMove = true;
                FileInfo fileInfo = new FileInfo(expectPath);
                if ( fileInfo.Exists ) {
                    doMove = EditorUtility.DisplayDialog( "Texture Info " + rawTextureName + " already exists",
                                                          "Do you want to replace it with new texture infor?",
                                                          "Yes",
                                                          "No" );
                }

                if ( doMove )
                    AssetDatabase.MoveAsset ( textureInfoPath, expectPath );
            }
        }
        AssetDatabase.StopAssetEditing();

        // atlas texture
        _progress( 0.8f, "Syncing atlas texture" );
        string atlasTexturePath = AssetDatabase.GetAssetPath(_atlas.texture);
        string expectAtlasTexturePath = Path.Combine( atlasAssetsDir, _atlas.name + ".png" );
        if ( _atlas.texture != null &&
             atlasTexturePath != expectAtlasTexturePath )
        {
            bool doMove = true;
            FileInfo fileInfo = new FileInfo(expectAtlasTexturePath);
            if ( fileInfo.Exists ) {
                doMove = EditorUtility.DisplayDialog( "Atlas Texture " + _atlas.name + " already exists",
                                                      "Do you want to replace it with new atlas texture?",
                                                      "Yes",
                                                      "No" );
            }

            if ( doMove ) {
                AssetDatabase.MoveAsset ( atlasTexturePath, expectAtlasTexturePath );
                AssetDatabase.ImportAsset ( expectAtlasTexturePath );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void Build ( exAtlas _atlas, System.Action<float,string> _progress ) {
        TextureImporter importSettings = null;

        string atlasAssetsDir = Path.Combine( Path.GetDirectoryName (AssetDatabase.GetAssetPath(_atlas)), _atlas.name  );
        string atlasTexturePath = Path.Combine( atlasAssetsDir, _atlas.name + ".png" );

        // check if create atlas directory
        _progress( 0.1f, "Checking atlas directory" );
        if ( new DirectoryInfo(atlasAssetsDir).Exists == false ) {
            Directory.CreateDirectory (atlasAssetsDir);
            AssetDatabase.ImportAsset (atlasAssetsDir);
        }

        // destroy last built texture
        Texture2D atlasTexture = _atlas.texture; 
        if ( atlasTexture ) {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(atlasTexture));
        }

        // check if create atlas texture
        _progress( 0.2f, "Checking atlas texture" );
        atlasTexture = AssetDatabase.LoadAssetAtPath( atlasTexturePath, typeof(Texture2D) ) as Texture2D;
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
        string rawAtlasGUID = exEditorUtility.AssetToGUID(_atlas);
        foreach ( exTextureInfo textureInfo in _atlas.textureInfos ) {
            Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D>(textureInfo.rawTextureGUID); 
            if ( exEditorUtility.IsValidForAtlas(rawTexture) == false ) {
                exEditorUtility.ImportTextureForAtlas(rawTexture);
            }

            bool dirty = false;
            if ( textureInfo.rawAtlasGUID != rawAtlasGUID ) {
                textureInfo.rawAtlasGUID = rawAtlasGUID;
                dirty = true;
            }
            if ( textureInfo.texture != atlasTexture ) {
                textureInfo.texture = atlasTexture;
                dirty = true;
            }
            if ( dirty )
                EditorUtility.SetDirty(textureInfo);

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

        EditorUtility.SetDirty(_atlas);
    }
}
