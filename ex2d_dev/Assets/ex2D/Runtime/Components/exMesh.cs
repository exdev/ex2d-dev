// ======================================================================================
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
public enum exUpdateFlags {
	None		= 0,  ///< none
	Index	    = 1,  ///< update the indices
	Vertex		= 2,  ///< update the vertices
	UV	        = 4,  ///< update the uv coordination
	Color	    = 8,  ///< update the vertex color
    Normal      = 16, ///< update the normal, not implemented yet
    Text	    = 32, ///< update the text, only used in sprite font

	VertexAndIndex = (Index | Vertex),
	AllExcludeIndex = (Vertex | UV | Color | Normal | Text),
	All = (AllExcludeIndex | Index),
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
    public static bool enableDoubleBuffer = true;   // for profiling

    public const int QUAD_INDEX_COUNT = 6;
    public const int QUAD_VERTEX_COUNT = 4;
    public const int MAX_VERTEX_COUNT = 65000;
    public const int MAX_QUAD_COUNT = MAX_VERTEX_COUNT / QUAD_VERTEX_COUNT;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    // 这个类由exLayer动态创建的，不会进行任何序列化操作，所以字段的序列化标记其实没用。
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] private Renderer cachedRenderer;
    [System.NonSerialized] private MeshFilter cachedFilter;

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
    [System.NonSerialized] public List<exSpriteBase> spriteList = new List<exSpriteBase>();

    /// sprite序列，用于索引indices，顺序和sprite在indices中的顺序一致，也就是按照深度值从小到大排序。Only used by exLayer, just place here for convenience.
    /// 可用此序列访问到所有在能在mesh显示的sprite
    [System.NonSerialized] public List<exSpriteBase> sortedSpriteList = new List<exSpriteBase>();

    [System.NonSerialized] private Mesh mesh0;    ///< first mesh buffer
    [System.NonSerialized] private Mesh mesh1;    ///< second mesh buffer, only used in dynamic mode
    [System.NonSerialized] private bool isEvenMeshBuffer = true; ///< select first mesh or second

    /// cache mesh.vertices
    /// 依照sprite在spriteList中的相同顺序排列，每个sprite的顶点都放在连续的一段区间中
    /// vertices的数量和索引都保持和uvs, colors, normals, tangents一致
    [System.NonSerialized] public List<Vector3> vertices = new List<Vector3>();

    /// cache mesh.triangles (按深度排序)
    // 如果不手动给出QUAD_INDEX_COUNT，按List初始分配个数(4个)，则添加一个quad就要分配两次内存
    [System.NonSerialized] public List<int> indices = new List<int>(QUAD_INDEX_COUNT); 

    [System.NonSerialized] public List<Vector2> uvs = new List<Vector2>();       ///< cache mesh.vertices
    [System.NonSerialized] public List<Color32> colors32 = new List<Color32>();  ///< cache mesh.colors32

    [System.NonSerialized] public exUpdateFlags updateFlags = exUpdateFlags.None;         ///< current mesh buffer update flags
    [System.NonSerialized] public exUpdateFlags lastUpdateFlags = exUpdateFlags.None;     ///< last mesh buffer update flags

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    public int vertexCount {
        get {
            return vertices.Count;
        }
    }

    public bool hasTriangle {
        get {
            return sortedSpriteList.Count > 0;
        }
    }

    private bool isDynamic {
        get {
            return (!ReferenceEquals(mesh0, null) && !ReferenceEquals(mesh1, null));    // default is false
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake () {
        Init();
    }

    void OnDestroy () {
        Clear();
        if (mesh0 != null) {
            mesh0.Destroy();
        }
        mesh0 = null;
        if (mesh1 != null) {
            mesh1.Destroy();
        }
        mesh1 = null;
        cachedFilter.sharedMesh = null;
        cachedFilter = null;
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
        res.Init();
        return res;
    }

    // ------------------------------------------------------------------ 
    /// Actually apply all buffer changes
    // ------------------------------------------------------------------ 

    public void Apply (exUpdateFlags _additionalUpdateFlags = exUpdateFlags.None) {
        updateFlags |= _additionalUpdateFlags;

        Mesh mesh;
        if (isDynamic && updateFlags != exUpdateFlags.None) {
            mesh = SwapMeshBuffer();
        }
        else {
            mesh = GetMeshBuffer();
        }
        
        if ((updateFlags & exUpdateFlags.VertexAndIndex) == exUpdateFlags.VertexAndIndex) {
            // 如果索引还未更新就减少顶点数量，索引可能会成为非法的，所以这里要把索引一起清空
            mesh.triangles = null;  //这里如果使用clear，那么uv和color就必须赋值，否则有时会出错
        }
        if ((updateFlags & exUpdateFlags.Vertex) != 0 || 
            (updateFlags & exUpdateFlags.Index) != 0) {           // 如果要重设triangles，则必须同时重设vertices，否则mesh将显示不出来
            mesh.vertices = vertices.ToArray();
        }
        if ((updateFlags & exUpdateFlags.UV) != 0) {
            mesh.uv = uvs.ToArray();
        }
        if ((updateFlags & exUpdateFlags.Color) != 0) {
            mesh.colors32 = colors32.ToArray();
        }
        if ((updateFlags & exUpdateFlags.Index) != 0) {
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
        else if((updateFlags & exUpdateFlags.Vertex) != 0) { 
            // 如果没有更新triangles并且更新了vertex位置，则需要手动更新bbox
            mesh.RecalculateBounds();
        }
        if ((updateFlags & exUpdateFlags.Normal) != 0) {
            Vector3[] normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; ++i) {
                normals[i] = new Vector3(0, 0, -1);
            }
            mesh.normals = normals;
        }
        updateFlags = exUpdateFlags.None;
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
        updateFlags |= (exUpdateFlags.Color | exUpdateFlags.UV | exUpdateFlags.Normal);   //need to flush to mesh if not defined exLayer.FORCE_UPDATE_VERTEX_INFO
    }
 
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void SetDynamic (bool _dynamic) {
        //Debug.Log(string.Format("[SetDynamic|exMesh] _dynamic: {0} isDynamic: " + isDynamic, _dynamic));
        if (isDynamic == _dynamic) {
            return;
        }
        if (_dynamic) {
            // create all buffer
            exDebug.Assert(mesh0 != null);
            if (mesh0 == null) {
                mesh0 = CreateMesh();
            }
            mesh0.MarkDynamic();

            exDebug.Assert(mesh1 == null);
            if (mesh1 == null) {
                mesh1 = CreateMesh();
            }
            mesh1.MarkDynamic();

            lastUpdateFlags = exUpdateFlags.All;    // init new created mesh buffer
        }
        else {
            if (isEvenMeshBuffer != true) {
                isEvenMeshBuffer = true;
                updateFlags |= lastUpdateFlags;
            }
            // destroy another buffer
            if (mesh1 != null) {
                mesh1.Destroy();
            }
            mesh1 = null;
        }
    }

    // ------------------------------------------------------------------ 
    // Output debug info
    // ------------------------------------------------------------------ 

    [ContextMenu("Output Mesh Info")]
    [System.Diagnostics.Conditional("EX_DEBUG")]
    public void OutputDebugInfo () {
        Mesh mesh = GetMeshBuffer();
        if (mesh == null) {
            Debug.Log("mesh is null");
            return;
        }

        Debug.Log(string.Format("exMesh SpriteCount: {0} Current mesh buffer: {1}", spriteList.Count, isEvenMeshBuffer ? 0 : 1), this);

        string vertexInfo = "Vertex Buffer: ";
        //foreach (var v in vertices) {
        //    vertexInfo += v;
        //    vertexInfo += ", ";
        //}
        //Debug.Log(vertexInfo, this);
        
        vertexInfo = "Mesh.vertices[" + mesh.vertexCount + "]: ";
        foreach (var v in mesh.vertices) {
            vertexInfo += v.ToString("F3");
            vertexInfo += ", ";
        }
        Debug.Log(vertexInfo, this);

        string indicesInfo = "Index Buffer: ";
        //foreach (var index in indices) {
        //    indicesInfo += index;
        //    indicesInfo += ",";
        //}
        //Debug.Log(indicesInfo, this);

        indicesInfo = "Mesh.indices[" + mesh.triangles.Length + "]: ";
        foreach (var index in mesh.triangles) {
            indicesInfo += index;
            indicesInfo += ",";
        }
        Debug.Log(indicesInfo, this);

        string uvInfo = "UV Buffer: ";
        //foreach (var uv in uvs) {
        //    uvInfo += uv;
        //    uvInfo += ",";
        //}
        //Debug.Log(uvInfo, this);
        
        uvInfo = "Mesh.uvs: ";
        foreach (var uv in mesh.uv) {
            uvInfo += uv.ToString("F4");
            uvInfo += ",";
        }
        Debug.Log(uvInfo, this);

        uvInfo = "Mesh.colors: ";
        foreach (var c in mesh.colors) {
            uvInfo += c;
            uvInfo += ",";
        }
        Debug.Log(uvInfo, this);

        uvInfo = "Mesh.normals: ";
        foreach (var n in mesh.normals) {
            uvInfo += n;
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
        if (mesh0 != null) {
            mesh0.Clear();
        }
        if (mesh1 != null) {
            mesh1.Clear();
        }
        updateFlags = exUpdateFlags.None;
    }

    // ------------------------------------------------------------------ 
    /// If we are using dynamic layer, the mesh is double buffered so that we can get the best performance on iOS devices.
    /// http://forum.unity3d.com/threads/118723-Huge-performance-loss-in-Mesh.CreateVBO-for-dynamic-meshes-IOS
    // ------------------------------------------------------------------ 
    
    private Mesh SwapMeshBuffer() {
        exDebug.Assert(isDynamic);
        if (enableDoubleBuffer) {
    		isEvenMeshBuffer = !isEvenMeshBuffer;
            exUpdateFlags currentBufferUpdate = updateFlags;
            updateFlags |= lastUpdateFlags;          // combine changes during two frame
            lastUpdateFlags = currentBufferUpdate;   // for next buffer
        }
        return GetMeshBuffer();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    private Mesh GetMeshBuffer () {
        if (isEvenMeshBuffer) {
            if (mesh0 == null) {
                mesh0 = CreateMesh();
            }
            if (cachedFilter.sharedMesh != mesh0) {
                cachedFilter.sharedMesh = mesh0;
            }
            return mesh0;
        }
        else {
            exDebug.Assert(isDynamic);
            if (mesh1 == null) {
                mesh1 = CreateMesh();
            }
            if (cachedFilter.sharedMesh != mesh1) {
                cachedFilter.sharedMesh = mesh1;
            }
            return mesh1;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private Mesh CreateMesh () {
        Mesh mesh = new Mesh();
        mesh.name = "ex2D mesh";
        mesh.hideFlags = HideFlags.DontSave;
        return mesh;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void Init () {
        if (cachedFilter == null) {
            cachedFilter = gameObject.GetComponent<MeshFilter>();
        }
        if (mesh0 == null) {
            mesh0 = CreateMesh();
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
#if UNITY_EDITOR || EX_DEBUG
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
#endif
    }
}
