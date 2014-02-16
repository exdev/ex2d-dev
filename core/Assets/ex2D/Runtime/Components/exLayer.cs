// ======================================================================================
// File         : exLayer.cs
// Author       : 
// Last Change  : 08/17/2013 | 15:27:56
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
/// NOTE: Don't add this component yourself, use ex2DRenderer.instance.CreateLayer instead.
//
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
public class exLayer : MonoBehaviour
{
    public static int maxDynamicMeshVertex = 90000;    ///< 超过这个数量的话，dynamic layer将会自动进行拆分

    // ------------------------------------------------------------------ 
    /// 该接口只能显式实现，用于限制对exLayeredSprite私有方法的访问
    // ------------------------------------------------------------------ 

    public interface IFriendOfLayer {
        float globalDepth { get; set; }
        //void DoSetDepth (float _depth);
        void DoSetBufferSize (int _vertexCount, int _indexCount);
        void SetMaterialDirty ();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    struct UpdateSpriteWhileRecreating {

        private static exLayeredSprite spriteNeedsToSetBufferSize = null;
        private static int newVertexCount;
        private static int newIndexCount;

        private static exLayeredSprite spriteNeedsToSetMaterialDirty = null;

        // ------------------------------------------------------------------ 
        // Desc:
        // ------------------------------------------------------------------ 

        public static void RegisterNewBufferSize (exLayeredSprite _sprite, int _vertexCount, int _indexCount) {
            spriteNeedsToSetBufferSize = _sprite;
            newVertexCount = _vertexCount;
            newIndexCount = _indexCount;
        }

        // ------------------------------------------------------------------ 
        // Desc:
        // ------------------------------------------------------------------ 

        public static void RegisterMaterialDirty (exLayeredSprite _sprite) {
            spriteNeedsToSetMaterialDirty = _sprite;
        }

        // ------------------------------------------------------------------ 
        // Desc:
        // ------------------------------------------------------------------ 

        public static void TryUpdate (exLayeredSprite _sprite) {
            if (ReferenceEquals(_sprite, spriteNeedsToSetBufferSize)) {
                (spriteNeedsToSetBufferSize as IFriendOfLayer).DoSetBufferSize(newVertexCount, newIndexCount);
                spriteNeedsToSetBufferSize = null;
            }
            if (ReferenceEquals(_sprite, spriteNeedsToSetMaterialDirty)) {
                (spriteNeedsToSetMaterialDirty as IFriendOfLayer).SetMaterialDirty();
                spriteNeedsToSetMaterialDirty = null;
            }
        }

        // ------------------------------------------------------------------ 
        // Desc:
        // ------------------------------------------------------------------ 

        public static void Clear () {
            spriteNeedsToSetBufferSize = null;
            spriteNeedsToSetMaterialDirty = null;
        }
    }
    
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
    private bool ordered_ = true;
    public bool ordered {
        get {
            return ordered_;
        }
        set { 
            if (ordered_ == value) {
                return;
            }
            ordered_ = value;
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
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    // TODO: save z even if not using custom z
    [SerializeField] 
    private float zMin_;
    public float customZ {
        get {
            return zMin_;
        }
        set {
            if (zMin_ == value) {
                return;
            }
            zMin_ = value;
            SetWorldBoundsMinZ(zMin_);
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] public List<exMesh> meshList = new List<exMesh>(); ///< 排在前面的mesh会被先渲染

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

    /// 缓存需要更改depth的sprite，在渲染前统一重新计算它们及所有子sprite的depth。
    /// 不立刻更新的原因主要是为了能够批量一次性刷新，以节约遍历hierarchy及多次设置depth的开销。
    /// 如果SetDirty用的go包含sprite，则放在这个list
    [System.NonSerialized] private List<exLayeredSprite> depthDirtySpriteList = new List<exLayeredSprite>();

    /// 缓存包含需要更改depth的sprite的go，在渲染前统一重新计算它们及所有子sprite的depth。
    /// 不立刻更新的原因主要是为了能够批量一次性刷新，以节约遍历hierarchy及多次设置depth的开销。
    /// 如果SetDirty用的go不包含sprite，则放在这个list
    [System.NonSerialized] private List<GameObject> depthDirtyGoList = new List<GameObject>();

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    /// \NOTE You should not deactivate the layer manually. 
    ///       If you want to change the visibility of the whole layer, you should set its show property.
    void OnDisable () {
        DestroyMeshes();
    }

    // If layer can be standalone, we should check whether the layer belongs to any 2D Renderer, 
    // otherwise we need to call GenerateMeshes when OnEnable.

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Maintains meshes to render all sprites
    // ------------------------------------------------------------------ 
 
    [ContextMenu("UpdateSprites")]
    public void UpdateSprites () {
        if (show_ == false) {
            return;
        }

        // Remove null sprites
        // 有可能是子sprite被父sprite一起加入了layer中，但从未被激活过，导致OnDestroy没有调用到，所以必须先判断是否为空
        for (int m = meshList.Count - 1; m >= 0; --m) {
            exMesh mesh = meshList[m];
            for (int i = 0; i < mesh.spriteList.Count; ++i) {
                exLayeredSprite sprite = mesh.spriteList[i];
                if (sprite == null) {
                    exDebug.Assert(ReferenceEquals(sprite, null) == false);
                    RemoveFromMesh (sprite, mesh);
                }
            }
        }

        // compact mesh
        for (int m = 0; m < meshList.Count; ) {
            exMesh mesh = meshList[m];
            bool meshDestroyed = (mesh == null);
            if (meshDestroyed || mesh.spriteList.Count == 0) {
                if (meshDestroyed == false) {
                    mesh.gameObject.Destroy ();
                }
                meshList.RemoveAt (m);
                if (m - 1 >= 0 && m < meshList.Count) {
                    int maxVertexCount = (layerType_ == exLayerType.Dynamic) ? maxDynamicMeshVertex : exMesh.MAX_VERTEX_COUNT;
                    if (meshList [m - 1].vertices.Count < maxVertexCount) {
                        ShiftSpritesDown (m - 1, maxVertexCount, maxVertexCount);
                    }
                }
            }
            else {
                ++m;
            }
        }

        // update depth
        UpdateAllSpritesDepth();

        // update
        for (int m = meshList.Count - 1; m >= 0 ; --m) {
            exMesh mesh = meshList[m];
            exUpdateFlags meshUpdateFlags = exUpdateFlags.None;
            for (int i = 0; i < mesh.spriteList.Count; ++i) {
                exLayeredSprite sprite = mesh.spriteList[i];
                if (alphaHasChanged) {
                    sprite.updateFlags |= exUpdateFlags.Color;
                }
                if (sprite.isInIndexBuffer) {
                    //if (sprite.transparent == false) {
                        sprite.UpdateTransform();
                    //}
                    if (sprite.updateFlags != exUpdateFlags.None) {
                        exUpdateFlags spriteUpdateFlags = sprite.UpdateBuffers(mesh.vertices, mesh.uvs, mesh.colors32, mesh.indices);
                        meshUpdateFlags |= spriteUpdateFlags;
                    }
                }
            }
            mesh.Apply(meshUpdateFlags);
        }
        alphaHasChanged = false;
    }

    // ------------------------------------------------------------------ 
    /// Add an exLayeredSprite to this layer. 
    /// If sprite is disabled, it will keep invisible until you enable it.
    /// \param _recursively Also add all sprites in the hierarchy
    /// NOTE: You can also use exLayeredSprite.SetLayer for convenience.
    // ------------------------------------------------------------------ 

    public void Add (exLayeredSprite _sprite, bool _recursively = true) {
        if (_recursively == true) {
            exLayer oldLayer = _sprite.layer;
            if (ReferenceEquals (oldLayer, this)) {
                return;
            }
            if (oldLayer != null) {
                oldLayer.Remove (_sprite, true);
            }

            exLayeredSprite[] spritesToAdd = _sprite.GetComponentsInChildren<exLayeredSprite> (true);
            for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
                DoAddSprite (spritesToAdd [spriteIndex], true);
            }
            if (_sprite.cachedTransform.IsChildOf (cachedTransform) == false) {
                _sprite.cachedTransform.parent = cachedTransform_;
            }
            UpdateNowInEditMode ();
        }
        else {
            DoAddSprite (_sprite, true);
        }
    }

    // ------------------------------------------------------------------ 
    /// 将一个Go及所有子物体中的sprite添加到layer中
    /// NOTE: 如果Go及其父物体都不是layer的子物体，Go的父物体将被设置为layer
    // ------------------------------------------------------------------ 

    public void Add (GameObject _gameObject) {
        exLayeredSprite[] spritesToAdd = _gameObject.GetComponentsInChildren<exLayeredSprite>(true);
        for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
            DoAddSprite(spritesToAdd[spriteIndex], true);
        }
        if (_gameObject.transform.IsChildOf(cachedTransform) == false) {
            _gameObject.transform.parent = cachedTransform_;
        }
        UpdateNowInEditMode();
    }

    // ------------------------------------------------------------------ 
    /// \param _recursively Also remove all sprites in the hierarchy
    // ------------------------------------------------------------------ 

    public void Remove (exLayeredSprite _sprite, bool _recursively = true) {
        if (_recursively) {
            Remove(_sprite.gameObject, _recursively);
        }
        else {
            if (_sprite.layer != this) {
                Debug.LogWarning ("Sprite not in this layer.");
                return;
            }
            int meshIndex = IndexOfMesh (_sprite);
            if (meshIndex != -1) {
                RemoveFromMesh (_sprite, meshList [meshIndex]);
                _sprite.layer = null;
            }
            else {
                _sprite.ResetLayerProperties ();  //if mesh has been destroyed, just reset sprite
            }
            if (_sprite.spriteIdInLayer == nextSpriteUniqueId - 1 && nextSpriteUniqueId > 0) {
                --nextSpriteUniqueId;
            }
            _sprite.spriteIdInLayer = 0;
        }
    }
    
    // ------------------------------------------------------------------ 
    /// \param _recursively Also remove all sprites in the hierarchy
    // ------------------------------------------------------------------ 

    public void Remove (GameObject _gameObject, bool _recursively = true) {
        if (_recursively) {
            exLayeredSprite[] spritesToRemove = _gameObject.GetComponentsInChildren<exLayeredSprite> (true);
            for (int i = 0; i < spritesToRemove.Length; ++i) {
                Remove (spritesToRemove [i], false);
            }
            UpdateNowInEditMode ();
        } 
        else {
            exLayeredSprite sprite = _gameObject.GetComponent<exLayeredSprite> ();
            if (sprite != null) {
                Remove (sprite, false);
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exLayeredSprite
    /// NOTE: This function should only be called by exLayeredSprite
    // ------------------------------------------------------------------ 

    internal void ShowSprite (exLayeredSprite _sprite) {
        if (_sprite.isInIndexBuffer == false) {
            int meshIndex = IndexOfMesh(_sprite);
            if (meshIndex != -1) {
                AddIndices(meshList[meshIndex], _sprite);
                UpdateNowInEditMode();
            }
        }
        else {
            _sprite.transparent = false;
            UpdateNowInEditMode();
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    internal void HideSprite (exLayeredSprite _sprite) {
        exDebug.Assert(false, "GetMeshToAdd要获取mesh中最先和最后渲染的sprite，要保证sprite都在sortedSpriteList中");
        if (_sprite.isInIndexBuffer) {
            int meshIndex = IndexOfMesh(_sprite);
            if (meshIndex != -1) {
                RemoveIndices(meshList[meshIndex], _sprite);
                exDebug.Assert(_sprite.indexBufferIndex == -1);
                UpdateNowInEditMode();
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal void FastShowSprite (exLayeredSprite _sprite) {
        if (_sprite.isInIndexBuffer == false) {
            ShowSprite (_sprite);
        }
        else {
            _sprite.transparent = false;
            UpdateNowInEditMode();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal void FastHideSprite (exLayeredSprite _sprite) {
        _sprite.transparent = true;
        UpdateNowInEditMode();
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
        float interval = 0.01f;
        for (int i = meshList.Count - 1; i >= 0; --i) {
            exMesh mesh = meshList[i];
            if (mesh != null) {
                mesh.transform.position = new Vector3(0, 0, z);
                z += interval;
            }
        }
        return z;
    }
        
    // ------------------------------------------------------------------ 
    /// 如果Hierarchy中有未加入到layer中的sprite，初始化他们。
    // ------------------------------------------------------------------ 
        
    public void GenerateMeshes () {
        exLayeredSprite[] spriteList = GetComponentsInChildren<exLayeredSprite>(true);
        foreach (exLayeredSprite sprite in spriteList) {
            if (ReferenceEquals(sprite.layer, this) == false) {
                DoAddSprite(sprite, false);
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// Desc:
    // ------------------------------------------------------------------ 
        
    public void DestroyMeshes () {
        //exLayeredSprite[] spriteList = GetComponentsInChildren<exLayeredSprite>();
        //foreach (exLayeredSprite sprite in spriteList) {
        //    // reset sprite
        //    sprite.indexBufferIndex = -1;
        //    sprite.layer = null;
        //}
        exLayeredSprite[] spriteList = GetComponentsInChildren<exLayeredSprite>(true);
        foreach (exLayeredSprite sprite in spriteList) {
            if (sprite != null) {
                if (sprite.layer != null && ReferenceEquals(sprite.layer, this) == false) {
                    Debug.LogError("Sprite's hierarchy is invalid!", sprite);
                }
                sprite.ResetLayerProperties();
            }
        }
        for (int i = meshList.Count - 1; i >= 0; --i) {
            exMesh mesh = meshList[i];
            if (mesh != null) {
                // 这里不应该使用Destroy，因为当停止执行editor的当时，Destroy不会根据editor下的行为自动使用DestroyImmediate，会导致layer的mesh无法被销毁的情况出现
                mesh.gameObject.DestroyImmediate();
            }
        }
        meshList.Clear();
        nextSpriteUniqueId = 0;
    }

    // ------------------------------------------------------------------ 
    /// 如果在运行时改变了GameObject的parent，而这个GameObject又包含Sprite的话，则需要手动调用这个方法以请求重新计算该GameObject的所有子Sprite的depth，不论这个Gameobject的parent之前是否已经调用过这个方法。
    /// 只需要对改变了parent的GameObject调用即可，不需要对它的子sprite调用。
    // ------------------------------------------------------------------ 
    
    public void SetDepthDirty (GameObject _go) {
        exLayeredSprite sprite = _go.GetComponent(typeof(exLayeredSprite)) as exLayeredSprite;
        if (sprite != null) {
            // 直接存sprite更快
            SetDepthDirty(sprite);
        }
        else {
            // 确保list里所有的go及子树不相互包含，避免重复计算depth
            Transform myTransform = _go.transform;
            for (int i = depthDirtyGoList.Count - 1; i >= 0; --i) {
                GameObject other = depthDirtyGoList[i];
                if (ReferenceEquals(other, _go)) {
                    return;
                }
                if (other == null) {
                    depthDirtyGoList.RemoveAt(i);
                    continue;
                }
                Transform otherTransform = other.transform;
                bool alreadyBeContainedInOthersHierarchy = myTransform.IsChildOf(otherTransform);
                if (alreadyBeContainedInOthersHierarchy) {
                    return;
                }
                bool otherIsChild = otherTransform.IsChildOf(myTransform);
                if (otherIsChild) {
                    depthDirtyGoList.RemoveAt(i);
                }
            }
            // 集中缓存到一个list里，待渲染前再统一重新计算
            depthDirtyGoList.Add(_go);
        }
    }

    // ------------------------------------------------------------------ 
    /// 如果在运行时改变了sprite的parent，需要手动调用这个方法以请求重新计算该Sprite和所有子Sprite的depth，不论它的parent之前是否已经调用过这个方法。
    /// 只需要对改变了parent的sprite调用即可，不需要对它的子sprite调用。
    // ------------------------------------------------------------------ 
    
    public void SetDepthDirty (exLayeredSprite _sprite) {
        SetDepthDirtyFlag(_sprite, true);

        // 确保list里所有的sprite及子树不相互包含，避免重复计算depth
        Transform spriteTransform = _sprite.cachedTransform;
        for (int i = depthDirtySpriteList.Count - 1; i >= 0; --i) {
            exLayeredSprite other = depthDirtySpriteList[i];
            if (ReferenceEquals(other, _sprite)) {
                return;
            }
            if (other == null) {
                depthDirtySpriteList.RemoveAt(i);
                continue;
            }
            bool alreadyBeContainedInOthersHierarchy = spriteTransform.IsChildOf(other.cachedTransform);
            if (alreadyBeContainedInOthersHierarchy) {
                return;
            }
            bool otherIsChild = other.cachedTransform.IsChildOf(spriteTransform);
            if (otherIsChild) {
                depthDirtySpriteList.RemoveAt(i);
                other.updateFlags &= ~exUpdateFlags.SelfDepth;
            }
        }
        // 集中缓存到一个list里，待渲染前再统一重新计算
        depthDirtySpriteList.Add(_sprite);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void SetDepthDirtyFlag (exLayeredSprite _sprite, bool _dirty) {
        if (_dirty) {
            _sprite.updateFlags |= exUpdateFlags.SelfDepth;
        }
        else {
            _sprite.updateFlags &= ~exUpdateFlags.SelfDepth;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    private void UpdateSpriteDepth (exLayeredSprite _sprite, float _parentGlobalDepth) {
        // caculate global depth
        float newGlobalDepth = _parentGlobalDepth + _sprite.depth;
        if ((_sprite as IFriendOfLayer).globalDepth == newGlobalDepth) {
            //Debug.Log("[UpdateSpriteDepth|exLayer] return");
            return;
        }
        (_sprite as IFriendOfLayer).globalDepth = newGlobalDepth;
        // update mesh
        int oldMeshIndex = IndexOfMesh (_sprite);
        exDebug.Assert(oldMeshIndex != -1);
        exMesh mesh = meshList[oldMeshIndex];
        if (IsRenderOrderChangedAmongMeshes(_sprite, oldMeshIndex)) {
            //Debug.Log(string.Format("[UpdateSpriteDepth|exLayer] mesh: {0}", mesh));
            RemoveFromMesh(_sprite, mesh); // 这里需要保证depth改变后也能正常remove
            UpdateSpriteWhileRecreating.TryUpdate(_sprite);
            AddToMesh(_sprite, GetMeshToAdd(_sprite));
        }
        else {
            // 这里不对查找进行优化，如果有必要可再加
            //int oldSortedSpriteIndex = mesh.sortedSpriteList.BinarySearch(_sprite);
            //exDebug.Assert(oldSortedSpriteIndex >= 0);  // 用原来的depth进行搜索，如果搜索不到，说明sortedSpriteList的排序是错的，这可能是因为值被animtion误改了。
            int oldSortedSpriteIndex = mesh.sortedSpriteList.IndexOf(_sprite);
            exDebug.Assert(oldSortedSpriteIndex >= 0);
            //
            if (IsRenderOrderChangedInMesh(_sprite, oldMeshIndex, oldSortedSpriteIndex)) {
                //Debug.Log("[UpdateSpriteDepth|exLayer] changed");
                RemoveFromMesh(_sprite, mesh); // 这里需要保证depth改变后也能正常remove
                UpdateSpriteWhileRecreating.TryUpdate(_sprite);
                AddToMesh(_sprite, mesh);
            }
            //else {
            //    Debug.Log("[UpdateSpriteDepth|exLayer] not changed");
            //}
        }
    }
    
    // ------------------------------------------------------------------ 
    // 遍历_go及所有子sprite，按深度优先更新它们的globalDepth并且刷新mesh
    // ------------------------------------------------------------------ 

    private float GetParentGlobalDepth (GameObject _go) {
        exLayeredSprite parent = _go.GetComponentUpwards<exLayeredSprite>();
        return parent != null ? (parent as IFriendOfLayer).globalDepth : 0.0f;
    }

    // ------------------------------------------------------------------ 
    // 遍历_go及所有子sprite，按深度优先更新它们的globalDepth并且刷新mesh
    // ------------------------------------------------------------------ 

    private void UpdateSpriteDepthRecursively (GameObject _go, float _parentGlobalDepth) {
        // TODO: 考虑跳过未显示的sprite
        exLayeredSprite sprite = _go.GetComponent(typeof(exLayeredSprite)) as exLayeredSprite;
        if (sprite != null) {
            UpdateSpriteDepth(sprite, _parentGlobalDepth);
        }
        float globalDepth = sprite != null ? (sprite as IFriendOfLayer).globalDepth : _parentGlobalDepth;
        Transform trans = _go.transform;
        for (int i = 0, childCount = trans.childCount; i < childCount; ++i) {
            UpdateSpriteDepthRecursively(trans.GetChild(i).gameObject, globalDepth);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal void SetSpriteBufferSize (exLayeredSprite _sprite, int _vertexCount, int _indexCount) {
        // 先更新所有depth值，如果遍历到_sprite，则更新的同时重设buffer size

        UpdateSpriteWhileRecreating.RegisterNewBufferSize(_sprite, _vertexCount, _indexCount);
        UpdateAllSpritesDepth();
        UpdateSpriteWhileRecreating.Clear();
        
        // 假如更新depth时没遍历到_sprite，则手动更新
        
        int meshIndex = IndexOfMesh(_sprite);
        if (meshIndex != -1) {
            RemoveFromMesh (_sprite, meshList[meshIndex]);
        }
        (_sprite as IFriendOfLayer).DoSetBufferSize(_vertexCount, _indexCount);
        AddToMesh(_sprite, GetMeshToAdd(_sprite));
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal void RefreshSpriteMaterial (exLayeredSprite _sprite) {
        // 先更新所有depth值，如果遍历到_sprite，则更新的同时SetMaterialDirty
        
        UpdateSpriteWhileRecreating.RegisterMaterialDirty(_sprite);
        UpdateAllSpritesDepth();
        UpdateSpriteWhileRecreating.Clear();

        // 假如更新depth时没遍历到_sprite，则手动更新
        
        int meshIndex = IndexOfMesh(_sprite);
        if (meshIndex != -1) {
            RemoveFromMesh (_sprite, meshList[meshIndex]);
        }
        (_sprite as IFriendOfLayer).SetMaterialDirty();
        AddToMesh(_sprite, GetMeshToAdd(_sprite));
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

    public int IndexOfMesh (exLayeredSprite _sprite) {
        Material mat = _sprite.material;
        for (int i = 0; i < meshList.Count; ++i) {
            exMesh mesh = meshList[i];
            if (mesh != null && object.ReferenceEquals(mesh.material, mat)) {
                bool containsSprite = (_sprite.spriteIndexInMesh >= 0 && _sprite.spriteIndexInMesh < mesh.spriteList.Count && 
                                      ReferenceEquals(mesh.spriteList[_sprite.spriteIndexInMesh], _sprite));
                //exDebug.Assert(containsSprite == mesh.spriteList.Contains(_sprite), "wrong sprite.spriteIndex");
                if (containsSprite) {
                    return i;
                }
            }
        }
        return -1;
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private exMesh CreateNewMesh (Material _mat, int _index) {
        exMesh mesh = exMesh.Create(this);
        mesh.material = _mat;
        mesh.SetDynamic(layerType_ == exLayerType.Dynamic);
        meshList.Insert(_index, mesh);
        mesh.UpdateDebugName(this);
        ex2DRenderer.instance.ResortLayerDepth();
        return mesh;
    }

    // ------------------------------------------------------------------ 
    /// 从当前mesh list里查找空的mesh，如果找到则直接拿来用，找不到则创建一个新的
    // ------------------------------------------------------------------ 

    private exMesh GetNewMesh (Material _mat, int _index) {
        for (int i = 0; i < meshList.Count; i++) {
            exMesh mesh = meshList[i];
            if (mesh != null && mesh.spriteList.Count == 0) {
                mesh.material = _mat;
                mesh.UpdateDebugName(this);
                if (i < _index) {
                    meshList.RemoveAt(i);
                    meshList.Insert(_index - 1, mesh);
                    ex2DRenderer.instance.ResortLayerDepth();
                }
                else if (i > _index) {
                    meshList.RemoveAt(i);
                    meshList.Insert(_index, mesh);
                    ex2DRenderer.instance.ResortLayerDepth();
                }
                return mesh;
            }
        }
        return CreateNewMesh(_mat, _index);
    }

    // ------------------------------------------------------------------ 
    /// To update scene view in edit mode immediately
    /// 所有方法调用，及用作调用参数的表达式都不会被编译进*非*UNITY_EDITOR的版本
    // ------------------------------------------------------------------ 

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void CheckDuplicated (exLayeredSprite _sprite) {
#if UNITY_EDITOR
        if (ordered_ == false) {
            return;
        }
        Material mat = _sprite.material;
        for (int i = meshList.Count - 1; i >= 0; --i) {
            exMesh mesh = meshList[i];
            if (mesh != null && ReferenceEquals(mesh.material, mat) ) {
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

    private void ShiftSprite (exMesh _src, exMesh _dst, exLayeredSprite _sprite) {
#if EX_DEBUG
        int oldVertexCount = _sprite.vertexCount;
#endif
        RemoveFromMesh(_sprite, _src);
#if EX_DEBUG
        exDebug.Assert (_sprite.vertexCount == oldVertexCount);
#endif
        AddToMesh(_sprite, _dst);
        SetDepthDirtyFlag(_sprite, false);    // 小优化，不需要再更新depth，因为这里已经同时把depth更新过了
#if EX_DEBUG
        exDebug.Assert (_sprite.vertexCount == oldVertexCount);
#endif
    }

    // ------------------------------------------------------------------ 
    /// Set the vertex count of the mesh by rearranging sprites between meshes
    /// \param _meshIndex the index of the mesh to set
    /// \param _newVertexCount the new vertex count of the mesh to set 由于每个sprite的vertex数量不同，这个值只能尽量达到，但一定不会超出
    /// \param _maxVertexCount the max vertex count of meshes in layer
    // ------------------------------------------------------------------ 

    private void ShiftSpritesUp (int _meshIndex, int _newVertexCount, int _maxVertexCount) {
        exDebug.Assert(_newVertexCount <= _maxVertexCount);
        exMesh mesh = meshList[_meshIndex];
        exDebug.Assert(_newVertexCount != mesh.vertices.Count);
        exDebug.Assert(mesh.vertices.Count > _newVertexCount);

        int delta = mesh.vertices.Count - _newVertexCount;
        int realDelta = 0;
        for (int i = mesh.sortedSpriteList.Count - 1; i >= 0; --i) {
    	    exLayeredSprite aboveSprite = mesh.sortedSpriteList[i];
            realDelta += aboveSprite.vertexCount;
            if (realDelta >= delta) {
                exMesh dstMesh;
                bool noAboveMesh = (_meshIndex == meshList.Count - 1 || ReferenceEquals (meshList [_meshIndex + 1].material, mesh.material) == false);
                if (noAboveMesh) {
                    dstMesh = GetNewMesh (mesh.material, _meshIndex + 1);
                    exDebug.Assert (dstMesh.vertices.Count + realDelta <= _maxVertexCount);
                }
                else {
                    dstMesh = meshList [_meshIndex + 1];
                    if (dstMesh.vertices.Count + realDelta > _maxVertexCount) {
                        ShiftSpritesUp (_meshIndex + 1, _maxVertexCount - realDelta, _maxVertexCount);
                    }
                }
                for (int shiftIndex = mesh.sortedSpriteList.Count - 1; shiftIndex >= i; --shiftIndex) {
                    ShiftSprite (mesh, dstMesh, mesh.sortedSpriteList[shiftIndex]);
                }
                return;
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// Set the vertex count of the mesh by rearranging sprites between meshes
    /// \param _meshIndex the index of the mesh to set
    /// \param _newVertexCount the new vertex count of the mesh to set 由于每个sprite的vertex数量不同，这个值只能尽量达到，但一定不会超出
    /// \param _maxVertexCount the max vertex count of meshes in layer
    // ------------------------------------------------------------------ 

    private void ShiftSpritesDown (int _meshIndex, int _newVertexCount, int _maxVertexCount) {
        exDebug.Assert(_newVertexCount <= _maxVertexCount);
        exDebug.Assert(_newVertexCount != meshList[_meshIndex].vertices.Count);
        exDebug.Assert(meshList[_meshIndex].vertices.Count <= _newVertexCount);

        // shift sprites from above mesh to this mesh, and compact all same material meshes above this one
        for (int i = _meshIndex + 1; i < meshList.Count; ++i) {
            exMesh srcMesh = meshList[i];
            exMesh dstMesh = meshList[i - 1];
            if (ReferenceEquals(dstMesh.material, srcMesh.material) == false) {
                break;
            }
            int dstVertexCount = (i == _meshIndex + 1 ? _newVertexCount : _maxVertexCount);
            while (srcMesh.sortedSpriteList.Count > 0) {
                exLayeredSprite sprite = srcMesh.sortedSpriteList[0];
                if (dstMesh.vertices.Count + sprite.vertexCount <= dstVertexCount) {
                    ShiftSprite (srcMesh, dstMesh, sprite);
                }
                else {
                    break;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    private int GetBelowVertexCountInMesh (int _meshIndex, exLayeredSprite _sprite, int _maxVertexCount, out int _aboveSpriteIndex) {
        exMesh mesh = meshList[_meshIndex];
        _aboveSpriteIndex = mesh.sortedSpriteList.BinarySearch(_sprite);
        if (_aboveSpriteIndex < 0) {
            _aboveSpriteIndex = ~_aboveSpriteIndex;
            exDebug.Assert(0 < _aboveSpriteIndex && _aboveSpriteIndex <= mesh.sortedSpriteList.Count - 1, "no need to shift the mesh");
        }
        else {
            exDebug.Assert(0 < _aboveSpriteIndex && _aboveSpriteIndex < mesh.sortedSpriteList.Count - 1, "no need to shift the mesh");
            ++_aboveSpriteIndex;     // just insert above same depth sprite
        }
        if (_aboveSpriteIndex <= mesh.sortedSpriteList.Count) {
            int belowVertexCount = 0;
            for (int i = 0; i < _aboveSpriteIndex; ++i) {
                belowVertexCount += mesh.sortedSpriteList[i].vertexCount;
            }
            return belowVertexCount;
        }
        return mesh.vertices.Count;
    }

    // ------------------------------------------------------------------ 
    /// Shift sprites to the above mesh to make it has space to insert new sprite
    /// \param _meshIndex The index of the mesh to insert
    /// \param _sprite The sprite to insert
    /// \param _maxVertexCount The max vertex count of meshes in layer
    /// \return The mesh to insert
    // ------------------------------------------------------------------ 

    private exMesh GetShiftedMesh (int _meshIndex, exLayeredSprite _sprite, int _maxVertexCount) {
        exMesh mesh = meshList[_meshIndex];
        int newSpriteVertexCount = _sprite.vertexCount;
        int aboveSpriteIndex;
        GetBelowVertexCountInMesh(_meshIndex, _sprite, _maxVertexCount, out aboveSpriteIndex);
        int belowVertexCount = mesh.vertices.Count;
        for (int i = mesh.sortedSpriteList.Count - 1; i >= aboveSpriteIndex; --i) {
        	exLayeredSprite aboveSprite = mesh.sortedSpriteList[i];
            belowVertexCount -= aboveSprite.vertexCount;
            if (belowVertexCount + newSpriteVertexCount <= _maxVertexCount) {
                ShiftSpritesUp(_meshIndex, belowVertexCount, _maxVertexCount); // 上移
                return mesh;
            }
        }
        // 完全不能容纳，则把新的sprite和在它上面的sprite都再送到上一个mesh中
        if (_meshIndex + 1 < meshList.Count) {
            int aboveVertexCount = mesh.vertices.Count - belowVertexCount;
            int aboveMeshVertexCount = _maxVertexCount - aboveVertexCount - newSpriteVertexCount;
            ShiftSpritesUp(_meshIndex + 1, aboveMeshVertexCount, _maxVertexCount);    // 空出上一个mesh
            ShiftSpritesUp(_meshIndex, belowVertexCount, _maxVertexCount);            // 把需要渲染在上面的sprite移到上面的mesh
            return meshList[_meshIndex + 1];
        }
        return mesh;
    }
    
    // ------------------------------------------------------------------ 
    /// Split the mesh
    /// \param _meshIndex The index of the mesh to split
    /// \param _seperatorSprite 深度小于等于它的sprite将会被分隔到新创建的下层的mesh中
    /// \param _maxVertexCount The max vertex count of meshes in layer
    // ------------------------------------------------------------------ 

    private void SplitMesh (int _meshIndex, exLayeredSprite _seperatorSprite, int _maxVertexCount) {
        int t;
        int belowVertexCount = GetBelowVertexCountInMesh(_meshIndex, _seperatorSprite, _maxVertexCount, out t);
        ShiftSpritesUp(_meshIndex, belowVertexCount, _maxVertexCount);    // 上移
    }

    // ------------------------------------------------------------------ 
    /// 在保证渲染次序的前提下，获得可供插入的mesh，必要的话会进行mesh的拆分和创建操作。
    /// 这个算法保证mesh不产生零散的碎片，效率应该还有优化的余地。
    // ------------------------------------------------------------------ 

    private exMesh GetMeshToAdd (exLayeredSprite _sprite) {
        Material mat = _sprite.material;
        int maxVertexCount = (layerType_ == exLayerType.Dynamic) ? maxDynamicMeshVertex : exMesh.MAX_VERTEX_COUNT;
        // TODO: 如果sprite的vertex count大于maxVertexCount
        int restVertexCount = maxVertexCount - _sprite.vertexCount;
        for (int i = meshList.Count - 1; i >= 0; --i) {
            exMesh mesh = meshList[i];
            if (mesh == null) continue;
            
            //split mesh if batch failed
            exDebug.Assert(exLayeredSprite.enableFastShowHide);    // 要获取mesh中最先和最后渲染的sprite，要保证sprite都在sortedSpriteList中
            if (mesh.sortedSpriteList.Count == 0) continue;        // 跳过空的mesh，尽量把sprite合并到已有的mesh里面

            exLayeredSprite top = mesh.sortedSpriteList[mesh.sortedSpriteList.Count - 1];
            bool aboveTopSprite = _sprite >= top;
            if (aboveTopSprite) {   // 在这个mesh之上层 TODO: 如果是unordered，还可以优化成检查同一个depth的mesh后面是否有相同材质的mesh
                if (ReferenceEquals(mesh.material, mat) && mesh.vertices.Count <= restVertexCount) {
                    return mesh;
                }
                else {
                    return GetNewMesh(mat, i + 1);   //在mesh上层创建一个新mesh
                }
            }
            else {
                exLayeredSprite bot = mesh.sortedSpriteList[0];
                bool aboveBottomSprite = _sprite > bot;
                if (aboveBottomSprite) {   // 在这个mesh的depth内
                    if (ReferenceEquals(mesh.material, mat)) {
                        if (mesh.vertices.Count <= restVertexCount) {
                            return mesh;
                        }
                        else {
                            // mesh太大，把同材质的连续mesh的上面的sprite分出去，然后用新加的sprite依次填满空出来的格子
                            return GetShiftedMesh(i, _sprite, maxVertexCount);
                        }
                    }
                    else {
                        // 两个相同材质的sprite中间插入了另一个材质的sprite，则需要将上下两个sprite拆分到两个不同的mesh
                        // 然后将上面的sprite往上移动，直到该mesh只包含下面的sprite，然后插入其它材质的mesh
                        SplitMesh(i, _sprite, maxVertexCount);
                        return GetNewMesh(mat, i + 1);
                    }
                }
                // 否则和bot的深度相等，这时交由下层的mesh去处理
            }
        }
        if (meshList.Count > 0) {
            exMesh bottomMesh = meshList[0];
            if (ReferenceEquals(bottomMesh.material, mat) && bottomMesh.vertices.Count <= restVertexCount) {
                // 插入到最下面一个mesh
                return bottomMesh;
            }
            // 在最下面创建一个新mesh
            exMesh newMesh = GetNewMesh(mat, 0);
            if (ReferenceEquals(bottomMesh.material, mat)) {
                ShiftSpritesDown(0, restVertexCount, maxVertexCount);   // 向下把mesh都填满
            }
            return newMesh;
        }
        return GetNewMesh(mat, 0);
    }
    
    // ------------------------------------------------------------------ 
    /// Do add the sprite to the layer
    /// \param _newSprite 如果为true，则将sprite渲染到其它相同depth的sprite上面
    // ------------------------------------------------------------------ 
    
    private void DoAddSprite (exLayeredSprite _sprite, bool _newSprite) {
        Material mat = _sprite.material;
        if (mat == null) {
#if EX_DEBUG
            Debug.LogWarning("Ignore null material sprite", _sprite);
#endif
            return;
        }

        _sprite.layer = this;

        // set sprite id
        CheckDuplicated(_sprite);
        if (ordered_ && _newSprite || _sprite.spriteIdInLayer == -1) {
            _sprite.spriteIdInLayer = nextSpriteUniqueId;
            ++nextSpriteUniqueId;
        }
        else {
            nextSpriteUniqueId = Mathf.Max(_sprite.spriteIdInLayer + 1, nextSpriteUniqueId);
        }

        // caculate depth
        (_sprite as IFriendOfLayer).globalDepth = GetParentGlobalDepth(_sprite.gameObject) + _sprite.depth;

        // find available mesh
        exMesh mesh = GetMeshToAdd(_sprite);
        exDebug.Assert(mesh.vertices.Count + _sprite.vertexCount <= (layerType_ == exLayerType.Dynamic ? maxDynamicMeshVertex : exMesh.MAX_VERTEX_COUNT),
            string.Format("Invalid mesh vertex count : {0}", (mesh.vertices.Count + _sprite.vertexCount)));
        AddToMesh(_sprite, mesh);
    }

    // ------------------------------------------------------------------ 
    /// Add an exLayeredSprite to the mesh. 
    // ------------------------------------------------------------------ 

    private void AddToMesh (exLayeredSprite _sprite, exMesh _mesh) {
        exDebug.Assert(_mesh.spriteList.Contains(_sprite) == false, "Can't add duplicated sprite");

        _sprite.updateFlags = exUpdateFlags.None;
        _sprite.spriteIndexInMesh = _mesh.spriteList.Count;
        _mesh.spriteList.Add(_sprite);

        _sprite.FillBuffers(_mesh.vertices, _mesh.uvs, _mesh.colors32);
        if (exLayeredSprite.enableFastShowHide) {
            AddIndices(_mesh, _sprite);
            _sprite.transparent = !_sprite.visible;
        }
        else if (_sprite.visible) {
            AddIndices(_mesh, _sprite);
        }
        
        exDebug.Assert(_mesh.vertices.Count == _mesh.uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(_mesh.vertices.Count == _mesh.colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void RemoveFromMesh (exLayeredSprite _sprite, exMesh _mesh) {
        _mesh.spriteList.RemoveAt(_sprite.spriteIndexInMesh);
        int vertexCount = _sprite.vertexCount;
        for (int i = _sprite.spriteIndexInMesh; i < _mesh.spriteList.Count; ++i) {
            exLayeredSprite sprite = _mesh.spriteList[i];
            // update sprite and vertic index after removed sprite
            sprite.spriteIndexInMesh = i;
            sprite.vertexBufferIndex -= vertexCount;
            // update indices to make them match new vertic index
            if (sprite.isInIndexBuffer) {
                int indexEnd = sprite.indexBufferIndex + sprite.indexCount;
                for (int index = sprite.indexBufferIndex; index < indexEnd; ++index) {
                    if (index >= _mesh.indices.Count) {
                        Debug.Log(string.Format("[RemoveFromMesh|exLayer] index: {1} _mesh.indices.Count: {0}", _mesh.indices.Count, index));
                    }
                    _mesh.indices.buffer[index] -= vertexCount;
                }
            }
        }
        _mesh.updateFlags |= exUpdateFlags.VertexAndIndex;

        // update vertices
        _mesh.vertices.RemoveRange(_sprite.vertexBufferIndex, vertexCount);
        _mesh.colors32.RemoveRange(_sprite.vertexBufferIndex, vertexCount);
        _mesh.uvs.RemoveRange(_sprite.vertexBufferIndex, vertexCount);

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

    private void AddIndices (exMesh _mesh, exLayeredSprite _sprite) {
        exDebug.Assert(!_sprite.isInIndexBuffer);
        if (!_sprite.isInIndexBuffer) {
            int sortedSpriteIndex;
            if (_mesh.sortedSpriteList.Count > 0) {
                sortedSpriteIndex = _mesh.sortedSpriteList.BinarySearch(_sprite);   // TODO: benchmark
                exDebug.Assert(sortedSpriteIndex < 0, sortedSpriteIndex.ToString());  //sprite实现的比较方法决定了这种情况下不可能找到等同的排序
                if (sortedSpriteIndex < 0) {
                    sortedSpriteIndex = ~sortedSpriteIndex;
                }
                if (sortedSpriteIndex >= _mesh.sortedSpriteList.Count) {
                    // this sprite's depth is biggest
                    _sprite.indexBufferIndex = _mesh.indices.Count;
#if EX_DEBUG
                    exLayeredSprite lastSprite = _mesh.sortedSpriteList[_mesh.sortedSpriteList.Count - 1];
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
            exDebug.Assert(indexCount > 0);
            _mesh.indices.AddRange(indexCount);
            for (int i = _mesh.indices.Count - 1 - indexCount; i >= _sprite.indexBufferIndex ; --i) {
                _mesh.indices.buffer[i + indexCount] = _mesh.indices.buffer[i];
            }
            _sprite.updateFlags |= exUpdateFlags.Index;
            // update other sprites indexBufferIndex
            for (int i = sortedSpriteIndex; i < _mesh.sortedSpriteList.Count; ++i) {
                exLayeredSprite otherSprite = _mesh.sortedSpriteList[i];
                otherSprite.indexBufferIndex += indexCount;
            }
            // insert into _sortedSpriteList
            _mesh.sortedSpriteList.Insert(sortedSpriteIndex, _sprite);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void RemoveIndices (exMesh _mesh, exLayeredSprite _sprite) {
        exDebug.Assert(_sprite.isInIndexBuffer);
        if (_sprite.isInIndexBuffer) {
            // update indices
            _mesh.indices.RemoveRange(_sprite.indexBufferIndex, _sprite.indexCount);
            _mesh.updateFlags |= exUpdateFlags.Index;
            
            // update indexBufferIndex and sortedSpriteList
            for (int i = _mesh.sortedSpriteList.Count - 1; i >= 0; --i) {
                exLayeredSprite otherSprite = _mesh.sortedSpriteList[i];
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
            _sprite.isInIndexBuffer = false;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private exLayeredSprite GetNearestSpriteFromBelowMesh (int _curMeshIndex) {
        for (int belowMeshIndex = _curMeshIndex - 1; belowMeshIndex >= 0; --belowMeshIndex) {
            exMesh belowMesh = meshList[belowMeshIndex];
            if (belowMesh != null && belowMesh.sortedSpriteList.Count > 0) {
                return belowMesh.sortedSpriteList[belowMesh.sortedSpriteList.Count - 1];
            }
        }
        return null;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private exLayeredSprite GetNearestSpriteFromAboveMesh (int _curMeshIndex) {
        for (int aboveMeshIndex = _curMeshIndex + 1; aboveMeshIndex < meshList.Count; ++aboveMeshIndex) {
            exMesh aboveMesh = meshList[aboveMeshIndex];
            if (aboveMesh != null && aboveMesh.sortedSpriteList.Count > 0) {
                return aboveMesh.sortedSpriteList[0];
            }
        }
        return null;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private bool IsRenderOrderChangedAmongMeshes (exLayeredSprite _sprite, int _oldMeshIndex) { 
        exLayeredSprite aboveSprite = GetNearestSpriteFromAboveMesh(_oldMeshIndex);
        if (aboveSprite != null && _sprite > aboveSprite) {
            return true;
        }
        exLayeredSprite belowSprite = GetNearestSpriteFromBelowMesh(_oldMeshIndex);
        return (belowSprite != null && _sprite < belowSprite);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private bool IsRenderOrderChangedInMesh (exLayeredSprite _sprite, int _oldMeshIndex, int _oldSortedSpriteIndex) { 
        exMesh mesh = meshList[_oldMeshIndex];
        if (_oldSortedSpriteIndex < mesh.sortedSpriteList.Count - 1) {
            exLayeredSprite aboveSprite = mesh.sortedSpriteList[_oldSortedSpriteIndex + 1];
            if (_sprite > aboveSprite) { // 是否要更后渲染
                return true;
            }
        }
        if (_oldSortedSpriteIndex > 0) {
            exLayeredSprite belowSprite = mesh.sortedSpriteList[_oldSortedSpriteIndex - 1];
            return (_sprite < belowSprite);
        }
        return false;
    }

    // ------------------------------------------------------------------ 
    /// 最终更新sprite的depth，只有当sprite在depthDirtySpriteList并且有exUpdateFlags.SelfDepth标记时，或者当sprite的任一父物体在depthDirtySpriteList时，depth才会重新计算
    /// 调用这个方法后exLayeredSprite.globalDepth才会更新
    // ------------------------------------------------------------------ 

    private void UpdateAllSpritesDepth () {
        for (int i = 0; i < depthDirtySpriteList.Count; ++i) {
            exLayeredSprite sprite = depthDirtySpriteList[i];
            if (sprite != null) {
                UpdateSpriteDepthRecursively(sprite.gameObject, GetParentGlobalDepth(sprite.gameObject));
            }
        }
        depthDirtySpriteList.Clear();

        for (int i = 0; i < depthDirtyGoList.Count; ++i) {
            GameObject go = depthDirtyGoList[i];
            if (go != null) {
                UpdateSpriteDepthRecursively(go, GetParentGlobalDepth(go));
            }
        }
        depthDirtyGoList.Clear();
    }
}