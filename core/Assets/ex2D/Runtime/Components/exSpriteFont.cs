﻿// ======================================================================================
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
using System.Text;
using ex2D.Detail;

//// ------------------------------------------------------------------ 
///// The type of font effect
//// ------------------------------------------------------------------ 

//public enum exOutlineType {
//    Outline4 = 1,   ///< up down left right
//    Outline4X,      ///< top-left top-right bottom-left bottom-right
//    Outline8,       ///< 
//};

///////////////////////////////////////////////////////////////////////////////
/// 
/// A component to render exFont in the layer 
/// 
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/2D Sprite Font")]
public class exSpriteFont : exLayeredSprite, exISpriteFont {

    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////

    /// 每个exSpriteFont都有单独的一个exFont实例
    [SerializeField] protected exFont font_ = new exFont();

    exFont exISpriteFont.font {
        get { return font_; }
    }

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
                if (oldText.Length != text_.Length) {
                    UpdateCapacity();
                }
                updateFlags |= exUpdateFlags.Text;
            }
        }
    }

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

    // ------------------------------------------------------------------ 
    [SerializeField] protected bool wrapWord_ = false;
    // ------------------------------------------------------------------ 

    public bool wrapWord {
        get { return wrapWord_; }
        set {
            if (wrapWord_ != value) {
                wrapWord_ = value;
                updateFlags |= exUpdateFlags.Text;
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
                customLineHeight_ = true;
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    [SerializeField] protected int lineHeight_ = 0;
    /// the fixed line space applied between two lines.
    // ------------------------------------------------------------------ 

    public int lineHeight {
        get { return customLineHeight_ ? lineHeight_ : font_.fontSize; }
        set {
            if (lineHeight_ != value) {
                lineHeight_ = value;
                updateFlags |= exUpdateFlags.Vertex;
                if (customLineHeight_ == false) {
                    lineHeight_ = font_.fontSize;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected bool customLineHeight_ = false;
    ///
    // ------------------------------------------------------------------ 

    public bool customLineHeight {
        get { return customLineHeight_; }
        set {
            if (customLineHeight_ != value) {
                customLineHeight_ = value;
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected int letterSpacing_ = 0;
    /// the fixed width applied between two characters in the text. 
    // ------------------------------------------------------------------ 

    public int letterSpacing {
        get { return letterSpacing_; }
        set {
            if (letterSpacing_ != value) {
                letterSpacing_ = value;
                if (wrapWord_) {
                    updateFlags |= exUpdateFlags.Text;
                }
                else {
                    updateFlags |= exUpdateFlags.Vertex;
                }
            }
        }
    }

    // ------------------------------------------------------------------ 
    [SerializeField] protected int wordSpacing_ = 0;
    /// the fixed width applied between two words in the text. 
    // ------------------------------------------------------------------ 

    public int wordSpacing {
        get { return wordSpacing_; }
        set {
            if (wordSpacing_ != value) {
                wordSpacing_ = value;
                if (wrapWord_) {
                    updateFlags |= exUpdateFlags.Text;
                }
                else {
                    updateFlags |= exUpdateFlags.Vertex;
                }
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
        set {
            width_ = value;
            if (wrapWord_) {
                updateFlags |= exUpdateFlags.Text;
            }
        }
    }

    public override float height {
        get { return height_; }
        set { height_ = value; }
    }
    
    public override bool customSize {
        get { return true; }
        set { customSize_ = true; }
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

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices,
                                                    exList<Vector2> _uvs,
                                                    exList<Color32> _colors32,
                                                    exList<int> _indices) {
        if (font_.isValid == false) {
            var retval = exUpdateFlags.None;
            bool updateIndex = (updateFlags & (exUpdateFlags.Index | exUpdateFlags.Text)) != 0 && _indices != null;
            if (updateIndex) {
                // ensure index buffer in vertex range
                int indexBufferEnd = indexBufferIndex + indexCount - 5;
                for (int i = indexBufferIndex, v = vertexBufferIndex; i < indexBufferEnd; i += 6, v += 4) {
                    _indices.buffer[i] = v;
                    _indices.buffer[i + 1] = v + 1;
                    _indices.buffer[i + 2] = v + 2;
                    _indices.buffer[i + 3] = v + 2;
                    _indices.buffer[i + 4] = v + 3;
                    _indices.buffer[i + 5] = v;
                }
                updateFlags &= ~exUpdateFlags.Index;
                retval |= exUpdateFlags.Index;
            }
            bool updateVertex = (updateFlags & exUpdateFlags.Text) != 0;
            if (updateVertex) {
                Vector3 anyPoint = _vertices.buffer.Length > 0 ? _vertices.buffer[0] : new Vector3();
                for (int i = 0; i < vertexCount_; ++i) {
                    _vertices.buffer[vertexBufferIndex + i] = anyPoint;
                }
                updateFlags &= ~exUpdateFlags.Text;
                retval |= exUpdateFlags.Vertex;
            }
            return retval;
        }

        bool transparentDirty = (updateFlags & exUpdateFlags.Transparent) != 0;
        if (transparentDirty) {
            updateFlags &= ~exUpdateFlags.Transparent;
            if (transparent_) {
                // make vertex invisible
                Vector3 anyPoint = _vertices.buffer.Length > 0 ? _vertices.buffer[0] : new Vector3();
                for (int i = 0; i < vertexCount_; ++i) {
                    _vertices.buffer[vertexBufferIndex + i] = anyPoint;
                }
                updateFlags &= ~exUpdateFlags.Vertex;
                return updateFlags | exUpdateFlags.Vertex;   // 既然不可见，直接让mesh更新各个buffer即可。这里不重设updateFlags，所以变回可见时会正确初始化。
            }
            else {
                // revert vertex
                updateFlags |= exUpdateFlags.Vertex;
            }
        }
        else if (transparent_) {
            return exUpdateFlags.None;
        }

        return SpriteFontBuilder.UpdateBuffers (this, Space.World, layer_.alpha, _vertices, _uvs, 
                                                 _colors32, _indices, vertexBufferIndex, indexBufferIndex);
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
        
        SpriteFontBuilder.BuildText(this, _space, vertices, 0);
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
                layer_.SetSpriteBufferSize (this, 
                                            textCapacity * exMesh.QUAD_VERTEX_COUNT, 
                                            textCapacity * exMesh.QUAD_INDEX_COUNT);
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
        
        if (layer_ != null) {
            layer_.OnFontTextureRebuilt();
        }
    }
}

namespace ex2D.Detail {

    ///////////////////////////////////////////////////////////////////////////////
    //
    /// The sprite font geometry helper
    //
    ///////////////////////////////////////////////////////////////////////////////

    internal static class SpriteFontBuilder {

        ///////////////////////////////////////////////////////////////////////////////
        // Temp parameters for BuildText
        ///////////////////////////////////////////////////////////////////////////////

        private static Color topFinalColor;
        private static Color botFinalColor;

        private static exList<Vector3> vertices;
        private static exList<Vector2> uvs;
        private static exList<Color32> colors32;
        private static int vbIndex;
        //private static Vector2 texelSize;

        ///////////////////////////////////////////////////////////////////////////////
        // Functions
        ///////////////////////////////////////////////////////////////////////////////

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 
        
        public static void GetVertexAndIndexCount (string _text, out int _vertexCount, out int _indexCount) {
            int textLength = _text.Length;
            _vertexCount = textLength * exMesh.QUAD_VERTEX_COUNT;
            _indexCount = textLength * exMesh.QUAD_INDEX_COUNT;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        public static exUpdateFlags UpdateBuffers (exISpriteFont _sprite, Space _space, float _alpha, 
                                                   exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices, int _vbIndex, int _ibIndex) {
#if UNITY_EDITOR
            if (_sprite.text == null || _sprite.vertexCount < _sprite.text.Length * exMesh.QUAD_VERTEX_COUNT) {
                Debug.LogError("顶点缓冲长度不够，是否绕开属性直接修改了text_?: " + _sprite.vertexCount, _sprite as Object);
                return _sprite.updateFlags;
            }
#endif
            bool colorUpdated = false;   // 有渐变色时，每个字符单独计算顶点色，否则全局填充顶点色

            // Debug.Log(string.Format("[UpdateBuffers|SpriteFontBuilder] _vbIndex: {0} _ibIndex: {1}", _vbIndex, _ibIndex));
            if ((_sprite.updateFlags & exUpdateFlags.Text) != 0) {
                if (_alpha > 0.0f) {
                    // 初始化渐变色要用到的全局参数
                    Color tmp = _sprite.color; tmp.a *= _alpha;
                    topFinalColor = _sprite.topColor * tmp;
                    botFinalColor = _sprite.botColor * tmp;
                    if (_sprite.fontSize != 0) {
                        botFinalColor = exMath.Lerp(topFinalColor, botFinalColor, 1.0f / _sprite.fontSize);   // 预除
                    }
                    else {
                        botFinalColor = topFinalColor;
                        Debug.LogWarning("Failed to use gradient font color due to invalid font size", _sprite as Object);
                    }
                }
                else {
                    topFinalColor = new Color ();
                    botFinalColor = new Color ();
                }
                colorUpdated = topFinalColor != botFinalColor;
                if ((_sprite.updateFlags & exUpdateFlags.Text & ~exUpdateFlags.Color) != 0 || colorUpdated) {
                    BuildText(_sprite, _space, _vertices, _vbIndex, _uvs, colorUpdated ? _colors32 : null);
                }
            }
            if ((_sprite.updateFlags & exUpdateFlags.Index) != 0 && _indices != null) {
                // update index buffer
                int indexBufferEnd = _ibIndex + _sprite.indexCount - 5;
                for (int i = _ibIndex, v = _vbIndex; i < indexBufferEnd; i += 6, v += 4) {
                    _indices.buffer[i] = v;
                    _indices.buffer[i + 1] = v + 1;
                    _indices.buffer[i + 2] = v + 2;
                    _indices.buffer[i + 3] = v + 2;
                    _indices.buffer[i + 4] = v + 3;
                    _indices.buffer[i + 5] = v;
                }
            }
            if ((_sprite.updateFlags & exUpdateFlags.Color) != 0 && _colors32 != null && colorUpdated == false) {
                // if not gradient we just need to fill same color here
                Color tmp = _sprite.topColor * _sprite.color;
                Color32 color32 = new Color(tmp.r, tmp.g, tmp.b, tmp.a * _alpha);
                int vertexBufferEnd = _vbIndex + _sprite.text.Length * 4;
                for (int i = _vbIndex; i < vertexBufferEnd; ++i) {
                    _colors32.buffer[i] = color32;
                }
            }
            exUpdateFlags updatedFlags = _sprite.updateFlags;
            _sprite.updateFlags = exUpdateFlags.None;
            return updatedFlags;
        }
        
        // ------------------------------------------------------------------ 
        // Desc:
        // ------------------------------------------------------------------ 

        public static void BuildText (exISpriteFont _sprite, Space _space, exList<Vector3> _vertices, int _vbIndex, exList<Vector2> _uvs = null, exList<Color32> _colors32 = null) {
            // 保存变量到全局，减少内部传参
            vertices = _vertices;
            uvs = _uvs;
            colors32 = _colors32;
            vbIndex = _vbIndex;

            // request font texture, this may called OnFontTextureRebuilt to set _sprite.updateFlags
            _sprite.font.RequestCharactersInTexture (_sprite.text);
            _sprite.height = 0.0f;
            float displayWidth = 0; // 实际渲染的宽度不等于sprite.width(换行宽度)
            int visibleVertexCount = 0;
            if (_sprite.font.isValid) {
                visibleVertexCount = BuildTextInLocalSpace(_sprite, out displayWidth);
            }
            if (visibleVertexCount == 0 && _sprite.vertexCount >= 4) {
                visibleVertexCount = 4;
                // 放四个假的点，用于计算后续变换，才能得出正确的boundingbox
                _vertices.buffer[_vbIndex + 0] = new Vector3();
                _vertices.buffer[_vbIndex + 1] = new Vector3();
                _vertices.buffer[_vbIndex + 2] = new Vector3();
                _vertices.buffer[_vbIndex + 3] = new Vector3();
            }
            int invisibleVertexStart = _vbIndex + visibleVertexCount;

            // calculate anchor and offset
            Vector2 anchorOffset = new Vector2();
            // convert to top left
            switch (_sprite.textAlign) {
            case TextAlignment.Left:
                break;
            case TextAlignment.Center:
                anchorOffset.x = displayWidth * 0.5f;
                break;
            case TextAlignment.Right:
                anchorOffset.x = displayWidth;
                break;
            }
            // convert anchor from top center to user defined
            switch ( _sprite.anchor ) {
            case Anchor.TopLeft   :                                        anchorOffset.y = 0.0f;                  break;
            case Anchor.TopCenter : anchorOffset.x -= displayWidth * 0.5f; anchorOffset.y = 0.0f;                  break;
            case Anchor.TopRight  : anchorOffset.x -= displayWidth;        anchorOffset.y = 0.0f;                  break;
            case Anchor.MidLeft   :                                        anchorOffset.y = _sprite.height * 0.5f; break;
            case Anchor.MidCenter : anchorOffset.x -= displayWidth * 0.5f; anchorOffset.y = _sprite.height * 0.5f; break;
            case Anchor.MidRight  : anchorOffset.x -= displayWidth;        anchorOffset.y = _sprite.height * 0.5f; break;
            case Anchor.BotLeft   :                                        anchorOffset.y = _sprite.height;        break;
            case Anchor.BotCenter : anchorOffset.x -= displayWidth * 0.5f; anchorOffset.y = _sprite.height;        break;
            case Anchor.BotRight  : anchorOffset.x -= displayWidth;        anchorOffset.y = _sprite.height;        break;
            default               : anchorOffset.x -= displayWidth * 0.5f; anchorOffset.y = _sprite.height * 0.5f; break;
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
            for (int i = _vbIndex; i < invisibleVertexStart; i += 4) {
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
            bool hasPivot = invisibleVertexStart > _vbIndex;
            if (hasPivot) {
                // hide invisible vertex 防止撑大boundingbox
                int vbEnd = _vbIndex + _sprite.vertexCount;
                for (int i = invisibleVertexStart; i < vbEnd; i += 4) {
    #if UNITY_EDITOR
                    if (i >= _vertices.buffer.Length) {
                        Debug.Log(string.Format("[BuildText|exSpriteFont] i: {0} vbEnd: " + vbEnd + " _vertices: " + _vertices.buffer.Length, i));
                    }
    #endif
                    _vertices.buffer[i + 0] = _vertices.buffer[_vbIndex];
                    _vertices.buffer[i + 1] = _vertices.buffer[_vbIndex];
                    _vertices.buffer[i + 2] = _vertices.buffer[_vbIndex];
                    _vertices.buffer[i + 3] = _vertices.buffer[_vbIndex];
                }
            }
            
            vertices = null;
            uvs = null;
            colors32 = null;

            // TODO: pixel-perfect
        }
        
        // ------------------------------------------------------------------ 
        /// Return used vertex count
        // ------------------------------------------------------------------ 
        
        private static int BuildTextInLocalSpace (exISpriteFont _sprite, out float maxWidth) {
            if ((_sprite.updateFlags & exUpdateFlags.Color) == 0) {
                colors32 = null;    // no need to update color
            }
            //texelSize = new Vector2();
            if ((_sprite.updateFlags & exUpdateFlags.UV) == 0) {
                uvs = null;         // no need to update uv
            }
            //else if (uvs != null && _sprite.font.texture != null) {
            //    texelSize = _sprite.font.texture.texelSize;
            //}

            float halfLineHeightMargin = (_sprite.lineHeight - _sprite.fontSize) * 0.5f;
            _sprite.height = halfLineHeightMargin;
            maxWidth = 0;

            // NOTE: 这里不用考虑预分配的空间用不完的情况，因为大部分情况下只需输出单行文本。
            //       而且一个mesh其实也显示不了太多字。
            StringBuilder strBuilder = new StringBuilder(_sprite.text.Length);
            int cur_index = 0;
            int parsedVbIndex = vbIndex;
            bool finished = false;

            while ( finished == false ) {
                int line_width = 0;
                strBuilder.Length = 0;  // Clear
                exTextUtility.CalcTextLine ( out line_width, 
                                            out cur_index,
                                            strBuilder,
                                            _sprite.text,
                                            cur_index,
                                            (int)_sprite.width,
                                            _sprite.font,
                                            _sprite.wordSpacing,
                                            _sprite.letterSpacing,
                                            _sprite.wrapWord,
                                            false,
                                            false );
                int lineStart = parsedVbIndex;
                float lineWidth = BuildLine(strBuilder.ToString(), _sprite, ref parsedVbIndex, - _sprite.height);
                if (lineWidth > maxWidth) {
                    maxWidth = lineWidth;
                }

                // text alignment
                switch (_sprite.textAlign) {
                case TextAlignment.Left:
                    // top left
                    break;
                case TextAlignment.Center:
                    // convert to top center
                    float halfLineWidth = lineWidth * 0.5f;
                    for (int i = lineStart; i < parsedVbIndex; ++i) {
                        vertices.buffer[i].x -= halfLineWidth;
                    }
                    break;
                case TextAlignment.Right:
                    // convert to top right
                    for (int i = lineStart; i < parsedVbIndex; ++i) {
                        vertices.buffer[i].x -= lineWidth;
                    }
                    break;
                }
                _sprite.height += _sprite.lineHeight;
                finished = (cur_index >= _sprite.text.Length);
            }

            _sprite.height += halfLineHeightMargin;

            return parsedVbIndex - vbIndex;
        }
        
        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 

        private static bool BuildChar (exFont _font, char _char, Vector2 _pos, int _vbIndex, out float charWidth, out float advance) {
            CharacterInfo ci;
            if (_font.GetCharInfo(_char, out ci) == false) {
                // character is not present, it will not display
                // Debug.Log("character is not present: " + c, this);
                vertices.buffer[_vbIndex + 0] = new Vector3();
                vertices.buffer[_vbIndex + 1] = new Vector3();
                vertices.buffer[_vbIndex + 2] = new Vector3();
                vertices.buffer[_vbIndex + 3] = new Vector3();
                charWidth = -1;
                advance = -1;
                return false;
            }
            Vector3 min = new Vector3(_pos.x + ci.vert.xMin, _pos.y + ci.vert.yMin, 0.0f);
            Vector3 max = new Vector3(_pos.x + ci.vert.xMax, _pos.y + ci.vert.yMax, 0.0f);
            vertices.buffer[_vbIndex + 0] = new Vector3(min.x, max.y, 0.0f);
            vertices.buffer[_vbIndex + 1] = min;
            vertices.buffer[_vbIndex + 2] = new Vector3(max.x, min.y, 0.0f);
            vertices.buffer[_vbIndex + 3] = max;
            
            // advance x
            charWidth = ci.vert.width;
            advance = ci.width;

            // set uv
            if (uvs != null) {
                Vector2 start = new Vector2(ci.uv.xMin, ci.uv.yMin);
                Vector2 end = new Vector2(ci.uv.xMax, ci.uv.yMax);
                if (ci.flipped) {
                    uvs.buffer[_vbIndex + 0] = start;
                    uvs.buffer[_vbIndex + 1] = new Vector2(end.x, start.y);
                    uvs.buffer[_vbIndex + 2] = end;
                    uvs.buffer[_vbIndex + 3] = new Vector2(start.x, end.y);
                }
                else {
                    uvs.buffer[_vbIndex + 0] = start;
                    uvs.buffer[_vbIndex + 1] = new Vector2(start.x, end.y);
                    uvs.buffer[_vbIndex + 2] = end;
                    uvs.buffer[_vbIndex + 3] = new Vector2(end.x, start.y);
                }
            }

            // set color
            if (colors32 != null) {
                Color32 t = exMath.Lerp(topFinalColor, botFinalColor, -ci.vert.yMin);
                Color32 b = exMath.Lerp(topFinalColor, botFinalColor, -ci.vert.yMax);
                colors32.buffer[_vbIndex + 0] = b;
                colors32.buffer[_vbIndex + 1] = t;
                colors32.buffer[_vbIndex + 2] = t;
                colors32.buffer[_vbIndex + 3] = b;
            }
            return true;
        }

        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 
        
        private static float BuildLine (string _text, exISpriteFont _sprite, ref int _parsedVbIndex, float _top) {
            // cache property
            var letterSpacing = _sprite.letterSpacing;
            var wordSpacing = _sprite.wordSpacing;
            bool useKerning = _sprite.useKerning;
            exFont font = _sprite.font;
            //
            Vector2 pos = new Vector2(0.0f, _top);
            float lastAdvance = 0.0f;
            float lastWidth = 0.0f;
            for (int _charIndex = 0; _charIndex < _text.Length; ++_charIndex, _parsedVbIndex += 4, pos.x += letterSpacing) {
                char c = _text[_charIndex];
                
                // if new line
                if ( c == '\n' || c == '\r' ) {
                    vertices.buffer[_parsedVbIndex + 0] = new Vector3();
                    vertices.buffer[_parsedVbIndex + 1] = new Vector3();
                    vertices.buffer[_parsedVbIndex + 2] = new Vector3();
                    vertices.buffer[_parsedVbIndex + 3] = new Vector3();
                    ++_charIndex;
                    _parsedVbIndex += 4;
                    break;
                }
                
                bool hasPreviousChar = _charIndex > 0;
                if (hasPreviousChar) {
                    pos.x += lastAdvance;
                    // kerning
                    if (useKerning) {
                        pos.x += font.GetKerning(_text[_charIndex - 1], c);
                    }
                    // wordSpacing
                    if (c == ' ') {
                        pos.x += wordSpacing;
                    }
                }

                float width, advance;
                if (BuildChar(font, c, pos, _parsedVbIndex, out width, out advance)) {
                    // advance x
                    lastWidth = width;
                    lastAdvance = advance;
                }
            }
            return pos.x + lastWidth;
        }
    }
}
