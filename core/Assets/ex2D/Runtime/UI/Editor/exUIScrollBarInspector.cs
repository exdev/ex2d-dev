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
    protected SerializedProperty cooldownProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        directionProp = serializedObject.FindProperty("direction");
        scrollViewProp = serializedObject.FindProperty("scrollView");
        cooldownProp = serializedObject.FindProperty("cooldown");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        EditorGUILayout.PropertyField ( directionProp );
        EditorGUILayout.PropertyField ( scrollViewProp );
        EditorGUILayout.PropertyField ( cooldownProp );

        //
        if ( serializedObject.isEditingMultipleObjects == false ) {
            exUIScrollBar scrollBar = target as exUIScrollBar;
            Transform transBar = scrollBar.transform.Find("__bar");
            if ( transBar ) {
                GUIStyle style = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label);
                style.normal.textColor = Color.green;
                EditorGUILayout.LabelField( "__bar", "founded!", style );
            }
            else {
                GUIStyle style = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label);

                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField( "__bar", "not found!", style );

                style.normal.textColor = Color.yellow;
                EditorGUILayout.LabelField( "Please add a child named \"__bar\".", style );
            }
        }

        EditorGUILayout.Space();
    }
}
