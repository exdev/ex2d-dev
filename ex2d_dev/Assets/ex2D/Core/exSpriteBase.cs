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
    
    [System.NonSerialized] public int verticesCount = 4;
    [System.NonSerialized] public int lIndicesCount = 6;   // 考虑是不是从verticeCount算出来
    
    // cached for layer
    [System.NonSerialized] public int lSpriteIndex = -1;
    [System.NonSerialized] public int lVerticesIndex = -1; // layerVerticesIndex
    [System.NonSerialized] public int lIndicesIndex = -1;  // layerIndicesIndex // TODO: 使用专门的标志或属性来表示是否已经显示

    // dirty flags
    // TODO: 这些标记更新时，应该通知所在layer，而不是让layer每一帧来获取
    [System.NonSerialized] public bool updateTransform = true;
    [System.NonSerialized] public bool updateUv = true;
    [System.NonSerialized] public bool updateColor = true;
    [System.NonSerialized] public bool updateDepth = true;

    // used to check whether transform is changed
    // 使用非法值以确保它们第一次比较时不等于sprite的真正transform
    private Vector3 lastPos = new Vector3(float.NaN, float.NaN, float.NaN);
    private Quaternion lastRotation = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
    private Vector3 lastScale = new Vector3(float.NaN, float.NaN, float.NaN);

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    private Transform cachedTransform_;
    public Transform cachedTransform {
        get {
            if (cachedTransform_ == null)
                cachedTransform_ = transform; 
            return cachedTransform_; 
        }
    }

    private exLayer layer_;
    internal exLayer layer {
        get {
            return layer_;
        }
        set {
            if (layer_ == value) {
                return;
            }
            if (layer_) {
                layer_.Remove(this);
            }
            if (value) {
                value.Add(this);
            }
            layer_ = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void OnEnable () {
        if (layer_) {
            layer_.Show(this);
        }
    }

    void OnDisable () {
        if (layer_) {
            layer_.Hide(this);
        }
    }

    void OnDestroy () {
        if (layer_) {
            layer_.Remove(this);
        }
        layer_ = null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    public void UpdateDirtyFlags () {
        Vector3 p = cachedTransform.position;
        updateTransform = (lastPos.x != p.x || lastPos.y != p.y || lastPos.z != p.z);
        lastPos = p;
        
        Quaternion r = cachedTransform_.rotation;
        updateTransform = updateTransform ||
                            lastRotation.x != r.x || lastRotation.y != r.y ||
                            lastRotation.z != r.z || lastRotation.w != r.w;
        lastRotation = r;

        Vector3 s = cachedTransform_.lossyScale;
        updateTransform = updateTransform || (lastScale.x != s.x || lastScale.y != s.y || lastScale.z != s.z);
        lastScale = s;
    }
}
