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
    //Tiled,
    //Diced,
}

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite component
///
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/2D Sprite")]
public class exSprite : exLayeredSprite {

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
            // 假定不论textureInfo如何，都不改变index数量
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
                    updateFlags |= (exUpdateFlags.Vertex | exUpdateFlags.UV);
                    UpdateMaterial();
                    return;
                }
                else if (isOnEnabled_) {
                    Show();
                }
            }
            else if (textureInfo_ != null && isOnEnabled_) {
                textureInfo_ = value;
                Hide();
            }
            textureInfo_ = value;
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
                CheckBufferSize ();
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 tilling_ = new Vector2(10.0f, 10.0f);
    // ------------------------------------------------------------------ 

    public Vector2 tilling {
        get { return tilling_; }
        set {
            if ( tilling_ != value ) {
                tilling_ = value;
                CheckBufferSize ();
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

    // TODO: check border change if sliced

#region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices) {
        if (textureInfo_ != null) {
            switch (spriteType_) {
            case exSpriteType.Simple:
                SimpleUpdateBuffers (_vertices, _uvs, _indices);
                break;
            case exSpriteType.Sliced:
                SlicedUpdateBuffers (_vertices, _uvs, _indices);
                break;
            //case exSpriteType.Tiled:
            //    TiledUpdateBuffers (_vertices, _uvs, _indices);
            //    break;
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
                for (int i = 0; i < currentVertexCount; ++i) {
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
                for (int i = indexBufferIndex; i < indexBufferIndex + indexCount; ++i) {
                    _indices.buffer[i] = vertexBufferIndex;
                }
                return exUpdateFlags.All;   // TODO: remove from layer if no material
            }
            else {
                Vector3 pos = cachedTransform.position;
                for (int i = vertexBufferIndex; i < vertexBufferIndex + vertexCount; ++i) {
                    _vertices.buffer[i] = pos;
                }
                return exUpdateFlags.All;   // TODO: remove from layer if no material
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void SlicedUpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices) {
        SimpleUpdateBuffers (_vertices, _uvs, _indices);
        if (textureInfo_.hasBorder == false) {
            if (_indices != null) {
                for (int i = 6; i < indexCount; ++i) {
                    _indices.buffer[indexBufferIndex + i] = vertexBufferIndex;  // hide unused triangle
                }
                return;
            }
        }
        if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Vertex) != 0) {
            SlicedUpdateVertexBuffer(_vertices, vertexBufferIndex);
        }
        if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
            int index = indexBufferIndex - 1;
            for (int i = 0; i <= 10; ++i) {
                if (i != 3 && i != 7) {     // 0 1 2 4 5 6 8 9 10
                    int blVertexIndex = vertexBufferIndex + i;
                    _indices.buffer[++index] = blVertexIndex;
                    _indices.buffer[++index] = blVertexIndex + 4;
                    _indices.buffer[++index] = blVertexIndex + 5;
                    _indices.buffer[++index] = blVertexIndex + 5;
                    _indices.buffer[++index] = blVertexIndex + 1;
                    _indices.buffer[++index] = blVertexIndex;
                }
            }
        }
        if (/*transparent_ == false && */(updateFlags & exUpdateFlags.UV) != 0 && textureInfo_ != null) {
            float xStep1, xStep2, yStep1, yStep2;
            if (textureInfo_.rotated == false) {
                yStep1 = (float)textureInfo_.borderBottom / textureInfo_.height;  // uv step, not position step
                yStep2 = (float)(textureInfo_.height - textureInfo_.borderTop) / textureInfo_.height;
                xStep1 = (float)textureInfo_.borderLeft / textureInfo_.width;
                xStep2 = (float)(textureInfo_.width - textureInfo_.borderRight) / textureInfo_.width;
            }
            else {
                xStep1 = (float)textureInfo_.borderBottom / textureInfo_.height;  // uv step, not position step
                xStep2 = (float)(textureInfo_.height - textureInfo_.borderTop) / textureInfo_.height;
                yStep1 = (float)textureInfo_.borderLeft / textureInfo_.width;
                yStep2 = (float)(textureInfo_.width - textureInfo_.borderRight) / textureInfo_.width;
            }
            Vector2 uv0, uv15;
            uv0 = _uvs.buffer[vertexBufferIndex + 0];
            uv15 = _uvs.buffer[vertexBufferIndex + 2];
            Vector2 uv5 = new Vector2(uv0.x + (uv15.x - uv0.x) * xStep1, uv0.y + (uv15.y - uv0.y) * yStep1);
            Vector2 uv10 = new Vector2(uv0.x + (uv15.x - uv0.x) * xStep2, uv0.y + (uv15.y - uv0.y) * yStep2);

            if (textureInfo_.rotated == false) {
                //_uvs.buffer[vertexBufferIndex + 0] = uv0;
                _uvs.buffer[vertexBufferIndex + 1] = new Vector2(uv5.x, uv0.y);
                _uvs.buffer[vertexBufferIndex + 2] = new Vector2(uv10.x, uv0.y);
                _uvs.buffer[vertexBufferIndex + 3] = new Vector2(uv15.x, uv0.y);

                _uvs.buffer[vertexBufferIndex + 4] = new Vector2(uv0.x, uv5.y);
                _uvs.buffer[vertexBufferIndex + 5] = uv5;
                _uvs.buffer[vertexBufferIndex + 6] = new Vector2(uv10.x, uv5.y);
                _uvs.buffer[vertexBufferIndex + 7] = new Vector2(uv15.x, uv5.y);

                _uvs.buffer[vertexBufferIndex + 8] = new Vector2(uv0.x, uv10.y);
                _uvs.buffer[vertexBufferIndex + 9] = new Vector2(uv5.x, uv10.y);
                _uvs.buffer[vertexBufferIndex + 10] = uv10;
                _uvs.buffer[vertexBufferIndex + 11] = new Vector2(uv15.x, uv10.y);
            
                _uvs.buffer[vertexBufferIndex + 12] = new Vector2(uv0.x, uv15.y);
                _uvs.buffer[vertexBufferIndex + 13] = new Vector2(uv5.x, uv15.y);
                _uvs.buffer[vertexBufferIndex + 14] = new Vector2(uv10.x, uv15.y);
                _uvs.buffer[vertexBufferIndex + 15] = uv15;
            }
            else {
                //_uvs.buffer[vertexBufferIndex + 0] = uv0;
                _uvs.buffer[vertexBufferIndex + 1] = new Vector2(uv0.x, uv5.y);
                _uvs.buffer[vertexBufferIndex + 2] = new Vector2(uv0.x, uv10.y);
                _uvs.buffer[vertexBufferIndex + 3] = new Vector2(uv0.x, uv15.y);

                _uvs.buffer[vertexBufferIndex + 4] = new Vector2(uv5.x, uv0.y);
                _uvs.buffer[vertexBufferIndex + 5] = uv5;
                _uvs.buffer[vertexBufferIndex + 6] = new Vector2(uv5.x, uv10.y);
                _uvs.buffer[vertexBufferIndex + 7] = new Vector2(uv5.x, uv15.y);

                _uvs.buffer[vertexBufferIndex + 8] = new Vector2(uv10.x, uv0.y);
                _uvs.buffer[vertexBufferIndex + 9] = new Vector2(uv10.x, uv5.y);
                _uvs.buffer[vertexBufferIndex + 10] = uv10;
                _uvs.buffer[vertexBufferIndex + 11] = new Vector2(uv10.x, uv15.y);
            
                _uvs.buffer[vertexBufferIndex + 12] = new Vector2(uv15.x, uv0.y);
                _uvs.buffer[vertexBufferIndex + 13] = new Vector2(uv15.x, uv5.y);
                _uvs.buffer[vertexBufferIndex + 14] = new Vector2(uv15.x, uv10.y);
                _uvs.buffer[vertexBufferIndex + 15] = uv15;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    private void SlicedUpdateVertexBuffer (exList<Vector3> _vertices, int _startIndex) {
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
        float yStep1 = (float)textureInfo_.borderBottom / height_;        // position step, not uv step
        float yStep2 = (height_ - textureInfo_.borderTop) / height_;
        _vertices.buffer[_startIndex + 4] = v0 + (v12 - v0) * yStep1;
        _vertices.buffer[_startIndex + 7] = v3 + (v15 - v3) * yStep1;
        _vertices.buffer[_startIndex + 8] = v0 + (v12 - v0) * yStep2;
        _vertices.buffer[_startIndex + 11] = v3 + (v15 - v3) * yStep2;
        // mid columns
        float xStep1 = (float)textureInfo_.borderLeft / width_;
        float xStep2 = (width_ - textureInfo_.borderRight) / width_;
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

    private void TiledUpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices) {
        
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
        vertices.AddRange(vertexCount);
        
        switch (spriteType_) {
            case exSpriteType.Simple:
                SimpleUpdateVertexBuffer(vertices, 0, _space);
                break;
            case exSpriteType.Sliced:
                SimpleUpdateVertexBuffer(vertices, 0, _space);
                SlicedUpdateVertexBuffer (vertices, 0);
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

    void SimpleUpdateVertexBuffer (exList<Vector3> _vertices, int _startIndex, Space _space) {
        SpriteBuilder.SimpleUpdateVertexBuffer (this, textureInfo_, useTextureOffset_, _vertices, _startIndex, _space);

        if (_space == Space.World) {
            _vertices.buffer[_startIndex + 0] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[_startIndex + 0]);
            _vertices.buffer[_startIndex + 1] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[_startIndex + 1]);
            _vertices.buffer[_startIndex + 2] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[_startIndex + 2]);
            _vertices.buffer[_startIndex + 3] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[_startIndex + 3]);
            // 将z都设为0，使mesh所有mesh的厚度都为0，这样在mesh进行深度排序时会方便一些。但是不能用于3D Sprite
            _vertices.buffer[_startIndex + 0].z = 0;
            _vertices.buffer[_startIndex + 1].z = 0;
            _vertices.buffer[_startIndex + 2].z = 0;
            _vertices.buffer[_startIndex + 3].z = 0;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void SimpleUpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices) {
        SpriteBuilder.SimpleUpdateBuffers(this, textureInfo_, useTextureOffset_, Space.World, _vertices, _uvs, _indices, vertexBufferIndex, indexBufferIndex);

        _vertices.buffer[vertexBufferIndex + 0] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[vertexBufferIndex + 0]);
        _vertices.buffer[vertexBufferIndex + 1] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[vertexBufferIndex + 1]);
        _vertices.buffer[vertexBufferIndex + 2] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[vertexBufferIndex + 2]);
        _vertices.buffer[vertexBufferIndex + 3] = cachedWorldMatrix.MultiplyPoint3x4(_vertices.buffer[vertexBufferIndex + 3]);
        // 将z都设为0，使mesh所有mesh的厚度都为0，这样在mesh进行深度排序时会方便一些。但是不能用于3D Sprite
        _vertices.buffer[vertexBufferIndex + 0].z = 0;
        _vertices.buffer[vertexBufferIndex + 1].z = 0;
        _vertices.buffer[vertexBufferIndex + 2].z = 0;
        _vertices.buffer[vertexBufferIndex + 3].z = 0;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void UpdateVertexAndIndexCount () {
        if (layer_ == null) {
            SpriteBuilder.GetVertexAndIndexCount(spriteType_, out currentVertexCount, out currentIndexCount);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	void CheckBufferSize () {
		if (layer_ != null) {
			int newVertexCount, newIndexCount;
			SpriteBuilder.GetVertexAndIndexCount (spriteType_, out newVertexCount, out newIndexCount);
			if (currentVertexCount != newVertexCount || currentIndexCount != newIndexCount) {
				// rebuild geometry
				exLayer myLayer = layer_;
				myLayer.Remove (this, false);
				myLayer.Add (this, false);
				exDebug.Assert (currentVertexCount == newVertexCount && currentIndexCount == newIndexCount);
			}
			else {
				updateFlags |= exUpdateFlags.All;
			}
		}
	}

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Vector2 GetTextureOffset () {
        Vector2 anchorOffset = Vector2.zero;
        if (useTextureOffset_) {
            switch (anchor_) {
            case Anchor.TopLeft:
                anchorOffset.x = textureInfo_.trim_x;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height);
                break;
            case Anchor.TopCenter:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height);
                break;
            case Anchor.TopRight:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width);
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height);
                break;
            //
            case Anchor.MidLeft:
                anchorOffset.x = textureInfo_.trim_x;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            case Anchor.MidCenter:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            case Anchor.MidRight:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width);
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            //
            case Anchor.BotLeft:
                anchorOffset.x = textureInfo_.trim_x;
                anchorOffset.y = textureInfo_.trim_y;
                break;
            case Anchor.BotCenter:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = textureInfo_.trim_y;
                break;
            case Anchor.BotRight:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width);
                anchorOffset.y = textureInfo_.trim_y;
                break;
            //
            default:
                anchorOffset.x = textureInfo_.trim_x - (textureInfo_.rawWidth - textureInfo_.width) * 0.5f;
                anchorOffset.y = textureInfo_.trim_y - (textureInfo_.rawHeight - textureInfo_.height) * 0.5f;
                break;
            }
            Vector2 customSizeScale = new Vector2 (width_ / textureInfo_.width, height_ / textureInfo_.height);
            anchorOffset.x *= customSizeScale.x;
            anchorOffset.y *= customSizeScale.y;
        }
        return anchorOffset;
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
    
    internal static void SimpleUpdateBuffers (exSpriteBase _sprite, exTextureInfo _textureInfo, bool _useTextureOffset, Space _space,
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
        if (/*transparent_ == false && */(_sprite.updateFlags & exUpdateFlags.UV) != 0 && _textureInfo != null) {
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

    internal static void SimpleUpdateVertexBuffer (exSpriteBase _sprite, exTextureInfo textureInfo_, bool useTextureOffset_, exList<Vector3> _vertices, int _startIndex, Space _space) {
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
            case Anchor.TopLeft     : anchorOffset.x = halfWidth;   anchorOffset.y = -halfHeight;  break;
            case Anchor.TopCenter   : anchorOffset.x = 0.0f;        anchorOffset.y = -halfHeight;  break;
            case Anchor.TopRight    : anchorOffset.x = -halfWidth;  anchorOffset.y = -halfHeight;  break;

            case Anchor.MidLeft     : anchorOffset.x = halfWidth;   anchorOffset.y = 0.0f;         break;
            case Anchor.MidCenter   : anchorOffset.x = 0.0f;        anchorOffset.y = 0.0f;         break;
            case Anchor.MidRight    : anchorOffset.x = -halfWidth;  anchorOffset.y = 0.0f;         break;

            case Anchor.BotLeft     : anchorOffset.x = halfWidth;   anchorOffset.y = halfHeight;   break;
            case Anchor.BotCenter   : anchorOffset.x = 0.0f;        anchorOffset.y = halfHeight;   break;
            case Anchor.BotRight    : anchorOffset.x = -halfWidth;  anchorOffset.y = halfHeight;   break;

            default                 : anchorOffset.x = 0.0f;        anchorOffset.y = 0.0f;         break;
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
        v0 += offset;
        v1 += offset;
        v2 += offset;
        v3 += offset;

        Vector2 shear = _sprite.shear;
        if (shear.x != 0) {
            float scaleY;
            if (_space == Space.World) {
                // 在有rotation时，shear本来就会有冲突，所以这里不需要lossyScale。
                scaleY = _sprite.GetWorldScaleY();
            }
            else {
                scaleY = _sprite.transform.localScale.y;
            }
            float offsetX = scaleY * shear.x;
            float topOffset = offsetX * (halfHeight + anchorOffset.y);
            float botOffset = offsetX * (-halfHeight + anchorOffset.y);
            v0.x += botOffset;
            v1.x += topOffset;
            v2.x += topOffset;
            v3.x += botOffset;
        }
        if (shear.y != 0) {
            float scaleX;
            if (_space == Space.World) {
                // 在有rotation时，shear本来就会有冲突，所以这里不需要lossyScale。
                scaleX = _sprite.GetWorldScaleX();
            }
            else {
                scaleX = _sprite.transform.localScale.x;
            }
            float offsetY = scaleX * shear.y;
            float leftOffset = offsetY * (-halfWidth + anchorOffset.x);
            float rightOffset = offsetY * (halfWidth + anchorOffset.x);
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
    
    public static void GetVertexAndIndexCount (exSpriteType _spriteType, out int _vertexCount, out int _indexCount) {
        // 假定不论textureInfo如何，都不改变index, vertex数量
        switch (_spriteType) {
        case exSpriteType.Simple:
            _vertexCount = exMesh.QUAD_VERTEX_COUNT;
            _indexCount = exMesh.QUAD_INDEX_COUNT;
            break;
        case exSpriteType.Sliced:
            _vertexCount = 4 * 4;
            _indexCount = exMesh.QUAD_INDEX_COUNT * 9;
            break;
        //case exSpriteType.Tiled:
        //    int quadCount = (int)Mathf.Ceil (tilling_.x) * (int)Mathf.Ceil (tilling_.y);
        //    _vertexCount = exMesh.QUAD_VERTEX_COUNT * quadCount;
        //    _indexCount = exMesh.QUAD_INDEX_COUNT * quadCount;
        //    break;
        //exSpriteType.Diced:
        //    break;
        default:
            _vertexCount = exMesh.QUAD_VERTEX_COUNT;
            _indexCount = exMesh.QUAD_INDEX_COUNT;
            break;
        }
    }
}
}
