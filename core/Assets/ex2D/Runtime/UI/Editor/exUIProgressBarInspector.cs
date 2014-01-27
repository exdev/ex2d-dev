// ======================================================================================
// File         : exUIProgressBarInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/26/2013 | 16:48:19 PM | Saturday,October
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
// exUIProgressBarInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(exUIProgressBar))]
class exUIProgressBarInspector : exUIControlInspector {

    protected SerializedProperty progressProp;
    protected SerializedProperty barSizeProp;
    protected SerializedProperty directionProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        progressProp = serializedObject.FindProperty("progress_");
        barSizeProp = serializedObject.FindProperty("barSize_");
        directionProp = serializedObject.FindProperty("direction");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        bool barChanged = false;

        //
        EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( progressProp, new GUIContent("Progress") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exUIProgressBar progressBar = obj as exUIProgressBar;
                if ( progressBar == null ) {
                    continue;
                }
                progressProp.floatValue = Mathf.Clamp( progressProp.floatValue, 0.0f, 1.0f );
            }
            barChanged = true;
        }

        //
        EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( barSizeProp, new GUIContent("Bar Size") );
        if ( EditorGUI.EndChangeCheck() ) {
            barChanged = true;
        }

        //
        EditorGUILayout.PropertyField ( directionProp );

        //
        if ( serializedObject.isEditingMultipleObjects == false ) {
            exUIProgressBar progressBar = target as exUIProgressBar;
            Transform transBar = progressBar.transform.Find("__bar");
            if ( transBar ) {
                GUIStyle style = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).label);
                style.normal.textColor = Color.green;
                EditorGUILayout.LabelField( "__bar", "founded!", style );

                exSprite bar = transBar.GetComponent<exSprite>();
                if ( bar ) {
                    if ( bar.customSize == false ) {
                        bar.customSize = true;
                        EditorUtility.SetDirty(bar);
                    }
                    if ( bar.anchor != Anchor.TopLeft ) {
                        bar.anchor = Anchor.TopLeft;
                        EditorUtility.SetDirty(bar);
                    }
                    if ( barChanged ) {
                        exUIProgressBar.SetBarSize ( bar, barSizeProp.floatValue, progressProp.floatValue, progressBar.direction );
                        EditorUtility.SetDirty(bar);
                    }
                }
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
