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
/// Layer manager
/// NOTE: Don't add this component yourself, use exLayer.Create instead.
//
///////////////////////////////////////////////////////////////////////////////

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class exLayer : MonoBehaviour 
{
    public enum LayerType {
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
            if(value == LayerType.Dynamic && meshFilter && meshFilter.mesh) {
                meshFilter.mesh.MarkDynamic();
            }
        }
    }

    private bool dirty = true;  //@TODO: 对dirty进行更新
    private MeshFilter meshFilter;

    //按深度排序，深度一样的话，按加入的时间排序
    List<exSpriteBase> allSprites = new List<exSpriteBase>();

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }
    
    void OnDestroy() {
        RemoveAll();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Create a new GameObject contains an exLayer component
    // ------------------------------------------------------------------ 
    
    public static exLayer Create(ex2DMng _2dMng) {
        GameObject go = new GameObject("exLayer");
        go.hideFlags = exReleaseFlag.hideAndDontSave;
        go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.receiveShadows = false;
        mr.castShadows = false;
        //@TODO: 对Material进行设置
        exLayer res = go.AddComponent<exLayer>();
        res.CreateMesh();
        _2dMng.Add(res);
        return res;
    }
    
    // ------------------------------------------------------------------ 
    /// Maintains a mesh to render all sprites
    // ------------------------------------------------------------------ 
    
    public void UpdateMesh() {
        if (!dirty) {
            return;
        }
        //@TODO: 删除下列测试代码，根据allSprites顺序检查dirty标记，然后根据sprite类型来更新mesh
        /* sprite变化时，如果整个mesh共用一份数据缓存，做法是
         * 加入/删除或顶点数量改变：更新所有位于所在顶点之后的数据
         * 显示/隐藏：重建indices
         * 
         * 顶点位移(缩放旋转)：修改部分vertices
         * 整个位移：修改部分vertices
         * 改变UV：修改部分uvs
         * 改变颜色：修改部分colors
         * 
         * 改变深度：重建indices
         * 改变材质(贴图)：暂不考虑
         * 
         * 为了减少GC，把所有mesh数据缓存到layer里，而不是直接从mesh重新拿。
         */

        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        int[] indices = new int[6];
        Vector2[] uvs = new Vector2[4];
        Color[] colors = new Color[4];

        // build vertices & normals
        vertices[0] = new Vector3( -1.0f, -1.0f, 0.0f );
        vertices[1] = new Vector3( -1.0f,  1.0f, 0.0f );
        vertices[2] = new Vector3(  1.0f,  1.0f, 0.0f );
        vertices[3] = new Vector3(  1.0f, -1.0f, 0.0f );

        normals[0] = new Vector3( 0.0f, 0.0f, -1.0f );
        normals[1] = new Vector3( 0.0f, 0.0f, -1.0f );
        normals[2] = new Vector3( 0.0f, 0.0f, -1.0f );
        normals[3] = new Vector3( 0.0f, 0.0f, -1.0f );

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        indices[3] = 3;
        indices[4] = 0;
        indices[5] = 2;

        exSprite sprite = (allSprites[0] as exSprite);
        exTextureInfo textureInfo = sprite.textureInfo;
        float xStart  = (float)textureInfo.x / (float)textureInfo.texture.width;
        float yStart  = (float)textureInfo.y / (float)textureInfo.texture.height;
        float xEnd    = (float)(textureInfo.x + textureInfo.width) / (float)textureInfo.texture.width;
        float yEnd    = (float)(textureInfo.x + textureInfo.height) / (float)textureInfo.texture.height;

        uvs[0] = new Vector2 ( xStart,  yStart );
        uvs[1] = new Vector2 ( xStart,  yEnd );
        uvs[2] = new Vector2 ( xEnd,    yEnd );
        uvs[3] = new Vector2 ( xEnd,    yStart );

        colors[0] = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
        colors[1] = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
        colors[2] = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
        colors[3] = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

        //offset
        meshFilter.mesh.vertices = vertices; //sprite 0 - 10
        meshFilter.mesh.normals = normals; 
        meshFilter.mesh.triangles = indices; //Assigning triangles will automatically Recalculate the bounding volume.
        meshFilter.mesh.uv = uvs;
        meshFilter.mesh.colors = colors;
    }
    
    // ------------------------------------------------------------------ 
    /// Add an exSpriteBase to this layer. 
    /// NOTE: This function should only be called by exSpriteBase
    // ------------------------------------------------------------------ 
    
    public void Add(exSpriteBase sprite) {
        //if(!object.ReferenceEquals(this, sprite.layer))
        exAssert.False(allSprites.Contains(sprite), "can't add duplicated sprite");
        sprite.layerCachedIndex = allSprites.Count;
        allSprites.Add(sprite);
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove(exSpriteBase sprite) {
        allSprites.RemoveAt(sprite.layerCachedIndex);
        for (int i = sprite.layerCachedIndex; i < allSprites.Count ; ++i) {
            allSprites[i].layerCachedIndex = i;
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    public void Optimize() {
        exAssert.True(layerType == LayerType.Dynamic, "consider using Static LayerType", false);
        meshFilter.mesh.Optimize();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// Remove all exSpriteBases out of this layer 
    // ------------------------------------------------------------------ 
    
    void RemoveAll() {
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
    
    void CreateMesh() {
        if (!meshFilter.sharedMesh) {
            meshFilter.mesh.name = "exLayer mesh";  //a new mesh will be created and assigned
            meshFilter.mesh.hideFlags = HideFlags.DontSave;
        }
        if (layerType == LayerType.Dynamic) {
            meshFilter.mesh.MarkDynamic();
        }
    }
}