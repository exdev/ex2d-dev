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
    //[System.NonSerialized] protected MeshFilter cachedFilter_;
    //public MeshFilter cachedFilter {
    //    get {
    //        if (cachedFilter_ == null) {
    //            cachedFilter_ = GetComponent<MeshFilter>();
    //        }
    //        return cachedFilter_;
    //    }
    //}
    [System.NonSerialized] protected Mesh mesh_;
    public Mesh mesh {
        get {
            if (mesh_ == null) {
                mesh_ = GetComponent<MeshFilter>().sharedMesh;
            }
            return mesh_;
        }
        private set {
            mesh_ = value;
            GetComponent<MeshFilter>().sharedMesh = mesh_;
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
        exDebug.Assert(mesh_ == null, "mesh_ == null");
        mesh = new Mesh();
        mesh.name = "ex2D Mesh";
        mesh.hideFlags = HideFlags.DontSave;
    }
    
    protected void OnDestroy () {
        vertices = null;
        indices = null;
        uvs = null;
        colors32 = null;
        
        if (mesh != null) {
            mesh_.Destroy();
        }
        mesh = null;
        cachedRenderer.sharedMaterial = null;
        cachedRenderer_ = null;
    }

    // ------------------------------------------------------------------ 
    /// OnEnable functoin inherit from MonoBehaviour,
    /// exStandaloneSprite will enable the renderer if they exist. 
    /// 
    /// \note if you inherit from exStandaloneSprite, and implement your own OnEnable function, 
    /// you need to override this and call base.OnEnable() in your OnEnable block.
    // ------------------------------------------------------------------ 

    protected void OnEnable () {
        isOnEnabled_ = true;
        Show ();
        bool reloadNonSerialized = (vertices.Count == 0);
        if (reloadNonSerialized) {
            cachedRenderer.sharedMaterial = material;
            FillBuffers(vertices, uvs, colors32);
        }
    }

    // ------------------------------------------------------------------ 
    /// OnDisable functoin inherit from MonoBehaviour,
    /// exStandaloneSprite will disable the renderer if they exist. 
    /// 
    /// \note if you inherit from exStandaloneSprite, and implement your own OnDisable function, 
    /// you need to override this and call base.OnDisable() in your OnDisable block.
    // ------------------------------------------------------------------ 

    protected void OnDisable () {
        isOnEnabled_ = false;
        Hide ();
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

    protected override void Show () {
        cachedRenderer.enabled = true;
    }

    protected override void Hide () {
        cachedRenderer.enabled = false;
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    protected override void UpdateMaterial () {
        material_ = null;   // set dirty, make material update.
        cachedRenderer.sharedMaterial = material;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    internal override float GetScaleX (Space _space) {
        if (_space == Space.World) {
            return transform.lossyScale.x;
        }
        else {
            return transform.localScale.x;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    internal override float GetScaleY (Space _space) {
        if (_space == Space.World) {
            return transform.lossyScale.y;
        }
        else {
            return transform.localScale.y;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    internal override void FillBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32) {
        UpdateVertexAndIndexCount ();
        // fill vertex buffer
        base.FillBuffers (_vertices, _uvs, _colors32);
        // fill index buffer
        indices.AddRange (indexCount);
        updateFlags |= exUpdateFlags.Index;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected abstract void UpdateVertexAndIndexCount ();
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void UpdateBufferSize () {
        int oldVertexCount = vertexCount_;
        int oldIndexCount = indexCount_;
        UpdateVertexAndIndexCount ();
        if (vertexCount_ != oldVertexCount || indexCount_ != oldIndexCount) {
            // re-alloc buffer
            vertices.Clear ();
            uvs.Clear ();
            colors32.Clear ();
            indices.Clear ();
            FillBuffers (vertices, uvs, colors32);
        }
    }
}
