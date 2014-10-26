// ======================================================================================
// File         : CheckBatchingWindow.cs
// Author       : Jare Guo
// Last Change  : 10/25/2014
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

class CheckBatchingWindow : EditorWindow {

    class LayerInfo {
        public exLayer layer;
        public int materials;
        public List<BatchInfo> batchInfoList = new List<BatchInfo>();
        public bool folded;
    }

    class BatchInfo {
        public exMesh mesh;
        public int index;
        public string name;
        public float pos_z;
        public bool folded;
    }

    // ------------------------------------------------------------------ 
    /// Register menu
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/Debugger/Check Batching")]
    public static CheckBatchingWindow NewWindow () {
        return EditorWindow.GetWindow<CheckBatchingWindow>();
    }

    // ------------------------------------------------------------------ 
    // Properties
    // ------------------------------------------------------------------ 

    bool autoRefresh = false;

    int totalMaterialCount = -1;
    int totalBatchCount = -1;
    int totalSpriteCount = -1;
    int totalActiveSpriteCount = -1;
    int totalActiveMeshCount = -1;

    List<LayerInfo> layerInfoList = new List<LayerInfo>();

    AnimBool showStatistics = new AnimBool(true);
    AnimBool showBatches = new AnimBool(true);
    Vector2 scrollPos;

    // ------------------------------------------------------------------ 
    // Builtin Events 
    // ------------------------------------------------------------------ 

    void OnEnable() {
        showStatistics = new AnimBool(showStatistics.target, Repaint);
        showBatches = new AnimBool(showBatches.target, Repaint);
        Refresh();
    }

    void OnDestroy() {
        layerInfoList.Clear();
    }

    void OnFocus() {
        Refresh();
    }

    void OnInspectorUpdate() {
        if (autoRefresh) {
            Refresh();
        }
    }

    void OnGUI () {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.Space ();
        if (DrawOptions()) {
            EditorGUILayout.Space ();
            EditorGUI.indentLevel = 0;
            if ( ex2DRenderer.instance == null ) {
                EditorGUILayout.HelpBox("ex2DRenderer not found", MessageType.Error);
                EditorGUILayout.EndScrollView();
                return;
            }
            DrawStatistics();
            DrawBatches();
        }

        EditorGUILayout.EndScrollView();
    }

    bool DrawOptions() {
        EditorGUI.indentLevel = 1;

        bool isDebug = exDebug.enabled;
        bool newIsDebug = EditorGUILayout.Toggle("Debugging", isDebug);
        if (newIsDebug != isDebug) {
            exDebug.enabled = newIsDebug;
        }

#if !EX_DEBUG
        GUIStyle errorStyle = new GUIStyle();
        errorStyle.fontSize = EditorStyles.boldLabel.fontSize;
        errorStyle.fontStyle = FontStyle.Bold;
        errorStyle.normal.textColor = Color.red;

        if (EditorApplication.isPlaying) {
            EditorGUILayout.HelpBox("Unable to check batching when Debugging is disabled, \nyou must stop the game before setting it.", MessageType.Error);
        }
        else {
            EditorGUILayout.HelpBox("Unable to check batching when Debugging is disabled.", MessageType.Error);
        }
        return false;
#endif

        EditorGUILayout.BeginHorizontal();

        autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);
        if (!autoRefresh) {
            if (GUILayout.Button("Refresh")) {
                Refresh();
                Repaint();
            }
            GUILayout.FlexibleSpace();
        }

