// ======================================================================================
// File         : exLayeredSprite.cs
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

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite component used in layer
///
///////////////////////////////////////////////////////////////////////////////

public abstract class exLayeredSprite : exSpriteBase, System.IComparable<exLayeredSprite> {

    public static bool enableFastShowHide = true;

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////

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
                if (layer_ != null) {
                    exLayer originalLayer = layer_;
                    originalLayer.Remove(this, false);
                    originalLayer.Add(this, false);
                }
                //updateFlags |= exUpdateFlags.Index;
            }
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
    
    [System.NonSerialized] protected Matrix4x4 cachedWorldMatrix;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////
    
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
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    protected override void Show () {
        if (layer_ != null) {
            if (enableFastShowHide) {
                layer_.FastShowSprite (this);
            }
            else {
                layer_.ShowSprite (this);
            }
        }
    }

    protected override void Hide () {
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
            layer_.Remove(this, false);
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
                    exLayeredSprite parentSprite = parentTransform.GetComponent<exLayeredSprite>();
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
    // Desc: 
    // ------------------------------------------------------------------ 
    
    protected override void UpdateMaterial () {
        if (layer_ != null) {
            exLayer myLayer = layer_;
            myLayer.Remove(this, false);
            material_ = null;   // set dirty, make material update.
            if (ReferenceEquals(material, null) == false) {
                myLayer.Add(this, false);
            }
        }
        else {
            material_ = null;   // set dirty, make material update.
        }
    }

    // ------------------------------------------------------------------ 
    /// \NOTE: 这个方法比lossyScale快了6倍，但返回的scale不完全精确，因为不计入rotation的影响。
    // ------------------------------------------------------------------ 

    public override float GetWorldScaleX () {
        // 这里直接从matrix拿未计入rotation影响的scale，在已知matrix的情况下，速度比较快lossyScale了6倍。
        exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
        return (new Vector3(cachedWorldMatrix.m00, cachedWorldMatrix.m10, cachedWorldMatrix.m20)).magnitude;
    }

    // ------------------------------------------------------------------ 
    /// \NOTE: 这个方法比lossyScale快了6倍，但返回的scale不完全精确，因为不计入rotation的影响。
    // ------------------------------------------------------------------ 

    public override float GetWorldScaleY () {
        exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
        return (new Vector3(cachedWorldMatrix.m01, cachedWorldMatrix.m11, cachedWorldMatrix.m21)).magnitude;
    }

    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public static bool operator > (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.depth_ > _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer > _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    /// 如果他们在同一个layer，则当layer是unordered时这个比较才有可能相等
    // ------------------------------------------------------------------ 
    
    public static bool operator >= (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.depth_ > _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer >= _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public static bool operator < (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.depth_ < _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer < _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    /// 如果他们在同一个layer，则当layer是unordered时这个比较才有可能相等
    // ------------------------------------------------------------------ 
    
    public static bool operator <= (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.depth_ < _rhs.depth_ || (_lhs.depth_ == _rhs.depth_ && _lhs.spriteIdInLayer <= _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public int CompareTo(exLayeredSprite _other) {
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

    internal override void FillBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32) {
        vertexBufferIndex = _vertices.Count;
        base.FillBuffers (_vertices, _uvs, _colors32);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected abstract Vector3[] GetVertices (Space _space);
        
    // ------------------------------------------------------------------ 
	/// Get vertices of the sprite
	/// NOTE: This function returns an empty array If sprite is invisible
    // ------------------------------------------------------------------ 

    public Vector3[] GetLocalVertices () {
        return GetVertices(Space.Self);
    }
    
	// ------------------------------------------------------------------ 
	/// Get vertices of the sprite
	/// NOTE: This function returns an empty array If sprite is invisible
	// ------------------------------------------------------------------ 

    public Vector3[] GetWorldVertices () {
        return GetVertices(Space.World);
    }

#if UNITY_EDITOR

    // ------------------------------------------------------------------ 
    /// Get sprite's geometry data
    // ------------------------------------------------------------------ 

    public void GetBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors, exList<int> _indices = null) {
        _vertices.Clear();
        _uvs.Clear();
        if (_indices != null) {
            _indices.Clear();
        }
        if (visible) {
            exUpdateFlags originalFlags = updateFlags;
            int originalVertexBufferIndex = vertexBufferIndex;
            int originalIndexBufferIndex = indexBufferIndex;

            FillBuffers(_vertices, _uvs, _colors);
            UpdateTransform();

            if (_indices != null) {
                _indices.AddRange(indexCount);
            }
            indexBufferIndex = 0;
            updateFlags |= exUpdateFlags.Index;
            UpdateBuffers(_vertices, _uvs, _colors, _indices);

            vertexBufferIndex = originalVertexBufferIndex;
            indexBufferIndex = originalIndexBufferIndex;
            updateFlags = originalFlags;
        }
    }

#endif

#endregion
    
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
    /// Calculate the world AA bounding rect of the sprite
    // ------------------------------------------------------------------ 

    public Rect GetAABoundingRect () {
        Vector3[] vertices = GetWorldVertices();
        return exGeometryUtility.GetAABoundingRect(vertices);
    }
    
}
