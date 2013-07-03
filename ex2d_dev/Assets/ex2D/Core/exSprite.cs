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
                width_ = width;
                height_ = height;
                updateFlags |= UpdateFlags.Vertex;
            }
            updateFlags |= UpdateFlags.UV;  // 换了texture，UV也会重算，不换texture就更要改UV，否则没有换textureInfo的必要了。
            textureInfo_ = value;
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
            //Matrix4x4 toWorld = cachedTransform.localToWorldMatrix;
            //Vector3 pos = cachedTransform.position;
            //toWorld = Matrix4x4.TRS(cachedTransform.position, cachedTransform.rotation, cachedTransform.lossyScale);
            float halfWidth = width_ * 0.5f;
            float halfHeight = height_ * 0.5f;
            _vertices[vertexBufferIndex + 0] = cachedTransform.TransformPoint(new Vector3(-halfWidth, -halfHeight, 0.0f));
            _vertices[vertexBufferIndex + 1] = cachedTransform.TransformPoint(new Vector3(-halfWidth, halfHeight, 0.0f));
            _vertices[vertexBufferIndex + 2] = cachedTransform.TransformPoint(new Vector3(halfWidth, halfHeight, 0.0f));
            _vertices[vertexBufferIndex + 3] = cachedTransform.TransformPoint(new Vector3(halfWidth, -halfHeight, 0.0f));
        }
        if ((updateFlags & UpdateFlags.UV) != 0) {
            Vector2 texelSize = textureInfo.texture.texelSize;
            float xStart = (float)textureInfo.x * texelSize.x;
            float yStart = (float)textureInfo.y * texelSize.y;
            float xEnd = (float)(textureInfo.x + textureInfo.width) * texelSize.x;
            float yEnd = (float)(textureInfo.y + textureInfo.height) * texelSize.y;
            _uvs[vertexBufferIndex + 0] = new Vector2(xStart, yStart);
            _uvs[vertexBufferIndex + 1] = new Vector2(xStart, yEnd);
            _uvs[vertexBufferIndex + 2] = new Vector2(xEnd, yEnd);
            _uvs[vertexBufferIndex + 3] = new Vector2(xEnd, yStart);
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
    
#endregion

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
}
