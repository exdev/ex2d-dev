// ======================================================================================
// File         : ex3DSpriteFont.cs
// Author       : 
// Last Change  : 08/31/2013
// Description  : 
// ======================================================================================
//#if stash
///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ex2D.Detail;

///////////////////////////////////////////////////////////////////////////////
/// 
/// A component to render exBitmapFont in the scene 
/// 
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/3D Sprite Font")]
public class ex3DSpriteFont : exStandaloneSprite {

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
                    Debug.LogWarning("invalid bitmap font texture");
                }
                updateFlags |= exUpdateFlags.Text;

                if (font_ == null || ReferenceEquals(font_.texture, value.texture) == false) {
                    // texture changed
                    font_ = value;
                    UpdateMaterial();   // 前面update过text了
                    return;
                }
                if (isOnEnabled_ && visible == false) {
                    font_ = value;
                    if (visible) {
                        Show();
                    }
                }
            }
            else if (visible) {
                // become invisible
                Hide();
            }
            font_ = value;
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
                if (text_.Length >= exMesh.MAX_QUAD_COUNT) {    // todo: check multiline
                    text_ = text_.Substring(0, exMesh.MAX_QUAD_COUNT);
                    Debug.LogError("Too many character on one sprite: " + value.Length, this);
                }
                UpdateBufferSize();
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

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

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
            return isOnEnabled_ && font_ != null && font_.texture != null;
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////

    #region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices) {
        if (updateFlags == exUpdateFlags.None) {
            return exUpdateFlags.None;
        }
        else {
            return SpriteFontBuilder.UpdateBuffers (this, text_, Space.Self, ref topColor_, ref botColor_, 1.0f, 
                                                    _vertices, _uvs, _colors32, _indices, 0, 0);
        }
    }

    #endregion  // Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (Space _space) {
        // TODO: only return the rotated bounding box of the sprite font
        int visibleVertexCount = text_.Length * 4;
        exList<Vector3> vertices = exList<Vector3>.GetTempList();
        vertices.AddRange(visibleVertexCount);
        Matrix4x4 mat = _space == Space.World ? cachedWorldMatrix : Matrix4x4.identity;
        BuildText(vertices, 0, ref mat);
        return vertices.ToArray();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void UpdateVertexAndIndexCount () {
        SpriteFontBuilder.GetVertexAndIndexCount (text_, out vertexCount_, out indexCount_);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 
    
    void BuildText (exList<Vector3> _vertices, int _vbIndex, ref Matrix4x4 _spriteMatrix, exList<Vector2> _uvs = null) {
        // TODO: use space instead of _spriteMatrix
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
            shearOffsetX = GetScaleY(Space.World) * shear_.x;
        }
        if (shear_.y != 0) {
            shearOffsetY = GetScaleX(Space.World) * shear_.y;
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
}
//#endif