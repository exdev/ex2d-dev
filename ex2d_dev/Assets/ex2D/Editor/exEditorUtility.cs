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

    static Material materialLine;
    static Material materialAlphaBlended;

    static Texture2D textureCheckerboard;
    static Texture2D textureHelp;
    static Texture2D textureAnimationPlay;
    static Texture2D textureAnimationNext;
    static Texture2D textureAnimationPrev;
    static Texture2D textureAddEvent;
    static Texture2D textureEventMarker;

    static GUIStyle styleRectBorder = null;

    ///////////////////////////////////////////////////////////////////////////////
    // special material
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static Material LineMaterial () {
        if ( materialLine == null ) {
            materialLine = new Material( "Shader \"Lines/Colored Blended\" {" +
                                    "SubShader { Pass { " +
                                    "    Blend SrcAlpha OneMinusSrcAlpha " +
                                    "    ZWrite Off Cull Off Fog { Mode Off } " +
                                    "    BindChannels {" +
                                    "      Bind \"vertex\", vertex Bind \"color\", color }" +
                                    "} } }" );
            materialLine.hideFlags = HideFlags.HideAndDontSave;
            materialLine.shader.hideFlags = HideFlags.HideAndDontSave;
        }
        return materialLine;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static Material AlphaBlendedMaterial () {
        if ( materialAlphaBlended == null ) {
            materialAlphaBlended = new Material( Shader.Find("ex2D/Alpha Blended") );
        }
        return materialAlphaBlended;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // special texture
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

    public static Texture2D HelpTexture () { return FindBuiltinTexture( ref textureHelp, "_help" ); }
    public static Texture2D AnimationPlayTexture () { return FindBuiltinTexture( ref textureAnimationPlay, "Animation.Play" ); }
    public static Texture2D AnimationNextTexture () { return FindBuiltinTexture( ref textureAnimationNext, "Animation.NextKey" ); }
    public static Texture2D AnimationPrevTexture () { return FindBuiltinTexture( ref textureAnimationPrev, "Animation.PrevKey" ); }
    public static Texture2D AddEventTexture () { return FindBuiltinTexture( ref textureAddEvent, "Animation.AddEvent" ); }
    public static Texture2D EventMarkerTexture () { return FindBuiltinTexture( ref textureEventMarker, "Animation.EventMarker" ); }

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
    /// \return the checkerboard texture
    /// return a checkerboard texture
    // ------------------------------------------------------------------ 

    public static Texture2D CheckerboardTexture () {
        if ( textureCheckerboard == null )
            textureCheckerboard = FindTexture ( "checkerboard_64x64.png" ); 
        return textureCheckerboard;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // styles
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \return the new gui style
    /// create rect border gui style
    // ------------------------------------------------------------------ 

    public static GUIStyle RectBorderStyle () {
        // create sprite select box style
        if ( styleRectBorder == null ) {
            styleRectBorder = new GUIStyle();
            styleRectBorder.normal.background = FindTexture( "border.png" );
            styleRectBorder.border = new RectOffset( 2, 2, 2, 2 );
            styleRectBorder.alignment = TextAnchor.MiddleCenter;
        }
        return styleRectBorder; 
    }

    ///////////////////////////////////////////////////////////////////////////////
    // draws
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void DrawRectBorder ( Rect _rect, Color _borderColor ) {
        // Graphics.DrawTexture( _rect, FindTexture("border.png"), new Rect(0,0,1,1), 2, 2, 2, 2, _borderColor * 0.5f );

        Color old = GUI.backgroundColor;
        GUI.backgroundColor = _borderColor * 2.0f;
            RectBorderStyle().Draw( _rect, false, true, true, true );
        GUI.backgroundColor = old;
    }

    // ------------------------------------------------------------------ 
    /// \param _rect the rect
    /// \param _backgroundColor the background color of the rect
    /// \param _borderColor the border color of the rect
    /// draw a rect in editor
    // ------------------------------------------------------------------ 

    public static void DrawRect ( Rect _rect, Color _backgroundColor, Color _borderColor ) {
        // backgroundColor
        Color old = GUI.color;
        GUI.color = _backgroundColor;
            GUI.DrawTexture( _rect, EditorGUIUtility.whiteTexture );
        GUI.color = old;

        // border
        old = GUI.backgroundColor;
        GUI.backgroundColor = _borderColor;
            RectBorderStyle().Draw( _rect, false, true, true, true );
        GUI.backgroundColor = old;
    }

    // ------------------------------------------------------------------ 
    /// \param _xStart the start point x
    /// \param _yStart the start point y
    /// \param _xEnd the end point x
    /// \param _yEnd the end point y
    /// \param _color the color of the line
    /// \param _width the width of the line
    /// draw a line in editor
    // ------------------------------------------------------------------ 

    public static void DrawLine ( float _xStart, float _yStart, 
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
            else {
                Handles.DrawLine( a, b );
            }
        Handles.color = savedColor;

        // Material line2D = (Material)EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");
        // line2D.SetPass(0);
        // GL.Begin( GL.LINES );
        // GL.Color(_color);
        // GL.Vertex3( xStart, yStart, 0 );
        // GL.Vertex3( xEnd, yEnd, 0 );
        // GL.End();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void DrawRectLine ( Vector3[] _points, Color _color ) {
        LineMaterial().SetPass(0);
        GL.Begin( GL.LINES );
            GL.Color(_color);

            GL.Vertex3( _points[0].x, _points[0].y, 0.0f );
            GL.Vertex3( _points[1].x, _points[1].y, 0.0f );

            GL.Vertex3( _points[1].x, _points[1].y, 0.0f );
            GL.Vertex3( _points[2].x, _points[2].y, 0.0f );

            GL.Vertex3( _points[2].x, _points[2].y, 0.0f );
            GL.Vertex3( _points[3].x, _points[3].y, 0.0f );

            GL.Vertex3( _points[3].x, _points[3].y, 0.0f );
            GL.Vertex3( _points[0].x, _points[0].y, 0.0f );
        GL.End();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // assets
    ///////////////////////////////////////////////////////////////////////////////

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
