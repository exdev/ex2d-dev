// ======================================================================================
// File         : exUIScrollView.cs
// Author       : Wu Jie 
// Last Change  : 10/16/2013 | 14:22:21 PM | Wednesday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIScrollView : exUIControl {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    public override EventDef GetEventDef ( string _name ) {
        EventDef eventDef = exUIControl.FindEventDef ( eventDefs, _name );
        if ( eventDef == null )
            eventDef = base.GetEventDef(_name);
        return eventDef;
    }

    public override string[] GetEventDefNames () {
        string[] baseNames = base.GetEventDefNames();
        string[] names = new string[baseNames.Length + eventDefs.Length];

        for ( int i = 0; i < baseNames.Length; ++i ) {
            names[i] = baseNames[i];
        }

        for ( int i = 0; i < eventDefs.Length; ++i ) {
            names[i+baseNames.Length] = eventDefs[i].name;
        }

        return names;
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // event-defs
    public static new EventDef[] eventDefs = new EventDef[] {
        new EventDef ( "onScroll",           new Type[] { typeof(exUIControl), typeof(Vector2) }, typeof(Action<exUIControl,Vector2>) ),
        new EventDef ( "onContentResized",   new Type[] { typeof(exUIControl), typeof(Vector2) }, typeof(Action<exUIControl,Vector2>) ),
    };

    // events
    public event System.Action<exUIControl,Vector2> onScroll;
    public event System.Action<exUIControl,Vector2> onContentResized;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    //
    [SerializeField] protected Vector2 contentSize_ = Vector2.zero;
    public Vector2 contentSize {
        get { return contentSize_; }
        set {
            if ( contentSize_ != value ) {
                contentSize_ = value;
                if ( onContentResized != null ) 
                    onContentResized ( this, contentSize_ );
            }
        }
    }

    public bool acceptMouseDrag = false;
    public Transform contentAnchor = null;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    //
    bool dragging = false;
    int draggingID = -1;
    Vector3 originalAnchorPos = Vector3.zero;
    Vector2 scrollOffset = Vector2.zero;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();
        grabMouseOrTouch = true;

        if ( contentAnchor != null )
            originalAnchorPos = contentAnchor.localPosition;

        onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
            if ( dragging )
                return;

            if ( _point.isTouch || ( acceptMouseDrag && _point.GetMouseButton(0) ) ) {
                dragging = true;
                draggingID = _point.id;

                exUIMng.inst.SetFocus(this);
            }
        };

        onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
            if ( ( _point.isTouch || ( acceptMouseDrag && _point.GetMouseButton(0)) ) && _point.id == draggingID  ) {
                if ( dragging ) {
                    dragging = false;
                    draggingID = -1;
                }
            }
        };

        onHoverMove += delegate ( exUIControl _sender, List<exHotPoint> _points ) {
            for ( int i = 0; i < _points.Count; ++i ) {
                exHotPoint point = _points[i];
                if ( ( point.isTouch || ( acceptMouseDrag && point.GetMouseButton(0)) ) && point.id == draggingID  ) {
                    Scroll ( point.worldDelta );
                    break;
                }
            }
        };
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Scroll ( Vector2 _delta ) {
        scrollOffset += _delta;

        if ( contentAnchor != null )
            contentAnchor.localPosition = new Vector3 ( originalAnchorPos.x + scrollOffset.x,
                                                        originalAnchorPos.y + scrollOffset.y,
                                                        originalAnchorPos.z );

        if ( onScroll != null ) 
            onScroll ( this, scrollOffset );
    }
}
