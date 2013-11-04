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
using System.Reflection;

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
    
    protected SerializedProperty priorityProp;
    protected SerializedProperty activeProp;
    protected SerializedProperty grabMouseOrTouchProp;
    protected SerializedProperty useColliderProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        priorityProp = serializedObject.FindProperty("priority");
        activeProp = serializedObject.FindProperty("active_");
        grabMouseOrTouchProp = serializedObject.FindProperty("grabMouseOrTouch");
        useColliderProp = serializedObject.FindProperty("useCollider");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        // if settingsStyles is null
        if ( styles == null ) {
            styles = new Styles();
        }

        EditorGUILayout.PropertyField ( priorityProp );

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
            List<string> eventDefNameList = new List<string>(); 
            eventDefNameList.Add( "Event List" );
            eventDefNameList.AddRange( uiControl.GetEventDefNames() );

            foreach ( exUIControl.EventTrigger eventTrigger in uiControl.events ) {
                int idx = eventDefNameList.IndexOf(eventTrigger.name);
                if ( idx != -1 ) {
                    eventDefNameList.RemoveAt(idx);
                }
            }

            int choice = EditorGUILayout.Popup ( "Add Event", 0, eventDefNameList.ToArray() );
            if ( choice != 0 ) {
                exUIControl.EventDef eventDef = uiControl.GetEventDef( eventDefNameList[choice] );
                exUIControl.EventTrigger newTrigger = new exUIControl.EventTrigger ( eventDef.name );
                uiControl.events.Add(newTrigger);
                EditorUtility.SetDirty(target);
            }

            // event triggers
            for ( int i = 0; i < uiControl.events.Count; ++i ) {
                EditorGUILayout.Space();

                exUIControl.EventTrigger eventTrigger = uiControl.events[i];
                exUIControl.EventDef eventDef = uiControl.GetEventDef( eventTrigger.name );
                if ( EventField ( eventTrigger, eventDef ) ) {
                    uiControl.events.RemoveAt(i);
                    --i;
                    EditorUtility.SetDirty(target);
                }

                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.Space();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected bool EventField ( exUIControl.EventTrigger _eventTrigger, exUIControl.EventDef _def ) {
        bool deleted = false;

		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);

            GUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                    // name
                    GUILayout.Toggle( true, _def.name, "dragtab");

                    // delete
                    if ( GUILayout.Button( styles.iconToolbarMinus, 
                                           "InvisibleButton", 
                                           GUILayout.Width(styles.iconToolbarMinus.width), 
                                           GUILayout.Height(styles.iconToolbarMinus.height) ) ) 
                    {
                        deleted = true;
                    }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
                GUILayout.BeginVertical();

                    // slots
                    for ( int i = 0; i < _eventTrigger.slots.Count; ++i ) {
                        exUIControl.SlotInfo slotInfo = SlotField ( _eventTrigger.slots[i], _def );
                        if ( slotInfo == null ) {
                            _eventTrigger.slots.RemoveAt(i);
                            --i;
                            EditorUtility.SetDirty(target);
                        }
                    }

                    // new slot
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GameObject receiver = EditorGUILayout.ObjectField( null, typeof(GameObject), true, GUILayout.Width(150) ) as GameObject;
                        if ( receiver != null ) {
                            exUIControl.SlotInfo slotInfo = new exUIControl.SlotInfo();
                            slotInfo.receiver = receiver;
                            _eventTrigger.slots.Add(slotInfo);
                            EditorUtility.SetDirty(target);
                        }
                        GUILayout.Label( styles.iconToolbarPlus, GUILayout.Width(20) );
                    EditorGUILayout.EndHorizontal();

                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();

		GUILayout.Space(4f);
		GUILayout.EndHorizontal();

        return deleted;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected exUIControl.SlotInfo SlotField ( exUIControl.SlotInfo _slot, exUIControl.EventDef _eventDef ) {
        exUIControl.SlotInfo slot = _slot;

        EditorGUILayout.BeginHorizontal();
            // receiver
            EditorGUI.BeginChangeCheck();
            slot.receiver = EditorGUILayout.ObjectField( slot.receiver, typeof(GameObject), true ) as GameObject;
            if ( EditorGUI.EndChangeCheck() ) {
                EditorUtility.SetDirty(target);
            }

            if ( slot.receiver != null ) {
                // get valid methods
                List<string> methodNames = new List<string>(); 
                methodNames.Add( "None" );

                MonoBehaviour[] allMonoBehaviours = slot.receiver.GetComponents<MonoBehaviour>();
                for ( int i = 0; i < allMonoBehaviours.Length; ++i ) {
                    MonoBehaviour monoBehaviour =  allMonoBehaviours[i]; 

                    // don't get method from control
                    if ( monoBehaviour is exUIControl )
                        continue;

                    MethodInfo[] methods = monoBehaviour.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    for ( int m = 0; m < methods.Length; ++m ) {
                        MethodInfo mi = methods[m];
                        ParameterInfo[] miParameterTypes = mi.GetParameters();
                        if ( mi.ReturnType == typeof(void) && 
                             miParameterTypes.Length == _eventDef.parameterTypes.Length ) 
                        {
                            bool notMatch = false;
                            for ( int p = 0; p < miParameterTypes.Length; ++p ) {
                                if ( miParameterTypes[p].ParameterType != _eventDef.parameterTypes[p] ) {
                                    notMatch = true;
                                    break;
                                }
                            }

                            if ( notMatch == false && methodNames.IndexOf(mi.Name) == -1 ) {
                                methodNames.Add(mi.Name);
                            }
                        }
                    }
                }

                EditorGUI.BeginChangeCheck();
                int choice = methodNames.IndexOf(_slot.method);
                choice = EditorGUILayout.Popup ( choice == -1 ? 0 : choice, methodNames.ToArray(), GUILayout.Width(100) );
                if ( EditorGUI.EndChangeCheck() ) {
                    _slot.method = methodNames[choice];
                    EditorUtility.SetDirty(target);
                }
            }
            else {
                slot = null;
            }

            // Delete
            if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                slot = null;
            }
            GUILayout.Space(3f);
        EditorGUILayout.EndHorizontal();

        return slot;
    }
}

