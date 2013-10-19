// ======================================================================================
// File         : exUIScrollBar.cs
// Author       : Wu Jie 
// Last Change  : 10/19/2013 | 17:09:10 PM | Saturday,October
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

public class exUIScrollBar : exUIControl {

	public enum Direction {
		Vertical,
		Horizontal,
	};

	public Direction direction = Direction.Vertical;
    public exUIScrollView scrollView = null;

    protected exUIButton btnBar = null;
    protected exSprite background = null;

    protected float contentRatio = 1.0f;
    protected float offsetRatio = 1.0f;
    protected float offset = 0.0f;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        btnBar.grabMouseOrTouch = true;
        btnBar.onHoverMove += delegate ( exUIControl _sender, List<exHotPoint> _points ) {
            // TODO:
        }

        btnBar.transform.localPosition = Vector3.zero;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetContentRatio ( float _contentRatio ) {
    }
}
