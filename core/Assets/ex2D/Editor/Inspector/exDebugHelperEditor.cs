// ======================================================================================
// File         : exDebugHelperEditor.cs
// Author       : Wu Jie 
// Last Change  : 11/25/2011 | 23:49:23 PM | Friday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

[CustomEditor(typeof(exDebugHelper))]
public class exDebugHelperEditor : Editor {

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exDebugHelper curEdit;

    SerializedProperty offsetProp;
    SerializedProperty printStyleProp;
    SerializedProperty fpsStyleProp;
    SerializedProperty fpsAnchorProp;
    SerializedProperty logStyleProp;
    SerializedProperty timeScaleStyleProp;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        if ( target != curEdit ) {
            curEdit = target as exDebugHelper;
        }
        offsetProp = serializedObject.FindProperty ("offset");
        printStyleProp = serializedObject.FindProperty ("printStyle");
        fpsStyleProp = serializedObject.FindProperty ("fpsStyle");
        fpsAnchorProp = serializedObject.FindProperty ("fpsAnchor");
        logStyleProp = serializedObject.FindProperty ("logStyle");
        timeScaleStyleProp = serializedObject.FindProperty ("timeScaleStyle");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    override public void OnInspectorGUI () {

        // settings 
        serializedObject.Update();
            // Show Fps
            curEdit.showFps = EditorGUILayout.Toggle( "Show Fps", curEdit.showFps );
            EditorGUILayout.PropertyField (offsetProp, true);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField (fpsStyleProp, true);
            EditorGUILayout.PropertyField (fpsAnchorProp, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Show Screen Print
            curEdit.showScreenPrint = EditorGUILayout.Toggle( "Show Screen Print", curEdit.showScreenPrint );
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField (printStyleProp, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Show Screen Log
            curEdit.showScreenLog = EditorGUILayout.Toggle( "Show Screen Log", curEdit.showScreenLog );
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField (logStyleProp, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Enable Time Scale Debug
            curEdit.enableTimeScaleDebug = EditorGUILayout.Toggle( "Enable Time Scale Debug", curEdit.enableTimeScaleDebug );
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField (timeScaleStyleProp, true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();

            // Show Screen Debug Text
            curEdit.showScreenDebugText = EditorGUILayout.Toggle( "Show Screen Debug Text", curEdit.showScreenDebugText );
        serializedObject.ApplyModifiedProperties();

        // check dirty 
        if ( GUI.changed ) {
            EditorUtility.SetDirty(curEdit);
        }
    }
}