        EditorGUILayout.EndHorizontal();
        return true;
    }

    void DrawStatistics() {
        showStatistics.target = EditorGUILayout.Foldout(showStatistics.target, "Statistics:");
        if (EditorGUILayout.BeginFadeGroup(showStatistics.faded)) {
            ++EditorGUI.indentLevel;

            string info = "";
            info += string.Format("Materials: {0}  Meshes: {1}  Sprites: {2}  Layers: {3}", totalMaterialCount, totalBatchCount, totalSpriteCount, ex2DRenderer.instance.layerList.Count);
            info += "\n";
            info += string.Format("Active Meshes: {1}  Active Sprites: {0}", totalActiveSpriteCount, totalActiveMeshCount);
            EditorGUILayout.SelectableLabel(info);
        
            --EditorGUI.indentLevel;
        }
        EditorGUILayout.EndFadeGroup();
    }

    void DrawBatches() {
        showBatches.target = EditorGUILayout.Foldout(showBatches.target, "Batches:");
        if (EditorGUILayout.BeginFadeGroup(showBatches.faded)) {
            ++EditorGUI.indentLevel;

            for (int i = 0; i < layerInfoList.Count; i++) {
                var layerInfo = layerInfoList[i];
                var layer = layerInfo.layer;
                if (!layer) {
                    Refresh();
                    Repaint();
                    return;
                }
                var layerTitle = string.Format("Layer [{0}]   (Actual Materials: {1}, Meshes: {2})", layer.gameObject.name, layerInfo.materials, layerInfo.batchInfoList.Count);

                EditorGUILayout.BeginHorizontal();

                layerInfo.folded = EditorGUILayout.Foldout(layerInfo.folded, "");

                GUILayout.Space(-24);
                if (GUILayout.Button(layerTitle, EditorStyles.label)) {
                    Selection.activeGameObject = layer.gameObject;
                }

                if (layer.ordered) {
                    EditorGUILayout.LabelField("Ordered", EditorStyles.boldLabel);  // GUILayout.ExpandWidth(true)
                }
                else {
                    EditorGUILayout.LabelField("Unordered");
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();


                if (layerInfo.folded) {
                    ++EditorGUI.indentLevel;

                    for (int j = 0; j < layerInfo.batchInfoList.Count; j++) {
                        var batchInfo = layerInfo.batchInfoList[j];
                        var mesh = batchInfo.mesh;
                        if (!mesh) {
                            Refresh();
                            Repaint();
                            return;
                        }
                        var batchTitle = string.Format("Mesh [{0}]   (Sprites: {3}, Index: {1}, Z: {2})", batchInfo.name, batchInfo.index, batchInfo.pos_z, mesh.sortedSpriteList.Count);

                        EditorGUILayout.BeginHorizontal();
                        batchInfo.folded = EditorGUILayout.Foldout(batchInfo.folded, "");
                        GUILayout.Space(-10);
                        if (GUILayout.Button(batchTitle, EditorStyles.label)) {
                            Selection.activeObject = mesh;
                        }
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();

                        if (batchInfo.folded) {
                            ++EditorGUI.indentLevel;

                            for (int k = 0; k < mesh.sortedSpriteList.Count; k++) {
                                var sprite = mesh.sortedSpriteList[k];
                                if (!sprite) {
                                    Refresh();
                                    Repaint();
                                    return;
                                }
                                var spriteTitle = string.Format("Sprite [{0}]   (Depth: {1}, GlobalDepth: {2})", sprite.gameObject.name, sprite.depth, (sprite as exLayer.IFriendOfLayer).globalDepth);

                                EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(65);
                                if (GUILayout.Button(spriteTitle, EditorStyles.label)) {
                                    Selection.activeObject = sprite;
                                }
                                EditorGUILayout.EndHorizontal();
                            }

                            --EditorGUI.indentLevel;
                        }
                    }

                    --EditorGUI.indentLevel;
                }
            }
            
            --EditorGUI.indentLevel;
        }
        EditorGUILayout.EndFadeGroup();
    }

    string GetBatchName(exMesh mesh) {
        string matName;
        Material mat = mesh.material;
        if (mat != null) {
            if (mat.mainTexture) {
                matName = mat.mainTexture.name;
            }
            else {
                matName = mat.name;
            }
        }
        else {
            matName = "None";
        }
        return matName;
    }

    void Refresh() {
        if (!autoRefresh) {
            //Debug.Log("CheckBatching Refreshed");
        }
#if EX_DEBUG
        totalMaterialCount = ex2DRenderer.GetMaterialCount();
#endif
        totalBatchCount = 0;
        totalSpriteCount = 0;
        totalActiveMeshCount = 0;
        totalActiveSpriteCount = 0;

        var layerIndex = 0;

        var layers = ex2DRenderer.instance.layerList;
        foreach (var layer in layers) {
            if (!layer) {
                continue;
            }

            LayerInfo layerInfo;
            if (layerIndex < layerInfoList.Count) {
                // keep folded unchanged
                layerInfo = layerInfoList[layerIndex];
            }
            else {
                layerInfo = new LayerInfo();
                layerInfoList.Add(layerInfo);
            }
            layerIndex++;

            layerInfo.layer = layer;

            HashSet<Material> materialCounter = new HashSet<Material>();
            var batchIndex = 0;
            foreach (var mesh in layer.meshList) {
                if (!mesh) {
                    continue;
                }
                ++totalBatchCount;
                if (mesh.material) {
                    materialCounter.Add(mesh.material);
                }

                BatchInfo batchInfo;
                if (batchIndex < layerInfo.batchInfoList.Count) {
                    // keep folded unchanged
                    batchInfo = layerInfo.batchInfoList[batchIndex];
                }
                else {
                    batchInfo = new BatchInfo();
                    layerInfo.batchInfoList.Add(batchInfo);
                }
                batchIndex++;

                batchInfo.mesh = mesh;
                batchInfo.index = totalBatchCount;
                batchInfo.name = GetBatchName(mesh);
                batchInfo.pos_z = mesh.transform.position.z;

                if (mesh.gameObject.activeSelf) {
                    ++totalActiveMeshCount;
                }
                totalSpriteCount += mesh.sortedSpriteList.Count;

                foreach (var sprite in mesh.sortedSpriteList) {
                    if (sprite.visible) {
                        ++totalActiveSpriteCount;
                    }
	            }
	        }
            layerInfo.batchInfoList.RemoveRange(batchIndex, layerInfo.batchInfoList.Count - batchIndex);
            layerInfo.materials = materialCounter.Count;
        }
        layerInfoList.RemoveRange(layerIndex, layerInfoList.Count - layerIndex);
    }
}
