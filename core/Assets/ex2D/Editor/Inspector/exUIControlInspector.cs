// ======================================================================================
// File         : exUIControlInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/08/2013 | 11:41:29 AM | Tuesday,October
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
[CustomEditor(typeof(exUIControl))]
class exUIControlInspector : exPlaneInspector {

    SerializedProperty activeProp;
    SerializedProperty grabMouseOrTouchProp;
    SerializedProperty useColliderProp;

    SerializedProperty onHoverInSlotsProp;
    SerializedProperty onHoverOutSlotsProp;

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

        // active
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( activeProp, new GUIContent("Active") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exUIControl ctrl = obj as exUIControl;
                if ( ctrl ) {
                    ctrl.activeSelf = activeProp.boolValue;
                    EditorUtility.SetDirty(ctrl);
                }
            }
        }

        // grabMouseOrTouch
        EditorGUILayout.PropertyField ( grabMouseOrTouchProp, new GUIContent("Grab Mouse Or Touch") );

        // use collider
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( useColliderProp, new GUIContent("Use Collider") );
        if ( EditorGUI.EndChangeCheck() ) {
            if ( useColliderProp.boolValue ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exUIControl ctrl = obj as exUIControl;
                    if ( ctrl ) {
                        Collider collider = ctrl.GetComponent<Collider>();
                        if ( collider == null ) {
                            collider = ctrl.gameObject.AddComponent<BoxCollider>();
                        }

                        BoxCollider boxCollider = collider as BoxCollider;
                        if ( boxCollider != null ) {
                            Rect localRect = ctrl.GetLocalAABoundingRect();
                            boxCollider.center = new Vector3( localRect.center.x, localRect.center.y, boxCollider.center.z); 
                            boxCollider.size = new Vector3 ( localRect.width, localRect.height, boxCollider.size.z ); 
                        }
                    }
                }
            }
            else {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exUIControl ctrl = obj as exUIControl;
                    if ( ctrl ) {
                        Collider[] colliders = ctrl.GetComponents<Collider>();
                        for ( int i = 0; i < colliders.Length; ++i ) {
                            Object.DestroyImmediate(colliders[i]);
                        }
                    }
                }
            }
        }

        if ( useColliderProp.boolValue ) {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
                if ( GUILayout.Button("Sync Collider", GUILayout.MinWidth(50), GUILayout.Height(20) ) ) {
                    foreach ( Object obj in serializedObject.targetObjects ) {
                        exUIControl ctrl = obj as exUIControl;
                        if ( ctrl ) {
                            BoxCollider boxCollider = ctrl.GetComponent<BoxCollider>();
                            Rect localRect = ctrl.GetLocalAABoundingRect();
                            boxCollider.center = new Vector3( localRect.center.x, localRect.center.y, boxCollider.center.z); 
                            boxCollider.size = new Vector3 ( localRect.width, localRect.height, boxCollider.size.z ); 
                        }
                    }
                }
            EditorGUILayout.EndHorizontal();
        }

        // slots
        EditorGUILayout.PropertyField (onHoverInSlotsProp, true );
        EditorGUILayout.PropertyField (onHoverOutSlotsProp, true );

        serializedObject.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void OnSceneGUI () {
        base.OnSceneGUI();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void InitProperties () {
        base.InitProperties();

        activeProp = serializedObject.FindProperty("active_");
        grabMouseOrTouchProp = serializedObject.FindProperty("grabMouseOrTouch");
        useColliderProp = serializedObject.FindProperty("useCollider");

        onHoverInSlotsProp = serializedObject.FindProperty("onHoverInSlots");
        onHoverOutSlotsProp = serializedObject.FindProperty("onHoverOutSlots");
    }
}

