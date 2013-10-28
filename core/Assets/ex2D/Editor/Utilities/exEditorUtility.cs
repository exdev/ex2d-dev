// ======================================================================================
// File         : exEditorUtility.cs
// Author       : Wu Jie 
// Last Change  : 06/16/2013 | 14:37:44 PM | Sunday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

///////////////////////////////////////////////////////////////////////////////
///
/// editor helper function
///
///////////////////////////////////////////////////////////////////////////////

public static class exEditorUtility {

    ///////////////////////////////////////////////////////////////////////////////
    // NOTE: At the beginning, I try to do all resources initliazed in exEditorUtility.Init()
    // and called it in ex2DEditor. However in this way, it will suffer some random crash
    // in Unity. I have to shift the code to on-demand init to avoid crash.
    ///////////////////////////////////////////////////////////////////////////////

    // meshQuad
    static Mesh meshQuad_;
    public static Mesh meshQuad {
        get { 
            if ( meshQuad_ == null ) {
                meshQuad_ = new Mesh();
                meshQuad_.hideFlags = HideFlags.HideAndDontSave;
            }
            return meshQuad_;
        }
    }

    // materialQuad
    static Material materialQuad_;
    public static Material materialQuad {
        get {
            if ( materialQuad_ == null ) {
                materialQuad_ = new Material( Shader.Find("ex2D/Alpha Blended") );
                materialQuad_.hideFlags = HideFlags.HideAndDontSave;
            }
            return materialQuad_; 
        }
    } 

    // materialLine
    static Material materialLine_;
    public static Material materialLine {
        get {
            if ( materialLine_ == null ) {
                materialLine_ = new Material( "Shader \"Lines/Colored Blended\" {" +
                                             "SubShader { Pass { " +
                                             "    Blend SrcAlpha OneMinusSrcAlpha " +
                                             "    ZWrite Off Cull Off Fog { Mode Off } " +
                                             "    BindChannels {" +
                                             "      Bind \"vertex\", vertex Bind \"color\", color }" +
                                             "} } }" );
                materialLine_.hideFlags = HideFlags.HideAndDontSave;
                materialLine_.shader.hideFlags = HideFlags.HideAndDontSave;
            }
            return materialLine_;
        }
    }

    // materialAlphaBlended
    static Material materialAlphaBlended_;
    public static Material materialAlphaBlended {
        get {
            if ( materialAlphaBlended_ == null ) {
                materialAlphaBlended_ = new Material( Shader.Find("ex2D/Alpha Blended") );
                materialAlphaBlended_.hideFlags = HideFlags.HideAndDontSave;
            }
            return materialAlphaBlended_;
        }
    }

    // materialAlphaBlended
    static Material materialAlphaBlendedVertColor_;
    public static Material materialAlphaBlendedVertColor {
        get {
            if ( materialAlphaBlendedVertColor_ == null ) {
                materialAlphaBlendedVertColor_ = new Material( Shader.Find("ex2D/Alpha Blended (Use Vertex Color)") );
                materialAlphaBlendedVertColor_.hideFlags = HideFlags.HideAndDontSave;
            }
            return materialAlphaBlendedVertColor_;
        }
    }

    // textureCheckerboard
    static Texture2D textureCheckerboard_;
    public static Texture2D textureCheckerboard {
        get {
            if ( textureCheckerboard_ == null ) {
                textureCheckerboard_ = FindTexture ( "checkerboard_64x64.png" ); 
            }
            return textureCheckerboard_;
        }
    }

    // textureHelp
    static Texture2D textureHelp_;
    public static Texture2D textureHelp {
        get {
            if ( textureHelp_ == null ) {
                FindBuiltinTexture( ref textureHelp_, "_help" );
            }
            return textureHelp_;
        }
    }

    // textureAnimationPlay
    static Texture2D textureAnimationPlay_;
    public static Texture2D textureAnimationPlay {
        get {
            if ( textureAnimationPlay_ == null ) {
                FindBuiltinTexture( ref textureAnimationPlay_, "Animation.Play" );
            }
            return textureAnimationPlay_; 
        }
    }

    // textureAnimationNext
    static Texture2D textureAnimationNext_;
    public static Texture2D textureAnimationNext {
        get {
            if ( textureAnimationNext_ == null ) {
                FindBuiltinTexture( ref textureAnimationNext_, "Animation.NextKey" );
            }
            return textureAnimationNext_;
        }
    }

    // textureAnimationPrev
    static Texture2D textureAnimationPrev_;
    public static Texture2D textureAnimationPrev {
        get {
            if ( textureAnimationPrev_ == null ) {
                FindBuiltinTexture( ref textureAnimationPrev_, "Animation.PrevKey" );
            }
            return textureAnimationPrev_;
        }
    }

    // textureAddEvent
    static Texture2D textureAddEvent_;
    public static Texture2D textureAddEvent {
        get {
            if ( textureAddEvent_ == null ) {
                FindBuiltinTexture( ref textureAddEvent_, "Animation.AddEvent" );
            }
            return textureAddEvent_;
        }
    }

    // textureEventMarker
    static Texture2D textureEventMarker_;
    public static Texture2D textureEventMarker {
        get {
            if ( textureEventMarker_ == null ) {
                FindBuiltinTexture( ref textureEventMarker_, "Animation.EventMarker" );
            }
            return textureEventMarker_;
        }
    }

