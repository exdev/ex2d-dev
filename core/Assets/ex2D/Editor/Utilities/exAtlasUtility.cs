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
    public class LayoutException : System.Exception {
        public string message = "";
        public LayoutException ( string _message ) {
            message = _message;
        }
    }

    public class Element {
        public string id = ""; // texture-info is its name, charinfo is its "bitmapfont.name@id"
        public int x = 0;
        public int y = 0;
        public int width = 1;
        public int height = 1;
        public bool rotated = false;
        public int dicedID = -1;

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
                if ( textureInfo.shouldDiced ) {
                    exTextureInfo.Dice dice = textureInfo.GetDiceData( dicedID );
                    dice.width = width;
                    dice.height = height;
                    dice.x = x;
                    dice.y = y;
                    dice.rotated = rotated;
                    textureInfo.SetDiceData( dicedID, dice );
                }
                else {
                    textureInfo.x = x;
                    textureInfo.y = y;
                    textureInfo.width = width;
                    textureInfo.height = height;
                    textureInfo.rotated = rotated;
                }

                EditorUtility.SetDirty(textureInfo);
            }
            else if ( charInfo != null ) {
                charInfo.x = x;
                charInfo.y = y;
                charInfo.width = width;
                charInfo.height = height;
                charInfo.rotated = rotated;
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
        public Vector2? Insert ( Element _el, int _padding, bool _allowRotate ) {
            // when this node is already occupied (when it has children),
            // forward to child nodes recursively
            if (right != null) {
                Vector2? pos = right.Insert(_el, _padding, _allowRotate);
                if (pos != null)
                    return pos;
                return bottom.Insert(_el, _padding, _allowRotate);
            }

            // determine trimmed and padded sizes
            float elWidth = _el.rotatedWidth;
            float elHeight = _el.rotatedHeight;
            float paddedWidth = elWidth + _padding;
            float paddedHeight = elHeight + _padding;

            // trimmed element size must fit within current node rect
            if ( elWidth > rect.width || elHeight > rect.height ) {

                if ( _allowRotate == false )
                    return null;

                if ( elHeight > rect.width || elWidth > rect.height ) {
                    return null;
                }
                else {
                    _el.rotated = !_el.rotated;
                    elWidth = _el.rotatedWidth;
                    elHeight = _el.rotatedHeight;
                    paddedWidth = elWidth + _padding;
                    paddedHeight = elHeight + _padding;
                }
            }

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
            if ( info.shouldDiced ) {
                int editorDiceCount = info.editorDiceXCount * info.editorDiceYCount;
                for ( int diceIndex = 0; diceIndex < editorDiceCount; ++diceIndex ) {
                    exTextureInfo.Dice dice = info.GetDiceData(diceIndex);

                    Element el = new Element();
                    el.x = 0;
                    el.y = 0;
                    el.rotated = false;
                    el.textureInfo = info;
                    el.id = info.name + "@" + diceIndex;
                    el.dicedID = diceIndex;
                    el.width = dice.width;
                    el.height = dice.height;
                    elements.Add(el);
                }
            }
            else {
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
        }
        foreach ( exBitmapFont bitmapFont in _atlas.bitmapFonts ) {
            foreach ( exBitmapFont.CharInfo info in bitmapFont.charInfos ) {
                Element el = new Element();
                el.x = 0;
                el.y = 0;
                el.rotated = false;
                el.charInfo = info;
                el.id = info.id + "@" + bitmapFont.name;
                el.width = info.width;
                el.height = info.height;
                elements.Add(el);
            }
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
                mySortBy = exAtlas.SortBy.Area;
                break;

            // TODO { 
            // case exAtlas.Algorithm.MaxRect:
            //     mySortBy = exAtlas.SortBy.Area;
            //     break;
            // } TODO end 

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

    public static void Pack ( List<Element> _elements, 
                              exAtlas.Algorithm _algorithm, 
                              int _atlasWidth, 
                              int _atlasHeight, 
                              int _padding,
                              bool _allowRotate ) {
        switch ( _algorithm ) {
        case exAtlas.Algorithm.Basic:
            BasicPack ( _elements, _atlasWidth, _atlasHeight, _padding, _allowRotate );
            break;

        case exAtlas.Algorithm.Tree:
            TreePack ( _elements, _atlasWidth, _atlasHeight, _padding, _allowRotate );
            break;

        case exAtlas.Algorithm.MaxRect:
            MaxRectPack ( _elements, _atlasWidth, _atlasHeight, _padding, _allowRotate );
            break;
        } 
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static Rect MaxRect_ScoreRect ( List<Rect> _freeRects, int _width, int _height, bool _allowRotate, ref int _score1, ref int _score2 ) {
        _score1 = int.MaxValue;
        _score2 = int.MaxValue;
        Rect newRect = new Rect( 0, 0, 1, 1 );
        bool found = false;

        //
        for ( int i = 0; i < _freeRects.Count; ++i ) {
            Rect freeRect = _freeRects[i];

            //
            if ( freeRect.width >= _width && freeRect.height >= _height ) {
                int leftoverHoriz = System.Math.Abs((int)_freeRects[i].width - _width);
                int leftoverVert = System.Math.Abs((int)_freeRects[i].height - _height);
                int shortSideFit = System.Math.Min(leftoverHoriz, leftoverVert);
                int longSideFit = System.Math.Max(leftoverHoriz, leftoverVert);

                if ( shortSideFit < _score1 || (shortSideFit == _score1 && longSideFit < _score2) ) {
                    newRect.x = _freeRects[i].x;
                    newRect.y = _freeRects[i].y;
                    newRect.width = _width;
                    newRect.height = _height;
                    _score1 = shortSideFit;
                    _score2 = longSideFit;

                    found = true;
                }
            }

            // rotated
            if ( _allowRotate && freeRect.width >= _height && freeRect.height >= _width ) {
                int leftoverHoriz = System.Math.Abs((int)_freeRects[i].width - _height);
                int leftoverVert = System.Math.Abs((int)_freeRects[i].height - _width);
                int shortSideFit = System.Math.Min(leftoverHoriz, leftoverVert);
                int longSideFit = System.Math.Max(leftoverHoriz, leftoverVert);

                if ( shortSideFit < _score1 || (shortSideFit == _score1 && longSideFit < _score2) ) {
                    newRect.x = _freeRects[i].x;
                    newRect.y = _freeRects[i].y;
                    newRect.width = _height;
                    newRect.height = _width;
                    _score1 = shortSideFit;
                    _score2 = longSideFit;

                    found = true;
                }
            }
        }

        //
        if ( found == false ) {
            _score1 = int.MaxValue;
            _score2 = int.MaxValue;
        }

        return newRect;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void MaxRect_CleanUpFreeRects ( List<Rect> _freeRects ) {
        for ( int i = 0; i < _freeRects.Count; ++i ) {
            for ( int j = i+1; j < _freeRects.Count; ++j ) {
                if ( exGeometryUtility.RectRect_Contains(_freeRects[i],_freeRects[j]) == -1 ) {
                    _freeRects.RemoveAt(i);
                    --i;
                    break;
                }
                if ( exGeometryUtility.RectRect_Contains(_freeRects[j],_freeRects[i]) == -1 ) {
                    _freeRects.RemoveAt(j);
                    --j;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void MaxRect_PlaceRect ( List<Rect> _freeRects, Rect _rect ) {
        for ( int i = 0; i < _freeRects.Count; ++i ) {
            if ( MaxRect_SplitFreeNode( _freeRects, _freeRects[i], _rect ) ) {
                _freeRects.RemoveAt(i);
                --i;
            }
        }

        MaxRect_CleanUpFreeRects(_freeRects);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static bool MaxRect_SplitFreeNode ( List<Rect> _freeRects, Rect _freeNode, Rect _usedNode ) {
        // Test with SAT if the rectangles even intersect.
        if ( _usedNode.x >= _freeNode.x + _freeNode.width || _usedNode.x + _usedNode.width <= _freeNode.x ||
             _usedNode.y >= _freeNode.y + _freeNode.height || _usedNode.y + _usedNode.height <= _freeNode.y )
            return false;

        if ( _usedNode.x < _freeNode.x + _freeNode.width && _usedNode.x + _usedNode.width > _freeNode.x ) {

            // New node at the top side of the used node.
            if ( _usedNode.y > _freeNode.y && _usedNode.y < _freeNode.y + _freeNode.height ) {
                Rect newNode = _freeNode;
                newNode.height = _usedNode.y - newNode.y;
                _freeRects.Add(newNode);
            }

            // New node at the bottom side of the used node.
            if ( _usedNode.y + _usedNode.height < _freeNode.y + _freeNode.height ) {
                Rect newNode = _freeNode;
                newNode.y = _usedNode.y + _usedNode.height;
                newNode.height = _freeNode.y + _freeNode.height - (_usedNode.y + _usedNode.height);
                _freeRects.Add(newNode);
            }
        }

        if ( _usedNode.y < _freeNode.y + _freeNode.height && _usedNode.y + _usedNode.height > _freeNode.y ) {

            // New node at the left side of the used node.
            if ( _usedNode.x > _freeNode.x && _usedNode.x < _freeNode.x + _freeNode.width ) {
                Rect newNode = _freeNode;
                newNode.width = _usedNode.x - newNode.x;
                _freeRects.Add(newNode);
            }

            // New node at the right side of the used node.
            if ( _usedNode.x + _usedNode.width < _freeNode.x + _freeNode.width ) {
                Rect newNode = _freeNode;
                newNode.x = _usedNode.x + _usedNode.width;
                newNode.width = _freeNode.x + _freeNode.width - (_usedNode.x + _usedNode.width);
                _freeRects.Add(newNode);
            }
        }

        return true;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void MaxRectPack ( List<Element> _elements, 
                                     int _atlasWidth, 
                                     int _atlasHeight, 
                                     int _padding,
                                     bool _allowRotate ) {
        List<Rect> freeRects = new List<Rect>();
        freeRects.Add ( new Rect( 0, 0, _atlasWidth + _padding, _atlasHeight + _padding ) );

        List<Element> processElements = _elements.GetRange(0,_elements.Count);
        while ( processElements.Count > 0 ) {
            int bestScore1 = int.MaxValue;
            int bestScore2 = int.MaxValue;
            int bestElementIdx = -1;
            Rect bestRect = new Rect( 0, 0, 1, 1 );

            for ( int i = 0; i < processElements.Count; ++i ) {
                int score1 = int.MaxValue;
                int score2 = int.MaxValue;
                Rect newRect = MaxRect_ScoreRect ( freeRects, 
                                                   processElements[i].width + _padding, 
                                                   processElements[i].height + _padding, 
                                                   _allowRotate,
                                                   ref score1, ref score2 );

                if ( score1 < bestScore1 || (score1 == bestScore1 && score2 < bestScore2) ) {
                    bestScore1 = score1;
                    bestScore2 = score2;
                    bestRect = newRect;
                    bestElementIdx = i;
                }
            }

            if ( bestElementIdx == -1 ) {
                throw new LayoutException( "Failed to layout atlas elements" );
            }
            MaxRect_PlaceRect( freeRects, bestRect );

            // apply the best-element
            Element bestElement = processElements[bestElementIdx];
            bestElement.x = (int)bestRect.x;
            bestElement.y = (int)bestRect.y;
            if ( bestElement.width + _padding != bestRect.width )
                bestElement.rotated = true;
            else
                bestElement.rotated = false;

            // remove the processed(inserted) element
            processElements.RemoveAt( bestElementIdx );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void TreePack ( List<Element> _elements, 
                                  int _atlasWidth, 
                                  int _atlasHeight, 
                                  int _padding,
                                  bool _allowRotate ) {
        PackNode root = new PackNode( new Rect( 0,
                                                0,
                                                _atlasWidth,
                                                _atlasHeight ) );
        foreach ( Element el in _elements ) {
            Vector2? pos = root.Insert (el, _padding, _allowRotate);
            if (pos != null) {
                el.x = (int)pos.Value.x;
                el.y = (int)pos.Value.y;
            }
            else {
                // log warning but continue processing other elements
                throw new LayoutException( "Failed to layout atlas element " + el.id );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void BasicPack ( List<Element> _elements, 
                                   int _atlasWidth, 
                                   int _atlasHeight, 
                                   int _padding,
                                   bool _allowRotate ) {
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
                throw new LayoutException( "Failed to layout element " + el.id );
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
            _progress( 0.2f + (float)i/(float)_objects.Length * 0.8f, "Add element " + o.name );

            if ( o is Texture2D ) {
                Texture2D rawTexture = o as Texture2D;

                // if the texture already in the atlas, warning and skip it.
                if ( exAtlasUtility.Exists(_atlas,rawTexture) ) {
                    Debug.LogWarning ( "The texture " + o.name + " already exists in the atlas" );
                    continue;
                }

                bool trimmed = false;
                Rect trimRect = new Rect ( 0, 0, rawTexture.width, rawTexture.height );
                if ( _atlas.trimElements ) {
                    Rect trimResult = exTextureUtility.GetTrimTextureRect( rawTexture, 
                                                                           _atlas.trimThreshold,
                                                                           new Rect( 0, 0, rawTexture.width, rawTexture.height )  );
                    if ( trimResult.width > 0 && trimResult.height > 0 ) {
                        trimmed = true;
                        trimRect = trimResult;
                    }
                    else {
                        Debug.LogWarning ( "Can't not trim texture " + o.name + ", empty pixel in it" );
                    }
                }

                //
                exTextureInfo textureInfo = exGenericAssetUtility<exTextureInfo>.LoadExistsOrCreate( atlasAssetsDir, rawTexture.name );

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
                    textureInfo.trim = trimmed;
                    textureInfo.trimThreshold = _atlas.trimThreshold;
                    EditorUtility.SetDirty(textureInfo);
                }

                textureInfo.ClearDiceData();

                // trim diced data
                if ( textureInfo.shouldDiced ) {
                    textureInfo.GenerateDiceData();
                    textureInfo.BeginDiceData();
                    int editorDiceCount = textureInfo.editorDiceXCount * textureInfo.editorDiceYCount;
                    for ( int diceIndex = 0; diceIndex < editorDiceCount; ++diceIndex ) {
                        exTextureInfo.Dice dice = textureInfo.GetDiceData(diceIndex);
                        Rect trimResult = exTextureUtility.GetTrimTextureRect( rawTexture, 
                                                                               textureInfo.trimThreshold, 
                                                                               new Rect( dice.trim_x, dice.trim_y, dice.width, dice.height ) );
                        //
                        dice.offset_x = (int)(trimResult.x - dice.trim_x);
                        dice.offset_y = (int)(trimResult.y - dice.trim_y);
                        dice.width = (int)trimResult.width;
                        dice.height = (int)trimResult.height;

                        textureInfo.SetDiceData( diceIndex, dice );
                    }
                    textureInfo.EndDiceData();
                }

                if ( _atlas.textureInfos.IndexOf(textureInfo) == -1 )
                    _atlas.textureInfos.Add(textureInfo);
            }
            else {
                Object rawFontInfo = o;
                if ( o is exBitmapFont ) {
                    exBitmapFont bitmapFont = o as exBitmapFont;
                    rawFontInfo = exEditorUtility.LoadAssetFromGUID<Object>( bitmapFont.rawFontGUID );
                    if ( rawFontInfo == null ) {
                        Debug.LogWarning ( "Can't not find raw font info from " + bitmapFont.name );
                    }
                }

                // start parsing font-info
                if ( rawFontInfo != null && exBitmapFontUtility.IsFontInfo(rawFontInfo) ) {

                    bool doReplace = true;
                    exBitmapFont bitmapFont = exGenericAssetUtility<exBitmapFont>.LoadExistsOrCreate( atlasAssetsDir, rawFontInfo.name );
                    if ( string.IsNullOrEmpty(bitmapFont.rawFontGUID) == false 
                      && bitmapFont.rawFontGUID != exEditorUtility.AssetToGUID(rawFontInfo) ) 
                    {
                        doReplace = EditorUtility.DisplayDialog( "BitmapFont " + bitmapFont.name + " already exists, it is bind to font-info " + AssetDatabase.GUIDToAssetPath(bitmapFont.rawFontGUID),
                                                                 "Do you want to bind it with new font-info " + AssetDatabase.GetAssetPath(rawFontInfo),
                                                                 "Yes",
                                                                 "No" );
                    }

                    if ( doReplace ) {
                        bitmapFont.rawAtlasGUID = exEditorUtility.AssetToGUID(_atlas);

                        if ( exBitmapFontUtility.Parse( bitmapFont, rawFontInfo ) ) {
                            bitmapFont.texture = _atlas.texture; // overwrite the raw texture
                            if ( _atlas.bitmapFonts.IndexOf(bitmapFont) == -1 )
                                _atlas.bitmapFonts.Add(bitmapFont);
                        }
                        else {
                            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bitmapFont));
                            Debug.LogError ( "Parse Error: " + rawFontInfo.name );
                        }
                    }
                }

            }
        }

        _atlas.needLayout = true;
        _atlas.needRebuild = true;
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
        string rawAtlasGUID = exEditorUtility.AssetToGUID(_atlas);

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
            // check textureInfo itself
            exTextureInfo textureInfo = _atlas.textureInfos[i]; 
            string textureInfoPath = AssetDatabase.GetAssetPath(textureInfo);
            if ( textureInfo == null ) {
                _atlas.textureInfos.RemoveAt(i);
                AssetDatabase.DeleteAsset ( textureInfoPath );
                --i;
                continue;
            }

            // check raw-texture
            string rawTexturePath = AssetDatabase.GUIDToAssetPath(textureInfo.rawTextureGUID);
            string rawTextureName = Path.GetFileNameWithoutExtension(rawTexturePath);
            Texture2D rawTexture = (Texture2D)AssetDatabase.LoadAssetAtPath( rawTexturePath, typeof(Texture2D) ); 
            if ( rawTexture == null ) {
                _atlas.textureInfos.RemoveAt(i);
                AssetDatabase.DeleteAsset ( textureInfoPath );
                --i;
                continue;
            }

            // check if texture info same with raw-texture
            string expectPath = Path.Combine( atlasAssetsDir, rawTextureName + ".asset" );
            expectPath = expectPath.Replace("\\", "/");
            if ( textureInfoPath != expectPath ) {
                bool doMove = true;
                FileInfo fileInfo = new FileInfo(expectPath);
                if ( fileInfo.Exists ) {
                    doMove = EditorUtility.DisplayDialog( "Texture Info " + rawTextureName + " already exists",
                                                          "Do you want to replace it with new texture infor?",
                                                          "Yes",
                                                          "No" );
                }

                if ( doMove ) {
                    AssetDatabase.DeleteAsset ( expectPath );
                    AssetDatabase.MoveAsset ( textureInfoPath, expectPath );
                }
            }

            //
            Rect trimRect = new Rect ( 0, 0, rawTexture.width, rawTexture.height );
            if ( textureInfo.trim ) {
                trimRect = exTextureUtility.GetTrimTextureRect( rawTexture,
                                                                textureInfo.trimThreshold,
                                                                new Rect(0, 0, rawTexture.width, rawTexture.height) );
            }
            textureInfo.rawAtlasGUID = exEditorUtility.AssetToGUID(_atlas);
            textureInfo.name = rawTexture.name;
            textureInfo.rawWidth = rawTexture.width;
            textureInfo.rawHeight = rawTexture.height;
            textureInfo.trim_x = (int)trimRect.x;
            textureInfo.trim_y = (int)trimRect.y;
            textureInfo.width = (int)trimRect.width;
            textureInfo.height = (int)trimRect.height;

            textureInfo.ClearDiceData();

            // trim diced data
            if ( textureInfo.shouldDiced ) {
                textureInfo.GenerateDiceData();
                textureInfo.BeginDiceData();
                int editorDiceCount = textureInfo.editorDiceXCount * textureInfo.editorDiceYCount;
                for ( int diceIndex = 0; diceIndex < editorDiceCount; ++diceIndex ) {
                    exTextureInfo.Dice dice = textureInfo.GetDiceData(diceIndex);
                    Rect trimResult = exTextureUtility.GetTrimTextureRect( rawTexture, 
                                                                           textureInfo.trimThreshold, 
                                                                           new Rect( dice.trim_x, dice.trim_y, dice.width, dice.height ) );
                    //
                    dice.offset_x = (int)(trimResult.x - dice.trim_x);
                    dice.offset_y = (int)(trimResult.y - dice.trim_y);
                    dice.width = (int)trimResult.width;
                    dice.height = (int)trimResult.height;

                    textureInfo.SetDiceData( diceIndex, dice );
                }
                textureInfo.EndDiceData();
            }
        }

        // bitmapfont
        _progress( 0.6f, "Syncing bitmap fonts" );
        for ( int i = 0; i < _atlas.bitmapFonts.Count; ++i ) {
            // check bitmapfont
            exBitmapFont bitmapFont = _atlas.bitmapFonts[i]; 
            string bitmapFontPath = AssetDatabase.GetAssetPath(bitmapFont);
            if ( bitmapFont == null ) {
                _atlas.bitmapFonts.RemoveAt(i);
                AssetDatabase.DeleteAsset ( bitmapFontPath );
                --i;
                continue;
            }

            // check raw-fontinfo
            string rawFontPath = AssetDatabase.GUIDToAssetPath(bitmapFont.rawFontGUID);
            string rawFontName = Path.GetFileNameWithoutExtension(rawFontPath);
            FileInfo rawFontFileInfo = new FileInfo(rawFontPath);
            if ( rawFontFileInfo.Exists == false ) {
                _atlas.textureInfos.RemoveAt(i);
                AssetDatabase.DeleteAsset ( bitmapFontPath );
                --i;
                continue;
            }

            // check if bitmapfont same with raw-fontinfo
            string expectPath = Path.Combine( atlasAssetsDir, rawFontName + ".asset" );
            expectPath = expectPath.Replace("\\", "/");
            if ( bitmapFontPath != expectPath ) {
                bool doMove = true;
                FileInfo fileInfo = new FileInfo(expectPath);
                if ( fileInfo.Exists ) {
                    doMove = EditorUtility.DisplayDialog( "Texture Info " + rawFontName + " already exists",
                                                          "Do you want to replace it with new texture infor?",
                                                          "Yes",
                                                          "No" );
                }

                if ( doMove ) {
                    AssetDatabase.DeleteAsset ( expectPath );
                    AssetDatabase.MoveAsset ( bitmapFontPath, expectPath );
                }
            }

            bitmapFont.rawAtlasGUID = rawAtlasGUID;
        }
        AssetDatabase.StopAssetEditing();

        // atlas texture
        _progress( 0.8f, "Syncing atlas texture" );
        string atlasTexturePath = AssetDatabase.GetAssetPath(_atlas.texture);
        string expectAtlasTexturePath = Path.Combine( atlasAssetsDir, _atlas.name + ".png" );
        expectAtlasTexturePath = expectAtlasTexturePath.Replace("\\", "/");
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

        string rawAtlasGUID = exEditorUtility.AssetToGUID(_atlas);
        Color32[] pixels = new Color32[atlasTexture.width * atlasTexture.height];

        // fill raw texture-info to atlas
        _progress( 0.4f, "Filling texture-info to atlas" );
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

            // copy raw texture into atlas texture
            if ( textureInfo.isDiced ) {
                foreach (exTextureInfo.Dice dice in textureInfo.dices) {
                    if (dice.sizeType != exTextureInfo.DiceType.Empty) {
                        exTextureUtility.Fill( ref pixels
                                               , atlasTexture.width
                                               , rawTexture
                                               , textureInfo.name + "[" + dice.offset_x + "]" + "[" + dice.offset_y + "]"
                                               , dice.x
                                               , dice.y
                                               , dice.trim_x
                                               , dice.trim_y
                                               , dice.width
                                               , dice.height
                                               , dice.rotated
                                             );
                    }
                }
            }
            else {
                exTextureUtility.Fill( ref pixels
                                       , atlasTexture.width
                                       , rawTexture
                                       , textureInfo.name
                                       , textureInfo.x
                                       , textureInfo.y
                                       , textureInfo.trim_x
                                       , textureInfo.trim_y
                                       , textureInfo.width
                                       , textureInfo.height
                                       , textureInfo.rotated
                                     );
            }
        }

        // fill raw bitmapfont to atlas
        _progress( 0.6f, "Filling bitmap-font to atlas" );
        foreach ( exBitmapFont bitmapFont in _atlas.bitmapFonts ) {
            Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D>(bitmapFont.rawTextureGUID); 
            if ( exEditorUtility.IsValidForAtlas(rawTexture) == false ) {
                exEditorUtility.ImportTextureForAtlas(rawTexture);
            }

            bool dirty = false;
            if ( bitmapFont.rawAtlasGUID != rawAtlasGUID ) {
                bitmapFont.rawAtlasGUID = rawAtlasGUID;
                dirty = true;
            }
            if ( bitmapFont.texture != atlasTexture ) {
                bitmapFont.texture = atlasTexture;
                dirty = true;
            }
            if ( dirty )
                EditorUtility.SetDirty(bitmapFont);
                foreach ( exBitmapFont.CharInfo charInfo in bitmapFont.charInfos ) {

                // copy raw texture into atlas texture
                exTextureUtility.Fill( ref pixels
                                       , atlasTexture.width
                                       , rawTexture
                                       , bitmapFont.name + "@" + charInfo.id
                                       , charInfo.x
                                       , charInfo.y
                                       , charInfo.trim_x
                                       , charInfo.trim_y
                                       , charInfo.width
                                       , charInfo.height
                                       , charInfo.rotated
                                     );

                // DISABLE: character don't need padding bleed { 
                // // apply padding bleed
                // if ( _atlas.usePaddingBleed ) {
                //     exTextureUtility.ApplyPaddingBleed( atlasTexture,
                //                                         new Rect( charInfo.x, charInfo.y, charInfo.width, charInfo.height ) );
                // }
                // } DISABLE end 
            }
        }
        atlasTexture.SetPixels32(pixels);

        //
        _progress( 0.8f, "Bleed the texture" );
        ApplyBleed ( _atlas, atlasTexture );

        // write new atlas texture to disk
        _progress( 1.0f, "Importing atlas texture" );
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
        AssetDatabase.SaveAssets();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void ApplyBleed ( exAtlas _atlas, Texture2D _atlasTexture ) {
        //
        if ( _atlas.useContourBleed ) {
            Color32[] srcPixels = _atlasTexture.GetPixels32(0);
            Color32[] result = new Color32[srcPixels.Length];
            for ( int i = 0; i < srcPixels.Length; ++i ) {
                result[i] = srcPixels[i];
            }

            foreach ( exTextureInfo textureInfo in _atlas.textureInfos ) {
                // copy raw texture into atlas texture
                if ( textureInfo.isDiced ) {
                    foreach (exTextureInfo.Dice dice in textureInfo.dices) {
                        if (dice.sizeType != exTextureInfo.DiceType.Empty) {
                            exTextureUtility.ApplyContourBleed ( ref result, 
                                                                 srcPixels,
                                                                 _atlasTexture.width,
                                                                 new Rect( dice.x, dice.y, dice.rotatedWidth, dice.rotatedHeight ) );
                        }
                    }
                }
                else {
                    exTextureUtility.ApplyContourBleed ( ref result, 
                                                         srcPixels, 
                                                         _atlasTexture.width,
                                                         new Rect( textureInfo.x, textureInfo.y, textureInfo.rotatedWidth, textureInfo.rotatedHeight ) );
                }
            }

            foreach ( exBitmapFont bitmapFont in _atlas.bitmapFonts ) {
                foreach ( exBitmapFont.CharInfo charInfo in bitmapFont.charInfos ) {
                    exTextureUtility.ApplyContourBleed( ref result, 
                                                        srcPixels, 
                                                        _atlasTexture.width,
                                                        new Rect( charInfo.x, charInfo.y, charInfo.rotatedWidth, charInfo.rotatedHeight ) ); 
                }
            }

            _atlasTexture.SetPixels32(result);
        }

        //
        if ( _atlas.usePaddingBleed ) {
            Color32[] srcPixels = _atlasTexture.GetPixels32(0);
            Color32[] result = new Color32[srcPixels.Length];
            for ( int i = 0; i < srcPixels.Length; ++i ) {
                result[i] = srcPixels[i];
            }

            foreach ( exTextureInfo textureInfo in _atlas.textureInfos ) {
                // copy raw texture into atlas texture
                if ( textureInfo.isDiced ) {
                    foreach (exTextureInfo.Dice dice in textureInfo.dices) {
                        if (dice.sizeType != exTextureInfo.DiceType.Empty) {
                            exTextureUtility.ApplyPaddingBleed ( ref result, 
                                                                 srcPixels,
                                                                 _atlasTexture.width,
                                                                 _atlasTexture.height,
                                                                 new Rect( dice.x, dice.y, dice.rotatedWidth, dice.rotatedHeight ) );
                        }
                    }
                }
                else {
                    exTextureUtility.ApplyPaddingBleed ( ref result, 
                                                         srcPixels, 
                                                         _atlasTexture.width,
                                                         _atlasTexture.height,
                                                         new Rect( textureInfo.x, textureInfo.y, textureInfo.rotatedWidth, textureInfo.rotatedHeight ) );
                }
            }

            foreach ( exBitmapFont bitmapFont in _atlas.bitmapFonts ) {
                foreach ( exBitmapFont.CharInfo charInfo in bitmapFont.charInfos ) {
                    exTextureUtility.ApplyPaddingBleed( ref result, 
                                                        srcPixels, 
                                                        _atlasTexture.width,
                                                        _atlasTexture.height,
                                                        new Rect( charInfo.x, charInfo.y, charInfo.rotatedWidth, charInfo.rotatedHeight ) ); 
                }
            }

            _atlasTexture.SetPixels32(result);
        }
    }
}
