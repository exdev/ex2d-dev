// ======================================================================================
// File         : exUIButtonInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/16/2013 | 13:39:33 PM | Wednesday,October
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
[CustomEditor(typeof(exUIButton))]
class exUIButtonInspector : exUIControlInspector {

    protected SerializedProperty allowDragProp;
    protected SerializedProperty dragThresholdProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        allowDragProp = serializedObject.FindProperty("allowDrag");
        dragThresholdProp = serializedObject.FindProperty("dragThreshold");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        EditorGUILayout.PropertyField ( allowDragProp );
        GUI.enabled = !allowDragProp.boolValue;
        EditorGUILayout.PropertyField ( dragThresholdProp );
        GUI.enabled = true;

        EditorGUILayout.Space();
    }
}
