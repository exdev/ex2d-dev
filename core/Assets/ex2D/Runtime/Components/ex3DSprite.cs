// ======================================================================================
// File         : ex3DSprite.cs
// Author       : 
// Last Change  : 08/25/2013
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
public class ex3DSprite : exStandaloneSprite, exISprite {

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
            // 如果用户在运行时改变了textureInfo，则需要重新设置textureInfo
            // 假定不论textureInfo如何，都不改变index数量
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
                    if (old == null || ReferenceEquals(old, value) || value.rawWidth != old.rawWidth || value.rawHeight != old.rawHeight) {
                        UpdateBufferSize ();
                        updateFlags |= exUpdateFlags.Vertex;    // tile数量可能不变，但是间距可能会改变
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
                if (spriteType_ == exSpriteType.Tiled) {
                    customSize_ = true;
                }
                UpdateBufferSize ();
                updateFlags |= exUpdateFlags.All;
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
            if (spriteType_ == exSpriteType.Tiled) {
                UpdateBufferSize ();
                updateFlags |= exUpdateFlags.UV;
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
            if (spriteType_ == exSpriteType.Tiled) {
                UpdateBufferSize ();
                updateFlags |= exUpdateFlags.UV;
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
        exDebug.Assert(textureInfo_ != null, "textureInfo_ == null");
        if (updateFlags == exUpdateFlags.None) {
            return exUpdateFlags.None;
        }
        
        switch (spriteType_) {
        case exSpriteType.Simple:
            SpriteBuilder.SimpleUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
            break;
        case exSpriteType.Sliced:
            SpriteBuilder.SlicedUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
            break;
        case exSpriteType.Tiled:
            SpriteBuilder.TiledUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
            break;
        //case exSpriteType.Diced:
        //    break;
        }
        if ((updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null) {
            Color32 color32 = new Color (color_.r, color_.g, color_.b, color_.a);
            for (int i = 0; i < vertexCount_; ++i) {
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
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (Space _space) {
        if (textureInfo_ == null) {
            return new Vector3[0];
        }

        exList<Vector3> vertices = exList<Vector3>.GetTempList();
        UpdateBufferSize();
        vertices.AddRange(vertexCount_);

        switch (spriteType_) {
        case exSpriteType.Simple:
            SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, vertices, 0);
            break;
        case exSpriteType.Sliced:
            SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, vertices, 0);
            SpriteBuilder.SimpleVertexBufferToSliced(this, textureInfo_, vertices, 0);
            break;
        case exSpriteType.Tiled:
            SpriteBuilder.TiledUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, vertices, 0);
            break;
        //case exSpriteType.Diced:
            //    break;
        }

        return vertices.ToArray();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void UpdateVertexAndIndexCount () {
        this.GetVertexAndIndexCount(out vertexCount_, out indexCount_);
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////
    
}
