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

///////////////////////////////////////////////////////////////////////////////
//
//
//
///////////////////////////////////////////////////////////////////////////////

public static class exSpriteUtility {

    public static void GetTilingCount (exISprite _sprite, out int _colCount, out int _rowCount) {
        exTextureInfo ti = _sprite.textureInfo;
        float rawTiledWidth = _sprite.width + (ti.rawWidth - ti.width);
        _colCount = (int)Mathf.Ceil(Mathf.Abs(rawTiledWidth) / ti.rawWidth);
        float rawTiledHeight = _sprite.height + (ti.rawHeight - ti.height);
        _rowCount = (int)Mathf.Ceil(Mathf.Abs(rawTiledHeight / ti.rawHeight));
    }
}

namespace ex2D.Detail {

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
        // 假定不论textureInfo如何，都不改变index, vertex数量
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
            if (_vertexCount > exMesh.MAX_VERTEX_COUNT) {
                Debug.LogWarning(_sprite.gameObject.name + " is too big. Consider using a bigger texture.");
                int sqrCount = (int)Mathf.Sqrt(exMesh.MAX_VERTEX_COUNT);
                if (colCount > sqrCount) {
                    _sprite.width = _sprite.textureInfo.width * sqrCount;
                }
                if (rowCount > sqrCount) {
                    _sprite.height = _sprite.textureInfo.height * sqrCount;
                }
                exSpriteUtility.GetTilingCount (_sprite, out colCount, out rowCount);
                quadCount = colCount * rowCount;
                _vertexCount = quadCount * exMesh.QUAD_VERTEX_COUNT;
                exDebug.Assert (_vertexCount <= exMesh.MAX_VERTEX_COUNT);
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