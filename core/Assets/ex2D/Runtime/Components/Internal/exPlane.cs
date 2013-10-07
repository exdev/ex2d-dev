// ======================================================================================
// File         : exPlane.cs
// Author       : 
// Last Change  : 10/07/2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ------------------------------------------------------------------ 
/// The anchor position of the plane
// ------------------------------------------------------------------ 

public enum Anchor {
    TopLeft = 0, ///< the top-left of the sprite  
    TopCenter,   ///< the top-center of the sprite
    TopRight,    ///< the top-right of the sprite
    MidLeft,     ///< the middle-left of the sprite
    MidCenter,   ///< the middle-center of the sprite
    MidRight,    ///< the middle-right of the sprite
    BotLeft,     ///< the bottom-left of the sprite
    BotCenter,   ///< the bottom-center of the sprite
    BotRight,    ///< the bottom-right of the sprite
}

///////////////////////////////////////////////////////////////////////////////
///
///
///
///////////////////////////////////////////////////////////////////////////////

[ExecuteInEditMode]
public abstract class exPlane : MonoBehaviour {

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected float width_ = 1.0f;
    /// the width of the plane
    // ------------------------------------------------------------------ 

    public virtual float width {
        get { return width_; }
        set { width_ = value; }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float height_ = 1.0f;
    /// the height of the plane
    // ------------------------------------------------------------------ 

    public virtual float height {
        get { return height_; }
        set { height_ = value; }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Anchor anchor_ = Anchor.MidCenter;
    /// the anchor position used in this plane
    // ------------------------------------------------------------------ 

    public virtual Anchor anchor {
        get { return anchor_; }
        set { anchor_ = value; }
    }

}
