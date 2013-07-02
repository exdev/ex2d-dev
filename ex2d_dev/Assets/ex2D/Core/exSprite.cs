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
            if (ReferenceEquals(textureInfo_.texture, value.texture) == false) {
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

    //void Start () {
	
    //}
	
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    //void Update () {
	
    //}
}
