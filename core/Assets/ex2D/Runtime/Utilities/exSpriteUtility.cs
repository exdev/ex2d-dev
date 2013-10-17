// ======================================================================================
// File         : exSpriteUtility.cs
// Author       : 
// Last Change  : 09/03/2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

namespace ex2D.Detail {

///////////////////////////////////////////////////////////////////////////////
//
/// The sprite utilities
//
///////////////////////////////////////////////////////////////////////////////

public static class exSpriteUtility {

    public static exSprite NewSimpleSprite ( GameObject _go, exTextureInfo _info, 
                                             int _width, int _height, Color _color ) {
        exSprite sprite = _go.GetComponent<exSprite>();
        if ( sprite == null ) {
            sprite = _go.AddComponent<exSprite>();
        }
        if ( sprite.shader == null )
            sprite.shader = Shader.Find("ex2D/Alpha Blended");
        sprite.spriteType = exSpriteType.Simple;
        sprite.textureInfo = _info;

        sprite.customSize = true;
        sprite.width = _width;
        sprite.height = _height;

        sprite.color = _color;

        return sprite;
    }

    public static exSprite NewSlicedSprite ( GameObject _go, exTextureInfo _info, 
                                             int _left, int _right, int _top, int _bottom,
                                             int _width, int _height, Color _color, 
                                             bool _borderOnly ) {
        exSprite sprite = _go.GetComponent<exSprite>();
        if ( sprite == null ) {
            sprite = _go.AddComponent<exSprite>();
        }
        if ( sprite.shader == null )
            sprite.shader = Shader.Find("ex2D/Alpha Blended");
        sprite.spriteType = exSpriteType.Sliced;
        sprite.textureInfo = _info;

        sprite.borderOnly = _borderOnly;
        sprite.customBorderSize = true;
        sprite.leftBorderSize = _left;
        sprite.rightBorderSize = _right;
        sprite.topBorderSize = _top;
        sprite.bottomBorderSize = _bottom;

        sprite.customSize = true;
        sprite.width = _width;
        sprite.height = _height;

        sprite.color = _color;

        return sprite;
    }

    public static exSpriteFont NewSpriteFont ( GameObject _go, Font _font, int _fontSize, Color _color, string _text ) {
        exSpriteFont spriteFont = _go.GetComponent<exSpriteFont>();
        if ( spriteFont == null ) {
            spriteFont = _go.AddComponent<exSpriteFont>();
        }
        if ( spriteFont.shader == null )
            spriteFont.shader = Shader.Find("ex2D/Alpha Blended (Use Vertex Color)");
        spriteFont.SetFont (_font);
        spriteFont.fontSize = _fontSize;
        spriteFont.color = _color;
        spriteFont.text = _text;

        return spriteFont;
    }

    public static exSpriteFont NewSpriteFont ( GameObject _go, exBitmapFont _font, Color _color, string _text ) {
        exSpriteFont spriteFont = _go.GetComponent<exSpriteFont>();
        if ( spriteFont == null ) {
            spriteFont = _go.AddComponent<exSpriteFont>();
        }
        if ( spriteFont.shader == null )
            spriteFont.shader = Shader.Find("ex2D/Alpha Blended");
        spriteFont.SetFont (_font);
        spriteFont.color = _color;
        spriteFont.text = _text;

        return spriteFont;
    }

    public static void GetDicingCount (exTextureInfo _ti, out int _colCount, out int _rowCount) {
        _colCount = 1;
        _rowCount = 1;
        if (_ti != null) {
            if (_ti.diceUnitWidth > 0 && _ti.width > 0) {
                _colCount = Mathf.CeilToInt((float)_ti.width / _ti.diceUnitWidth);
            }
            if (_ti.diceUnitHeight > 0 && _ti.height > 0) {
                _rowCount = Mathf.CeilToInt((float)_ti.height / _ti.diceUnitHeight);
            }
        }
    }

