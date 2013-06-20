// ======================================================================================
// File         : ex2DMng.cs
// Author       : Jare
// Last Change  : 06/15/2013 | 23:03:37
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
/// The 2D Manager component
/// For managing exLayers and responsing Camera OnPreRender event
///
///////////////////////////////////////////////////////////////////////////////

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ex2DMng : MonoBehaviour
{
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] public static ex2DMng instance;

    private List<exLayer> layerList = new List<exLayer>();

    
    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void Awake () {
        if (!instance) {
            instance = this;
        }
        if (camera.orthographic != true) {
            Debug.LogWarning("Set ex2DMng's camera projection to orthographic");
            camera.orthographic = true;
        }
    }

    // ------------------------------------------------------------------ 
    // NOTE: 使用DrawMesh时，要在OnRenderObject时调用，使用DrawMeshNow时，要在OnPreCull中调用
    // 使用MeshRenderer时，要在OnPreRender中调用
    // ------------------------------------------------------------------ 

    void OnPreCull () {
        // TODO: 如果检测到屏幕大小改变，同步更新Camera的orthographicSize
        for (int i = 0; i < layerList.Count; ++i) {
            layerList[i].UpdateMesh();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        for (int i = 0; i < layerList.Count; ++i) {
            Object.DestroyImmediate(layerList[i].gameObject);
        }
        layerList.Clear();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public exLayer CreateLayer () {
        exLayer layer = exLayer.Create(this);
        layerList.Add(layer);
        return layer;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void DestroyLayer (exLayer layer) {
        exDebug.Assert(layerList.Contains(layer), "can't find layer in ex2DMng");
        layerList.Remove(layer);
        Object.DestroyImmediate(layer.gameObject);
    }
}
