// ======================================================================================
// File         : exMesh.cs
// Author       : Jare
// Last Change  : 06/23/2013 | 15:27:56
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
/// Generated mesh operator. All exLayer have at least one.
/// This class performs actions selectively depending on what has changed. 
/// 通常来说这个类只需要对exLayer可见
//
///////////////////////////////////////////////////////////////////////////////

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class exMesh : MonoBehaviour
{
    const int RESERVED_INDEX_COUNT = 6;    // 如果不手动给出，按List初始分配个数(4个)，则添加一个quad就要分配两次内存

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] public exLayer layer;
    
    //material
    Renderer cachedRenderer;
    public Material material {
        get {
            if (cachedRenderer) {
                return cachedRenderer.sharedMaterial;
            }
            else {
                return null;
            }
        }
        set {
            if (cachedRenderer) {
                cachedRenderer.sharedMaterial = value;
            }
            else {
                Debug.LogError("no MeshRenderer");
            }
        }
    }

    [System.NonSerialized]
    public List<exSpriteBase> spriteList = new List<exSpriteBase>();
    
    //mesh
#if EX_DEBUG
    [SerializeField]
#endif
    private Mesh mesh;

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
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    public int vertexCount {
        get {
            return vertices.Count;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
        cachedRenderer = gameObject.GetComponent<MeshRenderer>();
    } 

    void OnDestroy () {
        RemoveAll();
        layer = null;
        cachedRenderer = null;
        mesh = null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Create a new GameObject contains an exMesh component
    // ------------------------------------------------------------------ 

    public static exMesh Create (exLayer _layer) {
        GameObject go = new GameObject("_exMesh");
        //go.hideFlags = exReleaseFlag.hideAndDontSave;
        go.transform.parent = _layer.transform;
        exMesh res = go.AddComponent<exMesh>();
        res.layer = _layer;
        res.CreateMesh();
        return res;
    }

    // ------------------------------------------------------------------ 
    /// Maintains a mesh to render all sprites
    // ------------------------------------------------------------------ 

    public void UpdateMesh () {
        for (int i = 0; i < spriteList.Count; ++i) {
            exSpriteBase sprite = spriteList[i];
            exDebug.Assert(sprite.isOnEnabled == sprite.isInIndexBuffer);

            if (sprite.isOnEnabled) {
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
            mesh.triangles = null;  //这里如果使用clear，那么uv和color就必须赋值，否则有时会出错
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
            bool visible =  (indices.Count > 0);
            if (gameObject.activeSelf != visible) {
                gameObject.SetActive(visible);
            }
        }
        else if((updateFlags & UpdateFlags.Vertex) != 0) { 
            // 如果没有更新triangles并且更新了vertex位置，则需要手动更新bbox
            mesh.RecalculateBounds();
        }
        if ((updateFlags & UpdateFlags.Normal) != 0) {
            var normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; ++i) {
                normals[i] = new Vector3(0, 0, -1);
            }
            mesh.normals = normals;
        }
        updateFlags = UpdateFlags.None;
    }

    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to this mesh. 
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

    public void Remove (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(layer, _sprite.layer);
        exDebug.Assert(hasSprite == spriteList.Contains(_sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to remove");
            return;
        }
        spriteList.RemoveAt(_sprite.spriteIndex);
        
        for (int i = _sprite.spriteIndex; i < spriteList.Count; ++i) {
            exSpriteBase sprite = spriteList[i];
            // update sprite and vertic index after removed sprite
            sprite.spriteIndex = i;
            sprite.vertexBufferIndex -= _sprite.vertexCount;
            // update indices to make them match new vertic index
            if (sprite.isInIndexBuffer) {
                for (int index = sprite.indexBufferIndex; index < sprite.indexBufferIndex + sprite.indexCount; ++index) {
                    indices[index] -= _sprite.vertexCount;
                }
            }
        }
        updateFlags |= UpdateFlags.VertexAndIndex;

        // update vertices
        vertices.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);
        colors32.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);
        uvs.RemoveRange(_sprite.vertexBufferIndex, _sprite.vertexCount);

#if FORCE_UPDATE_VERTEX_INFO
        bool removeBack = (_sprite.spriteIndex == spriteList.Count);
        if (!removeBack) {
            updateFlags |= (UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);
        }
#else
        updateFlags |= (UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);
#endif

        if (_sprite.isInIndexBuffer) {
            RemoveIndices(_sprite);
        }

        exDebug.Assert(_sprite.indexBufferIndex == -1);
        exDebug.Assert(vertices.Count == uvs.Count, "uvs array needs to be the same size as the vertices array");
        exDebug.Assert(vertices.Count == colors32.Count, "colors32 array needs to be the same size as the vertices array");
    }
    
    // ------------------------------------------------------------------ 
    /// Show an exSpriteBase
    // ------------------------------------------------------------------ 

    public void ShowSprite (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(layer, _sprite.layer);
        exDebug.Assert(hasSprite == spriteList.Contains(_sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to show");
            return;
        }
        // show
        if (!_sprite.isInIndexBuffer) {
            AddIndices(_sprite);
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void HideSprite (exSpriteBase _sprite) {
        bool hasSprite = object.ReferenceEquals(layer, _sprite.layer);
        exDebug.Assert(hasSprite == spriteList.Contains(_sprite), "wrong sprite.layer");
        if (!hasSprite) {
            Debug.LogError("can't find sprite to hide");
            return;
        }
        // hide
        if (_sprite.isInIndexBuffer) {
            RemoveIndices(_sprite);
        }
        exDebug.Assert(_sprite.indexBufferIndex == -1);
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public bool Contains (exSpriteBase _sprite) {
        bool hasSprite = (_sprite.spriteIndex >= 0 && _sprite.spriteIndex < spriteList.Count && 
                          object.ReferenceEquals(spriteList[_sprite.spriteIndex], _sprite));
#if EX_DEBUG
        exDebug.Assert(hasSprite == spriteList.Contains(_sprite), "wrong sprite.layer");
        bool sameLayer = object.ReferenceEquals(layer, _sprite.layer);
        exDebug.Assert(sameLayer);
        bool sameMaterial = (_sprite.material == material);
        exDebug.Assert(!hasSprite || sameMaterial);
#endif
        return hasSprite;
    }

    // ------------------------------------------------------------------ 
    /// Compact all reserved buffers
    // ------------------------------------------------------------------ 

    public void Compact () {
        // 其实mscorlib在实现TrimExcess时还是会预留10%的buffer
        spriteList.TrimExcess();
        vertices.TrimExcess();
        indices.TrimExcess();
        uvs.TrimExcess();
        colors32.TrimExcess();
#if FORCE_UPDATE_VERTEX_INFO
        updateFlags |= (UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);
#endif
    }
 
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void MarkDynamic () {
        mesh.MarkDynamic();
    }
   
    // ------------------------------------------------------------------ 
    // Output debug info
    // ------------------------------------------------------------------ 

    [ContextMenu("Output Mesh Info")]
    [System.Diagnostics.Conditional("EX_DEBUG")]
    public void OutputDebugInfo () {
        Debug.Log("exLayer MeshInfo: SpriteCount: " + spriteList.Count, this);
        string vertexInfo = "Cache Vertices: ";
        foreach (var v in vertices) {
            vertexInfo += v;
            vertexInfo += ", ";
        }
        Debug.Log(vertexInfo, this);
        
        vertexInfo = "Mesh.vertices: ";
        foreach (var v in mesh.vertices) {
            vertexInfo += v;
            vertexInfo += ", ";
        }
        Debug.Log(vertexInfo, this);

        string indicesInfo = "Cache Indices: ";
        foreach (var index in indices) {
            indicesInfo += index;
            indicesInfo += ",";
        }
        Debug.Log(indicesInfo, this);

        indicesInfo = "Mesh.triangles: ";
        foreach (var index in mesh.triangles) {
            indicesInfo += index;
            indicesInfo += ",";
        }
        Debug.Log(indicesInfo, this);

        string uvInfo = "Cache uvs: ";
        foreach (var uv in uvs) {
            uvInfo += uv;
            uvInfo += ",";
        }
        Debug.Log(uvInfo, this);
        
        uvInfo = "Mesh.uvs: ";
        foreach (var uv in mesh.uv) {
            uvInfo += uv;
            uvInfo += ",";
        }
        Debug.Log(uvInfo, this);
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// Add and resort indices by depth
    // ------------------------------------------------------------------ 

    void AddIndices (exSpriteBase _sprite) {
        exDebug.Assert(!_sprite.isInIndexBuffer);
        if (!_sprite.isInIndexBuffer) {
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
        exDebug.Assert(_sprite.isInIndexBuffer);
        if (_sprite.isInIndexBuffer) {
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

    public void RemoveAll (bool removeSpriteFromLayer = true) {
        if (removeSpriteFromLayer) {
            while (spriteList.Count > 0) {
                exSpriteBase sprite = spriteList[spriteList.Count - 1];
                exDebug.Assert(sprite);
                if (sprite) {
                    sprite.SetLayer(null);
                }
            }
        }
        spriteList.Clear();
        vertices.Clear();
        indices.Clear();
        uvs.Clear();
        colors32.Clear();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void CreateMesh () {
        exDebug.Assert(!mesh);
        if (!mesh) {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            cachedRenderer = gameObject.GetComponent<MeshRenderer>();
            cachedRenderer.receiveShadows = false;
            cachedRenderer.castShadows = false;
            if (!meshFilter.sharedMesh) {
                mesh = new Mesh();
                mesh.name = "ex2D mesh";
                mesh.hideFlags = HideFlags.DontSave;
                meshFilter.sharedMesh = mesh;
            }
            else {
                mesh = meshFilter.sharedMesh;
            }
            if (layer.layerType == LayerType.Dynamic) {
                mesh.MarkDynamic();
            }
        }
    }
}
