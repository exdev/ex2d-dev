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

[AddComponentMenu("ex2D/Sprite")]
public class exSprite : exSpriteBase {

    ///////////////////////////////////////////////////////////////////////////////
    // serialize
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// The texture info used in this sprite
    // ------------------------------------------------------------------ 

    [SerializeField]
    private exTextureInfo textureInfo_ = null;
    public exTextureInfo textureInfo {
        get { return textureInfo_; }
        set {
            if (ReferenceEquals(textureInfo_, value)) {
                return;
            }
            if (textureInfo_ == null || ReferenceEquals(textureInfo_.texture, value.texture) == false) {
                // material changed, update layer to make mesh change
                if (layer_ != null) {
                    exLayer myLayer = layer_;
                    myLayer.Remove(this);
                    myLayer.Add(this);
                }
            }
            if (customSize_ == false && (value.width != width_ || value.height != height_)) {
                width_ = value.width;
                height_ = value.height;
                updateFlags |= UpdateFlags.Vertex;
            }
            updateFlags |= UpdateFlags.UV;  // 换了texture，UV也会重算，不换texture就更要改UV，否则没有换textureInfo的必要了。
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
                updateFlags |= UpdateFlags.Vertex;
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

    private Material material_;
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
        // TODO: if material changed, update sprite's exMesh
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
                        updateFlags |= UpdateFlags.Vertex;
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

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

#region Functions used to update geometry buffer
       
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public override UpdateFlags FillBuffers (List<Vector3> _vertices, List<int> _indices, List<Vector2> _uvs, List<Color32> _colors32) {
        vertexBufferIndex = _vertices.Count;

        _vertices.Add(new Vector3());
        _vertices.Add(new Vector3());
        _vertices.Add(new Vector3());
        _vertices.Add(new Vector3());
        _colors32.Add(new Color32());
        _colors32.Add(new Color32());
        _colors32.Add(new Color32());
        _colors32.Add(new Color32());
        _uvs.Add(new Vector2());
        _uvs.Add(new Vector2());
        _uvs.Add(new Vector2());
        _uvs.Add(new Vector2());

        updateFlags = (UpdateFlags.Vertex | UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);

        bool show = isOnEnabled;
        if (show) {
            AddToIndices(_indices);
        }
        return updateFlags;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public override UpdateFlags UpdateBuffers (List<Vector3> _vertices, List<int> _indices, List<Vector2> _uvs, List<Color32> _colors32) {
        if ((updateFlags & UpdateFlags.Vertex) != 0) {
            UpdateVertexBuffer(_vertices, vertexBufferIndex);
        }
        if ((updateFlags & UpdateFlags.UV) != 0) {
            Vector2 texelSize = textureInfo.texture.texelSize;
            float xStart = (float)textureInfo.x * texelSize.x;
            float yStart = (float)textureInfo.y * texelSize.y;
            float xEnd = (float)(textureInfo.x + textureInfo.width) * texelSize.x;
            float yEnd = (float)(textureInfo.y + textureInfo.height) * texelSize.y;
            if ( textureInfo.rotated ) {
                _uvs[vertexBufferIndex + 0] = new Vector2(xStart, yEnd);
                _uvs[vertexBufferIndex + 1] = new Vector2(xEnd, yEnd);
                _uvs[vertexBufferIndex + 2] = new Vector2(xEnd, yStart);
                _uvs[vertexBufferIndex + 3] = new Vector2(xStart, yStart);
            }
            else {
                _uvs[vertexBufferIndex + 0] = new Vector2(xStart, yStart);
                _uvs[vertexBufferIndex + 1] = new Vector2(xStart, yEnd);
                _uvs[vertexBufferIndex + 2] = new Vector2(xEnd, yEnd);
                _uvs[vertexBufferIndex + 3] = new Vector2(xEnd, yStart);
            }
        }
        if ((updateFlags & UpdateFlags.Color) != 0) {
            _colors32[vertexBufferIndex + 0] = new Color32(255, 255, 255, 255);
            _colors32[vertexBufferIndex + 1] = new Color32(255, 255, 255, 255);
            _colors32[vertexBufferIndex + 2] = new Color32(255, 255, 255, 255);
            _colors32[vertexBufferIndex + 3] = new Color32(255, 255, 255, 255);
        }
        if ((updateFlags & UpdateFlags.Index) != 0) {
            // TODO: resort
            TestIndices(_indices);
        }
        UpdateFlags spriteUpdateFlags = updateFlags;
        updateFlags = UpdateFlags.None;
        return spriteUpdateFlags;
    }

    // ------------------------------------------------------------------ 
    // Add and resort indices by depth
    // ------------------------------------------------------------------ 

    public override void AddToIndices (List<int> _indices) {
        exDebug.Assert(!isInIndexBuffer);
        if (!isInIndexBuffer) {
            indexBufferIndex = _indices.Count;
            _indices.Add(vertexBufferIndex + 0);
            _indices.Add(vertexBufferIndex + 1);
            _indices.Add(vertexBufferIndex + 2);
            _indices.Add(vertexBufferIndex + 3);
            _indices.Add(vertexBufferIndex + 0);
            _indices.Add(vertexBufferIndex + 2);
        
            updateFlags |= UpdateFlags.Index;

            // TODO: resort indices by depth
            TestIndices(_indices);
        }
    }

#endregion // Functions used to update geometry buffer
    
    // ------------------------------------------------------------------ 
    /// Calculate the world AABB rect of the sprite
    // ------------------------------------------------------------------ 

    public override Rect GetAABoundingRect () {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying && cachedTransform == null) {
            cachedTransform = transform;
        }
#endif
        List<Vector3> vertices = new List<Vector3>(vertexCount);
        for (int i = 0; i < vertexCount; ++i) {
            vertices.Add(new Vector3());
        }
        UpdateVertexBuffer(vertices, 0);
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
        float offsetX = 0.0f;
        float offsetY = 0.0f;
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        if (useTextureOffset_) {
            switch (anchor_) {
            //
            case Anchor.TopLeft:
                offsetX = -halfWidth - textureInfo.trim_x + textureInfo.rawWidth;
                offsetY = -halfHeight - textureInfo.trim_y;
                break;
            case Anchor.TopCenter:
                offsetX = -halfWidth - textureInfo.trim_x + textureInfo.rawWidth * 0.5f;
                offsetY = -halfHeight - textureInfo.trim_y;
                break;
            case Anchor.TopRight:
                offsetX = -halfWidth - textureInfo.trim_x;
                offsetY = -halfHeight - textureInfo.trim_y;
                break;
            //
            case Anchor.MidLeft:
                offsetX = -halfWidth - textureInfo.trim_x + textureInfo.rawWidth;
                offsetY = -halfHeight - textureInfo.trim_y + textureInfo.rawHeight * 0.5f;
                break;
            case Anchor.MidCenter:
                offsetX = -halfWidth - textureInfo.trim_x + textureInfo.rawWidth * 0.5f;
                offsetY = -halfHeight - textureInfo.trim_y + textureInfo.rawHeight * 0.5f;
                break;
            case Anchor.MidRight:
                offsetX = -halfWidth - textureInfo.trim_x;
                offsetY = -halfHeight - textureInfo.trim_y + textureInfo.rawHeight * 0.5f;
                break;
            //
            case Anchor.BotLeft:
                offsetX = -halfWidth - textureInfo.trim_x + textureInfo.rawWidth;
                offsetY = -halfHeight - textureInfo.trim_y + textureInfo.rawHeight;
                break;
            case Anchor.BotCenter:
                offsetX = -halfWidth - textureInfo.trim_x + textureInfo.rawWidth * 0.5f;
                offsetY = -halfHeight - textureInfo.trim_y + textureInfo.rawHeight;
                break;
            case Anchor.BotRight:
                offsetX = -halfWidth - textureInfo.trim_x;
                offsetY = -halfHeight - textureInfo.trim_y + textureInfo.rawHeight;
                break;
            default:
                offsetX = -halfWidth - textureInfo.trim_x + textureInfo.rawWidth * 0.5f;
                offsetY = -halfHeight - textureInfo.trim_y + textureInfo.rawHeight * 0.5f;
                break;
            }
        }
        else {
            switch ( anchor_ ) {
            case Anchor.TopLeft     : offsetX = halfWidth;   offsetY = -halfHeight;  break;
            case Anchor.TopCenter   : offsetX = 0.0f;        offsetY = -halfHeight;  break;
            case Anchor.TopRight    : offsetX = -halfWidth;  offsetY = -halfHeight;  break;

            case Anchor.MidLeft     : offsetX = halfWidth;   offsetY = 0.0f;         break;
            case Anchor.MidCenter   : offsetX = 0.0f;        offsetY = 0.0f;         break;
            case Anchor.MidRight    : offsetX = -halfWidth;  offsetY = 0.0f;         break;

            case Anchor.BotLeft     : offsetX = halfWidth;   offsetY = halfHeight;   break;
            case Anchor.BotCenter   : offsetX = 0.0f;        offsetY = halfHeight;   break;
            case Anchor.BotRight    : offsetX = -halfWidth;  offsetY = halfHeight;   break;

            default                 : offsetX = 0.0f;        offsetY = 0.0f;         break;
            }
        }
        //Matrix4x4 toWorld = cachedTransform.localToWorldMatrix;
        //Vector3 pos = cachedTransform.position;
        //toWorld = Matrix4x4.TRS(cachedTransform.position, cachedTransform.rotation, cachedTransform.lossyScale);

        offsetX += offset_.x;
        offsetY += offset_.y;
        _vertices[_startIndex + 0] = cachedTransform.TransformPoint(new Vector3(-halfWidth + offsetX, -halfHeight + offsetY, 0.0f));
        _vertices[_startIndex + 1] = cachedTransform.TransformPoint(new Vector3(-halfWidth + offsetX,  halfHeight + offsetY, 0.0f));
        _vertices[_startIndex + 2] = cachedTransform.TransformPoint(new Vector3( halfWidth + offsetX,  halfHeight + offsetY, 0.0f));
        _vertices[_startIndex + 3] = cachedTransform.TransformPoint(new Vector3( halfWidth + offsetX, -halfHeight + offsetY, 0.0f));
        // TODO: pixel-perfect
    }
}
