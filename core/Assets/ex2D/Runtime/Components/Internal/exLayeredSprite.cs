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

public abstract class exLayeredSprite : exSpriteBase, System.IComparable<exLayeredSprite>, exLayer.IFriendOfLayer {

    public static bool enableFastShowHide = true;

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    [SerializeField] protected float depth_ = 0;
    /// The sorting depth of this sprite relative to its parent sprite. Sprite with lower depth are rendered before sprites with higher depth.
    // ------------------------------------------------------------------ 

    public float depth {
        get {
            return depth_;
            //if (float.IsNaN(depthNotApplyedToMesh)) {
            //    return depth_;
            //}
            //else {
            //    return depthNotApplyedToMesh;
            //}
        }
        set {
            if ( depth_ != value ) {
                depth_ = value;
                SetDepthDirty();
            }
        }
    }

#if !EX_DEBUG
    [HideInInspector]
#endif
    /// 用于相同depth的sprite之间的排序
    public int spriteIdInLayer = -1;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // cached for geometry buffers
    [System.NonSerialized] internal int spriteIndexInMesh = -1;
    [System.NonSerialized] internal int vertexBufferIndex = -1;
    [System.NonSerialized] internal int indexBufferIndex = -1;
    
    /// fast show hide
    [System.NonSerialized] protected bool transparent_ = true;
    public bool transparent {
        get { return transparent_; }
        set {
            if ( transparent_ != value ) {
                transparent_ = value;
                updateFlags |= exUpdateFlags.Transparent;
            }
        }
    }

