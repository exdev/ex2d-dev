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

// TODO: 手动添加SpriteMng时，检查Camera属性，如果不是正交，报错

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
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    private static ex2DMng instance_;
    public static ex2DMng instance {
        get {
            if (!instance_) {
                GameObject go = new GameObject("2D Manager");
                Camera camera = go.AddComponent<Camera>();
                instance_ = go.AddComponent<ex2DMng>();
                go.hideFlags = HideFlags.DontSave | exReleaseFlag.notEditable;
                camera.orthographic = true;
                camera.orthographicSize = Screen.height;
                go.SetActive(true);
            }
            return instance_;
        }
    }

    private List<exLayer> layerList = new List<exLayer>();

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void Awake () {
        instance_ = this;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void OnPreRender () {
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