    // styleRectBorder
    static GUIStyle styleRectBorder_ = null;
    public static GUIStyle styleRectBorder {
        get {
            if ( styleRectBorder_ == null ) {
                styleRectBorder_ = new GUIStyle();
                styleRectBorder_.normal.background = FindTexture( "border.png" );
                styleRectBorder_.border = new RectOffset( 2, 2, 2, 2 );
                styleRectBorder_.alignment = TextAnchor.MiddleCenter;
            }
            return styleRectBorder_; 
        }
    }

    //
    public static Vector2[] resolutionList = new Vector2[] {
        new Vector2 ( -1, -1 ),      // None
        new Vector2 ( 320, 480 ),  // iPhone3 Tall
        new Vector2 ( 480, 320 ),  // iPhone3 Wide
        new Vector2 ( 640, 960 ),  // iPhone4 Tall
        new Vector2 ( 960, 640 ),  // iPhone4 Wide
        new Vector2 ( 640, 1136 ), // iPhone5 Tall
        new Vector2 ( 1136, 640 ), // iPhone5 Wide
        new Vector2 ( 768, 1024 ), // iPad Tall
        new Vector2 ( 1024, 768 ), // iPad Wide
        new Vector2 ( 0, 0 ), // Custom
    };
    public static string[] resolutionDescList = new string[] { 
        "None",
        "320 x 480 (iPhone3 Tall)",  // iPhone3 Tall
        "480 x 320 (iPhone3 Wide)",  // iPhone3 Wide
        "640 x 960 (iPhone4 Tall)",  // iPhone4 Tall
        "960 x 640 (iPhone4 Wide)",  // iPhone4 Wide
        "640 x 1136 (iPhone5 Tall)", // iPhone5 Tall
        "1136 x 640 (iPhone5 Wide)", // iPhone5 Wide
        "768 x 1024 (iPad Tall)",    // iPad Tall
        "1024 x 768 (iPad Wide)",    // iPad Wide
        "Custom",
    };

    ///////////////////////////////////////////////////////////////////////////////
    // GUI draws helper 
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GUI_DrawRectBorder ( Rect _rect, Color _borderColor ) {
        // Graphics.DrawTexture( _rect, FindTexture("border.png"), new Rect(0,0,1,1), 2, 2, 2, 2, _borderColor * 0.5f );

        Color old = GUI.backgroundColor;
        GUI.backgroundColor = _borderColor * 2.0f;
            styleRectBorder.Draw( _rect, false, true, true, true );
        GUI.backgroundColor = old;
    }

    // ------------------------------------------------------------------ 
    /// \param _rect the rect
    /// \param _backgroundColor the background color of the rect
    /// \param _borderColor the border color of the rect
    /// draw a rect in editor
    // ------------------------------------------------------------------ 

