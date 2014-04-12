// ======================================================================================
// File         : exSpriteFontOverlap.cs
// Author       : Wu Jie 
// Last Change  : 11/14/2013 | 14:56:27 PM | Thursday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

public class exSpriteFontOverlap : MonoBehaviour {

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    public exSpriteFont overlap;
    public exSpriteFont original;

    [SerializeField] protected string text_;
    public string text {
        set {
            if ( text_ != value ) {
                text_ = value;
                if ( original != null ) original.text = text_;
                if ( overlap != null ) overlap.text = text_;
            }
        }
        get {
            return text_;
        }
    } 

    [SerializeField] protected float depth_;
    public float depth {
        set {
            if ( depth_ != value ) {
                depth_ = value;
                if ( original != null ) original.depth = depth_;
                if ( overlap != null ) overlap.depth = depth_ + 0.1f;
            }
        }
        get {
            return depth_;
        }
    } 
}

