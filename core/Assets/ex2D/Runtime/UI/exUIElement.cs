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
using System.Text;

///////////////////////////////////////////////////////////////////////////////
///
/// The ui-layout element
///
///////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public class exUIElement {

    public enum ContentType {
        Text,
        Markdown,
        Texture2D,
        TextureInfo,
    };

    public class LineInfo {
        public string name = "";
        public int height = 0;
        public List<exUIElement> elements = new List<exUIElement>();

        public bool isBlock = false;
        public bool pushWidth = false;
        public bool pushHeight = false;

        public int width {
            get {
                int result = 0;
                for ( int i = 0; i < elements.Count; ++i ) {
                    result += elements[i].GetTotalWidth();
                }
                return result;
            }
        }
        public int count { get { return elements.Count; } }
        public void Add ( exUIElement _el ) {
            elements.Add(_el);
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    public string name = "New Element";
    public string id = "el"; // for css
    public exUIStyle style = new exUIStyle();
    public List<exUIElement> children = new List<exUIElement>();

    public ContentType contentType = ContentType.Text;
    public string text = "";
    public Object image = null;

    public bool isContent { get { return isContent_; } }
    public bool isFirstLine { get { return isFirstLine_; } }

    public List<exUIElement> normalFlows { get { return normalFlows_; } } 
    List<exUIElement> normalFlows_ = new List<exUIElement>(); 

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    public exUIElement parent_ = null;
    public exUIElement parent {
        // DISABLE { 
        // set {
        //     // your child or yourself can not become your parent
        //     if ( IsSelfOrAncestorOf (value) )
        //         return;

        //     if ( !ReferenceEquals(parent_, value) ) {
        //         if ( !ReferenceEquals(parent_, null) ) {
        //             parent_.children.Remove(this);
        //             parent_ = null;
        //             dirty = true;
        //         }

        //         if ( !ReferenceEquals(value, null) ) {
        //             parent_ = value;
        //             if ( parent_.children.IndexOf (this) == -1 ) {
        //                 parent_.children.Add(this);
        //                 dirty = true;
        //             }
        //         }
        //     }
        // }
        // } DISABLE end 
        get {
            return parent_;
        }
    }

    public exUIElement owner = null;

    // computed style
    [System.NonSerialized] public int x = 0;
    [System.NonSerialized] public int y = 0;
    [System.NonSerialized] public int width = 0;
    [System.NonSerialized] public int height = 0;

    [System.NonSerialized] public int minWidth = 0;
    [System.NonSerialized] public int maxWidth = 0;
    [System.NonSerialized] public int minHeight = 0;
    [System.NonSerialized] public int maxHeight = 0;

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

    [System.NonSerialized] public Color contentColor = new Color( 0.0f, 0.0f, 0.0f, 0.0f );
    [System.NonSerialized] public exCSS_wrap wrap = exCSS_wrap.Normal;
    [System.NonSerialized] public exCSS_horizontal_align horizontalAlign = exCSS_horizontal_align.Left;
    [System.NonSerialized] public exCSS_vertical_align verticalAlign = exCSS_vertical_align.Top;
    [System.NonSerialized] public int letterSpacing = 0;
    [System.NonSerialized] public int wordSpacing = 0;
    [System.NonSerialized] public int lineHeight = 0;
    [System.NonSerialized] public exCSS_display display = exCSS_display.Inline;

    // bool dirty = false;
    bool isContent_ = false;
    bool isFirstLine_ = false;
    bool hasPushHeightChild = false;
    List<LineInfo> lines = new List<LineInfo>();

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool IsSelfOrAncestorOf ( exUIElement _el ) {
        if ( _el == null )
            return false;

        if ( _el == this )
            return true;

        exUIElement next = _el.parent;
        while ( next != null ) {
            if ( next == this )
                return true;
            next = next.parent;
        }
        return false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void CloneComputedStyle ( exUIElement _el ) {
        x = _el.x;
        y = _el.y;
        width = _el.width;
        height = _el.height;

        minWidth = _el.minWidth;
        maxWidth =  _el.maxWidth;
        minHeight = _el.minHeight;
        maxHeight = _el.maxHeight;

        marginLeft = _el.marginLeft;
        marginRight = _el.marginRight;
        marginTop = _el.marginTop;
        marginBottom = _el.marginBottom;

        borderSizeLeft = _el.borderSizeLeft;
        borderSizeRight = _el.borderSizeRight;
        borderSizeTop = _el.borderSizeTop;
        borderSizeBottom = _el.borderSizeBottom;

        paddingLeft = _el.paddingLeft;
        paddingRight = _el.paddingRight;
        paddingTop = _el.paddingTop;
        paddingBottom = _el.paddingBottom;

        borderImage = _el.borderImage;
        borderColor = _el.borderColor;

        backgroundImage = _el.backgroundImage;
        backgroundColor = _el.backgroundColor;

        font = _el.font;
        fontSize = _el.fontSize;
        contentColor = _el.contentColor;
        wrap = _el.wrap;
        horizontalAlign = _el.horizontalAlign;
        verticalAlign = _el.verticalAlign;
        letterSpacing = _el.letterSpacing;
        wordSpacing = _el.wordSpacing;
        lineHeight = _el.lineHeight;
        display = _el.display;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public exUIElement Clone () {
        exUIElement newEL = new exUIElement ();
        newEL.name = name;
        newEL.id = id;
        newEL.text = text;
        newEL.image = image;
        newEL.contentType = contentType;
        newEL.style = style.Clone();

        for ( int i = 0; i < children.Count; ++i ) {
            exUIElement childEL = children[i].Clone();
            newEL.AddElement (childEL);
        }

        return newEL;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool IsEmptyContent () {
        switch ( contentType ) {
        case ContentType.Text:
        case ContentType.Markdown:
            return string.IsNullOrEmpty(text);

        case ContentType.TextureInfo:
        case ContentType.Texture2D:
            return (image == null);

        default:
            return false;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void GetPosition ( out int _x, out int _y ) {
        _x = x;
        _y = y;

        exUIElement parentEL = parent_;
        while ( parentEL != null ) {
            _x += parentEL.x;
            _y += parentEL.y;

            parentEL = parentEL.parent;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetDirty () {
        // dirty = true;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddElement ( exUIElement _el ) {
        if ( _el == null )
            return;

        if ( _el.parent == this )
            return;

        // you can not add your parent or yourself as your child
        if ( _el.IsSelfOrAncestorOf (this) )
            return;

        exUIElement lastParent = _el.parent;
        if ( lastParent != null ) {
            lastParent.RemoveElement(_el);
            lastParent.SetDirty();
        }

        children.Add(_el);
        SetDirty();

        _el.parent_ = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RemoveElement ( exUIElement _el ) {
        if ( _el == null )
            return;

        int idx = children.IndexOf(_el);
        if ( idx != -1 ) {
            children.RemoveAt(idx);
            _el.parent_ = null;
        }
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

        if ( _el.parent != null )
            _el.parent.RemoveElement(_el);
        this.children.Insert( idx, _el );
        _el.parent_ = this;
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

    public string GetName ( int _idx ) {
        if ( isContent )
            return name;
        return "[" + _idx + "]" + name;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetLineHeight () {
        if ( display == exCSS_display.Inline ) {
            if ( contentType == ContentType.Text )
                return lineHeight;

            return height;
        }

        return height 
            + marginTop + marginBottom 
            + borderSizeTop + borderSizeBottom
            + paddingTop + paddingBottom;
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

    public int GetContentHeight () {
        return height;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetTotalWidth () {
        return width 
            + marginLeft + marginRight 
            + borderSizeLeft + borderSizeRight
            + paddingLeft + paddingRight;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Layout_PreProcess () {
        hasPushHeightChild = false;
        isContent_ = false;
        isFirstLine_ = false;

        display = style.display; 

        for ( int i = 0; i < children.Count; ++i ) {
            children[i].Layout_PreProcess();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void ComputeContentBox ( int _x, int _y, int _width, int _height ) {
        // compute style
        style.Compute ( this, _x, _y, _width, _height );

        // ======================================================== 
        // property relate with content-width 
        // http://www.w3.org/TR/CSS2/visudet.html#Computing_widths_and_margins
        // ======================================================== 

        // calculate width
        if ( style.width.type == exCSS_size_push.Type.Length ) {
            width = (int)style.width.val;
        }
        else if ( style.width.type == exCSS_size_push.Type.Percentage ) {
            width = Mathf.FloorToInt ( style.width.val/100.0f * (float)_width );
        }
        else if ( style.width.type == exCSS_size_push.Type.Auto ||
                  style.width.type == exCSS_size_push.Type.Push ) {
            width = _width 
                   - marginLeft - marginRight
                   - borderSizeLeft - borderSizeRight
                   - paddingLeft - paddingRight;
            width = System.Math.Max ( width, 0 );
        }
        width = System.Math.Min ( System.Math.Max ( width, minWidth ), maxWidth );

        // ======================================================== 
        // property relate with content-height 
        // http://www.w3.org/TR/CSS2/visudet.html#Computing_heights_and_margins
        // ======================================================== 

        // calculate height
        if ( style.height.type == exCSS_size_push.Type.Length ) {
            height = (int)style.height.val;
        }
        else if ( style.height.type == exCSS_size_push.Type.Percentage ) {
            height = Mathf.FloorToInt ( style.height.val/100.0f * (float)_height );
        }
        else if ( style.height.type == exCSS_size_push.Type.Auto ) {
            height = 0;
        }
        else if ( style.height.type == exCSS_size_push.Type.Push ) {
            height = _height 
                   - marginTop - marginBottom
                   - borderSizeTop - borderSizeBottom
                   - paddingTop - paddingBottom;
            height = System.Math.Max ( height, 0 );
        }
        height = System.Math.Min ( System.Math.Max ( height, minHeight ), maxHeight );

        // ======================================================== 
        // calculate position
        // ======================================================== 

        x = _x + marginLeft + borderSizeLeft + paddingLeft;
        y = _y + marginTop + borderSizeTop + paddingTop;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // _x is offset from parent-content-x
    // _y is offset from parent-content-y
    // ------------------------------------------------------------------ 

    public void Layout ( int _x, int _y, int _width, int _height ) {

        ComputeContentBox ( _x, _y, _width, _height );

        // ======================================================== 
        // process content, inlnie elements in normal-flow
        // ======================================================== 

        if ( display == exCSS_display.Inline ) {
            BreakInlineElement ( _x, _y, _width, _height );
        }
        else {
            AddContentToNormalFlow ( _x, _y, width, height );
        } 

        // ======================================================== 
        // layout the children
        // ======================================================== 

        int cur_child_x = 0;
        int cur_child_y = 0;
        int maxLineWidth = 0;
        int maxLineHeight = 0;

        lines.Clear();
        LineInfo curLine = new LineInfo();
        curLine.name = "[" + lines.Count + "]" + name;

        for ( int i = 0; i < normalFlows_.Count; ++i ) {
            exUIElement childEL = normalFlows_[i];

            // do layout if they are not content-line-elements
            if ( childEL.isContent == false ) {
                // if this is a block, wrap it directly
                if ( childEL.display == exCSS_display.Block ) {
                    // add last line
                    if ( curLine.count > 0 ) {
                        if ( curLine.pushWidth )
                            LayoutPushLineElements( curLine, _x, cur_child_y, width, height, ref maxLineWidth, ref maxLineHeight );
                        curLine.height = maxLineHeight;
                        lines.Add(curLine);
                        curLine = new LineInfo();
                        curLine.name = "[" + lines.Count + "]" + name;
                    }

                    // check and store max-line-width
                    if ( cur_child_x > maxLineWidth )
                        maxLineWidth = cur_child_x;

                    // advance last line
                    cur_child_x = 0;
                    cur_child_y += maxLineHeight;
                }

                //
                if ( childEL.display != exCSS_display.Inline && childEL.style.width.type == exCSS_size_push.Type.Push ) {
                    childEL.ComputeContentBox( cur_child_x, cur_child_y, 0, 0 );
                }
                else {
                    childEL.Layout( cur_child_x, cur_child_y, width, height );
                }
            }
            else {
                childEL.x = cur_child_x + childEL.marginLeft + childEL.paddingLeft + childEL.borderSizeLeft;
                childEL.y = cur_child_y;
            }

            // block element will occupy the line and force the next element start a new line 
            if ( childEL.display == exCSS_display.Block ) {
                maxLineHeight = childEL.GetLineHeight();
                curLine.height = maxLineHeight;

                // advance
                cur_child_x = 0;
                cur_child_y += maxLineHeight;

                maxLineHeight = 0;
                int childWidth = childEL.GetTotalWidth();
                if ( childWidth > maxLineWidth )
                    maxLineWidth = childWidth;

                // add this line
                curLine.Add(childEL);
                if ( childEL.style.width.type == exCSS_size_push.Type.Push ) {
                    curLine.pushWidth = true;
                }
                if ( childEL.style.height.type == exCSS_size_push.Type.Push ) {
                    curLine.pushHeight = true;
                    hasPushHeightChild = true;
                }
                curLine.isBlock = true;
                lines.Add(curLine);
                curLine = new LineInfo();
                curLine.name = "[" + lines.Count + "]" + name;
            }
            else {
                // if this is not a content-inline element, we will BreakTextIntoElements here.
                if ( childEL.isContent == false && childEL.display == exCSS_display.Inline ) {
                    if ( childEL.normalFlows.Count > 0 ) {
                        normalFlows_.InsertRange ( i+1, childEL.normalFlows );
                        // childEL.normalFlows.Clear();
                    }
                    continue;
                }

                bool needWrap = false;

                //
                if ( childEL.isContent ) {
                    if ( childEL.isFirstLine ) {
                        if ( childEL.wrap != exCSS_wrap.Normal ) {
                            needWrap = true;
                        }
                        else {
                            if ( wrap != exCSS_wrap.Normal && wrap != exCSS_wrap.NoWrap ) {
                                needWrap = true;
                            }
                            else {
                                int childTotalWidth = childEL.GetTotalWidth();
                                if ( (curLine.count > 0) && (cur_child_x + childTotalWidth) > width ) {
                                    needWrap = true;
                                }
                            }
                        }
                    }
                    else {
                        needWrap = true;
                    }
                }
                else if ( childEL.display == exCSS_display.InlineBlock ) {
                    if ( wrap == exCSS_wrap.Normal || wrap == exCSS_wrap.PreWrap ) {
                        int childTotalWidth = childEL.GetTotalWidth();
                        if ( (curLine.count > 0) && (cur_child_x + childTotalWidth) > width ) {
                            needWrap = true;
                        }
                    }
                }

                // need wrap
                if ( needWrap ) {
                    // add line-info if this is not a block (NOTE: we add block element below)
                    if ( curLine.count > 0 ) {
                        if ( curLine.pushWidth )
                            LayoutPushLineElements( curLine, _x, cur_child_y, width, height, ref maxLineWidth, ref maxLineHeight );
                        curLine.height = maxLineHeight;
                        lines.Add(curLine);
                        curLine = new LineInfo();
                        curLine.name = "[" + lines.Count + "]" + name;
                    }

                    // check and store max-line-width
                    if ( cur_child_x > maxLineWidth )
                        maxLineWidth = cur_child_x;

                    // re-adjust x, wrap it
                    childEL.x = childEL.x - cur_child_x;
                    cur_child_x = 0;

                    // re-adjust y
                    childEL.y = childEL.y + maxLineHeight;

                    // advance-y
                    cur_child_y += maxLineHeight;
                    maxLineHeight = 0;
                }

                // advance-x
                cur_child_x += childEL.GetTotalWidth();

                // get max-line-height
                int childLineHeight = childEL.GetLineHeight();
                if ( childLineHeight > maxLineHeight ) {
                    maxLineHeight = childLineHeight;
                }

                curLine.Add(childEL);
                if ( childEL.display != exCSS_display.Inline && 
                     childEL.style.width.type == exCSS_size_push.Type.Push ) 
                {
                    curLine.pushWidth = true;
                }
                if ( childEL.display != exCSS_display.Inline && 
                     childEL.style.height.type == exCSS_size_push.Type.Push ) 
                {
                    curLine.pushHeight = true;
                    hasPushHeightChild = true;
                }
            }
        }

        // end-line check
        if ( normalFlows_.Count > 0 ) {
            // add the rest line
            if ( curLine.count > 0 ) {
                if ( curLine.pushWidth )
                    LayoutPushLineElements( curLine, _x, cur_child_y, width, height, ref maxLineWidth, ref maxLineHeight );
                curLine.height = maxLineHeight;
                lines.Add(curLine);
            }

            //
            if ( cur_child_x > maxLineWidth )
                maxLineWidth = cur_child_x;
            cur_child_y += maxLineHeight;
        }

        // re-calculate width & height
        if ( display == exCSS_display.Inline ) {
            if ( style.height.type == exCSS_size_push.Type.Auto ) {
                height += cur_child_y;
                height = System.Math.Min ( System.Math.Max ( height, minHeight ), maxHeight );
            }

            // re-adjust x, if we are multi-line
            if ( normalFlows_.Count > 1 ) {
                x = marginLeft + paddingLeft + borderSizeLeft;
                // width = _width - marginRight - borderSizeRight - paddingRight;
            }
            else {
                width = maxLineWidth 
                    - marginLeft - marginRight
                    - borderSizeLeft - borderSizeRight
                    - paddingLeft - paddingRight;
            }

            // re-adjust y
            y -= (marginTop + paddingTop + borderSizeTop);
        }
        else {
            // calculate auto height
            if ( style.height.type == exCSS_size_push.Type.Auto ) {
                height += cur_child_y;
                height = System.Math.Min ( System.Math.Max ( height, minHeight ), maxHeight );
            }

            // calculate auto width
            if ( style.width.type == exCSS_size_push.Type.Auto ) {
                // TODO: if overflow == true
                width = System.Math.Min ( maxLineWidth, width );
                // else
                // width = maxLineWidth;

                width = System.Math.Min ( System.Math.Max ( width, minWidth ), maxWidth );
            }
        }

        // TODO: I think only parent set dirty
        // dirty = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LayoutPushLineElements ( LineInfo _lineInfo, int _x, int _y, int _width, int _height, ref int _maxLineWidth, ref int _maxLineHeight ) {
        int lineWidth = _lineInfo.width;
        int remainWidth = _width - lineWidth;

        if ( remainWidth <= 0 ) {
            return;
        }

        List<exUIElement> pushElements = new List<exUIElement>();
        for ( int i = 0; i < _lineInfo.elements.Count; ++i ) {
            exUIElement el = _lineInfo.elements[i];
            if ( el.display != exCSS_display.Inline && 
                 el.style.width.type == exCSS_size_push.Type.Push )
            {
                pushElements.Add(el);
            }
        }

        //
        if ( pushElements.Count > 0 ) {
            int cur_x = 0;
            int pushSize = remainWidth/pushElements.Count;

            for ( int i = 0; i < _lineInfo.elements.Count; ++i ) {
                exUIElement el = _lineInfo.elements[i];
                if ( el.display != exCSS_display.Inline && 
                     el.style.width.type == exCSS_size_push.Type.Push )
                {
                    // NOTE: this is because in _lineInfo.width we calculate lineWidth by GetTotalWidth() for each element
                    int width = pushSize 
                        + el.marginLeft + el.marginRight
                        + el.borderSizeLeft + el.borderSizeRight
                        + el.paddingLeft + el.paddingRight;
                    el.Layout ( cur_x, _y, width, _height );

                    int childLineHeight = el.GetLineHeight();
                    if ( childLineHeight > _maxLineHeight ) {
                        _maxLineHeight = childLineHeight;
                    }
                }
                else {
                    el.x = cur_x + el.marginLeft + el.borderSizeLeft + el.paddingLeft;
                }
                cur_x += el.GetTotalWidth();
            }

            //
            if ( _width > _maxLineWidth ) {
                _maxLineWidth = _width;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AdjustLines ( int _width, int _height ) {
        AdjustBlockElement ( _width, _height );

        // adjust push height
        if ( hasPushHeightChild ) {
            int remainHeight = System.Math.Max( _height - GetTotalHeight(), 0 );
            if ( remainHeight > 0 ) {
                // get the push height element counts in all lines
                int pushHeightCount = 0;
                for ( int i = 0; i < lines.Count; ++i ) {
                    LineInfo lineInfo = lines[i];
                    if ( lineInfo.pushHeight ) {
                        ++pushHeightCount;
                    }
                }

                // adjust the line-height and the element x,y
                int pushSize = (pushHeightCount > 0) ? (remainHeight/pushHeightCount) : 0;
                int cur_y = 0;
                for ( int i = 0; i < lines.Count; ++i ) {
                    LineInfo lineInfo = lines[i];
                    int maxLineHeight = lineInfo.height;

                    for ( int j = 0; j < lineInfo.elements.Count; ++j ) {
                        exUIElement el = lineInfo.elements[j];

                        if ( el.display == exCSS_display.Inline ) {
                            el.y = cur_y;
                        }
                        else {
                            if ( el.style.height.type == exCSS_size_push.Type.Push ) {
                                el.height = pushSize; 

                                int totalHeight = el.GetTotalHeight();
                                if ( maxLineHeight < totalHeight ) {
                                    maxLineHeight = totalHeight;
                                }
                            }
                            el.y = cur_y + el.marginTop + el.borderSizeTop + el.paddingTop;
                        }
                    }

                    if ( lineInfo.pushHeight ) {
                        lineInfo.height = maxLineHeight;
                    }

                    cur_y += lineInfo.height;
                }
                height = cur_y;
            }
        }

        // adjust line elements ( do not adjust inline element )
        for ( int i = 0; i < lines.Count; ++i ) {
            LineInfo curLine = lines[i];
            if ( curLine.isBlock == false ) {
                AdjustLineElements ( curLine, width );
            }
        }
        lines.Clear();

        // adjust children
        for ( int i = 0; i < normalFlows_.Count; ++i ) {
            exUIElement childEL = normalFlows_[i];

            // skip inline elements
            if ( childEL.display == exCSS_display.Inline )
                continue;

            childEL.AdjustLines ( width, height );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AdjustBlockElement ( int _width, int _height ) {
        if ( display == exCSS_display.Block ) {
            if ( style.width.type == exCSS_size_push.Type.Auto && contentType == ContentType.Text ) {
                width = _width 
                    - marginLeft - marginRight
                    - borderSizeLeft - borderSizeRight
                    - paddingLeft - paddingRight;
                width = System.Math.Max ( width, 0 );
                width = System.Math.Min ( System.Math.Max ( width, minWidth ), maxWidth );
            }
            else {
                // NOTE: auto-margin is a little wested when we have style.width.type == exCSS_size_push.Type.Push
                if ( ( style.marginLeft.type == exCSS_size.Type.Auto || style.marginRight.type == exCSS_size.Type.Auto ) ) {
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

                    x = marginLeft + borderSizeLeft + paddingLeft;
                }
            }

            if ( style.marginRight.type != exCSS_size.Type.Auto ) {
                int remainWidth = _width - width - borderSizeLeft - borderSizeRight - paddingLeft - paddingRight;
                remainWidth -= marginLeft; 
                remainWidth = System.Math.Max ( remainWidth, 0 );
                marginRight = remainWidth;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AdjustLineElements ( LineInfo _lineInfo, int _width ) {

        int lineWidth = _lineInfo.width;
        int lineHeight = _lineInfo.height;
        int remainWidth = System.Math.Max( _width - lineWidth, 0 );
        int offset_x = 0;
        int offset_y = 0;

        //
        if ( _lineInfo.pushWidth == false ) {
            switch ( horizontalAlign ) {
            case exCSS_horizontal_align.Left:
                offset_x = 0;
                break;

            case exCSS_horizontal_align.Center:
                offset_x = remainWidth/2;
                break;

            case exCSS_horizontal_align.Right:
                offset_x = remainWidth;
                break;
            }
        }

        //
        for ( int i = 0; i < _lineInfo.elements.Count; ++i ) {
            exUIElement el = _lineInfo.elements[i];

            int remainHeight = System.Math.Max( lineHeight - el.GetLineHeight(), 0 );
            switch ( el.verticalAlign ) {
            case exCSS_vertical_align.Top:
                offset_y = 0;
                break;

            case exCSS_vertical_align.Middle:
                offset_y = remainHeight/2;
                break;

            case exCSS_vertical_align.Bottom:
                offset_y = remainHeight;
                break;
            }

            // adjust the el to make it draw the text in the middle of the line-height
            if ( el.isContent ) {
                remainHeight = System.Math.Max( el.GetLineHeight() - el.GetContentHeight(), 0 );
                int offset_y_2 = remainHeight/2;
                offset_y += offset_y_2;
            }

            el.x += offset_x;
            el.y += offset_y;

            // re-adjust owner, if it is not multi-line
            if ( el.isContent && el.isFirstLine && el.owner != null && el.owner.normalFlows.Count == 1 ) {
                el.owner.x = el.x;
                el.owner.y = el.y;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddContentToNormalFlow ( int _x, int _y, int _width, int _height ) {
        normalFlows_.Clear();

        exUIElement newEL = new exUIElement ();
        newEL.name = "__inline_content";
        newEL.id = id;
        newEL.text = text;
        newEL.image = image;
        newEL.contentType = contentType;
        newEL.style = style.InlineContent( _width, _height );
        if ( newEL.style.width.type != exCSS_size_push.Type.Auto ) {
            newEL.style.width.type = exCSS_size_push.Type.Length;
            newEL.style.width.val = _width;
        }
        if ( newEL.style.height.type != exCSS_size_push.Type.Auto ) {
            newEL.style.height.type = exCSS_size_push.Type.Length;
            newEL.style.height.val = _height;
        }
        newEL.parent_ = parent_; // NOTE: DO NOT use AddElement which will make this element become real child.

        normalFlows_.Add(newEL);
        normalFlows_.AddRange(children);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void BreakInlineElement ( int _x, int _y, int _width, int _height ) {
        int cur_x = _x;
        int cur_y = 0;

        // NOTE: ony image element affect by width & height
        int imgWidth = width;
        int imgHeight = height;

        normalFlows_.Clear();

        switch ( contentType ) {
        case ContentType.Text:
            // inline text never care the width, height
            BreakTextIntoElements ( text, _width, ref cur_x, ref cur_y );
            break;

        case ContentType.Markdown:
            // TODO:
            break;

        case ContentType.Texture2D:
            Texture2D texture = image as Texture2D;
            if ( texture != null ) {
                if ( style.width.type == exCSS_size_push.Type.Auto ) {
                    imgWidth = texture.width;
                }
                if ( style.height.type == exCSS_size_push.Type.Auto ) {
                    imgHeight = texture.height;
                }
                AddImageIntoElements ( texture, contentType, imgWidth, imgHeight, ref cur_x, ref cur_y );
            }
            break;

        case ContentType.TextureInfo:
            exTextureInfo textureInfo = image as exTextureInfo;
            if ( textureInfo != null ) {
                if ( style.width.type == exCSS_size_push.Type.Auto ) {
                    imgWidth = textureInfo.width;
                }
                if ( style.height.type == exCSS_size_push.Type.Auto ) {
                    imgHeight = textureInfo.height;
                }
                AddImageIntoElements ( textureInfo, contentType, imgWidth, imgHeight, ref cur_x, ref cur_y );
            }
            break;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddImageIntoElements ( Object _image, 
                                ContentType _contentType, 
                                int _width, 
                                int _height, 
                                ref int _x, 
                                ref int _y ) {
        int cur_x = 0;
        int cur_y = _y;

        exUIElement newEL = new exUIElement();
        newEL.CloneComputedStyle (this);
        newEL.owner = this;
        newEL.name = "[0]" + "__line";
        newEL.isContent_ = true;
        newEL.isFirstLine_ = true;
        newEL.style = null;
        newEL.image = _image;
        newEL.contentType = _contentType;

        newEL.x = cur_x;
        newEL.y = cur_y;
        newEL.width = _width;
        newEL.height = _height;

        normalFlows_.Add(newEL);

        _x = _x + _width;
        _y = _y + _height;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void BreakTextIntoElements ( string _text, 
                                 int _width, 
                                 ref int _x, 
                                 ref int _y ) {
        if ( string.IsNullOrEmpty(_text) )
            return;

        exTextUtility.WrapMode wrapMode = exTextUtility.WrapMode.None;

        switch ( wrap ) {
        case exCSS_wrap.Normal:    wrapMode = exTextUtility.WrapMode.Word;    break;
        case exCSS_wrap.Pre:       wrapMode = exTextUtility.WrapMode.Pre;     break;
        case exCSS_wrap.NoWrap:    wrapMode = exTextUtility.WrapMode.None;    break;
        case exCSS_wrap.PreWrap:   wrapMode = exTextUtility.WrapMode.PreWrap; break;
        }

        bool finished = false;
        int line_width = 0;
        int cur_x = 0 + marginLeft + borderSizeLeft + paddingLeft;
        int cur_y = _y;
        int cur_index = 0;
        int cur_width = _width - _x - (marginLeft + borderSizeLeft + paddingLeft);
        StringBuilder builder = new StringBuilder(_text.Length);

        int line_id = 0;
        bool begin = true;
        bool firstWordCheck = (_x > 0) && (wrapMode == exTextUtility.WrapMode.Word);
        bool endLineCheck = ( marginRight > 0 || paddingRight > 0 || borderSizeRight > 0 );

        if ( font is Font ) {
            Font ttfFont = font as Font;
            ttfFont.RequestCharactersInTexture ( _text, fontSize, FontStyle.Normal );

            while ( finished == false ) {
                // int start_index = cur_index;
                builder.Length = 0;
                int last_index = cur_index;

                //
                finished = exTextUtility.CalcTextLine ( ref line_width, 
                                                        ref cur_index,
                                                        ref builder,
                                                        _text,
                                                        cur_index,
                                                        cur_width,
                                                        ttfFont,
                                                        fontSize,
                                                        wrapMode,
                                                        wordSpacing,
                                                        letterSpacing );

                // if inline-block's first line exceed, it will start from next line 
                if ( firstWordCheck ) {
                    firstWordCheck = false;
                    if ( line_width > cur_width ) {
                        string line_text = builder.ToString();
                        if ( line_text.IndexOf(' ') == -1 ) {
                            finished = false;
                            cur_x = 0;
                            cur_width = _width - (marginLeft + borderSizeLeft + paddingLeft);
                            cur_index = 0;
                            continue;
                        }
                    }
                }

                // end-line re-check
                if ( finished && endLineCheck ) {
                    endLineCheck = false;
                    cur_width -= (marginRight + borderSizeRight + paddingRight);
                    if ( line_width > cur_width ) {
                        finished = false;
                        cur_x = 0;
                        // cur_width = cur_width;
                        cur_index = last_index;
                        continue;
                    }
                }

                // generate element
                exUIElement newEL = new exUIElement();
                newEL.CloneComputedStyle (this);
                newEL.owner = this;
                newEL.name = "[" + line_id + "]" + "__line";
                newEL.isContent_ = true;
                newEL.isFirstLine_ = begin;
                newEL.style = null;
                newEL.text = builder.ToString();
                newEL.contentType = ContentType.Text;

                newEL.x = cur_x;
                newEL.y = cur_y;
                newEL.width = line_width;
                newEL.height = fontSize;

                if ( begin == false ) {
                    newEL.marginLeft = 0;
                    newEL.paddingLeft = 0;
                    newEL.borderSizeLeft = 0;
                }
                if ( finished == false ) {
                    newEL.marginRight = 0;
                    newEL.paddingRight = 0;
                    newEL.borderSizeRight = 0;
                }
                if ( begin ) {
                    begin = false;
                }

                normalFlows_.Add(newEL);

                //
                cur_x = 0;
                cur_y += lineHeight;
                cur_width = _width;
                ++cur_index;
                ++line_id;
            }

            _x = line_width;
            _y = cur_y;
        }
        else if ( font is exBitmapFont ) {

            exBitmapFont bitmapFont = font as exBitmapFont;

            while ( finished == false ) {
                // int start_index = cur_index;
                builder.Length = 0;
                int last_index = cur_index;

                //
                finished = exTextUtility.CalcTextLine ( ref line_width, 
                                                        ref cur_index,
                                                        ref builder,
                                                        _text,
                                                        cur_index,
                                                        cur_width,
                                                        bitmapFont,
                                                        fontSize,
                                                        wrapMode,
                                                        wordSpacing,
                                                        letterSpacing );

                // if inline-block's first line exceed, it will start from next line 
                if ( firstWordCheck ) {
                    firstWordCheck = false;
                    if ( line_width > cur_width ) {
                        string line_text = builder.ToString();
                        if ( line_text.IndexOf(' ') == -1 ) {
                            finished = false;
                            cur_x = 0;
                            cur_width = _width - (marginLeft + borderSizeLeft + paddingLeft);
                            cur_index = 0;
                            continue;
                        }
                    }
                }

                // end-line re-check
                if ( finished && endLineCheck ) {
                    endLineCheck = false;
                    cur_width -= (marginRight + borderSizeRight + paddingRight);
                    if ( line_width > cur_width ) {
                        finished = false;
                        cur_x = 0;
                        // cur_width = cur_width;
                        cur_index = last_index;
                        continue;
                    }
                }

                // generate element
                exUIElement newEL = new exUIElement();
                newEL.CloneComputedStyle (this);
                newEL.owner = this;
                newEL.name = "[" + line_id + "]" + "__line";
                newEL.isContent_ = true;
                newEL.isFirstLine_ = begin;
                newEL.style = null;
                newEL.text = builder.ToString();
                newEL.contentType = ContentType.Text;

                newEL.x = cur_x;
                newEL.y = cur_y;
                newEL.width = line_width;
                newEL.height = bitmapFont.lineHeight;

                if ( begin == false ) {
                    newEL.marginLeft = 0;
                    newEL.paddingLeft = 0;
                    newEL.borderSizeLeft = 0;
                }
                if ( finished == false ) {
                    newEL.marginRight = 0;
                    newEL.paddingRight = 0;
                    newEL.borderSizeRight = 0;
                }
                if ( begin ) {
                    begin = false;
                }

                normalFlows_.Add(newEL);

                //
                cur_x = 0;
                cur_y += lineHeight;
                cur_width = _width;
                ++cur_index;
                ++line_id;
            }

            _x = line_width;
            _y = cur_y;
        }
    }
}

