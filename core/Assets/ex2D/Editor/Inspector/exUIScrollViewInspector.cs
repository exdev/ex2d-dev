// ======================================================================================
// File         : exUIScrollViewInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/16/2013 | 16:29:15 PM | Wednesday,October
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
[CustomEditor(typeof(exUIScrollView))]
class exUIScrollViewInspector : exUIControlInspector {

    SerializedProperty contentSizeProp;
    SerializedProperty acceptMouseDragProp;
    SerializedProperty contentAnchorProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        InitProperties();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        base.OnInspectorGUI();

        serializedObject.Update();

            EditorGUILayout.PropertyField ( contentSizeProp, new GUIContent("Content Size") );
            EditorGUILayout.PropertyField ( acceptMouseDragProp );
            EditorGUILayout.PropertyField ( contentAnchorProp );

        serializedObject.ApplyModifiedProperties();

        if ( serializedObject.isEditingMultipleObjects == false ) {
            exUIScrollView scrollView = target as exUIScrollView;
            if ( scrollView != null &&
                 scrollView.contentAnchor == null &&
                 scrollView.transform.childCount > 0 ) 
            {
                scrollView.contentAnchor = scrollView.transform.GetChild(0);
                EditorUtility.SetDirty (target);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void InitProperties () {
        base.InitProperties();

        contentSizeProp = serializedObject.FindProperty("contentSize_");
        acceptMouseDragProp = serializedObject.FindProperty("acceptMouseDrag");
        contentAnchorProp = serializedObject.FindProperty("contentAnchor");
    }
}
