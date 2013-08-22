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

// ------------------------------------------------------------------ 
/// The type of sprite
// ------------------------------------------------------------------ 

public enum exSpriteType {
    Simple = 0,
    Sliced,
    Tiled,
    Diced,
}

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite component
///
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/ex2D Sprite")]
public class exSprite : exSpriteBase {

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// The texture info used in this sprite. If it's null, sprite will become invisible.
    // ------------------------------------------------------------------ 

    [SerializeField]
    private exTextureInfo textureInfo_ = null;
    public exTextureInfo textureInfo {
        get { return textureInfo_; }
        set {
            if (ReferenceEquals(textureInfo_, value)) {
                return;
            }
            if (value != null) {
                if (value.texture == null) {
                    Debug.LogWarning("invalid textureInfo");
                }
                if (customSize_ == false && (value.width != width_ || value.height != height_)) {
                    width_ = value.width;
                    height_ = value.height;
                    updateFlags |= exUpdateFlags.Vertex;
                }
                else if (useTextureOffset_) {
                    updateFlags |= exUpdateFlags.Vertex;
                }
                updateFlags |= exUpdateFlags.UV;  // 换了texture，UV也会重算，不换texture就更要改UV，否则没有换textureInfo的必要了。

                if (textureInfo_ == null || ReferenceEquals(textureInfo_.texture, value.texture) == false) {
                    // texture changed
                    textureInfo_ = value;
                    UpdateMaterial();
                    return;
                }
                else if (textureInfo_ == null && isOnEnabled_ && layer_ != null) {
                    // become visible
                    if (enableFastShowHide) {
                        layer_.FastShowSprite(this);
                    }
                    else {
                        layer_.ShowSprite(this);
                    }
                }
            }
            else if (textureInfo_ != null && isOnEnabled_ && layer_ != null) {
                textureInfo_ = value;
                // become invisible
                if (enableFastShowHide) {
                    layer_.FastHideSprite(this);
                }
                else {
                    layer_.HideSprite(this);
                }
            }
            textureInfo_ = value;

#if UNITY_EDITOR
            if (layer_ != null) {
                layer_.UpdateNowInEditMode();
            }
#endif
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
                    int newVertexCount, newIndexCount;
                    GetVertexAndIndexCount(value, out newVertexCount, out newIndexCount);
                    if (currentVertexCount != newVertexCount || currentIndexCount != newIndexCount) {
                        // rebuild geometry
                        exLayer myLayer = layer_;
                        myLayer.Remove(this);
                        myLayer.Add(this);
                        exDebug.Assert(currentVertexCount == newVertexCount && currentIndexCount == newIndexCount);
                    }
                }
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] private int currentVertexCount = -1;
    [System.NonSerialized] private int currentIndexCount = -1;

    public override int vertexCount {
        get {
            return currentVertexCount;
        }
    }

