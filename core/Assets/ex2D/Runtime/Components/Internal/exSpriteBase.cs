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
/// The anchor position of the exSpriteBase in 2D space
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
public abstract class exSpriteBase : MonoBehaviour, System.IComparable<exSpriteBase> {

    public static bool enableFastShowHide = true;

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
#if UNITY_EDITOR
            if (layer_ != null) {
                layer_.UpdateNowInEditMode();
            }
#endif
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
#if UNITY_EDITOR
                    if (layer_ != null) {
                        layer_.UpdateNowInEditMode();
                    }
#endif
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
#if UNITY_EDITOR
                    if (layer_ != null) {
                        layer_.UpdateNowInEditMode();
                    }
#endif
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
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float depth_ = 0;
    /// The sorting depth of this sprite in its layer. Sprite with lower depth are rendered before sprites with higher depth.
    // ------------------------------------------------------------------ 

    public float depth {
        get { return depth_; }
        set {
            if ( depth_ != value ) {
                depth_ = value;
                // 先直接重加到layer里，以后再做优化
                exLayer originalLayer = layer_;
                SetLayer(null);
                SetLayer(originalLayer);
                //updateFlags |= exUpdateFlags.Index;
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
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
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
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
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
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
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

    /// 用于相同depth的sprite之间的排序
#if !EX_DEBUG
    [HideInInspector]
#endif
    public int spriteIdInLayer = -1;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // cached for geometry buffers
    [System.NonSerialized] internal int spriteIndexInMesh = -1;
    [System.NonSerialized] internal int vertexBufferIndex = -1;
    [System.NonSerialized] internal int indexBufferIndex = -1;
    
    /// If OnEnable, isOnEnabled_ is true. If OnDisable, isOnEnabled_ is false.
    [System.NonSerialized] protected bool isOnEnabled_;

    /// fast show hide
    [System.NonSerialized] protected bool transparent_ = false;
    public bool transparent {
        get { return transparent_; }
        set {
            if ( transparent_ != value ) {
                transparent_ = value;
                updateFlags |= exUpdateFlags.Color;
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// The current updateFlags
    // ------------------------------------------------------------------ 

    [System.NonSerialized] public exUpdateFlags updateFlags = exUpdateFlags.All;    // this value will reset after every UpdateBuffers()

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////
    
    public abstract int vertexCount { get; }
    public abstract int indexCount { get; }

    [System.NonSerialized]
    protected exLayer layer_ = null;
    public exLayer layer {
        get {
            return layer_;
        }
        internal set {
            if (value != null) {
                exDebug.Assert(layer_ == null, "Sprite should remove from last layer before add to new one");
                OnPreAddToLayer();
            }
            layer_ = value;
        }
    }

    [System.NonSerialized] private Material material_;
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
    
    ///如果从layer中隐藏，isInIndexBuffer必须设为false
    public bool isInIndexBuffer {
        get {
            return indexBufferIndex != -1;
        }
        set {
            if (value == false) {
                indexBufferIndex = -1;
            }
            else {
                Debug.LogError("isInIndexBuffer can not set to true, use SetLayer instead.");
            }
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
        if (layer_ != null && visible) {
            if (enableFastShowHide) {
                layer_.FastShowSprite (this);
            }
            else {
                layer_.ShowSprite (this);
            }
        }
    }

    void OnDisable () {
        isOnEnabled_ = false;
        if (layer_ != null) {
            if (enableFastShowHide) {
                layer_.FastHideSprite (this);
            }
            else {
                layer_.HideSprite (this);
            }
        }
    }

    void OnDestroy () {
        if (layer_ != null) {
            layer_.Remove(this);
        }
        exDebug.Assert(visible == false);
        exDebug.Assert(isInIndexBuffer == false);
    }

#if UNITY_EDITOR
    
    // Allows drag & dropping of this sprite onto layer in the editor
    void LateUpdate () {
        if (UnityEditor.EditorApplication.isPlaying == false) {
            // Run through the parents and see if this sprite attached to a layer
            Transform parentTransform = cachedTransform.parent;
            while (parentTransform != null) {
                exLayer parentLayer = parentTransform.GetComponent<exLayer>();
                if (parentLayer != null) {
                    // Checks to ensure that the sprite is still parented to the right layer
                    SetLayer(parentLayer);
                    return;
                }
                else {
                    exSpriteBase parentSprite = parentTransform.GetComponent<exSpriteBase>();
                    if (parentSprite != null) {
                        SetLayer(parentSprite.layer_);
                        return;
                    }
                    else {
                        parentTransform = parentTransform.parent;
                    }
                }
            }
            // No parent
            SetLayer(null);
        }
    }

#endif

    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public static bool operator > (exSpriteBase _lhs, exSpriteBase _rhs) {
        return _lhs.depth_ > _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer > _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    /// 如果他们在同一个layer，则当layer是unordered时这个比较才有可能相等
    // ------------------------------------------------------------------ 
    
    public static bool operator >= (exSpriteBase _lhs, exSpriteBase _rhs) {
        return _lhs.depth_ > _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer >= _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public static bool operator < (exSpriteBase _lhs, exSpriteBase _rhs) {
        return _lhs.depth_ < _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer < _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    /// 如果他们在同一个layer，则当layer是unordered时这个比较才有可能相等
    // ------------------------------------------------------------------ 
    
    public static bool operator <= (exSpriteBase _lhs, exSpriteBase _rhs) {
        return _lhs.depth_ < _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer <= _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public int CompareTo(exSpriteBase _other) {
        if (depth_ < _other.depth_)
        {
            return -1;
        }
        if (depth_ > _other.depth_)
        {
            return 1;
        }
        if (spriteIdInLayer < _other.spriteIdInLayer)
        {
            return -1;
        }
        if (spriteIdInLayer > _other.spriteIdInLayer)
        {
            return 1;
        }
        return 0;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// 只重设layer相关属性，但不真的从layer或mesh中删除。
    // ------------------------------------------------------------------ 
    
    internal void ResetLayerProperties () {
        layer_ = null;
        isInIndexBuffer = false;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void SetLayer (exLayer _layer = null) {
        if (ReferenceEquals(layer_, _layer)) {
            return;
        }
        //bool isInited = (cachedTransform != null);
        //if (isInited) {
        if (_layer != null) {
            _layer.Add(this);
        }
        else if (layer_ != null) {
            layer_.Remove(this);
        }
        //}
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected virtual void OnPreAddToLayer () { }

#region Functions used to update geometry buffer.

    // ------------------------------------------------------------------ 
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    internal virtual void FillBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32) {
        vertexBufferIndex = _vertices.Count;
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

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected abstract Vector3[] GetVertices (ref Matrix4x4 _spriteMatrix);
        
    // ------------------------------------------------------------------ 
	/// Get vertices of the sprite
	/// NOTE: This function returns an empty array If sprite is invisible
    // ------------------------------------------------------------------ 

    public Vector3[] GetLocalVertices () {
        Matrix4x4 identity = Matrix4x4.identity;
        return GetVertices(ref identity);
    }
    
	// ------------------------------------------------------------------ 
	/// Get vertices of the sprite
	/// NOTE: This function returns an empty array If sprite is invisible
	// ------------------------------------------------------------------ 

    public Vector3[] GetWorldVertices () {
        Matrix4x4 l2w = cachedTransform.localToWorldMatrix;
        return GetVertices(ref l2w);
    }

#if UNITY_EDITOR

    // ------------------------------------------------------------------ 
    /// Get sprite's geometry data
    // ------------------------------------------------------------------ 

    public void GetBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<int> _indices = null) {
        _vertices.Clear();
        _uvs.Clear();
        if (_indices != null) {
            _indices.Clear();
            _indices.AddRange(indexCount);
        }
        if (visible) {
            exUpdateFlags originalFlags = updateFlags;
            int originalVertexBufferIndex = vertexBufferIndex;
            int originalIndexBufferIndex = indexBufferIndex;

            FillBuffers(_vertices, _uvs, null);
            UpdateTransform();

            indexBufferIndex = 0;
            updateFlags |= exUpdateFlags.Index;
            UpdateBuffers(_vertices, _uvs, null, _indices);

            vertexBufferIndex = originalVertexBufferIndex;
            indexBufferIndex = originalIndexBufferIndex;
            updateFlags = originalFlags;
        }
    }

#endif

#endregion

    // ------------------------------------------------------------------ 
    /// Calculate the world AA bounding rect of the sprite
    // ------------------------------------------------------------------ 

    public Rect GetAABoundingRect () {
        Vector3[] vertices = GetWorldVertices();
        return exGeometryUtility.GetAABoundingRect(vertices);
    }

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
    
    protected void UpdateMaterial () {
        if (layer_ != null) {
            exLayer myLayer = layer_;
            myLayer.Remove(this);   // TODO: do not need to re-add children
            material_ = null;   // set dirty, make material update.
            myLayer.Add(this);
        }
        else {
            material_ = null;   // set dirty, make material update.
        }
    }
}
