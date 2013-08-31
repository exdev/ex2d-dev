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
                EnsureBufferSize ();
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
                EnsureBufferSize ();
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
            SpriteBuilder.SlicedUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
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
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void EnsureBufferSize () {
        int newVertexCount, newIndexCount;
        SpriteBuilder.GetVertexAndIndexCount (spriteType_, out newVertexCount, out newIndexCount);
        if (currentVertexCount != newVertexCount || currentIndexCount != newIndexCount) {
            // re-alloc buffer
            vertices.Clear ();
            uvs.Clear ();
            colors32.Clear ();
            indices.Clear ();
            FillBuffers (vertices, uvs, colors32);
        }
    }
}
