// ======================================================================================
// File         : ex2DRendererInspector.cs
// Author       : Wu Jie 
// Last Change  : 07/08/2013 | 10:51:18 AM | Monday,July
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
// BoardPatternInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(ex2DRenderer))]
class ex2DRendererInspector : Editor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	override public void OnInspectorGUI () {
        DrawDefaultInspector(); 

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Reset Camera", GUILayout.Height(20) ) ) {
                ((ex2DRenderer)target).ResetCamera(true);
            }
            if ( GUILayout.Button("Edit...", GUILayout.MinWidth(50), GUILayout.Height(20) ) ) {
                EditorWindow.GetWindow<exSceneEditor>();
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}

