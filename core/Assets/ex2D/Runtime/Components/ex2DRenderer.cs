// ======================================================================================
// File         : ex2DRenderer.cs
// Author       : 
// Last Change  : 08/17/2013 | 15:27:56
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
[AddComponentMenu("ex2D/2D Renderer")]
public class ex2DRenderer : MonoBehaviour {
        
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    public List<exLayer> layerList = new List<exLayer>();   ///< 按Layer的渲染次序排序，先渲染的放在前面。
    
    [SerializeField] 
    private bool customizeLayerZ_ = false;
    public bool customizeLayerZ {
        get {
            return customizeLayerZ_;
        }
        set { 
            if (customizeLayerZ_ == value) {
                return;
            }
            customizeLayerZ_ = value;
            ResortLayerDepth ();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] private static ex2DRenderer instance_;
    public static ex2DRenderer instance {
        get {
            if (instance_ == null) {
                instance_ = Object.FindObjectOfType (typeof(ex2DRenderer)) as ex2DRenderer;
                // TODO: if not found, create new one ?
            }
            return instance_;
        }
    }
    
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
        if (instance_ == null) {
            instance_ = this;
        }
        cachedCamera = camera;
    }
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    void OnEnable () {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying) {
            if (instance_ == null) {
                instance_ = this;
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
            if (ReferenceEquals(this, instance_)) {
                instance_ = null;
            }
        }
#endif
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        if (ReferenceEquals(this, instance_)) {
            instance_ = null;
        }
        cachedCamera = null;
    }

    // ------------------------------------------------------------------ 
    /// Main update
    // ------------------------------------------------------------------ 

    void LateUpdate () {
        for ( int i = layerList.Count-1; i >= 0; --i ) {
            if ( layerList[i] == null )
                layerList.RemoveAt(i);
        }
        
        UpdateLayers();
    }

#if EX_DEBUG
    void Reset () {
        if (instance_ == null) {
            instance_ = this;
        }
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
    // Desc: 
    // ------------------------------------------------------------------ 

    public void InsertLayer ( int _idx, exLayer _layer ) {
        if ( _idx < 0 )
            _idx = 0;
        if ( _idx >= layerList.Count )
            _idx = layerList.Count;

        layerList.Insert( _idx, _layer );
        ResortLayerDepth();

        //
        _layer.GenerateMeshes();
    }
    
    // ------------------------------------------------------------------ 
    /// Find the layer by name, if not existed, return null
    // ------------------------------------------------------------------ 

    public exLayer GetLayer (string _layerName) {
        for (int i = 0; i < layerList.Count; ++i) {
            exLayer layer = layerList[i];
        	if (layer != null && layer.name == _layerName) {
                return layer;
        	}
        }
        return null;
    }
  
    // ------------------------------------------------------------------ 
    /// 重新排列所有layer的Z轴，使在LayerList越前端的layer能渲染在越上层。
    // ------------------------------------------------------------------ 

    public void ResortLayerDepth () {
        float cameraZ = cachedCamera.transform.position.z + cachedCamera.nearClipPlane;
        float layerInterval = (cachedCamera.farClipPlane - cachedCamera.nearClipPlane) / (layerList.Count + 1);
        float layerZ = cameraZ + layerInterval;
        for (int i = 0; i < layerList.Count; ++i) {
            exLayer layer = layerList[i];
            if (layer != null) {
                if (customizeLayerZ_) {
                    layer.SetWorldBoundsMinZ(layer.customZ);
                }
                else {
                    layer.SetWorldBoundsMinZ(layerZ);
                    layerZ += layerInterval;
                }
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
                return null;
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
