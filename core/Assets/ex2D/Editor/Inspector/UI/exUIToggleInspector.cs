// ======================================================================================
// File         : exUIToggleInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/24/2013 | 14:10:35 PM | Thursday,October
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
[CustomEditor(typeof(exUIToggle))]
class exUIToggleInspector : exUIControlInspector {

    protected SerializedProperty isCheckedProp;
    protected SerializedProperty isRadioProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        isCheckedProp = serializedObject.FindProperty("isChecked_");
        isRadioProp = serializedObject.FindProperty("isRadio");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        EditorGUILayout.PropertyField ( isCheckedProp, new GUIContent("Is Checked") );
        EditorGUILayout.PropertyField ( isRadioProp );

        EditorGUILayout.Space();
    }
}
