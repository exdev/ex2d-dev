// ======================================================================================
// File         : exUIScrollBarInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/21/2013 | 11:19:40 AM | Monday,October
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
[CustomEditor(typeof(exUIScrollBar))]
class exUIScrollBarInspector : exUIControlInspector {

    protected SerializedProperty directionProp;
    protected SerializedProperty scrollViewProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        directionProp = serializedObject.FindProperty("direction");
        scrollViewProp = serializedObject.FindProperty("scrollView");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        EditorGUILayout.PropertyField ( directionProp );
        EditorGUILayout.PropertyField ( scrollViewProp );

        EditorGUILayout.Space();
    }
}
