using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//@TODO: 由Sprite创建Layer，同时设好renderer的属性。

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class exLayer : MonoBehaviour {
    
    public enum LayerType {
        Static = 0,
        Dynamic,
    }

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

    void Awake() {
        meshFilter = GetComponent<MeshFilter>();
    }

    public static exLayer Create(ex2DMng _2dMng) {
        GameObject go = new GameObject("exLayer");
        go.hideFlags = exReleaseFlag.hideAndDontSave;
        go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.receiveShadows = false;
        mr.castShadows = false;
        //@TODO: 对Material进行设置
        exLayer res = go.AddComponent<exLayer>();
        _2dMng.Add(res);
        return res;
    }

    public void UpdateMesh() {
        if (!dirty) {
            return;
        }

        if (!meshFilter.mesh) {
            meshFilter.mesh = new Mesh();
            meshFilter.mesh.name = "exLayer mesh";
            meshFilter.mesh.hideFlags = HideFlags.DontSave;
            if (layerType == LayerType.Dynamic) {
                meshFilter.mesh.MarkDynamic();
            }
        }
        
        //@TODO: 删除下列测试代码，根据allSprites顺序检查dirty标记，然后根据sprite类型来更新mesh

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
        meshFilter.mesh.triangles = indices; //depth 0 - 10
        meshFilter.mesh.uv = uvs;
        meshFilter.mesh.colors = colors;
    }

    //should only called by exSpriteBase
    public void Add(exSpriteBase sprite) {
#if UNITY_EDITOR
        //if(!object.ReferenceEquals(this, sprite.layer))
        if (allSprites.Contains(sprite)) {
            Debug.LogError("can't add duplicated sprite");
            return;
        }
#endif
        sprite.layerCachedIndex = allSprites.Count;
        allSprites.Add(sprite);
    }

    public void Remove(exSpriteBase sprite) {
        allSprites.RemoveAt(sprite.layerCachedIndex);
        for (int i = sprite.layerCachedIndex; i < allSprites.Count ; ++i) {
            allSprites[i].layerCachedIndex = i;
        }
    }

    void RemoveAll() {
        while (allSprites.Count > 0) {
            exSpriteBase sprite = allSprites[allSprites.Count - 1];
            sprite.layer = null;
        }
        allSprites.Clear();
        allSprites = null;
    }

    void OnDestroy() {
        RemoveAll();
    }

}