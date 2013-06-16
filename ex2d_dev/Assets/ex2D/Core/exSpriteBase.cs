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
    
    // cached for layer
    [System.NonSerialized] public int lSpriteIndex = -1;
    [System.NonSerialized] public int lVerticesIndex = -1; // layerVerticesIndex
    [System.NonSerialized] public int lIndicesIndex = -1;  // layerIndicesIndex
    [System.NonSerialized] public int lIndicesCount = 6;   // 考虑是不是从verticeCount算出来

    // dirty flags
    // TODO: 这些标记更新时，应该通知所在layer，而不是让layer每一帧来获取
    [System.NonSerialized] public bool updateTransform = true;
    [System.NonSerialized] public bool updateUv = true;
    [System.NonSerialized] public bool updateColor = true;
    [System.NonSerialized] public bool updateDepth = true;

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
}
