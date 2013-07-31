// ======================================================================================
// File         : exGeometryUtility.cs
// Author       : Wu Jie 
// Last Change  : 06/22/2013 | 00:49:39 AM | Saturday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;

///////////////////////////////////////////////////////////////////////////////
/// 
/// the geometry helper class
/// 
///////////////////////////////////////////////////////////////////////////////

public static class exGeometryUtility {

    // ------------------------------------------------------------------ 
    /// \param _a rect a
    /// \param _b rect b
    /// \result the contains result
    /// check if rect contains, 1 is _a contains _b, -1 is _b contains _a, 0 is no contains 
    // ------------------------------------------------------------------ 

    public static int RectRect_Contains ( Rect _a, Rect _b ) {
        if ( _a.xMin <= _b.xMin &&
             _a.xMax >= _b.xMax &&
             _a.yMin <= _b.yMin &&
             _a.yMax >= _b.yMax )
        {
            // a contains b
            return 1;
        }
        if ( _b.xMin <= _a.xMin &&
             _b.xMax >= _a.xMax &&
             _b.yMin <= _a.yMin &&
             _b.yMax >= _a.yMax )
        {
            // b contains a
            return -1;
        }
        return 0;
    }

    // ------------------------------------------------------------------ 
    /// \param _a rect a
    /// \param _b rect b
    /// \result the intersect result
    /// check if two rect intersection
    // ------------------------------------------------------------------ 

    public static bool RectRect_Intersect ( Rect _a, Rect _b ) {
        if ( (_a.xMin <= _b.xMin && _a.xMax >= _b.xMin) ||
             (_b.xMin <= _a.xMin && _b.xMax >= _a.xMin ) ) 
        {
            if ( (_a.yMin <= _b.yMin && _a.yMax >= _b.yMin) ||
                 (_b.yMin <= _a.yMin && _b.yMax >= _a.yMin ) ) 
            {
                return true;
            }
        }
        return false;
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static Rect Rect_FloorToInt ( Rect _rect ) {
        return new Rect ( Mathf.FloorToInt(_rect.x),
                          Mathf.FloorToInt(_rect.y),
                          Mathf.FloorToInt(_rect.width),
                          Mathf.FloorToInt(_rect.height) );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static Rect GetAABoundingRect ( Vector3[] _vertices ) {
        Rect boundingRect = new Rect();
        boundingRect.x = _vertices[0].x;
        boundingRect.y = _vertices[0].y;
        for (int i = 1; i < _vertices.Length; ++i) {
            Vector3 vertex = _vertices[i];
            if (vertex.x < boundingRect.xMin) {
                boundingRect.xMin = vertex.x;
            }
            else if (vertex.x > boundingRect.xMax) {
                boundingRect.xMax = vertex.x;
            }
            if (vertex.y < boundingRect.yMin) {
                boundingRect.yMin = vertex.y;
            }
            else if (vertex.y > boundingRect.yMax) {
                boundingRect.yMax = vertex.y;
            }
        }
        return boundingRect;
    }
}
