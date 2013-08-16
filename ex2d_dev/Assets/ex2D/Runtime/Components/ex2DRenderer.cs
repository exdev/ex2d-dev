// ======================================================================================
// File         : ex2DRenderer.cs
// Author       : Jare
// Last Change  : 06/15/2013 | 23:03:37
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
/// The 2D Renderer component
/// For managing exLayers and responsing Camera OnPreRender event
///
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("ex2D/ex2D Renderer")]
public class ex2DRenderer : MonoBehaviour {
    
    ///////////////////////////////////////////////////////////////////////////////
    // nested classes, enums
    ///////////////////////////////////////////////////////////////////////////////

    public struct MaterialTableKey { /*: System.IComparable<MaterialTableKey>, System.IEquatable<MaterialTableKey>*/ 
        
    // ------------------------------------------------------------------ 
    /// In Xamarin.iOS, if MaterialTableKey is a type as dictionary keys, we should manually implement its IEqualityComparer,
    /// and provide an instance to the Dictionary<TKey, TValue>(IEqualityComparer<TKey>) constructor.
    /// See http://docs.xamarin.com/guides/ios/advanced_topics/limitations for more info.
    // ------------------------------------------------------------------ 
        
        public class Comparer : IEqualityComparer<MaterialTableKey> {
            static Comparer instance_;
            public static Comparer instance {
                get {
                    if (instance_ == null) {
                        instance_ = new Comparer();
                    }
                    return instance_;
                }
            }
            public bool Equals (MaterialTableKey _lhs, MaterialTableKey _rhs) {
                return object.ReferenceEquals(_lhs.shader, _rhs.shader) && object.ReferenceEquals(_lhs.texture, _rhs.texture);
            }
            public int GetHashCode(MaterialTableKey _obj) {
                int shaderHashCode, texHashCode;
                if (_obj.shader != null) {
                    shaderHashCode = _obj.shader.GetHashCode();
                }
                else {
                    shaderHashCode = 0x00000000;
                }
                if (_obj.texture != null) {
                    texHashCode = _obj.texture.GetHashCode() * 1313;
                }
                else {
                    texHashCode = 0x00000000;
                }
                return shaderHashCode ^ texHashCode;
            }
        } 
        
        public Shader shader;
        public Texture texture;

        public MaterialTableKey (Shader _shader, Texture _texture) {
            shader = _shader;
            texture = _texture;
        }
        public MaterialTableKey (Material _material) 
            : this(_material.shader, _material.mainTexture) {
        }
//        public bool Equals (MaterialTableKey _other) {
//            return Comparer.instance.Equals(this, _other);
//        }
//        public override int GetHashCode () {
//            return Comparer.instance.GetHashCode(this);
//        }
//        public int CompareTo(MaterialTableKey _other) {
//            int texCompare = texture.GetHashCode().CompareTo(_other.texture.GetHashCode());
//            if (texCompare == 0) {
//                return shader.GetHashCode().CompareTo(_other.shader.GetHashCode());
//            }
//            else {
//                return texCompare;
//            }
//        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    public List<exLayer> layerList = new List<exLayer>();   ///< 按Layer的渲染次序排序，先渲染的放在前面。

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] public static ex2DRenderer instance;

    private Camera cachedCamera;
    
    private static Dictionary<MaterialTableKey, Material> materialTable = new Dictionary<MaterialTableKey, Material>(MaterialTableKey.Comparer.instance);

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
        cachedCamera = camera;
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void OnEnable () {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            if (!instance) {
                instance = this;
            }
            cachedCamera = camera;
        }
#endif
        for ( int i = 0; i < layerList.Count; ++i ) {
            exLayer layer = layerList[i];
            if ( layer != null ) {
                layer.GenerateMeshes();
            }
        }
        ResortLayerDepth();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void OnDisable () {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            instance = null;
        }
#endif
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    void OnPreRender () {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            return;
        }
#endif
        UpdateLayers();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        instance = null;
        cachedCamera = null;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LateUpdate () {
        for ( int i = layerList.Count-1; i >= 0; --i ) {
            if ( layerList[i] == null )
                layerList.RemoveAt(i);
        }
    }

#if EX_DEBUG
    void Reset () {
        instance = this;
    }
#endif

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////

