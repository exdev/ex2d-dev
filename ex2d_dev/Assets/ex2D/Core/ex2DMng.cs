// ======================================================================================
// File         : ex2DMng.cs
// Author       : Jare
// Last Change  : 06/15/2013 | 23:03:37
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// The 2D Manager component
/// For managing exLayers and responsing Camera OnPreRender event
///
///////////////////////////////////////////////////////////////////////////////

[RequireComponent(typeof(Camera))]
[AddComponentMenu("ex2D/2D Manager")]
public class ex2DMng : MonoBehaviour {
    
    ///////////////////////////////////////////////////////////////////////////////
    // nested classes, enums
    ///////////////////////////////////////////////////////////////////////////////

    public struct MaterialTableKey : IComparable<MaterialTableKey>, IEquatable<MaterialTableKey> {

        public Shader shader;
        public Texture texture;

        public MaterialTableKey (Shader _shader, Texture _texture) {
            this.shader = _shader;
            this.texture = _texture;
        }
        public MaterialTableKey (Material _material) 
            : this(_material.shader, _material.mainTexture) {
        }
        public bool Equals (MaterialTableKey _other) {
            return object.ReferenceEquals(shader, _other.shader) && object.ReferenceEquals(texture, _other.texture);
        }
        public override int GetHashCode () {
            int shaderHashCode, texHashCode;
            if (shader != null) {
                shaderHashCode = shader.GetHashCode();
            }
            else {
                shaderHashCode = 0x00000000;
            }
            if (texture != null) {
                texHashCode = texture.GetHashCode() * 1313;
            }
            else {
                texHashCode = 0x00000000;
            }
            return shaderHashCode ^ texHashCode;
        }
        public int CompareTo(MaterialTableKey _other) {
            int texCompare = texture.GetHashCode().CompareTo(_other.texture.GetHashCode());
            if (texCompare == 0) {
                return shader.GetHashCode().CompareTo(_other.shader.GetHashCode());
            }
            else {
                return texCompare;
            }
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    public List<exLayer> layerList = new List<exLayer>();

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] public static ex2DMng instance;

    private Camera cachedCamera;
    
    private static Dictionary<MaterialTableKey, Material> materialTable = new Dictionary<MaterialTableKey, Material>();

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void Awake () {
        if (!instance) {
            instance = this;
        }
        //materialTable = new Dictionary<MaterialTableKey, Material>();
        cachedCamera = camera;
        if (cachedCamera.orthographic != true) {
            Debug.LogWarning("Set ex2DMng's camera projection to orthographic");
            cachedCamera.orthographic = true;
        }
    }
    
    // ------------------------------------------------------------------ 
    //
    // ------------------------------------------------------------------ 

    void OnEnable () {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying) {
            //用于重新编译过后，重新初始化非序列化变量
            //exDebug.Assert(materialTable != null);
            if (!instance) {
                instance = this;
            }
            cachedCamera = camera;
            //for (int l = 0; l < layerList.Count; ++l) {
            //    exLayer layer = layerList[l];
            //    for (int i = 0; i < layer.spriteList.Count; ++i) {
            //        exSpriteBase sprite = layer.spriteList[i];
            //        Material mat = sprite.material;
            //        materialTable[new MaterialTableKey(mat)] = mat;
            //    }
            //}
        }
#endif
    }

    void OnDisable () {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying) {
            instance = null;
        }
#endif
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void OnPreRender () {
        RenderScene();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying) {
            DestroyAllLayer();
        }
#else
        DestroyAllLayer();
#endif
        instance = null;
        cachedCamera = null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void DestroyAllLayer () {
        for (int i = 0; i < layerList.Count; ++i) {
            layerList[i].Clear();
        }
        layerList.Clear();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public exLayer CreateLayer () {
        GameObject layerGo = new GameObject("New Layer");
        exLayer layer = layerGo.AddComponent<exLayer>();
        layerList.Add(layer);
        return layer;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void DestroyLayer ( int _idx ) { DestroyLayer ( layerList[_idx] ); }
    public void DestroyLayer (exLayer layer) {
        exDebug.Assert(layerList.Contains(layer), "can't find layer in ex2DMng");
        layerList.Remove(layer);
        layer.Clear();
        layer.gameObject.Destory();
    }
    
    // ------------------------------------------------------------------ 
    /// Return shared material matchs given shader and texture
    // ------------------------------------------------------------------ 

    public static Material GetMaterial (Shader shader, Texture texture) {
        MaterialTableKey key = new MaterialTableKey(shader, texture);
        Material mat;
        if ( ! materialTable.TryGetValue(key, out mat) ) {
            mat = new Material(shader);
            mat.hideFlags = HideFlags.DontSave;
            mat.mainTexture = texture;
            materialTable.Add(key, mat);
        }
        return mat;
    }

    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void RenderScene () {
        if (cachedCamera.orthographicSize != Screen.height) {
            //cachedCamera.orthographicSize = Screen.height;
        }
        
        for (int i = 0; i < layerList.Count; ++i) {
            layerList[i].UpdateAllMeshes();
        }
    }
}
