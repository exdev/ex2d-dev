﻿// ======================================================================================
// File         : exLayer.cs
// Author       : Jare
// Last Change  : 06/15/2013 | 22:34:19
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

#define USE_DRAW_MESH
#if USE_DRAW_MESH
    //#define DRAW_MESH_NOW
#endif

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ------------------------------------------------------------------ 
/// The type of update for mesh
// ------------------------------------------------------------------ 

[System.FlagsAttribute]
public enum UpdateFlags {
	None		= 0,  ///< none
	Index	    = 1,  ///< update the indices
	Vertex		= 2,  ///< update the vertices
	UV	        = 4,  ///< update the uv coordination
	Color	    = 8,  ///< update the vertex color
	Normal	    = 16, ///< update the normal

	VertexAndIndex = (Index | Vertex),
	All = (Index | Vertex | UV | Color | Normal), ///< update all
};

///////////////////////////////////////////////////////////////////////////////
//
/// The layer component
/// NOTE: Don't add this component yourself, use exLayer.Create instead.
//
///////////////////////////////////////////////////////////////////////////////

#if !USE_DRAW_MESH
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
#endif
public class exLayer : MonoBehaviour
{
    public enum LayerType
    {
        Static = 0,
        Dynamic,
    }
    const int RESERVED_INDEX_COUNT = 6;    // 如果不手动给出，按List初始分配个数(4个)，则添加一个quad就要分配两次内存
    const bool CAN_RESERVE_VERTEX = false; // 删除mesh最后面的顶点时，仅先从index buffer中清除，vertices等数据不标记为脏

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
                mesh.MarkDynamic();
            }
            else if (value == LayerType.Static){
                Compact();
            }
        }
    }

#if !EX_DEBUG
    [System.NonSerialized]
#endif
    public Mesh mesh;

#if USE_DRAW_MESH

#if !EX_DEBUG
    [System.NonSerialized]
#endif
    public Material material;

#elif UNITY_EDITOR
    /// test only
    public Material material {
        set {
            renderer.material = value;
        }
    }
#endif

#if !EX_DEBUG
    [System.NonSerialized]
