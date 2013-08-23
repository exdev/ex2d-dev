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
    //
    ///////////////////////////////////////////////////////////////////////////////

    public static Mesh meshQuad;

    public static Material materialQuad;
    public static Material materialLine;
    public static Material materialAlphaBlended;

    public static Texture2D textureCheckerboard;
    public static Texture2D textureHelp;
    public static Texture2D textureAnimationPlay;
    public static Texture2D textureAnimationNext;
    public static Texture2D textureAnimationPrev;
    public static Texture2D textureAddEvent;
    public static Texture2D textureEventMarker;

    public static GUIStyle styleRectBorder = null;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////
    
    public static void Init () {
        // meshes
        meshQuad = new Mesh();
        meshQuad.hideFlags = HideFlags.HideAndDontSave;

        // materials
        materialQuad = new Material( Shader.Find("ex2D/Alpha Blended") );
        materialQuad.hideFlags = HideFlags.HideAndDontSave;

        materialAlphaBlended = new Material( Shader.Find("ex2D/Alpha Blended") );
        materialAlphaBlended.hideFlags = HideFlags.HideAndDontSave;

        materialLine = new Material( "Shader \"Lines/Colored Blended\" {" +
                                     "SubShader { Pass { " +
                                     "    Blend SrcAlpha OneMinusSrcAlpha " +
                                     "    ZWrite Off Cull Off Fog { Mode Off } " +
                                     "    BindChannels {" +
                                     "      Bind \"vertex\", vertex Bind \"color\", color }" +
                                     "} } }" );
        materialLine.hideFlags = HideFlags.HideAndDontSave;
        materialLine.shader.hideFlags = HideFlags.HideAndDontSave;

        // textures
        textureCheckerboard = FindTexture ( "checkerboard_64x64.png" ); 
        FindBuiltinTexture( ref textureHelp, "_help" );
        FindBuiltinTexture( ref textureAnimationPlay, "Animation.Play" );
        FindBuiltinTexture( ref textureAnimationNext, "Animation.NextKey" );
        FindBuiltinTexture( ref textureAnimationPrev, "Animation.PrevKey" );
        FindBuiltinTexture( ref textureAddEvent, "Animation.AddEvent" );
        FindBuiltinTexture( ref textureEventMarker, "Animation.EventMarker" );

        // styles
        styleRectBorder = new GUIStyle();
        styleRectBorder.normal.background = FindTexture( "border.png" );
        styleRectBorder.border = new RectOffset( 2, 2, 2, 2 );
        styleRectBorder.alignment = TextAnchor.MiddleCenter;
    }

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

    public static void GL_DrawRectLine ( Vector3[] _points, Color _color ) {
        if ( _points.Length < 4 ) {
            Debug.LogWarning ("Failed to call GL_DrawRectLine, not enough points");
            return;
        }

        materialLine.SetPass(0);
        GL.Begin( GL.LINES );
            GL.Color(_color);

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

    static Texture2D FindBuiltinTexture ( ref Texture2D _texture, string _name ) {
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
}