    public static void GUI_DrawRect ( Rect _rect, Color _backgroundColor, Color _borderColor ) {
        // backgroundColor
        Color old = GUI.color;
        GUI.color = _backgroundColor;
            GUI.DrawTexture( _rect, EditorGUIUtility.whiteTexture );
        GUI.color = old;

        // border
        old = GUI.backgroundColor;
        GUI.backgroundColor = _borderColor;
            styleRectBorder.Draw( _rect, false, true, true, true );
        GUI.backgroundColor = old;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GUI_DrawRawTextureInfo ( Rect _rect, 
                                                exTextureInfo _textureInfo, 
                                                Color _color, 
                                                float _scale = 1.0f,
                                                bool _useTextureOffset = false ) 
    {
        // check texture info 
        if ( _textureInfo == null )
            return;

        // check raw/atlas texture
        Texture2D texture = exEditorUtility.LoadAssetFromGUID<Texture2D>( _textureInfo.rawTextureGUID );
        if ( texture == null )
            return;

        // calculate uv
        Rect uv = new Rect ( (float)_textureInfo.trim_x/(float)texture.width,
                             (float)_textureInfo.trim_y/(float)texture.height,
                             (float)_textureInfo.width/(float)texture.width,
                             (float)_textureInfo.height/(float)texture.height );

        float width = _textureInfo.width * _scale;
        float height = _textureInfo.height * _scale;

        float offsetX = 0.0f;
        float offsetY = 0.0f;

        if ( _useTextureOffset ) {
            offsetX = (_textureInfo.rawWidth - _textureInfo.width) * 0.5f - _textureInfo.trim_x;
            offsetX *= _scale;

            offsetY = (_textureInfo.rawHeight - _textureInfo.height) * 0.5f - _textureInfo.trim_y;
            offsetY *= _scale;
        }

        //
        Rect pos = new Rect( _rect.center.x - width * 0.5f - offsetX,
                             _rect.center.y - height * 0.5f + offsetY,
                             width, 
                             height );

        // draw the texture
        Color old = GUI.color;
        GUI.color = _color;
        GUI.DrawTextureWithTexCoords( pos, texture, uv );
        GUI.color = old;

        // DEBUG { 
        // exEditorUtility.GUI_DrawRectBorder ( _rect, Color.white );
        // } DEBUG end 
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GUI_DrawTextureInfo ( Rect _rect,
                                             exTextureInfo _textureInfo, 
                                             Color _color,
                                             bool _useTextureOffset = false ) {
        if (_textureInfo == null) {
            return;
        }
        if (_textureInfo.texture == null) {
            return;
        }

        float s0 = (float) _textureInfo.x / (float) _textureInfo.texture.width;
        float s1 = (float) (_textureInfo.x+_textureInfo.rotatedWidth)  / (float) _textureInfo.texture.width;
        float t0 = (float) _textureInfo.y / (float) _textureInfo.texture.height;
        float t1 = (float) (_textureInfo.y+_textureInfo.rotatedHeight) / (float) _textureInfo.texture.height;

        materialAlphaBlended.mainTexture = _textureInfo.texture;
        materialAlphaBlended.SetPass(0);
        GL.Begin(GL.QUADS);
            GL.Color(_color);

            if ( _textureInfo.isDiced ) {
                Vector2 texelSize = _textureInfo.texture.texelSize;
                float x_scale = _rect.width/_textureInfo.width;
                float y_scale = _rect.height/_textureInfo.height;

                foreach ( exTextureInfo.Dice dice in _textureInfo.dices ) {
                    if ( dice.sizeType != exTextureInfo.DiceType.Empty ) {
                        s0 = dice.x * texelSize.x;
                        t0 = dice.y * texelSize.y;
                        s1 = (dice.x + dice.rotatedWidth) * texelSize.x;
                        t1 = (dice.y + dice.rotatedHeight) * texelSize.y;
                        float start_x = dice.trim_x * x_scale;
                        float start_y = dice.trim_y * y_scale;
                        float end_x = (dice.trim_x + dice.width) * x_scale;
                        float end_y = (dice.trim_y + dice.height) * y_scale;

                        if ( dice.rotated == false ) {
                            GL.TexCoord2 ( s0, t0 );
                            GL.Vertex3 ( _rect.x + start_x, _rect.yMax - start_y, 0.0f );

                            GL.TexCoord2 ( s0, t1 );
                            GL.Vertex3 ( _rect.x + start_x, _rect.yMax - end_y, 0.0f );

                            GL.TexCoord2 ( s1, t1 );
                            GL.Vertex3 ( _rect.x + end_x, _rect.yMax - end_y, 0.0f );

                            GL.TexCoord2 ( s1, t0 );
                            GL.Vertex3 ( _rect.x + end_x, _rect.yMax - start_y, 0.0f );
                        }
                        else {
                            GL.TexCoord2 ( s1, t0 );
                            GL.Vertex3 ( _rect.x + start_x, _rect.yMax - start_y, 0.0f );

                            GL.TexCoord2 ( s0, t0 );
                            GL.Vertex3 ( _rect.x + start_x, _rect.yMax - end_y, 0.0f );

                            GL.TexCoord2 ( s0, t1 );
                            GL.Vertex3 ( _rect.x + end_x, _rect.yMax - end_y, 0.0f );

                            GL.TexCoord2 ( s1, t1 );
                            GL.Vertex3 ( _rect.x + end_x, _rect.yMax - start_y, 0.0f );
                        }
                    }
                }
            }
            else {
                if ( _textureInfo.rotated == false ) {
                    GL.TexCoord2 ( s0, t0 );
                    GL.Vertex3 ( _rect.x, _rect.yMax, 0.0f );

                    GL.TexCoord2 ( s0, t1 );
                    GL.Vertex3 ( _rect.x, _rect.y, 0.0f );

                    GL.TexCoord2 ( s1, t1 );
                    GL.Vertex3 ( _rect.xMax, _rect.y, 0.0f );

                    GL.TexCoord2 ( s1, t0 );
                    GL.Vertex3 ( _rect.xMax, _rect.yMax, 0.0f );
                }
                else {
                    GL.TexCoord2 ( s1, t0 );
                    GL.Vertex3 ( _rect.x, _rect.yMax, 0.0f );

                    GL.TexCoord2 ( s0, t0 );
                    GL.Vertex3 ( _rect.x, _rect.y, 0.0f );

                    GL.TexCoord2 ( s0, t1 );
                    GL.Vertex3 ( _rect.xMax, _rect.y, 0.0f );

                    GL.TexCoord2 ( s1, t1 );
                    GL.Vertex3 ( _rect.xMax, _rect.yMax, 0.0f );
                }
            }

        GL.End();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // GL Draw Helper
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \param _xStart the start point x
    /// \param _yStart the start point y
    /// \param _xEnd the end point x
    /// \param _yEnd the end point y
    /// \param _color the color of the line
    /// \param _width the width of the line
    /// draw a line in editor
    // ------------------------------------------------------------------ 

    public static void GL_DrawLineAA ( float _xStart, float _yStart, 
                                       float _xEnd, float _yEnd,
                                       Color _color, 
                                       int _width ) 
    {
        int xStart = (int)_xStart;
        int yStart = (int)_yStart;
        int xEnd = (int)_xEnd;
        int yEnd = (int)_yEnd;

        Vector3 a = new Vector3( xStart, yStart, 0 );
        Vector3 b = new Vector3( xEnd, yEnd, 0 );

        if ( (b - a).sqrMagnitude <= 0.0001f )
            return;

        Color savedColor = Handles.color;
        Handles.color = _color;
        if ( _width > 1 )
            Handles.DrawAAPolyLine(_width, new Vector3[] {a,b} );
        else
            Handles.DrawLine( a, b );
        Handles.color = savedColor;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_DrawLine ( float _start_x, float _start_y, float _end_x, float _end_y, Color _color ) {
        materialLine.SetPass(0);
        GL.Begin(GL.LINES);
            GL.Color(_color);
            GL.Vertex3( _start_x, _start_y, 0.0f );
            GL.Vertex3( _end_x, _end_y, 0.0f );
        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_DrawLines ( Vector2[] _points, Color _color ) {
        if ( _points.Length < 2 ) {
            Debug.LogWarning ("Failed to call GL_DrawLines, not enough points");
            return;
        }

        materialLine.SetPass(0);
        GL.Begin( GL.LINES );
            GL.Color(_color);

            for ( int i = 0; i < _points.Length; i += 2 ) {
                GL.Vertex3( _points[i+0].x, _points[i+0].y, 0.0f );
                GL.Vertex3( _points[i+1].x, _points[i+1].y, 0.0f );
            }
        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_DrawRectLine ( Vector3[] _points, Color _color, bool ignoreZ = true ) {
        if ( _points.Length % 4 != 0 ) {
            Debug.LogWarning ("Failed to call GL_DrawRectLine, not enough points");
            return;
        }

        materialLine.SetPass(0);
        GL.Begin( GL.LINES );
            GL.Color(_color);
            if (ignoreZ) {
                for ( int i = 0; i < _points.Length; i += 4 ) {
                    GL.Vertex3( _points[i+0].x, _points[i+0].y, 0.0f );
                    GL.Vertex3( _points[i+1].x, _points[i+1].y, 0.0f );

                    GL.Vertex3( _points[i+1].x, _points[i+1].y, 0.0f );
                    GL.Vertex3( _points[i+2].x, _points[i+2].y, 0.0f );

                    GL.Vertex3( _points[i+2].x, _points[i+2].y, 0.0f );
                    GL.Vertex3( _points[i+3].x, _points[i+3].y, 0.0f );

                    GL.Vertex3( _points[i+3].x, _points[i+3].y, 0.0f );
                    GL.Vertex3( _points[i+0].x, _points[i+0].y, 0.0f );
                }
            }
            else {
                for ( int i = 0; i < _points.Length; i += 4 ) {
                    GL.Vertex3( _points[i+0].x, _points[i+0].y, _points[i+0].z );
                    GL.Vertex3( _points[i+1].x, _points[i+1].y, _points[i+1].z );

                    GL.Vertex3( _points[i+1].x, _points[i+1].y, _points[i+1].z );
                    GL.Vertex3( _points[i+2].x, _points[i+2].y, _points[i+2].z );

                    GL.Vertex3( _points[i+2].x, _points[i+2].y, _points[i+2].z );
                    GL.Vertex3( _points[i+3].x, _points[i+3].y, _points[i+3].z );

                    GL.Vertex3( _points[i+3].x, _points[i+3].y, _points[i+3].z );
                    GL.Vertex3( _points[i+0].x, _points[i+0].y, _points[i+0].z );
                }
            }
        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_DrawTexture ( Vector2 _center, 
                                        Vector2 _size, 
                                        Texture2D _texture, 
                                        Rect _uv, 
                                        Color _color ) {

        float s0 = _uv.xMin;
        float s1 = _uv.xMax;
        float t0 = _uv.yMin;
        float t1 = _uv.yMax;
        float half_w = _size.x * 0.5f;
        float half_h = _size.y * 0.5f;

        materialAlphaBlended.mainTexture = _texture;
        materialAlphaBlended.SetPass(0);

        GL.Begin(GL.QUADS);
            GL.Color(_color);

            GL.TexCoord2 ( s0, t0 );
            GL.Vertex3 ( -half_w + _center.x, -half_h + _center.y, 0.0f );

            GL.TexCoord2 ( s0, t1 );
            GL.Vertex3 ( -half_w + _center.x,  half_h + _center.y, 0.0f );

            GL.TexCoord2 ( s1, t1 );
            GL.Vertex3 (  half_w + _center.x,  half_h + _center.y, 0.0f );

            GL.TexCoord2 ( s1, t0 );
            GL.Vertex3 (  half_w + _center.x, -half_h + _center.y, 0.0f );
        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_DrawTextureInfo ( exTextureInfo _textureInfo, Vector2 _pos, Color _color ) {
        if (_textureInfo == null) {
            return;
        }
        if (_textureInfo.texture == null) {
            return;
        }

        Vector2 halfSize = new Vector2( _textureInfo.width * 0.5f,
                                        _textureInfo.height * 0.5f );

        float s0 = (float) _textureInfo.x / (float) _textureInfo.texture.width;
        float s1 = (float) (_textureInfo.x+_textureInfo.rotatedWidth)  / (float) _textureInfo.texture.width;
        float t0 = (float) _textureInfo.y / (float) _textureInfo.texture.height;
        float t1 = (float) (_textureInfo.y+_textureInfo.rotatedHeight) / (float) _textureInfo.texture.height;

        materialAlphaBlended.mainTexture = _textureInfo.texture;
        materialAlphaBlended.SetPass(0);
        GL.Begin(GL.QUADS);
            GL.Color(_color);

            if ( _textureInfo.isDiced ) {
                Vector2 texelSize = _textureInfo.texture.texelSize;

                foreach ( exTextureInfo.Dice dice in _textureInfo.dices ) {
                    if ( dice.sizeType != exTextureInfo.DiceType.Empty ) {
                        s0 = dice.x * texelSize.x;
                        t0 = dice.y * texelSize.y;
                        s1 = (dice.x + dice.rotatedWidth) * texelSize.x;
                        t1 = (dice.y + dice.rotatedHeight) * texelSize.y;

                        if ( dice.rotated == false ) {
                            GL.TexCoord2 ( s0, t0 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x,
                                         -halfSize.y + _pos.y + dice.trim_y, 
                                         0.0f );

                            GL.TexCoord2 ( s0, t1 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x,
                                         -halfSize.y + _pos.y + dice.trim_y + dice.height,
                                         0.0f );

                            GL.TexCoord2 ( s1, t1 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x + dice.width,
                                         -halfSize.y + _pos.y + dice.trim_y + dice.height,
                                         0.0f );

                            GL.TexCoord2 ( s1, t0 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x + dice.width,
                                         -halfSize.y + _pos.y + dice.trim_y,
                                         0.0f );
                        }
                        else {
                            GL.TexCoord2 ( s1, t0 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x,
                                         -halfSize.y + _pos.y + dice.trim_y, 
                                         0.0f );

                            GL.TexCoord2 ( s0, t0 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x,
                                         -halfSize.y + _pos.y + dice.trim_y + dice.height,
                                         0.0f );

                            GL.TexCoord2 ( s0, t1 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x + dice.width,
                                         -halfSize.y + _pos.y + dice.trim_y + dice.height,
                                         0.0f );

                            GL.TexCoord2 ( s1, t1 );
                            GL.Vertex3 ( -halfSize.x + _pos.x + dice.trim_x + dice.width,
                                         -halfSize.y + _pos.y + dice.trim_y,
                                         0.0f );
                        }
                    }
                }
            }
            else {
                if ( _textureInfo.rotated == false ) {
                    GL.TexCoord2 ( s0, t0 );
                    GL.Vertex3 ( -halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );

                    GL.TexCoord2 ( s0, t1 );
                    GL.Vertex3 ( -halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                    GL.TexCoord2 ( s1, t1 );
                    GL.Vertex3 (  halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                    GL.TexCoord2 ( s1, t0 );
                    GL.Vertex3 (  halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );
                }
                else {
                    GL.TexCoord2 ( s1, t0 );
                    GL.Vertex3 ( -halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );

                    GL.TexCoord2 ( s0, t0 );
                    GL.Vertex3 ( -halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                    GL.TexCoord2 ( s0, t1 );
                    GL.Vertex3 (  halfSize.x + _pos.x,  halfSize.y + _pos.y, 0.0f );

                    GL.TexCoord2 ( s1, t1 );
                    GL.Vertex3 (  halfSize.x + _pos.x, -halfSize.y + _pos.y, 0.0f );
                }
            }

        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_DrawWireFrame ( exPlane _node, Color _color, bool ignoreZ = true ) {
        exISprite sprite = _node as exISprite;
        if ( sprite == null || sprite.vertexCount < 1000) {
            Vector3[] vertices = _node.GetWorldVertices();
            if (vertices.Length > 0) {
                if ( sprite != null && sprite.spriteType == exSpriteType.Sliced) {
                    Vector3[] rectVertices = new Vector3[16];
                    rectVertices[0] = vertices[0];
                    rectVertices[1] = vertices[4];
                    rectVertices[2] = vertices[7];
                    rectVertices[3] = vertices[3];
                    rectVertices[4] = vertices[8];
                    rectVertices[5] = vertices[12];
                    rectVertices[6] = vertices[15];
                    rectVertices[7] = vertices[11];
                    rectVertices[8] = vertices[0];
                    rectVertices[9] = vertices[12];
                    rectVertices[10] = vertices[13];
                    rectVertices[11] = vertices[1];
                    rectVertices[12] = vertices[2];
                    rectVertices[13] = vertices[14];
                    rectVertices[14] = vertices[15];
                    rectVertices[15] = vertices[3];
                    vertices = rectVertices;
                }
                GL_DrawRectLine(vertices, _color, ignoreZ);
            }
        }
        else {
            Vector3[] vertices = _node.GetLocalVertices();
            if (vertices.Length > 0) {
                Rect aabb = exGeometryUtility.GetAABoundingRect(vertices);
                Matrix4x4 l2w = _node.transform.localToWorldMatrix;
                vertices = new Vector3[4] {
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMin, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMax, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMax, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMin, 0)),
                };
                GL_DrawRectLine(vertices, _color, ignoreZ);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    // GL_UI
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_UI_DrawBorderRectangle ( float _x, float _y, float _width, float _height, 
                                                   float _top, float _right, float _bottom, float _left, 
                                                   Color _color ) 
    {
        //  
        //  0--------------------------1
        //  |   |                  |   |
        //  |---4------------------5---|
        //  |   |                  |   |
        //  |   |                  |   |
        //  |   |                  |   |
        //  |---7------------------6---|
        //  |   |                  |   |
        //  3--------------------------2
        //  

        // GL.Vertex3( _x, _y, 0.0f ); // 0
        // GL.Vertex3( _x + _width, _y, 0.0f ); // 1
        // GL.Vertex3( _x + _width, _y + _height, 0.0f ); // 2
        // GL.Vertex3( _x, _y + _height, 0.0f ); // 3
        // GL.Vertex3( _x + _left, _y + _top, 0.0f ); // 4
        // GL.Vertex3( _x + _width - _right, _y + _top , 0.0f ); // 5
        // GL.Vertex3( _x + _width - _right, _y + _height - _bottom, 0.0f ); // 6
        // GL.Vertex3( _x + _left, _y + _height - _bottom, 0.0f ); // 7

        materialLine.SetPass(0);
        GL.Begin(GL.TRIANGLES);
            GL.Color(_color);
            GL.Vertex3( _x, _y, 0.0f ); // 0
            GL.Vertex3( _x + _width, _y, 0.0f ); // 1
            GL.Vertex3( _x + _left, _y + _top, 0.0f ); // 4
            GL.Vertex3( _x + _left, _y + _top, 0.0f ); // 4
            GL.Vertex3( _x + _width, _y, 0.0f ); // 1
            GL.Vertex3( _x + _width - _right, _y + _top , 0.0f ); // 5

            GL.Vertex3( _x + _width - _right, _y + _top , 0.0f ); // 5
            GL.Vertex3( _x + _width, _y, 0.0f ); // 1
            GL.Vertex3( _x + _width - _right, _y + _height - _bottom, 0.0f ); // 6
            GL.Vertex3( _x + _width - _right, _y + _height - _bottom, 0.0f ); // 6
            GL.Vertex3( _x + _width, _y, 0.0f ); // 1
            GL.Vertex3( _x + _width, _y + _height, 0.0f ); // 2

            GL.Vertex3( _x, _y + _height, 0.0f ); // 3
            GL.Vertex3( _x + _left, _y + _height - _bottom, 0.0f ); // 7
            GL.Vertex3( _x + _width - _right, _y + _height - _bottom, 0.0f ); // 6
            GL.Vertex3( _x, _y + _height, 0.0f ); // 3
            GL.Vertex3( _x + _width - _right, _y + _height - _bottom, 0.0f ); // 6
            GL.Vertex3( _x + _width, _y + _height, 0.0f ); // 2

            GL.Vertex3( _x, _y + _height, 0.0f ); // 3
            GL.Vertex3( _x, _y, 0.0f ); // 0
            GL.Vertex3( _x + _left, _y + _top, 0.0f ); // 4
            GL.Vertex3( _x, _y + _height, 0.0f ); // 3
            GL.Vertex3( _x + _left, _y + _top, 0.0f ); // 4
            GL.Vertex3( _x + _left, _y + _height - _bottom, 0.0f ); // 7
        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_UI_DrawBorderTexture ( float _x, float _y, float _width, float _height, 
                                                 float _top, float _right, float _bottom, float _left, 
                                                 float _uv_top, float _uv_right, float _uv_bottom, float _uv_left, 
                                                 float _s0, float _t0, float _s1, float _t1,
                                                 Texture2D _texture,
                                                 Color _color,
                                                 bool _rotated ) 
    {
        //  
        //  0---1------------------2---3
        //  |   |                  |   |
        //  4---5------------------6---7
        //  |   |                  |   |
        //  |   |                  |   |
        //  |   |                  |   |
        //  8---9------------------10--11
        //  |   |                  |   |
        //  12--13-----------------14--15
        //  

        // GL.Vertex3( _x,            _y, 0.0f ); // 0
        // GL.Vertex3( _x + x_step_1, _y, 0.0f ); // 1
        // GL.Vertex3( _x + x_step_2, _y, 0.0f ); // 2
        // GL.Vertex3( _x + _width,   _y, 0.0f ); // 3

        // GL.Vertex3( _x,            _y + y_step_1, 0.0f ); // 4
        // GL.Vertex3( _x + x_step_1, _y + y_step_1, 0.0f ); // 5
        // GL.Vertex3( _x + x_step_2, _y + y_step_1, 0.0f ); // 6
        // GL.Vertex3( _x + _width,   _y + y_step_1, 0.0f ); // 7

        // GL.Vertex3( _x,            _y + y_step_2, 0.0f ); // 8 
        // GL.Vertex3( _x + x_step_1, _y + y_step_2, 0.0f ); // 9 
        // GL.Vertex3( _x + x_step_2, _y + y_step_2, 0.0f ); // 10
        // GL.Vertex3( _x + _width,   _y + y_step_2, 0.0f ); // 11

        // GL.Vertex3( _x,            _y + _height, 0.0f ); // 12
        // GL.Vertex3( _x + x_step_1, _y + _height, 0.0f ); // 13
        // GL.Vertex3( _x + x_step_2, _y + _height, 0.0f ); // 14
        // GL.Vertex3( _x + _width,   _y + _height, 0.0f ); // 15

        // GL.TexCoord2 ( s0, t3 ); // 0
        // GL.TexCoord2 ( s1, t3 ); // 1
        // GL.TexCoord2 ( s2, t3 ); // 2
        // GL.TexCoord2 ( s3, t3 ); // 3

        // GL.TexCoord2 ( s0, t2 ); // 4
        // GL.TexCoord2 ( s1, t2 ); // 5
        // GL.TexCoord2 ( s2, t2 ); // 6
        // GL.TexCoord2 ( s3, t2 ); // 7

        // GL.TexCoord2 ( s0, t1 ); // 8 
        // GL.TexCoord2 ( s1, t1 ); // 9 
        // GL.TexCoord2 ( s2, t1 ); // 10
        // GL.TexCoord2 ( s3, t1 ); // 11

        // GL.TexCoord2 ( s0, t0 ); // 12
        // GL.TexCoord2 ( s1, t0 ); // 13
        // GL.TexCoord2 ( s2, t0 ); // 14
        // GL.TexCoord2 ( s3, t0 ); // 15

        float x_step_1 = _left;
        float x_step_2 = _width - _right;
        float y_step_1 = _top;
        float y_step_2 = _height - _bottom;

        float s0, s1, s2, s3;
        float t0, t1, t2, t3;

        Vector2[] uv = null;

        if ( _rotated == false ) {
            s0 = _s0;
            s1 = _s0 + _uv_left * _texture.texelSize.x;
            s2 = _s1 - _uv_right * _texture.texelSize.x;
            s3 = _s1;

            t0 = _t0;
            t1 = _t0 + _uv_bottom * _texture.texelSize.y;
            t2 = _t1 - _uv_top * _texture.texelSize.y;
            t3 = _t1;

            uv = new Vector2[] {
                new Vector2 ( s0, t3 ),
                new Vector2 ( s1, t3 ),
                new Vector2 ( s2, t3 ),
                new Vector2 ( s3, t3 ),

                new Vector2 ( s0, t2 ),
                new Vector2 ( s1, t2 ),
                new Vector2 ( s2, t2 ),
                new Vector2 ( s3, t2 ),

                new Vector2 ( s0, t1 ),
                new Vector2 ( s1, t1 ),
                new Vector2 ( s2, t1 ),
                new Vector2 ( s3, t1 ),

                new Vector2 ( s0, t0 ),
                new Vector2 ( s1, t0 ),
                new Vector2 ( s2, t0 ),
                new Vector2 ( s3, t0 ),
            };
        }
        else {
            s0 = _s0;
            s1 = _s0 + _uv_top * _texture.texelSize.x;
            s2 = _s1 - _uv_bottom * _texture.texelSize.x;
            s3 = _s1;

            t0 = _t0;
            t1 = _t0 + _uv_left * _texture.texelSize.y;
            t2 = _t1 - _uv_right * _texture.texelSize.y;
            t3 = _t1;

            uv = new Vector2[] {
                new Vector2 ( s0, t0 ),
                new Vector2 ( s0, t1 ),
                new Vector2 ( s0, t2 ),
                new Vector2 ( s0, t3 ),

                new Vector2 ( s1, t0 ),
                new Vector2 ( s1, t1 ),
                new Vector2 ( s1, t2 ),
                new Vector2 ( s1, t3 ),

                new Vector2 ( s2, t0 ),
                new Vector2 ( s2, t1 ),
                new Vector2 ( s2, t2 ),
                new Vector2 ( s2, t3 ),

                new Vector2 ( s3, t0 ),
                new Vector2 ( s3, t1 ),
                new Vector2 ( s3, t2 ),
                new Vector2 ( s3, t3 ),
            };
        }

        Vector3[] pos = new Vector3[] {
            new Vector3 ( _x,            _y, 0.0f ),
            new Vector3 ( _x + x_step_1, _y, 0.0f ),
            new Vector3 ( _x + x_step_2, _y, 0.0f ),
            new Vector3 ( _x + _width,   _y, 0.0f ),

            new Vector3 ( _x,            _y + y_step_1, 0.0f ),
            new Vector3 ( _x + x_step_1, _y + y_step_1, 0.0f ),
            new Vector3 ( _x + x_step_2, _y + y_step_1, 0.0f ),
            new Vector3 ( _x + _width,   _y + y_step_1, 0.0f ),

            new Vector3 ( _x,            _y + y_step_2, 0.0f ),
            new Vector3 ( _x + x_step_1, _y + y_step_2, 0.0f ),
            new Vector3 ( _x + x_step_2, _y + y_step_2, 0.0f ),
            new Vector3 ( _x + _width,   _y + y_step_2, 0.0f ),

            new Vector3 ( _x,            _y + _height, 0.0f ),
            new Vector3 ( _x + x_step_1, _y + _height, 0.0f ),
            new Vector3 ( _x + x_step_2, _y + _height, 0.0f ),
            new Vector3 ( _x + _width,   _y + _height, 0.0f ),
        };
        int[] indices = new int[] {
            0, 1, 2, 4, 5, 6, 8, 9, 10 
        };

        materialAlphaBlended.mainTexture = _texture;
        materialAlphaBlended.SetPass(0);
        GL.Begin(GL.TRIANGLES);
            GL.Color(_color);

            for ( int i = 0; i < indices.Length; ++i ) {
                int idx = indices[i];
                GL.TexCoord2 ( uv[idx+0].x,  uv[idx+0].y );
                GL.Vertex3   ( pos[idx+0].x, pos[idx+0].y, pos[idx+0].z );

                GL.TexCoord2 ( uv[idx+1].x,  uv[idx+1].y );
                GL.Vertex3   ( pos[idx+1].x, pos[idx+1].y, pos[idx+1].z );

                GL.TexCoord2 ( uv[idx+4].x,  uv[idx+4].y );
                GL.Vertex3   ( pos[idx+4].x, pos[idx+4].y, pos[idx+4].z );

                GL.TexCoord2 ( uv[idx+4].x,  uv[idx+4].y );
                GL.Vertex3   ( pos[idx+4].x, pos[idx+4].y, pos[idx+4].z );

                GL.TexCoord2 ( uv[idx+1].x,  uv[idx+1].y );
                GL.Vertex3   ( pos[idx+1].x, pos[idx+1].y, pos[idx+1].z );

                GL.TexCoord2 ( uv[idx+5].x,  uv[idx+5].y );
                GL.Vertex3   ( pos[idx+5].x, pos[idx+5].y, pos[idx+5].z );
            }
        GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void GL_UI_DrawRectangle ( float _x, float _y, float _width, float _height, 
                                             Color _color ) 
    {
        materialLine.SetPass(0);
        GL.Begin(GL.TRIANGLES);
            GL.Color(_color);
            GL.Vertex3( _x, _y, 0.0f ); 
            GL.Vertex3( _x + _width, _y, 0.0f );
            GL.Vertex3( _x + _width, _y + _height , 0.0f );
            GL.Vertex3( _x, _y, 0.0f ); 
            GL.Vertex3( _x + _width, _y + _height , 0.0f );
            GL.Vertex3( _x, _y + _height , 0.0f );
        GL.End();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // assets
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public static string SaveFileInProject ( string _title, string _dirPath, string _fileName, string _extension ) {
		string path = EditorUtility.SaveFilePanel(_title, _dirPath, _fileName, _extension);

        // cancelled
		if ( path.Length == 0 )
			return "";

		string cwd = System.IO.Directory.GetCurrentDirectory().Replace("\\","/") + "/assets/";
		if ( path.ToLower().IndexOf(cwd.ToLower()) != 0 ) {
			path = "";
			EditorUtility.DisplayDialog(_title, "Assets must be saved inside the Assets folder", "Ok");
		}
		else {
			path = path.Substring ( cwd.Length - "/assets".Length );
		}
		return path;
	}

    // ------------------------------------------------------------------ 
    /// \param _o the asset object
    /// \return the result
    /// check if the asset is a directory
    // ------------------------------------------------------------------ 

    public static bool IsDirectory ( Object _o ) {
        string path = AssetDatabase.GetAssetPath(_o);
        if ( string.IsNullOrEmpty(path) == false ) {
            DirectoryInfo info = new DirectoryInfo(path);
            return info.Exists;
        }
        return false;
    }

    // ------------------------------------------------------------------ 
    /// \param _o the asset object
    /// \return the guid
    /// get the guid of the asset path, if not found, return empty string
    // ------------------------------------------------------------------ 

    public static string AssetToGUID ( Object _o ) {
        if ( _o == null )
            return "";
        return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_o));
    }

    // ------------------------------------------------------------------ 
    /// \param _guid asset path guid
    /// \return the asset
    /// load the asset from path guid
    // ------------------------------------------------------------------ 

    public static T LoadAssetFromGUID<T> ( string _guid ) where T : Object {
        if ( string.IsNullOrEmpty(_guid) )
            return null;
        string assetPath = AssetDatabase.GUIDToAssetPath(_guid);
        return AssetDatabase.LoadAssetAtPath( assetPath, typeof(T) ) as T;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // texture
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static float CalculateTextureInfoScale ( Rect _rect, exTextureInfo _textureInfo ) {
        float scale = 1.0f;
        if ( _textureInfo == null )
            return scale;

        float width = _textureInfo.width;
        float height = _textureInfo.height;

        // confirm the scale, width and height
        if ( width > _rect.width && height > _rect.height ) {
            scale = Mathf.Min( _rect.width / width, 
                               _rect.height / height );
        }
        else if ( width > _rect.width ) {
            scale = _rect.width / width;
        }
        else if ( height > _rect.height ) {
            scale = _rect.height / height;
        }
        return scale;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static Texture2D FindBuiltinTexture ( ref Texture2D _texture, string _name ) {
        // NOTE: hack from "unity editor resources" in Unity3D Contents
        if ( _texture == null ) {
            _texture = EditorGUIUtility.FindTexture(_name);
            if ( _texture == null ) {
                Debug.LogError ( string.Format ( "can't find {0} texture", _name ) );
                return null;
            }
        }
        return _texture;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static Texture2D FindTexture ( string _name ) {
        string path = "Assets/ex2D/Editor/Res/Textures";
        path = Path.Combine ( path, _name );
        Texture2D tex = AssetDatabase.LoadAssetAtPath( path, typeof(Texture2D) ) as Texture2D;
        if ( tex == null ) {
            Debug.LogError ( "can't find texture at " + path );
            return null;
        }
        return tex;
    }

    // ------------------------------------------------------------------ 
    /// \param _tex
    /// \return is valid or not
    /// check if the texture settings is valid for atlas build
    // ------------------------------------------------------------------ 

    public static bool IsValidForAtlas ( Texture2D _tex ) {
        string path = AssetDatabase.GetAssetPath(_tex);
        TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
        if ( importer.textureType != TextureImporterType.Advanced ||
             importer.textureFormat != TextureImporterFormat.AutomaticTruecolor ||
             importer.npotScale != TextureImporterNPOTScale.None ||
             importer.isReadable != true ||
             importer.mipmapEnabled != false )
        {
            return false;
        }
        return true;
    }

    // ------------------------------------------------------------------ 
    /// \param _tex
    /// change the import texture settings to make it fit for atlas 
    // ------------------------------------------------------------------ 

    public static void ImportTextureForAtlas ( Texture2D _tex ) {
        string path = AssetDatabase.GetAssetPath(_tex);
        TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;

        importer.textureFormat = TextureImporterFormat.AutomaticTruecolor;
        importer.textureType = TextureImporterType.Advanced;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
    }

    // ------------------------------------------------------------------ 
    /// \param _tex
    /// \return is valid or not
    /// check if the texture settings is valid for atlas build
    // ------------------------------------------------------------------ 

    public static bool IsValidForBitmapFont ( Texture2D _tex ) {
        string path = AssetDatabase.GetAssetPath(_tex);
        TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
        if ( importer.npotScale != TextureImporterNPOTScale.None ||
             importer.mipmapEnabled != false )
        {
            return false;
        }
        return true;
    }

    // ------------------------------------------------------------------ 
    /// \param _tex
    /// change the import texture settings to make it fit for atlas 
    // ------------------------------------------------------------------ 

    public static void ImportTextureForBitmapFont ( Texture2D _tex ) {
        string path = AssetDatabase.GetAssetPath(_tex);
        TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;

        importer.npotScale = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
    }
}
