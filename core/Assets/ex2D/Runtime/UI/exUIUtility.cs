// ======================================================================================
// File         : exUIUtility.cs
// Author       : Wu Jie 
// Last Change  : 01/09/2014 | 14:42:36 PM | Thursday,January
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
//
/// The sprite utilities
//
///////////////////////////////////////////////////////////////////////////////

public static class exUIUtility {

    public static Vector3 GetOffset ( this exPlane _plane ) {
        Vector2 anchorOffset = Vector2.zero;
        Vector2 size = new Vector2( _plane.width, _plane.height );

        switch (_plane.anchor) {
        case Anchor.TopLeft:    anchorOffset = new Vector3( -size.x*0.5f,  size.y*0.5f, 0.0f ); break;
        case Anchor.TopCenter:  anchorOffset = new Vector3(         0.0f,  size.y*0.5f, 0.0f ); break;
        case Anchor.TopRight:   anchorOffset = new Vector3(  size.x*0.5f,  size.y*0.5f, 0.0f ); break;
        case Anchor.MidLeft:    anchorOffset = new Vector3( -size.x*0.5f,         0.0f, 0.0f ); break;
        case Anchor.MidCenter:  anchorOffset = new Vector3(         0.0f,         0.0f, 0.0f ); break;
        case Anchor.MidRight:   anchorOffset = new Vector3(  size.x*0.5f,         0.0f, 0.0f ); break;
        case Anchor.BotLeft:    anchorOffset = new Vector3( -size.x*0.5f, -size.y*0.5f, 0.0f ); break;
        case Anchor.BotCenter:  anchorOffset = new Vector3(         0.0f, -size.y*0.5f, 0.0f ); break;
        case Anchor.BotRight:   anchorOffset = new Vector3(  size.x*0.5f, -size.y*0.5f, 0.0f ); break;
        }

        Vector3 scaledOffset = _plane.offset + anchorOffset;
        Vector3 lossyScale = _plane.transform.lossyScale;
        scaledOffset.x *= lossyScale.x;
        scaledOffset.y *= lossyScale.y;

        return anchorOffset;
    }
}
