﻿// ======================================================================================
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
    TopLeft = 0, ///< the top-left of the plane  
    TopCenter,   ///< the top-center of the plane
    TopRight,    ///< the top-right of the plane
    MidLeft,     ///< the middle-left of the plane
    MidCenter,   ///< the middle-center of the plane
    MidRight,    ///< the middle-right of the plane
    BotLeft,     ///< the bottom-left of the plane
    BotCenter,   ///< the bottom-center of the plane
    BotRight,    ///< the bottom-right of the plane
}

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite base component
///
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
public abstract class exSpriteBase : MonoBehaviour, System.IComparable<exSpriteBase> {

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
        set { customSize_ = value; }
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
                //updateFlags |= UpdateFlags.Index;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 offset_ = Vector2.zero;
    /// the offset based on the anchor, the final position of the plane equals to offset + anchor
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
    
    /// 用于相同depth的sprite之间的排序
    public int spriteIdInLayer = -1;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] public int vertexCount = 4;
    [System.NonSerialized] public int indexCount = 6;
    
    // cached for geometry buffers
    [System.NonSerialized] internal int spriteIndexInMesh = -1;
    [System.NonSerialized] internal int vertexBufferIndex = -1;
    [System.NonSerialized] internal int indexBufferIndex = -1;
    
    /// If OnEnable, isOnEnabled_ is true. If OnDisable, isOnEnabled_ is false.
    [System.NonSerialized] protected bool isOnEnabled_;

    // ------------------------------------------------------------------ 
    /// The current updateFlags
    // ------------------------------------------------------------------ 

    [System.NonSerialized] public exUpdateFlags updateFlags = exUpdateFlags.All;    // this value will reset after every UpdateBuffers()

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] private Transform cachedTransform_ = null;    
    public Transform cachedTransform {
        get {
            if (ReferenceEquals(cachedTransform_, null)) {
                cachedTransform_ = transform;
            }
            return cachedTransform_;
        }
    }

    [System.NonSerialized]
    protected exLayer layer_ = null;
    public exLayer layer {
        get {
            return layer_;
        }
        internal set {
            layer_ = value;
        }
    }

    public abstract Material material { get; }

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

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void OnEnable () {
        isOnEnabled_ = true;
        if (layer_ != null && visible) {
            layer_.ShowSprite(this);
        }
    }

    void OnDisable () {
        isOnEnabled_ = false;
        if (layer_ != null) {
            layer_.HideSprite(this);
        }
    }

    void OnDestroy () {
        if (layer_ != null) {
            layer_.Remove(this);
        }
        exDebug.Assert(visible == false);
        exDebug.Assert(isInIndexBuffer == false);
    }

    // ------------------------------------------------------------------ 
    /// Compare sprites by depth and index
    // ------------------------------------------------------------------ 
    
    public int CompareTo(exSpriteBase _other) {
        if (depth_ < _other.depth)
        {
            return -1;
        }
        if (depth_ > _other.depth)
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
        if (layer_ == _layer) {
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
    
#region Functions used to update geometry buffer.

    // ------------------------------------------------------------------ 
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    internal void FillBuffers (List<Vector3> _vertices, List<Vector2> _uvs, List<Color32> _colors32) {
        vertexBufferIndex = _vertices.Count;

        for (int i = 0; i < vertexCount; ++i) {
            _vertices.Add(new Vector3());
            _colors32.Add(new Color32());
            _uvs.Add(new Vector2());
        }
        
        updateFlags |= (exUpdateFlags.Vertex | exUpdateFlags.Color | exUpdateFlags.UV | exUpdateFlags.Normal);
    }

    // ------------------------------------------------------------------ 
    /// \return the update flags of changed buffer
    /// 
    /// Update sprite's geometry data to buffers selectively depending on what has changed. 
    /// NOTE: 这个方法不应修改对象中除了updateFlags外的其它字段
    // ------------------------------------------------------------------ 

    internal abstract exUpdateFlags UpdateBuffers (List<Vector3> _vertices, List<Vector2> _uvs, List<Color32> _colors32, List<int> _indices = null);

#if UNITY_EDITOR

    // ------------------------------------------------------------------ 
    /// Get sprite's geometry data
    // ------------------------------------------------------------------ 

    public void GetBuffers (List<Vector3> _vertices, List<Vector2> _uvs) {
        _vertices.Clear();
        _uvs.Clear();
        if (visible) {
            exUpdateFlags originalFlags = updateFlags;
            List<Color32> colors = new List<Color32>(vertexCount);

            int originalVertexBufferIndex = vertexBufferIndex;
            FillBuffers(_vertices, _uvs, colors);
            UpdateBuffers(_vertices, _uvs, colors);
            vertexBufferIndex = originalVertexBufferIndex;

            updateFlags = originalFlags;
        }
    }

#endif

#endregion

    // ------------------------------------------------------------------ 
    /// Calculate the bounding rect of the plane
    // ------------------------------------------------------------------ 

    public abstract Rect GetAABoundingRect ();

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public abstract Vector3[] GetVertices ();
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    public void UpdateTransform () {
        if (cachedTransform.hasChanged) {
            updateFlags |= exUpdateFlags.Vertex;
            cachedTransform.hasChanged = false;
            // TODO: 根据parent更换layer
        }
    }
}