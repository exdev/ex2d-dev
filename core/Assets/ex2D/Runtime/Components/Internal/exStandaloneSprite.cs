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
        mesh.MarkDynamic();
    }
    
    protected new void OnDestroy () {
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

    protected new void OnEnable () {
        isOnEnabled = true;
        Show ();
        bool reloadNonSerialized = (vertices.Count == 0);
        if (reloadNonSerialized) {
            cachedRenderer.sharedMaterial = material;
            exDebug.Assert(indices.Count == 0 && uvs.Count == 0 && colors32.Count == 0);
            UpdateVertexAndIndexCount ();
            FillBuffers();
        }
    }

    // ------------------------------------------------------------------ 
    /// OnDisable functoin inherit from MonoBehaviour,
    /// exStandaloneSprite will disable the renderer if they exist. 
    /// 
    /// \note if you inherit from exStandaloneSprite, and implement your own OnDisable function, 
    /// you need to override this and call base.OnDisable() in your OnDisable block.
    // ------------------------------------------------------------------ 

    protected new void OnDisable () {
        isOnEnabled = false;
        Hide ();
    }

    // ------------------------------------------------------------------ 
    /// Update mesh
    // ------------------------------------------------------------------ 
    
    protected new void LateUpdate () {
        if (updateFlags != exUpdateFlags.None && visible) {
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

    // ------------------------------------------------------------------ 
    /// Get world vertices of the sprite
    /// NOTE: This function returns an empty array If sprite is invisible
    // ------------------------------------------------------------------ 

    public override Vector3[] GetWorldVertices () {
        Vector3[] dest = GetVertices(Space.Self);   // standalone sprite can only get local vertices.
        Matrix4x4 l2w = transform.localToWorldMatrix;
        for (int i = 0; i < dest.Length; ++i) {
            dest[i] = l2w.MultiplyPoint3x4 (dest[i]);
        }
        return dest;
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \NOTE: 此方法调用后必须同时刷新buffer大小
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
            FillBuffers ();
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    private void FillBuffers () {
        vertices.AddRange(vertexCount_);
        colors32.AddRange(vertexCount_);
        uvs.AddRange(vertexCount_);
        indices.AddRange (indexCount_);
        updateFlags |= exUpdateFlags.All;
    }
}
