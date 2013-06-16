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

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    private LayerType _layerType = LayerType.Dynamic;
    public LayerType layerType {
        get {
            return _layerType;
        }
        set {
            if (_layerType == value) {
                return;
            }
            _layerType = value;
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
    private List<int> indices = new List<int>();

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
            if (sprite.updateTransform) {
                //sprite.updateTransform = false;
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
            Debug.Log(string.Format("[UpdateMesh|exLayer] uvs.Count: {0}", uvs.Count));
            meshFilter.mesh.uv = uvs.ToArray();
        }
        if (colorChanged) {
            colorChanged = false;
            Debug.Log(string.Format("[UpdateMesh|exLayer] colors32.Count: {0}", colors32.Count));
            meshFilter.mesh.colors32 = colors32.ToArray();
        }
        if (indiceChanged) {
            indiceChanged = false;
            Debug.Log(string.Format("[UpdateMesh|exLayer] indices.Count: {0}", indices.Count));
            Debug.Log(string.Format("[UpdateMesh|exLayer] vertices.Count: {0}", vertices.Count));
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

    public void Add (exSpriteBase sprite) {
        bool hasSprite = object.ReferenceEquals(this, sprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(sprite), "wrong sprite.layer");
        if (hasSprite) {
            Debug.LogError("[Add|exLayer] can't add duplicated sprite");
            return;
        }

        sprite.lSpriteIndex = allSprites.Count;
        allSprites.Add(sprite);

        sprite.lVerticesIndex = vertices.Count;
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

        AddIndices(sprite);
        
        exDebug.Assert(vertices.Count == uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(vertices.Count == colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase oldSprite) {
        bool hasSprite = object.ReferenceEquals(this, oldSprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(oldSprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to remove");
            return;
        }

        allSprites.RemoveAt(oldSprite.lSpriteIndex);
        
        for (int i = oldSprite.lSpriteIndex; i < allSprites.Count; ++i) {
            // update sprite and vertic index after removed sprite
            allSprites[i].lSpriteIndex = i;
            allSprites[i].lVerticesIndex -= oldSprite.verticesCount;
            // update indices to make them match new vertic index
            for (int index = allSprites[i].lIndicesIndex; index < allSprites[i].lIndicesCount; ++index) {
            	indices[index] -= oldSprite.verticesCount;
            }
        }

        // update vertices
        // TODO: 如果sprite的顶点在vertices的最后，把vertices的坑留着，只改变indices
        int removeStart = vertices.Count - oldSprite.verticesCount;
        for (int i = oldSprite.lVerticesIndex; i < removeStart; ++i) {
            vertices[i] = vertices[i + oldSprite.verticesCount];
            colors32[i] = colors32[i + oldSprite.verticesCount];
            uvs[i] = uvs[i + oldSprite.verticesCount];
        }
        vertices.RemoveRange(removeStart, oldSprite.verticesCount);
        verticeChanged = true;

        colors32.RemoveRange(removeStart, oldSprite.verticesCount);
        colorChanged = true;

        uvs.RemoveRange(removeStart, oldSprite.verticesCount);
        uvChanged = true;

        rebuildNormal = true;
        
        RemoveIndices(oldSprite);

        exDebug.Assert(vertices.Count == uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(vertices.Count == colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exSpriteBase
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    public void Show (exSpriteBase sprite) {
        bool hasSprite = object.ReferenceEquals(this, sprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to show");
            return;
        }
        bool indicesAdded = sprite.lIndicesIndex != -1;
        if (!indicesAdded) {
            AddIndices(sprite);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void Hide (exSpriteBase sprite) {
        bool hasSprite = object.ReferenceEquals(this, sprite.layer);
        exDebug.Assert(hasSprite == allSprites.Contains(sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to hide");
            return;
        }
        
        RemoveIndices(sprite);
    }

    // ------------------------------------------------------------------ 
    // 将Mesh优化过后，sprite的索引将失效，
    // 但是可以将整个mesh烘焙成一个新sprite，然后移除原有全部sprite
    // ------------------------------------------------------------------ 

    /*public void Optimize () {
        exDebug.Assert(layerType == LayerType.Dynamic, "consider using Static LayerType", false);
        meshFilter.mesh.Optimize();
        // TODO: 检查这个方法是否会导致这个mesh不可编辑
    }*/

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

    void AddIndices (exSpriteBase sprite) {
        sprite.lIndicesIndex = indices.Count;
        Debug.Log(string.Format("[AddIndices|exLayer] sprite.lVerticesIndex: {0}", sprite.lVerticesIndex));
        indices.Add(sprite.lVerticesIndex + 0);
        indices.Add(sprite.lVerticesIndex + 1);
        indices.Add(sprite.lVerticesIndex + 2);
        indices.Add(sprite.lVerticesIndex + 3);
        indices.Add(sprite.lVerticesIndex + 0);
        indices.Add(sprite.lVerticesIndex + 2);
        indiceChanged = true;
        // TODO: resort indices by depth
        // TODO: 修复多个sprite依次开关会无法显示的bug
        TestIndices(sprite);
    }

    // ------------------------------------------------------------------ 
    /// NOTE: Only remove indices, keep others unchanged.
    // ------------------------------------------------------------------ 
    
    void RemoveIndices (exSpriteBase sprite) {
        bool indicesAdded = sprite.lIndicesIndex != -1;
        if (indicesAdded) {
            for (int i = sprite.lIndicesIndex + sprite.lIndicesCount; i < indices.Count; ++i) {
                indices[i - sprite.lIndicesCount] = indices[i];
            }
            indices.RemoveRange(indices.Count - sprite.lIndicesCount, sprite.lIndicesCount);
            sprite.lIndicesIndex = -1;
            indiceChanged = true;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    [System.Diagnostics.Conditional("EX_DEBUG")]
    void TestIndices (exSpriteBase sprite) {
        //check indice is valid
        for (int i = sprite.lIndicesIndex; i < sprite.lIndicesIndex + sprite.lIndicesCount; ++i) {
            if (indices[i] < sprite.lVerticesIndex || indices[i] > sprite.lVerticesIndex + sprite.verticesCount) {
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
}