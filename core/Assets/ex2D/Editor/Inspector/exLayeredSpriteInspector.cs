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

    SerializedProperty depthProp;

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
        base.OnInspectorGUI();

        // NOTE: DO NOT call serializedObject.ApplyModifiedProperties ();
        serializedObject.Update ();

        EditorGUILayout.Space();

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
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected virtual void OnSceneGUI () {
        exLayeredSprite sprite = target as exLayeredSprite;
        Vector3[] vertices = sprite.GetWorldVertices();
        if (vertices.Length > 0) {
            Vector3[] vertices2 = new Vector3[vertices.Length+1];
            for ( int i = 0; i < vertices.Length; ++i )
                vertices2[i] = vertices[i];
            vertices2[vertices.Length] = vertices[0];

            Handles.DrawPolyLine( vertices2 );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void InitProperties () {
        base.InitProperties();
        depthProp = serializedObject.FindProperty("depth_");
    }
}