    //// ------------------------------------------------------------------ 
    //// Desc:
    //// ------------------------------------------------------------------ 

    //public void DestroyAllLayers () {
    //    for (int i = 0; i < layerList.Count; ++i) {
    //        exLayer layer = layerList[i];
    //        if (layer != null) {
    //            layer.gameObject.Destroy();
    //        }
    //    }
    //    layerList.Clear();
    //}

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public exLayer CreateLayer () {
        GameObject layerGo = new GameObject("New Layer");
        exLayer layer = layerGo.AddComponent<exLayer>();
        layerList.Add(layer);
        ResortLayerDepth();
        return layer;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void DestroyLayer ( int _idx ) { DestroyLayer ( layerList[_idx] ); }
    public void DestroyLayer (exLayer _layer) {
        exDebug.Assert(layerList.Contains(_layer), "can't find layer in ex2DRenderer");
        layerList.Remove(_layer);
        if (_layer != null) {
            _layer.gameObject.Destroy();
        }
    }
    
    // ------------------------------------------------------------------ 
    /// 重新排列所有layer的Z轴，使在LayerList越前端的layer能渲染在越上层。
    // ------------------------------------------------------------------ 

    public void ResortLayerDepth () {
        float cameraZ = cachedCamera.transform.position.z + cachedCamera.nearClipPlane;
        float interval = 0.01f;
        //if (Mathf.Abs(cameraZ) < 100000) {
        //    interval = 0.01f;
        //}
        //else {
        //    interval = 1f;
        //}
        float occupiedZ = cameraZ;
        for (int i = 0; i < layerList.Count; ++i) {
            exLayer layer = layerList[i];
            if (layer != null) {
                occupiedZ = layer.SetWorldBoundsMinZ(occupiedZ + interval);
            }
        }
    }

    // ------------------------------------------------------------------ 
    /// Return shared material matchs given shader and texture
    // ------------------------------------------------------------------ 

    public static Material GetMaterial (Shader _shader, Texture _texture) {
        if (_shader == null) {
            _shader = Shader.Find("ex2D/Alpha Blended");
            if (_shader == null) {
                _shader = new Shader();
            }
        }
        MaterialTableKey key = new MaterialTableKey(_shader, _texture);
        Material mat;
        if ( !materialTable.TryGetValue(key, out mat) || mat == null ) {
            mat = new Material(_shader);
            mat.hideFlags = HideFlags.DontSave;
            mat.mainTexture = _texture;
            materialTable[key] = mat;
        }
        return mat;
    }

    // ------------------------------------------------------------------ 
    /// Force refresh all layers. This is called by ex2D.
    // ------------------------------------------------------------------ 
    
    public void ForceRenderScene () {
        for (int i = 0; i < layerList.Count; ++i) {
            exLayer layer = layerList[i];
            if (layer != null) {
                layer.DestroyMeshes();
                layer.GenerateMeshes();
            }
        }
        ResortLayerDepth();
        UpdateLayers();
    }

    // ------------------------------------------------------------------ 
    /// This is called by ex2D when it is about to render the scene.
    // ------------------------------------------------------------------ 
    
    public void UpdateLayers () {
        for (int i = 0; i < layerList.Count; ++i) {
            exLayer layer = layerList[i];
            if (layer != null) {
                layer.UpdateSprites();
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // \param exclusiveCamera 是否独占相机，如果为true，将会自动设置相机的cullingMask
    //
    // Desc:
    // ------------------------------------------------------------------ 

    public void ResetCamera (bool exclusiveCamera) {
        if (cachedCamera.orthographic != true) {
            cachedCamera.orthographic = true;
        }
        if (cachedCamera.orthographicSize != Screen.height/2.0f) {
            cachedCamera.orthographicSize = Screen.height/2.0f;
        }
        cachedCamera.transform.rotation = Quaternion.identity;
        cachedCamera.transform.SetLossyScale(Vector3.one);
        if (exclusiveCamera) {
            // TODO: set unity layer
        }
    }
}
