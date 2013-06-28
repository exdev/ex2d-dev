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

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite base component
///
///////////////////////////////////////////////////////////////////////////////

public class exSpriteBase : MonoBehaviour {
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] public int vertexCount = 4;
    [System.NonSerialized] public int indexCount = 6;
    
    // cached for exMesh
    [System.NonSerialized] internal int spriteIndex = -1;
    [System.NonSerialized] internal int vertexBufferIndex = -1;
    [System.NonSerialized] internal int indexBufferIndex = -1;    //如果从layer中隐藏，这个值必须为-1

    // ------------------------------------------------------------------ 
    /// The current updateFlags
    // ------------------------------------------------------------------ 
    // TODO: this value will reset after every UpdateDirtyFlags()

    [System.NonSerialized] public UpdateFlags updateFlags = UpdateFlags.All;

    [System.NonSerialized] public Transform cachedTransform = null;     ///< only available after Awake

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized]
    private exLayer layer_ = null;
    public exLayer layer {
        get {
            return layer_;
        }
        internal set {
            layer_ = value;
        }
    }

    public virtual Material material { get { return null; } }
    
    /// Is component enabled and gameobject activeInHierarchy? 
    private bool isOnEnabled_;
    public bool isOnEnabled {
        get {
            return isOnEnabled_;
        }
    }

    public bool isInIndexBuffer {
        get {
            return indexBufferIndex != -1;
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
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void UpdateDirtyFlags () {
        if (cachedTransform.hasChanged) {
            updateFlags |= UpdateFlags.Vertex;
            cachedTransform.hasChanged = false;
        }
    }
}
