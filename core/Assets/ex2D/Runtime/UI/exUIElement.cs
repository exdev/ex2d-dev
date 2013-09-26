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
    [System.NonSerialized] public int letterSpacing = 0;
    [System.NonSerialized] public int wordSpacing = 0;
    [System.NonSerialized] public int lineHeight = 0;
    [System.NonSerialized] public exCSS_display display = exCSS_display.Inline;

    bool dirty = false;
    bool isContent_ = false;

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
        dirty = true;
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

    public int GetTotalHeight () {
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
        display = style.display; 

        for ( int i = 0; i < children.Count; ++i ) {
            children[i].Layout_PreProcess();
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // _x is offset from parent-content-x
    // _y is offset from parent-content-y
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
        width = System.Math.Min ( System.Math.Max ( width, minWidth ), maxWidth );

        // NOTE: auto-margin is a little wested when we have style.width.type == exCSS_size_push.Type.Push
        if ( display == exCSS_display.Block ) {
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
        height = System.Math.Min ( System.Math.Max ( height, minHeight ), maxHeight );

        // ======================================================== 
        // calculate position
        // ======================================================== 

        x = x + marginLeft + borderSizeLeft + paddingLeft;
        y = y + marginTop + borderSizeTop + paddingTop;

        // ======================================================== 
        // process content, inlnie elements in normal-flow
        // ======================================================== 

        if ( display == exCSS_display.Inline ) {
            BreakContentToNormalFlow ( _x, _y, width, height );
        }
        else {
            AddElementsToNormalFlow ( _x, _y, width, height );
        } 

        // ======================================================== 
        // layout the children
        // ======================================================== 

        int cur_child_x = 0;
        int cur_child_y = 0;
        int maxLineWidth = 0;
        int maxLineHeight = 0;
        int lineChildCount = 0;

        for ( int i = 0; i < normalFlows_.Count; ++i ) {
            exUIElement child = normalFlows_[i];
            bool needWrap = false;

            // do layout if they are not content-line-elements
            if ( child.isContent == false ) {
                child.Layout( cur_child_x, cur_child_y, width, height );
            }
            else {
                child.x = cur_child_x + child.marginLeft + child.paddingLeft + child.borderSizeLeft;
                child.y = cur_child_y;
            }

            // if this is not a content-inline element, we will BreakTextIntoElements here.
            if ( child.isContent == false && child.display == exCSS_display.Inline ) {
                if ( child.normalFlows.Count > 0 ) {
                    normalFlows_.InsertRange ( i+1, child.normalFlows );

                    // TEMP { 
                    // re-adjust multi-line inline element. this is good for debug
                    // if ( child.normalFlows.Count > 1 ) {
                    //     child.x = 0;
                    // }
                    // } TEMP end 

                    child.normalFlows.Clear();
                }

                // TEMP { 
                // re-adjust y when it is a inline element. this is good for debug
                child.y -= (child.marginTop + child.paddingTop + child.borderSizeTop);
                // } TEMP end 

                continue;
            }
            
            //
            if ( child.isContent ) {
                if ( wrap != exCSS_wrap.Normal && wrap != exCSS_wrap.NoWrap ) {
                    needWrap = true;
                }
                else {
                    int childTotalWidth = child.GetTotalWidth();
                    if ( (lineChildCount > 0) && (cur_child_x + childTotalWidth) > width ) {
                        needWrap = true;
                    }
                }
            }
            else if ( child.display == exCSS_display.Block ) {
                needWrap = true;
            }
            else if ( child.display == exCSS_display.InlineBlock ) {
                if ( wrap == exCSS_wrap.Normal || wrap == exCSS_wrap.PreWrap ) {
                    int childTotalWidth = child.GetTotalWidth();
                    if ( (lineChildCount > 0) && (cur_child_x + childTotalWidth) > width ) {
                        needWrap = true;
                    }
                }
            }

            // need wrap
            if ( needWrap ) {

                // TODO: adjust last line childrens

                // check and store max-line-width
                if ( cur_child_x > maxLineWidth )
                    maxLineWidth = cur_child_x;

                // re-adjust x, wrap it
                child.x = child.x - cur_child_x;
                cur_child_x = 0;

                // re-adjust y
                child.y = child.y + maxLineHeight;

                // advance-y
                cur_child_y += maxLineHeight;
                maxLineHeight = 0;
                lineChildCount = 0;
            }

            // advance-x
            cur_child_x += child.GetTotalWidth();

            // get max-line-height
            int childTotalHeight = child.GetTotalHeight();
            if ( childTotalHeight > maxLineHeight ) {
                maxLineHeight = childTotalHeight;
            }

            ++lineChildCount;

            // start a new line directly if this is a block
            if ( child.display == exCSS_display.Block ) {
                if ( cur_child_x > maxLineWidth )
                    maxLineWidth = cur_child_x;
                cur_child_x = 0;
                cur_child_y += maxLineHeight;
                maxLineHeight = 0;
                lineChildCount = 0;
            }
        }

        // end-line check
        if ( cur_child_x > maxLineWidth )
            maxLineWidth = cur_child_x;
        cur_child_y += maxLineHeight;

        // calculate auto height
        if ( style.height.type == exCSS_size.Type.Auto ) {
            height += cur_child_y;
            height = System.Math.Min ( System.Math.Max ( height, minHeight ), maxHeight );
        }

        // calculate auto width
        if ( style.width.type == exCSS_size.Type.Auto ) {
            bool useContentWidth = false;

            if ( display == exCSS_display.InlineBlock ||
                 display == exCSS_display.Inline ) 
            {
                useContentWidth = true;
            }

            if ( display == exCSS_display.Block ) {
                if ( contentType == ContentType.Texture2D ||
                     contentType == ContentType.TextureInfo )
                {
                    useContentWidth = true;
                }
                else if ( parent != null && parent.display != exCSS_display.Block && parent.style.width.type == exCSS_size.Type.Auto ) {
                    useContentWidth = true;
                }
            }

            if ( useContentWidth ) {
                width = maxLineWidth;
                width = System.Math.Min ( System.Math.Max ( width, minWidth ), maxWidth );
            }
        }

        // TODO: I think only parent set dirty
        dirty = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void AddElementsToNormalFlow ( int _x, int _y, int _width, int _height ) {
        normalFlows_.Clear();

        exUIElement newEL = new exUIElement ();
        newEL.name = name;
        newEL.id = id;
        newEL.text = text;
        newEL.image = image;
        newEL.contentType = contentType;
        newEL.style = style.InlineContent();
        newEL.style.display = exCSS_display.Inline;
        newEL.parent_ = parent_; // NOTE: DO NOT use AddElement which will make this element become real child.

        normalFlows_.Add(newEL);
        normalFlows_.AddRange(children);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void BreakContentToNormalFlow ( int _x, int _y, int _width, int _height ) {
        int cur_x = _x;
        int cur_y = 0;
        int imgWidth = _width;
        int imgHeight = _height;

        normalFlows_.Clear();

        switch ( contentType ) {
        case ContentType.Text:
            BreakTextIntoElements ( text, _width, ref cur_x, ref cur_y );
            break;

        case ContentType.Markdown:
            // TODO:
            break;

        case ContentType.Texture2D:
            Texture2D texture = image as Texture2D;
            if ( texture != null ) {
                if ( style.width.type == exCSS_size.Type.Auto ) {
                    imgWidth = texture.width;
                }
                if ( style.height.type == exCSS_size.Type.Auto ) {
                    imgHeight = texture.height;
                }
                AddImageIntoElements ( texture, contentType, imgWidth, imgHeight, ref cur_x, ref cur_y );
            }
            break;

        case ContentType.TextureInfo:
            exTextureInfo textureInfo = image as exTextureInfo;
            if ( textureInfo != null ) {
                if ( style.width.type == exCSS_size.Type.Auto ) {
                    imgWidth = textureInfo.width;
                }
                if ( style.height.type == exCSS_size.Type.Auto ) {
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
        newEL.name = name + " [0]";
        newEL.isContent_ = true;
        newEL.style = null;
        newEL.display = exCSS_display.Inline; 
        newEL.x = cur_x;
        newEL.y = cur_y;
        newEL.image = _image;
        newEL.contentType = _contentType;
        newEL.width = _width;
        newEL.height = _height;
        newEL.contentColor = contentColor;

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
        int cur_x = 0;
        int cur_y = _y;
        int cur_index = 0;
        int cur_width = _width - _x;
        bool firstLineCheck = (_x > 0 && display != exCSS_display.Inline);
        StringBuilder builder = new StringBuilder(_text.Length);
        int line_id = 0;
        bool begin = true;

        if ( font is Font ) {
            (font as Font).RequestCharactersInTexture ( _text, fontSize, FontStyle.Normal );

            while ( finished == false ) {
                // int start_index = cur_index;
                builder.Length = 0;

                // TODO { 
                // // if begin, apply margin-left, padding-left and border-left
                // int x_offset = 0;
                // if ( firstLineCheck ) {
                //     x_offset = marginLeft + paddingLeft + borderSizeLeft;
                //     cur_width -= x_offset;
                // }
                // } TODO end 

                //
                finished = exTextUtility.CalcTextLine ( ref line_width, 
                                                        ref cur_index,
                                                        ref builder,
                                                        _text,
                                                        cur_index,
                                                        cur_width,
                                                        font as Font,
                                                        fontSize,
                                                        wrapMode,
                                                        wordSpacing,
                                                        letterSpacing );

                // TODO { 
                // if end, apply margin-right, padding-right and border-right
                // if ( finished ) {
                // }
                // } TODO end 

                // if inline-block's first line exceed, it will start from next line 
                if ( firstLineCheck ) {
                    firstLineCheck = false;
                    if ( finished == false || line_width > cur_width ) {
                        finished = false;
                        cur_x = 0;
                        cur_width = _width;
                        cur_index = 0;
                        continue;
                    }
                }

                // generate element
                exUIElement newEL = new exUIElement();
                newEL.CloneComputedStyle (this);
                newEL.name = name + " [" + line_id + "]";
                newEL.isContent_ = true;
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

            while ( finished == false ) {
                // int start_index = cur_index;
                builder.Length = 0;

                //
                finished = exTextUtility.CalcTextLine ( ref line_width, 
                                                        ref cur_index,
                                                        ref builder,
                                                        _text,
                                                        cur_index,
                                                        cur_width,
                                                        font as exBitmapFont,
                                                        fontSize,
                                                        wrapMode,
                                                        wordSpacing,
                                                        letterSpacing );

                // if inline-block's first line exceed, it will start from next line 
                if ( firstLineCheck ) {
                    firstLineCheck = false;
                    if ( finished == false || line_width > cur_width ) {
                        finished = false;
                        cur_x = 0;
                        cur_width = _width;
                        cur_index = 0;
                        continue;
                    }
                }

                // generate element
                exUIElement newEL = new exUIElement();
                newEL.CloneComputedStyle (this);
                newEL.name = name + " [" + line_id + "]";
                newEL.isContent_ = true;
                newEL.style = null;
                newEL.text = builder.ToString();
                newEL.contentType = ContentType.Text;

                newEL.x = cur_x;
                newEL.y = cur_y;
                newEL.width = line_width;
                newEL.height = fontSize;

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

