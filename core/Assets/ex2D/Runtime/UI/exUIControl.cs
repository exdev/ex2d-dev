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
        public bool capturePhase = false; 
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
    public static string[] eventNames = new string[] {
        "onFocus",
        "onUnfocus",
        "onActive",
        "onDeactive",
        "onHoverIn",
        "onHoverOut",
        "onHoverMove",
        "onPressDown",
        "onPressUp",
        "onMouseWheel",
    };

    // events
    List<exUIEventListener> onFocus;
    List<exUIEventListener> onUnfocus;
    List<exUIEventListener> onActive;
    List<exUIEventListener> onDeactive;
    List<exUIEventListener> onHoverIn;
    List<exUIEventListener> onHoverOut;
    List<exUIEventListener> onHoverMove;
    List<exUIEventListener> onPressDown;
    List<exUIEventListener> onPressUp;
    List<exUIEventListener> onMouseWheel;

    // event easy function
    public void OnFocus      ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onFocus,      _event ); }
    public void OnUnfocus    ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onUnfocus,    _event ); }
    public void OnActive     ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onActive,     _event ); }
    public void OnDeactive   ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onDeactive,   _event ); }
    // TODO { 
    // public void OnHoverEnter    ( exUIEvent _event )  { if ( onHoverIn    != null ) exUIMng.inst.DispatchEvent( this, onHoverIn,    _event ); }
    // public void OnHoverLeave   ( exUIEvent _event )  { if ( onHoverOut   != null ) exUIMng.inst.DispatchEvent( this, onHoverOut,   _event ); }
    // } TODO end 
    public void OnHoverIn    ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onHoverIn,    _event ); }
    public void OnHoverOut   ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onHoverOut,   _event ); }
    public void OnHoverMove  ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onHoverMove,  _event ); }
    public void OnPressDown  ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onPressDown,  _event ); }
    public void OnPressUp    ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onPressUp,    _event ); }
    public void OnMouseWheel ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, onMouseWheel, _event ); }
    
    public virtual void CacheEventListeners () {
        onFocus = eventListenerTable["onFocus"];
        onUnfocus = eventListenerTable["onUnfocus"];
        onActive = eventListenerTable["onActive"];
        onDeactive = eventListenerTable["onDeactive"];
        onHoverIn = eventListenerTable["onHoverIn"];
        onHoverOut = eventListenerTable["onHoverOut"];
        onHoverMove = eventListenerTable["onHoverMove"];
        onPressDown = eventListenerTable["onPressDown"];
        onPressUp = eventListenerTable["onPressUp"];
        onMouseWheel = eventListenerTable["onMouseWheel"];
    }

    public virtual string[] GetEventNames () {
        string[] names = new string[eventNames.Length];
        for ( int i = 0; i < eventNames.Length; ++i ) {
            names[i] = eventNames[i];
        }
        return names;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void AddEventListeners ( exUIControl _ctrl, string _eventName, List<SlotInfo> _slots ) {
        foreach ( SlotInfo slot in _slots ) {
            bool foundMethod = false;
            if ( slot.receiver == null )
                continue;

            MonoBehaviour[] allMonoBehaviours = slot.receiver.GetComponents<MonoBehaviour>();
            for ( int i = 0; i < allMonoBehaviours.Length; ++i ) {
                MonoBehaviour monoBehaviour =  allMonoBehaviours[i]; 

                // don't get method from control
                if ( monoBehaviour is exUIControl )
                    continue;

                MethodInfo mi = monoBehaviour.GetType().GetMethod( slot.method, 
                                                                   BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                                                   null,
                                                                   new Type[] { typeof(exUIEvent) },
                                                                   null );
                if ( mi != null ) {
                    Action<exUIEvent> func 
                        = (Action<exUIEvent>)Delegate.CreateDelegate( typeof(Action<exUIEvent>), monoBehaviour, mi);
                    _ctrl.AddEventListener ( _eventName, func, slot.capturePhase );
                    foundMethod = true;
                }
            }

            if ( foundMethod == false ) {
                Debug.LogWarning ("Can not find method " + slot.method + " in " + slot.receiver.name );
            }
        }
    }

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

                exUIEvent uiEvent = new exUIEvent();
                uiEvent.bubbles = false;
                if ( active_ )
                    OnActive( uiEvent );
                else 
                    OnDeactive( uiEvent );

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

                exUIEvent uiEvent = new exUIEvent();
                uiEvent.bubbles = false;
                if ( active_ )
                    OnActive( uiEvent );
                else 
                    OnDeactive( uiEvent );
            }
        }
    }

    public int priority = 0;
    public bool useCollider = false;
    public bool grabMouseOrTouch = false;
    public List<EventTrigger> events = new List<EventTrigger>();

    // event listener
    protected Dictionary<string,List<exUIEventListener>> eventListenerTable = new Dictionary<string,List<exUIEventListener>>();
    protected List<exUIEventListener>[] cachedEventListenerTable;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void Awake () {
        string[] eventNames = GetEventNames ();
        for ( int i = 0; i < eventNames.Length; ++i ) {
            string eventName = eventNames[i];
            if ( eventListenerTable.ContainsKey(eventName) == false ) {
                eventListenerTable.Add( eventName, new List<exUIEventListener>() );
            }
        }

        for ( int i = 0; i < events.Count; ++i ) {
            EventTrigger eventTrigger = events[i];
            AddEventListeners ( this, eventTrigger.name, eventTrigger.slots ); 
        }

        CacheEventListeners();
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

    public void AddEventListener ( string _name, System.Action<exUIEvent> _func, bool _capturePhase = false ) {
        List<exUIEventListener> eventListeners = null;

        if ( eventListenerTable.ContainsKey(_name) ) {
            eventListeners = eventListenerTable[_name];
            for ( int i = 0; i < eventListeners.Count; ++i ) {
                exUIEventListener eventListener = eventListeners[i];
                if ( eventListener.func == _func &&
                     eventListener.capturePhase == _capturePhase ) 
                {
                    return;
                }
            }
        }

        if ( eventListeners == null ) {
            eventListeners = new List<exUIEventListener>();
            eventListenerTable.Add(_name, eventListeners);
        }

        //
        exUIEventListener newEventInfo = new exUIEventListener();
        newEventInfo.func = _func;
        newEventInfo.capturePhase = _capturePhase;
        eventListeners.Add(newEventInfo);
    }

    public void RemoveEventListener ( string _name, System.Action<exUIEvent> _func, bool _capturePhase = false ) {
        List<exUIEventListener> eventListeners = null;

        if ( eventListenerTable.ContainsKey(_name) ) {
            eventListeners = eventListenerTable[_name];
            for ( int i = 0; i < eventListeners.Count; ++i ) {
                exUIEventListener eventListener = eventListeners[i];
                if ( eventListener.func == _func &&
                     eventListener.capturePhase == _capturePhase ) 
                {
                    eventListeners.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public void DispatchEvent ( string _name, exUIEvent _event ) {
        if ( eventListenerTable.ContainsKey(_name) ) {
            List<exUIEventListener> eventListeners = eventListenerTable[_name];
            exUIMng.inst.DispatchEvent ( this, eventListeners, _event );
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

