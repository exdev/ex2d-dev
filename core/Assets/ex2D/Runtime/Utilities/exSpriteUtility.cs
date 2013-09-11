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

    public static void GetTilingCount (exISprite _sprite, out int _colCount, out int _rowCount) {
        exTextureInfo ti = _sprite.textureInfo;
        if (ti != null && ti.width + _sprite.tiledSpacing.x != 0 && ti.height + _sprite.tiledSpacing.y != 0) {
            _colCount = Mathf.Max((int)Mathf.Ceil(_sprite.width / (ti.width + _sprite.tiledSpacing.x)), 1);
            _rowCount = Mathf.Max((int)Mathf.Ceil(_sprite.height / (ti.height + _sprite.tiledSpacing.y)), 1);
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
                if (_newTi.diceUnitX == 0 && _newTi.diceUnitY == 0) {
                    Debug.LogWarning ("The texture info does not diced!");
                }
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
            break;
        case exSpriteType.Tiled:
            int colCount, rowCount;
            exSpriteUtility.GetTilingCount (_sprite, out colCount, out rowCount);
            int quadCount = colCount * rowCount;
            _vertexCount = quadCount * exMesh.QUAD_VERTEX_COUNT;
            const int maxVertex = exMesh.MAX_VERTEX_COUNT;
            //const int maxVertex = 40000;
            if (_vertexCount > maxVertex) {
                Debug.LogWarning(_sprite.gameObject.name + " is too big. Consider using a bigger texture.");
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
        //case exSpriteType.Diced:
            //    break;
        default:
            _vertexCount = exMesh.QUAD_VERTEX_COUNT;
            _indexCount = exMesh.QUAD_INDEX_COUNT;
            break;
        }
    }
}
}