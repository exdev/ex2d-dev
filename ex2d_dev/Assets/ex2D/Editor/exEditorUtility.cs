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

    static Texture2D textureWhite;
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
    /// \return the white texture
    /// return a small white texture
    // ------------------------------------------------------------------ 

    public static Texture2D WhiteTexture () {
        if ( textureWhite == null )
            textureWhite = FindTexture ( "pixel.png" ); 
        return textureWhite;
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
    // Desc: 
    // ------------------------------------------------------------------ 

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
            GUI.DrawTexture( _rect, WhiteTexture() );
        GUI.color = old;

        // border
        old = GUI.backgroundColor;
        GUI.backgroundColor = _borderColor;
            RectBorderStyle().Draw( _rect, false, true, true, true );
        GUI.backgroundColor = old;
    }
}
