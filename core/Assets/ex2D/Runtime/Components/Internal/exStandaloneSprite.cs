// ======================================================================================
// File         : exStandaloneSprite.cs
// Author       : 
// Last Change  : 08/25/2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// The standalone sprite component which maintains its own Mesh
///
///////////////////////////////////////////////////////////////////////////////
/// 
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public abstract class exStandaloneSprite : exSpriteBase {
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // TODO: scale ?

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] protected Renderer cachedRenderer_;
    public Renderer cachedRenderer {
        get {
            if (cachedRenderer_ == null) {
                cachedRenderer_ = renderer;
            }
            return cachedRenderer_;
        }
    }
    [System.NonSerialized] protected MeshFilter cachedFilter_;
    public MeshFilter cachedFilter {
        get {
            if (cachedFilter_ == null) {
                cachedFilter_ = GetComponent<MeshFilter>();
            }
            return cachedFilter_;
        }
    }
    [System.NonSerialized] protected Mesh mesh_;
    public Mesh mesh {
        get {
            if (mesh_ == null) {
                mesh_ = cachedFilter.sharedMesh;
            }
            return mesh_;
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // cached geometry buffers
    ///////////////////////////////////////////////////////////////////////////////

    /// cache mesh_.vertices
    /// vertices的数量和索引都保持和uvs, colors, normals, tangents一致
    [System.NonSerialized] protected exList<Vector3> vertices = new exList<Vector3>();

    /// cache mesh_.triangles (按深度排序)
    // 如果不手动给出QUAD_INDEX_COUNT，按List初始分配个数(4个)，则添加一个quad就要分配两次内存
    [System.NonSerialized] protected exList<int> indices = new exList<int>(exMesh.QUAD_INDEX_COUNT); 

    [System.NonSerialized] protected exList<Vector2> uvs = new exList<Vector2>();       ///< cache mesh_.vertices
    [System.NonSerialized] protected exList<Color32> colors32 = new exList<Color32>();  ///< cache mesh_.colors32
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    protected void Awake () {
        //cachedFilter_ = gameObject.GetComponent<MeshFilter>();
        //cachedRenderer_ = gameObject.GetComponent<MeshRenderer>();
        exDebug.Assert(mesh_ == null, "mesh_ == null");
        Mesh mesh = new Mesh();
        mesh.name = "ex2D Mesh";
        mesh.hideFlags = HideFlags.DontSave;
        cachedFilter.sharedMesh = mesh;

        FillBuffers(vertices, uvs, colors32);
    }
    
    protected void OnDestroy () {
        vertices = null;
        indices = null;
        uvs = null;
        colors32 = null;
        
        if (mesh != null) {
            mesh_.Destroy();
        }
        mesh_ = null;
        cachedFilter.sharedMesh = null;
        cachedFilter_ = null;
        cachedRenderer.sharedMaterial = null;
        cachedRenderer_ = null;
    }

    // ------------------------------------------------------------------ 
    /// OnEnable functoin inherit from MonoBehaviour,
    /// When exPlane.enabled set to true, this function will be invoked,
    /// exPlane will enable the renderer if they exist. 
    /// 
    /// \note if you inherit from exPlane, and implement your own Awake function, 
    /// you need to override this and call base.OnEnable() in your OnEnable block.
    // ------------------------------------------------------------------ 

    protected void OnEnable () {
        cachedRenderer.enabled = true;

        bool reloadNonSerialized = (vertices.Count == 0);
        if (reloadNonSerialized) {
            FillBuffers(vertices, uvs, colors32);
            //Vector3[] v = mesh.vertices;
            //vertices.FromArray (ref v);
            //int[] i = mesh.triangles;
            //indices.FromArray(ref i);
            //Vector2[] uv = mesh.uv;
            //uvs.FromArray(ref uv);
            //Color32[] c = mesh.colors32;
            //colors32.FromArray(ref c);
        }
    }

    // ------------------------------------------------------------------ 
    /// OnDisable functoin inherit from MonoBehaviour,
    /// When exPlane.enabled set to false, this function will be invoked,
    /// exPlane will disable the renderer if they exist. 
    /// 
    /// \note if you inherit from exPlane, and implement your own Awake function, 
    /// you need to override this and call base.OnDisable() in your OnDisable block.
    // ------------------------------------------------------------------ 

    protected void OnDisable () {
        cachedRenderer.enabled = false;
    }

    // ------------------------------------------------------------------ 
    /// Update mesh
    // ------------------------------------------------------------------ 
    
    void LateUpdate () {
        if (visible) {
            exUpdateFlags meshUpdateFlags = UpdateBuffers (vertices, uvs, colors32, indices);
            exMesh.FlushBuffers (mesh, meshUpdateFlags, vertices, indices, uvs, colors32);
            if ((meshUpdateFlags & exUpdateFlags.Index) != 0) {
                bool realVisible = (indices.Count > 0);
                if (cachedRenderer.enabled != realVisible) {
                    cachedRenderer.enabled = realVisible;
                }
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////
}
