// ======================================================================================
// File         : exLayer.cs
// Author       : Jare
// Last Change  : 06/15/2013 | 22:34:19
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
//
/// The layer component
/// NOTE: Don't add this component yourself, use exLayer.Create instead.
//
///////////////////////////////////////////////////////////////////////////////

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class exLayer : MonoBehaviour
{
    public enum LayerType
    {
        Static = 0,
        Dynamic,
    }

    const int RESERVED_INDICE_COUNT = 6;    // 如果不手动给出，按List初始分配个数(4个)，则添加一个quad就要分配两次内存

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
            if (value == LayerType.Dynamic && meshFilter && meshFilter.mesh) {
                meshFilter.mesh.MarkDynamic();
            }
            else if (value == LayerType.Static){
                Compact();
            }
        }
    }

    private MeshFilter meshFilter;
    
    private List<exSpriteBase> allSprites = new List<exSpriteBase>();

    /// cache mesh.vertices
    /// 依照sprite在allSprites中的相同顺序排列，每个sprite的顶点都放在连续的一段区间中
    /// vertices的数量和索引都保持和uvs, colors, normals, tangents一致
    private List<Vector3> vertices = new List<Vector3>();

    /// cache mesh.triangles
    /// 按深度排序，深度一样的话，按加入的时间排序
    private List<int> indices = new List<int>(RESERVED_INDICE_COUNT);

    private List<Vector2> uvs = new List<Vector2>();       ///< cache mesh.vertices
    private List<Color32> colors32 = new List<Color32>();  ///< cache mesh.colors32
    
    private bool verticeChanged = true;
    private bool indiceChanged = true;
    private bool uvChanged = true;
    private bool colorChanged = true;
    private bool rebuildNormal = true;
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
        meshFilter = GetComponent<MeshFilter>();
    }

    void OnDestroy () {
        RemoveAll();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Create a new GameObject contains an exLayer component
    // ------------------------------------------------------------------ 

    public static exLayer Create (ex2DMng _2dMng) {
        GameObject go = new GameObject("exLayer");
        go.hideFlags = exReleaseFlag.hideAndDontSave;
        go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.receiveShadows = false;
        mr.castShadows = false;
        // TODO: 对Material进行设置
        exLayer res = go.AddComponent<exLayer>();
        res.CreateMesh();
        _2dMng.Add(res);
        return res;
    }

    // ------------------------------------------------------------------ 
    /// Maintains a mesh to render all sprites
    // ------------------------------------------------------------------ 

    public void UpdateMesh () {
        /* sprite变化时，如果整个mesh共用一份数据缓存，做法是
         * (done)加入/删除或顶点数量改变：更新所有位于所在顶点之后的数据，重建indices
         * (done)显示/隐藏：重建indices
         * 
         * (done)顶点位移(缩放旋转)：修改部分vertices
         * (done)整个位移：修改部分vertices
         * (done)改变UV：修改部分uvs
         * (done)改变颜色：修改部分colors
         * 
         * 改变深度：重建indices
         * 改变材质(贴图)：暂不考虑
         * 
         * 为了减少GC，把所有mesh数据缓存到layer里，而不是直接从mesh重新拿。
         */
        
        foreach (exSpriteBase sprite in allSprites) {
            // TODO: 把对mesh的操作做成虚函数由各个sprite自己进行
            sprite.UpdateDirtyFlags();
            if (sprite.updateTransform) {
                sprite.updateTransform = false;
                verticeChanged = true;
                var pos = sprite.transform.position;
                vertices[sprite.lVerticesIndex + 0] = pos + new Vector3(-1.0f, -1.0f, 0.0f);
                vertices[sprite.lVerticesIndex + 1] = pos + new Vector3(-1.0f, 1.0f, 0.0f);
                vertices[sprite.lVerticesIndex + 2] = pos + new Vector3(1.0f, 1.0f, 0.0f);
                vertices[sprite.lVerticesIndex + 3] = pos + new Vector3(1.0f, -1.0f, 0.0f);
            }
            if (sprite.updateUv) {
                sprite.updateUv = false;
                uvChanged = true;
                exTextureInfo textureInfo = (sprite as exSprite).textureInfo;
                float xStart = (float)textureInfo.x / (float)textureInfo.texture.width;
                float yStart = (float)textureInfo.y / (float)textureInfo.texture.height;
                float xEnd = (float)(textureInfo.x + textureInfo.width) / (float)textureInfo.texture.width;
                float yEnd = (float)(textureInfo.y + textureInfo.height) / (float)textureInfo.texture.height;
                uvs[sprite.lVerticesIndex + 0] = new Vector2(xStart, yStart);
                uvs[sprite.lVerticesIndex + 1] = new Vector2(xStart, yEnd);
                uvs[sprite.lVerticesIndex + 2] = new Vector2(xEnd, yEnd);
                uvs[sprite.lVerticesIndex + 3] = new Vector2(xEnd, yStart);
            }
            if (sprite.updateColor) {
                sprite.updateColor = false;
                colorChanged = true;
                colors32[sprite.lVerticesIndex + 0] = new Color32(255, 255, 255, 255);
                colors32[sprite.lVerticesIndex + 1] = new Color32(255, 255, 255, 255);
                colors32[sprite.lVerticesIndex + 2] = new Color32(255, 255, 255, 255);
                colors32[sprite.lVerticesIndex + 3] = new Color32(255, 255, 255, 255);
            }
            if (sprite.updateDepth) {
                sprite.updateDepth = false;
                indiceChanged = true;
                // TODO: resort
                TestIndices(sprite);
            }
        }

        if (verticeChanged) {
            verticeChanged = false;
            meshFilter.mesh.vertices = vertices.ToArray();
        }
        if (uvChanged) {
            uvChanged = false;
            meshFilter.mesh.uv = uvs.ToArray();
        }
        if (colorChanged) {
            colorChanged = false;
            meshFilter.mesh.colors32 = colors32.ToArray();
        }
        if (indiceChanged) {
            indiceChanged = false;
            meshFilter.mesh.triangles = indices.ToArray(); // Assigning triangles will automatically Recalculate the bounding volume.
        }
        if (rebuildNormal) {
            rebuildNormal = false;
            var normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; ++i) {
                normals[i] = new Vector3(0, 0, -1);
            }
            meshFilter.mesh.normals = normals;
        }
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to this layer. 
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    public void Add (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(this, _sprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(_sprite), "wrong sprite.layer");
        if (hasSprite) {
            Debug.LogError("[Add|exLayer] can't add duplicated sprite");
            return;
        }

        _sprite.lSpriteIndex = allSprites.Count;
        allSprites.Add(_sprite);

        _sprite.lVerticesIndex = vertices.Count;
        vertices.Add(new Vector3());
        vertices.Add(new Vector3());
        vertices.Add(new Vector3());
        vertices.Add(new Vector3());
        verticeChanged = true;

        colors32.Add(new Color32());
        colors32.Add(new Color32());
        colors32.Add(new Color32());
        colors32.Add(new Color32());
        colorChanged = true;

        uvs.Add(new Vector2());
        uvs.Add(new Vector2());
        uvs.Add(new Vector2());
        uvs.Add(new Vector2());
        uvChanged = true;

        rebuildNormal = true;

        AddIndices(_sprite);
        
        exDebug.Assert(vertices.Count == uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(vertices.Count == colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _oldSprite) {
        bool hasSprite = object.ReferenceEquals(this, _oldSprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(_oldSprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to remove");
            return;
        }

        allSprites.RemoveAt(_oldSprite.lSpriteIndex);
        
        for (int i = _oldSprite.lSpriteIndex; i < allSprites.Count; ++i) {
            // update sprite and vertic index after removed sprite
            allSprites[i].lSpriteIndex = i;
            allSprites[i].lVerticesIndex -= _oldSprite.verticesCount;
            // update indices to make them match new vertic index
            for (int index = allSprites[i].lIndicesIndex; index < allSprites[i].lIndicesCount; ++index) {
                indices[index] -= _oldSprite.verticesCount;
            }
        }

        // update vertices
        // TODO: 如果sprite的顶点在vertices的最后，把vertices的坑留着，只改变indices
        vertices.RemoveRange(_oldSprite.lVerticesIndex, _oldSprite.verticesCount);
        verticeChanged = true;

        colors32.RemoveRange(_oldSprite.lVerticesIndex, _oldSprite.verticesCount);
        colorChanged = true;

        uvs.RemoveRange(_oldSprite.lVerticesIndex, _oldSprite.verticesCount);
        uvChanged = true;

        rebuildNormal = true;
        
        RemoveIndices(_oldSprite);

        exDebug.Assert(vertices.Count == uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(vertices.Count == colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exSpriteBase
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    public void Show (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(this, _sprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(_sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to show");
            return;
        }
        bool indicesAdded = _sprite.lIndicesIndex != -1;
        if (!indicesAdded) {
            AddIndices(_sprite);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void Hide (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(this, _sprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(_sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to hide");
            return;
        }
        
        RemoveIndices(_sprite);
    }

    // ------------------------------------------------------------------ 
    /// Compact all reserved buffers
    /// 其实mscorlib在实现时还是会预留10%的buffer
    // ------------------------------------------------------------------ 

    public void Compact () {
        allSprites.TrimExcess();
        vertices.TrimExcess();
        indices.TrimExcess();
        uvs.TrimExcess();
        colors32.TrimExcess();
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// Add and resort indices by depth
    // ------------------------------------------------------------------ 

    void AddIndices (exSpriteBase _sprite) {
        _sprite.lIndicesIndex = indices.Count;
        indices.Add(_sprite.lVerticesIndex + 0);
        indices.Add(_sprite.lVerticesIndex + 1);
        indices.Add(_sprite.lVerticesIndex + 2);
        indices.Add(_sprite.lVerticesIndex + 3);
        indices.Add(_sprite.lVerticesIndex + 0);
        indices.Add(_sprite.lVerticesIndex + 2);
        indiceChanged = true;
        // TODO: resort indices by depth
        // TODO: 修复多个sprite依次开关会无法显示的bug
        TestIndices(_sprite);
    }

    // ------------------------------------------------------------------ 
    /// NOTE: Only remove indices, keep others unchanged.
    // ------------------------------------------------------------------ 
    
    void RemoveIndices (exSpriteBase _sprite) {
        bool indicesAdded = _sprite.lIndicesIndex != -1;
        if (indicesAdded) {
            // update indices
            indices.RemoveRange(_sprite.lIndicesIndex, _sprite.lIndicesCount);
            
            // update indices index
            for (int i = 0; i < allSprites.Count; ++i) {
                if (allSprites[i].lIndicesIndex > _sprite.lIndicesIndex) {
                    allSprites[i].lIndicesIndex -= _sprite.lIndicesCount;
                    exDebug.Assert(allSprites[i].lIndicesIndex >= _sprite.lIndicesIndex);
                }
            }
            _sprite.lIndicesIndex = -1;

            indiceChanged = true;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    [System.Diagnostics.Conditional("EX_DEBUG")]
    void TestIndices (exSpriteBase _sprite) {
        // check indice is valid
        for (int i = _sprite.lIndicesIndex; i < _sprite.lIndicesIndex + _sprite.lIndicesCount; ++i) {
            if (indices[i] < _sprite.lVerticesIndex || indices[i] > _sprite.lVerticesIndex + _sprite.verticesCount) {
                Debug.LogError("[exLayer] Wrong triangle index!");
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// Remove all exSpriteBases out of this layer 
    // ------------------------------------------------------------------ 

    void RemoveAll () {
        while (allSprites.Count > 0) {
            exSpriteBase sprite = allSprites[allSprites.Count - 1];
            sprite.layer = null;
        }
        allSprites.Clear();
        allSprites = null;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void CreateMesh () {
        if (!meshFilter.sharedMesh) {
            meshFilter.mesh.name = "exLayer mesh";  //a new mesh will be created and assigned
            meshFilter.mesh.hideFlags = HideFlags.DontSave;
        }
        if (layerType == LayerType.Dynamic) {
            meshFilter.mesh.MarkDynamic();
        }
    }
    
    // ------------------------------------------------------------------ 
    // Output debug info
    // ------------------------------------------------------------------ 

    [ContextMenu("Output Indices")]
    [System.Diagnostics.Conditional("EX_DEBUG")]
    void OutputIndices () {
        string indicesInfo = "Indices: ";
        foreach (var index in indices) {
            indicesInfo += index;
            indicesInfo += ",";
        }
        Debug.Log(indicesInfo);
    }
}