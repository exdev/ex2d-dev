// ======================================================================================
// File         : exSpriteColorControllerInspector.cs
// Author       : Wu Jie 
// Last Change  : 11/12/2013 | 10:11:02 AM | Tuesday,November
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
[CustomEditor(typeof(exSpriteColorController))]
class exSpriteColorControllerInspector : Editor {

    protected SerializedProperty colorProp;
    protected SerializedProperty colorInfosProp;

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
        colorProp = serializedObject.FindProperty("color_");
        colorInfosProp = serializedObject.FindProperty("colorInfos");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected virtual void DoInspectorGUI () {
        // color
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( colorProp, new GUIContent("Color") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteColorController ctrl = obj as exSpriteColorController;
                if ( ctrl ) {
                    ctrl.color = colorProp.colorValue;
                    EditorUtility.SetDirty(ctrl);
                }
            }
        }

        // color Infos
        EditorGUILayout.PropertyField ( colorInfosProp, true );

        //
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Register Colors...", GUILayout.MinWidth(50), GUILayout.Height(20) ) ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSpriteColorController ctrl = obj as exSpriteColorController;
                    if ( ctrl ) {
                        // first we need to recover the color
                        for ( int i = 0; i < ctrl.colorInfos.Count; ++i ) {
                            exSpriteColorController.ColorInfo colorInfo = ctrl.colorInfos[i];
                            if ( colorInfo.sprite != null ) {
                                colorInfo.sprite.color = colorInfo.color * Color.white;
                            }
                        }

                        // register new value
                        ctrl.RegisterAllChildren();

                        // reset the color
                        for ( int i = 0; i < ctrl.colorInfos.Count; ++i ) {
                            exSpriteColorController.ColorInfo colorInfo = ctrl.colorInfos[i];
                            if ( colorInfo.sprite != null ) {
                                colorInfo.sprite.color = colorInfo.color * ctrl.color;
                            }
                        }

                        EditorUtility.SetDirty(ctrl);
                    }
                }
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}

