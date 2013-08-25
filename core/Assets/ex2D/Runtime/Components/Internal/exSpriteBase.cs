// ======================================================================================
// File         : exSpriteBase.cs
// Author       : 
// Last Change  : 06/15/2013 | 09:51:27 AM | Saturday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ------------------------------------------------------------------ 
/// The anchor position of the exSpriteBase
// ------------------------------------------------------------------ 

public enum Anchor {
    TopLeft = 0, ///< the top-left of the sprite  
    TopCenter,   ///< the top-center of the sprite
    TopRight,    ///< the top-right of the sprite
    MidLeft,     ///< the middle-left of the sprite
    MidCenter,   ///< the middle-center of the sprite
    MidRight,    ///< the middle-right of the sprite
    BotLeft,     ///< the bottom-left of the sprite
    BotCenter,   ///< the bottom-center of the sprite
    BotRight,    ///< the bottom-right of the sprite
}

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite base component
///
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
public abstract class exSpriteBase : MonoBehaviour {

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected bool customSize_ = false;
    /// if customSize set to true, users are free to set the exSpriteBase.width and exSpriteBase.height of the sprite,
    /// otherwise there is no effect when assign value to width or height.
    // ------------------------------------------------------------------ 

    public virtual bool customSize {
        get { return customSize_; }
        set {
            customSize_ = value; 
            updateFlags |= exUpdateFlags.Vertex;
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float width_ = 1.0f;
    /// the width of the sprite
    /// 
    /// \note if you want to custom the width of it, you need to set exSpriteBase.customSize to true
    // ------------------------------------------------------------------ 

    public virtual float width {
        get { return width_; }
        set {
            if (customSize_) {
                if (width_ != value) {
                    width_ = value;
                    updateFlags |= exUpdateFlags.Vertex;
                }
            }
            else {
                Debug.LogWarning("Can not set sprite's width when sprite is not using customSize!");
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float height_ = 1.0f;
    /// the height of the sprite
    /// 
    /// \note if you want to custom the height of it, you need to set exSpriteBase.customSize to true
    // ------------------------------------------------------------------ 

    public virtual float height {
        get { return height_; }
        set {
            if (customSize_) {
                if (height_ != value) {
                    height_ = value;
                    updateFlags |= exUpdateFlags.Vertex;
                }
            }
            else {
                Debug.LogWarning("Can not set sprite's height when sprite is not using customSize!");
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Anchor anchor_ = Anchor.MidCenter;
    /// the anchor position used in this sprite
    // ------------------------------------------------------------------ 

    public Anchor anchor {
        get { return anchor_; }
        set {
            if ( anchor_ != value ) {
                anchor_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Color color_ = new Color(1f, 1f, 1f, 1f);
    /// the color of the sprite
    // ------------------------------------------------------------------ 

    public Color color {
        get { return color_; }
        set {
            if ( color_ != value ) {
                color_ = value;
                updateFlags |= exUpdateFlags.Color;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 offset_ = Vector2.zero;
    /// the offset based on the anchor, the final position of the sprite equals to offset + anchor
    // ------------------------------------------------------------------ 

    public Vector2 offset {
        get { return offset_; }
        set { 
            if ( offset_ != value ) {
                offset_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 shear_ = Vector2.zero;
    /// stretch sprite into skew
    // ------------------------------------------------------------------ 

    public Vector2 shear {
        get { return shear_; }
        set { 
            if ( shear_ != value ) {
                shear_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] private Shader shader_ = null;
    /// The shader used to render this sprite
    // ------------------------------------------------------------------ 

    public Shader shader {
        get { return shader_; }
        set {
            if (ReferenceEquals(shader_, value)) {
                return;
            }
            shader_ = value;
            UpdateMaterial();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    /// If OnEnable, isOnEnabled_ is true. If OnDisable, isOnEnabled_ is false.
    [System.NonSerialized] protected bool isOnEnabled_;

    [System.NonSerialized] public exUpdateFlags updateFlags = exUpdateFlags.All;    // this value will reset after every UpdateBuffers()

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////
    
    public abstract int vertexCount { get; }
    public abstract int indexCount { get; }

    [System.NonSerialized] protected Material material_;
    public Material material {
        get {
            if (material_ != null) {
                return material_;
            }
            material_ = ex2DRenderer.GetMaterial(shader_, texture);
            return material_;
        }
    }

    protected abstract Texture texture { get; }

    /// 当前sprite是否可见？只返回sprite自身属性，不一定真的显示在任一camera中。
    public virtual bool visible {
        get {
            return isOnEnabled_;
        }
    }
    
    [System.NonSerialized] protected Transform cachedTransform_ = null;    
    public Transform cachedTransform {
        get {
            if (ReferenceEquals(cachedTransform_, null)) {
                cachedTransform_ = transform;
                cachedWorldMatrix = cachedTransform_.localToWorldMatrix;
            }
            return cachedTransform_;
        }
    }

    [System.NonSerialized] protected Matrix4x4 cachedWorldMatrix;

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void OnEnable () {
        isOnEnabled_ = true;
    }

    void OnDisable () {
        isOnEnabled_ = false;
    }

    void OnDestroy () {
        exDebug.Assert(visible == false);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

#region Functions used to update geometry buffer.

    // ------------------------------------------------------------------ 
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    internal virtual void FillBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32) {
        _vertices.AddRange(vertexCount);
        if (_colors32 != null) {
            _colors32.AddRange(vertexCount);
        }
        _uvs.AddRange(vertexCount);
        updateFlags |= exUpdateFlags.AllExcludeIndex;
    }

    // ------------------------------------------------------------------ 
    /// \return the update flags of changed buffer
    /// 
    /// Update sprite's geometry data to buffers selectively depending on what has changed. 
    /// NOTE: 这个方法不应修改对象中除了updateFlags外的其它字段
    // ------------------------------------------------------------------ 

    internal abstract exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices = null);

#endregion

    // ------------------------------------------------------------------ 
    /// Calculate the world AA bounding rect of the sprite
    // ------------------------------------------------------------------ 

    public abstract Rect GetAABoundingRect ();

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    public void UpdateTransform () {
        if (cachedTransform.hasChanged) {
            cachedTransform_.hasChanged = false;
            cachedWorldMatrix = cachedTransform_.localToWorldMatrix;
            updateFlags |= exUpdateFlags.Vertex;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    protected virtual void UpdateMaterial () {
        material_ = null;   // set dirty, make material update.
    }
}
