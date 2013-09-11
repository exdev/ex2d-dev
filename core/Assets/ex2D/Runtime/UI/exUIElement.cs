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
                    dirty = true;
                }

                if ( !ReferenceEquals(value, null) ) {
                    parent_ = value;
                    if ( parent_.children.IndexOf (this) == -1 ) {
                        parent_.children.Add(this);
                        dirty = true;
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

    [System.NonSerialized] public Object font;
    [System.NonSerialized] public int fontSize;
    [System.NonSerialized] public Color textColor;
    [System.NonSerialized] public exCSS_white_space whitespace;
    [System.NonSerialized] public int letterSpacing;
    [System.NonSerialized] public int wordSpacing;
    [System.NonSerialized] public int lineHeight;

    bool dirty = false;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetDirty () {
        dirty = true;
    }

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

        // compute style
        style.Compute ( this, _x, _y, _width, _height );

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
            // Vector2 nowrapSize = CalcTextSize ( text, width );
            // width = nowrapSize.x;
        }

        // calculate content size after width calculated
        string text = content.Replace ( "\n", " " );
        text.Trim();
        Vector2 contentTextSize = Vector2.zero;
        if ( string.IsNullOrEmpty(text) == false ) {
            contentTextSize = CalcTextSize ( text, width );
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
        int maxHeight = 0;

        for ( int i = 0; i < children.Count; ++i ) {
            exUIElement child = children[i];
            child.Layout( child_x, child_y, width, height );

            // calculate max-height for the prev children
            int childTotalHeight = child.GetTotalHeight();
            if ( childTotalHeight > maxHeight ) {
                maxHeight = childTotalHeight;
                // TODO: re-calculate inline blocks
            }

            // advance the child 
            if ( child.style.display == exCSS_display.Block ) {
                child_x = child_start_x;
                child_y = child_y + maxHeight;
                maxHeight = 0;
            }
            else if ( child.style.display == exCSS_display.InlineBlock ) {
                // TODO: offset child
                // if the child width < content_width, next child will not start from child_start_x
                // else, next child will start from child_start_x
            }

            // TODO: We assume inline element not have child, this will save us a lot of work! 
        }

        // calculate the height after child
        if ( style.height.type == exCSS_size.Type.Auto ) {
            height += child_y - child_start_y;  
        }

        // TODO: I think only parent set dirty
        dirty = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Vector2 CalcTextSize ( string _content, int _width ) {
        bool wrap = false; // TODO
        Vector2 size = Vector2.zero;
        GUIStyle fontHelper = exTextUtility.fontHelper;

        if ( font is Font ) {
            fontHelper.font = font as Font;
            fontHelper.fontSize = fontSize;
            fontHelper.fontStyle = FontStyle.Normal; 
            fontHelper.wordWrap = wrap;
            fontHelper.richText = false;
            fontHelper.normal.textColor = textColor;

            GUIContent uiContent = new GUIContent(_content);

            if ( wrap == false ) {
                size = fontHelper.CalcSize (uiContent);
            }
            else {
                size.x = _width;
                size.y = fontHelper.CalcHeight( uiContent, _width );
            }
        }

        return size;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void DrawText ( Rect _rect, string _content ) {
        bool wrap = false; // TODO
        GUIStyle fontHelper = exTextUtility.fontHelper;

        if ( font is Font ) {
            fontHelper.font = font as Font;
            fontHelper.fontSize = fontSize;
            fontHelper.fontStyle = FontStyle.Normal; 
            fontHelper.wordWrap = wrap;
            fontHelper.richText = false;
            fontHelper.normal.textColor = textColor;

            GUIContent uiContent = new GUIContent(_content);
            fontHelper.Draw ( _rect,
                              uiContent,
                              false,
                              false, 
                              true,
                              false );
        }
    }
}


