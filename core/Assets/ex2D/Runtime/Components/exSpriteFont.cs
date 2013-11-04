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
using ex2D.Detail;

// ------------------------------------------------------------------ 
/// The type of font effect
// ------------------------------------------------------------------ 

public enum exOutlineType {
    Outline4 = 1,   ///< up down left right
    Outline4X,      ///< top-left top-right bottom-left bottom-right
    Outline8,       ///< 
};

///////////////////////////////////////////////////////////////////////////////
/// 
/// A component to render exFont in the layer 
/// 
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/2D Sprite Font")]
public class exSpriteFont : exLayeredSprite {

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////

    /// 每个exSpriteFont都有单独的一个exFont实例
    [SerializeField] protected exFont font_ = new exFont();
    
    public exBitmapFont bitmapFont {
        get {
            return font_.bitmapFont;
        }
    }

    public Font dynamicFont {
        get {
            return font_.dynamicFont;
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
            }
        }
    }

/*    public int lineHeight {
        get {
            return font_.lineHeight;
        }
        set {
            if (font_.lineHeight != value) {
                font_.lineHeight = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }*/

    public int fontSize {
        get {
            return font_.fontSize;
        }
        set {
            if (font_.fontSize != value) {
                font_.fontSize = value;
                updateFlags |= exUpdateFlags.Text;
            }
        }
    }

    public FontStyle fontStyle {
        get {
            return font_.fontStyle;
        }
        set {
            if (font_.fontStyle != value) {
                font_.fontStyle = value;
                updateFlags |= exUpdateFlags.Text;
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
                }
            }
        }
    }

