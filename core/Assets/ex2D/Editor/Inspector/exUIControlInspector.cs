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

    public class Styles {
        public GUIStyle toolbarDropDown = "TE ToolbarDropDown";
        public Texture iconToolbarPlus = EditorGUIUtility.FindTexture ("Toolbar Plus");
        public Texture iconToolbarMinus = EditorGUIUtility.FindTexture("Toolbar Minus");
    }
    protected static Styles styles = null;

    SerializedProperty activeProp;
    SerializedProperty grabMouseOrTouchProp;
    SerializedProperty useColliderProp;

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

        // if settingsStyles is null
        if ( styles == null ) {
            styles = new Styles();
        }

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

        if ( serializedObject.isEditingMultipleObjects == false ) {
            exUIControl uiControl = target as exUIControl;

            // event adding selector
            List<string> eventNameList = new List<string>(); 
            eventNameList.Add( "Event List" );
            eventNameList.AddRange( uiControl.GetEventNames() );

            foreach ( exUIControl.EventTrigger eventTrigger in uiControl.events ) {
                int idx = eventNameList.IndexOf(eventTrigger.name);
                if ( idx != -1 ) {
                    eventNameList.RemoveAt(idx);
                }
            }

            int choice = EditorGUILayout.Popup ( "Add Event", 0, eventNameList.ToArray() );
            if ( choice != 0 ) {
                // exUIControl.EventTrigger eventTrigger = uiControl.GetEvent( eventNameList[choice] )
            }

            EditorGUILayout.Space();

            // event triggers
            for ( int i = 0; i < uiControl.events.Count; ++i ) {
                exUIControl.EventTrigger eventTrigger = uiControl.events[i];
                EventField ( eventTrigger );
                EditorGUILayout.Space();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void EventField ( exUIControl.EventTrigger _eventTrigger ) {
		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);

            GUILayout.BeginVertical();
                // name
                GUILayout.Toggle( true, _eventTrigger.name, "dragtab");

                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
                GUILayout.BeginVertical();

                    // slots
                    for ( int i = 0; i < _eventTrigger.slots.Count; ++i ) {
                        exUIControl.SlotInfo slotInfo = _eventTrigger.slots[i];
                        SlotField (slotInfo);
                    }
                    SlotField (null);

                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();

		GUILayout.Space(4f);
		GUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected exUIControl.SlotInfo SlotField ( exUIControl.SlotInfo _slot ) {
        bool isNew = _slot == null ? true : false;
        exUIControl.SlotInfo slot = _slot;
        if ( isNew )
            slot = new exUIControl.SlotInfo();

        EditorGUILayout.BeginHorizontal();
            slot.receiver = EditorGUILayout.ObjectField( "Receiver", slot.receiver, typeof(GameObject), true ) as GameObject;

            if ( isNew == false ) {
                if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                }
            }
            GUILayout.Space(3f);
        GUILayout.EndHorizontal();

        return slot;
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
    }
}