#endif
    public List<exSpriteBase> spriteList = new List<exSpriteBase>();

    /// cache mesh.vertices
    /// 依照sprite在spriteList中的相同顺序排列，每个sprite的顶点都放在连续的一段区间中
    /// vertices的数量和索引都保持和uvs, colors, normals, tangents一致
    private List<Vector3> vertices = new List<Vector3>();

    /// cache mesh.triangles
    /// 按深度排序，深度一样的话，按加入的时间排序
    private List<int> indices = new List<int>(RESERVED_INDEX_COUNT);

    private List<Vector2> uvs = new List<Vector2>();       ///< cache mesh.vertices
    private List<Color32> colors32 = new List<Color32>();  ///< cache mesh.colors32

    private UpdateFlags updateFlags = UpdateFlags.None;

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
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
        GameObject go = new GameObject("_exLayer");
        go.hideFlags = exReleaseFlag.hideAndDontSave;
        // TODO: 对Material进行设置
        exLayer res = go.AddComponent<exLayer>();
        res.CreateMesh();
        return res;
    }

    // ------------------------------------------------------------------ 
    /// Maintains a mesh to render all sprites
    // ------------------------------------------------------------------ 

    public void UpdateMesh () {
        for (int i = 0; i < spriteList.Count; ++i) {
            exSpriteBase sprite = spriteList[i];
            exDebug.Assert((sprite.enabled && sprite.gameObject.activeInHierarchy) == sprite.IsInIndexBuffer);

            if (sprite.enabled) {
                // TODO: 把对mesh的操作做成虚函数由各个sprite自己进行
                sprite.UpdateDirtyFlags();
                updateFlags |= sprite.updateFlags;
                if ((sprite.updateFlags & UpdateFlags.Vertex) != 0) {
                    var pos = sprite.cachedTransform.position;
                    vertices[sprite.vertexBufferIndex + 0] = pos + new Vector3(-1.0f, -1.0f, 0.0f);
                    vertices[sprite.vertexBufferIndex + 1] = pos + new Vector3(-1.0f, 1.0f, 0.0f);
                    vertices[sprite.vertexBufferIndex + 2] = pos + new Vector3(1.0f, 1.0f, 0.0f);
                    vertices[sprite.vertexBufferIndex + 3] = pos + new Vector3(1.0f, -1.0f, 0.0f);
                }
                if ((sprite.updateFlags & UpdateFlags.UV) != 0) {
                    exTextureInfo textureInfo = (sprite as exSprite).textureInfo;
                    float xStart = (float)textureInfo.x / (float)textureInfo.texture.width;
                    float yStart = (float)textureInfo.y / (float)textureInfo.texture.height;
                    float xEnd = (float)(textureInfo.x + textureInfo.width) / (float)textureInfo.texture.width;
                    float yEnd = (float)(textureInfo.y + textureInfo.height) / (float)textureInfo.texture.height;
                    uvs[sprite.vertexBufferIndex + 0] = new Vector2(xStart, yStart);
                    uvs[sprite.vertexBufferIndex + 1] = new Vector2(xStart, yEnd);
                    uvs[sprite.vertexBufferIndex + 2] = new Vector2(xEnd, yEnd);
                    uvs[sprite.vertexBufferIndex + 3] = new Vector2(xEnd, yStart);
                }
                if ((sprite.updateFlags & UpdateFlags.Color) != 0) {
                    colors32[sprite.vertexBufferIndex + 0] = new Color32(255, 255, 255, 255);
                    colors32[sprite.vertexBufferIndex + 1] = new Color32(255, 255, 255, 255);
                    colors32[sprite.vertexBufferIndex + 2] = new Color32(255, 255, 255, 255);
                    colors32[sprite.vertexBufferIndex + 3] = new Color32(255, 255, 255, 255);
                }
                if ((sprite.updateFlags & UpdateFlags.Index) != 0) {
                    // TODO: resort
                    TestIndices(sprite);
                }
                sprite.updateFlags = UpdateFlags.None;
            }
        }
        if ((updateFlags & UpdateFlags.VertexAndIndex) == UpdateFlags.VertexAndIndex) {
            // 如果索引还未更新就减少顶点数量，索引可能会成为非法的，所以这里要把索引一起清空
            mesh.Clear(true);
        }
        if ((updateFlags & UpdateFlags.Vertex) != 0 || 
            (updateFlags & UpdateFlags.Index) != 0) {           // 如果要重设triangles，则必须同时重设vertices，否则mesh将显示不出来
            mesh.vertices = vertices.ToArray();
        }
        if ((updateFlags & UpdateFlags.UV) != 0) {
            mesh.uv = uvs.ToArray();
        }
        if ((updateFlags & UpdateFlags.Color) != 0) {
            mesh.colors32 = colors32.ToArray();
        }
        if ((updateFlags & UpdateFlags.Index) != 0) {
            mesh.triangles = indices.ToArray();      // Assigning triangles will automatically Recalculate the bounding volume.
        }
        if ((updateFlags & UpdateFlags.Normal) != 0) {
            var normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; ++i) {
                normals[i] = new Vector3(0, 0, -1);
            }
            mesh.normals = normals;
        }
        updateFlags = UpdateFlags.None;
#if USE_DRAW_MESH
#   if DRAW_MESH_NOW
            material.SetPass(0);
            Graphics.DrawMeshNow(mesh, transform.localToWorldMatrix, -1);
#   else
        //if (Time.frameCount % 2 == 0) {
            Graphics.DrawMesh(mesh, transform.localToWorldMatrix, material, gameObject.layer, null, 0, null, false, false);
        //}
