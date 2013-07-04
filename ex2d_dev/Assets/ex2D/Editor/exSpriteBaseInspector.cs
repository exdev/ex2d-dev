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
        bool oldCustomSize = customSizeProp.boolValue;
        EditorGUILayout.PropertyField ( customSizeProp, new GUIContent("Custom Size") );
        if ( customSizeProp.boolValue != oldCustomSize ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.customSize = customSizeProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        if ( customSizeProp.boolValue ) {
            // width
            float oldWidth = widthProp.floatValue;
            EditorGUILayout.PropertyField ( widthProp, new GUIContent("Width") );
            if ( widthProp.floatValue != oldWidth ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSpriteBase sp = obj as exSpriteBase;
                    if ( sp ) {
                        sp.width = widthProp.floatValue;
                        EditorUtility.SetDirty(sp);
                    }
                }
            }

            // height
            float oldHeight = heightProp.floatValue;
            EditorGUILayout.PropertyField ( heightProp, new GUIContent("Height") );
            if ( heightProp.floatValue != oldHeight ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSpriteBase sp = obj as exSpriteBase;
                    if ( sp ) {
                        sp.height = heightProp.floatValue;
                        EditorUtility.SetDirty(sp);
                    }
                }
            }
        }
        else {
            GUI.enabled = false;
            if ( serializedObject.isEditingMultipleObjects == false ) {
                exSpriteBase spriteBase = serializedObject.targetObject as exSpriteBase;
                EditorGUILayout.FloatField ( new GUIContent("Width"), spriteBase.width );
                EditorGUILayout.FloatField ( new GUIContent("Height"), spriteBase.height );
            }
            GUI.enabled = true;
        }

        //
        int oldAnchor = anchorProp.enumValueIndex;
        EditorGUILayout.PropertyField ( anchorProp, new GUIContent("Anchor") );
        if ( anchorProp.enumValueIndex != oldAnchor ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.anchor = (Anchor)anchorProp.enumValueIndex;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        //
        Vector2 oldOffset = offsetProp.vector2Value;
        EditorGUILayout.PropertyField ( offsetProp, new GUIContent("Offset"), true ); // TODO: don't expand, just display in one line
        if ( offsetProp.vector2Value != oldOffset ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.offset = offsetProp.vector2Value;
                    EditorUtility.SetDirty(sp);
                }
            }
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
    }
}

