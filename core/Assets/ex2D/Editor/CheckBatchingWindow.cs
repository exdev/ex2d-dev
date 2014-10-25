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

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

class CheckBatchingWindow : EditorWindow {

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

    int materials = -1;
    int meshes = -1;
    int sprites = -1;
    int activeSprites = -1;
    int activeMeshes = -1;

    // ------------------------------------------------------------------ 
    // Builtin Events 
    // ------------------------------------------------------------------ 

    void OnEnable() {
        Refresh();
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
        EditorGUILayout.Space ();
        EditorGUI.indentLevel = 1;

        bool isDebug = exDebug.enabled;
        bool newIsDebug = EditorGUILayout.Toggle("Debugging", isDebug);
        if (newIsDebug != isDebug) {
            exDebug.enabled = newIsDebug;
        }
        EditorGUILayout.Space ();

        EditorGUILayout.BeginHorizontal();

        autoRefresh = EditorGUILayout.Toggle("Auto Refresh", autoRefresh);

        if (!autoRefresh) {
            if (GUILayout.Button("Refresh")) {
                Refresh();
            }
        }

        EditorGUILayout.EndHorizontal();

#if !EX_DEBUG
        GUIStyle errorStyle = new GUIStyle();
        errorStyle.fontSize = EditorStyles.boldLabel.fontSize;
        errorStyle.fontStyle = FontStyle.Bold;
        errorStyle.normal.textColor = Color.red;

        if (EditorApplication.isPlaying) {
            EditorGUILayout.LabelField( "Unable to check batching when Debugging is disabled, \nyou must stop the game before setting it.", errorStyle );
        }
        else {
            EditorGUILayout.LabelField( "Unable to check batching when Debugging is disabled.", errorStyle );
        }
        return;
#endif

        if ( ex2DRenderer.instance == null ) {
            GUILayout.Label ( "ex2DRenderer not found" );
            return;
        }


        EditorGUI.indentLevel = 0;
        EditorGUILayout.LabelField("Statistics:", EditorStyles.boldLabel);

        
        EditorGUI.indentLevel = 1;
        string info = "";
        info += string.Format("Materials: {0}  Meshes: {1}  Sprites: {2}  Layers: {3}", materials, meshes, sprites, ex2DRenderer.instance.layerList.Count);
        info += "\n";
        info += string.Format("Active Meshes: {1}  Active Sprites: {0}", activeSprites, activeMeshes);
        EditorGUILayout.SelectableLabel(info);

        //GUILayout.Label("Build:");
        //GUILayout.Space (10);

        //GUILayout.BeginHorizontal();
        //EditorGUILayout.TextArea(text);
        //GUILayout.EndHorizontal();
    }

    void Refresh() {
        if (!autoRefresh) {
            //Debug.Log("CheckBatching Refreshed");
        }
        materials = ex2DRenderer.GetMaterialCount();

        meshes = 0;
        sprites = 0;
        activeMeshes = 0;
        activeSprites = 0;
        var layers = ex2DRenderer.instance.layerList;
        foreach (var layer in layers) {
            meshes += layer.meshList.Count;
            foreach (var mesh in layer.meshList) {
                if (mesh.gameObject.activeSelf) {
                    ++activeMeshes;
                }
                sprites += mesh.sortedSpriteList.Count;

                foreach (var sprite in mesh.sortedSpriteList) {
                    if (sprite.visible) {
                        ++activeSprites;
                    }
	            }
	        }
        }
    }
}