#   endif
#endif
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to this layer. 
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    public void Add (exSpriteBase _sprite, bool _show = true) {
        bool hasSprite = spriteList.Contains(_sprite);
        if (hasSprite) {
            Debug.LogError("[Add|exLayer] can't add duplicated sprite");
            return;
        }

        _sprite.spriteIndex = spriteList.Count;
        spriteList.Add(_sprite);

        // TODO: 把添加过程放到sprite里，则不必更新_sprite.updateFlags
        _sprite.vertexBufferIndex = vertices.Count;
        vertices.Add(new Vector3());
        vertices.Add(new Vector3());
        vertices.Add(new Vector3());
        vertices.Add(new Vector3());
        colors32.Add(new Color32());
        colors32.Add(new Color32());
        colors32.Add(new Color32());
        colors32.Add(new Color32());
        uvs.Add(new Vector2());
        uvs.Add(new Vector2());
        uvs.Add(new Vector2());
        uvs.Add(new Vector2());
        _sprite.updateFlags |= (UpdateFlags.Vertex | UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);

        updateFlags |= (UpdateFlags.Vertex | UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);

        if (_show) {
            AddIndices(_sprite);
        }
        
        exDebug.Assert(vertices.Count == uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(vertices.Count == colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _oldSprite) {
        bool hasSprite = object.ReferenceEquals(this, _oldSprite.layer);
        exDebug.Assert(hasSprite == spriteList.Contains(_oldSprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to remove");
            return;
        }

        spriteList.RemoveAt(_oldSprite.spriteIndex);
        
        for (int i = _oldSprite.spriteIndex; i < spriteList.Count; ++i) {
            exSpriteBase sprite = spriteList[i];
            // update sprite and vertic index after removed sprite
            sprite.spriteIndex = i;
            sprite.vertexBufferIndex -= _oldSprite.vertexCount;
            // update indices to make them match new vertic index
            for (int index = sprite.indexBufferIndex; index < sprite.indexBufferIndex + sprite.indexCount; ++index) {
                indices[index] -= _oldSprite.vertexCount;
            }
        }

        // update vertices
        // TODO: 如果sprite的顶点在vertices的最后，把vertices的坑留着，只改变indices
        vertices.RemoveRange(_oldSprite.vertexBufferIndex, _oldSprite.vertexCount);
        colors32.RemoveRange(_oldSprite.vertexBufferIndex, _oldSprite.vertexCount);
        uvs.RemoveRange(_oldSprite.vertexBufferIndex, _oldSprite.vertexCount);

#pragma warning disable 0429
        bool needUpdateVertices = (!CAN_RESERVE_VERTEX || _oldSprite.spriteIndex < spriteList.Count);
#pragma warning restore 0429
        if (needUpdateVertices) {
            updateFlags |= (UpdateFlags.Vertex | UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);
        }

        if (_oldSprite.IsInIndexBuffer) {
            RemoveIndices(_oldSprite);
        }

        exDebug.Assert(_oldSprite.indexBufferIndex == -1);
        exDebug.Assert(vertices.Count == uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(vertices.Count == colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exSpriteBase
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 

    public void ShowSprite (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(this, _sprite.layer);
        exDebug.Assert(hasSprite == spriteList.Contains(_sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to show");
            return;
        }
        // show
        if (!_sprite.IsInIndexBuffer) {
            AddIndices(_sprite);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void HideSprite (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(this, _sprite.layer);
        exDebug.Assert(hasSprite == spriteList.Contains(_sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to hide");
            return;
        }
        // hide
        if (_sprite.IsInIndexBuffer) {
            RemoveIndices(_sprite);
        }
        exDebug.Assert(_sprite.indexBufferIndex == -1);
    }

    // ------------------------------------------------------------------ 
    /// Compact all reserved buffers
    /// 其实mscorlib在实现时还是会预留10%的buffer
    // ------------------------------------------------------------------ 

    public void Compact () {
        spriteList.TrimExcess();
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
        exDebug.Assert(!_sprite.IsInIndexBuffer);
        if (!_sprite.IsInIndexBuffer) {
            _sprite.indexBufferIndex = indices.Count;
            indices.Add(_sprite.vertexBufferIndex + 0);
            indices.Add(_sprite.vertexBufferIndex + 1);
            indices.Add(_sprite.vertexBufferIndex + 2);
            indices.Add(_sprite.vertexBufferIndex + 3);
            indices.Add(_sprite.vertexBufferIndex + 0);
            indices.Add(_sprite.vertexBufferIndex + 2);
        
            updateFlags |= UpdateFlags.Index;

            // TODO: resort indices by depth
            TestIndices(_sprite);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    void RemoveIndices (exSpriteBase _sprite) {
        exDebug.Assert(_sprite.IsInIndexBuffer);
        if (_sprite.IsInIndexBuffer) {
            // update indices
            indices.RemoveRange(_sprite.indexBufferIndex, _sprite.indexCount);
            
            // update indexBufferIndex
            // TODO: 这里是性能瓶颈，应该设法优化
            for (int i = 0; i < spriteList.Count; ++i) {
                exSpriteBase sprite = spriteList[i];
                if (sprite.indexBufferIndex > _sprite.indexBufferIndex) {
                    sprite.indexBufferIndex -= _sprite.indexCount;
                    exDebug.Assert(sprite.indexBufferIndex >= _sprite.indexBufferIndex);
                }
            }
            //TODO
            _sprite.indexBufferIndex = -1;

            updateFlags |= UpdateFlags.Index;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    [System.Diagnostics.Conditional("EX_DEBUG")]
    void TestIndices (exSpriteBase _sprite) {
        // check indice is valid
        for (int i = _sprite.indexBufferIndex; i < _sprite.indexBufferIndex + _sprite.indexCount; ++i) {
            if (indices[i] < _sprite.vertexBufferIndex || indices[i] > _sprite.vertexBufferIndex + _sprite.vertexCount) {
                Debug.LogError("[exLayer] Wrong triangle index!");
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// Remove all exSpriteBases out of this layer 
    // ------------------------------------------------------------------ 

    void RemoveAll () {
        while (spriteList.Count > 0) {
            exSpriteBase sprite = spriteList[spriteList.Count - 1];
            exDebug.Assert(sprite);
            if (sprite) {
                sprite.layer = null;
            }
        }
        spriteList.Clear();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void CreateMesh () {
        exDebug.Assert(!mesh);
        if (!mesh) {
#if USE_DRAW_MESH
            mesh = new Mesh();
#else
            //var meshFilter = gameObject.AddComponent<MeshFilter>();
            //var mr = gameObject.AddComponent<MeshRenderer>();
            var meshFilter = gameObject.GetComponent<MeshFilter>();
            var mr = gameObject.GetComponent<MeshRenderer>();
            mr.receiveShadows = false;
            mr.castShadows = false;
            mesh = meshFilter.mesh;
#endif
            mesh.name = "exLayer mesh";
            mesh.hideFlags = HideFlags.DontSave;
            if (layerType == LayerType.Dynamic) {
                mesh.MarkDynamic();
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // Output debug info
    // ------------------------------------------------------------------ 

    [ContextMenu("Output Mesh Info")]
    [System.Diagnostics.Conditional("EX_DEBUG")]
    void OutputMeshInfo () {
        string vertexInfo = "Vertices: ";
        foreach (var v in vertices) {
            vertexInfo += v;
            vertexInfo += ", ";
        }
        Debug.Log(vertexInfo);
        
        vertexInfo = "Mesh.vertices: ";
        foreach (var v in mesh.vertices) {
            vertexInfo += v;
            vertexInfo += ", ";
        }
        Debug.Log(vertexInfo);

        string indicesInfo = "Indices: ";
        foreach (var index in indices) {
            indicesInfo += index;
            indicesInfo += ",";
        }
        Debug.Log(indicesInfo);

        indicesInfo = "Mesh.triangles: ";
        foreach (var index in mesh.triangles) {
            indicesInfo += index;
            indicesInfo += ",";
        }
        Debug.Log(indicesInfo);
    }
}