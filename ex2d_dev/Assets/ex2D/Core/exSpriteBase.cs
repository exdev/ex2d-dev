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
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] public int vertexCount = 4;
    [System.NonSerialized] public int indexCount = 6;
    
    // cached for layer
    [System.NonSerialized] public int spriteIndex = -1;
    [System.NonSerialized] public int vertexBufferIndex = -1;
    [System.NonSerialized] public int indexBufferIndex = -1;

    // ------------------------------------------------------------------ 
    /// The current updateFlags
    // ------------------------------------------------------------------ 
    // TODO: this value will reset after every UpdateDirtyFlags()

    [System.NonSerialized] public UpdateFlags updateFlags = UpdateFlags.All;

    [System.NonSerialized] public Transform cachedTransform = null;     ///< only available after Awake

    // used to check whether transform is changed
    // 使用非法值以确保它们第一次比较时不等于sprite的真正transform
    private Vector3 lastPos = new Vector3(float.NaN, float.NaN, float.NaN);
    private Quaternion lastRotation = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
    private Vector3 lastScale = new Vector3(float.NaN, float.NaN, float.NaN);

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
                    value.Add(this);
                }
            }
            layer_ = value;
        }
    }

    public bool HasIndexBuffer {
        get {
            return indexBufferIndex != -1;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
        cachedTransform = transform;
        if (layer_) {
            layer_.Add(this);
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
        exDebug.Assert(!layer_ || ((enabled && gameObject.activeInHierarchy) == HasIndexBuffer), 
                       "a sprite's logic visibility should equals to it's triangle visibility");
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    public void UpdateDirtyFlags () {
        Quaternion newQuat = cachedTransform.rotation;
        Vector3 newPos = cachedTransform.position;
        Vector3 newScale = cachedTransform.lossyScale;
        if ((updateFlags & UpdateFlags.Vertex) == 0) {
            bool vertexChanged;
            vertexChanged = (lastPos.x != newPos.x || lastPos.y != newPos.y || lastPos.z != newPos.z);
            vertexChanged = vertexChanged ||
                            lastRotation.x != newQuat.x || lastRotation.y != newQuat.y ||
                            lastRotation.z != newQuat.z || lastRotation.w != newQuat.w;
            vertexChanged = vertexChanged || 
                            (lastScale.x != newScale.x || lastScale.y != newScale.y || lastScale.z != newScale.z);
            if (vertexChanged) {
                updateFlags |= UpdateFlags.Vertex;
            }
        }
        lastPos = newPos;
        lastRotation = newQuat;
        lastScale = newScale;
    }
}
