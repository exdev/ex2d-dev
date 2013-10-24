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

    public enum ShowCondition {
        Always,
        OnlyIfNeeded,
        WhenDragging,
    }

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
        new EventDef ( "onScrollFinished",   new Type[] { typeof(exUIControl) }, typeof(Action<exUIControl>) ),
        new EventDef ( "onContentResized",   new Type[] { typeof(exUIControl), typeof(Vector2) }, typeof(Action<exUIControl,Vector2>) ),
    };

    // events
    public event System.Action<exUIControl,Vector2> onScroll;
    public event System.Action<exUIControl> onScrollFinished;
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
    public ShowCondition showCondition = ShowCondition.Always;
    public bool allowHorizontalScroll = true;
    public bool allowVerticalScroll = true;
    public Transform contentAnchor = null;

    public float scrollSpeed = 0.5f;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    //
    bool dragging = false;
    int draggingID = -1;
    Vector3 originalAnchorPos = Vector3.zero;
    Vector2 scrollOffset = Vector2.zero;

    bool damping = false;
    Vector2 velocity = Vector2.zero;

    bool spring = false;

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

                damping = false;
                spring = false;
                velocity = Vector2.zero;

                exUIMng.inst.SetFocus(this);
            }
        };

        onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
            if ( draggable && ( _point.isTouch || _point.GetMouseButton(0) ) && _point.id == draggingID ) {
                if ( dragging ) {
                    dragging = false;
                    draggingID = -1;

                    StartScroll ();
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
                    if ( Mathf.Abs(constrainOffset.x) > 0.001f ) delta.x *= 0.5f;
                    if ( Mathf.Abs(constrainOffset.y) > 0.001f ) delta.y *= 0.5f;

                    //
                    velocity = Vector2.Lerp ( velocity, velocity + (delta / Time.deltaTime) * scrollSpeed, 0.67f );
                    if ( Mathf.Sign(velocity.x) != Mathf.Sign(delta.x) )
                        velocity.x = 0.0f;
                    if ( Mathf.Sign(velocity.y) != Mathf.Sign(delta.y) )
                        velocity.y = 0.0f;

                    Scroll (delta);

                    break;
                }
            }
        };

        onMouseWheel += delegate ( exUIControl _sender, float _delta ) {
            // TODO: if ( mouseWheelByHorizontal )

            Vector2 delta = new Vector2( 0.0f, -_delta * 100.0f );
            Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset ( new Rect( scrollOffset.x, scrollOffset.y, width, height ), 
                                                                             new Rect( 0.0f, 0.0f, contentSize_.x, contentSize_.y ) );
            if ( Mathf.Abs(constrainOffset.y) > 0.001f ) delta.y *= 0.5f;

            velocity = Vector2.Lerp ( velocity, velocity + (delta / Time.deltaTime) * scrollSpeed, 0.67f );
            if ( Mathf.Sign(velocity.x) != Mathf.Sign(delta.x) )
                velocity.x = 0.0f;
            if ( Mathf.Sign(velocity.y) != Mathf.Sign(delta.y) )
                velocity.y = 0.0f;

            Scroll (delta);
            StartScroll ();
        };
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LateUpdate () {
        Vector2 constrainOffset = Vector2.zero;
        Vector2 deltaScroll = Vector2.zero;
        bool doScroll = (damping || spring);

        if ( damping || spring  )
            constrainOffset = exGeometryUtility.GetConstrainOffset ( new Rect( scrollOffset.x, scrollOffset.y, width, height ), 
                                                                     new Rect( 0.0f, 0.0f, contentSize_.x, contentSize_.y ) );

        // deceleration
        velocity.x *= 0.9f;
        velocity.y *= 0.9f;

        // process damping first
        if ( damping ) {

            if ( Mathf.Abs(constrainOffset.x) > 0.001f ) {
                if ( dragEffect != DragEffect.MomentumAndSpring ) {
                    velocity.x = 0.0f;
                }
                else {
                    spring = true;
                }

                // more deceleration
                // velocity.x *= 0.8f;
            }

            if ( Mathf.Abs(constrainOffset.y) > 0.001f ) {
                if ( dragEffect != DragEffect.MomentumAndSpring ) {
                    velocity.y = 0.0f;
                }
                else {
                    spring = true;
                }

                // more deceleration
                // velocity.y *= 0.8f;
            }

            //
            if ( velocity.sqrMagnitude < 1.0f ) {
                damping = false;
                velocity = Vector2.zero;
            }
            else {
                deltaScroll = velocity * Time.deltaTime;
            }
        }

        // process spring
        if ( spring ) {
            Vector2 before = contentAnchor.localPosition;
            Vector2 after = exMath.SpringLerp ( before, before - constrainOffset, 15.0f, Time.deltaTime );
            Vector2 deltaSpring = after - before;

            if ( deltaSpring.sqrMagnitude < 0.001f ) {
                deltaScroll = -constrainOffset;
                spring = false;
            }
            else {
                deltaScroll = deltaScroll + deltaSpring;
            }
        }

        //
        if ( doScroll ) {
            Scroll ( deltaScroll );

            bool shouldFinish = (damping || spring); 
            if ( shouldFinish ) {
                if ( onScrollFinished != null ) 
                    onScrollFinished(this);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void StartScroll () {
        if ( dragEffect != DragEffect.None ) {
            damping = true;

            Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset ( new Rect( scrollOffset.x, scrollOffset.y, width, height ), 
                                                                             new Rect( 0.0f, 0.0f, contentSize_.x, contentSize_.y ) );
            if ( Mathf.Abs(constrainOffset.x) > 0.001f ) {
                if ( dragEffect == DragEffect.MomentumAndSpring ) {
                    velocity.x *= 0.5f;
                }
            }

            if ( Mathf.Abs(constrainOffset.y) > 0.001f ) {
                if ( dragEffect == DragEffect.MomentumAndSpring ) {
                    velocity.y *= 0.5f;
                }
            }
        }
        else {
            velocity = Vector2.zero;
            damping = false;

            if ( onScrollFinished != null ) 
                onScrollFinished(this);
        }
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

        if ( contentAnchor != null ) {
            contentAnchor.localPosition = new Vector3 ( originalAnchorPos.x + scrollOffset.x,
                                                        originalAnchorPos.y + scrollOffset.y,
                                                        originalAnchorPos.z );
        }

        if ( onScroll != null ) {
            onScroll ( this, scrollOffset );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Vector2 GetScrollOffset () {
        return scrollOffset;
    }
}
