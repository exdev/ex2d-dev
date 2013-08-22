// ======================================================================================
// File         : exSpriteFont.cs
// Author       : 
// Last Change  : 08/17/2013 | 15:27:56
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
/// \class exSpriteFont
/// 
/// A component to render exBitmapFont in the layer 
/// 
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/ex2D Sprite Font")]
public class exSpriteFont : exSpriteBase {
    
    // ------------------------------------------------------------------ 
    /// The type of font effect
    // ------------------------------------------------------------------ 

    public enum OutlineType {
        Outline4 = 1,   ///< up down left right
        Outline4X,      ///< top-left top-right bottom-left bottom-right
        Outline8,       ///< 
    };

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
            if (ReferenceEquals(font_, value)) {
                return;
            }
            if (value != null) {
                if (value.texture == null) {
                    Debug.LogWarning("invalid font texture");
                }
                updateFlags |= exUpdateFlags.Text;

                if (font_ == null || ReferenceEquals(font_.texture, value.texture) == false) {
                    // texture changed
                    font_ = value;
                    UpdateMaterial();
                    return;
                }
                if (layer_ != null && isOnEnabled_ && visible == false) {
                    font_ = value;
                    if (visible) {
                        // become visible
                        if (enableFastShowHide) {
                            layer_.FastShowSprite(this);
                        }
                        else {
                            layer_.ShowSprite(this);
                        }
                    }
                }
            }
            else if (layer_ != null && visible) {
                // become invisible
                if (enableFastShowHide) {
                    layer_.FastHideSprite(this);
                }
                else {
                    layer_.HideSprite(this);
                }
            }
            font_ = value;

#if UNITY_EDITOR
            if (layer_ != null) {
                layer_.UpdateNowInEditMode();
            }
#endif
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
            if (text_ != value) {
                string oldText = text_;
                text_ = value;
                if (text_.Length >= exMesh.MAX_QUAD_COUNT) {
                    text_ = text_.Substring(0, exMesh.MAX_QUAD_COUNT);
                    Debug.LogError("Too many character on one sprite: " + value.Length, this);
                }
                // TODO: check multiline
                if (oldText.Length != text_.Length) {
                    UpdateCapacity();   // TODO: 如果在一帧内反复修改文本，会造成多余的layer改动，考虑留到update时再处理
                }
                updateFlags |= exUpdateFlags.Text;
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    //// ------------------------------------------------------------------ 
    //[SerializeField] protected bool useMultiline_ = false;
    ///// If useMultiline is true, the exSpriteFont.text accept multiline string. 
    //// ------------------------------------------------------------------ 

    //public bool useMultiline {
    //    get { return useMultiline_; }
    //    set {
    //        if ( useMultiline_ != value ) {
    //            useMultiline_ = value;
    //            updateFlags |= exUpdateFlags.Text;  // TODO: only need to update vertex ?
    //        }
    //    }
    //}

    // ------------------------------------------------------------------ 
    [SerializeField] protected TextAlignment textAlign_ = TextAlignment.Left;
    /// The alignment method used in the text
    // ------------------------------------------------------------------ 

    public TextAlignment textAlign {
        get { return textAlign_; }
        set {
            if (textAlign_ != value) {
                textAlign_ = value;
                updateFlags |= exUpdateFlags.Vertex;
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
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
            if (useKerning_ != value) {
                useKerning_ = value;
                updateFlags |= exUpdateFlags.Vertex;
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
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
            if (spacing_ != value) {
                spacing_ = value;
                updateFlags |= exUpdateFlags.Vertex;
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    // color option

    // ------------------------------------------------------------------ 
    [SerializeField] protected Color topColor_ = Color.white;
    // TODO: use gradient
    /// the color of the vertices at top 
    // ------------------------------------------------------------------ 
    
    public Color topColor {
        get { return topColor_; }
        set {
            if (topColor_ != value) {
                topColor_ = value;
                updateFlags |= exUpdateFlags.Color;
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
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
            if (botColor_ != value) {
                botColor_ = value;
                updateFlags |= exUpdateFlags.Color;
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    // outline option

    // ------------------------------------------------------------------ 
    [SerializeField] protected bool useOutline_ = false;
    /// If useOutline is true, the component will render the text with outline
    // ------------------------------------------------------------------ 

    public bool useOutline {
        get { return useOutline_; }
        set {
            if (useOutline_ != value) {
                useOutline_ = value;
                updateFlags |= exUpdateFlags.Text; 
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected float outlineWidth_ = 1.0f;
    /// The width of the outline text
    // ------------------------------------------------------------------ 

    public float outlineWidth {
        get { return outlineWidth_; }
        set {
            if (outlineWidth_ != value) {
                outlineWidth_ = value;
                if (useOutline_) {
                    updateFlags |= exUpdateFlags.Vertex;
                }
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Color outlineColor_ = Color.black;
    /// The color of the outline text
    // ------------------------------------------------------------------ 

    public Color outlineColor {
        get { return outlineColor_; }
        set {
            if (outlineColor_ != value) {
                outlineColor_ = value;
                if (useOutline_) {
                    updateFlags |= exUpdateFlags.Color;
                }
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    // shadow option

    // ------------------------------------------------------------------ 
    [SerializeField] protected bool useShadow_ = false;
    /// If useShadow is true, the component will render the text with shadow
    // ------------------------------------------------------------------ 

    public bool useShadow {
        get { return useShadow_; }
        set {
            if (useShadow_ != value) {
                useShadow_ = value;
                updateFlags |= exUpdateFlags.Text; 
#if UNITY_EDITOR
                if (layer_ != null) {
                    layer_.UpdateNowInEditMode();
                }
#endif
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Vector2 shadowBias_ = new Vector2(1.0f, -1.0f);
    /// The bias of the shadow text 
    // ------------------------------------------------------------------ 

    public Vector2 shadowBias {
        get { return shadowBias_; }
        set {
            if (shadowBias_ != value) {
                shadowBias_ = value;
                if (useShadow_) {
                    updateFlags |= exUpdateFlags.Vertex;
#if UNITY_EDITOR
                    if (layer_ != null) {
                        layer_.UpdateNowInEditMode();
                    }
#endif
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected Color shadowColor_ = Color.black;
    /// The color of the shadow text 
    // ------------------------------------------------------------------ 

    public Color shadowColor {
        get { return shadowColor_; }
        set {
            if (shadowColor_ != value) {
                shadowColor_ = value;
                if (useShadow_) {
                    updateFlags |= exUpdateFlags.Color;
#if UNITY_EDITOR
                    if (layer_ != null) {
                        layer_.UpdateNowInEditMode();
                    }
#endif
                }
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// 返回capacity的顶点数量，实际渲染的字符顶点数可能少于这个值。
    // ------------------------------------------------------------------ 

    public override int vertexCount {
        get {
            //if (layer_ == null) {
            //    UpdateCapacity();
            //}
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

    public override bool visible {
        get {
            return isOnEnabled_ && font_ != null && font_.texture != null && font_.charInfos.Count > 0;
        }
    }

    [System.NonSerialized] private int vertexCountCapacity = 0;
    [System.NonSerialized] private int indexCountCapacity = 0;
    [System.NonSerialized] private bool lockCapacity = false;
    /*
    ///////////////////////////////////////////////////////////////////////////////
    // geometry buffers
    ///////////////////////////////////////////////////////////////////////////////
    
    [System.NonSerialized] private List<Vector3> vertices = new List<Vector3>();  ///< 按文本顺序排列
    [System.NonSerialized] private List<Vector2> uvs = new List<Vector2>();       ///< 按文本顺序排列
    [System.NonSerialized] private List<Color32> colors32 = new List<Color32>();  ///< 按文本顺序排列

    /// 不需要按顺序排列，面片数量和文本数量保持一致即可
    [System.NonSerialized] private List<int> indices = new List<int>(); 
    */
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////

    #region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    /// Add sprite's geometry data to buffers
    // ------------------------------------------------------------------ 

    internal override void FillBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32) {
        if (layer_ == null) {
            UpdateCapacity();
        }
        base.FillBuffers(_vertices, _uvs, _colors32);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices) {
#if UNITY_EDITOR
        if (text_ == null || vertexCountCapacity < text_.Length * exMesh.QUAD_VERTEX_COUNT) {
            Debug.LogError("[UpdateBuffers|exSpriteFont] 顶点缓冲长度不够，是否绕开属性直接修改了text_?: " + vertexCountCapacity, this);
            return updateFlags;
        }
#endif
        if ((updateFlags & exUpdateFlags.Text) != 0) {
            exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
            BuildText(_vertices, vertexBufferIndex, ref cachedWorldMatrix, _uvs);
            updateFlags |= (exUpdateFlags.Vertex | exUpdateFlags.UV | exUpdateFlags.Color);
        }
        else if ((updateFlags & exUpdateFlags.Vertex) != 0) {
            exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
            BuildText(_vertices, vertexBufferIndex, ref cachedWorldMatrix);
        }
        if ((updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
            // update index buffer
            int indexBufferEnd = indexBufferIndex + indexCountCapacity - 5;
            for (int i = indexBufferIndex, v = vertexBufferIndex; i < indexBufferEnd; i += 6, v += 4) {
                _indices.buffer[i] = v;
                _indices.buffer[i + 1] = v + 1;
                _indices.buffer[i + 2] = v + 2;
                _indices.buffer[i + 3] = v + 2;
                _indices.buffer[i + 4] = v + 3;
                _indices.buffer[i + 5] = v;
            }
        }
        if ((updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null) {
            Color32 top, bot;
            if (transparent_ == false) {
                top = new Color (topColor_.r, topColor_.g, topColor_.b, topColor_.a * layer_.alpha);
                bot = new Color (botColor_.r, botColor_.g, botColor_.b, botColor_.a * layer_.alpha);
            }
            else {
                top = new Color32 ();
                bot = new Color32 ();
            }
            int vertexBufferEnd = vertexBufferIndex + text_.Length * 4;
            for (int i = vertexBufferIndex; i < vertexBufferEnd; i += 4) {
                _colors32.buffer[i + 0] = bot;
                _colors32.buffer[i + 1] = top;
                _colors32.buffer[i + 2] = top;
                _colors32.buffer[i + 3] = bot;
            }
        }
        exUpdateFlags updatedFlags = updateFlags;
        updateFlags = exUpdateFlags.None;
        return updatedFlags;
    }

    #endregion  // Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (ref Matrix4x4 _spriteMatrix) {
        // TODO: only return the rotated bounding box of the sprite font
        int visibleVertexCount = text_.Length * 4;
        exList<Vector3> vertices = exList<Vector3>.GetTempList();
        vertices.AddRange(visibleVertexCount);
        BuildText(vertices, 0, ref _spriteMatrix);
        return vertices.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected override void OnPreAddToLayer () {
        exDebug.Assert(layer_ == null);
        if (layer_ == null) {
            UpdateCapacity();
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

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
    
    void BuildText (exList<Vector3> _vertices, int _vbIndex, ref Matrix4x4 _spriteMatrix, exList<Vector2> _uvs = null) {
        width_ = 0.0f;    // 和SpriteBase一致，用于表示实际宽度
        height_ = 0.0f;   // 和SpriteBase一致，用于表示实际高度
        int invisibleVertexStart = -1;
        int visibleVertexCount;
        if (font_ != null) {
            BuildTextInLocalSpace(_vertices, _vbIndex, _uvs);
            visibleVertexCount = text_.Length * 4;
        }
        else {
            //System.Array.Clear(_vertices.buffer, _vbIndex, visibleVertexCount);
            visibleVertexCount = 0;
        }
        if (vertexCountCapacity > visibleVertexCount) {
            invisibleVertexStart = visibleVertexCount;
            _vertices.buffer[invisibleVertexStart + 0] = new Vector3();
            _vertices.buffer[invisibleVertexStart + 1] = new Vector3();
            _vertices.buffer[invisibleVertexStart + 2] = new Vector3();
            _vertices.buffer[invisibleVertexStart + 3] = new Vector3();
        }

        // calculate anchor and offset
        float anchorOffsetX = 0.0f;
        float anchorOffsetY;
        // convert to top left
        switch (textAlign_) {
        case TextAlignment.Left:
            break;
        case TextAlignment.Center:
            anchorOffsetX = width_ * 0.5f;
            break;
        case TextAlignment.Right:
            anchorOffsetX = width_;
            break;
        }
        // convert anchor from top center to user defined
        switch ( anchor_ ) {
        case Anchor.TopLeft   :                                   anchorOffsetY = 0.0f;           break;
        case Anchor.TopCenter : anchorOffsetX -= (width_ * 0.5f); anchorOffsetY = 0.0f;           break;
        case Anchor.TopRight  : anchorOffsetX -= width_;          anchorOffsetY = 0.0f;           break;
        case Anchor.MidLeft   :                                   anchorOffsetY = height_ * 0.5f; break;
        case Anchor.MidCenter : anchorOffsetX -= (width_ * 0.5f); anchorOffsetY = height_ * 0.5f; break;
        case Anchor.MidRight  : anchorOffsetX -= width_;          anchorOffsetY = height_ * 0.5f; break;
        case Anchor.BotLeft   :                                   anchorOffsetY = height_;        break;
        case Anchor.BotCenter : anchorOffsetX -= (width_ * 0.5f); anchorOffsetY = height_;        break;
        case Anchor.BotRight  : anchorOffsetX -= width_;          anchorOffsetY = height_;        break;
        default               : anchorOffsetX -= (width_ * 0.5f); anchorOffsetY = height_ * 0.5f; break;
        }
        // offset
        anchorOffsetX += offset_.x;
        anchorOffsetY += offset_.y;

        float shearOffsetX = 0.0f;
        float shearOffsetY = 0.0f;
        if (shear_.x != 0) {
            float worldScaleY = (new Vector3(_spriteMatrix.m01, _spriteMatrix.m11, _spriteMatrix.m21)).magnitude;
            shearOffsetX = worldScaleY * shear_.x;
        }
        if (shear_.y != 0) {
            float worldScaleX = (new Vector3(_spriteMatrix.m00, _spriteMatrix.m10, _spriteMatrix.m20)).magnitude;
            shearOffsetY = worldScaleX * shear_.y;
        }
        int vbEnd = _vbIndex + vertexCountCapacity;
        for (int i = _vbIndex; i < vbEnd; i += 4) {
            if (invisibleVertexStart == -1 || i <= invisibleVertexStart) {
                Vector3 v0 = _vertices.buffer[i + 0];
                Vector3 v1 = _vertices.buffer[i + 1];
                Vector3 v2 = _vertices.buffer[i + 2];
                Vector3 v3 = _vertices.buffer[i + 3];
                // apply anchor and offset
                v0.x += anchorOffsetX; v0.y += anchorOffsetY;
                v1.x += anchorOffsetX; v1.y += anchorOffsetY;
                v2.x += anchorOffsetX; v2.y += anchorOffsetY;
                v3.x += anchorOffsetX; v3.y += anchorOffsetY;
                // apply transform
                v0 = _spriteMatrix.MultiplyPoint3x4(v0);
                v1 = _spriteMatrix.MultiplyPoint3x4(v1);
                v2 = _spriteMatrix.MultiplyPoint3x4(v2);
                v3 = _spriteMatrix.MultiplyPoint3x4(v3);

                v0.z = 0; v1.z = 0; v2.z = 0; v3.z = 0;
                // shear
                if (shear_.x != 0) {
                    float halfCharHeight = (v1.y - v0.y) * 0.5f;
                    float topOffset = shearOffsetX * (halfCharHeight + anchorOffsetY);
                    float botOffset = shearOffsetX * (-halfCharHeight + anchorOffsetY);
                    v0.x += botOffset;
                    v1.x += topOffset;
                    v2.x += topOffset;
                    v3.x += botOffset;
                }
                if (shear_.y != 0) {
                    float halfCharWidth = (v2.y - v0.y) * 0.5f;
                    float leftOffset = shearOffsetY * (-halfCharWidth + anchorOffsetX);
                    float rightOffset = shearOffsetY * (halfCharWidth + anchorOffsetX);
                    v0.y += leftOffset;
                    v1.y += leftOffset;
                    v2.y += rightOffset;
                    v3.y += rightOffset;
                }
                _vertices.buffer[i + 0] = v0;
                _vertices.buffer[i + 1] = v1;
                _vertices.buffer[i + 2] = v2;
                _vertices.buffer[i + 3] = v3;
            }
            else if (invisibleVertexStart != -1) {
                if (i >= _vertices.buffer.Length) {
                    Debug.Log(string.Format("[BuildText|exSpriteFont] i: {0} vbEnd: " + vbEnd + " _vertices: " + _vertices.buffer.Length, i));
                }
                _vertices.buffer[i + 0] = _vertices.buffer[invisibleVertexStart];
                _vertices.buffer[i + 1] = _vertices.buffer[invisibleVertexStart];
                _vertices.buffer[i + 2] = _vertices.buffer[invisibleVertexStart];
                _vertices.buffer[i + 3] = _vertices.buffer[invisibleVertexStart];
            }
        }
        // TODO: pixel-perfect
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 
    
    private void BuildTextInLocalSpace (exList<Vector3> _vertices, int _vbIndex, exList<Vector2> _uvs) {
        Vector2 texelSize = new Vector2();
        if (_uvs != null && font_.texture != null) {
            texelSize = font_.texture.texelSize;
        }

        int parsedVBIndex = _vbIndex;
        for (int charIndex = 0; charIndex < text_.Length; ) {
            int lineStart = parsedVBIndex;
            // build line
            float lineWidth = BuildLine(_vertices, _uvs, ref charIndex, ref parsedVBIndex, texelSize, -height_);
            // text alignment
            switch (textAlign_) {
            case TextAlignment.Left:
                // convert to top left
                break;
            case TextAlignment.Center:
                // convert to top center
                float halfLineWidth = lineWidth * 0.5f;
                for (int i = lineStart; i < parsedVBIndex; ++i) {
                    _vertices.buffer[i].x -= halfLineWidth;
                }
                break;
            case TextAlignment.Right:
                // convert to top right
                for (int i = lineStart; i < parsedVBIndex; ++i) {
                    _vertices.buffer[i].x -= lineWidth;
                }
                break;
            }
            // update width and height
            if (lineWidth > width) {
                width_ = lineWidth;
            }
            height_ += font_.lineHeight;
            if (charIndex < text_.Length) {
                height_ += spacing_.y;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    float BuildLine (exList<Vector3> _vertices, exList<Vector2> _uvs, ref int _charIndex, ref int _vbIndex, Vector2 _texelSize, float _top) {
        // TODO: cache vertices, only transform them if text not changed.
        int firstChar = _charIndex;
        float curX = 0.0f;
        float lastAdvance = 0.0f;
        float lastWidth = 0.0f;
        for (; _charIndex < text_.Length; ++_charIndex, _vbIndex += 4, curX += spacing_.x) {
            char c = text_[_charIndex];

            // if new line  // TODO: auto wrap
            if (c == '\n') {
                _vertices.buffer[_vbIndex + 0] = new Vector3();
                _vertices.buffer[_vbIndex + 1] = new Vector3();
                _vertices.buffer[_vbIndex + 2] = new Vector3();
                _vertices.buffer[_vbIndex + 3] = new Vector3();
                ++_charIndex;
                _vbIndex += 4;
                break;
            }

            if (_charIndex > firstChar) {   // if has previous character
                curX += lastAdvance;
                // kerning
                if (useKerning_) {
                    curX += font_.GetKerning(text_[_charIndex - 1], c);
                }
            }

            exBitmapFont.CharInfo ci = font_.GetCharInfo(c);
            if (ci == null) {
                // character is not present, it will not display
                // Debug.Log("character is not present: " + c, this);
                _vertices.buffer[_vbIndex + 0] = new Vector3();
                _vertices.buffer[_vbIndex + 1] = new Vector3();
                _vertices.buffer[_vbIndex + 2] = new Vector3();
                _vertices.buffer[_vbIndex + 3] = new Vector3();
                continue;
            }

            // build text vertices
            float x = curX + ci.xoffset;
            float y = _top - ci.yoffset;
            _vertices.buffer[_vbIndex + 0] = new Vector3(x, y - ci.height, 0.0f);
            _vertices.buffer[_vbIndex + 1] = new Vector3(x, y, 0.0f);
            _vertices.buffer[_vbIndex + 2] = new Vector3(x + ci.width, y, 0.0f);
            _vertices.buffer[_vbIndex + 3] = new Vector3(x + ci.width, y - ci.height, 0.0f);

            lastWidth = ci.width;
            lastAdvance = ci.xadvance;

            // build uv
            if (_uvs != null) {
                Vector2 start = new Vector2(ci.x * _texelSize.x, ci.y * _texelSize.y);
                Vector2 end = new Vector2((ci.x + ci.rotatedWidth) * _texelSize.x, (ci.y + ci.rotatedHeight) * _texelSize.y);
                if (ci.rotated) {
                    _uvs.buffer[_vbIndex + 0] = new Vector2(end.x, start.y);
                    _uvs.buffer[_vbIndex + 1] = start;
                    _uvs.buffer[_vbIndex + 2] = new Vector2(start.x, end.y);
                    _uvs.buffer[_vbIndex + 3] = end;
                }
                else {
                    _uvs.buffer[_vbIndex + 0] = start;
                    _uvs.buffer[_vbIndex + 1] = new Vector2(start.x, end.y);
                    _uvs.buffer[_vbIndex + 2] = end;
                    _uvs.buffer[_vbIndex + 3] = new Vector2(end.x, start.y);
                }
            }
        }
        return curX + lastWidth;
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    int GetTextCapacity (int _oldTextCapacity) {
        if (text_ == null) {
            return 0;
        }
        // TODO: check multiline
        int textLength = text_.Length;
        //            if (useShadow_) {
        //                textLength += text_.Length;
        //            }
        //            if (useOutline_) {
        //                textLength += (text_.Length * 4);
        //            }
        exDebug.Assert (textLength <= exMesh.MAX_QUAD_COUNT);
        int textCapacity = _oldTextCapacity;
#if UNITY_EDITOR && !EX_DEBUG
        if (UnityEditor.EditorApplication.isPlaying == false) {
            textCapacity = textLength;
        }
        else
#endif
        if (layer_ != null && layer_.layerType == exLayerType.Dynamic) {
            if (textLength > textCapacity) {
                // append
                textCapacity = Mathf.Max(textCapacity, 1);
                while (textLength > textCapacity) {
                    textCapacity <<= 1;
                }
            }
            else {
                // trim
                while (textLength < textCapacity / 2 && textCapacity > 4) {
                    textCapacity >>= 1;
                }
                return textCapacity;
            }
        }
        else {
            textCapacity = textLength;
        }
        if (textCapacity > exMesh.MAX_QUAD_COUNT) {
            textCapacity = exMesh.MAX_QUAD_COUNT;
        }
        return textCapacity;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateCapacity () {
        if (lockCapacity) {
            exDebug.Assert(text_ == null || vertexCountCapacity >= text_.Length * exMesh.QUAD_VERTEX_COUNT);
            return;
        }
        int oldTextCapacity = vertexCountCapacity / exMesh.QUAD_VERTEX_COUNT;
        int textCapacity = GetTextCapacity(oldTextCapacity);
        
        if (textCapacity != oldTextCapacity) {
            if (layer_ != null) {
                // remove from layer
                exLayer myLayer = layer_;
                myLayer.Remove(this);
                // change capacity
                vertexCountCapacity = textCapacity * exMesh.QUAD_VERTEX_COUNT;
                indexCountCapacity = textCapacity * exMesh.QUAD_INDEX_COUNT;
                // re-add to layer
                lockCapacity = true;
                myLayer.Add(this);
                //Debug.Log("Update Capacity");
                lockCapacity = false;
            }
            else {
                vertexCountCapacity = textCapacity * exMesh.QUAD_VERTEX_COUNT;
                indexCountCapacity = textCapacity * exMesh.QUAD_INDEX_COUNT;
            }
        }
    }
}
