// ======================================================================================
// File         : exISprite.cs
// Author       : 
// Last Change  : 09/03/2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
//
//
//
///////////////////////////////////////////////////////////////////////////////

public interface exISpriteBase {
    bool customSize { get; set; }
    float width { get; set; }
    float height { get; set; }
    Anchor anchor { get; set; }
    Color color { get; set; }
    Vector2 offset { get; set; }
    Vector2 shear { get; set; }
    Shader shader { get; set; }
    int vertexCount { get; }
    int indexCount { get; }
    Material material { get; }
    bool visible { get; }
}

///////////////////////////////////////////////////////////////////////////////
//
// Interface for exSprite and ex3DSprite
//
///////////////////////////////////////////////////////////////////////////////

public interface exISprite : exISpriteBase {
    exTextureInfo textureInfo { get; set; }
    bool useTextureOffset { get; set; }
    exSpriteType spriteType { get; set; }
    Vector2 tilling { get; set; }
}

///////////////////////////////////////////////////////////////////////////////
///
/// The extension methods for exSprite and ex3DSprite
///
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
}