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
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( textureInfoProp, new GUIContent("Texture Info") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.textureInfo = textureInfoProp.objectReferenceValue as exTextureInfo;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // useTextureOffset
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( useTextureOffsetProp, new GUIContent("Use Texture Offset") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.useTextureOffset = useTextureOffsetProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // shader
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( shaderProp, new GUIContent("Shader") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.shader = shaderProp.objectReferenceValue as Shader;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Edit...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                EditorWindow.GetWindow<exSceneEditor>();
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
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

