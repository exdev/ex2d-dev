// ======================================================================================
// File         : exUIEventDispatcher.cs
// Author       : Wu Jie 
// Last Change  : 10/04/2013 | 11:09:53 AM | Friday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIEventDispatcher : MonoBehaviour {

    [System.Serializable]
    public class MessageInfo {
        public GameObject receiver = null;
        public string method = "";
    }

    public float width = 0.0f;
    public float height = 0.0f;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool isActive {
        get {
            if ( gameObject.activeInHierarchy == false )
                return false;

            if ( enabled == false )
                return false;
            exUIEventDispatcher p = parent;
            while ( p != null ) {
                if ( p.enabled == false )
                    return false;
                p = p.parent;
            }
            return true;
        }
    }

    public exUIEventDispatcher parent;
    public List<exUIEventDispatcher> children = new List<exUIEventDispatcher>();

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void FindAndAddChild ( exUIEventDispatcher _dispatcher ) {
        _dispatcher.children.Clear();
        FindAndAddChildRecursively (_dispatcher, _dispatcher.transform );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void FindAndAddChildRecursively ( exUIEventDispatcher _dispatcher, Transform _trans ) {
        foreach ( Transform child in _trans ) {
            exUIEventDispatcher child_dispatcher = child.GetComponent<exUIEventDispatcher>();
            if ( child_dispatcher ) {
                _dispatcher.AddChild (child_dispatcher);
                exUIEventDispatcher.FindAndAddChild (child_dispatcher);
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

    void OnDestroy () {
        if ( parent != null ) {
            parent.RemoveChild(this);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool IsSelfOrAncestorOf ( exUIEventDispatcher _dispatcher ) {
        if ( _dispatcher == null )
            return false;

        if ( _dispatcher == this )
            return true;

        exUIEventDispatcher next = _dispatcher.parent;
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

    public void AddChild ( exUIEventDispatcher _dispatcher ) {
        if ( _dispatcher == null )
            return;

        if ( _dispatcher.parent == this )
            return;

        // you can not add your parent or yourself as your child
        if ( _dispatcher.IsSelfOrAncestorOf (this) )
            return;

        exUIEventDispatcher lastParent = _dispatcher.parent;
        if ( lastParent != null ) {
            lastParent.RemoveChild(_dispatcher);
        }

        children.Add(_dispatcher);
        _dispatcher.parent = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RemoveChild ( exUIEventDispatcher _dispatcher ) {
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

    protected void ProcessMessageInfoList ( List<MessageInfo> _messageInfos ) {
        for ( int i = 0; i < _messageInfos.Count; ++i ) {
            MessageInfo msgInfo = _messageInfos[i];
            if ( msgInfo.receiver != null ) {
                msgInfo.receiver.SendMessage ( msgInfo.method, SendMessageOptions.DontRequireReceiver );
            }
        }
    }
}

