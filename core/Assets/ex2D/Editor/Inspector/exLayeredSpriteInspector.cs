// ======================================================================================
// File         : exSpriteBaseInspector.cs
// Author       : Wu Jie 
// Last Change  : 07/04/2013 | 15:34:38 PM | Thursday,July
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
[CustomEditor(typeof(exLayeredSpriteInspector))]
class exLayeredSpriteInspector : exSpriteBaseInspector {

    protected SerializedProperty depthProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();
        depthProp = serializedObject.FindProperty("depth_");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        // depth
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( depthProp, new GUIContent("Depth") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exLayeredSprite sp = obj as exLayeredSprite;
                if ( sp ) {
                    sp.depth = depthProp.floatValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        EditorGUILayout.Space();
    }
}

