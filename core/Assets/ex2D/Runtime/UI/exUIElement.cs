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
public class exUIInlineElement {
    // TODO:
}

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

    public List<exUIElement> computedElements { get { return computedElements_; } } 

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
    [System.NonSerialized] public int x = 0;
    [System.NonSerialized] public int y = 0;
    [System.NonSerialized] public int width = 0;
    [System.NonSerialized] public int height = 0;

    [System.NonSerialized] public int marginLeft = 0;
    [System.NonSerialized] public int marginRight = 0;
    [System.NonSerialized] public int marginTop = 0;
    [System.NonSerialized] public int marginBottom = 0;

    [System.NonSerialized] public int borderSizeLeft = 0;
    [System.NonSerialized] public int borderSizeRight = 0;
    [System.NonSerialized] public int borderSizeTop = 0;
    [System.NonSerialized] public int borderSizeBottom = 0;

    [System.NonSerialized] public int paddingLeft = 0;
    [System.NonSerialized] public int paddingRight = 0;
    [System.NonSerialized] public int paddingTop = 0;
    [System.NonSerialized] public int paddingBottom = 0;

    [System.NonSerialized] public Object borderImage = null;
    [System.NonSerialized] public Color borderColor = new Color( 0.0f, 0.0f, 0.0f, 0.0f );

    [System.NonSerialized] public Object backgroundImage = null;
    [System.NonSerialized] public Color backgroundColor = new Color( 0.0f, 0.0f, 0.0f, 0.0f );

    [System.NonSerialized] public Object font = null;
    [System.NonSerialized] public int fontSize = 0;
    [System.NonSerialized] public Color textColor = new Color( 0.0f, 0.0f, 0.0f, 0.0f );
    [System.NonSerialized] public exCSS_white_space whitespace = exCSS_white_space.NoWrap;
    [System.NonSerialized] public int letterSpacing = 0;
    [System.NonSerialized] public int wordSpacing = 0;
    [System.NonSerialized] public int lineHeight = 0;
    [System.NonSerialized] public exCSS_display display = exCSS_display.Inline;

    bool dirty = false;
    List<exUIElement> computedElements_ = new List<exUIElement>(); 

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

        if ( style.display == exCSS_display.Block ) {
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

            if ( style.marginRight.type != exCSS_size.Type.Auto ) {
                int remainWidth = _width - width - borderSizeLeft - borderSizeRight - paddingLeft - paddingRight;
                remainWidth -= marginLeft; 
                remainWidth = System.Math.Max ( remainWidth, 0 );
                marginRight = remainWidth;
            }
        }

        if ( style.minWidth.type == exCSS_min_size.Type.Length )
            width = System.Math.Max ( width, (int)style.minWidth.val );
        if ( style.maxWidth.type == exCSS_max_size.Type.Length )
            width = System.Math.Min ( width, (int)style.maxWidth.val );

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
        // layout content elemtns 
        // ======================================================== 

        computedElements_.Clear();
        int offset_x = (parent != null) ? (_x - parent.x) : 0;
        BreakTextIntoElements ( content, offset_x, width );
        computedElements_.AddRange( children );

        // ======================================================== 
        // layout the children
        // ======================================================== 

        int child_start_x = x;
        int child_start_y = y;
        int child_x = child_start_x;
        int child_y = child_start_y;
        int maxWidth = 0;
        int maxHeight = 0;
        int lineChildIndex = 0;
        int lineChildCount = 0;

        for ( int i = 0; i < computedElements_.Count; ++i ) {
            exUIElement child = computedElements_[i];
            bool newLine = false;
            ++lineChildCount;

            // do layout if they are not content-line-elements
            if ( child.style != null )
                child.Layout( child_x, child_y, width, height );

            // calculate max-height for the prev element
            int childTotalHeight = child.GetTotalHeight();
            if ( childTotalHeight > maxHeight ) {
                maxHeight = childTotalHeight;
            }

            // advance the child 
            if ( child.display == exCSS_display.Block ) {
                newLine = true;
            }
            else if ( child.display == exCSS_display.InlineBlock ) 
            {
                int childTotalWidth = child.GetTotalWidth();
                if ( childTotalWidth > _width ) {
                    newLine = true;
                }
                else {
                    child_x += childTotalWidth;
                }
            }
            else if ( child.display == exCSS_display.Inline ) {
                int advance_x;
                int advance_y;
                bool startFromZero;
                child.GetAdvance ( out advance_x, out advance_y, out startFromZero );
                if ( startFromZero ) {
                    child_x = advance_x;
                }
                else {
                    child_x += advance_x;
                }
                child_y += advance_y;
            }

            //
            if ( newLine ) {
                // TODO: re-layout last-line elements ( vertical aligement, based on max-height )

                //
                int lineWidth = child_x - child_start_x;
                if ( lineWidth > maxWidth )
                    maxWidth = lineWidth;

                //
                child_x = child_start_x;
                child_y = child_y + maxHeight;
                maxHeight = 0;
                lineChildIndex = i+1;
                lineChildCount = 0;
            }
        }

        // calculate the height after child
        if ( style.height.type == exCSS_size.Type.Auto ) {
            height += child_y - child_start_y;  
        }
        if ( style.width.type == exCSS_size.Type.Auto ) {
            if ( display == exCSS_display.InlineBlock ||
                 display == exCSS_display.Inline ) {
                width = maxWidth;
            }
        }

        // TODO: I think only parent set dirty
        dirty = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: This is for inline element 
    // ------------------------------------------------------------------ 

    void GetAdvance ( out int _advance_x, out int _advance_y, out bool _startFromZero ) {
        _advance_x = 0;
        _advance_y = 0;
        _startFromZero = false;

        if ( computedElements_.Count > 0 ) {
            exUIElement lastChild = computedElements_[computedElements_.Count-1];
            _advance_x = lastChild.width;
            for ( int i = 0; i < computedElements_.Count; ++i ) {
                _advance_y += computedElements_[i].height;
            }
        }
        else {
            _advance_x = width;
            _advance_y = height;
        }

        if ( computedElements_.Count > 1 )
            _startFromZero = true;
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void BreakTextIntoElements ( string _text, int _offset_x, int _width ) {
        exTextUtility.WrapMode wrapMode = exTextUtility.WrapMode.None;

        switch ( whitespace ) {
        case exCSS_white_space.Normal:  wrapMode = exTextUtility.WrapMode.Word;     break;
        case exCSS_white_space.Pre:     wrapMode = exTextUtility.WrapMode.Pre;      break;
        case exCSS_white_space.NoWrap:  wrapMode = exTextUtility.WrapMode.None;     break;
        case exCSS_white_space.PreWrap: wrapMode = exTextUtility.WrapMode.PreWrap;  break;
        }

        if ( font is Font ) {
            (font as Font).RequestCharactersInTexture ( _text, fontSize, FontStyle.Normal );

            int last_line_width = 0;
            bool finished = false;
            int cur_x = _offset_x;
            int cur_y = 0;
            int cur_index = 0;
            int cur_width = _width - _offset_x;
            bool firstLineCheck = (_offset_x > 0 && display == exCSS_display.InlineBlock);

            while ( finished == false ) {
                int start_index = cur_index;

                //
                finished = exTextUtility.CalcTextLine ( ref last_line_width, 
                                                        ref cur_index,
                                                        _text,
                                                        cur_index,
                                                        cur_width,
                                                        font as Font,
                                                        fontSize,
                                                        wrapMode,
                                                        wordSpacing,
                                                        letterSpacing );
                // generate element
                exUIElement newEL = new exUIElement();
                newEL.style = null;
                newEL.display = exCSS_display.Inline; 
                newEL.font = font;
                newEL.fontSize = fontSize;
                newEL.textColor = textColor;
                newEL.whitespace = whitespace; 
                newEL.letterSpacing = letterSpacing;
                newEL.wordSpacing = wordSpacing;
                newEL.lineHeight = lineHeight;
                newEL.width = cur_width;
                newEL.height = lineHeight;
                newEL.x = this.x + cur_x;
                newEL.y = this.y + cur_y;
                string lineContent = this.content.Substring( start_index, cur_index - start_index ); 
                if ( whitespace != exCSS_white_space.Pre ) {
                    lineContent = lineContent.Trim();
                }
                newEL.content = lineContent;
                computedElements_.Add(newEL);

                // if inline-block's first line exceed, it will start from next line 
                if ( firstLineCheck ) {
                    if ( last_line_width > cur_width ) {
                        cur_x = 0;
                        cur_width = _width;
                        continue;
                    }
                    firstLineCheck = false;
                }

                //
                cur_x = 0;
                cur_y += lineHeight;
                cur_width = _width;
                ++cur_index;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void CalcTextSize ( out int _last_line_width,
                        out int _size_x,
                        out int _size_y,
                        string _text, 
                        int _offset_x,
                        int _width ) 
    {
        _last_line_width = 0; 
        int lines = 0;
        exTextUtility.WrapMode wrapMode = exTextUtility.WrapMode.None;

        switch ( whitespace ) {
        case exCSS_white_space.Normal:  wrapMode = exTextUtility.WrapMode.Word;     break;
        case exCSS_white_space.Pre:     wrapMode = exTextUtility.WrapMode.Pre;      break;
        case exCSS_white_space.NoWrap:  wrapMode = exTextUtility.WrapMode.None;     break;
        case exCSS_white_space.PreWrap: wrapMode = exTextUtility.WrapMode.PreWrap;  break;
        }

        if ( font is Font ) {
            exTextUtility.CalcTextSize ( ref _last_line_width,
                                         ref lines,
                                         _text,
                                         _offset_x,
                                         _width,
                                         font as Font,
                                         fontSize,
                                         wrapMode,
                                         wordSpacing,
                                         letterSpacing );
        }

        if ( lines > 1 )
            _size_x = _width;
        else 
            _size_x = _last_line_width; 
        _size_y = lines * lineHeight;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void DrawText ( int _offset_x, int _width, string _text ) {
        int last_line_width = 0; 
        int lines = 0;
        exTextUtility.WrapMode wrapMode = exTextUtility.WrapMode.None;

        switch ( whitespace ) {
        case exCSS_white_space.Normal:  wrapMode = exTextUtility.WrapMode.Word;     break;
        case exCSS_white_space.Pre:     wrapMode = exTextUtility.WrapMode.Pre;      break;
        case exCSS_white_space.NoWrap:  wrapMode = exTextUtility.WrapMode.None;     break;
        case exCSS_white_space.PreWrap: wrapMode = exTextUtility.WrapMode.PreWrap;  break;
        }

        if ( font is Font ) {
            exTextUtility.BuildTextMesh ( ref last_line_width,
                                          ref lines,
                                          _text,
                                          _offset_x,
                                          _width,
                                          font as Font,
                                          fontSize,
                                          wrapMode,
                                          wordSpacing,
                                          letterSpacing );
        }
    }
}


// DELME { 
// CalcTextSize
// bool wrap = false; // TODO
// Vector2 size = Vector2.zero;
// GUIStyle fontHelper = exTextUtility.fontHelper;

// if ( font is Font ) {
//     fontHelper.font = font as Font;
//     fontHelper.fontSize = fontSize;
//     fontHelper.fontStyle = FontStyle.Normal; 
//     fontHelper.wordWrap = wrap;
//     fontHelper.richText = false;
//     fontHelper.normal.textColor = textColor;

//     GUIContent uiContent = new GUIContent(_text);

//     if ( wrap == false ) {
//         size = fontHelper.CalcSize (uiContent);
//     }
//     else {
//         size.x = _width;
//         size.y = fontHelper.CalcHeight( uiContent, _width );
//     }
// }

// return size;
// } DELME end 

// DELME { 
// DrawText
// bool wrap = false;
// switch ( whitespace ) {
// case exCSS_white_space.Normal:  wrap = true;  break;
// case exCSS_white_space.Pre:     wrap = true;  break;
// case exCSS_white_space.NoWrap:  wrap = false; break;
// case exCSS_white_space.PreWrap: wrap = true;  break;
// }

// GUIStyle fontHelper = exTextUtility.fontHelper;
// if ( font is Font ) {
//     fontHelper.font = font as Font;
//     fontHelper.fontSize = fontSize;
//     fontHelper.fontStyle = FontStyle.Normal; 
//     fontHelper.wordWrap = wrap;
//     fontHelper.richText = false;
//     fontHelper.normal.textColor = textColor;
//     fontHelper.Draw ( _rect,
//                       new GUIContent(_content),
//                       false,
//                       false, 
//                       true,
//                       false );
// }
// } DELME end 
