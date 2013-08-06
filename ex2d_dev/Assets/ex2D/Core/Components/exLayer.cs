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
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// show/hide layer and all its sprites.
    // ------------------------------------------------------------------ 
    
    [SerializeField] 
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
    
    [SerializeField] 
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
            bool dynamic = (value == exLayerType.Dynamic);
            for (int i = 0; i < meshList.Count; ++i) {
                meshList[i].SetDynamic(dynamic);
            }
            if (value == exLayerType.Static){
                Compact();
                // TODO: batch same material meshes
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    /// the layer's alpha affects every sprites drawn by the layer.
    // ------------------------------------------------------------------ 
    
    [SerializeField] 
    private float alpha_ = 1.0f;
    public float alpha {
        get {
            return alpha_;
        }
        set { 
            if (alpha_ == value) {
                return;
            }
            alpha_ = value;
            alphaHasChanged = true;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] private List<exMesh> meshList = new List<exMesh>(); ///< 排在前面的mesh会被先渲染

    [System.NonSerialized] private Transform cachedTransform_ = null;
    public Transform cachedTransform {
        get {
            if (ReferenceEquals(cachedTransform_, null)) {
                cachedTransform_ = transform;
            }
            return cachedTransform_;
        }
    }

    [System.NonSerialized] private int nextSpriteUniqueId = 0;
    [System.NonSerialized] private bool alphaHasChanged = false;

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    // If layer can be standalone, we should check whether the layer belongs to any 2D Manager, otherwise we need to call GenerateMeshes when OnEnable.

    /// \NOTE You should not deactivate the layer manually. 
    ///       If you want to change the visibility of the whole layer, you should set its show property.
    void OnDisable () {
        DestroyMeshes();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Maintains meshes to render all sprites
    // ------------------------------------------------------------------ 
 
    [ContextMenu("UpdateSprites")]
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
                    if (alphaHasChanged) {
                        sprite.updateFlags |= exUpdateFlags.Color;
                    }
                    exDebug.Assert(sprite.isInIndexBuffer == sprite.visible);
                    if (sprite.isInIndexBuffer) {
                        sprite.UpdateTransform();
                        exUpdateFlags spriteUpdateFlags = sprite.UpdateBuffers(mesh.vertices, mesh.uvs, mesh.colors32, mesh.indices);
                        meshUpdateFlags |= spriteUpdateFlags;
                    }
                }
                // TODO: 如果需要排序，进行排序并且更新相关mesh的indices
                mesh.Apply(meshUpdateFlags);
            }
            alphaHasChanged = false;
        }
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to this layer. 
    /// If sprite is disabled, it will keep invisible until you enable it.
    /// NOTE: You can also use exSpriteBase.SetLayer for convenience.
    // ------------------------------------------------------------------ 

    public void Add (exSpriteBase _sprite) {
        Add(_sprite, true);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _sprite) {
        exSpriteBase[] spritesToRemove = _sprite.GetComponentsInChildren<exSpriteBase>(true);
        for (int i = 0; i < spritesToRemove.Length; ++i) {
            exSpriteBase sprite = spritesToRemove[i];
            if (sprite.layer != this) {
                Debug.LogWarning("Sprite not in this layer.");
                return;
            }
            exMesh mesh = GetMesh(sprite);
            if (mesh != null) {
                RemoveFromMesh(sprite, mesh);
                sprite.layer = null;
            }
            else {
                sprite.ResetLayerProperties();  //if mesh has been destroyed, just reset sprite
            }
            if (sprite.spriteIdInLayer == nextSpriteUniqueId - 1) {
                --nextSpriteUniqueId;
            }
            sprite.spriteIdInLayer = 0;	
        }
        UpdateNowInEditMode();
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exSpriteBase
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    internal void ShowSprite (exSpriteBase _sprite) {
        if (!_sprite.isInIndexBuffer) {
            exMesh mesh = GetMesh(_sprite);
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
            exMesh mesh = GetMesh(_sprite);
            if (mesh != null) {
                RemoveIndices(mesh, _sprite);
                exDebug.Assert(_sprite.indexBufferIndex == -1);
                UpdateNowInEditMode();
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Compact () {
        float meshZ = 0.0f;
        for (int i = 0; i < meshList.Count; ++i) {
            exMesh mesh = meshList[i];
            if (mesh != null) {
                meshZ = mesh.transform.position.z;
                break;
            }
        }

        DestroyMeshes();
        GenerateMeshes();
        meshList.TrimExcess();

        for (int i = 0; i < meshList.Count; ++i) {
            exMesh mesh = meshList[i];
            if (mesh != null) {
                mesh.transform.position = new Vector3(0, 0, meshZ);
            }
        }
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
        
    // ------------------------------------------------------------------ 
    /// Desc:
    // ------------------------------------------------------------------ 
        
    public void GenerateMeshes () {
        nextSpriteUniqueId = 0;
        exSpriteBase[] spriteList = GetComponentsInChildren<exSpriteBase>();
        foreach (exSpriteBase sprite in spriteList) {
            Add(sprite, false);
        }
    }

    // ------------------------------------------------------------------ 
    /// Desc:
    // ------------------------------------------------------------------ 
        
    public void DestroyMeshes () {
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
    

    // ------------------------------------------------------------------ 
    /// To update scene view in edit mode immediately
    /// 所有方法调用，及用作调用参数的表达式都不会被编译进*非*UNITY_EDITOR的版本
    // ------------------------------------------------------------------ 

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void UpdateNowInEditMode () {
#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying == false) {
            UpdateSprites();
        }
#endif
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private exMesh GetMesh (exSpriteBase _sprite) {
        Material mat = _sprite.material;
        for (int i = 0; i < meshList.Count; ++i) {
            exMesh mesh = meshList[i];
            if (mesh != null && object.ReferenceEquals(mesh.material, mat)) {
                bool containsSprite = (_sprite.spriteIndexInMesh >= 0 && _sprite.spriteIndexInMesh < mesh.spriteList.Count && 
                                      ReferenceEquals(mesh.spriteList[_sprite.spriteIndexInMesh], _sprite));
                exDebug.Assert(containsSprite == mesh.spriteList.Contains(_sprite), "wrong sprite.spriteIndex");
                if (containsSprite) {
                    return mesh;
                }
            }
        }
        return null;
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private exMesh CreateNewMesh (Material _mat) {
        exMesh mesh = exMesh.Create(this);
        mesh.material = _mat;
        mesh.SetDynamic(layerType_ == exLayerType.Dynamic);
        meshList.Add(mesh);
        ex2DMng.instance.ResortLayerDepth();
        return mesh;
    }
    

    // ------------------------------------------------------------------ 
    /// To update scene view in edit mode immediately
    /// 所有方法调用，及用作调用参数的表达式都不会被编译进*非*UNITY_EDITOR的版本
    // ------------------------------------------------------------------ 

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void CheckDuplicated (exSpriteBase _sprite) {
#if UNITY_EDITOR
        Material mat = _sprite.material;
        for (int i = meshList.Count - 1; i >= 0; --i) {
            exMesh mesh = meshList[i];
            if (mesh != null && mesh.material == mat ) {        // TODO: check depth
                for (int j = 0; j < mesh.spriteList.Count; ++j) {
                    if (_sprite.spriteIdInLayer == mesh.spriteList[j].spriteIdInLayer) {
                        _sprite.spriteIdInLayer = -1;        //duplicated
                        break;
                    }
                }
            }
        }
#endif
    }
    
    // ------------------------------------------------------------------ 
    /// \param _newSprite 如果为true，则将sprite渲染到其它相同depth的sprite上面
    // ------------------------------------------------------------------ 
    
    private void Add (exSpriteBase _sprite, bool _newSprite) {
        exLayer oldLayer = _sprite.layer;
        if (oldLayer == this) {
            return;
        }
        if (oldLayer != null) {
            oldLayer.Remove(_sprite);
        }

        exSpriteBase[] spritesToAdd = _sprite.GetComponentsInChildren<exSpriteBase>(true);
        for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
            exSpriteBase sprite = spritesToAdd[spriteIndex];
            
            Material mat = sprite.material;
            if (mat == null) {
                Debug.LogError("no material assigned in sprite", sprite);
                continue;
            }

            sprite.layer = this;

            // Check sprite id
            CheckDuplicated(sprite);
            if (_newSprite || sprite.spriteIdInLayer == -1) {
                sprite.spriteIdInLayer = nextSpriteUniqueId;
                ++nextSpriteUniqueId;
            }
            else {
                nextSpriteUniqueId = Mathf.Max(sprite.spriteIdInLayer + 1, nextSpriteUniqueId);
            }
    
            // Find available mesh
            // TODO: 就算材质相同，如果中间有其它材质挡着，也要拆分多个mesh
            exMesh sameDrawcallMesh = null;
            int maxVertexCount = (layerType_ == exLayerType.Dynamic) ? MAX_DYNAMIC_VERTEX_COUNT : exMesh.MAX_VERTEX_COUNT;
            maxVertexCount -= sprite.vertexCount;
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
                sameDrawcallMesh = CreateNewMesh(mat);
            }
            AddToMesh(sprite, sameDrawcallMesh);
        }
        if (_sprite.cachedTransform.IsChildOf(cachedTransform) == false) {
            _sprite.cachedTransform.parent = cachedTransform_;
        }
        UpdateNowInEditMode();
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to the mesh. 
    // ------------------------------------------------------------------ 

    private void AddToMesh (exSpriteBase _sprite, exMesh _mesh) {
        bool hasSprite = _mesh.spriteList.Contains(_sprite);
        if (hasSprite) {
            Debug.LogError("[Add|exLayer] can't add duplicated sprite");
            return;
        }
        _sprite.updateFlags = exUpdateFlags.None;
        _sprite.spriteIndexInMesh = _mesh.spriteList.Count;
        _mesh.spriteList.Add(_sprite);

        _sprite.FillBuffers(_mesh.vertices, _mesh.uvs, _mesh.colors32);
        if (_sprite.visible) {
            AddIndices(_mesh, _sprite);
        }
        
        exDebug.Assert(_mesh.vertices.Count == _mesh.uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(_mesh.vertices.Count == _mesh.colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void RemoveFromMesh (exSpriteBase _sprite, exMesh _mesh) {
        _mesh.spriteList.RemoveAt(_sprite.spriteIndexInMesh);
        for (int i = _sprite.spriteIndexInMesh; i < _mesh.spriteList.Count; ++i) {
            exSpriteBase sprite = _mesh.spriteList[i];
            // update sprite and vertic index after removed sprite
            sprite.spriteIndexInMesh = i;
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
        bool removeLastSprite = (_sprite.spriteIndexInMesh == _mesh.spriteList.Count);
        if (!removeLastSprite) {
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
                exDebug.Assert(sortedSpriteIndex < 0, sortedSpriteIndex.ToString());  //sprite实现的比较方法决定了这种情况下不可能找到等同的排序
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
}