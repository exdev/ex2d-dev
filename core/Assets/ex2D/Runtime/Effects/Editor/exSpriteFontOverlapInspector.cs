// ======================================================================================
// File         : exSpriteFontOverlapInspector.cs
// Author       : Wu Jie 
// Last Change  : 11/14/2013 | 15:09:41 PM | Thursday,November
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
[CustomEditor(typeof(exSpriteFontOverlap))]
class exSpriteFontOverlapInspector : Editor {

    protected SerializedProperty overlapProp;
    protected SerializedProperty originalProp;
    protected SerializedProperty textProp;
    protected SerializedProperty depthProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        InitProperties ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        serializedObject.Update ();

        // NOTE DANGER: Unity didn't allow user change script in custom inspector { 
        SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField ( scriptProp );
        // } DANGER end 

        DoInspectorGUI ();
        serializedObject.ApplyModifiedProperties ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected virtual void InitProperties () {
        overlapProp = serializedObject.FindProperty("overlap");
        originalProp = serializedObject.FindProperty("original");
        textProp = serializedObject.FindProperty("text_");
        depthProp = serializedObject.FindProperty("depth_");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected virtual void DoInspectorGUI () {
        if ( serializedObject.isEditingMultipleObjects == false ) {

            exSpriteFontOverlap editTarget = serializedObject.targetObject as exSpriteFontOverlap; 

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( overlapProp );
            if ( EditorGUI.EndChangeCheck() ) {
                editTarget.overlap = overlapProp.objectReferenceValue as exSpriteFont;
                editTarget.overlap.text = editTarget.text;
                editTarget.overlap.depth = editTarget.depth + 0.1f;
                EditorUtility.SetDirty(editTarget);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( originalProp );
            if ( EditorGUI.EndChangeCheck() ) {
                editTarget.original = originalProp.objectReferenceValue as exSpriteFont;
                editTarget.original.text = editTarget.text;
                editTarget.original.depth = editTarget.depth + 0.1f;
                EditorUtility.SetDirty(editTarget);
            }
        }

        //
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( textProp, new GUIContent("Text") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFontOverlap editTarget = obj as exSpriteFontOverlap;
                editTarget.text = textProp.stringValue;
                EditorUtility.SetDirty(editTarget);
            }
        }

        //
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( depthProp, new GUIContent("Depth") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFontOverlap editTarget = obj as exSpriteFontOverlap;
                editTarget.depth = depthProp.floatValue;
                EditorUtility.SetDirty(editTarget);
            }
        }
    }
}

