// ======================================================================================
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

#if EX_DEBUG
[System.Serializable]
#endif
public class exLayer
{
    const int MAX_DYNAMIC_VERTEX_COUNT = 300;    ///< 超过这个数量的话，layer将会自动进行拆分

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    private LayerType layerType_ = LayerType.Dynamic;
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

    public List<exMesh> meshList = new List<exMesh>();

    //public bool dirty = true;

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    //// ------------------------------------------------------------------ 
    //// Desc:
    //// ------------------------------------------------------------------ 

    //public exLayer () {
    //}

    // ------------------------------------------------------------------ 
    /// Maintains a mesh to render all sprites
    // ------------------------------------------------------------------ 

    public void UpdateAllMeshes () {
        //if (dirty) {
            for (int i = 0; i < meshList.Count; ++i) {
                meshList[i].UpdateMesh();
            }
        //    dirty = false;
        //}
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to this layer. 
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    public void Add (exSpriteBase _sprite, bool _show = true) {
        // TODO: 在exSpriteBase中添加
        Material mat = _sprite.material;
        if (!mat) {
            Debug.LogError("no material assigned in sprite", _sprite);
            return;
        }
        // TODO: 就算材质相同，如果中间有其它材质挡着，也要拆分多个mesh
        exMesh sameDrawcallMesh = null;
        if (layerType == LayerType.Dynamic) {
            for (int i = 0; i < meshList.Count; ++i) {
                exMesh mesh = meshList[i];
		        if (mesh.material == mat && mesh.vertexCount < MAX_DYNAMIC_VERTEX_COUNT) {
                    sameDrawcallMesh = meshList[i];
                    break;
		        }
            }
        }
        else {
            for (int i = 0; i < meshList.Count; ++i) {
                exMesh mesh = meshList[i];
		        if (mesh.material == mat) {
                    sameDrawcallMesh = meshList[i];
                    break;
		        }
            }
        }
        if (sameDrawcallMesh == null) {
            sameDrawcallMesh = exMesh.Create(this);
            sameDrawcallMesh.material = mat;
            meshList.Add(sameDrawcallMesh);
        }
        sameDrawcallMesh.Add(_sprite, _show);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _sprite) {
        exMesh mesh = FindMesh(_sprite);
        if (mesh != null) {
            mesh.Remove(_sprite);
        }
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exSpriteBase
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    public void ShowSprite (exSpriteBase _sprite) {
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
    
    public void HideSprite (exSpriteBase _sprite) {
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
        meshList.TrimExcess();
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
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Clear() {
        for (int i = 0; i < meshList.Count; ++i) {
            Object.DestroyImmediate(meshList[i].gameObject);
        }
        meshList.Clear();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private exMesh FindMesh (exSpriteBase _sprite) {
        Material mat = _sprite.material;
        for (int i = 0; i < meshList.Count; ++i) {
            exMesh mesh = meshList[i];
		    if (object.ReferenceEquals(mesh.material, mat) && mesh.Contains(_sprite)) {
                return mesh;
		    }
        }
        Debug.LogError("sprite not exist");
        return null;
    }

#if UNITY_EDITOR

    // ------------------------------------------------------------------ 
    /// 按照绘制的先后次序返回所有sprite，供编辑器使用
    // ------------------------------------------------------------------ 

    public IEnumerator<exSpriteBase> GetEnumerator () { 
        foreach (exMesh mesh in meshList) {
            foreach (exSpriteBase sprite in mesh.spriteList) {
                yield return sprite;
            }
        }
    }

#endif
}
