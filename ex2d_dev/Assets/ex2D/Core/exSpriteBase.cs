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
                    updateFlags |= UpdateFlags.Vertex;
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
                    updateFlags |= UpdateFlags.Vertex;
                }
            }
            else {
                Debug.LogWarning("Can not set sprite's height when sprite is not using customSize!");
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Anchor anchor_ = Anchor.MidCenter;
    /// the anchor position used in this plane
    // ------------------------------------------------------------------ 

    public Anchor anchor {
        get { return anchor_; }
        set {
            if ( anchor_ != value ) {
                anchor_ = value;
                updateFlags |= UpdateFlags.Vertex;
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
                updateFlags |= UpdateFlags.Vertex;
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
                updateFlags |= UpdateFlags.Vertex;
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] public int vertexCount = 4;
    [System.NonSerialized] public int indexCount = 6;
    
    // cached for geometry buffers
    [System.NonSerialized] internal int spriteIndex = -1;
    [System.NonSerialized] internal int vertexBufferIndex = -1;
    [System.NonSerialized] internal int indexBufferIndex = -1;

    // ------------------------------------------------------------------ 
    /// The current updateFlags
    // ------------------------------------------------------------------ 
    // TODO: this value will reset after every UpdateBuffers()

    [System.NonSerialized] public UpdateFlags updateFlags = UpdateFlags.All;

    [System.NonSerialized] public Transform cachedTransform = null;     ///< only available after Awake

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
            layer_ = value;
        }
    }

    public abstract Material material { get; }
    
    /// Is component enabled and gameobject activeInHierarchy? If false, the sprite is hidden.
    private bool isOnEnabled_;
    public bool isOnEnabled {
        get {
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying) {
                return this && enabled && gameObject.activeInHierarchy;
            }
#endif
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

    //public bool isInVertexBuffer {
    //    get {
    //        bool awaked = cachedTransform;
    //        bool isInLayer = layer_ && awaked;
    //        exDebug.Assert(isInLayer == layer && 0 <= spriteIndex && spriteIndex < layer.spriteList.Count &&
    //                       object.ReferenceEquals(this, layer.spriteList[spriteIndex]));
    //        return isInLayer;
    //    }
    //}

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
        cachedTransform = transform;
    }

    void OnEnable () {
        isOnEnabled_ = true;
        if (layer_ != null) {
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
        exDebug.Assert(isOnEnabled == isInIndexBuffer, 
                       "a sprite's logic visibility should equals to it's triangle visibility", this);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// 只重设layer相关属性，但不真的从layer或mesh中删除。
    // ------------------------------------------------------------------ 
    
    public void ResetLayerProperties () {
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
    /// \return the update flags of changed buffer
    /// 
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    public abstract UpdateFlags FillBuffers (List<Vector3> _vertices, List<int> _indices, List<Vector2> _uvs, List<Color32> _colors32);

    // ------------------------------------------------------------------ 
    /// \return the update flags of changed buffer
    /// 
    /// Update sprite's geometry data to buffers selectively depending on what has changed. 
    // ------------------------------------------------------------------ 

    public abstract UpdateFlags UpdateBuffers (List<Vector3> _vertices, List<int> _indices, List<Vector2> _uvs, List<Color32> _colors32);

    // ------------------------------------------------------------------ 
    /// Add sprite's vertex indices to the buffer
    // ------------------------------------------------------------------ 

    public abstract void AddToIndices (List<int> _indices);

#if UNITY_EDITOR

    // ------------------------------------------------------------------ 
    /// Get sprite's geometry data
    // ------------------------------------------------------------------ 

    public void GetBuffers (List<Vector3> _vertices, List<Vector2> _uvs) {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying && cachedTransform == null) {
            cachedTransform = transform;
        }
#endif
        UpdateFlags originalFlags = updateFlags;
        int originalIndexBufferIndex = indexBufferIndex;
        int originalVertexBufferIndex = vertexBufferIndex;

        indexBufferIndex = -1;
        _vertices.Clear();
        _uvs.Clear();
        List<int> indices = new List<int>(indexCount);
        List<Color32> colors = new List<Color32>(vertexCount);
        FillBuffers(_vertices, indices, _uvs, colors);
        UpdateBuffers(_vertices, indices, _uvs, colors);

        updateFlags = originalFlags;
        indexBufferIndex = originalIndexBufferIndex;
        vertexBufferIndex = originalVertexBufferIndex;
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
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying && cachedTransform == null) {
            cachedTransform = transform;
        }
#endif
        if (cachedTransform.hasChanged) {
            updateFlags |= UpdateFlags.Vertex;
            cachedTransform.hasChanged = false;
            // TODO: 根据parent更换layer
        }
    }
}
