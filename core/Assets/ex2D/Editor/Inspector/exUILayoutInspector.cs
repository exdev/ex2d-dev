// ======================================================================================
// File         : exUILayoutInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/03/2013 | 10:10:37 AM | Thursday,October
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
[CustomEditor(typeof(exUILayout))]
class exUILayoutInspector : Editor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	override public void OnInspectorGUI () {
        DrawDefaultInspector(); 

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Sync...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                exUILayout layout = target as exUILayout;
                layout.Sync();
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}

