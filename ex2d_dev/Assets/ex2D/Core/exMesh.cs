﻿// ======================================================================================
// File         : exMesh.cs
// Author       : Jare
// Last Change  : 06/23/2013 | 15:27:56
// Description  : 
// ======================================================================================

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
	Normal	    = 16, ///< update the normal, not implemented yet

	VertexAndIndex = (Index | Vertex),
	All = (Index | Vertex | UV | Color | Normal), ///< update all
};

///////////////////////////////////////////////////////////////////////////////
//
/// The exMesh component. All exLayer have at least one.
/// Used to maintain and render the generated mesh, and flush geometry buffers to mesh.
/// This class performs actions selectively depending on what has changed. 
//
///////////////////////////////////////////////////////////////////////////////

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class exMesh : MonoBehaviour
{
    public const int QUAD_INDEX_COUNT = 6;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
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
                UpdateDebugName();
            }
            else {
                Debug.LogError("no MeshRenderer");
            }
        }
    }

    /// sprite序列，用于索引vertices。Only used by exLayer, just place here for convenience.
    public List<exSpriteBase> spriteList = new List<exSpriteBase>();

    /// sprite序列，用于索引indices，顺序和sprite在indices中的顺序一致，也就是按照深度值从小到大排序。Only used by exLayer, just place here for convenience.
    public List<exSpriteBase> sortedSpriteList = new List<exSpriteBase>();
    
    [HideInInspector] public Mesh mesh;

    /// cache mesh.vertices
    /// 依照sprite在spriteList中的相同顺序排列，每个sprite的顶点都放在连续的一段区间中
    /// vertices的数量和索引都保持和uvs, colors, normals, tangents一致
    public List<Vector3> vertices = new List<Vector3>();

    /// cache mesh.triangles (按深度排序)
    // 如果不手动给出，按List初始分配个数(4个)，则添加一个quad就要分配两次内存
    public List<int> indices = new List<int>(QUAD_INDEX_COUNT); 

    public List<Vector2> uvs = new List<Vector2>();       ///< cache mesh.vertices
    public List<Color32> colors32 = new List<Color32>();  ///< cache mesh.colors32

    public UpdateFlags updateFlags = UpdateFlags.None;

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
        CreateMesh();
    }

    void OnDestroy () {
        Clear();
        mesh.Destroy();
        mesh = null;
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = null;
        cachedRenderer.sharedMaterial = null;
        cachedRenderer = null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Create a new GameObject contains an exMesh component
    // ------------------------------------------------------------------ 

    public static exMesh Create (exLayer _layer) {
        GameObject go = new GameObject();
        // 当在EX_DEBUG模式下，如果显示着GO的Inspector，再启动游戏，由于GO是DontSave的，会先被销毁。这时Unity将会报错，但不影响运行，这个问题在类似插件中也会存在。
        go.hideFlags = exReleaseFlags.hideAndDontSave | exReleaseFlags.notEditable;
        exMesh res = go.AddComponent<exMesh>();
        res.UpdateDebugName(_layer.name);
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            res.CreateMesh();
        }
#endif
        return res;
    }

    // ------------------------------------------------------------------ 
    /// Actually apply all buffer changes
    // ------------------------------------------------------------------ 

    public void Apply (UpdateFlags _additionalUpdateFlags = UpdateFlags.None) {
        updateFlags |= _additionalUpdateFlags;
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
            mesh.triangles = indices.ToArray();      // During runtime, assigning triangles will automatically Recalculate the bounding volume.
            bool visible = (indices.Count > 0);
            if (gameObject.activeSelf != visible) {
                gameObject.SetActive(visible);
            }
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying == false) {
                mesh.RecalculateBounds();
            }
#endif
        }
        else if((updateFlags & UpdateFlags.Vertex) != 0) { 
            // 如果没有更新triangles并且更新了vertex位置，则需要手动更新bbox
            mesh.RecalculateBounds();
        }
        if ((updateFlags & UpdateFlags.Normal) != 0) {
            Vector3[] normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; ++i) {
                normals[i] = new Vector3(0, 0, -1);
            }
            mesh.normals = normals;
        }
        updateFlags = UpdateFlags.None;
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
        updateFlags |= (UpdateFlags.Color | UpdateFlags.UV | UpdateFlags.Normal);   //need to flush to mesh if not defined exLayer.FORCE_UPDATE_VERTEX_INFO
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
        if (mesh == null) {
            Debug.Log("mesh is null");
            return;
        }

        string vertexInfo = "Vertex Buffer: ";
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

        string indicesInfo = "Index Buffer: ";
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

        string uvInfo = "UV Buffer: ";
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
    // Desc:
    // ------------------------------------------------------------------ 

    public void Clear () {
        spriteList.Clear();
        sortedSpriteList.Clear();
        vertices.Clear();
        indices.Clear();
        uvs.Clear();
        colors32.Clear();

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isPlaying == false) {
            Apply();
        }
#endif
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void CreateMesh () {
        if (mesh == null) {
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (!meshFilter.sharedMesh) {
                mesh = new Mesh();
                mesh.name = "ex2D mesh";
                mesh.hideFlags = HideFlags.DontSave;
                meshFilter.sharedMesh = mesh;
            }
            else {
                mesh = meshFilter.sharedMesh;
            }
        }
        if (cachedRenderer == null) {
            cachedRenderer = gameObject.GetComponent<MeshRenderer>();
            cachedRenderer.receiveShadows = false;
            cachedRenderer.castShadows = false;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    [System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("EX_DEBUG")]
    void UpdateDebugName (string layerName = null) {
        if (string.IsNullOrEmpty(layerName)) {
            string[] splitted = gameObject.name.Split('@');
            if (splitted.Length > 0) {
                layerName = gameObject.name.Split('@')[1];
            }
        }
        string newName = "_exMesh";
        Material mat = material;
        if (mat != null) {
            if (mat.mainTexture) {
                newName += ("[" + mat.mainTexture.name + "]");
            }
            else {
                newName += ("[" + mat.name + "]");
            }
        }
        newName += ("@" + layerName);
        gameObject.name = newName;
    }
}
