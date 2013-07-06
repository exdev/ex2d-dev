// ======================================================================================
// File         : exLayer.cs
// Author       : Jare
// Last Change  : 06/15/2013 | 22:34:19
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

#define FORCE_UPDATE_VERTEX_INFO ///< 删除mesh最后面的顶点时，仅先从index buffer和vertex buffer中清除，其它数据不标记为脏。因为是尾端的冗余数据，不同步也可以。

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ------------------------------------------------------------------ 
/// The type of layer
// ------------------------------------------------------------------ 

public enum LayerType
{
    Static = 0,
    Dynamic,    ///< 当layerType转换成dynamic后，新添加的sprite时将判断mesh顶点数，超出限制的将自动添加到新的mesh中。
}

///////////////////////////////////////////////////////////////////////////////
//
/// The layer class
/// NOTE: Don't add this component yourself, use ex2DMng.instance.CreateLayer instead.
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
                for (int s = 0; s < mesh.spriteList.Count; ++s) {
            	    exSpriteBase sprite = mesh.spriteList[s];
                    exDebug.Assert(sprite);
                    if (sprite) {
                        sprite.ResetLayerProperties();
                    }
                }
                //mesh.Clear();
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

    public void UpdateSprites () {
        for (int m = 0; m < meshList.Count; ++m) {
            exMesh mesh = meshList[m];
            if (mesh == null) {
                continue;
            }
            UpdateFlags meshUpdateFlags = UpdateFlags.None;
            for (int i = 0; i < mesh.spriteList.Count; ++i) {
                exSpriteBase sprite = mesh.spriteList[i];
                exDebug.Assert(sprite.isOnEnabled == sprite.isInIndexBuffer);
                
                if (sprite.isOnEnabled) {
                    sprite.UpdateTransform();
                    UpdateFlags spriteUpdateFlags = sprite.UpdateBuffers(mesh.vertices, mesh.indices, mesh.uvs, mesh.colors32);
                    meshUpdateFlags |= spriteUpdateFlags;
                }
            }
            mesh.Apply(meshUpdateFlags);
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
        if (_sprite.transform.IsChildOf(transform) == false) {
            _sprite.transform.parent = transform;
        }
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
        Add(sameDrawcallMesh, _sprite);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _sprite) {
        exMesh mesh = FindMesh(_sprite);
        if (mesh != null) {
            Remove(mesh, _sprite);
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
                bool hasSprite = mesh.spriteList.Contains(_sprite);
                if (!hasSprite) {
                    Debug.LogError("can't find sprite to show");
                    return;
                }
                // show
                if (!_sprite.isInIndexBuffer) {
                    _sprite.AddToIndices(mesh.indices);
                    mesh.updateFlags |= UpdateFlags.Index;
                }

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying) {
                    mesh.Apply();
                }
#endif
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
                bool hasSprite = mesh.spriteList.Contains(_sprite);
                if (!hasSprite) {
                    Debug.LogError("can't find sprite to hide");
                    return;
                }
                // hide
                if (_sprite.isInIndexBuffer) {
                    RemoveIndices(mesh, _sprite);
                    mesh.updateFlags |= UpdateFlags.Index;
                }
                exDebug.Assert(_sprite.indexBufferIndex == -1);

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying) {
                    mesh.Apply();
                }
#endif
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

    // ------------------------------------------------------------------ 
    /// \z      限定整个layer的World BoundingBox的最小z值
    /// \return 整个layer的World BoundingBox的最大z值
    /// 
    /// 设置layer的深度，用于layer之间的排序
    // ------------------------------------------------------------------ 

    public float SetWorldBoundsMinZ (float z) {
        for (int i = 0; i < meshList.Count; ++i) {
            exMesh mesh = meshList[i];
            mesh.transform.position = new Vector3(0, 0, z);
        }
        return z;
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
		    if (mesh != null && object.ReferenceEquals(mesh.material, mat)) {
                bool containsSprite = (_sprite.spriteIndex >= 0 && _sprite.spriteIndex < mesh.spriteList.Count && 
                                      ReferenceEquals(mesh.spriteList[_sprite.spriteIndex], _sprite));
#if EX_DEBUG
                exDebug.Assert(containsSprite == mesh.spriteList.Contains(_sprite), "wrong sprite.spriteIndex");
                bool sameMaterial = (_sprite.material == mesh.material);
                exDebug.Assert(!containsSprite || sameMaterial);
#endif
                if (containsSprite) {
                    return mesh;
                }
		    }
        }
        return null;
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to the mesh. 
    // ------------------------------------------------------------------ 

    private void Add (exMesh _mesh, exSpriteBase _sprite) {
        bool hasSprite = _mesh.spriteList.Contains(_sprite);
        if (hasSprite) {
            Debug.LogError("[Add|exLayer] can't add duplicated sprite");
            return;
        }

        _sprite.spriteIndex = _mesh.spriteList.Count;
        _mesh.spriteList.Add(_sprite);

        UpdateFlags spriteUpdateFlags = _sprite.FillBuffers(_mesh.vertices, _mesh.indices, _mesh.uvs, _mesh.colors32);
        _mesh.updateFlags |= spriteUpdateFlags;
        
        exDebug.Assert(_mesh.vertices.Count == _mesh.uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(_mesh.vertices.Count == _mesh.colors32.Count, "colors32 array needs to be the same size as the vertices array");

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            _mesh.Apply();
        }
#endif
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void Remove (exMesh _mesh, exSpriteBase _sprite) {
        bool hasSprite = _mesh.spriteList.Contains(_sprite);
        if (!hasSprite) {
            Debug.LogError("can't find sprite to remove");
            return;
        }
        _mesh.spriteList.RemoveAt(_sprite.spriteIndex);
        
        for (int i = _sprite.spriteIndex; i < _mesh.spriteList.Count; ++i) {
            exSpriteBase sprite = _mesh.spriteList[i];
            // update sprite and vertic index after removed sprite
            sprite.spriteIndex = i;
            sprite.vertexBufferIndex -= _sprite.vertexCount;
            // update indices to make them match new vertic index
            if (sprite.isInIndexBuffer) {
                for (int index = sprite.indexBufferIndex; index < sprite.indexBufferIndex + sprite.indexCount; ++index) {
                    _mesh.indices[index] -= _sprite.vertexCount;
                }
            }
        }
        _mesh.updateFlags |= UpdateFlags.VertexAndIndex;

        // update vertices
        _mesh.vertices.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);
        _mesh.colors32.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);
        _mesh.uvs.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);

