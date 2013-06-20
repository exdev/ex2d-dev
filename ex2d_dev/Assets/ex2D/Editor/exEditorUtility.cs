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

    static Texture2D textureCheckerboard;
    static Texture2D textureHelp;

    static GUIStyle styleRectBorder = null;

    ///////////////////////////////////////////////////////////////////////////////
    // special texture
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \return the helper texture
    /// return a help texture
    // ------------------------------------------------------------------ 

    public static Texture2D HelpTexture () {
        // NOTE: hack from "unity editor resources" in Unity3D Contents
        if ( textureHelp == null ) {
            textureHelp = EditorGUIUtility.FindTexture("_help");
            if ( textureHelp == null ) {
                Debug.LogError ( "can't find help texture" );
                return null;
            }
        }
        return textureHelp;
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
        else 
            Handles.DrawLine( a, b );

        Handles.color = savedColor;
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
