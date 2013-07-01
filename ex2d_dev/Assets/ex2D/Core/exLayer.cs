﻿// ======================================================================================
// File         : exLayer.cs
// Author       : Jare
// Last Change  : 06/15/2013 | 22:34:19
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ------------------------------------------------------------------ 
/// The type of layer
/// Dynamic: 当layerType转换成dynamic后，新添加的sprite时将判断mesh顶点数，超出限制的将自动添加到新的mesh中。
// ------------------------------------------------------------------ 

public enum LayerType
{
    Static = 0,
    Dynamic,
}

///////////////////////////////////////////////////////////////////////////////
//
/// The layer class
/// NOTE: Don't add this component yourself, use ex2DMng.CreateLayer instead.
//
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
public class exLayer : MonoBehaviour
{
    const int MAX_DYNAMIC_VERTEX_COUNT = 300;    ///< 超过这个数量的话，layer将会自动进行拆分
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    public bool show = true;

    [HideInInspector] [SerializeField] private LayerType layerType_ = LayerType.Dynamic;
    public LayerType layerType {
        get {
            return layerType_;
        }
        set {
            if (layerType_ == value) {
                return;
            }
            layerType_ = value;
#if UNITY_EDITOR
            if (value == LayerType.Static && Application.isPlaying) {
                Debug.LogWarning("can't change to static during runtime");
            }
#endif
            if (value == LayerType.Dynamic) {
                for (int i = 0; i < meshList.Count; ++i) {
                    meshList[i].MarkDynamic();
                }
            }
            else if (value == LayerType.Static){
                Compact();
                // TODO: batch same material meshes
            }
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    private List<exMesh> meshList = new List<exMesh>();

    [System.NonSerialized] public Transform cachedTransform = null;     ///< only available after Awake

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
        cachedTransform = transform;
        meshList.Clear();
    }

    void OnEnable () {
        exSpriteBase[] spriteList = GetComponentsInChildren<exSpriteBase>();
        foreach (exSpriteBase sprite in spriteList) {
            Add(sprite);
        }
    }

    void OnDisable () {
        //exSpriteBase[] spriteList = GetComponentsInChildren<exSpriteBase>();
        //foreach (exSpriteBase sprite in spriteList) {
        //    // reset sprite
        //    sprite.indexBufferIndex = -1;
        //    sprite.layer = null;
        //}
        for (int i = meshList.Count - 1; i >= 0; --i) {
            exMesh mesh = meshList[i];
            if (mesh != null) {
                mesh.RemoveAll(true);
                mesh.gameObject.DestroyImmediate(); //dont save go will auto destroy
            }
        }
		meshList.Clear();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Maintains meshes to render all sprites
    // ------------------------------------------------------------------ 

    public void UpdateAllMeshes () {
        for (int i = 0; i < meshList.Count; ++i) {
            if (meshList[i] != null) {
                meshList[i].UpdateMesh();
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to this layer. 
    /// If sprite is disabled, it will keep invisible until you enable it.
    /// NOTE: You can also use exSpriteBase.SetLayer for convenience.
    // ------------------------------------------------------------------ 

    public void Add (exSpriteBase _sprite) {
        exLayer oldLayer = _sprite.layer;
        if (oldLayer == this) {
            return;
        }
        if (oldLayer != null) {
            oldLayer.Remove(_sprite);
        }
        // TODO: 在exSpriteBase中添加
        Material mat = _sprite.material;
        if (!mat) {
            Debug.LogError("no material assigned in sprite", _sprite);
            return;
        }

        _sprite.layer = this;
        _sprite.transform.parent = transform;
        // TODO: 就算材质相同，如果中间有其它材质挡着，也要拆分多个mesh
        exMesh sameDrawcallMesh = null;
        if (layerType == LayerType.Dynamic) {
            for (int i = 0; i < meshList.Count; ++i) {
                exMesh mesh = meshList[i];
		        if (mesh != null && mesh.material == mat && mesh.vertexCount < MAX_DYNAMIC_VERTEX_COUNT) {
                    sameDrawcallMesh = meshList[i];
                    break;
		        }
            }
        }
        else {
            for (int i = 0; i < meshList.Count; ++i) {
                exMesh mesh = meshList[i];
		        if (mesh != null && mesh.material == mat) {
                    sameDrawcallMesh = meshList[i];
                    break;
		        }
            }
        }
        if (sameDrawcallMesh == null) {
            sameDrawcallMesh = exMesh.Create(this);
            sameDrawcallMesh.material = mat;
            if (layerType == LayerType.Dynamic) {
                sameDrawcallMesh.MarkDynamic();
            }
            meshList.Add(sameDrawcallMesh);
        }
        bool show = _sprite.isOnEnabled;
        sameDrawcallMesh.Add(_sprite, show);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _sprite) {
        exMesh mesh = FindMesh(_sprite);
        if (mesh != null) {
            mesh.Remove(_sprite);
        }
        else {
            _sprite.indexBufferIndex = -1;  //if mesh has been destroyed, just reset sprite
        }
        _sprite.layer = null;
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exSpriteBase
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    internal void ShowSprite (exSpriteBase _sprite) {
        if (!_sprite.isInIndexBuffer) {
            exMesh mesh = FindMesh(_sprite);
            if (mesh != null) {
                mesh.ShowSprite(_sprite);
            }
		}
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    internal void HideSprite (exSpriteBase _sprite) {
        if (_sprite.isInIndexBuffer) {
            exMesh mesh = FindMesh(_sprite);
            if (mesh != null) {
                mesh.HideSprite(_sprite);
            }
		}
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Compact () {
        // TDDO: 如果是dynamic，尽量把每个mesh的顶点都填充满。如果是static，把同材质的mesh都合并起来
        for (int i = meshList.Count - 1; i >= 0; --i) {
            if (meshList[i].spriteList.Count == 0) {
                Object.Destroy(meshList[i].gameObject);
                meshList.RemoveAt(i);
            }
            else {
                meshList[i].Compact();
            }
        }
        meshList.TrimExcess();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private exMesh FindMesh (exSpriteBase _sprite) {
        Material mat = _sprite.material;
        for (int i = 0; i < meshList.Count; ++i) {
            exMesh mesh = meshList[i];
		    if (mesh != null && object.ReferenceEquals(mesh.material, mat) && mesh.Contains(_sprite)) {
                return mesh;
		    }
        }
        return null;
    }
}
