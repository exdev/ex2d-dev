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

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public override UpdateFlags UpdateBuffers (List<Vector3> _vertices, List<int> _indices, List<Vector2> _uvs, List<Color32> _colors32) {
        if ((updateFlags & UpdateFlags.Vertex) != 0) {
            var pos = cachedTransform.position;
            _vertices[vertexBufferIndex + 0] = pos + new Vector3(-1.0f, -1.0f, 0.0f);
            _vertices[vertexBufferIndex + 1] = pos + new Vector3(-1.0f, 1.0f, 0.0f);
            _vertices[vertexBufferIndex + 2] = pos + new Vector3(1.0f, 1.0f, 0.0f);
            _vertices[vertexBufferIndex + 3] = pos + new Vector3(1.0f, -1.0f, 0.0f);
        }
        if ((updateFlags & UpdateFlags.UV) != 0) {
            float xStart = (float)textureInfo.x / (float)textureInfo.texture.width;
            float yStart = (float)textureInfo.y / (float)textureInfo.texture.height;
            float xEnd = (float)(textureInfo.x + textureInfo.width) / (float)textureInfo.texture.width;
            float yEnd = (float)(textureInfo.y + textureInfo.height) / (float)textureInfo.texture.height;
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