    //[System.NonSerialized] internal float depthNotApplyedToMesh = float.NaN;
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] protected exLayer layer_ = null;
    public exLayer layer {
        get {
            return layer_;
        }
        internal set {  // TODO: 这个接口做成私有
            if (value != null) {
                exDebug.Assert(layer_ == null, "Sprite should remove from last layer before add to new one");
                if (layer_ == null) {
                    OnPreAddToLayer();
                }
            }
            layer_ = value;
        }
    }
    
    // 保存计算好的绝对depth值，只有在调用IComparable接口时才用到。(如果要检查在哪里用到，可以注释掉IComparable的实现)
    // 当父节点的depth改变时，应该先更新父节点的globalDepth并应用到mesh上，再更新子节点。
    [System.NonSerialized] private float globalDepth_ = 0;
    float exLayer.IFriendOfLayer.globalDepth {
        get { return globalDepth_; }
        set { globalDepth_ = value; }
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
            // 这里确保transform不会先于gameObject销毁，如果返回值为空，很可能是因为exLayeredSprite被销毁了
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

    protected new void OnDestroy () {
        base.OnDestroy ();
        if (layer_ != null) {
            layer_.Remove(this, false);
        }
        exDebug.Assert(visible == false);
        exDebug.Assert(isInIndexBuffer == false);
    }

#if UNITY_EDITOR
    
    // Allows drag & dropping of this sprite onto layer in the editor
    protected new void LateUpdate () {
        base.LateUpdate ();
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
            layer_.RefreshSpriteMaterial(this);
        }
        else {
            material_ = null;   // set dirty, make material update.
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public override float GetScaleX (Space _space) {
        if (_space == Space.World) {
            // 在已知matrix的情况下，这个方法比lossyScale快了6倍，但返回的scale不完全精确，因为不计入rotation的影响。
            exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
            return (new Vector3(cachedWorldMatrix.m00, cachedWorldMatrix.m10, cachedWorldMatrix.m20)).magnitude;
        }
        else {
            return cachedTransform.localScale.x;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public override float GetScaleY (Space _space) {
        if (_space == Space.World) {
            // 在已知matrix的情况下，这个方法比lossyScale快了6倍，但返回的scale不完全精确，因为不计入rotation的影响。
            exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
            return (new Vector3(cachedWorldMatrix.m01, cachedWorldMatrix.m11, cachedWorldMatrix.m21)).magnitude;
        }
        else {
            return cachedTransform.localScale.y;
        }
    }
    
    // ------------------------------------------------------------------ 
    /// Get world vertices of the sprite
    /// NOTE: This function returns an empty array If sprite is invisible
    // ------------------------------------------------------------------ 

    public override Vector3[] GetWorldVertices () {
        Vector3[] dest = GetVertices(Space.World);
        if (layer_ != null) {
            int index = layer_.IndexOfMesh (this);
            if (index != -1) {
                // apply mesh transform
                Matrix4x4 l2w = layer_.meshList[index].transform.localToWorldMatrix;;
                for (int i = 0; i < dest.Length; ++i) {
                    dest[i] = l2w.MultiplyPoint3x4 (dest[i]);
                }
                return dest;
            }
        }
        return dest;
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public override void SetClip (exClipping _clip = null) {
        if (_clip != null && layer_ != null) {
            if (_clip.transform.IsChildOf (layer_.transform) == false) {
                Debug.LogError ("Can not add to clip which not in current layer!");
                return;
            }
        }
        base.SetClip (_clip);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices = null) {
        if ((updateFlags & exUpdateFlags.Transparent) != 0) {
            updateFlags &= ~exUpdateFlags.Transparent;
            if (transparent_) {
                Vector3 samePoint = _vertices.buffer[0];
                for (int i = 1; i < vertexCount_; ++i) {
                    _vertices.buffer[vertexBufferIndex + i] = samePoint;
                }
                updateFlags &= ~exUpdateFlags.Vertex;
            }
            else {
                updateFlags |= exUpdateFlags.Vertex;
            }
            return (exUpdateFlags.Transparent | exUpdateFlags.Vertex);
        }
        else if (transparent_ && (updateFlags & exUpdateFlags.Vertex) != 0) {
            updateFlags &= ~exUpdateFlags.Vertex;
        }
        return exUpdateFlags.None;
    }
    
    #region System.IComparable<exLayeredSprite>
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public static bool operator > (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.globalDepth_ > _rhs.globalDepth_ || (_lhs.globalDepth_ == _rhs.globalDepth_ && _lhs.spriteIdInLayer > _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    /// 如果他们在同一个layer，则当layer是unordered时这个比较才有可能相等
    // ------------------------------------------------------------------ 
    
    public static bool operator >= (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.globalDepth_ > _rhs.globalDepth_ || (_lhs.globalDepth_ == _rhs.globalDepth_ && _lhs.spriteIdInLayer >= _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public static bool operator < (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.globalDepth_ < _rhs.globalDepth_ || (_lhs.globalDepth_ == _rhs.globalDepth_ && _lhs.spriteIdInLayer < _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    /// 如果他们在同一个layer，则当layer是unordered时这个比较才有可能相等
    // ------------------------------------------------------------------ 
    
    public static bool operator <= (exLayeredSprite _lhs, exLayeredSprite _rhs) {
        return _lhs.globalDepth_ < _rhs.globalDepth_ || (_lhs.globalDepth_ == _rhs.globalDepth_ && _lhs.spriteIdInLayer <= _rhs.spriteIdInLayer);
    }
    
    // ------------------------------------------------------------------ 
    /// Compare sprites by render depth, ignore layer. Sprites with lower depth are rendered before sprites with higher depth. 
    // ------------------------------------------------------------------ 
    
    public int CompareTo(exLayeredSprite _other) {
        // TODO: 直接返回减法结果
        if (globalDepth_ < _other.globalDepth_)
        {
            return -1;
        }
        if (globalDepth_ > _other.globalDepth_)
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
    
    #endregion

    //void exLayer.IFriendOfLayer.DoSetDepth (float _depth) {
    //    depth_ = _depth;
    //}

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    void exLayer.IFriendOfLayer.DoSetBufferSize (int _vertexCount, int _indexCount) {
        vertexCount_ = _vertexCount;
        indexCount_ = _indexCount;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    void exLayer.IFriendOfLayer.SetMaterialDirty () {
        material_ = null;
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
        if (_layer != null) {
            _layer.Add(this);
        }
        else if (layer_ != null) {
            layer_.Remove(this);
        }
    }
    
    // ------------------------------------------------------------------ 
    /// 如果在运行时改变了sprite的parent，需要手动调用这个方法以请求重新计算该Sprite和所有子Sprite的depth，不论它的parent之前是否已经调用过这个方法。
    /// 只需要对改变了parent的sprite调用即可，不需要对它的子sprite调用。
    // ------------------------------------------------------------------ 
    
    public void SetDepthDirty () {
        if (layer_ != null && isInIndexBuffer) {
            //if (float.IsNaN(depthNotApplyedToMesh)) {
            //    depthNotApplyedToMesh = depth_;
            //}
            layer_.SetSpriteDepthDirty(this);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected virtual void OnPreAddToLayer () { }
    
    // ------------------------------------------------------------------ 
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    internal virtual void FillBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32) {
        vertexBufferIndex = _vertices.Count;
        _vertices.AddRange(vertexCount_);
        if (_colors32 != null) {
            _colors32.AddRange(vertexCount_);
        }
        _uvs.AddRange(vertexCount_);
        updateFlags |= exUpdateFlags.AllExcludeIndex;
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
            UpdateTransform();
            exUpdateFlags originalFlags = updateFlags;
            int originalVertexBufferIndex = vertexBufferIndex;
            int originalIndexBufferIndex = indexBufferIndex;

            FillBuffers(_vertices, _uvs, _colors);

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
}
