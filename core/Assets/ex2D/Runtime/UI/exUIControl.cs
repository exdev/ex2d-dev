// ======================================================================================
// File         : exUIControl.cs
// Author       : Wu Jie 
// Last Change  : 10/04/2013 | 15:59:02 PM | Friday,October
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

public class exUIControl : exPlane {

    [System.Serializable]
    public class SlotInfo {
        public GameObject receiver = null;
        public string method = "";
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool isActive {
        get {
            if ( gameObject.activeInHierarchy == false )
                return false;

            if ( enabled == false )
                return false;
            exUIControl p = parent;
            while ( p != null ) {
                if ( p.enabled == false )
                    return false;
                p = p.parent;
            }
            return true;
        }
    }

    [System.NonSerialized] public exUIControl parent;
    [System.NonSerialized] public List<exUIControl> children = new List<exUIControl>();

    ///////////////////////////////////////////////////////////////////////////////
    // events
    ///////////////////////////////////////////////////////////////////////////////

    // action ( sender )
    public event System.Action<GameObject> onHoverIn;
    public event System.Action<GameObject> onHoverOut;

    ///////////////////////////////////////////////////////////////////////////////
    // slots
    ///////////////////////////////////////////////////////////////////////////////

    public List<SlotInfo> onHoverInSlots;
    public List<SlotInfo> onHoverOutSlots;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void FindAndAddChild ( exUIControl _dispatcher ) {
        _dispatcher.children.Clear();
        FindAndAddChildRecursively (_dispatcher, _dispatcher.transform );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void FindAndAddChildRecursively ( exUIControl _dispatcher, Transform _trans ) {
        foreach ( Transform child in _trans ) {
            exUIControl child_dispatcher = child.GetComponent<exUIControl>();
            if ( child_dispatcher ) {
                _dispatcher.AddChild (child_dispatcher);
                exUIControl.FindAndAddChild (child_dispatcher);
            }
            else {
                FindAndAddChildRecursively( _dispatcher, child );
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void Awake () {
        AddSlotsToEvent ( "onHoverIn", onHoverInSlots, new Type[] { typeof(GameObject) }, typeof(System.Action<GameObject>) );
        AddSlotsToEvent ( "onHoverOut", onHoverOutSlots, new Type[] { typeof(GameObject) }, typeof(System.Action<GameObject>) );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddSlotsToEvent ( string _eventName, List<SlotInfo> _slots, Type[] _parameterTypes, Type _delegateType ) {
        Type controlType = this.GetType();
        EventInfo eventInfo = controlType.GetEvent(_eventName);
        if ( eventInfo != null ) {

            foreach ( SlotInfo slot in _slots ) {

                bool foundMethod = false;

                MonoBehaviour[] allMonoBehaviours = slot.receiver.GetComponents<MonoBehaviour>();
                foreach ( MonoBehaviour monoBehaviour in allMonoBehaviours ) {

                    MethodInfo mi = monoBehaviour.GetType().GetMethod( slot.method, 
                                                                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                                       null,
                                                                       _parameterTypes,
                                                                       null );
                    if ( mi != null ) {
                        var delegateForMethod = Delegate.CreateDelegate( _delegateType, monoBehaviour, mi);
                        eventInfo.AddEventHandler(this, delegateForMethod);
                        foundMethod = true;
                    }
                }

                if ( foundMethod == false ) {
                    Debug.LogWarning ("Can not find method " + slot.method + " in " + slot.receiver.name );
                }
            } 
        }
        else {
            Debug.LogWarning ("Can not find event " + _eventName + " in " + gameObject.name );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Send_OnHoverIn () {
        if ( onHoverIn != null )
            onHoverIn (gameObject);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Send_OnHoverOut () {
        if ( onHoverOut != null )
            onHoverOut (gameObject);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        if ( parent != null ) {
            parent.RemoveChild(this);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool IsSelfOrAncestorOf ( exUIControl _dispatcher ) {
        if ( _dispatcher == null )
            return false;

        if ( _dispatcher == this )
            return true;

        exUIControl next = _dispatcher.parent;
        while ( next != null ) {
            if ( next == this )
                return true;
            next = next.parent;
        }
        return false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddChild ( exUIControl _dispatcher ) {
        if ( _dispatcher == null )
            return;

        if ( _dispatcher.parent == this )
            return;

        // you can not add your parent or yourself as your child
        if ( _dispatcher.IsSelfOrAncestorOf (this) )
            return;

        exUIControl lastParent = _dispatcher.parent;
        if ( lastParent != null ) {
            lastParent.RemoveChild(_dispatcher);
        }

        children.Add(_dispatcher);
        _dispatcher.parent = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RemoveChild ( exUIControl _dispatcher ) {
        if ( _dispatcher == null )
            return;

        int idx = children.IndexOf(_dispatcher);
        if ( idx != -1 ) {
            children.RemoveAt(idx);
            _dispatcher.parent = null;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public exUIControl FindParent () {
        Transform tranParent = transform.parent;
        while ( tranParent != null ) {
            exUIControl el = tranParent.GetComponent<exUIControl>();
            if ( el != null )
                return el;
            tranParent = tranParent.parent;
        }
        return null;
    } 
}

