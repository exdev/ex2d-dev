// ======================================================================================
// File         : exUIToggleGroupInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/24/2013 | 15:29:47 PM | Thursday,October
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
using System.Reflection;

///////////////////////////////////////////////////////////////////////////////
// BoardPatternInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(exUIToggleGroup))]
class exUIToggleGroupInspector : exUIControlInspector {

    protected SerializedProperty indexProp;
    protected SerializedProperty togglesProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        indexProp = serializedObject.FindProperty("index_");
        togglesProp = serializedObject.FindProperty("toggles");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        EditorGUILayout.PropertyField ( indexProp, new GUIContent("index") );
        EditorGUILayout.PropertyField ( togglesProp, new GUIContent("toggles"), true );

        EditorGUILayout.Space();
    }
}