#if FORCE_UPDATE_VERTEX_INFO
        bool removeBack = (_sprite.spriteIndex == _mesh.spriteList.Count);
        if (!removeBack) {
            _mesh.updateFlags |= (UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);
        }
#else
        _mesh.updateFlags |= (UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);
#endif

        if (_sprite.isInIndexBuffer) {
            RemoveIndices(_mesh, _sprite);
        }

        exDebug.Assert(_sprite.indexBufferIndex == -1);
        exDebug.Assert(_mesh.vertices.Count == _mesh.uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(_mesh.vertices.Count == _mesh.colors32.Count, "colors32 array needs to be the same size as the vertices array");

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            _mesh.Apply();
        }
#endif
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void RemoveIndices (exMesh _mesh, exSpriteBase _sprite) {
        exDebug.Assert(_sprite.isInIndexBuffer);
        if (_sprite.isInIndexBuffer) {
            // update indices
            _mesh.indices.RemoveRange(_sprite.indexBufferIndex, _sprite.indexCount);
            _mesh.updateFlags |= UpdateFlags.Index;
            
            // update indexBufferIndex
            // TODO: 这里是性能瓶颈，应该设法优化
            for (int i = 0; i < _mesh.spriteList.Count; ++i) {
                exSpriteBase sprite = _mesh.spriteList[i];
                if (sprite.indexBufferIndex > _sprite.indexBufferIndex) {
                    sprite.indexBufferIndex -= _sprite.indexCount;
                    exDebug.Assert(sprite.indexBufferIndex >= _sprite.indexBufferIndex);
                }
            }
            _sprite.indexBufferIndex = -1;
        }
    }
}
