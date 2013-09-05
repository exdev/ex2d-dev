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

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    public string name = "New Element";
    public string id = "el"; // for css
    public string content = "";
    public exUIStyle style = new exUIStyle();

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

    // computed style
    [System.NonSerialized] public int x;
    [System.NonSerialized] public int y;
    [System.NonSerialized] public int width;
    [System.NonSerialized] public int height;

    [System.NonSerialized] public int marginLeft;
    [System.NonSerialized] public int marginRight;
    [System.NonSerialized] public int marginTop;
    [System.NonSerialized] public int marginBottom;

    [System.NonSerialized] public int borderSizeLeft;
    [System.NonSerialized] public int borderSizeRight;
    [System.NonSerialized] public int borderSizeTop;
    [System.NonSerialized] public int borderSizeBottom;

    [System.NonSerialized] public int paddingLeft;
    [System.NonSerialized] public int paddingRight;
    [System.NonSerialized] public int paddingTop;
    [System.NonSerialized] public int paddingBottom;

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

    public int GetTotalHeight () {
        return height 
            + marginTop + marginBottom 
            + borderSizeTop + borderSizeBottom
            + paddingTop + paddingBottom;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetTotalWidth () {
        return height 
            + marginLeft + marginRight 
            + borderSizeLeft + borderSizeRight
            + paddingLeft + paddingRight;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Layout ( int _x, int _y, int _width, int _height ) {
        x = _x;
        y = _y;

        marginLeft = style.GetMarginLeft(_width);
        marginRight = style.GetMarginRight(_width);
        marginTop = style.GetMarginTop(_height);
        marginBottom = style.GetMarginBottom(_height);

        borderSizeLeft = (int)style.borderSizeLeft.val;
        borderSizeRight = (int)style.borderSizeRight.val;
        borderSizeTop = (int)style.borderSizeTop.val;
        borderSizeBottom = (int)style.borderSizeBottom.val;

        paddingLeft = style.GetPaddingLeft(_width);
        paddingRight = style.GetPaddingRight(_width);
        paddingTop = style.GetPaddingTop(_height);
        paddingBottom = style.GetPaddingBottom(_height);

        // ======================================================== 
        // property relate with content-width 
        // http://www.w3.org/TR/CSS2/visudet.html#Computing_widths_and_margins
        // ======================================================== 

        // calculate width
        if ( style.width.type == exCSS_size.Type.Length ) {
            width = (int)style.width.val;
        }
        else if ( style.width.type == exCSS_size.Type.Percentage ) {
            width = Mathf.FloorToInt ( style.width.val/100.0f * (float)_width );
        }
        else if ( style.width.type == exCSS_size.Type.Auto ) {
            width = _width 
                   - marginLeft - marginRight
                   - borderSizeLeft - borderSizeRight
                   - paddingLeft - paddingRight;
            width = System.Math.Max ( width, 0 );
        }

        if ( style.width.type != exCSS_size.Type.Auto &&
             ( style.marginLeft.type == exCSS_size.Type.Auto || style.marginRight.type == exCSS_size.Type.Auto ) ) 
        {
            int remainWidth = _width - width - borderSizeLeft - borderSizeRight - paddingLeft - paddingRight;
            if ( style.marginLeft.type == exCSS_size.Type.Auto && style.marginRight.type == exCSS_size.Type.Auto ) {
                remainWidth = System.Math.Max ( remainWidth, 0 );
                marginLeft = remainWidth / 2;
                marginRight = remainWidth / 2;
            }
            else if ( style.marginLeft.type == exCSS_size.Type.Auto ) {
                remainWidth -= marginRight; 
                remainWidth = System.Math.Max ( remainWidth, 0 );
                marginLeft = remainWidth;
            }
            else if ( style.marginRight.type == exCSS_size.Type.Auto ) {
                remainWidth -= marginLeft; 
                remainWidth = System.Math.Max ( remainWidth, 0 );
                marginRight = remainWidth;
            }
        }

        if ( style.display == exCSS_display.Block && style.marginRight.type != exCSS_size.Type.Auto ) {
            int remainWidth = _width - width - borderSizeLeft - borderSizeRight - paddingLeft - paddingRight;
            remainWidth -= marginLeft; 
            remainWidth = System.Math.Max ( remainWidth, 0 );
            marginRight = remainWidth;
        }

        if ( style.minWidth.type == exCSS_min_size.Type.Length )
            width = System.Math.Max ( width, (int)style.minWidth.val );
        if ( style.maxWidth.type == exCSS_max_size.Type.Length )
            width = System.Math.Min ( width, (int)style.maxWidth.val );

        // check if wrap the block
        if ( style.width.type == exCSS_size.Type.Auto ) {
            // TODO: calculate the result in no-wrap
            // Vector2 nowrapSize = style.CalcTextSize ( text, width );
            // width = nowrapSize.x;
        }

        // calculate content size after width calculated
        string text = content.Replace ( "\n", " " );
        text.Trim();
        Vector2 contentTextSize = Vector2.zero;
        if ( string.IsNullOrEmpty(text) == false ) {
            contentTextSize = style.CalcTextSize ( text, width );
        }

        // ======================================================== 
        // property relate with content-height 
        // http://www.w3.org/TR/CSS2/visudet.html#Computing_heights_and_margins
        // ======================================================== 

        // calculate height
        if ( style.height.type == exCSS_size.Type.Length ) {
            height = (int)style.height.val;
        }
        else if ( style.height.type == exCSS_size.Type.Percentage ) {
            height = Mathf.FloorToInt ( style.height.val/100.0f * (float)_height );
        }
        else if ( style.height.type == exCSS_size.Type.Auto ) {
            height = 0;
            height += (int)contentTextSize.y;
        }

        if ( style.minHeight.type == exCSS_min_size.Type.Length )
            height = System.Math.Max ( height, (int)style.minHeight.val );
        if ( style.maxHeight.type == exCSS_max_size.Type.Length )
            height = System.Math.Min ( height, (int)style.maxHeight.val );

        // ======================================================== 
        // calculate position
        // ======================================================== 

        x = x + marginLeft + borderSizeLeft + paddingLeft;
        y = y + marginTop + borderSizeTop + paddingTop;

        // ======================================================== 
        // layout the children
        // ======================================================== 

        int child_start_x = x;
        int child_start_y = y + (int)contentTextSize.y;
        int child_x = child_start_x;
        int child_y = child_start_y;

        for ( int i = 0; i < children.Count; ++i ) {
            exUIElement child = children[i];
            child.Layout( child_x, child_y, width, height );

            if ( child.style.display == exCSS_display.Block ) {
                child_x = child_start_x;
                child_y = child_y + child.GetTotalHeight();
            }
            else if ( child.style.display == exCSS_display.InlineBlock ) {
                // TODO: offset child
            }

            // TODO: We assume inline element not have child, this will save us a lot of work! 
        }

        // calculate the height after child
        if ( style.height.type == exCSS_size.Type.Auto ) {
            height += child_y - child_start_y;  
        }
    }
}


