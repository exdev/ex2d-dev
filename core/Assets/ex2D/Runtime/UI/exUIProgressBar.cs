// ======================================================================================
// File         : exUIProgressBar.cs
// Author       : Wu Jie 
// Last Change  : 10/26/2013 | 15:59:46 PM | Saturday,October
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

public class exUIProgressBar : exUIControl {

	public enum Direction {
		Vertical,
		Horizontal,
	};

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
        new EventDef ( "onProgressChanged",      new Type[] { typeof(exUIControl), typeof(float) }, typeof(Action<exUIControl,float>) ),
    };

    // events
    public event System.Action<exUIControl,float> onProgressChanged;

    //
    [SerializeField] protected float progress_ = 0.0f;
    public float progress {
        get { return progress_; }
        set {
            if ( progress_ != value ) {
                progress_ = Mathf.Clamp( value, 0.0f, 1.0f );

                UpdateBar ();

                if ( onProgressChanged != null )
                    onProgressChanged ( this, progress_ );
            }
        }
    }

	public Direction direction = Direction.Horizontal;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    protected exSprite bar = null;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    public static void SetBarSize ( exSprite _bar, 
                                    exUIControl _ctrl, 
                                    float _progress, 
                                    Direction _direction ) 
    {
        if ( _bar != null ) {
            exSprite background = _ctrl.GetComponent<exSprite>();

            if ( _direction == Direction.Horizontal ) {
                float progressWidth = _progress * _ctrl.width;
                if ( background != null && background.spriteType == exSpriteType.Sliced ) {
                    progressWidth = _progress * (_ctrl.width - background.leftBorderSize - background.rightBorderSize);
                }


                if ( _bar.spriteType == exSpriteType.Sliced ) {
                    _bar.width = progressWidth + _bar.leftBorderSize + _bar.rightBorderSize;
                }
                else {
                    _bar.width = progressWidth;
                }
            }
            else {
                float progressHeight = _progress * _ctrl.height;

                if ( background != null && background.spriteType == exSpriteType.Sliced ) {
                    progressHeight = _progress * (_ctrl.height - background.topBorderSize - background.bottomBorderSize);
                }


                if ( _bar.spriteType == exSpriteType.Sliced ) {
                    _bar.height = progressHeight + _bar.topBorderSize + _bar.bottomBorderSize;
                }
                else {
                    _bar.height = progressHeight;
                }
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();

        // handle scroll bar
        Transform transBar = transform.Find("__bar");
        if ( transBar ) {
            bar = transBar.GetComponent<exSprite>();
            if ( bar ) {
                bar.customSize = true;
                bar.anchor = Anchor.TopLeft;
            }
        }

        UpdateBar ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateBar () {
        SetBarSize ( bar, this, progress_, direction );
    }
}
