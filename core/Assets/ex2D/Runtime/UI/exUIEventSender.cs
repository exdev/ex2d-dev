// ======================================================================================
// File         : exUIEventSender.cs
// Author       : Wu Jie 
// Last Change  : 10/04/2013 | 15:55:00 PM | Friday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIEventSender : MonoBehaviour {

    [System.Serializable]
    public class SlotInfo {
        public GameObject receiver = null;
        public string method = "";
    }

    [System.Serializable]
    public class Emitter {
        public string eventName;
        public List<SlotInfo> slots;
    }

    public List<Emitter> emitterList = new List<Emitter>();

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        exUIControl control = GetComponent<exUIControl>();
        if ( control != null ) {
            Type controlType = control.GetType();

            foreach ( Emitter emitter in emitterList ) {

                EventInfo eventInfo = controlType.GetEvent(emitter.eventName);
                if ( eventInfo != null ) {

                    foreach ( SlotInfo slot in emitter.slots ) {

                        bool foundMethod = false;

                        MonoBehaviour[] allMonoBehaviours = slot.receiver.GetComponents<MonoBehaviour>();
                        foreach ( MonoBehaviour monoBehaviour in allMonoBehaviours ) {

                            MethodInfo mi = monoBehaviour.GetType().GetMethod( slot.method, 
                                                                               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                                               null,
                                                                               new Type [] {
                                                                               typeof(GameObject),
                                                                               }, 
                                                                               null );
                            if ( mi != null ) {
                                Delegate delegateForMethod = Delegate.CreateDelegate( typeof(System.Action<GameObject>), monoBehaviour, mi);
                                eventInfo.AddEventHandler(control, delegateForMethod);
                                foundMethod = true;
                            }
                        }

                        if ( foundMethod == false ) {
                            Debug.LogWarning ("Can not find method " + slot.method + " in " + slot.receiver.name );
                        }
                    } 
                }
                else {
                    Debug.LogWarning ("Can not find event " + emitter.eventName + " in " + gameObject.name );
                }
            }
        }
        else {
            Debug.LogWarning ("Can not find exUIControl in this GameObject");
        }
    } 

    // DISABLE { 
    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // protected void ProcessSlots ( List<SlotInfo> _slotInfos ) {
    //     for ( int i = 0; i < _slotInfos.Count; ++i ) {
    //         SlotInfo slot = _slotInfos[i];
    //         if ( slot.receiver != null ) {
    //             slot.receiver.SendMessage ( slot.method, SendMessageOptions.DontRequireReceiver );
    //         }
    //     }
    // }
    // } DISABLE end 
}

