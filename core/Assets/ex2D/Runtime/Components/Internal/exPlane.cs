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
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 offset_ = Vector2.zero;
    /// the offset based on the anchor, the final position of the plane equals to offset + anchor
    // ------------------------------------------------------------------ 

    public virtual Vector2 offset {
        get { return offset_; }
        set { offset_ = value; }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool hasSprite {
        get {
            exSpriteBase sprite = GetComponent<exSpriteBase>();
            if ( sprite != null && sprite != this )
                return true;
            return false;
        }
    }

    // ------------------------------------------------------------------ 
    /// Calculate the world AA bounding rect of the sprite
    // ------------------------------------------------------------------ 

    public Rect GetWorldAABoundingRect () {
        Vector3[] vertices = GetWorldVertices();
        return exGeometryUtility.GetAABoundingRect(vertices);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Rect GetLocalAABoundingRect () {
        Vector3[] vertices = GetLocalVertices();
        return exGeometryUtility.GetAABoundingRect(vertices);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected virtual Vector3[] GetVertices (Space _space) {
        Vector2 anchorOffset;
        float halfHeight = height_ * 0.5f;
        float halfWidth = width_ * 0.5f;

        switch ( anchor_ ) {
        case Anchor.TopLeft   : anchorOffset.x = halfWidth;   anchorOffset.y = -halfHeight;  break;
        case Anchor.TopCenter : anchorOffset.x = 0.0f;        anchorOffset.y = -halfHeight;  break;
        case Anchor.TopRight  : anchorOffset.x = -halfWidth;  anchorOffset.y = -halfHeight;  break;

        case Anchor.MidLeft   : anchorOffset.x = halfWidth;   anchorOffset.y = 0.0f;         break;
        case Anchor.MidCenter : anchorOffset.x = 0.0f;        anchorOffset.y = 0.0f;         break;
        case Anchor.MidRight  : anchorOffset.x = -halfWidth;  anchorOffset.y = 0.0f;         break;

        case Anchor.BotLeft   : anchorOffset.x = halfWidth;   anchorOffset.y = halfHeight;   break;
        case Anchor.BotCenter : anchorOffset.x = 0.0f;        anchorOffset.y = halfHeight;   break;
        case Anchor.BotRight  : anchorOffset.x = -halfWidth;  anchorOffset.y = halfHeight;   break;

        default               : anchorOffset.x = 0.0f;        anchorOffset.y = 0.0f;         break;
        }
        anchorOffset.x += offset_.x;
        anchorOffset.y += offset_.y;

        // 
        Vector3 v0 = new Vector3 (-halfWidth + anchorOffset.x, -halfHeight + anchorOffset.y, 0.0f);
        Vector3 v1 = new Vector3 (-halfWidth + anchorOffset.x, halfHeight + anchorOffset.y, 0.0f);
        Vector3 v2 = new Vector3 (halfWidth + anchorOffset.x, halfHeight + anchorOffset.y, 0.0f);
        Vector3 v3 = new Vector3 (halfWidth + anchorOffset.x, -halfHeight + anchorOffset.y, 0.0f);

        if (_space == Space.World) {
            Matrix4x4 worldMatrix = transform.localToWorldMatrix;
            v0 = worldMatrix.MultiplyPoint3x4(v0);
            v1 = worldMatrix.MultiplyPoint3x4(v1);
            v2 = worldMatrix.MultiplyPoint3x4(v2);
            v3 = worldMatrix.MultiplyPoint3x4(v3);
        }

        return new Vector3[4] {
            v0, v1, v2, v3
        };
    }

    // ------------------------------------------------------------------ 
    /// Get vertices of the sprite
    /// NOTE: This function returns an empty array If sprite is invisible
    // ------------------------------------------------------------------ 

    public virtual Vector3[] GetLocalVertices () {
        return GetVertices(Space.Self);
    }

    // ------------------------------------------------------------------ 
    /// Get vertices of the sprite
    /// NOTE: This function returns an empty array If sprite is invisible
    // ------------------------------------------------------------------ 

    public virtual Vector3[] GetWorldVertices () {
        return GetVertices(Space.World);
    }
}
