// ======================================================================================
// File         : exSpriteFont.cs
// Author       : Jare
// Last Change  : 07/28/2013 | 22:13:41
// Description  : 
// ======================================================================================

#define  ENABLE
#if ENABLE

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
/// \class exSpriteFont
/// 
/// A component to render exBitmapFont in the game 
/// 
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D Sprite/Sprite Font")]
public class exSpriteFont : exSpriteBase {

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    [SerializeField] protected exBitmapFont font_;
    /// The referenced bitmap font asset
    // ------------------------------------------------------------------ 

    public exBitmapFont font {
        get { return font_; }
        set {
            if ( font_ != value ) {
                font_ = value;
                updateFlags |= exUpdateFlags.Text;
                UpdateMaterial();
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected string text_ = "Hello World!"; 
    /// The text to rendered. 
    /// \NOTE If you need to change the text frequently, you should use dynamic layer.
    // ------------------------------------------------------------------ 

    public string text {
        get { return text_; }
        set {
            if ( text_ != value ) {
                string oldText = text_;
                text_ = value;
                // TODO: check multiline
                if (oldText.Length != value.Length) {
                    UpdateCapacity();   // TODO: 如果在一帧内反复修改文本，会造成多余的layer改动，考虑留到update时再处理
                }
                updateFlags |= exUpdateFlags.Text;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected bool useMultiline_ = false;
    /// If useMultiline is true, the exSpriteFont.text accept multiline string. 
    // ------------------------------------------------------------------ 

    public bool useMultiline {
        get { return useMultiline_; }
        set {
            if ( useMultiline_ != value ) {
                useMultiline_ = value;
                updateFlags |= exUpdateFlags.Text;  // TODO: only need to update vertex ?
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected TextAlignment textAlign_ = TextAlignment.Left;
    /// The alignment method used in the text
    // ------------------------------------------------------------------ 

    public TextAlignment textAlign {
        get { return textAlign_; }
        set {
            if ( textAlign_ != value ) {
                textAlign_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected bool useKerning_ = false;
    /// If useKerning is true, the SpriteFont will use the exBitmapFont.KerningInfo in 
    /// the exSpriteFont.fontInfo to layout the text
    // ------------------------------------------------------------------ 

    public bool useKerning {
        get { return useKerning_; }
        set {
            if ( useKerning_ != value ) {
                useKerning_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 spacing_;
    /// spacing_.x : the fixed width applied between two characters in the text. 
    /// spacing_.y : the fixed line space applied between two lines.
    // ------------------------------------------------------------------ 

    public Vector2 spacing {
        get { return spacing_; }
        set {
            if ( spacing_ != value ) {
                spacing_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

    // color option

    // ------------------------------------------------------------------ 
    [SerializeField] protected Color topColor_ = Color.white; // TODO: use gradient
    /// the color of the vertices at top 
    // ------------------------------------------------------------------ 
    
    public Color topColor {
        get { return topColor_; }
        set {
            if ( topColor_ != value ) {
                topColor_ = value;
                updateFlags |= exUpdateFlags.Color;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Color botColor_ = Color.white;
    /// the color of the vertices at bottom 
    // ------------------------------------------------------------------ 

    public Color botColor {
        get { return botColor_; }
        set {
            if ( botColor_ != value ) {
                botColor_ = value;
                updateFlags |= exUpdateFlags.Color;
            }
        }
    }

    // outline option

    //// ------------------------------------------------------------------ 
    //[SerializeField] protected bool useOutline_ = false;
    ///// If useOutline is true, the component will render the text with outline
    //// ------------------------------------------------------------------ 

    //public bool useOutline {
    //    get { return useOutline_; }
    //    set {
    //        if ( useOutline_ != value ) {
    //            useOutline_ = value;
    //            updateFlags |= exUpdateFlags.Text; 
    //        }
    //    }
    //}

    //// ------------------------------------------------------------------ 
    //[SerializeField] protected float outlineWidth_ = 1.0f;
    ///// The width of the outline text
    //// ------------------------------------------------------------------ 

    //public float outlineWidth {
    //    get { return outlineWidth_; }
    //    set {
    //        if ( outlineWidth_ != value ) {
    //            outlineWidth_ = value;
    //            if ( useOutline_ ) {
    //                updateFlags |= exUpdateFlags.Vertex;
    //            }
    //        }
    //    }
    //}

    //// ------------------------------------------------------------------ 
    //[SerializeField] protected Color outlineColor_ = Color.black;
    ///// The color of the outline text
    //// ------------------------------------------------------------------ 

    //public Color outlineColor {
    //    get { return outlineColor_; }
    //    set {
    //        if ( outlineColor_ != value ) {
    //            outlineColor_ = value;
    //            if ( useOutline_ ) {
    //                updateFlags |= exUpdateFlags.Color;
    //            }
    //        }
    //    }
    //}

    // shadow option

    //// ------------------------------------------------------------------ 
    //[SerializeField] protected bool useShadow_ = false;
    ///// If useShadow is true, the component will render the text with shadow
    //// ------------------------------------------------------------------ 

    //public bool useShadow {
    //    get { return useShadow_; }
    //    set {
    //        if ( useShadow_ != value ) {
    //            useShadow_ = value;
    //            updateFlags |= exUpdateFlags.Text; 
    //        }
    //    }
    //}

    //// ------------------------------------------------------------------ 
    //[SerializeField] protected Vector2 shadowBias_ = new Vector2 ( 1.0f, -1.0f );
    ///// The bias of the shadow text 
    //// ------------------------------------------------------------------ 

    //public Vector2 shadowBias {
    //    get { return shadowBias_; }
    //    set {
    //        if ( shadowBias_ != value ) {
    //            shadowBias_ = value;
    //            if ( useShadow_ ) {
    //                updateFlags |= exUpdateFlags.Vertex;
    //            }
    //        }
    //    }
    //}

    //// ------------------------------------------------------------------ 
    //[SerializeField] protected Color shadowColor_ = Color.black;
    ///// The color of the shadow text 
    //// ------------------------------------------------------------------ 

    //public Color shadowColor {
    //    get { return shadowColor_; }
    //    set {
    //        if ( shadowColor_ != value ) {
    //            shadowColor_ = value;
    //            if ( useShadow_ ) {
    //                updateFlags |= exUpdateFlags.Color;
    //            }
    //        }
    //    }
    //}

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// 返回capacity的顶点数量，实际渲染的字符顶点数可能少于这个值。
    // ------------------------------------------------------------------ 

    public override int vertexCount {
        get { 
            return vertexCountCapacity;
        }
    }
    
    // ------------------------------------------------------------------ 
    /// 返回capacity的顶点索引数量，实际渲染的字符顶点索引数可能少于这个值。
    // ------------------------------------------------------------------ 

    public override int indexCount {
        get { 
            return indexCountCapacity;
        }
    }

    protected override Texture texture {
        get {
            if (font_ != null) {
                return font_.texture;
            }
            else {
                return null;
            }
        }
    }
    
    [System.NonSerialized] private int vertexCountCapacity = exMesh.QUAD_VERTEX_COUNT;
    [System.NonSerialized] private int indexCountCapacity = exMesh.QUAD_INDEX_COUNT;

    /*
    ///////////////////////////////////////////////////////////////////////////////
    // geometry buffers
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] private List<Vector3> vertices = new List<Vector3>();  ///< 按文本顺序排列
    [System.NonSerialized] private List<Vector2> uvs = new List<Vector2>();       ///< 按文本顺序排列
    [System.NonSerialized] private List<Color32> colors32 = new List<Color32>();  ///< 按文本顺序排列

    /// 不需要按顺序排列，面片数量和文本数量保持一致即可
    [System.NonSerialized] private List<int> indices = new List<int>(); 

    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////
    */

#region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (List<Vector3> _vertices, List<Vector2> _uvs, List<Color32> _colors32, List<int> _indices) {
        // TODO: if indices count changed, re add to layer
        //// pre check fontInfo
        //if ( fontInfo_ == null ) {
        //    _mesh.Clear();
        //    return;
        //}
        //if ((updateFlags & exUpdateFlags.Vertex) != 0) {
        //    UpdateVertexBuffer(_vertices, vertexBufferIndex);
        //}
        //if ((updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
        //    _indices[indexBufferIndex]     = vertexBufferIndex;
        //    _indices[indexBufferIndex + 1] = vertexBufferIndex + 1;
        //    _indices[indexBufferIndex + 2] = vertexBufferIndex + 2;
        //    _indices[indexBufferIndex + 3] = vertexBufferIndex + 2;
        //    _indices[indexBufferIndex + 4] = vertexBufferIndex + 3;
        //    _indices[indexBufferIndex + 5] = vertexBufferIndex;
        //}
        //if ((updateFlags & exUpdateFlags.UV) != 0) {
        //    Vector2 texelSize;
        //    if (textureInfo.texture != null) {
        //        texelSize = textureInfo.texture.texelSize;
        //    }
        //    else {
        //        texelSize = new Vector2(1.0f / textureInfo.rawWidth, 1.0f / textureInfo.rawHeight);
        //    }
        //    Vector2 start = new Vector2((float)textureInfo.x * texelSize.x, 
        //                                 (float)textureInfo.y * texelSize.y);
        //    Vector2 end = new Vector2((float)(textureInfo.x + textureInfo.rotatedWidth) * texelSize.x, 
        //                               (float)(textureInfo.y + textureInfo.rotatedHeight) * texelSize.y);
        //    if ( textureInfo.rotated ) {
        //        _uvs[vertexBufferIndex + 0] = new Vector2(end.x, start.y);
        //        _uvs[vertexBufferIndex + 1] = start;
        //        _uvs[vertexBufferIndex + 2] = new Vector2(start.x, end.y);
        //        _uvs[vertexBufferIndex + 3] = end;
        //    }
        //    else {
        //        _uvs[vertexBufferIndex + 0] = start;
        //        _uvs[vertexBufferIndex + 1] = new Vector2(start.x, end.y);
        //        _uvs[vertexBufferIndex + 2] = end;
        //        _uvs[vertexBufferIndex + 3] = new Vector2(end.x, start.y);
        //    }
        //}
        //if ((updateFlags & exUpdateFlags.Color) != 0) {
        //    _colors32[vertexBufferIndex + 0] = new Color32(255, 255, 255, 255);
        //    _colors32[vertexBufferIndex + 1] = new Color32(255, 255, 255, 255);
        //    _colors32[vertexBufferIndex + 2] = new Color32(255, 255, 255, 255);
        //    _colors32[vertexBufferIndex + 3] = new Color32(255, 255, 255, 255);
        //}
        exUpdateFlags spriteUpdateFlags = updateFlags;
        updateFlags = exUpdateFlags.None;
        return spriteUpdateFlags;
    }

#endregion // Functions used to update geometry buffer
    
    // ------------------------------------------------------------------ 
    /// Calculate the world AABB rect of the sprite
    // ------------------------------------------------------------------ 

    public override Rect GetAABoundingRect () {
        Vector3[] vertices = GetVertices();
        Rect boundingRect = new Rect();
        boundingRect.x = vertices[0].x;
        boundingRect.y = vertices[0].y;
        for (int i = 1; i < vertexCount; ++i) {
            Vector3 vertex = vertices[i];
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

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override Vector3[] GetVertices () {
        List<Vector3> vertices = new List<Vector3>(vertexCount);    // TODO: use global static temp List instead
        for (int i = 0; i < vertexCount; ++i) {
            vertices.Add(new Vector3());
        }
        if (cachedTransform.hasChanged == false) {
            cachedWorldMatrix = cachedTransform_.localToWorldMatrix;
        }
        UpdateVertexBuffer(vertices, 0);
        return vertices.ToArray();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// Get the character rect at _idx
    /// \param _idx the index of the character
    /// \return the rect
    // ------------------------------------------------------------------ 

    //public Rect GetCharRect ( int _idx ) {
    //    if ( meshFilter ) {
    //        if ( meshFilter_.sharedMesh != null ) {

    //            // ======================================================== 
    //            // init value 
    //            // ======================================================== 

    //            int numVerts = text_.Length * 4;
    //            int vertexCount = 0;

    //            // first shadow
    //            if ( useShadow_ ) {
    //                vertexCount += numVerts;
    //            }

    //            // second outline
    //            if ( useOutline_ ) {
    //                vertexCount += 8 * numVerts;
    //            }

    //            // finally normal
    //            int vertexStartAt = vertexCount;
    //            vertexCount += numVerts;

    //            //
    //            int vert_id = vertexStartAt + 4 * _idx;
    //            Vector3[] verts = meshFilter_.sharedMesh.vertices;
    //            return new Rect ( verts[vert_id].x, 
    //                              verts[vert_id].y, 
    //                              verts[vert_id+3].x - verts[vert_id].x,
    //                              verts[vert_id+3].y - verts[vert_id].y );
    //        }
    //    }
    //    return new Rect ( 0.0f, 0.0f, 0.0f, 0.0f );
    //}

    //// ------------------------------------------------------------------ 
    ///// Set the character alpha
    ///// \param _idx the index of the character
    ///// \param _topColor the top color to set
    ///// \param _botColor the bot color to set
    ///// \param _alpha the alpha value to set
    //// ------------------------------------------------------------------ 

    //public void SetCharColor ( int _idx, Color _topColor, Color _botColor, float _alpha ) {
    //    if ( meshFilter ) {
    //        if ( meshFilter_.sharedMesh != null ) {

    //            // ======================================================== 
    //            // init value 
    //            // ======================================================== 

    //            int numVerts = text_.Length * 4;
    //            int vertexCount = 0;

    //            // first shadow
    //            int shadowVertexStartAt = -1;
    //            if ( useShadow_ ) {
    //                shadowVertexStartAt = vertexCount;
    //                vertexCount += numVerts;
    //            }

    //            // second outline
    //            int outlineVertexStartAt = -1;
    //            if ( useOutline_ ) {
    //                outlineVertexStartAt = vertexCount;
    //                vertexCount += 8 * numVerts;
    //            }

    //            // finally normal
    //            int vertexStartAt = vertexCount;
    //            vertexCount += numVerts;

    //            // ======================================================== 
    //            // Update Color
    //            // ======================================================== 

    //            Color[] colors = new Color[vertexCount];
    //            Color newTopColor = new Color( _topColor.r, _topColor.g, _topColor.b, _alpha );
    //            Color newBotColor = new Color( _botColor.r, _botColor.g, _botColor.b, _alpha );

    //            for ( int i = 0; i < text_.Length; ++i ) {
    //                Color clrTop = topColor_;
    //                Color clrBot = botColor_;
    //                Color clrOutline = outlineColor_;
    //                Color clrShadow = shadowColor_;
    //                if ( i == _idx ) {
    //                    clrTop = newTopColor;
    //                    clrBot = newBotColor;
    //                    clrOutline.a = _alpha;
    //                    clrShadow.a = _alpha;
    //                }

    //                int vert_id = vertexStartAt + 4 * i;
    //                colors[vert_id+0] = colors[vert_id+1] = clrTop;
    //                colors[vert_id+2] = colors[vert_id+3] = clrBot;


    //                if ( outlineVertexStartAt != -1 ) {
    //                    vert_id = 4 * i;
    //                    int[] vi = new int[] {
    //                        outlineVertexStartAt + vert_id + 0 * numVerts,
    //                        outlineVertexStartAt + vert_id + 1 * numVerts,
    //                        outlineVertexStartAt + vert_id + 2 * numVerts,
    //                        outlineVertexStartAt + vert_id + 3 * numVerts,
    //                        outlineVertexStartAt + vert_id + 4 * numVerts,
    //                        outlineVertexStartAt + vert_id + 5 * numVerts,
    //                        outlineVertexStartAt + vert_id + 6 * numVerts,
    //                        outlineVertexStartAt + vert_id + 7 * numVerts
    //                    };
    //                    for ( int k = 0; k < vi.Length; ++k ) {
    //                        colors[vi[k]+0] = 
    //                        colors[vi[k]+1] = 
    //                        colors[vi[k]+2] = 
    //                        colors[vi[k]+3] = clrOutline;
    //                    }
    //                }
    //                if ( shadowVertexStartAt != -1 ) {
    //                    vert_id = shadowVertexStartAt + 4 * i;
    //                    colors[vert_id+0] = 
    //                    colors[vert_id+1] = 
    //                    colors[vert_id+2] = 
    //                    colors[vert_id+3] = clrShadow;
    //                }
    //            }
    //            meshFilter_.sharedMesh.colors = colors;
    //        }
    //    }
    //} 

//    // ------------------------------------------------------------------ 
//    // Desc: 
//    // ------------------------------------------------------------------ 

//    public void CalculateSize ( out float[] _lineWidths,
//                                out float[] _kernings, 
//                                out float _halfWidthScaled,
//                                out float _halfHeightScaled,
//                                out float _offsetX,
//                                out float _offsetY )
//    {
//        if ( useMultiline_ ) {
//            long lines = exStringHelper.CountLinesInString(text_);
//            _lineWidths = new float[lines];
//        }
//        else {
//            _lineWidths = new float[0];
//        }
//        _kernings = new float[Mathf.Max(text_.Length-1,0)];
//        float maxWidth = 0.0f;
//        float curWidth = 0.0f;
//        float height = fontInfo_.lineHeight;

//        int curLine = 0;
//        for ( int i = 0; i < text_.Length; ++i ) {
//            char c = text_[i];
//            if ( c == '\n' ) {
//                if ( useMultiline_ ) {
//                    if ( curWidth > maxWidth ) {
//                        maxWidth = curWidth;
//                    }
//                    _lineWidths[curLine] = curWidth;
//                    curWidth = 0.0f;
//                    height = height + fontInfo_.lineHeight + lineSpacing_;
//                    ++curLine;
//                }
//                continue;
//            }

//            // if we don't have the character, it will become space.
//            exBitmapFont.CharInfo charInfo = fontInfo_.GetCharInfo(c);
//            if ( charInfo != null ) {
//                curWidth = curWidth + charInfo.xadvance + tracking_;
//                if ( useKerning_ ) {
//                    if ( i < text_.Length - 1 ) {
//                        for ( int idx = 0; idx < fontInfo_.kernings.Count; ++idx ) {
//                            exBitmapFont.KerningInfo k = fontInfo_.kernings[idx];
//                            if ( k.first == c && k.second == text_[i+1] ) {
//                                curWidth += k.amount;
//                                _kernings[i] = k.amount;
//                                break;
//                            }
//                        }
//                    }
//                }
//            }
//        }
//        if ( curWidth > maxWidth ) {
//            maxWidth = curWidth;
//        }
//        if ( useMultiline_ ) {
//            _lineWidths[curLine] = curWidth;
//        }

//        Vector2 finalScale = new Vector2 ( scale_.x * ppfScale_.x, scale_.y * ppfScale_.y );
//        _halfWidthScaled = maxWidth * finalScale.x * 0.5f;
//        _halfHeightScaled = height * finalScale.y * 0.5f;
//        _offsetX = 0.0f;
//        _offsetY = 0.0f;

//        // calculate anchor offset
//        switch ( anchor_ ) {
//        case Anchor.TopLeft     : _offsetX = -_halfWidthScaled;  _offsetY = -_halfHeightScaled; break;
//        case Anchor.TopCenter   : _offsetX = 0.0f;               _offsetY = -_halfHeightScaled; break;
//        case Anchor.TopRight    : _offsetX = _halfWidthScaled;   _offsetY = -_halfHeightScaled; break;

//        case Anchor.MidLeft     : _offsetX = -_halfWidthScaled;  _offsetY = 0.0f;               break;
//        case Anchor.MidCenter   : _offsetX = 0.0f;               _offsetY = 0.0f;               break;
//        case Anchor.MidRight    : _offsetX = _halfWidthScaled;   _offsetY = 0.0f;               break;

//        case Anchor.BotLeft     : _offsetX = -_halfWidthScaled;  _offsetY = _halfHeightScaled;  break;
//        case Anchor.BotCenter   : _offsetX = 0.0f;               _offsetY = _halfHeightScaled;  break;
//        case Anchor.BotRight    : _offsetX = _halfWidthScaled;   _offsetY = _halfHeightScaled;  break;

//        default                 : _offsetX = 0.0f;               _offsetY = 0.0f;               break;
//        }
//        _offsetX -= offset_.x;
//        _offsetY += offset_.y;
//    }

//    // ------------------------------------------------------------------ 
//    /// \param _mesh the mesh to update
//    /// 
//    /// Update the _mesh depends on the exPlane.updateFlags
//    // ------------------------------------------------------------------ 

//    public void UpdateMesh ( Mesh _mesh ) {



//        // ======================================================== 
//        // init value 
//        // ======================================================== 

//        int numVerts = text_.Length * 4;
//        int numIndices = text_.Length * 6;
//        int vertexCount = 0;
//        int indexCount = 0;

//        // first shadow
//        int shadowVertexStartAt = -1;
//        int shadowIndexStartAt = -1;
//        if ( useShadow_ ) {
//            shadowVertexStartAt = vertexCount;
//            vertexCount += numVerts;

//            shadowIndexStartAt = indexCount;
//            indexCount += numIndices;
//        }

//        // second outline
//        int outlineVertexStartAt = -1;
//        int outlineIndexStartAt = -1;
//        if ( useOutline_ ) {
//            outlineVertexStartAt = vertexCount;
//            vertexCount += 8 * numVerts;

//            outlineIndexStartAt = indexCount; 
//            indexCount += 8 * numIndices;
//        }

//        // finally normal
//        int vertexStartAt = vertexCount;
//        vertexCount += numVerts;

//        int indexStartAt = indexCount;
//        indexCount += numIndices;

//        // ======================================================== 
//        // Update Vertex, UV and Indices 
//        // ======================================================== 

//        

//        // ======================================================== 
//        // Update Vertex Only 
//        // ======================================================== 

//        else if ( (updateFlags & UpdateFlags.Vertex) != 0 ) {

//            float[] lineWidths;
//            float[] kernings;
//            float halfWidthScaled;
//            float halfHeightScaled;
//            float offsetX;
//            float offsetY;
//            CalculateSize ( out lineWidths,
//                            out kernings, 
//                            out halfWidthScaled,
//                            out halfHeightScaled,
//                            out offsetX,
//                            out offsetY );

//            //
//            Vector3[] vertices  = new Vector3[vertexCount];
//            Vector2 finalScale  = new Vector2 ( scale_.x * ppfScale_.x, scale_.y * ppfScale_.y );

//            //
//            int curLine = 0;
//            float curX = 0.0f;
//            if ( useMultiline_ ) {
//                switch ( textAlign_ ) {
//                case TextAlign.Left:
//                    curX = 0.0f;
//                    break;
//                case TextAlign.Center:
//                    curX = halfWidthScaled - lineWidths[curLine] * 0.5f * finalScale.x;
//                    break;
//                case TextAlign.Right:
//                    curX = halfWidthScaled * 2.0f - lineWidths[curLine] * finalScale.x;
//                    break;
//                }
//            }
//            float curY = 0.0f;
//            for ( int i = 0; i < text_.Length; ++i ) {
//                int id = text_[i];

//                // if next line
//                if ( id == '\n' ) {
//                    if ( useMultiline_ ) {
//                        ++curLine;
//                        switch ( textAlign_ ) {
//                        case TextAlign.Left:
//                            curX = 0.0f;
//                            break;
//                        case TextAlign.Center:
//                            curX = halfWidthScaled - lineWidths[curLine] * 0.5f * finalScale.x;
//                            break;
//                        case TextAlign.Right:
//                            curX = halfWidthScaled * 2.0f - lineWidths[curLine] * finalScale.x;
//                            break;
//                        }
//                        curY = curY + (fontInfo_.lineHeight + lineSpacing_) * finalScale.y;
//                    }
//                    continue;
//                }

//                int vert_id = vertexStartAt + 4 * i;
//                // if we don't have the character, it will become space.
//                exBitmapFont.CharInfo charInfo = fontInfo_.GetCharInfo(id);

//                if ( charInfo != null ) {
//                    // build vertices & normals
//                    for ( int r = 0; r < 2; ++r ) {
//                        for ( int c = 0; c < 2; ++c ) {
//                            int j = r * 2 + c;

//                            // calculate the base pos
//                            float x = curX - halfWidthScaled + c * charInfo.width * finalScale.x + charInfo.xoffset * finalScale.x;
//                            float y = -curY + halfHeightScaled - r * charInfo.height * finalScale.y - charInfo.yoffset * finalScale.y;

//                            // calculate the pos affect by anchor
//                            x -= offsetX;
//                            y += offsetY;

//                            // calculate the shear
//                            x += y * shear_.x;
//                            y += x * shear_.y;

//                            // build vertices
//                            vertices[vert_id+j] = new Vector3( x, y, 0.0f );
//                        }
//                    }

//                    //
//                    curX = curX + (charInfo.xadvance + tracking_) * finalScale.x;
//                    if ( useKerning_ ) {
//                        if ( i < text_.Length - 1 ) {
//                            curX += kernings[i] * finalScale.x;
//                        }
//                    }
//                }
//            }

//            // update outline
//            if ( useOutline_ ) {
//                UpdateOutline ( outlineVertexStartAt, 
//                                -1, 
//                                vertexStartAt,
//                                vertices, 
//                                null, 
//                                null );
//            }

//            // update shadow
//            if ( useShadow_ ) {
//                UpdateShadow ( shadowVertexStartAt, 
//                               -1, 
//                               vertexStartAt, 
//                               vertices, 
//                               null, 
//                               null );
//            }

//            _mesh.vertices = vertices;
//            _mesh.bounds = GetMeshBounds ( offsetX, offsetY, halfWidthScaled * 2.0f, halfHeightScaled * 2.0f );

//            // update collider if we have
//            UpdateBoundRect ( offsetX, offsetY, halfWidthScaled * 2.0f, halfHeightScaled * 2.0f );
//            if ( collisionHelper ) 
//                collisionHelper.UpdateCollider();

//// #if UNITY_EDITOR
////             _mesh.RecalculateBounds();
//// #endif
//        }

//        // ======================================================== 
//        // Update Color
//        // ======================================================== 

//        if ( (updateFlags & UpdateFlags.Color) != 0 ||
//             (updateFlags & UpdateFlags.Text) != 0 ) {
//            Color[] colors = new Color[vertexCount];
//            for ( int i = 0; i < text_.Length; ++i ) {
//                int vert_id = vertexStartAt + 4 * i;
//                colors[vert_id+0] = colors[vert_id+1] = topColor_;
//                colors[vert_id+2] = colors[vert_id+3] = botColor_;


//                if ( outlineVertexStartAt != -1 ) {
//                    vert_id = 4 * i;
//                    int[] vi = new int[] {
//                        outlineVertexStartAt + vert_id + 0 * numVerts,
//                        outlineVertexStartAt + vert_id + 1 * numVerts,
//                        outlineVertexStartAt + vert_id + 2 * numVerts,
//                        outlineVertexStartAt + vert_id + 3 * numVerts,
//                        outlineVertexStartAt + vert_id + 4 * numVerts,
//                        outlineVertexStartAt + vert_id + 5 * numVerts,
//                        outlineVertexStartAt + vert_id + 6 * numVerts,
//                        outlineVertexStartAt + vert_id + 7 * numVerts
//                    };
//                    for ( int k = 0; k < vi.Length; ++k ) {
//                        colors[vi[k]+0] = 
//                        colors[vi[k]+1] = 
//                        colors[vi[k]+2] = 
//                        colors[vi[k]+3] = outlineColor_;
//                    }
//                }
//                if ( shadowVertexStartAt != -1 ) {
//                    vert_id = shadowVertexStartAt + 4 * i;
//                    colors[vert_id+0] = 
//                    colors[vert_id+1] = 
//                    colors[vert_id+2] = 
//                    colors[vert_id+3] = shadowColor_;
//                }
//            }
//            _mesh.colors = colors;
//        }

//        // NOTE: though we set updateFlags to None at exPlane::LateUpdate, 
//        //       the Editor still need this or it will caused editor keep dirty
//        updateFlags = UpdateFlags.None;
//    }

//    // ------------------------------------------------------------------ 
//    /// \param _mesh the mesh to update
//    /// 
//    /// Force to update the _mesh use the Text flags in exPlane.UpdateFlags
//    // ------------------------------------------------------------------ 

//    public void ForceUpdateMesh ( Mesh _mesh ) {
//        if ( _mesh == null )
//            return;

//        _mesh.Clear();
//        updateFlags = UpdateFlags.Text | UpdateFlags.Color;
//        UpdateMesh( _mesh );
//    }

//    // ------------------------------------------------------------------ 
//    // Desc: 
//    // ------------------------------------------------------------------ 

//    public override void Commit () {
//        if ( meshFilter ) {
//            if ( meshFilter_.sharedMesh != null ) {
//                UpdateMesh (meshFilter_.sharedMesh);
//            }
//        }
//    }

    ///////////////////////////////////////////////////////////////////////////////
    // Internal Functions
    ///////////////////////////////////////////////////////////////////////////////

//    // ------------------------------------------------------------------ 
//    // Desc: 
//    // ------------------------------------------------------------------ 

//    void UpdateOutline ( int _vertexStartAt, 
//                         int _indexStartAt, 
//                         int _srcVertexStartAt,
//                         Vector3[] _vertices, 
//                         Vector2[] _uvs, 
//                         int[] _indices ) {

//        int numVerts = text_.Length * 4;
//        int numIndices = text_.Length * 6;
//        float length = Mathf.Sqrt(outlineWidth_*outlineWidth_*0.5f);

//        for ( int i = 0; i < text_.Length; ++i ) {
//            int vert_id = 4 * i;
//            int idx_id = 6 * i;

//            int[] vi = new int[] {
//                _vertexStartAt + vert_id + 0 * numVerts,
//                _vertexStartAt + vert_id + 1 * numVerts,
//                _vertexStartAt + vert_id + 2 * numVerts,
//                _vertexStartAt + vert_id + 3 * numVerts,
//                _vertexStartAt + vert_id + 4 * numVerts,
//                _vertexStartAt + vert_id + 5 * numVerts,
//                _vertexStartAt + vert_id + 6 * numVerts,
//                _vertexStartAt + vert_id + 7 * numVerts
//            };
//            int[] ii = new int[] {
//                _indexStartAt + idx_id + 0 * numIndices,
//                _indexStartAt + idx_id + 1 * numIndices,
//                _indexStartAt + idx_id + 2 * numIndices,
//                _indexStartAt + idx_id + 3 * numIndices,
//                _indexStartAt + idx_id + 4 * numIndices,
//                _indexStartAt + idx_id + 5 * numIndices,
//                _indexStartAt + idx_id + 6 * numIndices,
//                _indexStartAt + idx_id + 7 * numIndices
//            };

//            //
//            for ( int j = 0; j < 4; ++j ) {
//                int srcVertexID = _srcVertexStartAt + vert_id + j;

//                //
//                _vertices[vi[0] + j] = _vertices[srcVertexID] + new Vector3( -outlineWidth_, 0.0f, 0.0f );
//                _vertices[vi[1] + j] = _vertices[srcVertexID] + new Vector3(  outlineWidth_, 0.0f, 0.0f );
//                _vertices[vi[2] + j] = _vertices[srcVertexID] + new Vector3( 0.0f, -outlineWidth_, 0.0f );
//                _vertices[vi[3] + j] = _vertices[srcVertexID] + new Vector3( 0.0f,  outlineWidth_, 0.0f );

//                //
//                _vertices[vi[4] + j] = _vertices[srcVertexID] + new Vector3( -length, -length, 0.0f );
//                _vertices[vi[5] + j] = _vertices[srcVertexID] + new Vector3( -length,  length, 0.0f );
//                _vertices[vi[6] + j] = _vertices[srcVertexID] + new Vector3(  length,  length, 0.0f );
//                _vertices[vi[7] + j] = _vertices[srcVertexID] + new Vector3(  length, -length, 0.0f );

//                // build uv
//                if ( _uvs != null ) {
//                    for ( int k = 0; k < vi.Length; ++k ) {
//                        _uvs[vi[k] + j] = _uvs[srcVertexID];
//                    }
//                }
//            }

//            // build indices
//            if ( _indices != null ) {
//                for ( int k = 0; k < ii.Length; ++k ) {
//                    _indices[ii[k] + 0] = vi[k] + 0;
//                    _indices[ii[k] + 1] = vi[k] + 1;
//                    _indices[ii[k] + 2] = vi[k] + 2;
//                    _indices[ii[k] + 3] = vi[k] + 2;
//                    _indices[ii[k] + 4] = vi[k] + 1;
//                    _indices[ii[k] + 5] = vi[k] + 3;
//                }
//            }
//        }
//    }

//    // ------------------------------------------------------------------ 
//    // Desc: 
//    // ------------------------------------------------------------------ 

//    void UpdateShadow ( int _vertexStartAt, 
//                        int _indexStartAt, 
//                        int _srcVertexStartAt,
//                        Vector3[] _vertices, 
//                        Vector2[] _uvs, 
//                        int[] _indices ) {

//        for ( int i = 0; i < text_.Length; ++i ) {
//            int vert_id = 4 * i;
//            int idx_id = 6 * i;

//            int vi = _vertexStartAt + vert_id;
//            int ii = _indexStartAt + idx_id;

//            //
//            for ( int j = 0; j < 4; ++j ) {
//                int srcVertexID = _srcVertexStartAt + vert_id + j;

//                //
//                _vertices[vi + j] = _vertices[srcVertexID] + new Vector3( shadowBias_.x, shadowBias_.y, 0.0f );

//                // build uv
//                if ( _uvs != null ) {
//                    _uvs[vi + j] = _uvs[srcVertexID];
//                }
//            }

//            // build indices
//            if ( _indices != null ) {
//                _indices[ii + 0] = vi + 0;
//                _indices[ii + 1] = vi + 1;
//                _indices[ii + 2] = vi + 2;
//                _indices[ii + 3] = vi + 2;
//                _indices[ii + 4] = vi + 1;
//                _indices[ii + 5] = vi + 3;
//            }
//        }
//    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    // TODO: 如果要避免总是更新同一个mesh的其他sprite，就要避免面数反复增减。这时可以采用的优化方式是将冗余的所有vertex的color设为透明

    void UpdateVertexBuffer (List<Vector3> _vertices, int _startIndex) {
        if (font_ == null) {
            return;
        }
        for (int i = 0; i < text_; ++i) {
            exBitmapFont.CharInfo ci = font_.GetCharInfo(text_[i]);
            if (ci == null) {
                continue;
            }

        }
            //            float halfWidthScaled;
            //            float halfHeightScaled;
            //            float offsetX;
            //            float offsetY;
            //            CalculateSize ( out lineWidths,
            //                            out kernings, 
            //                            out halfWidthScaled,
            //                            out halfHeightScaled,
            //                            out offsetX,
            //                            out offsetY );

            //            //
            //            Vector3[] vertices  = new Vector3[vertexCount];
            //            Vector2[] uvs       = new Vector2[vertexCount];
            //            int[] indices       = new int[indexCount];
            //            Vector2 finalScale  = new Vector2 ( scale_.x * ppfScale_.x, scale_.y * ppfScale_.y );

            //            //
            //            int curLine = 0;
            //            float curX = 0.0f;
            //            if ( useMultiline_ ) {
            //                switch ( textAlign_ ) {
            //                case TextAlign.Left:
            //                    curX = 0.0f;
            //                    break;
            //                case TextAlign.Center:
            //                    curX = halfWidthScaled - lineWidths[curLine] * 0.5f * finalScale.x;
            //                    break;
            //                case TextAlign.Right:
            //                    curX = halfWidthScaled * 2.0f - lineWidths[curLine] * finalScale.x;
            //                    break;
            //                }
            //            }
            //            float curY = 0.0f;
            //            for ( int i = 0; i < text_.Length; ++i ) {
            //                int id = text_[i];

            //                // if next line
            //                if ( id == '\n' ) {
            //                    if ( useMultiline_ ) {
            //                        ++curLine;
            //                        switch ( textAlign_ ) {
            //                        case TextAlign.Left:
            //                            curX = 0.0f;
            //                            break;
            //                        case TextAlign.Center:
            //                            curX = halfWidthScaled - lineWidths[curLine] * 0.5f * finalScale.x;
            //                            break;
            //                        case TextAlign.Right:
            //                            curX = halfWidthScaled * 2.0f - lineWidths[curLine] * finalScale.x;
            //                            break;
            //                        }
            //                        curY = curY + (fontInfo_.lineHeight + lineSpacing_) * finalScale.y;
            //                    }
            //                    continue;
            //                }

            //                int vert_id = vertexStartAt + 4 * i;
            //                int idx_id = indexStartAt + 6 * i;
            //                // if we don't have the character, it will become space.
            //                exBitmapFont.CharInfo charInfo = fontInfo_.GetCharInfo(id);

            //                //
            //                if ( charInfo != null ) {
            //                    // build vertices & normals
            //                    for ( int r = 0; r < 2; ++r ) {
            //                        for ( int c = 0; c < 2; ++c ) {
            //                            int j = r * 2 + c;

            //                            // calculate the base pos
            //                            float x = curX - halfWidthScaled + c * charInfo.width * finalScale.x + charInfo.xoffset * finalScale.x;
            //                            float y = -curY + halfHeightScaled - r * charInfo.height * finalScale.y - charInfo.yoffset * finalScale.y;

            //                            // calculate the pos affect by anchor
            //                            x -= offsetX;
            //                            y += offsetY;

            //                            // calculate the shear
            //                            float old_x = x;
            //                            x += y * shear_.x;
            //                            y += old_x * shear_.y;

            //                            // build vertices and normals
            //                            vertices[vert_id+j] = new Vector3( x, y, 0.0f );
            //                            // normals[vert_id+j] = new Vector3( 0.0f, 0.0f, -1.0f );
            //                        }
            //                    }

            //                    // build uv
            //                    float textureWidth = fontInfo_.pageInfos[0].texture.width;
            //                    float textureHeight = fontInfo_.pageInfos[0].texture.height;
            //                    float charUVWidth = (float)charInfo.width/(float)textureWidth;
            //                    float charUVHeight = (float)charInfo.height/(float)textureHeight;

            //                    float xStart  = charInfo.uv0.x;
            //                    float yStart  = charInfo.uv0.y;
            //                    float xEnd    = xStart + charUVWidth; 
            //                    float yEnd    = yStart + charUVHeight; 

            //                    //
            //                    uvs[vert_id + 0] = new Vector2 ( xStart,  yEnd );
            //                    uvs[vert_id + 1] = new Vector2 ( xEnd,    yEnd );
            //                    uvs[vert_id + 2] = new Vector2 ( xStart,  yStart );
            //                    uvs[vert_id + 3] = new Vector2 ( xEnd,    yStart );

            //                    // build indices
            //                    indices[idx_id + 0] = vert_id + 0;
            //                    indices[idx_id + 1] = vert_id + 1;
            //                    indices[idx_id + 2] = vert_id + 2;
            //                    indices[idx_id + 3] = vert_id + 2;
            //                    indices[idx_id + 4] = vert_id + 1;
            //                    indices[idx_id + 5] = vert_id + 3;

            //                    //
            //                    curX = curX + (charInfo.xadvance + tracking_) * finalScale.x;
            //                    if ( useKerning_ ) {
            //                        if ( i < text_.Length - 1 ) {
            //                            curX += kernings[i] * finalScale.x;
            //                        }
            //                    }
            //                }
            //            }





        switch ( anchor_ ) {
        case Anchor.TopLeft     : anchorOffsetX = halfWidth;   anchorOffsetY = -halfHeight;  break;
        case Anchor.TopCenter   : anchorOffsetX = 0.0f;        anchorOffsetY = -halfHeight;  break;
        case Anchor.TopRight    : anchorOffsetX = -halfWidth;  anchorOffsetY = -halfHeight;  break;

        case Anchor.MidLeft     : anchorOffsetX = halfWidth;   anchorOffsetY = 0.0f;         break;
        case Anchor.MidCenter   : anchorOffsetX = 0.0f;        anchorOffsetY = 0.0f;         break;
        case Anchor.MidRight    : anchorOffsetX = -halfWidth;  anchorOffsetY = 0.0f;         break;

        case Anchor.BotLeft     : anchorOffsetX = halfWidth;   anchorOffsetY = halfHeight;   break;
        case Anchor.BotCenter   : anchorOffsetX = 0.0f;        anchorOffsetY = halfHeight;   break;
        case Anchor.BotRight    : anchorOffsetX = -halfWidth;  anchorOffsetY = halfHeight;   break;

        default                 : anchorOffsetX = 0.0f;        anchorOffsetY = 0.0f;         break;
        }

        anchorOffsetX += offset_.x;
        anchorOffsetY += offset_.y;

        //v1 v2
        //v0 v3
        exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
        Vector3 v0 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(-halfWidth + anchorOffsetX, -halfHeight + anchorOffsetY, 0.0f));
        Vector3 v1 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(-halfWidth + anchorOffsetX, halfHeight + anchorOffsetY, 0.0f));
        Vector3 v2 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(halfWidth + anchorOffsetX, halfHeight + anchorOffsetY, 0.0f));
        Vector3 v3 = cachedWorldMatrix.MultiplyPoint3x4(new Vector3(halfWidth + anchorOffsetX, -halfHeight + anchorOffsetY, 0.0f));

        // 将z都设为0，使mesh所有mesh的厚度都为0，这样在mesh进行深度排序时会方便一些。但是不能用于3D Sprite
        v0.z = 0;
        v1.z = 0;
        v2.z = 0;
        v3.z = 0;

        if (shear_.x != 0) {
            // 这里直接从matrix拿未计入rotation影响的scale，在已知matrix的情况下，速度比较快lossyScale了6倍。
            // 在有rotation时，shear本来就会有冲突，所以这里不需要lossyScale。
            float worldScaleY = (new Vector3(cachedWorldMatrix.m01, cachedWorldMatrix.m11, cachedWorldMatrix.m21)).magnitude;
            float offsetX = worldScaleY * shear_.x;
            float topOffset = offsetX * (halfHeight + anchorOffsetY);
            float botOffset = offsetX * (-halfHeight + anchorOffsetY);
            v0.x += botOffset;
            v1.x += topOffset;
            v2.x += topOffset;
            v3.x += botOffset;
        }
        if (shear_.y != 0) {
            float worldScaleX = (new Vector3(cachedWorldMatrix.m00, cachedWorldMatrix.m10, cachedWorldMatrix.m20)).magnitude;
            float offsetY = worldScaleX * shear_.y;
            float leftOffset = offsetY * (-halfWidth + anchorOffsetX);
            float rightOffset = offsetY * (halfWidth + anchorOffsetX);
            v0.y += leftOffset;
            v1.y += leftOffset;
            v2.y += rightOffset;
            v3.y += rightOffset;
        }

        _vertices[_startIndex + 0] = v0;
        _vertices[_startIndex + 1] = v1;
        _vertices[_startIndex + 2] = v2;
        _vertices[_startIndex + 3] = v3;
        
        // TODO: pixel-perfect
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateCapacity () {
        // TODO: check multiline
        int textLength = text_.Length;
        int oldTextCapaticy = vertexCountCapacity / exMesh.QUAD_VERTEX_COUNT;
        int textCapaticy = oldTextCapaticy;
        // append
        while (textLength > textCapaticy) {
            textCapaticy <<= 1;
        }
        // trim
        while (textLength < textCapaticy / 2) {
            textCapaticy >>= 1;
        }
        if (textCapaticy != oldTextCapaticy) {
            if (layer_ != null) {
                // remove from layer
                exLayer myLayer = layer_;
                myLayer.Remove(this);
                // change capacity
                vertexCountCapacity = textCapaticy * exMesh.QUAD_VERTEX_COUNT;
                indexCountCapacity = textCapaticy * exMesh.QUAD_INDEX_COUNT;
                // readd to layer
                myLayer.Add(this);
            }
        }
    }
}

#endif