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

    #region Functions used to update geometry buffer

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    internal override exUpdateFlags UpdateBuffers (exList<Vector3> _vertices, exList<Vector2> _uvs, exList<Color32> _colors32, exList<int> _indices) {
        if (updateFlags == exUpdateFlags.None) {
            return exUpdateFlags.None;
        }
        else {
            SpriteFontParams sfp;
            sfp.text = text_;
            sfp.font = font_;
            sfp.spacing = spacing_;
            sfp.textAlign = textAlign_;
            sfp.useKerning = useKerning_;
            sfp.vertexCount = vertexCount_;
            sfp.indexCount = indexCount_;
            return SpriteFontBuilder.UpdateBuffers (this, ref sfp, Space.Self, ref topColor_, ref botColor_, 1.0f, 
                                                   _vertices, _uvs, _colors32, _indices, 0, 0);

        }
    }

    #endregion  // Functions used to update geometry buffer
    
    /*// ------------------------------------------------------------------ 
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
    }*/

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void UpdateVertexAndIndexCount () {
        SpriteFontBuilder.GetVertexAndIndexCount (text_, out vertexCount_, out indexCount_);
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Public Functions
    ///////////////////////////////////////////////////////////////////////////////

}
//#endif