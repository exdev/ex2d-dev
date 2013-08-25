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
    
    [System.NonSerialized] protected Renderer cachedRenderer;
    [System.NonSerialized] protected MeshFilter cachedFilter;
    [System.NonSerialized] protected Mesh mesh;
    
    /// cache mesh.vertices
    /// vertices的数量和索引都保持和uvs, colors, normals, tangents一致
    [System.NonSerialized] protected exList<Vector3> vertices = new exList<Vector3>();

    /// cache mesh.triangles (按深度排序)
    // 如果不手动给出QUAD_INDEX_COUNT，按List初始分配个数(4个)，则添加一个quad就要分配两次内存
    [System.NonSerialized] protected exList<int> indices = new exList<int>(exMesh.QUAD_INDEX_COUNT); 

    [System.NonSerialized] protected exList<Vector2> uvs = new exList<Vector2>();       ///< cache mesh.vertices
    [System.NonSerialized] protected exList<Color32> colors32 = new exList<Color32>();  ///< cache mesh.colors32
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized properties
    ///////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    void Awake () {
        cachedFilter = gameObject.GetComponent<MeshFilter>();
        cachedRenderer = gameObject.GetComponent<MeshRenderer>();
        mesh = new Mesh();
        mesh.name = "ex2D Mesh";
        mesh.hideFlags = HideFlags.DontSave;
        cachedFilter.sharedMesh = mesh;
    }
    
    void OnDestroy () {
        vertices = null;
        indices = null;
        uvs = null;
        colors32 = null;
        
        if (mesh != null) {
            mesh.Destroy();
        }
        mesh = null;
        
        //cachedFilter.sharedMesh = null;
        //cachedFilter = null;
        //cachedRenderer.sharedMaterial = null;
        //cachedRenderer = null;
        gameObject.GetComponent<MeshFilter>().sharedMesh = null;
        gameObject.GetComponent<MeshRenderer>().sharedMaterial = null;
    }
    
    void LateUpdate () {
        //exUpdateFlags spriteUpdateFlags = UpdateBuffers(mesh.vertices, mesh.uvs, mesh.colors32, mesh.indices);
        //meshUpdateFlags |= spriteUpdateFlags;
        //mesh.Apply(meshUpdateFlags);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////
}
