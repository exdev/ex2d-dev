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

public enum exLayerType
{
    Static = 0,
    Dynamic,    ///< 当layerType转换成dynamic后，新添加的sprite时将判断mesh顶点数，超出限制的将自动添加到新的mesh中。
}

///////////////////////////////////////////////////////////////////////////////
//
/// The layer component
/// NOTE: Don't add this component yourself, use ex2DMng.instance.CreateLayer instead.
//
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
public class exLayer : MonoBehaviour
{
    const int MAX_DYNAMIC_VERTEX_COUNT = 300;    ///< 超过这个数量的话，dynamic layer将会自动进行拆分
    const int MAX_STATIC_VERTEX_COUNT = 65000;   ///< 超过这个数量的话，static layer将会自动进行拆分
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// show/hide layer and all its sprites.
    // ------------------------------------------------------------------ 
    
    [HideInInspector] [SerializeField] 
    private bool show_ = true;
    public bool show {
        get {
            return show_;
        }
        set { 
            if (show_ == value) {
                return;
            }
            for (int i = 0; i < meshList.Count; ++i) {
                exMesh mesh = meshList[i];
                if (mesh != null) {
                    mesh.gameObject.SetActive(value && mesh.hasTriangle);
                }
            }
            show_ = value;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    [HideInInspector] [SerializeField] 
    private exLayerType layerType_ = exLayerType.Dynamic;
    public exLayerType layerType {
        get {
            return layerType_;
        }
        set {
            if (layerType_ == value) {
                return;
            }
            layerType_ = value;
#if UNITY_EDITOR
            if (value == exLayerType.Static && Application.isPlaying) {
                Debug.LogWarning("can't change to static during runtime");
            }
#endif
            if (value == exLayerType.Dynamic) {
                for (int i = 0; i < meshList.Count; ++i) {
                    meshList[i].MarkDynamic();
                }
            }
            else if (value == exLayerType.Static){
                Compact();
                // TODO: batch same material meshes
            }
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    private List<exMesh> meshList = new List<exMesh>(); ///< 排在前面的mesh会被先渲染

    [System.NonSerialized] private Transform cachedTransform_ = null;
    public Transform cachedTransform {
        get {
            if (ReferenceEquals(cachedTransform_, null)) {
                cachedTransform_ = transform;
            }
            return cachedTransform_;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
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
                    exDebug.Assert(sprite != null);
                    if (sprite != null) {
                        sprite.ResetLayerProperties();
                    }
                }
                mesh.Clear();
                mesh.gameObject.DestroyImmediate(); //dont save GO will auto destroy
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
        if (show_) {
            for (int m = meshList.Count - 1; m >= 0 ; --m) {
                exMesh mesh = meshList[m];
                if (mesh == null) {
                    meshList.RemoveAt(m);
                    continue;
                }
                exUpdateFlags meshUpdateFlags = exUpdateFlags.None;
                for (int i = 0; i < mesh.spriteList.Count; ++i) {
                    exSpriteBase sprite = mesh.spriteList[i];
                    exDebug.Assert(sprite.isOnEnabled == sprite.isInIndexBuffer);
                
                    if (sprite.isOnEnabled) {
                        sprite.UpdateTransform();
                        exUpdateFlags spriteUpdateFlags = sprite.UpdateBuffers(mesh.vertices, mesh.uvs, mesh.colors32, mesh.indices);
                        meshUpdateFlags |= spriteUpdateFlags;
                    }
                }
                // TODO: 如果需要排序，进行排序并且更新相关mesh的indices
                mesh.Apply(meshUpdateFlags);
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
        Material mat = _sprite.material;
        if (!mat) {
            Debug.LogError("no material assigned in sprite", _sprite);
            return;
        }
        _sprite.layer = this;
        if (_sprite.cachedTransform.IsChildOf(cachedTransform) == false) {
            _sprite.cachedTransform.parent = cachedTransform;
        }

        // Find available mesh
        // TODO: 就算材质相同，如果中间有其它材质挡着，也要拆分多个mesh
        exMesh sameDrawcallMesh = null;
        int maxVertexCount = (layerType == exLayerType.Dynamic) ? MAX_DYNAMIC_VERTEX_COUNT : MAX_STATIC_VERTEX_COUNT;
        maxVertexCount -= _sprite.vertexCount;
        for (int i = meshList.Count - 1; i >= 0; --i) {
            exMesh mesh = meshList[i];
		    if (mesh != null && mesh.material == mat && mesh.vertices.Count <= maxVertexCount) {
                //if (mesh.sortedSpriteList.Count > 0 && mesh.sortedSpriteList[mesh.sortedSpriteList.Count - 1].depth) {

                //}
                sameDrawcallMesh = meshList[i];
                break;
		    }
        }
        
        if (sameDrawcallMesh == null) {
            sameDrawcallMesh = exMesh.Create(this);
            sameDrawcallMesh.material = mat;
            if (layerType == exLayerType.Dynamic) {
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
                if (!_sprite.isInIndexBuffer) {
                    AddIndices(mesh, _sprite);
                }
                UpdateNowInEditMode();
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
                RemoveIndices(mesh, _sprite);
                mesh.updateFlags |= exUpdateFlags.Index;
                exDebug.Assert(_sprite.indexBufferIndex == -1);
                UpdateNowInEditMode();
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
            if (mesh != null) {
                mesh.transform.position = new Vector3(0, 0, z);
            }
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

        _sprite.FillBuffers(_mesh.vertices, _mesh.uvs, _mesh.colors32);
        bool show = _sprite.isOnEnabled;
        if (show) {
            AddIndices(_mesh, _sprite);
        }
        
        exDebug.Assert(_mesh.vertices.Count == _mesh.uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(_mesh.vertices.Count == _mesh.colors32.Count, "colors32 array needs to be the same size as the vertices array");

        UpdateNowInEditMode();
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void Remove (exMesh _mesh, exSpriteBase _sprite) {
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
        _mesh.updateFlags |= exUpdateFlags.VertexAndIndex;

        // update vertices
        _mesh.vertices.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);
        _mesh.colors32.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);
        _mesh.uvs.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);

#if FORCE_UPDATE_VERTEX_INFO
        bool removeBack = (_sprite.spriteIndex == _mesh.spriteList.Count);
        if (!removeBack) {
            _mesh.updateFlags |= (exUpdateFlags.Color | exUpdateFlags.UV | exUpdateFlags.Normal);
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

        UpdateNowInEditMode();
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void AddIndices (exMesh _mesh, exSpriteBase _sprite) {
        exDebug.Assert(!_sprite.isInIndexBuffer);
        if (!_sprite.isInIndexBuffer) {
            int sortedSpriteIndex;
            if (_mesh.sortedSpriteList.Count > 0) {
                sortedSpriteIndex = _mesh.sortedSpriteList.BinarySearch(_sprite);   // TODO: benchmark
                exDebug.Assert(sortedSpriteIndex < 0);  //sprite实现的比较方法决定了这种情况下不可能找到等同的排序
                if (sortedSpriteIndex < 0) {
                    // 取反获得索引
                    sortedSpriteIndex = ~sortedSpriteIndex;
                }
                if (sortedSpriteIndex >= _mesh.sortedSpriteList.Count) {
                    // this sprite's depth is biggest
                    _sprite.indexBufferIndex = _mesh.indices.Count;
#if EX_DEBUG
                    exSpriteBase lastSprite = _mesh.sortedSpriteList[_mesh.sortedSpriteList.Count - 1];
                    exDebug.Assert(_sprite.indexBufferIndex == lastSprite.indexBufferIndex + lastSprite.indexCount);
#endif
                }
                else {
                    _sprite.indexBufferIndex = _mesh.sortedSpriteList[sortedSpriteIndex].indexBufferIndex;
                }
            }
            else {
                sortedSpriteIndex = 0;
                _sprite.indexBufferIndex = 0;
            }
            // insert range into _indices
            int indexCount = _sprite.indexCount;
            if (indexCount == 6) {      // 大部分是6个
                _mesh.indices.Add(0);
                _mesh.indices.Add(0);
                _mesh.indices.Add(0);
                _mesh.indices.Add(0);
                _mesh.indices.Add(0);
                _mesh.indices.Add(0);
            }
            else {
                for (int i = 0; i < indexCount; ++i) {
                    _mesh.indices.Add(0);
                }
            }
            for (int i = _mesh.indices.Count - 1 - indexCount; i >= _sprite.indexBufferIndex ; --i) {
                _mesh.indices[i + indexCount] = _mesh.indices[i];
            }
            _sprite.updateFlags |= exUpdateFlags.Index;
            // update other sprites indexBufferIndex
            for (int i = sortedSpriteIndex; i < _mesh.sortedSpriteList.Count; ++i) {
                exSpriteBase otherSprite = _mesh.sortedSpriteList[i];
                otherSprite.indexBufferIndex += indexCount;
            }
            // insert into _sortedSpriteList
            _mesh.sortedSpriteList.Insert(sortedSpriteIndex, _sprite);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void RemoveIndices (exMesh _mesh, exSpriteBase _sprite) {
        exDebug.Assert(_sprite.isInIndexBuffer);
        if (_sprite.isInIndexBuffer) {
            // update indices
            _mesh.indices.RemoveRange(_sprite.indexBufferIndex, _sprite.indexCount);
            _mesh.updateFlags |= exUpdateFlags.Index;
            
            // update indexBufferIndex and sortedSpriteList
            for (int i = _mesh.sortedSpriteList.Count - 1; i >= 0; --i) {
                exSpriteBase otherSprite = _mesh.sortedSpriteList[i];
                if (otherSprite.indexBufferIndex > _sprite.indexBufferIndex) {
                    otherSprite.indexBufferIndex -= _sprite.indexCount;
                    exDebug.Assert(otherSprite.indexBufferIndex >= _sprite.indexBufferIndex);
                }
                else {
                    exDebug.Assert(otherSprite == _sprite);
                    _mesh.sortedSpriteList.RemoveAt(i);
                    break;
                }
            }
            _sprite.indexBufferIndex = -1;
        }
    }

    // ------------------------------------------------------------------ 
    /// To update scene view in edit mode immediately
    /// 所有方法调用，及用作调用参数的表达式都不会被编译进*非*EX_DEBUG的版本
    // ------------------------------------------------------------------ 

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void UpdateNowInEditMode () {
        if (UnityEditor.EditorApplication.isPlaying == false) {
            UpdateSprites();
        }
    }
}
