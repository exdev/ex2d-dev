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

//@TODO: 手动添加SpriteMng时，检查Camera属性，如果不是正交，报错

///////////////////////////////////////////////////////////////////////////////
///
/// Manage exLayers and response Camera OnPreRender event, 
///
///////////////////////////////////////////////////////////////////////////////

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ex2DMng : MonoBehaviour 
{
    ///////////////////////////////////////////////////////////////////////////////
    // serialize
    ///////////////////////////////////////////////////////////////////////////////
    
    [SerializeField] private static ex2DMng _instance;
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    List<exLayer> allLayers = new List<exLayer>();
    
    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////
    
    public static ex2DMng instance {
        get {
            if (!_instance) {
                GameObject go = new GameObject("2D Manager");
                Camera camera = go.AddComponent<Camera>();
                _instance = go.AddComponent<ex2DMng>();
                go.hideFlags = HideFlags.DontSave | exReleaseFlag.notEditable;
                camera.orthographic = true;
                camera.orthographicSize = Screen.height;
                go.SetActive(true);
            }
            return _instance;
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void Add(exLayer layer) {
        if (!allLayers.Contains(layer)) {
            allLayers.Add(layer);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void Remove(exLayer layer) {
        allLayers.Remove(layer);
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    void Awake () {
        _instance = this;
    }
	
	// ------------------------------------------------------------------ 
	// Desc:
	// ------------------------------------------------------------------ 
	
    void OnPreRender() { 
        //@TODO: 如果检测到屏幕大小改变，同步更新Camera的orthographicSize
        foreach (exLayer layer in allLayers) {
            layer.UpdateMesh();
        }
    }
}
