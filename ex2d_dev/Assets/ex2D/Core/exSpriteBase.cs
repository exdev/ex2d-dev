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

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite base component
///
///////////////////////////////////////////////////////////////////////////////

public class exSpriteBase : MonoBehaviour {
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] public int vertexCount = 4;
    [System.NonSerialized] public int indexCount = 6;
    
    // cached for layer
    [System.NonSerialized] public int spriteIndex = -1;
    [System.NonSerialized] public int vertexBufferIndex = -1;
    [System.NonSerialized] public int indexBufferIndex = -1;    //如果从layer中隐藏，这个值必须为-1

    // ------------------------------------------------------------------ 
    /// The current updateFlags
    // ------------------------------------------------------------------ 
    // TODO: this value will reset after every UpdateDirtyFlags()

    [System.NonSerialized] public UpdateFlags updateFlags = UpdateFlags.All;

    [System.NonSerialized] public Transform cachedTransform = null;     ///< only available after Awake

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////


    private exLayer layer_;
    public exLayer layer {
        get {
            return layer_;
        }
        set {
            if (layer_ == value) {
                return;
            }
            if (cachedTransform != null) {
                if (layer_) {
                    layer_.Remove(this);
                }
                if (value) {
                    bool show = enabled && gameObject.activeInHierarchy;
                    value.Add(this, show);
                }
            }
            layer_ = value;
        }
    }

    public bool IsInIndexBuffer {
        get {
            return indexBufferIndex != -1;
        }
    }

    public bool IsInVertexBuffer {
        get {
            bool awaked = cachedTransform;
            bool isInLayer = layer_ && awaked;
            exDebug.Assert(isInLayer == layer && 0 <= spriteIndex && spriteIndex < layer.spriteList.Count &&
                           object.ReferenceEquals(this, layer.spriteList[spriteIndex]));
            return isInLayer;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
        cachedTransform = transform;
        if (layer_) {
            bool show = enabled;
            layer_.Add(this, show);
        }
    }

    void OnEnable () {
        if (layer_) {
            layer_.ShowSprite(this);
        }
    }

    void OnDisable () {
        if (layer_) {
            layer_.HideSprite(this);
        }
    }

    void OnDestroy () {
        if (layer_) {
            layer_.Remove(this);
        }
        layer_ = null;
        exDebug.Assert(!layer_ || ((enabled && gameObject.activeInHierarchy) == IsInIndexBuffer), 
                       "a sprite's logic visibility should equals to it's triangle visibility");
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    public void UpdateDirtyFlags () {
        if (cachedTransform.hasChanged) {
            updateFlags |= UpdateFlags.Vertex;
            cachedTransform.hasChanged = false;
        }
    }
}
