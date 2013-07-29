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

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite component
///
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D Sprite/Sprite")]
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
                if (customSize_ == false && (value.width != width_ || value.height != height_)) {
                    width_ = value.width;
                    height_ = value.height;
                    updateFlags |= exUpdateFlags.Vertex;
                }
                updateFlags |= exUpdateFlags.UV;  // 换了texture，UV也会重算，不换texture就更要改UV，否则没有换textureInfo的必要了。

                if (textureInfo_ == null || ReferenceEquals(textureInfo_.texture, value.texture) == false) {
                    textureInfo_ = value;
                    UpdateMaterial();
                    return;
                }
                else if (textureInfo_ == null && isOnEnabled_ && layer_ != null) {
                    // become visible
                    layer_.ShowSprite(this);
                }
            }
            else if (textureInfo_ != null && isOnEnabled_ && layer_ != null) {
                // become invisible
                layer_.HideSprite(this);
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
    /// The shader used to render this sprite
    // ------------------------------------------------------------------ 

    [SerializeField]
    private Shader shader_ = null;
    public Shader shader {
        get { return shader_; }
        set {
            if (ReferenceEquals(shader_, value)) {
                return;
            }
            shader_ = value;
            if (layer_ != null) {
                exLayer myLayer = layer_;
                myLayer.Remove(this);
                myLayer.Add(this);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] private Material material_;
    public override Material material {
        get {
            if (material_ != null) {
                return material_;
            }
            if (textureInfo != null) {
                material_ = ex2DMng.GetMaterial(shader, textureInfo.texture);
            }
            else {
                material_ = ex2DMng.GetMaterial(shader, null);
            }
            return material_;
        }
    }

    public override bool customSize {
        get { return customSize_; }
        set {
            if (customSize_ != value) {
                customSize_ = value;
                if (customSize_ == false && textureInfo != null) {
                    if (textureInfo.width != width_ || textureInfo.height != height_) {
                        width_ = textureInfo.width;
                        height_ = textureInfo.height;
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

    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////

#region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (List<Vector3> _vertices, List<Vector2> _uvs, List<Color32> _colors32, List<int> _indices) {
        if ((updateFlags & exUpdateFlags.Vertex) != 0) {
            UpdateVertexBuffer(_vertices, vertexBufferIndex);
        }
        if ((updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
            _indices[indexBufferIndex]     = vertexBufferIndex;
            _indices[indexBufferIndex + 1] = vertexBufferIndex + 1;
            _indices[indexBufferIndex + 2] = vertexBufferIndex + 2;
            _indices[indexBufferIndex + 3] = vertexBufferIndex + 2;
            _indices[indexBufferIndex + 4] = vertexBufferIndex + 3;
            _indices[indexBufferIndex + 5] = vertexBufferIndex;
            TestIndices(_indices);
        }
        if ((updateFlags & exUpdateFlags.UV) != 0) {
            Vector2 texelSize;
            if (textureInfo.texture != null) {
                texelSize = textureInfo.texture.texelSize;
            }
            else {
                texelSize = new Vector2(1.0f / textureInfo.rawWidth, 1.0f / textureInfo.rawHeight);
            }
            Vector2 start = new Vector2((float)textureInfo.x * texelSize.x, 
                                         (float)textureInfo.y * texelSize.y);
            Vector2 end = new Vector2((float)(textureInfo.x + textureInfo.rotatedWidth) * texelSize.x, 
                                       (float)(textureInfo.y + textureInfo.rotatedHeight) * texelSize.y);
            if ( textureInfo.rotated ) {
                _uvs[vertexBufferIndex + 1] = start;
                _uvs[vertexBufferIndex + 2] = new Vector2(start.x, end.y);
                _uvs[vertexBufferIndex + 3] = end;
                _uvs[vertexBufferIndex + 0] = new Vector2(end.x, start.y);
            }
            else {
                _uvs[vertexBufferIndex + 0] = start;
                _uvs[vertexBufferIndex + 1] = new Vector2(start.x, end.y);
                _uvs[vertexBufferIndex + 2] = end;
                _uvs[vertexBufferIndex + 3] = new Vector2(end.x, start.y);
            }
        }
        if ((updateFlags & exUpdateFlags.Color) != 0) {
            _colors32[vertexBufferIndex + 0] = new Color32(255, 255, 255, 255); // TODO: * layer.alpha ?
            _colors32[vertexBufferIndex + 1] = new Color32(255, 255, 255, 255);
            _colors32[vertexBufferIndex + 2] = new Color32(255, 255, 255, 255);
            _colors32[vertexBufferIndex + 3] = new Color32(255, 255, 255, 255);
        }
        exUpdateFlags spriteUpdateFlags = updateFlags;
        updateFlags = exUpdateFlags.None;
        return spriteUpdateFlags;
    }

#endregion // Functions used to update geometry buffer
    
    // ------------------------------------------------------------------ 
    /// Calculate the world AABB rect of the sprite
    // ------------------------------------------------------------------ 

    public override Rect GetAABoundingRect () {
        Vector3[] vertices = GetVertices();
        Rect boundingRect = new Rect();
        boundingRect.x = vertices[0].x;
        boundingRect.y = vertices[0].y;
        for (int i = 1; i < vertexCount; ++i) {
            Vector3 vertex = vertices[i];
            if (vertex.x < boundingRect.xMin) {
                boundingRect.xMin = vertex.x;
            }
            else if (vertex.x > boundingRect.xMax) {
                boundingRect.xMax = vertex.x;
            }
            if (vertex.y < boundingRect.yMin) {
                boundingRect.yMin = vertex.y;
            }
            else if (vertex.y > boundingRect.yMax) {
                boundingRect.yMax = vertex.y;
            }
        }
        return boundingRect;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override Vector3[] GetVertices () {
        List<Vector3> vertices = new List<Vector3>(vertexCount);    // TODO: use global static temp List instead
        for (int i = 0; i < vertexCount; ++i) {
            vertices.Add(new Vector3());
        }
        if (cachedTransform.hasChanged == false) {
            cachedWorldMatrix = cachedTransform_.localToWorldMatrix;
        }
        UpdateVertexBuffer(vertices, 0);
        return vertices.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    [System.Diagnostics.Conditional("EX_DEBUG")]
    void TestIndices (List<int> _indices) {
        // check indice is valid
        for (int i = indexBufferIndex; i < indexBufferIndex + indexCount; ++i) {
            if (_indices[i] < vertexBufferIndex || _indices[i] > vertexBufferIndex + vertexCount) {
                Debug.LogError("[exLayer] Wrong triangle index!");
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void UpdateVertexBuffer (List<Vector3> _vertices, int _startIndex) {
        float anchorOffsetX;
        float anchorOffsetY;
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        if (useTextureOffset_) {
            switch (anchor_) {
            //
            case Anchor.TopLeft:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x + textureInfo_.rawWidth;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y;
                break;
            case Anchor.TopCenter:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x + textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y;
                break;
            case Anchor.TopRight:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y;
                break;
            //
            case Anchor.MidLeft:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x + textureInfo_.rawWidth;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y + textureInfo_.rawHeight * 0.5f;
                break;
            case Anchor.MidCenter:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x + textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y + textureInfo_.rawHeight * 0.5f;
                break;
            case Anchor.MidRight:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y + textureInfo_.rawHeight * 0.5f;
                break;
            //
            case Anchor.BotLeft:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x + textureInfo_.rawWidth;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y + textureInfo_.rawHeight;
                break;
            case Anchor.BotCenter:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x + textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y + textureInfo_.rawHeight;
                break;
            case Anchor.BotRight:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y + textureInfo_.rawHeight;
                break;
            default:
                anchorOffsetX = -halfWidth - textureInfo_.trim_x + textureInfo_.rawWidth * 0.5f;
                anchorOffsetY = -halfHeight - textureInfo_.trim_y + textureInfo_.rawHeight * 0.5f;
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
        exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
        Vector3 v0 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(-halfWidth + anchorOffsetX, -halfHeight + anchorOffsetY, 0.0f));
        Vector3 v1 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(-halfWidth + anchorOffsetX, halfHeight + anchorOffsetY, 0.0f));
        Vector3 v2 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(halfWidth + anchorOffsetX, halfHeight + anchorOffsetY, 0.0f));
        Vector3 v3 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(halfWidth + anchorOffsetX, -halfHeight + anchorOffsetY, 0.0f));

        // 将z都设为0，使mesh所有mesh的厚度都为0，这样在mesh进行深度排序时会方便一些。但是不能用于3D Sprite
        v0.z = 0;
        v1.z = 0;
        v2.z = 0;
        v3.z = 0;

        if (shear_.x != 0) {
            // 这里直接从matrix拿未计入rotation影响的scale，在已知matrix的情况下，速度比较快lossyScale了6倍。
            // 在有rotation时，shear本来就会有冲突，所以这里不需要lossyScale。
            float worldScaleY = (new Vector3(cachedWorldMatrix.m01, cachedWorldMatrix.m11, cachedWorldMatrix.m21)).magnitude;
            float offsetX = worldScaleY * shear_.x;
            float topOffset = offsetX * (halfHeight + anchorOffsetY);
            float botOffset = offsetX * (-halfHeight + anchorOffsetY);
            v0.x += botOffset;
            v1.x += topOffset;
            v2.x += topOffset;
            v3.x += botOffset;
        }
        if (shear_.y != 0) {
            float worldScaleX = (new Vector3(cachedWorldMatrix.m00, cachedWorldMatrix.m10, cachedWorldMatrix.m20)).magnitude;
            float offsetY = worldScaleX * shear_.y;
            float leftOffset = offsetY * (-halfWidth + anchorOffsetX);
            float rightOffset = offsetY * (halfWidth + anchorOffsetX);
            v0.y += leftOffset;
            v1.y += leftOffset;
            v2.y += rightOffset;
            v3.y += rightOffset;
        }

        _vertices[_startIndex + 0] = v0;
        _vertices[_startIndex + 1] = v1;
        _vertices[_startIndex + 2] = v2;
        _vertices[_startIndex + 3] = v3;
        
        // TODO: pixel-perfect
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void UpdateMaterial (/*Shader _newShader, Texture2D _newTexture*/) {
        if (layer_ != null) {
            exLayer myLayer = layer_;
            myLayer.Remove(this);
            material_ = null;   // set dirty, make material update.
            myLayer.Add(this);
        }
        else {
            material_ = null;   // set dirty, make material update.
        }
    }
}
