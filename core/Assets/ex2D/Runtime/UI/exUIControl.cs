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
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIControl : exPlane {

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

