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
    public class EventDef {
        public string name;
        public Type[] parameterTypes;
        public Type delegateType;

        public EventDef ( string _name, Type[] _parameterTypes, Type _delegateType ) {
            name = _name;
            parameterTypes = _parameterTypes;
            delegateType = _delegateType;
        }
    }

    public static EventDef FindEventDef ( EventDef[] _eventDefs, string _name ) {
        for ( int i = 0; i < _eventDefs.Length; ++i ) {
            EventDef eventDef = _eventDefs[i];
            if ( eventDef.name == _name )
                return eventDef; 
        }
        return null;
    }

    [System.Serializable]
    public class SlotInfo {
        public GameObject receiver = null;
        public string method = "";
    }

    [System.Serializable]
    public class EventTrigger {
        public string name;
        public List<SlotInfo> slots;

        public EventTrigger ( string _name ) {
            name = _name;
            slots = new List<SlotInfo>();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // events, slots and senders
    ///////////////////////////////////////////////////////////////////////////////

    // event-defs
    public static EventDef[] eventDefs = new EventDef[] {
        new EventDef ( "onFocus",      new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onUnfocus",    new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onActive",     new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onDeactive",   new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onHoverIn",    new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventDef ( "onHoverOut",   new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventDef ( "onHoverMove",  new Type[] { typeof(exUIControl), typeof(List<exHotPoint>) }, typeof(Action<exUIControl,List<exHotPoint>>) ),
        new EventDef ( "onPressDown",  new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventDef ( "onPressUp",    new Type[] { typeof(exUIControl), typeof(exHotPoint) }, typeof(Action<exUIControl,exHotPoint>) ),
        new EventDef ( "onMouseWheel", new Type[] { typeof(exUIControl), typeof(float) }, typeof(Action<exUIControl,float>) ),
    };

    // events
    public event Action<exUIControl> onFocus;
    public event Action<exUIControl> onUnfocus;
    public event Action<exUIControl> onActive;
    public event Action<exUIControl> onDeactive;
    public event Action<exUIControl,exHotPoint> onHoverIn;
    public event Action<exUIControl,exHotPoint> onHoverOut;
    public event Action<exUIControl,List<exHotPoint>> onHoverMove;
    public event Action<exUIControl,exHotPoint> onPressDown;
    public event Action<exUIControl,exHotPoint> onPressUp;
    public event Action<exUIControl,float> onMouseWheel;

    // event easy function
    public void Send_OnFocus () { if ( onFocus != null ) onFocus (this); }
    public void Send_OnUnfocus () { if ( onUnfocus != null ) onUnfocus (this); }
    public void Send_OnActive () { if ( onActive != null ) onActive (this); }
    public void Send_OnDeactive () { if ( onDeactive != null ) onDeactive (this); }
    public void Send_OnHoverIn ( exHotPoint _hotPoint ) { if ( onHoverIn != null ) onHoverIn (this,_hotPoint); }
    public void Send_OnHoverOut ( exHotPoint _hotPoint ) { if ( onHoverOut != null ) onHoverOut (this,_hotPoint); }
    public void Send_OnHoverMove ( List<exHotPoint> _hotPoints ) { if ( onHoverMove != null ) onHoverMove (this,_hotPoints); }
    public void Send_OnPressDown ( exHotPoint _hotPoint ) { if ( onPressDown != null ) onPressDown (this,_hotPoint); }
    public void Send_OnPressUp ( exHotPoint _hotPoint ) { if ( onPressUp != null ) onPressUp (this,_hotPoint); }
    public void Send_OnMouseWheel ( float _delta ) { if ( onMouseWheel != null ) onMouseWheel (this,_delta); }

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

    public int priority = 0;
    public bool useCollider = false;
    public bool grabMouseOrTouch = false;
    public List<EventTrigger> events = new List<EventTrigger>();

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void Awake () {
        for ( int i = 0; i < events.Count; ++i ) {
            EventTrigger eventTrigger = events[i];
            EventDef def = GetEventDef(eventTrigger.name);
            AddEventHandlers ( def.name, 
                               def.parameterTypes, 
                               def.delegateType, 
                               eventTrigger.slots );
        }
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

    public virtual EventDef GetEventDef ( string _name ) {
        return exUIControl.FindEventDef ( eventDefs, _name );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public virtual string[] GetEventDefNames () {
        string[] names = new string[eventDefs.Length];
        for ( int i = 0; i < eventDefs.Length; ++i ) {
            names[i] = eventDefs[i].name;
        }
        return names;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddEventHandlers ( string _eventName, Type[] _parameterTypes, Type _delegateType, List<SlotInfo> _slots ) {
        Type controlType = this.GetType();
        EventInfo eventInfo = controlType.GetEvent(_eventName);
        if ( eventInfo != null ) {

            foreach ( SlotInfo slot in _slots ) {

                bool foundMethod = false;

                MonoBehaviour[] allMonoBehaviours = slot.receiver.GetComponents<MonoBehaviour>();
                for ( int i = 0; i < allMonoBehaviours.Length; ++i ) {
                    MonoBehaviour monoBehaviour =  allMonoBehaviours[i]; 

                    // don't get method from control
                    if ( monoBehaviour is exUIControl )
                        continue;

                    MethodInfo mi = monoBehaviour.GetType().GetMethod( slot.method, 
                                                                       BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                                       null,
                                                                       _parameterTypes,
                                                                       null );
                    if ( mi != null ) {
                        Delegate delegateForMethod = Delegate.CreateDelegate( _delegateType, monoBehaviour, mi);
                        // NOTE: in iPhone, AddEventHandler will report "Attempting to JIT compile method" error.
                        //       this can only be fixed by use the following code. founded from
                        //       http://monotouch.2284126.n4.nabble.com/Attempting-to-JIT-compile-method-what-am-I-doing-wrong-td4492920.html
                        // eventInfo.AddEventHandler(this, delegateForMethod);
                        MethodInfo addMI = eventInfo.GetAddMethod();
                        addMI.Invoke( this, new object[] { delegateForMethod } );
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

    public bool IsSelfOrAncestorOf ( exUIControl _ctrl ) {
        if ( _ctrl == null )
            return false;

        if ( _ctrl == this )
            return true;

        exUIControl next = _ctrl.parent;
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

    public void AddChild ( exUIControl _ctrl ) {
        if ( _ctrl == null )
            return;

        if ( _ctrl.parent == this )
            return;

        // you can not add your parent or yourself as your child
        if ( _ctrl.IsSelfOrAncestorOf (this) )
            return;

        exUIControl lastParent = _ctrl.parent;
        if ( lastParent != null ) {
            lastParent.RemoveChild(_ctrl);
        }

        children.Add(_ctrl);
        _ctrl.parent = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RemoveChild ( exUIControl _ctrl ) {
        if ( _ctrl == null )
            return;

        int idx = children.IndexOf(_ctrl);
        if ( idx != -1 ) {
            children.RemoveAt(idx);
            _ctrl.parent = null;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Internal_SetActive ( bool _active ) {
        active_ = _active;
    }
}

