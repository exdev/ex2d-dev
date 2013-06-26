﻿// ======================================================================================
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

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ex2DMng : MonoBehaviour {

    [System.NonSerialized] public static ex2DMng instance;

    public List<exLayer> layerList = new List<exLayer>();

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    private Camera cachedCamera;
    
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
        cachedCamera = camera;
        if (cachedCamera.orthographic != true) {
            Debug.LogWarning("Set ex2DMng's camera projection to orthographic");
            cachedCamera.orthographic = true;
        }
    }

#if UNITY_EDITOR
    void OnEnable () {
        if (!instance) {
            instance = this;
        }
        cachedCamera = camera;
    }
#endif

    // ------------------------------------------------------------------ 
    // NOTE: 使用DrawMesh时，要在OnRenderObject时调用，使用DrawMeshNow时，要在OnPreCull中调用
    // 使用MeshRenderer时，要在OnPreRender中调用
    // ------------------------------------------------------------------ 

    void OnPreRender () {
        if (cachedCamera.orthographicSize != Screen.height) {
            //cachedCamera.orthographicSize = Screen.height;
        }
        
        for (int i = 0; i < layerList.Count; ++i) {
            layerList[i].UpdateAllMeshes();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        DestroyAllLayer();
        instance = null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void DestroyAllLayer () {
        for (int i = 0; i < layerList.Count; ++i) {
            layerList[i].Clear();
        }
        layerList.Clear();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public exLayer CreateLayer () {
        exLayer layer = new exLayer();
        layerList.Add(layer);
        return layer;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void DestroyLayer ( int _idx ) { DestroyLayer ( layerList[_idx] ); }
    public void DestroyLayer (exLayer layer) {
        exDebug.Assert(layerList.Contains(layer), "can't find layer in ex2DMng");
        layerList.Remove(layer);
        layer.Clear();
    }
}
