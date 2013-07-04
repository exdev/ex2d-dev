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
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// The 2D Manager component
/// For managing exLayers and responsing Camera OnPreRender event
///
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("ex2D/2D Manager")]
public class ex2DMng : MonoBehaviour {
    
    ///////////////////////////////////////////////////////////////////////////////
    // nested classes, enums
    ///////////////////////////////////////////////////////////////////////////////

    public struct MaterialTableKey : System.IComparable<MaterialTableKey>, System.IEquatable<MaterialTableKey> {

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
        if (!UnityEditor.EditorApplication.isPlaying) {
            if (!instance) {
                instance = this;
            }
            cachedCamera = camera;
        }
#endif
    }

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
        RenderScene();
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

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void DestroyAllLayer () {
        for (int i = 0; i < layerList.Count; ++i) {
            exLayer layer = layerList[i];
            if (layer != null) {
                layer.gameObject.Destroy();
            }
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
    public void DestroyLayer (exLayer _layer) {
        exDebug.Assert(layerList.Contains(_layer), "can't find layer in ex2DMng");
        layerList.Remove(_layer);
        if (_layer != null) {
            _layer.gameObject.Destroy();
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
    // Desc: 
    // ------------------------------------------------------------------ 
    
    public void ForceRenderScene () {
        for (int i = 0; i < layerList.Count; ++i) {
            if (layerList[i] != null) {
                layerList[i].enabled = false;
                layerList[i].enabled = true;
            }
        }
        RenderScene();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    public void RenderScene () {
        if (cachedCamera.orthographicSize != Screen.height) {
            //cachedCamera.orthographicSize = Screen.height;
        }
        
        for (int i = 0; i < layerList.Count; ++i) {
            if ( layerList[i] != null )
                layerList[i].UpdateSprites();
        }
    }
}
