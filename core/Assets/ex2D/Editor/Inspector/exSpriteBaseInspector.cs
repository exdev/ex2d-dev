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
[CustomEditor(typeof(exSpriteBase))]
class exSpriteBaseInspector : Editor {

    SerializedProperty customSizeProp;
    SerializedProperty widthProp;
    SerializedProperty heightProp;
    SerializedProperty anchorProp;
    SerializedProperty offsetProp;
    SerializedProperty shearProp;
    SerializedProperty colorProp;
    SerializedProperty shaderProp;

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
        // NOTE: DO NOT call serializedObject.ApplyModifiedProperties ();
        serializedObject.Update ();

        EditorGUILayout.Space();
        EditorGUIUtility.LookLikeInspector();

        // customSize
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( customSizeProp, new GUIContent("Custom Size") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.customSize = customSizeProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // if customSize == true
        EditorGUI.indentLevel++;
        if ( customSizeProp.boolValue ) {
            // width
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( widthProp, new GUIContent("Width") );
            if ( EditorGUI.EndChangeCheck() ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSpriteBase sp = obj as exSpriteBase;
                    if ( sp ) {
                        sp.width = widthProp.floatValue;
                        EditorUtility.SetDirty(sp);
                    }
                }
            }

            // height
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( heightProp, new GUIContent("Height") );
            if ( EditorGUI.EndChangeCheck() ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSpriteBase sp = obj as exSpriteBase;
                    if ( sp ) {
                        sp.height = heightProp.floatValue;
                        EditorUtility.SetDirty(sp);
                    }
                }
            }
        }
        // if customSize == false
        else {
            GUI.enabled = false;
            if ( serializedObject.isEditingMultipleObjects == false ) {
                exSpriteBase spriteBase = serializedObject.targetObject as exSpriteBase;
                EditorGUILayout.FloatField ( new GUIContent("Width"), spriteBase.width );
                EditorGUILayout.FloatField ( new GUIContent("Height"), spriteBase.height );
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel--;

        // anchor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( anchorProp, new GUIContent("Anchor") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.anchor = (Anchor)anchorProp.enumValueIndex;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // offset
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( offsetProp, new GUIContent("Offset"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.offset = offsetProp.vector2Value;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // shear
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( shearProp, new GUIContent("Shear"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.shear = shearProp.vector2Value;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // color
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( colorProp, new GUIContent("Color"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.color = colorProp.colorValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }
        
        // shader
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( shaderProp, new GUIContent("Shader") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.shader = shaderProp.objectReferenceValue as Shader;
                    EditorUtility.SetDirty(sp);
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected virtual void OnSceneGUI () {
        exSpriteBase sprite = target as exSpriteBase;
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

    protected void InitProperties () {
        customSizeProp = serializedObject.FindProperty("customSize_");
        widthProp = serializedObject.FindProperty("width_");
        heightProp = serializedObject.FindProperty("height_");
        anchorProp = serializedObject.FindProperty("anchor_");
        offsetProp = serializedObject.FindProperty("offset_");
        shearProp = serializedObject.FindProperty("shear_");
        colorProp = serializedObject.FindProperty("color_");
        shaderProp = serializedObject.FindProperty("shader_");
    }
}

