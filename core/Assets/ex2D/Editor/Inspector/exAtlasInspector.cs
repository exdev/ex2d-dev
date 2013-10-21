// ======================================================================================
// File         : exAtlasInspector.cs
// Author       : Wu Jie 
// Last Change  : 06/18/2013 | 00:17:37 AM | Tuesday,June
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
[CustomEditor(typeof(exAtlas))]
class exAtlasInspector : Editor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        DrawDefaultInspector(); 

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Edit...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                exAtlasEditor editor = EditorWindow.GetWindow<exAtlasEditor>();
                editor.Edit(target as exAtlas);
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}

