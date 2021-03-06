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

	public enum ContentDirection {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // event-defs
    public static new string[] eventNames = new string[] {
        "onScroll",
        "onScrollFinished",
        "onContentResized",
    };

    // events
    List<exUIEventListener> onScroll;
    List<exUIEventListener> onScrollFinished;
    List<exUIEventListener> onContentResized;

    public void OnScroll         ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onScroll", onScroll, _event ); }
    public void OnScrollFinished ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onScrollFinished", onScrollFinished,  _event ); }
    public void OnContentResized ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onContentResized", onContentResized,  _event ); }

    public override void CacheEventListeners () {
        base.CacheEventListeners();

        onScroll = eventListenerTable["onScroll"];
        onScrollFinished = eventListenerTable["onScrollFinished"];
        onContentResized = eventListenerTable["onContentResized"];
    }

    public override string[] GetEventNames () {
        string[] baseNames = base.GetEventNames();
        string[] names = new string[baseNames.Length + eventNames.Length];

        for ( int i = 0; i < baseNames.Length; ++i ) {
            names[i] = baseNames[i];
        }

        for ( int i = 0; i < eventNames.Length; ++i ) {
            names[i+baseNames.Length] = eventNames[i];
        }

        return names;
    }

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

                exUIEvent uiEvent = new exUIEvent();
                uiEvent.bubbles = false;
                OnContentResized (uiEvent);
            }
        }
    }

    public bool draggable = true; // can use left-mouse or touch drag to scroll the view
    public DragEffect dragEffect = DragEffect.MomentumAndSpring;
    public ShowCondition showCondition = ShowCondition.Always;
    public bool allowHorizontalScroll = true;
    public ContentDirection horizontalContentDir = ContentDirection.LeftToRight;
    public bool allowVerticalScroll = true;
    public ContentDirection verticalContentDir = ContentDirection.TopToBottom;
    public Transform contentAnchor = null;

    public float scrollSpeed = 0.5f;

    Vector2 scrollOffset_ = Vector2.zero;
    public Vector2 scrollOffset { get { return scrollOffset_; } }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    //
    bool dragging = false;
    int draggingID = -1;
    Vector3 originalAnchorPos = Vector3.zero;

    bool damping = false;
    Vector2 velocity = Vector2.zero;

    bool spring = false;
    Vector3 scrollDest = Vector3.zero;

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

        AddEventListener ( "onPressDown", 
                           delegate ( exUIEvent _event ) {
                               if ( dragging ) {
                                   _event.StopPropagation();
                                   return;
                               }

                               StartDrag(_event);
                           } );

        AddEventListener ( "onPressUp", 
                           delegate ( exUIEvent _event ) {
                               exUIPointEvent pointEvent = _event as exUIPointEvent;
                               if ( draggable && ( pointEvent.isTouch || pointEvent.GetMouseButton(0) ) && pointEvent.pointInfos[0].id == draggingID ) {
                                   if ( dragging ) {
                                       dragging = false;
                                       draggingID = -1;

                                       StartScroll ();
                                   }
                                   _event.StopPropagation();
                               }
                           } );

        AddEventListener ( "onHoverIn", 
                           delegate ( exUIEvent _event ) {
                               if ( dragging ) {
                                   _event.StopPropagation();
                                   return;
                               }

                               StartDrag(_event);
                           } );

        AddEventListener ( "onHoverMove", 
                           delegate ( exUIEvent _event ) {
                               if ( dragging == false ) {
                                    StartDrag(_event);
                                    return;
                               }

                               float contentX = (horizontalContentDir == ContentDirection.LeftToRight) ? 0.0f : -(contentSize_.x - width);
                               float contentY = (verticalContentDir == ContentDirection.TopToBottom) ? 0.0f : -(contentSize_.y - height);
                               Rect scrollRect = new Rect( scrollOffset_.x, scrollOffset_.y, width, height );
                               Rect contentRect = new Rect( contentX, contentY, Mathf.Max( contentSize_.x, width ), Mathf.Max( contentSize_.y, height ) );
                               Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset ( scrollRect, contentRect );

                               exUIPointEvent pointEvent = _event as exUIPointEvent;
                               for ( int i = 0; i < pointEvent.pointInfos.Length; ++i ) {
                                   exUIPointInfo point = pointEvent.pointInfos[i];
                                   if ( draggable && ( pointEvent.isTouch || pointEvent.GetMouseButton(0) ) && point.id == draggingID  ) {
                                       Vector2 delta = point.worldDelta; 
                                       delta.x = -delta.x;
                                       if ( Mathf.Abs(constrainOffset.x) > 0.001f ) delta.x *= 0.5f;
                                       if ( Mathf.Abs(constrainOffset.y) > 0.001f ) delta.y *= 0.5f;

                                       //
                                       velocity = Vector2.Lerp ( velocity, velocity + (delta / Time.deltaTime) * scrollSpeed, 0.67f );
                                       if ( Mathf.Sign(velocity.x) != Mathf.Sign(delta.x) )
                                           velocity.x = 0.0f;
                                       if ( Mathf.Sign(velocity.y) != Mathf.Sign(delta.y) )
                                           velocity.y = 0.0f;

                                       _event.StopPropagation();

                                       Scroll (delta);

                                       break;
                                   }
                               }
                           } );

        AddEventListener ( "onMouseWheel", 
                           delegate ( exUIEvent _event ) {
                               // TODO: if ( mouseWheelByHorizontal )

                               float contentX = (horizontalContentDir == ContentDirection.LeftToRight) ? 0.0f : -(contentSize_.x - width);
                               float contentY = (verticalContentDir == ContentDirection.TopToBottom) ? 0.0f : -(contentSize_.y - height);
                               exUIWheelEvent wheelEvent = _event as exUIWheelEvent;
                               Vector2 delta = new Vector2( 0.0f, -wheelEvent.delta * 100.0f );
                               Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset ( new Rect( scrollOffset_.x, scrollOffset_.y, width, height ), 
                                                                                                new Rect( contentX, contentY, contentSize_.x, contentSize_.y ) );
                               if ( Mathf.Abs(constrainOffset.y) > 0.001f ) delta.y *= 0.5f;

                               velocity = Vector2.Lerp ( velocity, velocity + (delta / Time.deltaTime) * scrollSpeed, 0.67f );
                               if ( Mathf.Sign(velocity.x) != Mathf.Sign(delta.x) )
                                   velocity.x = 0.0f;
                               if ( Mathf.Sign(velocity.y) != Mathf.Sign(delta.y) )
                                   velocity.y = 0.0f;

                               _event.StopPropagation();

                               Scroll (delta);
                               StartScroll ();
                           } );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LateUpdate () {
        Vector2 constrainOffset = Vector2.zero;
        Vector2 deltaScroll = Vector2.zero;
        bool doScroll = (damping || spring);

        float contentX = (horizontalContentDir == ContentDirection.LeftToRight) ? 0.0f : -(contentSize_.x - width);
        float contentY = (verticalContentDir == ContentDirection.TopToBottom) ? 0.0f : -(contentSize_.y - height);
        Rect scrollRect = new Rect( scrollOffset_.x, scrollOffset_.y, width, height );
        Rect contentRect = new Rect( contentX, contentY, Mathf.Max( contentSize_.x, width ), Mathf.Max( contentSize_.y, height ) );

        if ( damping || spring  )
            constrainOffset = exGeometryUtility.GetConstrainOffset ( scrollRect, contentRect );

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
            contentAnchor.localPosition = scrollDest;

            bool shouldFinish = (damping || spring); 
            if ( shouldFinish ) {
                exUIEvent uiEvent = new exUIEvent();
                uiEvent.bubbles = false;
                OnScrollFinished(uiEvent);
            }
        }
        else {
            contentAnchor.localPosition = Vector3.Lerp( contentAnchor.localPosition, scrollDest, 0.6f );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void StartDrag ( exUIEvent _event ) {
        exUIPointEvent pointEvent = _event as exUIPointEvent;
        if ( draggable && ( pointEvent.isTouch || pointEvent.GetMouseButton(0) ) ) {
            dragging = true;
            draggingID = pointEvent.mainPoint.id;

            damping = false;
            spring = false;
            velocity = Vector2.zero;

            exUIMng.inst.SetFocus(this);

            _event.StopPropagation();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void StartScroll () {
        if ( dragEffect != DragEffect.None ) {
            damping = true;

            float contentX = (horizontalContentDir == ContentDirection.LeftToRight) ? 0.0f : -(contentSize_.x - width);
            float contentY = (verticalContentDir == ContentDirection.TopToBottom) ? 0.0f : -(contentSize_.y - height);
            Vector2 constrainOffset = exGeometryUtility.GetConstrainOffset ( new Rect( scrollOffset_.x, scrollOffset_.y, width, height ), 
                                                                             new Rect( contentX, contentY, contentSize_.x, contentSize_.y ) );
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

            exUIEvent uiEvent = new exUIEvent();
            uiEvent.bubbles = false;
            OnScrollFinished(uiEvent);
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

        scrollOffset_ += _delta;

        if ( dragEffect != DragEffect.MomentumAndSpring ) {
            scrollOffset_.x = Mathf.Clamp( scrollOffset_.x, 0.0f, contentSize_.x - width );
            scrollOffset_.y = Mathf.Clamp( scrollOffset_.y, 0.0f, contentSize_.y - height );
        }

        if ( contentAnchor != null ) {
            scrollDest = new Vector3 ( originalAnchorPos.x - scrollOffset_.x,
                                 originalAnchorPos.y + scrollOffset_.y,
                                 originalAnchorPos.z );
            // contentAnchor.localPosition = scrollDest;
        }

        exUIEvent uiEvent = new exUIEvent();
        uiEvent.bubbles = false;
        OnScroll(uiEvent);
    }
}
