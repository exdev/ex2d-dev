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

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite base component
///
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
public abstract class exSpriteBase : exPlane, exISpriteBase {

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
    [System.NonSerialized] protected bool isOnEnabled;
    
    [System.NonSerialized] public exUpdateFlags updateFlags = exUpdateFlags.All;    // this value will reset after every UpdateBuffers()
    
    [System.NonSerialized] protected exClipping clip_;
    public exClipping clip {
        get {
            return clip_;
        }
        set {
            if (clip_ != value) {
                clip_ = value;
                UpdateMaterial();
            }
        }
    }
    
    [System.NonSerialized] internal Matrix4x4 cachedWorldMatrix;    // 内部使用，只有exLayeredSprite的值才可读

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \note if you want to custom the width of it, you need to set exSpriteBase.customSize to true
    // ------------------------------------------------------------------ 

    public override float width {
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
    /// \note if you want to custom the height of it, you need to set exSpriteBase.customSize to true
    // ------------------------------------------------------------------ 

    public override float height {
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
    /// the anchor position used in this sprite
    // ------------------------------------------------------------------ 

    public override Anchor anchor {
        get { return anchor_; }
        set {
            if ( anchor_ != value ) {
                anchor_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

    public override Vector2 offset {
        get { return offset_; }
        set { 
            if ( offset_ != value ) {
                offset_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }
    
    [System.NonSerialized] protected int vertexCount_ = -1;
    public int vertexCount {
        get {
            return vertexCount_;
        }
    }
    
    [System.NonSerialized] protected int indexCount_ = -1;
    public int indexCount {
        get {
            return indexCount_;
        }
    }
    
    [System.NonSerialized] protected Material material_;
    public Material material {
        get {
            if (material_ != null) {
                return material_;
            }
            if (clip_ != null) {
                material_ = clip_.GetClippedMaterial(shader_, texture);
                // if we don't have the clipping shader
                if ( material_ == null ) {
                    material_ = ex2DRenderer.GetMaterial(shader_, texture);
                }
            }
            else {
                material_ = ex2DRenderer.GetMaterial(shader_, texture);
            }
            return material_;
        }
    }

    protected abstract Texture texture { get; }

    /// 当前sprite是否可见？只返回sprite自身属性，不一定真的显示在任一camera中。
    public virtual bool visible {
        get {
            return isOnEnabled;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    protected void OnEnable () {
        isOnEnabled = true;
        if (visible) {
            Show();
        }
    }

    protected void OnDisable () {
        isOnEnabled = false;
        Hide();
    }

    protected void OnDestroy () {
        if (clip_ != null) {
            clip_.Remove(this);
        }
    }
    
#if UNITY_EDITOR

    // Allows drag & dropping of this sprite to change its clip in the editor
    protected void LateUpdate () {
        // 这里的处理方式和exLayeredSprite.LateUpdate一样
        // 如果exClipping不单单clip子物体，那就会复杂很多
        if (UnityEditor.EditorApplication.isPlaying == false) {
            // Run through the parents and see if this sprite attached to a clip
            Transform parentTransform = transform.parent;
            while (parentTransform != null) {
                exClipping parentClip = parentTransform.GetComponent<exClipping>();
                if (parentClip != null) {
                    SetClip(parentClip);
                    return;
                }
                else {
                    exSpriteBase parentSprite = parentTransform.GetComponent<exSpriteBase>();
                    if (parentSprite != null) {
                        SetClip(parentSprite.clip_);
                        return;
                    }
                    else {
                        parentTransform = parentTransform.parent;
                    }
                }
            }
            // No clip
            SetClip(null);
        }
    }

#endif
    
    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public virtual void SetClip (exClipping _clip = null) {
        if (ReferenceEquals(clip_, _clip)) {
            return;
        }
        if (_clip != null) {
            _clip.Add(this);
        }
        else if (clip_ != null) {
            clip_.Remove(this);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Interfaces
    ///////////////////////////////////////////////////////////////////////////////
    
#region Functions used to update geometry buffer.

    // ------------------------------------------------------------------ 
    /// \return the update flags of changed buffer
    /// 
    /// Update sprite's geometry data to buffers selectively depending on what has changed. 
    /// NOTE: 这个方法不应修改对象中除了updateFlags外的其它字段
    // ------------------------------------------------------------------ 

    internal abstract exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices = null);

#endregion
    
    // ------------------------------------------------------------------ 
    // Get lossy scale
    // ------------------------------------------------------------------ 

    internal abstract float GetScaleX (Space _space);
    
    // ------------------------------------------------------------------ 
    // Get lossy scale
    // ------------------------------------------------------------------ 

    internal abstract float GetScaleY (Space _space);

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void exISpriteBase.UpdateMaterial () {
        UpdateMaterial ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected abstract void UpdateMaterial ();
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected virtual void Show () { }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected virtual void Hide () { }
}
