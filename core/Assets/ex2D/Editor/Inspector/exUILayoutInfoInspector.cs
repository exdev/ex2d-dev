// ======================================================================================
// File         : exUILayoutInfoInspector.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 16:49:05 PM | Friday,August
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
[CustomEditor(typeof(exUILayoutInfo))]
class exUILayoutInfoInspector : Editor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	override public void OnInspectorGUI () {
        DrawDefaultInspector(); 

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Edit...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                exUILayoutEditor editor = EditorWindow.GetWindow<exUILayoutEditor>();
                editor.Edit(target as exUILayoutInfo);
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}

