// ======================================================================================
// File         : exSprite.cs
// Author       : 
// Last Change  : 06/15/2013 | 09:49:04 AM | Saturday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ex2D.Detail;

// ------------------------------------------------------------------ 
/// The type of sprite
// ------------------------------------------------------------------ 

public enum exSpriteType {
    Simple = 0,
    Sliced,
    Tiled,
    //Diced,
}

///////////////////////////////////////////////////////////////////////////////
///
/// A component to render sprite in the layer
///
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/2D Sprite")]
public class exSprite : exLayeredSprite, exISprite {

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    [SerializeField] private exTextureInfo textureInfo_ = null;
    /// The texture info used in this sprite. If it's null, sprite will become invisible.
    // ------------------------------------------------------------------ 

    public exTextureInfo textureInfo {
        get { return textureInfo_; }
        set {
            // 如果用户在运行时改变了textureInfo，则这里需要重新赋值
            exTextureInfo old = textureInfo_;
            textureInfo_ = value;
            if (value != null) {
                if (value.texture == null) {
                    Debug.LogWarning("invalid textureInfo");
                }
                if (spriteType_ != exSpriteType.Tiled) {
                    if (customSize_ == false && (value.width != width_ || value.height != height_)) {
                        width_ = value.width;
                        height_ = value.height;
                        updateFlags |= exUpdateFlags.Vertex;
                    }
                }
                else {
                    if (old == null || value.width != old.width || value.height != old.height) {
                        EnsureBufferSize ();
                        updateFlags |= exUpdateFlags.Vertex;
                    }
                }
                if (useTextureOffset_) {
                    updateFlags |= exUpdateFlags.Vertex;
                }
                updateFlags |= exUpdateFlags.UV;  // 换了texture，UV也会重算，不换texture就更要改UV，否则没有换textureInfo的必要了。

                if (old == null || ReferenceEquals(old.texture, value.texture) == false) {
                    // texture changed
                    updateFlags |= (exUpdateFlags.Vertex | exUpdateFlags.UV);
                    UpdateMaterial();
                }
                if (isOnEnabled_) {
                    Show();
                }
            }
            else if (isOnEnabled_ && old != null) {
                Hide();
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected bool useTextureOffset_ = false;
    /// if useTextureOffset is true, the sprite calculate the anchor 
    /// position depends on the original size of texture instead of the trimmed size 
    // ------------------------------------------------------------------ 

    public bool useTextureOffset {
        get { return useTextureOffset_; }
        set {
            if ( useTextureOffset_ != value ) {
                useTextureOffset_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected exSpriteType spriteType_ = exSpriteType.Simple;
    // ------------------------------------------------------------------ 

    public exSpriteType spriteType {
        get { return spriteType_; }
        set {
            if ( spriteType_ != value ) {
                spriteType_ = value;
                if (layer_ != null) {
                    EnsureBufferSize ();
                    updateFlags |= exUpdateFlags.All;
                }
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    protected override Texture texture {
        get {
            if (textureInfo_ != null) {
                return textureInfo_.texture;
            }
            else {
                return null;
            }
        }
    }

    public override bool customSize {
        get { return spriteType_ == exSpriteType.Tiled || customSize_; }
        set {
            if (spriteType_ == exSpriteType.Tiled) {
                customSize_ = true;
            }
            else if (customSize_ != value) {
                customSize_ = value;
                if (customSize_ == false && textureInfo_ != null) {
                    if (textureInfo_.width != width_ || textureInfo_.height != height_) {
                        width_ = textureInfo_.width;
                        height_ = textureInfo_.height;
                        updateFlags |= exUpdateFlags.Vertex;
                    }
                }
            }
        }
    }

    public override float width {
        get {
            if (customSize_ == false) {
                return textureInfo_ != null ? textureInfo_.width : 0;
            }
            else {
                return width_;
            }
        }
        set {
            base.width = value;
            if (spriteType_ == exSpriteType.Tiled && layer_ != null) {
                EnsureBufferSize ();
                updateFlags |= exUpdateFlags.All;
            }
        }
    }

    public override float height {
        get {
            if (customSize_ == false) {
                return textureInfo_ != null ? textureInfo_.height : 0;
            }
            else {
                return height_;
            }
        }
        set {
            base.height = value;
            if (spriteType_ == exSpriteType.Tiled && layer_ != null) {
                EnsureBufferSize ();
                updateFlags |= exUpdateFlags.All;
            }
        }
    }
    
    public override bool visible {
        get {
            return isOnEnabled_ && textureInfo_ != null;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////

    // TODO: check border change if sliced

#region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices) {
        if (updateFlags == exUpdateFlags.None) {
            return exUpdateFlags.None;
        }
        if (textureInfo_ != null) {
            switch (spriteType_) {
            case exSpriteType.Simple:
                SpriteBuilder.SimpleUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.World, 
                                                   _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);
                break;
            case exSpriteType.Sliced:
                SpriteBuilder.SlicedUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.World, 
                                                   _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);
                break;
            case exSpriteType.Tiled:
                SpriteBuilder.TiledUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.World, 
                                                  _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);
                break;
            //case exSpriteType.Diced:
            //    break;
            }
            if ((updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null) {
                exDebug.Assert (layer_ != null);
                Color32 color32;
                if (transparent_ == false) {
                    color32 = new Color (color_.r, color_.g, color_.b, color_.a * layer_.alpha);
                } else {
                    color32 = new Color32 ();
                }
                for (int i = 0; i < vertexCount_; ++i) {
                    _colors32.buffer [vertexBufferIndex + i] = color32;
                }
            }
            //if (transparent_ == false) {
            exUpdateFlags applyedFlags = updateFlags;
            updateFlags = exUpdateFlags.None;
            return applyedFlags;
            //}
            //else {
            //    exUpdateFlags applyedFlags = (updateFlags & exUpdateFlags.Color);
            //    updateFlags &= ~exUpdateFlags.Color;
            //    return applyedFlags;
            //}
        }
        else {
            if (_indices != null) {
                _vertices.buffer[vertexBufferIndex] = cachedTransform.position;
                for (int i = indexBufferIndex; i < indexBufferIndex + indexCount_; ++i) {
                    _indices.buffer[i] = vertexBufferIndex;
                }
                return exUpdateFlags.All;   // TODO: remove from layer if no material
            }
            else {
                Vector3 pos = cachedTransform.position;
                for (int i = vertexBufferIndex; i < vertexBufferIndex + vertexCount_; ++i) {
                    _vertices.buffer[i] = pos;
                }
                return exUpdateFlags.All;   // TODO: remove from layer if no material
            }
        }
    }
    
#endregion // Functions used to update geometry buffer
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (Space _space) {
        if (textureInfo_ == null) {
            return new Vector3[0];
        }

        exList<Vector3> vertices = exList<Vector3>.GetTempList();
        UpdateVertexAndIndexCount();
        vertices.AddRange(vertexCount_);
        
        switch (spriteType_) {
        case exSpriteType.Simple:
            SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, vertices, 0, _space);
            break;
        case exSpriteType.Sliced:
            SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, vertices, 0, _space);
            SpriteBuilder.SimpleVertexBufferToSliced(this, textureInfo_, vertices, 0);
            break;
        //case exSpriteType.Tiled:
        //    break;
        //case exSpriteType.Diced:
        //    break;
        }

        return vertices.ToArray();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    protected override void OnPreAddToLayer () {
        exDebug.Assert(layer_ == null);
        UpdateVertexAndIndexCount();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void UpdateVertexAndIndexCount () {
        if (layer_ == null) {
            this.GetVertexAndIndexCount(out vertexCount_, out indexCount_);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void EnsureBufferSize () {
        int newVertexCount, newIndexCount;
        this.GetVertexAndIndexCount (out newVertexCount, out newIndexCount);
        if (vertexCount_ != newVertexCount || indexCount_ != newIndexCount) {
            // rebuild geometry
            exLayer myLayer = layer_;
            myLayer.Remove (this, false);
            myLayer.Add (this, false);
            exDebug.Assert (vertexCount_ == newVertexCount && indexCount_ == newIndexCount);
        }
    }
}

namespace ex2D.Detail {

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite geometry helper
///
///////////////////////////////////////////////////////////////////////////////

internal static class SpriteBuilder {
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public static void SimpleUpdateBuffers (exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space,
                                                exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices, int _vbIndex, int _ibIndex) {
        if (/*transparent_ == false && */(_sprite.updateFlags & exUpdateFlags.Vertex) != 0) {
            SpriteBuilder.SimpleUpdateVertexBuffer(_sprite, _textureInfo, _useTextureOffset, _vertices, _vbIndex, _space);
        }
        if (/*transparent_ == false && */(_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
            _indices.buffer[_ibIndex]     = _vbIndex;
            _indices.buffer[_ibIndex + 1] = _vbIndex + 1;
            _indices.buffer[_ibIndex + 2] = _vbIndex + 2;
            _indices.buffer[_ibIndex + 3] = _vbIndex + 2;
            _indices.buffer[_ibIndex + 4] = _vbIndex + 3;
            _indices.buffer[_ibIndex + 5] = _vbIndex;
        }
        if (/*transparent_ == false && */(_sprite.updateFlags & exUpdateFlags.UV) != 0) {
            Vector2 texelSize;
            if (_textureInfo.texture != null) {
                texelSize = _textureInfo.texture.texelSize;
            }
            else {
                texelSize = new Vector2(1.0f / _textureInfo.rawWidth, 1.0f / _textureInfo.rawHeight);
            }
            Vector2 start = new Vector2((float)_textureInfo.x * texelSize.x, 
                                         (float)_textureInfo.y * texelSize.y);
            Vector2 end = new Vector2((float)(_textureInfo.x + _textureInfo.rotatedWidth) * texelSize.x, 
                                       (float)(_textureInfo.y + _textureInfo.rotatedHeight) * texelSize.y);
            if ( _textureInfo.rotated ) {
                _uvs.buffer[_vbIndex + 0] = new Vector2(end.x, start.y);
                _uvs.buffer[_vbIndex + 1] = start;
                _uvs.buffer[_vbIndex + 2] = new Vector2(start.x, end.y);
                _uvs.buffer[_vbIndex + 3] = end;
            }
            else {
                _uvs.buffer[_vbIndex + 0] = start;
                _uvs.buffer[_vbIndex + 1] = new Vector2(start.x, end.y);
                _uvs.buffer[_vbIndex + 2] = end;
                _uvs.buffer[_vbIndex + 3] = new Vector2(end.x, start.y);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void SimpleUpdateVertexBuffer (exSpriteBase _sprite, exTextureInfo textureInfo_, bool useTextureOffset_, exList<Vector3> _vertices, int _startIndex, Space _space) {
        Vector2 anchorOffset;
        float halfHeight = textureInfo_.height * 0.5f;
        float halfWidth = textureInfo_.width * 0.5f;

        if (useTextureOffset_) {
            switch (_sprite.anchor) {
            case Anchor.TopLeft:
                anchorOffset.x = halfWidth + textureInfo_.trim_x;
                anchorOffset.y = -halfHeight + textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height);
                break;
            case Anchor.TopCenter:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = -halfHeight + textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height);
                break;
            case Anchor.TopRight:
                anchorOffset.x = -halfWidth + textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width);
                anchorOffset.y = -halfHeight + textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height);
                break;
            //
            case Anchor.MidLeft:
                anchorOffset.x = halfWidth + textureInfo_.trim_x;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            case Anchor.MidCenter:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            case Anchor.MidRight:
                anchorOffset.x = -halfWidth + textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width);
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            //
            case Anchor.BotLeft:
                anchorOffset.x = halfWidth + textureInfo_.trim_x;
                anchorOffset.y = halfHeight + textureInfo_.trim_y;
                break;
            case Anchor.BotCenter:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = halfHeight + textureInfo_.trim_y;
                break;
            case Anchor.BotRight:
                anchorOffset.x = -halfWidth + textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width);
                anchorOffset.y = halfHeight + textureInfo_.trim_y;
                break;
            //
            default:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            }
        }
        else {
            switch ( _sprite.anchor ) {
            case Anchor.TopLeft   : anchorOffset.x = halfWidth;   anchorOffset.y = -halfHeight;  break;
            case Anchor.TopCenter : anchorOffset.x = 0.0f;        anchorOffset.y = -halfHeight;  break;
            case Anchor.TopRight  : anchorOffset.x = -halfWidth;  anchorOffset.y = -halfHeight;  break;

            case Anchor.MidLeft   : anchorOffset.x = halfWidth;   anchorOffset.y = 0.0f;         break;
            case Anchor.MidCenter : anchorOffset.x = 0.0f;        anchorOffset.y = 0.0f;         break;
            case Anchor.MidRight  : anchorOffset.x = -halfWidth;  anchorOffset.y = 0.0f;         break;

            case Anchor.BotLeft   : anchorOffset.x = halfWidth;   anchorOffset.y = halfHeight;   break;
            case Anchor.BotCenter : anchorOffset.x = 0.0f;        anchorOffset.y = halfHeight;   break;
            case Anchor.BotRight  : anchorOffset.x = -halfWidth;  anchorOffset.y = halfHeight;   break;

            default               : anchorOffset.x = 0.0f;        anchorOffset.y = 0.0f;         break;
            }
        }

        //v1 v2
        //v0 v3
        Vector3 v0 = new Vector3 (-halfWidth + anchorOffset.x, -halfHeight + anchorOffset.y, 0.0f);
        Vector3 v1 = new Vector3 (-halfWidth + anchorOffset.x, halfHeight + anchorOffset.y, 0.0f);
        Vector3 v2 = new Vector3 (halfWidth + anchorOffset.x, halfHeight + anchorOffset.y, 0.0f);
        Vector3 v3 = new Vector3 (halfWidth + anchorOffset.x, -halfHeight + anchorOffset.y, 0.0f);
        if (_sprite.customSize) {
            Vector2 customSizeScale = new Vector2 (_sprite.width / textureInfo_.width, _sprite.height / textureInfo_.height);
            v0.x *= customSizeScale.x;  v0.y *= customSizeScale.y;
            v1.x *= customSizeScale.x;  v1.y *= customSizeScale.y;
            v2.x *= customSizeScale.x;  v2.y *= customSizeScale.y;
            v3.x *= customSizeScale.x;  v3.y *= customSizeScale.y;
        }

        Vector3 offset = _sprite.offset;
        v0 += offset; v1 += offset; v2 += offset; v3 += offset;

        Vector2 shear = _sprite.shear;
        if (shear.x != 0) {
            float offsetX = _sprite.GetScaleY(_space) * shear.x;
            float topOffset = offsetX * (halfHeight + anchorOffset.y);
            float botOffset = offsetX * (-halfHeight + anchorOffset.y);
            v0.x += botOffset;
            v1.x += topOffset;
            v2.x += topOffset;
            v3.x += botOffset;
        }
        if (shear.y != 0) {
            float offsetY = _sprite.GetScaleX(_space) * shear.y;
            float leftOffset = offsetY * (-halfWidth + anchorOffset.x);
            float rightOffset = offsetY * (halfWidth + anchorOffset.x);
            v0.y += leftOffset;
            v1.y += leftOffset;
            v2.y += rightOffset;
            v3.y += rightOffset;
        }

        if (_space == Space.World) {
            exDebug.Assert((_sprite as exLayeredSprite) != null);
            v0 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v0);
            v1 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v1);
            v2 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v2);
            v3 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v3);
            // 将z都设为0，使mesh所有mesh的厚度都为0，这样在mesh进行深度排序时会方便一些。但是不能用于3D Sprite
            v0.z = 0;
            v1.z = 0;
            v2.z = 0;
            v3.z = 0;
        }

        _vertices.buffer[_startIndex + 0] = v0;
        _vertices.buffer[_startIndex + 1] = v1;
        _vertices.buffer[_startIndex + 2] = v2;
        _vertices.buffer[_startIndex + 3] = v3;

        // TODO: pixel-perfect
    }
        
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public static void SlicedUpdateBuffers (exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space,
                                             exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices, int _vbIndex, int _ibIndex) {
        SpriteBuilder.SimpleUpdateBuffers(_sprite, _textureInfo, _useTextureOffset, _space, 
                                          _vertices, _uvs, _indices, _vbIndex, _ibIndex);
        if ((_sprite.updateFlags & exUpdateFlags.Vertex) != 0) {
            SpriteBuilder.SimpleVertexBufferToSliced(_sprite, _textureInfo, _vertices, _vbIndex);
        }
        if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
            for (int i = 0; i <= 10; ++i) {
                if (i != 3 && i != 7) {     // 0 1 2 4 5 6 8 9 10
                    int blVertexIndex = _vbIndex + i;   // bottom left vertex index
                    _indices.buffer[_ibIndex++] = blVertexIndex;
                    _indices.buffer[_ibIndex++] = blVertexIndex + 4;
                    _indices.buffer[_ibIndex++] = blVertexIndex + 5;
                    _indices.buffer[_ibIndex++] = blVertexIndex + 5;
                    _indices.buffer[_ibIndex++] = blVertexIndex + 1;
                    _indices.buffer[_ibIndex++] = blVertexIndex;
                }
            }
        }
        if ((_sprite.updateFlags & exUpdateFlags.UV) != 0) {
            float xStep1, xStep2, yStep1, yStep2;
            if (_textureInfo.rotated == false) {
                yStep1 = (float)_textureInfo.borderBottom / _textureInfo.height;  // uv step, not position step
                yStep2 = (float)(_textureInfo.height - _textureInfo.borderTop) / _textureInfo.height;
                xStep1 = (float)_textureInfo.borderLeft / _textureInfo.width;
                xStep2 = (float)(_textureInfo.width - _textureInfo.borderRight) / _textureInfo.width;
            }
            else {
                xStep1 = (float)_textureInfo.borderBottom / _textureInfo.height;  // uv step, not position step
                xStep2 = (float)(_textureInfo.height - _textureInfo.borderTop) / _textureInfo.height;
                yStep1 = (float)_textureInfo.borderLeft / _textureInfo.width;
                yStep2 = (float)(_textureInfo.width - _textureInfo.borderRight) / _textureInfo.width;
            }
            Vector2 uv0, uv15;
            uv0 = _uvs.buffer[_vbIndex + 0];
            uv15 = _uvs.buffer[_vbIndex + 2];
            Vector2 uv5 = new Vector2(uv0.x + (uv15.x - uv0.x) * xStep1, uv0.y + (uv15.y - uv0.y) * yStep1);
            Vector2 uv10 = new Vector2(uv0.x + (uv15.x - uv0.x) * xStep2, uv0.y + (uv15.y - uv0.y) * yStep2);

            if (_textureInfo.rotated == false) {
                //_uvs.buffer[vertexBufferIndex + 0] = uv0;
                _uvs.buffer[_vbIndex + 1] = new Vector2(uv5.x, uv0.y);
                _uvs.buffer[_vbIndex + 2] = new Vector2(uv10.x, uv0.y);
                _uvs.buffer[_vbIndex + 3] = new Vector2(uv15.x, uv0.y);

                _uvs.buffer[_vbIndex + 4] = new Vector2(uv0.x, uv5.y);
                _uvs.buffer[_vbIndex + 5] = uv5;
                _uvs.buffer[_vbIndex + 6] = new Vector2(uv10.x, uv5.y);
                _uvs.buffer[_vbIndex + 7] = new Vector2(uv15.x, uv5.y);

                _uvs.buffer[_vbIndex + 8] = new Vector2(uv0.x, uv10.y);
                _uvs.buffer[_vbIndex + 9] = new Vector2(uv5.x, uv10.y);
                _uvs.buffer[_vbIndex + 10] = uv10;
                _uvs.buffer[_vbIndex + 11] = new Vector2(uv15.x, uv10.y);

                _uvs.buffer[_vbIndex + 12] = new Vector2(uv0.x, uv15.y);
                _uvs.buffer[_vbIndex + 13] = new Vector2(uv5.x, uv15.y);
                _uvs.buffer[_vbIndex + 14] = new Vector2(uv10.x, uv15.y);
                _uvs.buffer[_vbIndex + 15] = uv15;
            }
            else {
                //_uvs.buffer[vertexBufferIndex + 0] = uv0;
                _uvs.buffer[_vbIndex + 1] = new Vector2(uv0.x, uv5.y);
                _uvs.buffer[_vbIndex + 2] = new Vector2(uv0.x, uv10.y);
                _uvs.buffer[_vbIndex + 3] = new Vector2(uv0.x, uv15.y);

                _uvs.buffer[_vbIndex + 4] = new Vector2(uv5.x, uv0.y);
                _uvs.buffer[_vbIndex + 5] = uv5;
                _uvs.buffer[_vbIndex + 6] = new Vector2(uv5.x, uv10.y);
                _uvs.buffer[_vbIndex + 7] = new Vector2(uv5.x, uv15.y);

                _uvs.buffer[_vbIndex + 8] = new Vector2(uv10.x, uv0.y);
                _uvs.buffer[_vbIndex + 9] = new Vector2(uv10.x, uv5.y);
                _uvs.buffer[_vbIndex + 10] = uv10;
                _uvs.buffer[_vbIndex + 11] = new Vector2(uv10.x, uv15.y);

                _uvs.buffer[_vbIndex + 12] = new Vector2(uv15.x, uv0.y);
                _uvs.buffer[_vbIndex + 13] = new Vector2(uv15.x, uv5.y);
                _uvs.buffer[_vbIndex + 14] = new Vector2(uv15.x, uv10.y);
                _uvs.buffer[_vbIndex + 15] = uv15;
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // Change vertex buffer from simple to sliced
    // ------------------------------------------------------------------ 

    public static void SimpleVertexBufferToSliced (exSpriteBase _sprite, exTextureInfo textureInfo_, exList<Vector3> _vertices, int _startIndex) {
        /* vertex index:
        12 13 14 15
        8  9  10 11
        4  5  6  7 
        0  1  2  3 
        */
        // left right columns
        Vector3 v0 = _vertices.buffer[_startIndex + 0];
        Vector3 v12 = _vertices.buffer[_startIndex + 1];
        Vector3 v15 = _vertices.buffer[_startIndex + 2];
        Vector3 v3 = _vertices.buffer[_startIndex + 3];
        //_vertices.buffer[_startIndex + 0] = v0;
        //_vertices.buffer[_startIndex + 3] = v3;
        _vertices.buffer[_startIndex + 12] = v12;
        _vertices.buffer[_startIndex + 15] = v15;
        float height = _sprite.height;
        float yStep1 = (float)textureInfo_.borderBottom / height;        // position step, not uv step
        float yStep2 = (height - textureInfo_.borderTop) / height;
        _vertices.buffer[_startIndex + 4] = v0 + (v12 - v0) * yStep1;
        _vertices.buffer[_startIndex + 7] = v3 + (v15 - v3) * yStep1;
        _vertices.buffer[_startIndex + 8] = v0 + (v12 - v0) * yStep2;
        _vertices.buffer[_startIndex + 11] = v3 + (v15 - v3) * yStep2;
        // mid columns
        float width = _sprite.width;
        float xStep1 = (float)textureInfo_.borderLeft / width;
        float xStep2 = (width - textureInfo_.borderRight) / width;
        for (int i = 0; i <= 12; i += 4) {
            Vector3 left = _vertices.buffer[_startIndex + i];
            Vector3 right = _vertices.buffer[_startIndex + i + 3];
            _vertices.buffer[_startIndex + i + 1] = left + (right - left) * xStep1;
            _vertices.buffer[_startIndex + i + 2] = left + (right - left) * xStep2;
        }
    }
        
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public static void TiledUpdateBuffers (exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space, 
                                           exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices, int _vbIndex, int _ibIndex) {
        
        SpriteBuilder.SimpleUpdateBuffers(_sprite, _textureInfo, _useTextureOffset, _space, 
                                          _vertices, _uvs, _indices, _vbIndex, _ibIndex);

        if ((_sprite.updateFlags & exUpdateFlags.Vertex) != 0) {
            SimpleVertexBufferToTiled(_sprite, _textureInfo, _vertices, _vbIndex);
        }
        
        int colCount, rowCount;
        exSpriteUtility.GetTilingCount ((exISprite)_sprite, out colCount, out rowCount);

        if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
            int v = _vbIndex;
            int i = _ibIndex;
            int quadCount = colCount * rowCount;
            for (int q = 0; q < quadCount; ++q) {
                _indices.buffer[i++] = v;
                _indices.buffer[i++] = v + 1;
                _indices.buffer[i++] = v + 2;
                _indices.buffer[i++] = v + 2;
                _indices.buffer[i++] = v + 3;
                _indices.buffer[i++] = v;
                v += exMesh.QUAD_VERTEX_COUNT;
            }
        }

        if ((_sprite.updateFlags & exUpdateFlags.UV) != 0) {

        }
    }

    // ------------------------------------------------------------------ 
    // Change vertex buffer from simple to tiled
    // ------------------------------------------------------------------ 

    public static void SimpleVertexBufferToTiled (exSpriteBase _sprite, exTextureInfo textureInfo_, exList<Vector3> _vertices, int _startIndex) {
        /* tile index:
        8  9  10 11
        4  5  6  7 
        0  1  2  3 
        */
        /*Vector3 v0 = _vertices.buffer[_startIndex + 0];
        Vector3 v12 = _vertices.buffer[_startIndex + 1];
        Vector3 v15 = _vertices.buffer[_startIndex + 2];
        Vector3 v3 = _vertices.buffer[_startIndex + 3];*/
        return;
    }
}
}
