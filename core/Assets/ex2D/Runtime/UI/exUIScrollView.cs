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

	public enum DragEffect {
		None,
		Momentum,
		MomentumAndSpring,
	}

    // public enum ShowCondition {
    //     Always,
    //     OnlyIfNeeded,
    //     WhenDragging,
    // }

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

    public bool draggable = true; // can use left-mouse or touch drag to scroll the view
    public DragEffect dragEffect = DragEffect.MomentumAndSpring;
    public bool allowHorizontalScroll = true;
    public bool allowVerticalScroll = true;
    public Transform contentAnchor = null;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    //
    bool dragging = false;
    int draggingID = -1;
    Vector3 originalAnchorPos = Vector3.zero;
    Vector2 scrollOffset = Vector2.zero;

    bool damping = false;
    bool bouncing = false;

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

            if ( draggable && ( _point.isTouch || _point.GetMouseButton(0) ) ) {
                dragging = true;
                draggingID = _point.id;

                exUIMng.inst.SetFocus(this);
            }
        };

        onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
            if ( draggable && ( _point.isTouch || _point.GetMouseButton(0) ) && _point.id == draggingID ) {
                if ( dragging ) {
                    dragging = false;
                    draggingID = -1;
                }
            }
        };

        onHoverMove += delegate ( exUIControl _sender, List<exHotPoint> _points ) {
            for ( int i = 0; i < _points.Count; ++i ) {
                exHotPoint point = _points[i];
                if ( draggable && ( point.isTouch || point.GetMouseButton(0) ) && point.id == draggingID  ) {
                    Vector2 delta = point.worldDelta; 
                    Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset ( new Rect( scrollOffset.x, scrollOffset.y, width, height ), 
                                                                                     new Rect( 0.0f, 0.0f, contentSize_.x, contentSize_.y ) );
                    if ( constrainOffset.x > 0.001f ) delta.x *= 0.5f;
                    if ( constrainOffset.y > 0.001f ) delta.y *= 0.5f;

                    Scroll (delta);
                    break;
                }
            }
        };

        onMouseWheel += delegate ( exUIControl _sender, float _delta ) {
            // TODO: if ( mouseWheelByHorizontal )
        };
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Update () {
        // TODO:
        if ( damping ) {
        }

        if ( bouncing ) {
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void StartScroll ( Vector2 _delta ) {
        // TODO:
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Scroll ( Vector2 _delta ) {
        if ( allowHorizontalScroll == false )
            _delta.x = 0.0f;
        if ( allowVerticalScroll == false )
            _delta.y = 0.0f;

        scrollOffset += _delta;

        if ( dragEffect != DragEffect.MomentumAndSpring ) {
            scrollOffset.x = Mathf.Clamp( scrollOffset.x, 0.0f, contentSize_.x - width );
            scrollOffset.y = Mathf.Clamp( scrollOffset.y, 0.0f, contentSize_.y - height );
        }

        if ( contentAnchor != null )
            contentAnchor.localPosition = new Vector3 ( originalAnchorPos.x + scrollOffset.x,
                                                        originalAnchorPos.y + scrollOffset.y,
                                                        originalAnchorPos.z );

        if ( onScroll != null ) 
            onScroll ( this, scrollOffset );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Vector2 GetScrollOffset () {
        return scrollOffset;
    }
}