    public static void GetTilingCount (exISprite _sprite, out int _colCount, out int _rowCount) {
        exTextureInfo ti = _sprite.textureInfo;
        if (ti != null && ti.width + _sprite.tiledSpacing.x != 0 && ti.height + _sprite.tiledSpacing.y != 0) {
            _colCount = Mathf.Max(Mathf.CeilToInt(_sprite.width / (ti.width + _sprite.tiledSpacing.x)), 1);
            _rowCount = Mathf.Max(Mathf.CeilToInt(_sprite.height / (ti.height + _sprite.tiledSpacing.y)), 1);
        }
        else {
            _colCount = 1;
            _rowCount = 1;
        }
    }
    
    public static void SetTextureInfo (exSpriteBase _sprite, ref exTextureInfo _ti, exTextureInfo _newTi, bool _useTextureOffset, exSpriteType _spriteType) {
        exTextureInfo old = _ti;
        _ti = _newTi;
        if (_newTi != null) {
            if (_newTi.texture == null) {
                Debug.LogWarning("invalid textureInfo");
            }
            if (_spriteType == exSpriteType.Tiled) {
                if (old == null || ReferenceEquals(old, _newTi) || _newTi.rawWidth != old.rawWidth || _newTi.rawHeight != old.rawHeight) {
                    (_sprite as exISprite).UpdateBufferSize ();
                    _sprite.updateFlags |= exUpdateFlags.Vertex;    // tile数量可能不变，但是间距可能会改变
                }
            }
            else if (_spriteType == exSpriteType.Diced) {
                //if (_newTi.isDiced == false) {
                //    Debug.LogWarning ("The texture info is not diced!");
                //}
                (_sprite as exISprite).UpdateBufferSize ();
                _sprite.updateFlags |= exUpdateFlags.Vertex;
            }
            else {
                if (_sprite.customSize == false && (_newTi.width != _sprite.width || _newTi.height != _sprite.height)) {
                    _sprite.width = _newTi.width;
                    _sprite.height = _newTi.height;
                    _sprite.updateFlags |= exUpdateFlags.Vertex;
                }
            }
            if (_useTextureOffset) {
                _sprite.updateFlags |= exUpdateFlags.Vertex;
            }
            _sprite.updateFlags |= exUpdateFlags.UV;  // 换了texture，UV也会重算，不换texture就更要改UV，否则没有换textureInfo的必要了。

            if (old == null || ReferenceEquals(old.texture, _newTi.texture) == false) {
                // texture changed
                _sprite.updateFlags |= (exUpdateFlags.Vertex | exUpdateFlags.UV);
                (_sprite as exISprite).UpdateMaterial();
            }
        }
    }
}

///////////////////////////////////////////////////////////////////////////////
//
/// The extension methods for exSprite and ex3DSprite
//
///////////////////////////////////////////////////////////////////////////////

public static partial class exISpriteExtends {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static Vector2 GetTextureOffset (this exISprite _sprite) {
        Vector2 anchorOffset = Vector2.zero;
        if (_sprite.useTextureOffset) {
            exTextureInfo textureInfo = _sprite.textureInfo;
            switch (_sprite.anchor) {
            case Anchor.TopLeft:
                anchorOffset.x = textureInfo.trim_x;
                anchorOffset.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height);
                break;
            case Anchor.TopCenter:
                anchorOffset.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width) * 0.5f;
                anchorOffset.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height);
                break;
            case Anchor.TopRight:
                anchorOffset.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width);
                anchorOffset.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height);
                break;
            //
            case Anchor.MidLeft:
                anchorOffset.x = textureInfo.trim_x;
                anchorOffset.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height) * 0.5f;
                break;
            case Anchor.MidCenter:
                anchorOffset.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width) * 0.5f;
                anchorOffset.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height) * 0.5f;
                break;
            case Anchor.MidRight:
                anchorOffset.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width);
                anchorOffset.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height) * 0.5f;
                break;
            //
            case Anchor.BotLeft:
                anchorOffset.x = textureInfo.trim_x;
                anchorOffset.y = textureInfo.trim_y;
                break;
            case Anchor.BotCenter:
                anchorOffset.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width) * 0.5f;
                anchorOffset.y = textureInfo.trim_y;
                break;
            case Anchor.BotRight:
                anchorOffset.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width);
                anchorOffset.y = textureInfo.trim_y;
                break;
            //
            default:
                anchorOffset.x = textureInfo.trim_x - (textureInfo.rawWidth - textureInfo.width) * 0.5f;
                anchorOffset.y = textureInfo.trim_y - (textureInfo.rawHeight - textureInfo.height) * 0.5f;
                break;
            }
            Vector2 customSizeScale = new Vector2(_sprite.width / _sprite.textureInfo.width, _sprite.height / _sprite.textureInfo.height);
            anchorOffset.x *= customSizeScale.x;
            anchorOffset.y *= customSizeScale.y;
        }
        return anchorOffset;
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    public static void GetVertexAndIndexCount (this exISprite _sprite, out int _vertexCount, out int _indexCount) {
        switch (_sprite.spriteType) {
        case exSpriteType.Simple:
            _vertexCount = exMesh.QUAD_VERTEX_COUNT;
            _indexCount = exMesh.QUAD_INDEX_COUNT;
            break;
        case exSpriteType.Sliced:
            _vertexCount = 4 * 4;
            _indexCount = exMesh.QUAD_INDEX_COUNT * 9;
            if (_sprite.borderOnly) {
                _indexCount -= exMesh.QUAD_INDEX_COUNT;
            }
            break;
        case exSpriteType.Tiled: {
            int colCount, rowCount;
            exSpriteUtility.GetTilingCount (_sprite, out colCount, out rowCount);
            int quadCount = colCount * rowCount;
            _vertexCount = quadCount * exMesh.QUAD_VERTEX_COUNT;
            const int maxVertex = exMesh.MAX_VERTEX_COUNT;
            //const int maxVertex = 40000;
            if (_vertexCount > maxVertex) {
                Debug.LogWarning(_sprite.gameObject.name + " is too big. Consider using a bigger texture.", _sprite.gameObject);
                int sqrCount = (int)Mathf.Sqrt(maxVertex / exMesh.QUAD_VERTEX_COUNT);
                if (colCount > sqrCount) {
                    _sprite.width = (_sprite.textureInfo.width + _sprite.tiledSpacing.x) * sqrCount;
                }
                if (rowCount > sqrCount) {
                    _sprite.height = (_sprite.textureInfo.height + _sprite.tiledSpacing.y) * sqrCount;
                }
                exSpriteUtility.GetTilingCount (_sprite, out colCount, out rowCount);
                quadCount = colCount * rowCount;
                _vertexCount = quadCount * exMesh.QUAD_VERTEX_COUNT;
                exDebug.Assert (_vertexCount <= maxVertex);
            }
            _indexCount = quadCount * exMesh.QUAD_INDEX_COUNT;
            break;
        }
        case exSpriteType.Diced: {
            exTextureInfo ti = _sprite.textureInfo;
            if (ti == null || ti.isDiced == false) {
                _vertexCount = exMesh.QUAD_VERTEX_COUNT;
                _indexCount = exMesh.QUAD_INDEX_COUNT;
                return;
            }
            int quadCount = 0;
            DiceEnumerator dice = _sprite.textureInfo.dices;
            while (dice.MoveNext()) {
                if (dice.Current.sizeType != exTextureInfo.DiceType.Empty) {
                    ++quadCount;
                }
            }
            if (quadCount == 0) {
                quadCount = 1;
            }
            //int colCount, rowCount;
            //exSpriteUtility.GetDicingCount (_sprite, out colCount, out rowCount);
            //exDebug.Assert (quadCount <= colCount * rowCount);
            _vertexCount = quadCount * exMesh.QUAD_VERTEX_COUNT;
            _indexCount = quadCount * exMesh.QUAD_INDEX_COUNT;
            if (_vertexCount > exMesh.MAX_VERTEX_COUNT) {
                Debug.LogError("The texture info [" + _sprite.textureInfo.name + "] has too many dices! Please using a bigger dice value.", _sprite.textureInfo);
                _vertexCount = exMesh.QUAD_VERTEX_COUNT;
                _indexCount = exMesh.QUAD_INDEX_COUNT;
            }
            break;
        }
        default:
            _vertexCount = exMesh.QUAD_VERTEX_COUNT;
            _indexCount = exMesh.QUAD_INDEX_COUNT;
            break;
        }
    }
}
}
