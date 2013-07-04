// ======================================================================================
// File         : exSpriteInspector.cs
// Author       : Wu Jie 
// Last Change  : 07/04/2013 | 15:43:11 PM | Thursday,July
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
[CustomEditor(typeof(exSprite))]
class exSpriteInspector : exSpriteBaseInspector {

    SerializedProperty textureInfoProp;
    SerializedProperty useTextureOffsetProp;
    SerializedProperty shaderProp;

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

        // NOTE: DO NOT call serializedObject.ApplyModifiedProperties ();
        serializedObject.Update ();

        EditorGUILayout.Space();

        // textureInfo
        Object oldTextureInfo = textureInfoProp.objectReferenceValue;
        EditorGUILayout.PropertyField ( textureInfoProp, new GUIContent("Texture Info") );
        if ( textureInfoProp.objectReferenceValue != oldTextureInfo ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.textureInfo = textureInfoProp.objectReferenceValue as exTextureInfo;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // useTextureOffset
        bool oldUseTextureOffset = useTextureOffsetProp.boolValue;
        EditorGUILayout.PropertyField ( useTextureOffsetProp, new GUIContent("Use Texture Offset") );
        if ( useTextureOffsetProp.boolValue != oldUseTextureOffset ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.useTextureOffset = useTextureOffsetProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // useTextureOffset
        Object oldShader = shaderProp.objectReferenceValue;
        EditorGUILayout.PropertyField ( shaderProp, new GUIContent("Shader") );
        if ( shaderProp.objectReferenceValue != oldShader ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
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

    protected new void InitProperties () {
        base.InitProperties();
        textureInfoProp = serializedObject.FindProperty("textureInfo_");
        useTextureOffsetProp = serializedObject.FindProperty("useTextureOffset_");
        shaderProp = serializedObject.FindProperty("shader_");
    }
}

