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
            if (value != null) {
                if (isOnEnabled) {
                    Show ();
                }
            }
            else if (isOnEnabled && textureInfo_ != null) {
                Hide ();
            }
            // 如果用户在运行时改变了textureInfo，则需要重新设置textureInfo
            exSpriteUtility.SetTextureInfo (this, ref textureInfo_, value, useTextureOffset_, spriteType_);
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
                if (value == exSpriteType.Tiled) {
                    customSize_ = true;
                }
                else if (value == exSpriteType.Diced) {
                    if (textureInfo_ != null && textureInfo_.diceUnitWidth == 0 && textureInfo_.diceUnitHeight == 0) {
                        Debug.LogWarning ("The texture info is not diced!");
                    }
                }
                spriteType_ = value;
                UpdateBufferSize ();
                updateFlags |= exUpdateFlags.All;
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 tiledSpacing_ = new Vector2(0f, 0f);
    // ------------------------------------------------------------------ 

    public Vector2 tiledSpacing {
        get { return tiledSpacing_; }
        set {
            if ( tiledSpacing_ != value ) {
                tiledSpacing_ = value;
                UpdateBufferSize();
                updateFlags |= (exUpdateFlags.Vertex | exUpdateFlags.UV);
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected bool borderOnly_ = false;
    /// only used for sliced sprite
    // ------------------------------------------------------------------ 

    public bool borderOnly {
        get { return borderOnly_; }
        set {
            if ( borderOnly_ != value ) {
                borderOnly_ = value;
                if (spriteType_ == exSpriteType.Sliced) {
                    UpdateBufferSize();
                    updateFlags |= exUpdateFlags.All;
                }
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected bool customBorderSize_ = false;
    /// only used for sliced sprite
    // ------------------------------------------------------------------ 

    public bool customBorderSize {
        get { return customBorderSize_; }
        set {
            if ( customBorderSize_ != value ) {
                customBorderSize_ = value;
                if (spriteType_ == exSpriteType.Sliced) {
                    updateFlags |= exUpdateFlags.Vertex;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float leftBorderSize_;
    /// The left border size used for sliced sprite
    // ------------------------------------------------------------------ 

    public float leftBorderSize {
        get { return leftBorderSize_; }
        set {
            if ( leftBorderSize_ != value ) {
                leftBorderSize_ = value;
                if (spriteType_ == exSpriteType.Sliced) {
                    updateFlags |= exUpdateFlags.Vertex;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float rightBorderSize_;
    /// The right border size used for sliced sprite
    // ------------------------------------------------------------------ 

    public float rightBorderSize {
        get { return rightBorderSize_; }
        set {
            if ( rightBorderSize_ != value ) {
                rightBorderSize_ = value;
                if (spriteType_ == exSpriteType.Sliced) {
                    updateFlags |= exUpdateFlags.Vertex;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float topBorderSize_;
    /// The top border size used for sliced sprite
    // ------------------------------------------------------------------ 

    public float topBorderSize {
        get { return topBorderSize_; }
        set {
            if ( topBorderSize_ != value ) {
                topBorderSize_ = value;
                if (spriteType_ == exSpriteType.Sliced) {
                    updateFlags |= exUpdateFlags.Vertex;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float bottomBorderSize_;
    /// The bottom border size used for sliced sprite
    // ------------------------------------------------------------------ 

    public float bottomBorderSize {
        get { return bottomBorderSize_; }
        set {
            if ( bottomBorderSize_ != value ) {
                bottomBorderSize_ = value;
                if (spriteType_ == exSpriteType.Sliced) {
                    updateFlags |= exUpdateFlags.Vertex;
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
            return isOnEnabled && textureInfo_ != null;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////

    // TODO: check border change if sliced

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
        case exSpriteType.Tiled:
            SpriteBuilder.TiledUpdateBuffers (this, textureInfo_, useTextureOffset_, tiledSpacing_, Space.Self, _vertices, _uvs, _indices, 0, 0);
            break;
        case exSpriteType.Diced:
            SpriteBuilder.DicedUpdateBuffers (this, textureInfo_, useTextureOffset_, Space.Self, _vertices, _uvs, _indices, 0, 0);
            break;
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
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (Space _space) {
        if (textureInfo_ == null) {
            return new Vector3[0];
        }

        exList<Vector3> vb = exList<Vector3>.GetTempList();
        UpdateBufferSize();
        vb.AddRange(vertexCount_);

        switch (spriteType_) {
        case exSpriteType.Simple:
            SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, vb, 0);
            break;
        case exSpriteType.Sliced:
            SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, vb, 0);
            SpriteBuilder.SimpleVertexBufferToSliced(this, textureInfo_, vb, 0);
            break;
        case exSpriteType.Tiled:
            SpriteBuilder.TiledUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, tiledSpacing_, _space, vb, 0);
            break;
        case exSpriteType.Diced:
            SpriteBuilder.SimpleUpdateVertexBuffer(this, textureInfo_, useTextureOffset_, _space, vb, 0);
            SpriteBuilder.SimpleVertexBufferToDiced(this, textureInfo_, vb, 0);
            break;
        }

        return vb.ToArray();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void UpdateVertexAndIndexCount () {
        this.GetVertexAndIndexCount(out vertexCount_, out indexCount_);
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void exISprite.UpdateBufferSize () {
        UpdateBufferSize ();
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////
    
}