#if UNITY_EDITOR
    
    /// 该属性仅供编辑器使用，用户直接调用SetFont方法即可，无需设置类型。
    public exFont.TypeForEditor fontType {
        get {
            return font_.type;
        }
        set {
            if (font_.type != value) {
                font_.type = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

#endif

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    protected override Texture texture {
        get {
            return font_.texture;
        }
    }

    public override bool visible {
        get {
            return isOnEnabled && font_.isValid;
        }
    }
    
    public override float width {
        get { return width_; }
        set { width_ = value; }
    }

    public override float height {
        get { return height_; }
        set { height_ = value; }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected new void OnEnable () {
        font_.textureRebuildCallback += OnFontTextureRebuilt;
        base.OnEnable();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected new void OnDisable () {
        base.OnDisable();
        font_.textureRebuildCallback -= OnFontTextureRebuilt;
    }

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
        // save dirty flag because SpriteFontBuilder.UpdateBuffers will overwrite it
        exUpdateFlags applyedFlags = exUpdateFlags.None;
        bool transparentDirty = (updateFlags & exUpdateFlags.Transparent) != 0;
        if (transparentDirty) {
            updateFlags &= ~exUpdateFlags.Transparent;
            if (transparent_) {
                updateFlags &= ~exUpdateFlags.Vertex;
            }
            else { 
                updateFlags |= exUpdateFlags.Vertex;
            }
            applyedFlags |= (exUpdateFlags.Transparent | exUpdateFlags.Vertex);
        }
        else if (transparent_ && (updateFlags & exUpdateFlags.Vertex) != 0) {
            updateFlags &= ~exUpdateFlags.Vertex;
        }

        SpriteFontParams sfp;
        sfp.text = text_;
        sfp.font = font_;
        sfp.spacing = spacing_;
        sfp.textAlign = textAlign_;
        sfp.useKerning = useKerning_;
        sfp.vertexCount = vertexCount_;
        sfp.indexCount = indexCount_;
        applyedFlags |= SpriteFontBuilder.UpdateBuffers (this, ref sfp, Space.World, ref topColor_, ref botColor_, layer_.alpha, 
                                                _vertices, _uvs, _colors32, _indices, vertexBufferIndex, indexBufferIndex);
        
        if (transparentDirty && transparent_) {
            // 如果有exUpdateFlags.Text一样会被UpdateBuffers显示出来，需要重新设为不可见
            Vector3 samePoint = _vertices.buffer[0];
            for (int i = 1; i < vertexCount_; ++i) {
                _vertices.buffer[vertexBufferIndex + i] = samePoint;
            }
        }
        return applyedFlags;
    }

    #endregion  // Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (Space _space) {
        // TODO: only return the rotated bounding box of the sprite font
        exList<Vector3> vertices = exList<Vector3>.GetTempList();
        vertices.AddRange(vertexCount_);
        exDebug.Assert(vertexCount_ >= text_.Length * 4);
        
        SpriteFontParams sfp;
        sfp.text = text_;
        sfp.font = font_;
        sfp.spacing = spacing_;
        sfp.textAlign = textAlign_;
        sfp.useKerning = useKerning_;
        sfp.vertexCount = vertexCount_;
        sfp.indexCount = indexCount_;
        
        SpriteFontBuilder.BuildText(this, ref sfp, _space, vertices, 0, null);
        return vertices.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected override void OnPreAddToLayer () {
        UpdateCapacity();
    }

    //// ------------------------------------------------------------------ 
    //// Desc:
    //// ------------------------------------------------------------------ 
    // 这里不做优化，因为隐藏时就算重建贴图，消耗也只是加上updateFlag，只有显示时才会刷新
    //protected override void Hide () {
    //    base.Hide();
    //    font_.textureRebuildCallback -= OnFontTextureRebuilt;
    //}

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetFont (exBitmapFont _bitmapFont) {
        font_.Set(_bitmapFont);
        UpdateTexture();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetFont (Font _dynamicFont) {
        font_.Set(_dynamicFont);
        UpdateTexture();
    }

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

    void UpdateTexture () {
        if (font_.texture != null) {
            updateFlags |= exUpdateFlags.Text;
            UpdateMaterial();
        }
        else if (layer_ != null) {
            // become invisible
            Hide();
        }
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
        int oldTextCapacity = vertexCount_ / exMesh.QUAD_VERTEX_COUNT;
        int textCapacity = Mathf.Max(GetTextCapacity(oldTextCapacity), 1);  // layered sprite should always have at lease one quad
        
        if (textCapacity != oldTextCapacity) {
            if (layer_ != null) {
                layer_.OnPreSpriteChange(this);
                vertexCount_ = textCapacity * exMesh.QUAD_VERTEX_COUNT;
                indexCount_ = textCapacity * exMesh.QUAD_INDEX_COUNT;
                layer_.OnAfterSpriteChange(this);
            }
            else {
                vertexCount_ = textCapacity * exMesh.QUAD_VERTEX_COUNT;
                indexCount_ = textCapacity * exMesh.QUAD_INDEX_COUNT;
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnFontTextureRebuilt () {
        updateFlags |= exUpdateFlags.Text;  // TODO: only need to update UV
    }
}

namespace ex2D.Detail {

    ///////////////////////////////////////////////////////////////////////////////
    //
    /// 储存sprite font的相关属性以便于参数传递
    /// 由于C#不能实现多重继承，这里使用面向过程的方式实现代码重用。因为需要传递大量参数，使用SpriteFontParams来优化。
    /// 不使用来接口实现多重继承主要是出于性能考虑。
    //
    ///////////////////////////////////////////////////////////////////////////////

    internal struct SpriteFontParams {
        public string text;
        public exFont font;
        public Vector2 spacing;
        public TextAlignment textAlign;
        public bool useKerning;
        public int vertexCount;
        public int indexCount;
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    //
    /// The sprite font geometry helper
    //
    ///////////////////////////////////////////////////////////////////////////////

    internal static class SpriteFontBuilder {
        
        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 
        
        public static void GetVertexAndIndexCount (string _text, out int _vertexCount, out int _indexCount) {
            int textLength = _text.Length;  // todo: multiline
            _vertexCount = textLength * exMesh.QUAD_VERTEX_COUNT;
            _indexCount = textLength * exMesh.QUAD_INDEX_COUNT;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public static exUpdateFlags UpdateBuffers (exSpriteBase _sprite, ref SpriteFontParams sfp, Space _space, ref Color _topColor, ref Color _botColor, float _alpha, 
                                                   exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices, int _vbIndex, int _ibIndex) {
#if UNITY_EDITOR
            if (sfp.text == null || sfp.vertexCount < sfp.text.Length * exMesh.QUAD_VERTEX_COUNT) {
                Debug.LogError("顶点缓冲长度不够，是否绕开属性直接修改了text_?: " + sfp.vertexCount, _sprite);
                return _sprite.updateFlags;
            }
#endif
            //Debug.Log(string.Format("[UpdateBuffers|SpriteFontBuilder] _vbIndex: {0} _ibIndex: {1}", _vbIndex, _ibIndex));
            if ((_sprite.updateFlags & exUpdateFlags.Text) != 0) {
                //exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
                BuildText(_sprite, ref sfp, _space, _vertices, _vbIndex, _uvs);
                _sprite.updateFlags |= (exUpdateFlags.Vertex | exUpdateFlags.UV | exUpdateFlags.Color);
            }
            else if ((_sprite.updateFlags & exUpdateFlags.Vertex) != 0) {
                //exDebug.Assert(cachedWorldMatrix == cachedTransform.localToWorldMatrix);
                BuildText(_sprite, ref sfp, _space, _vertices, _vbIndex, null);
            }
            if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
                // update index buffer
                int indexBufferEnd = _ibIndex + sfp.indexCount - 5;
                for (int i = _ibIndex, v = _vbIndex; i < indexBufferEnd; i += 6, v += 4) {
                    _indices.buffer[i] = v;
                    _indices.buffer[i + 1] = v + 1;
                    _indices.buffer[i + 2] = v + 2;
                    _indices.buffer[i + 3] = v + 2;
                    _indices.buffer[i + 4] = v + 3;
                    _indices.buffer[i + 5] = v;
                }
            }
            if ((_sprite.updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null) {
                Color32 top, bot;
                if (_alpha > 0.0f) {
                    top = new Color (_topColor.r, _topColor.g, _topColor.b, _topColor.a * _alpha);
                    top *= _sprite.color;
                    bot = new Color (_botColor.r, _botColor.g, _botColor.b, _botColor.a * _alpha);
                    bot *= _sprite.color;
                }
                else {
                    top = new Color32 ();
                    bot = new Color32 ();
                }
                int vertexBufferEnd = _vbIndex + sfp.text.Length * 4;
                for (int i = _vbIndex; i < vertexBufferEnd; i += 4) {
                    _colors32.buffer[i + 0] = bot;
                    _colors32.buffer[i + 1] = top;
                    _colors32.buffer[i + 2] = top;
                    _colors32.buffer[i + 3] = bot;
                }
            }
            exUpdateFlags updatedFlags = _sprite.updateFlags;
            _sprite.updateFlags = exUpdateFlags.None;
            return updatedFlags;
        }
        
        // ------------------------------------------------------------------ 
        // Desc:
        // ------------------------------------------------------------------ 

        public static void BuildText (exSpriteBase _sprite, ref SpriteFontParams sfp, Space _space, exList<Vector3> _vertices, int _vbIndex, exList<Vector2> _uvs = null) {

            // It is advisable to always call RequestCharactersInTexture for any text on the screen you wish to render using custom font rendering functions, 
            // even if the characters are currently present in the texture, to make sure they don't get purged during texture rebuild.
            sfp.font.RequestCharactersInTexture (sfp.text);
            
            _sprite.width = 0.0f;    // 和SpriteBase一致，用于表示实际宽度
            _sprite.height = 0.0f;   // 和SpriteBase一致，用于表示实际高度
            int invisibleVertexStart = -1;
            int visibleVertexCount;
            if (sfp.font.isValid) {
                BuildTextInLocalSpace(_sprite, ref sfp, _vertices, _vbIndex, _uvs);
                visibleVertexCount = sfp.text.Length * 4;
            }
            else {
                //System.Array.Clear(_vertices.buffer, _vbIndex, visibleVertexCount);
                visibleVertexCount = 0;
            }
            if (_sprite.vertexCount > visibleVertexCount) {
                // hide invisible vertex
                for (int i = _vbIndex + visibleVertexCount, iMax = _vbIndex + _sprite.vertexCount; i < iMax; ++i) {
                    _vertices.buffer[i] = new Vector3();
                }
            }

            // calculate anchor and offset
            Vector2 anchorOffset = new Vector2();
            // convert to top left
            switch (sfp.textAlign) {
            case TextAlignment.Left:
                break;
            case TextAlignment.Center:
                anchorOffset.x = _sprite.width * 0.5f;
                break;
            case TextAlignment.Right:
                anchorOffset.x = _sprite.width;
                break;
            }
            // convert anchor from top center to user defined
            switch ( _sprite.anchor ) {
            case Anchor.TopLeft   :                                         anchorOffset.y = 0.0f;                  break;
            case Anchor.TopCenter : anchorOffset.x -= _sprite.width * 0.5f; anchorOffset.y = 0.0f;                  break;
            case Anchor.TopRight  : anchorOffset.x -= _sprite.width;        anchorOffset.y = 0.0f;                  break;
            case Anchor.MidLeft   :                                         anchorOffset.y = _sprite.height * 0.5f; break;
            case Anchor.MidCenter : anchorOffset.x -= _sprite.width * 0.5f; anchorOffset.y = _sprite.height * 0.5f; break;
            case Anchor.MidRight  : anchorOffset.x -= _sprite.width;        anchorOffset.y = _sprite.height * 0.5f; break;
            case Anchor.BotLeft   :                                         anchorOffset.y = _sprite.height;        break;
            case Anchor.BotCenter : anchorOffset.x -= _sprite.width * 0.5f; anchorOffset.y = _sprite.height;        break;
            case Anchor.BotRight  : anchorOffset.x -= _sprite.width;        anchorOffset.y = _sprite.height;        break;
            default               : anchorOffset.x -= _sprite.width * 0.5f; anchorOffset.y = _sprite.height * 0.5f; break;
            }
            // offset
            Vector3 offset = anchorOffset + _sprite.offset;

            Vector2 shearOffset = _sprite.shear;
            if (shearOffset.x != 0) {
                shearOffset.x = _sprite.GetScaleY(_space) * shearOffset.x;
            }
            if (shearOffset.y != 0) {
                shearOffset.y = _sprite.GetScaleX(_space) * shearOffset.y;
            }
            int vbEnd = _vbIndex + _sprite.vertexCount;
            for (int i = _vbIndex; i < vbEnd; i += 4) {
                if (invisibleVertexStart == -1 || i <= invisibleVertexStart) {
                    Vector3 v0 = _vertices.buffer[i + 0];
                    Vector3 v1 = _vertices.buffer[i + 1];
                    Vector3 v2 = _vertices.buffer[i + 2];
                    Vector3 v3 = _vertices.buffer[i + 3];
                    // apply anchor and offset
                    v0 += offset;
                    v1 += offset;
                    v2 += offset;
                    v3 += offset;
                    // shear
                    if (shearOffset.x != 0) {
                        float halfCharHeight = (v1.y - v0.y) * 0.5f;
                        float topOffset = shearOffset.x * (halfCharHeight + anchorOffset.y);
                        float botOffset = shearOffset.x * (-halfCharHeight + anchorOffset.y);
                        v0.x += botOffset;
                        v1.x += topOffset;
                        v2.x += topOffset;
                        v3.x += botOffset;
                    }
                    if (shearOffset.y != 0) {
                        float halfCharWidth = (v2.y - v0.y) * 0.5f;
                        float leftOffset = shearOffset.y * (-halfCharWidth + anchorOffset.x);
                        float rightOffset = shearOffset.y * (halfCharWidth + anchorOffset.x);
                        v0.y += leftOffset;
                        v1.y += leftOffset;
                        v2.y += rightOffset;
                        v3.y += rightOffset;
                    }
                    // transform
                    if (_space == Space.World) {
                        v0 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v0);
                        v1 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v1);
                        v2 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v2);
                        v3 = _sprite.cachedWorldMatrix.MultiplyPoint3x4(v3);
                        v0.z = 0; v1.z = 0; v2.z = 0; v3.z = 0;
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

        public static void BuildTextInLocalSpace (exSpriteBase _sprite, ref SpriteFontParams sfp, exList<Vector3> _vertices, int _vbIndex, exList<Vector2> _uvs) {
            Vector2 texelSize = new Vector2();
            if (_uvs != null && sfp.font.texture != null) {
                texelSize = sfp.font.texture.texelSize;
            }

            int parsedVBIndex = _vbIndex;
            for (int charIndex = 0; charIndex < sfp.text.Length; ) {
                int lineStart = parsedVBIndex;
                // build line
                float lineWidth = BuildLine(ref sfp, _vertices, _uvs, ref charIndex, ref parsedVBIndex, texelSize, - _sprite.height);
                // text alignment
                switch (sfp.textAlign) {
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
                if (lineWidth > _sprite.width) {
                    _sprite.width = lineWidth;
                }
                _sprite.height += sfp.font.fontSize;
                if (charIndex < sfp.text.Length) {
                    _sprite.height += sfp.spacing.y;
                }
            }
        }
        
        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 
        
        public static float BuildLine (ref SpriteFontParams sfp, exList<Vector3> _vertices, exList<Vector2> _uvs, ref int _charIndex, ref int _vbIndex, Vector2 _texelSize, float _top) {
            // TODO: cache vertices, only transform them if text not changed.
            int firstChar = _charIndex;
            float curX = 0.0f;
            float lastAdvance = 0.0f;
            float lastWidth = 0.0f;
            for (; _charIndex < sfp.text.Length; ++_charIndex, _vbIndex += 4, curX += sfp.spacing.x) {
                char c = sfp.text[_charIndex];
                
                // if new line
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
                    if (sfp.useKerning) {
                        curX += sfp.font.GetKerning(sfp.text[_charIndex - 1], c);
                    }
                }

                CharacterInfo ci;
                if (sfp.font.GetCharInfo(c, out ci) == false) {
                    // character is not present, it will not display
                    // Debug.Log("character is not present: " + c, this);
                    _vertices.buffer[_vbIndex + 0] = new Vector3();
                    _vertices.buffer[_vbIndex + 1] = new Vector3();
                    _vertices.buffer[_vbIndex + 2] = new Vector3();
                    _vertices.buffer[_vbIndex + 3] = new Vector3();
                    continue;
                }

                float x = curX;
                float y = _top;
                _vertices.buffer[_vbIndex + 0] = new Vector3(x + ci.vert.xMin, y + ci.vert.yMax, 0.0f);
                _vertices.buffer[_vbIndex + 1] = new Vector3(x + ci.vert.xMin, y + ci.vert.yMin, 0.0f);
                _vertices.buffer[_vbIndex + 2] = new Vector3(x + ci.vert.xMax, y + ci.vert.yMin, 0.0f);
                _vertices.buffer[_vbIndex + 3] = new Vector3(x + ci.vert.xMax, y + ci.vert.yMax, 0.0f);
                
                // advance x
                lastWidth = ci.vert.width;
                lastAdvance = ci.width;

                // build uv
                if (_uvs != null) {
                    if (ci.flipped) {
                        _uvs.buffer [_vbIndex + 0] = new Vector2 (ci.uv.xMin, ci.uv.yMin);
                        _uvs.buffer [_vbIndex + 1] = new Vector2 (ci.uv.xMax, ci.uv.yMin);
                        _uvs.buffer [_vbIndex + 2] = new Vector2 (ci.uv.xMax, ci.uv.yMax);
                        _uvs.buffer [_vbIndex + 3] = new Vector2 (ci.uv.xMin, ci.uv.yMax);
                    }
                    else {
                        _uvs.buffer [_vbIndex + 0] = new Vector2 (ci.uv.xMin, ci.uv.yMin);
                        _uvs.buffer [_vbIndex + 1] = new Vector2 (ci.uv.xMin, ci.uv.yMax);
                        _uvs.buffer [_vbIndex + 2] = new Vector2 (ci.uv.xMax, ci.uv.yMax);
                        _uvs.buffer [_vbIndex + 3] = new Vector2 (ci.uv.xMax, ci.uv.yMin);
                    }
                }
            }
            return curX + lastWidth;
        }
    }
}
