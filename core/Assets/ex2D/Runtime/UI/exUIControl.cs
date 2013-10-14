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

    [System.Serializable]
    public class EventSlot {
        public string name;
        public List<SlotInfo> slots;
        public Type[] parameterTypes;
        public Type delegateType;

        public EventSlot ( string _name, Type[] _parameterTypes, Type _delegateType ) {
            name = _name;
            parameterTypes = _parameterTypes;
            delegateType = _delegateType;
            slots = new List<SlotInfo>();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // events, slots and senders
    ///////////////////////////////////////////////////////////////////////////////

    // events
    public event Action<exUIControl> onFocus;
    public event Action<exUIControl> onUnfocus;
    public event Action<exUIControl> onActive;
    public event Action<exUIControl> onDeactive;
    public event Action<exUIControl,exHotPoint> onHoverIn;
    public event Action<exUIControl,exHotPoint> onHoverOut;
    public event Action<exUIControl,exHotPoint> onPressDown;
    public event Action<exUIControl,exHotPoint> onPressUp;
    public event Action<exUIControl,List<exHotPoint>> onPressMove;

    // event slots
    public EventSlot[] exUIControl_events = new EventSlot[] {
        new EventSlot ( "onFocus",      new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventSlot ( "onUnfocus",    new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventSlot ( "onActive",     new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventSlot ( "onDeactive",   new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventSlot ( "onHoverIn",    new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventSlot ( "onHoverOut",   new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventSlot ( "onPressDown",  new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventSlot ( "onPressUp",    new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventSlot ( "onPressMove",  new Type[] { typeof(exUIControl), typeof(List<exHotPoint>) }, typeof(Action<exUIControl,List<exHotPoint>>) ),
    };

    // event easy function
    public void Send_OnFocus () { if ( onFocus != null ) onFocus (this); }
    public void Send_OnUnfocus () { if ( onUnfocus != null ) onUnfocus (this); }
    public void Send_OnActive () { if ( onActive != null ) onActive (this); }
    public void Send_OnDeactive () { if ( onDeactive != null ) onDeactive (this); }
    public void Send_OnHoverIn ( exHotPoint _hotPoint ) { if ( onHoverIn != null ) onHoverIn (this,_hotPoint); }
    public void Send_OnHoverOut ( exHotPoint _hotPoint ) { if ( onHoverOut != null ) onHoverOut (this,_hotPoint); }
    public void Send_OnPressDown ( exHotPoint _hotPoint ) { if ( onPressDown != null ) onPressDown (this,_hotPoint); }
    public void Send_OnPressUp ( exHotPoint _hotPoint ) { if ( onPressUp != null ) onPressUp (this,_hotPoint); }
    public void Send_OnPressMove ( List<exHotPoint> _hotPoints ) { if ( onPressMove != null ) onPressMove (this,_hotPoints); }

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    [System.NonSerialized] public exUIControl parent;
    [System.NonSerialized] public List<exUIControl> children = new List<exUIControl>();

    [SerializeField] protected bool active_ = true;
    public bool activeInHierarchy {
        get {
            if ( active_ == false )
                return false;

            exUIControl p = parent;
            while ( p != null ) {
                if ( p.active_ == false )
                    return false;
                p = p.parent;
            }
            return true;
        }
        set {
            if ( active_ != value ) {
                active_ = value;
                if ( active_ )
                    Send_OnActive();
                else 
                    Send_OnDeactive();

                for ( int i = 0; i < children.Count; ++i ) {
                    children[i].activeInHierarchy = value;
                }
            }
        }
    }
    public bool activeSelf {
        get { 
            return active_; 
        }
        set { 
            if ( active_ != value ) {
                active_ = value;
                if ( active_ )
                    Send_OnActive();
                else 
                    Send_OnDeactive();
            }
        }
    }

    public bool useCollider = false;
    public bool grabMouseOrTouch = false;

    ///////////////////////////////////////////////////////////////////////////////
    // static functions
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
        InitEvents (exUIControl_events);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        if ( parent != null ) {
            parent.RemoveChild(this);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void InitEvents ( EventSlot[] _events ) {
        foreach ( EventSlot eventSlot in _events ) {
            AddSlotsToEvent ( eventSlot.name, eventSlot.slots, eventSlot.parameterTypes, eventSlot.delegateType );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void AddSlotsToEvent ( string _eventName, List<SlotInfo> _slots, Type[] _parameterTypes, Type _delegateType ) {
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

