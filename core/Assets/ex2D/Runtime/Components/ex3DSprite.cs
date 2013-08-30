// ======================================================================================
// File         : exSprite.cs
// Author       : 
// Last Change  : 06/15/2013 | 09:49:04 AM | Saturday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////
//#if STASH
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ex2D.Detail;

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite component
///
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/3D Sprite")]
public class ex3DSprite : exStandaloneSprite {

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
                SpriteBuilder.GetVertexAndIndexCount(spriteType_, out currentVertexCount, out currentIndexCount);
	            updateFlags |= exUpdateFlags.All;
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
                SpriteBuilder.GetVertexAndIndexCount(spriteType_, out currentVertexCount, out currentIndexCount);
	            updateFlags |= exUpdateFlags.All;
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
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    internal override void FillBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32) {
        UpdateVertexAndIndexCount();
        // fill vertex buffer
        base.FillBuffers (_vertices, _uvs, _colors32);
        // fill index buffer
        indices.AddRange(indexCount);
        updateFlags |= exUpdateFlags.Index;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices) {
        exDebug.Assert(textureInfo_ != null, "textureInfo_ == null");
        switch (spriteType_) {
        case exSpriteType.Simple:
            SpriteBuilder.SimpleUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
            break;
        case exSpriteType.Sliced:
            //SlicedUpdateBuffers (_vertices, _uvs, _indices);
            break;
        //case exSpriteType.Tiled:
        //    TiledUpdateBuffers (_vertices, _uvs, _indices);
        //    break;
        //case exSpriteType.Diced:
        //    break;
        }
        if ((updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null) {
            Color32 color32 = new Color (color_.r, color_.g, color_.b, color_.a);
            for (int i = 0; i < currentVertexCount; ++i) {
                _colors32.buffer [i] = color32;
            }
        }
        exUpdateFlags applyedFlags = updateFlags;
        updateFlags = exUpdateFlags.None;
        return applyedFlags;
    }

    //// ------------------------------------------------------------------ 
    //// Desc:
    //// ------------------------------------------------------------------ 

    //private void SlicedUpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices) {
    //    SimpleUpdateBuffers (_vertices, _uvs, _indices);
    //    if (textureInfo_.hasBorder == false) {
    //        if (_indices != null) {
    //            for (int i = 6; i < indexCount; ++i) {
    //                _indices.buffer[indexBufferIndex + i] = vertexBufferIndex;  // hide unused triangle
    //            }
    //            return;
    //        }
    //    }
    //    if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Vertex) != 0) {
    //        exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
    //        SlicedUpdateVertexBuffer(_vertices, vertexBufferIndex, ref cachedWorldMatrix);
    //    }
    //    if (/*transparent_ == false && */(updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
    //        int index = indexBufferIndex - 1;
    //        for (int i = 0; i <= 10; ++i) {
    //            if (i != 3 && i != 7) {     // 0 1 2 4 5 6 8 9 10
    //                int blVertexIndex = vertexBufferIndex + i;
    //                _indices.buffer[++index] = blVertexIndex;
    //                _indices.buffer[++index] = blVertexIndex + 4;
    //                _indices.buffer[++index] = blVertexIndex + 5;
    //                _indices.buffer[++index] = blVertexIndex + 5;
    //                _indices.buffer[++index] = blVertexIndex + 1;
    //                _indices.buffer[++index] = blVertexIndex;
    //            }
    //        }
    //    }
    //    if (/*transparent_ == false && */(updateFlags & exUpdateFlags.UV) != 0 && textureInfo_ != null) {
    //        float xStep1, xStep2, yStep1, yStep2;
    //        if (textureInfo_.rotated == false) {
    //            yStep1 = (float)textureInfo_.borderBottom / textureInfo_.height;  // uv step, not position step
    //            yStep2 = (float)(textureInfo_.height - textureInfo_.borderTop) / textureInfo_.height;
    //            xStep1 = (float)textureInfo_.borderLeft / textureInfo_.width;
    //            xStep2 = (float)(textureInfo_.width - textureInfo_.borderRight) / textureInfo_.width;
    //        }
    //        else {
    //            xStep1 = (float)textureInfo_.borderBottom / textureInfo_.height;  // uv step, not position step
    //            xStep2 = (float)(textureInfo_.height - textureInfo_.borderTop) / textureInfo_.height;
    //            yStep1 = (float)textureInfo_.borderLeft / textureInfo_.width;
    //            yStep2 = (float)(textureInfo_.width - textureInfo_.borderRight) / textureInfo_.width;
    //        }
    //        Vector2 uv0, uv15;
    //        uv0 = _uvs.buffer[vertexBufferIndex + 0];
    //        uv15 = _uvs.buffer[vertexBufferIndex + 2];
    //        Vector2 uv5 = new Vector2(uv0.x + (uv15.x - uv0.x) * xStep1, uv0.y + (uv15.y - uv0.y) * yStep1);
    //        Vector2 uv10 = new Vector2(uv0.x + (uv15.x - uv0.x) * xStep2, uv0.y + (uv15.y - uv0.y) * yStep2);

    //        if (textureInfo_.rotated == false) {
    //            //_uvs.buffer[vertexBufferIndex + 0] = uv0;
    //            _uvs.buffer[vertexBufferIndex + 1] = new Vector2(uv5.x, uv0.y);
    //            _uvs.buffer[vertexBufferIndex + 2] = new Vector2(uv10.x, uv0.y);
    //            _uvs.buffer[vertexBufferIndex + 3] = new Vector2(uv15.x, uv0.y);

    //            _uvs.buffer[vertexBufferIndex + 4] = new Vector2(uv0.x, uv5.y);
    //            _uvs.buffer[vertexBufferIndex + 5] = uv5;
    //            _uvs.buffer[vertexBufferIndex + 6] = new Vector2(uv10.x, uv5.y);
    //            _uvs.buffer[vertexBufferIndex + 7] = new Vector2(uv15.x, uv5.y);

    //            _uvs.buffer[vertexBufferIndex + 8] = new Vector2(uv0.x, uv10.y);
    //            _uvs.buffer[vertexBufferIndex + 9] = new Vector2(uv5.x, uv10.y);
    //            _uvs.buffer[vertexBufferIndex + 10] = uv10;
    //            _uvs.buffer[vertexBufferIndex + 11] = new Vector2(uv15.x, uv10.y);
            
    //            _uvs.buffer[vertexBufferIndex + 12] = new Vector2(uv0.x, uv15.y);
    //            _uvs.buffer[vertexBufferIndex + 13] = new Vector2(uv5.x, uv15.y);
    //            _uvs.buffer[vertexBufferIndex + 14] = new Vector2(uv10.x, uv15.y);
    //            _uvs.buffer[vertexBufferIndex + 15] = uv15;
    //        }
    //        else {
    //            //_uvs.buffer[vertexBufferIndex + 0] = uv0;
    //            _uvs.buffer[vertexBufferIndex + 1] = new Vector2(uv0.x, uv5.y);
    //            _uvs.buffer[vertexBufferIndex + 2] = new Vector2(uv0.x, uv10.y);
    //            _uvs.buffer[vertexBufferIndex + 3] = new Vector2(uv0.x, uv15.y);

    //            _uvs.buffer[vertexBufferIndex + 4] = new Vector2(uv5.x, uv0.y);
    //            _uvs.buffer[vertexBufferIndex + 5] = uv5;
    //            _uvs.buffer[vertexBufferIndex + 6] = new Vector2(uv5.x, uv10.y);
    //            _uvs.buffer[vertexBufferIndex + 7] = new Vector2(uv5.x, uv15.y);

    //            _uvs.buffer[vertexBufferIndex + 8] = new Vector2(uv10.x, uv0.y);
    //            _uvs.buffer[vertexBufferIndex + 9] = new Vector2(uv10.x, uv5.y);
    //            _uvs.buffer[vertexBufferIndex + 10] = uv10;
    //            _uvs.buffer[vertexBufferIndex + 11] = new Vector2(uv10.x, uv15.y);
            
    //            _uvs.buffer[vertexBufferIndex + 12] = new Vector2(uv15.x, uv0.y);
    //            _uvs.buffer[vertexBufferIndex + 13] = new Vector2(uv15.x, uv5.y);
    //            _uvs.buffer[vertexBufferIndex + 14] = new Vector2(uv15.x, uv10.y);
    //            _uvs.buffer[vertexBufferIndex + 15] = uv15;
    //        }
    //    }
    //}
    
    //// ------------------------------------------------------------------ 
    //// Desc: 
    //// ------------------------------------------------------------------ 

    //private void SlicedUpdateVertexBuffer (exList<Vector3> _vertices, int _startIndex, ref Matrix4x4 _spriteMatrix) {
    //    /* vertex index:
    //        12 13 14 15
    //        8  9  10 11
    //        4  5  6  7 
    //        0  1  2  3 
    //        */
    //    // left right columns
    //    Vector3 v0 = _vertices.buffer[_startIndex + 0];
    //    Vector3 v12 = _vertices.buffer[_startIndex + 1];
    //    Vector3 v15 = _vertices.buffer[_startIndex + 2];
    //    Vector3 v3 = _vertices.buffer[_startIndex + 3];
    //    //_vertices.buffer[_startIndex + 0] = v0;
    //    //_vertices.buffer[_startIndex + 3] = v3;
    //    _vertices.buffer[_startIndex + 12] = v12;
    //    _vertices.buffer[_startIndex + 15] = v15;
    //    float yStep1 = (float)textureInfo_.borderBottom / height_;        // position step, not uv step
    //    float yStep2 = (height_ - textureInfo_.borderTop) / height_;
    //    _vertices.buffer[_startIndex + 4] = v0 + (v12 - v0) * yStep1;
    //    _vertices.buffer[_startIndex + 7] = v3 + (v15 - v3) * yStep1;
    //    _vertices.buffer[_startIndex + 8] = v0 + (v12 - v0) * yStep2;
    //    _vertices.buffer[_startIndex + 11] = v3 + (v15 - v3) * yStep2;
    //    // mid columns
    //    float xStep1 = (float)textureInfo_.borderLeft / width_;
    //    float xStep2 = (width_ - textureInfo_.borderRight) / width_;
    //    for (int i = 0; i <= 12; i += 4) {
    //        Vector3 left = _vertices.buffer[_startIndex + i];
    //        Vector3 right = _vertices.buffer[_startIndex + i + 3];
    //        _vertices.buffer[_startIndex + i + 1] = left + (right - left) * xStep1;
    //        _vertices.buffer[_startIndex + i + 2] = left + (right - left) * xStep2;
    //    }
    //}
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void TiledUpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices) {
        
    }
    
#endregion // Functions used to update geometry buffer
    
    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void UpdateVertexAndIndexCount () {
        SpriteBuilder.GetVertexAndIndexCount(spriteType_, out currentVertexCount, out currentIndexCount);
    }
}
//#endif