    public override int indexCount {
        get {
            return currentIndexCount;
        }
    }

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
        get { return customSize_; }
        set {
            if (customSize_ != value) {
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

    #region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices) {
        switch (spriteType_) {
            case exSpriteType.Simple:
                SimpleUpdateBuffers (_vertices, _uvs, _indices);
                break;
            case exSpriteType.Sliced:
                SlicedUpdateBuffers (_vertices, _uvs, _indices);
                break;
            case exSpriteType.Tiled:
                break;
            case exSpriteType.Diced:
                break;
        }
        if ((updateFlags & exUpdateFlags.Color) != 0) {
            exDebug.Assert(layer_ != null);
            Color32 color32;
            if (transparent_ == false) {
                color32 = new Color(color_.r, color_.g, color_.b, color_.a * layer_.alpha);
            }
            else {
                color32 = new Color32 ();
            }
            for (int i = 0; i < currentVertexCount; ++i) {
                _colors32.buffer[vertexBufferIndex + i] = color32;
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
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void SimpleUpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices) {
        if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Vertex) != 0) {
            exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
            UpdateVertexBuffer(_vertices, vertexBufferIndex, ref cachedWorldMatrix);
        }
        if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
            _indices.buffer[indexBufferIndex]     = vertexBufferIndex;
            _indices.buffer[indexBufferIndex + 1] = vertexBufferIndex + 1;
            _indices.buffer[indexBufferIndex + 2] = vertexBufferIndex + 2;
            _indices.buffer[indexBufferIndex + 3] = vertexBufferIndex + 2;
            _indices.buffer[indexBufferIndex + 4] = vertexBufferIndex + 3;
            _indices.buffer[indexBufferIndex + 5] = vertexBufferIndex;
            TestIndices(_indices);
        }
        if (/*transparent_ == false && */(updateFlags & exUpdateFlags.UV) != 0 && textureInfo_ != null) {
            Vector2 texelSize;
            if (textureInfo_.texture != null) {
                texelSize = textureInfo_.texture.texelSize;
            }
            else {
                texelSize = new Vector2(1.0f / textureInfo_.rawWidth, 1.0f / textureInfo_.rawHeight);
            }
            Vector2 start = new Vector2((float)textureInfo_.x * texelSize.x, 
                                         (float)textureInfo_.y * texelSize.y);
            Vector2 end = new Vector2((float)(textureInfo_.x + textureInfo_.rotatedWidth) * texelSize.x, 
                                       (float)(textureInfo_.y + textureInfo_.rotatedHeight) * texelSize.y);
            if ( textureInfo_.rotated ) {
                _uvs.buffer[vertexBufferIndex + 0] = new Vector2(end.x, start.y);
                _uvs.buffer[vertexBufferIndex + 1] = start;
                _uvs.buffer[vertexBufferIndex + 2] = new Vector2(start.x, end.y);
                _uvs.buffer[vertexBufferIndex + 3] = end;
            }
            else {
                _uvs.buffer[vertexBufferIndex + 0] = start;
                _uvs.buffer[vertexBufferIndex + 1] = new Vector2(start.x, end.y);
                _uvs.buffer[vertexBufferIndex + 2] = end;
                _uvs.buffer[vertexBufferIndex + 3] = new Vector2(end.x, start.y);
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void SlicedUpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices) {
        //SimpleUpdateBuffers (_vertices, _uvs, _indices);
        //if (textureInfo_ == null || (textureInfo_.borderLeft == 0 && textureInfo_.borderRight == 0 && textureInfo_.borderTop == 0 && textureInfo_.borderBottom == 0)) {
        //    for (int i = 4; i < indexCount; --i) {
        //        _indices.buffer[indexBufferIndex + i] = vertexBufferIndex;  // hide unused triangle
        //    }
        //    return;
        //}
        //if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Vertex) != 0) {
        //    /* vertex index:
        //        12 13 14 15
        //        8  9  10 11
        //        4  5  6  7 
        //        0  1  2  3 
        //     */
        //    // left right columns
        //    Vector3 v0 = _vertices.buffer[vertexBufferIndex + 0];
        //    Vector3 v12 = _vertices.buffer[vertexBufferIndex + 1];
        //    Vector3 v15 = _vertices.buffer[vertexBufferIndex + 2];
        //    Vector3 v3 = _vertices.buffer[vertexBufferIndex + 3];
        //    float trimmedBorderTop = textureInfo_.borderTop - textureInfo_.trim_y;
        //    float trimmedBorderBottom = textureInfo_.borderBottom - (textureInfo_.rawHeight - textureInfo_.trim_y - textureInfo_.height);
        //    float yStep1 = trimmedBorderTop / height_;
        //    float yStep2 = (height_ - trimmedBorderBottom) / height_;
        //    _vertices.buffer[vertexBufferIndex + 4] = v0 + (v12 - v0) * yStep1;
        //    _vertices.buffer[vertexBufferIndex + 7] = v3 + (v15 - v3) * yStep1;
        //    _vertices.buffer[vertexBufferIndex + 8] = v0 + (v12 - v0) * yStep2;
        //    _vertices.buffer[vertexBufferIndex + 11] = v3 + (v15 - v3) * yStep2;
        //    // mid columns
        //    float trimmedBorderLeft = textureInfo_.borderLeft - textureInfo_.trim_x;
        //    float trimmedBorderRight = textureInfo_.borderRight - (textureInfo_.rawWidth - textureInfo_.trim_x - textureInfo_.width);
        //    float xStep1 = trimmedBorderLeft / width_;
        //    float xStep2 = (width_ - trimmedBorderRight) / width_;
        //    for (int i = 0; i <= 12; i += 4) {
        //        Vector3 left = _vertices.buffer[vertexBufferIndex + i];
        //        Vector3 right = _vertices.buffer[vertexBufferIndex + i + 3];
        //        _vertices.buffer[vertexBufferIndex + i + 1] = left + (right - left) * xStep1;
        //        _vertices.buffer[vertexBufferIndex + i + 2] = left + (right - left) * xStep2;
        //    }
        //}
        //if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
        //    int index = -1;
        //    for (int i = 0; i <= 10; ++i) {
        //        if (i != 3 || i != 7) {
        //            // 0 1 2 4 5 6 8 9 10
        //            _indices.buffer[++index] = i;
        //            _indices.buffer[++index] = i + 4;
        //            _indices.buffer[++index] = i + 5;
        //            _indices.buffer[++index] = i + 5;
        //            _indices.buffer[++index] = i + 1;
        //            _indices.buffer[++index] = i;
        //        }
        //    }
        //}
        //if (/*transparent_ == false && */(updateFlags & exUpdateFlags.UV) != 0 && textureInfo_ != null) {
        //    Vector2 uvbl = _uvs.buffer[vertexBufferIndex + 0];
        //    Vector2 uvtr = _uvs.buffer[vertexBufferIndex + 2];
        //    Vector2 start = new Vector2((float)textureInfo_.x * texelSize.x, 
        //                                (float)textureInfo_.y * texelSize.y);
        //    Vector2 end = new Vector2((float)(textureInfo_.x + textureInfo_.rotatedWidth) * texelSize.x, 
        //                              (float)(textureInfo_.y + textureInfo_.rotatedHeight) * texelSize.y);
        //    if ( textureInfo_.rotated ) {
        //        _uvs.buffer[vertexBufferIndex + 0] = new Vector2(end.x, start.y);
        //        _uvs.buffer[vertexBufferIndex + 1] = start;
        //        _uvs.buffer[vertexBufferIndex + 2] = new Vector2(start.x, end.y);
        //        _uvs.buffer[vertexBufferIndex + 3] = end;
        //    }
        //    else {
        //        _uvs.buffer[vertexBufferIndex + 0] = start;
        //        _uvs.buffer[vertexBufferIndex + 1] = new Vector2(start.x, end.y);
        //        _uvs.buffer[vertexBufferIndex + 2] = end;
        //        _uvs.buffer[vertexBufferIndex + 3] = new Vector2(end.x, start.y);
        //    }
        //}
    }

    #endregion // Functions used to update geometry buffer
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (ref Matrix4x4 _spriteMatrix) {
        exList<Vector3> vertices = exList<Vector3>.GetTempList();
        vertices.AddRange(vertexCount);
        UpdateVertexBuffer(vertices, 0, ref _spriteMatrix);
        return vertices.ToArray();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    protected override void OnPreAddToLayer () {
        exDebug.Assert(layer_ == null);
        if (layer_ == null) {
            GetVertexAndIndexCount(spriteType_, out currentVertexCount, out currentIndexCount);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    [System.Diagnostics.Conditional("EX_DEBUG")]
    void TestIndices (exList<int> _indices) {
#if UNITY_EDITOR
        // check indice is valid
        for (int i = indexBufferIndex; i < indexBufferIndex + indexCount; ++i) {
            if (_indices.buffer [i] < vertexBufferIndex || _indices.buffer [i] > vertexBufferIndex + vertexCount) {
                Debug.LogError ("[exLayer] Wrong triangle index!");
            }
        }
#endif
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void UpdateVertexBuffer (exList<Vector3> _vertices, int _startIndex, ref Matrix4x4 _spriteMatrix) {
        float anchorOffsetX;
        float anchorOffsetY;
        float halfHeight;
        float halfWidth;
        if (customSize_ == false) {
            if (textureInfo_ != null) {
                halfHeight = textureInfo_.height * 0.5f;
                halfWidth = textureInfo_.width * 0.5f;
            }
            else {
                halfHeight = 0;
                halfWidth = 0;
            }
        }
        else {
            halfHeight = height_ * 0.5f;
            halfWidth = width_ * 0.5f;
        }

        exDebug.Assert(halfWidth == width * 0.5f && halfHeight == height * 0.5f);

        if (useTextureOffset_) {
            switch (anchor_) {
            //
            case Anchor.TopLeft:
                anchorOffsetX = halfWidth + textureInfo_.trim_x;
                anchorOffsetY = halfHeight + textureInfo_.trim_y - textureInfo_.rawHeight;
                break;
            case Anchor.TopCenter:
                anchorOffsetX = halfWidth + textureInfo_.trim_x - textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = halfHeight + textureInfo_.trim_y - textureInfo_.rawHeight;
                break;
            case Anchor.TopRight:
                anchorOffsetX = halfWidth + textureInfo_.trim_x - textureInfo_.rawWidth;;
                anchorOffsetY = halfHeight + textureInfo_.trim_y - textureInfo_.rawHeight;
                break;
            //
            case Anchor.MidLeft:
                anchorOffsetX = halfWidth + textureInfo_.trim_x;
                anchorOffsetY = halfHeight + textureInfo_.trim_y - textureInfo_.rawHeight * 0.5f;
                break;
            case Anchor.MidCenter:
                anchorOffsetX = halfWidth + textureInfo_.trim_x - textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = halfHeight + textureInfo_.trim_y - textureInfo_.rawHeight * 0.5f;
                break;
            case Anchor.MidRight:
                anchorOffsetX = halfWidth + textureInfo_.trim_x - textureInfo_.rawWidth;;
                anchorOffsetY = halfHeight + textureInfo_.trim_y - textureInfo_.rawHeight * 0.5f;
                break;
            //
            case Anchor.BotLeft:
                anchorOffsetX = halfWidth + textureInfo_.trim_x;
                anchorOffsetY = halfHeight + textureInfo_.trim_y;
                break;
            case Anchor.BotCenter:
                anchorOffsetX = halfWidth + textureInfo_.trim_x - textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = halfHeight + textureInfo_.trim_y;
                break;
            case Anchor.BotRight:
                anchorOffsetX = halfWidth + textureInfo_.trim_x - textureInfo_.rawWidth;
                anchorOffsetY = halfHeight + textureInfo_.trim_y;
                break;
            default:
                anchorOffsetX = halfWidth + textureInfo_.trim_x - textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = halfHeight + textureInfo_.trim_y - textureInfo_.rawHeight * 0.5f;
                break;
            }
        }
        else {
            switch ( anchor_ ) {
            case Anchor.TopLeft     : anchorOffsetX = halfWidth;   anchorOffsetY = -halfHeight;  break;
            case Anchor.TopCenter   : anchorOffsetX = 0.0f;        anchorOffsetY = -halfHeight;  break;
            case Anchor.TopRight    : anchorOffsetX = -halfWidth;  anchorOffsetY = -halfHeight;  break;

            case Anchor.MidLeft     : anchorOffsetX = halfWidth;   anchorOffsetY = 0.0f;         break;
            case Anchor.MidCenter   : anchorOffsetX = 0.0f;        anchorOffsetY = 0.0f;         break;
            case Anchor.MidRight    : anchorOffsetX = -halfWidth;  anchorOffsetY = 0.0f;         break;

            case Anchor.BotLeft     : anchorOffsetX = halfWidth;   anchorOffsetY = halfHeight;   break;
            case Anchor.BotCenter   : anchorOffsetX = 0.0f;        anchorOffsetY = halfHeight;   break;
            case Anchor.BotRight    : anchorOffsetX = -halfWidth;  anchorOffsetY = halfHeight;   break;

            default                 : anchorOffsetX = 0.0f;        anchorOffsetY = 0.0f;         break;
            }
        }

        anchorOffsetX += offset_.x;
        anchorOffsetY += offset_.y;

        //v1 v2
        //v0 v3
        Vector3 v0 = _spriteMatrix.MultiplyPoint3x4(new Vector3(-halfWidth + anchorOffsetX, -halfHeight + anchorOffsetY, 0.0f));
        Vector3 v1 = _spriteMatrix.MultiplyPoint3x4(new Vector3(-halfWidth + anchorOffsetX, halfHeight + anchorOffsetY, 0.0f));
        Vector3 v2 = _spriteMatrix.MultiplyPoint3x4(new Vector3(halfWidth + anchorOffsetX, halfHeight + anchorOffsetY, 0.0f));
        Vector3 v3 = _spriteMatrix.MultiplyPoint3x4(new Vector3(halfWidth + anchorOffsetX, -halfHeight + anchorOffsetY, 0.0f));

        // 将z都设为0，使mesh所有mesh的厚度都为0，这样在mesh进行深度排序时会方便一些。但是不能用于3D Sprite
        v0.z = 0;
        v1.z = 0;
        v2.z = 0;
        v3.z = 0;

        if (shear_.x != 0) {
            // 这里直接从matrix拿未计入rotation影响的scale，在已知matrix的情况下，速度比较快lossyScale了6倍。
            // 在有rotation时，shear本来就会有冲突，所以这里不需要lossyScale。
            float worldScaleY = (new Vector3(_spriteMatrix.m01, _spriteMatrix.m11, _spriteMatrix.m21)).magnitude;
            float offsetX = worldScaleY * shear_.x;
            float topOffset = offsetX * (halfHeight + anchorOffsetY);
            float botOffset = offsetX * (-halfHeight + anchorOffsetY);
            v0.x += botOffset;
            v1.x += topOffset;
            v2.x += topOffset;
            v3.x += botOffset;
        }
        if (shear_.y != 0) {
            float worldScaleX = (new Vector3(_spriteMatrix.m00, _spriteMatrix.m10, _spriteMatrix.m20)).magnitude;
            float offsetY = worldScaleX * shear_.y;
            float leftOffset = offsetY * (-halfWidth + anchorOffsetX);
            float rightOffset = offsetY * (halfWidth + anchorOffsetX);
            v0.y += leftOffset;
            v1.y += leftOffset;
            v2.y += rightOffset;
            v3.y += rightOffset;
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
    
    public void GetVertexAndIndexCount (exSpriteType _spriteType, out int _vertexCount, out int _indexCount) {
        switch (spriteType_) {
        case exSpriteType.Simple:
            _vertexCount = exMesh.QUAD_VERTEX_COUNT;
            _indexCount = exMesh.QUAD_INDEX_COUNT;
            break;
        case exSpriteType.Sliced:
            _vertexCount = 4 * 4;
            _indexCount = exMesh.QUAD_INDEX_COUNT * 9;
            break;
        //case exSpriteType.Tiled:
        //    break;
        //case exSpriteType.Diced:
        //    break;
        default:
            _vertexCount = exMesh.QUAD_VERTEX_COUNT;
            _indexCount = exMesh.QUAD_INDEX_COUNT;
            break;
        }
    }
}
