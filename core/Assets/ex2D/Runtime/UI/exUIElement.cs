// ======================================================================================
// File         : exUIElement.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 22:52:19 PM | Friday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
///
/// The ui-layout element
///
///////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public class exUIElement {
    public string name = "New Element";
    public string id = "el"; // for css
    public string content = "";
    public exUIStyle style;

    public List<exUIElement> children = new List<exUIElement>();

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    public exUIElement parent_ = null;
    public exUIElement parent {
        set {
            if ( !ReferenceEquals(parent_, value) ) {
                if ( !ReferenceEquals(parent_, null) ) {
                    parent_.children.Remove(this);
                    parent_ = null;
                }

                if ( !ReferenceEquals(value, null) ) {
                    parent_ = value;
                    if ( parent_.children.IndexOf (this) == -1 ) {
                        parent_.children.Add(this);
                    }
                }
            }
        }
        get {
            return parent_;
        }
    }

    [System.NonSerialized] public int x;
    [System.NonSerialized] public int y;
    [System.NonSerialized] public int width;
    [System.NonSerialized] public int height;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddElement ( exUIElement _el ) {
        _el.parent = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RemoveElement ( exUIElement _el ) {
        if ( children.IndexOf(_el) != -1 ) {
            _el.parent = null;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetElementIndex ( exUIElement _el ) {
        return children.IndexOf(_el);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void InsertAt ( int _idx, exUIElement _el ) {
        int idx = _idx;
        if ( _el.parent == this ) {
            int curIdx = this.children.IndexOf(_el);
            exDebug.Assert( curIdx != -1, 
                            "Can't find the element in the parent children list.", 
                            true );
            if ( curIdx == _idx )
                return;

            if ( curIdx > _idx ) 
                idx = curIdx - 1;
        }

        _el.parent = null;
        this.children.Insert( idx, _el );
        _el.parent_ = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool Exists ( exUIElement _el ) {
        if ( this != _el ) {
            for ( int i = 0; i < this.children.Count; ++i ) {
                exUIElement child = this.children[i];
                if ( child.Exists (_el) )
                    return true;
            }

            return false;
        }
        return true;
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Layout ( int _x, int _y, int _width, int _height ) {
        float fWidth = 0.0f;
        float fHeight = 0.0f;
        x = _x;
        y = _y;

        // ======================================================== 
        // property relate with content-width 
        // ======================================================== 

        // calculate width
        if ( style.width.type == exCSS_size.Type.Length ) {
            fWidth = style.width.val;
        }
        else if ( style.width.type == exCSS_size.Type.Percentage ) {
            fWidth = style.width.val/100.0f * (float)_width;
        }
        else if ( style.width.type == exCSS_size.Type.Auto ) {
            if ( style.marginLeft.type == exCSS_size.Type.Auto ) style.marginLeft.val = 0.0f;
            if ( style.marginRight.type == exCSS_size.Type.Auto ) style.marginRight.val = 0.0f;

            fWidth = _width - style.GetMarginLeft(_width) - style.GetMarginRight(_width); 
            fWidth = Mathf.Max ( fWidth, 0.0f );
        }

        if ( style.minWidth.type == exCSS_min_size.Type.Length )
            fWidth = Mathf.Max ( fWidth, style.minWidth.val );
        if ( style.maxWidth.type == exCSS_max_size.Type.Length )
            fWidth = Mathf.Min ( fWidth, style.maxWidth.val );
        width = Mathf.FloorToInt(fWidth);
        int contentWidth = width 
            - style.GetPaddingLeft(_width) - style.GetPaddingRight(_width)
            - (int)style.borderSizeLeft.val - (int)style.borderSizeRight.val;

        // ======================================================== 
        // property relate with content-height 
        // ======================================================== 

        // calculate height
        if ( style.height.type == exCSS_size.Type.Length ) {
            fHeight = style.height.val;
        }
        else if ( style.height.type == exCSS_size.Type.Percentage ) {
            fHeight = style.height.val/100.0f * (float)_height;
        }
        else if ( style.height.type == exCSS_size.Type.Auto ) {
            if ( style.marginTop.type == exCSS_size.Type.Auto ) style.marginTop.val = 0.0f;
            if ( style.marginBottom.type == exCSS_size.Type.Auto ) style.marginBottom.val = 0.0f;

            fHeight = _height - style.GetMarginTop(_height) - style.GetMarginBottom(_height); 
            fHeight = Mathf.Max ( fHeight, 0.0f );
        }

        if ( style.minHeight.type == exCSS_min_size.Type.Length )
            fHeight = Mathf.Max ( fHeight, style.minHeight.val );
        if ( style.maxHeight.type == exCSS_max_size.Type.Length )
            fHeight = Mathf.Min ( fHeight, style.maxHeight.val );
        height = Mathf.FloorToInt(fHeight);
        int contentHeight = height 
            - style.GetPaddingTop(_height) - style.GetPaddingBottom(_height)
            - (int)style.borderSizeTop.val - (int)style.borderSizeBottom.val;

        // layout the children
        int child_x = x + style.GetMarginLeft(_width) + style.GetPaddingLeft(_width) + (int)style.borderSizeLeft.val;
        int child_y = y + style.GetMarginTop(_height) + style.GetPaddingTop(_height) + (int)style.borderSizeTop.val;
        for ( int i = 0; i < children.Count; ++i ) {
            exUIElement child = children[i];
            child.Layout( child_x, child_y, contentWidth, contentHeight );

            // TODO: offset child
        }
    }
}


