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

    // event-defs
    public static new string[] eventNames = new string[] {
        "onProgressChanged",
    };

    // events
    List<exUIEventListener> onProgressChanged;

    public void OnProgressChanged ( exUIEvent _event )  { exUIMng.inst.DispatchEvent( this, "onProgressChanged", onProgressChanged, _event ); }

    public override void CacheEventListeners () {
        base.CacheEventListeners();

        onProgressChanged = eventListenerTable["onProgressChanged"];
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
    [SerializeField] protected float progress_ = 0.0f;
    public float progress {
        get { return progress_; }
        set {
            if ( progress_ != value ) {
                progress_ = Mathf.Clamp( value, 0.0f, 1.0f );

                UpdateBar ();

                exUIRatioEvent ratioEvent = new exUIRatioEvent();
                ratioEvent.bubbles = false;
                ratioEvent.ratio = progress_;
                OnProgressChanged (ratioEvent);
            }
        }
    }

    //
    [SerializeField] protected float barSize_ = 0.0f;
    public float barSize {
        get { return barSize_; }
        set {
            if ( barSize_ != value ) {
                barSize_ = value;
                UpdateBar ();
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
                                    float _barSize, 
                                    float _progress, 
                                    Direction _direction ) 
    {
        if ( _bar != null ) {
            if ( _direction == Direction.Horizontal ) {
                if ( _bar.spriteType == exSpriteType.Sliced ) {
                    float progressWidth = _progress * (_barSize-_bar.leftBorderSize-_bar.rightBorderSize);
                    _bar.width = progressWidth + _bar.leftBorderSize + _bar.rightBorderSize;
                }
                else {
                    _bar.width = _progress * _barSize;
                }
            }
            else {
                if ( _bar.spriteType == exSpriteType.Sliced ) {
                    float progressHeight = _progress * (_barSize-_bar.topBorderSize-_bar.bottomBorderSize);
                    _bar.height = progressHeight + _bar.topBorderSize + _bar.bottomBorderSize;
                }
                else {
                    _bar.height = _progress * _barSize;
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
        }

        UpdateBar ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateBar () {
        SetBarSize ( bar, barSize_, progress_, direction );
    }
}
