// ======================================================================================
// File         : exCSSUI.cs
// Author       : Wu Jie 
// Last Change  : 09/02/2013 | 17:04:03 PM | Monday,September
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
/// css helper function
///
///////////////////////////////////////////////////////////////////////////////

public static class exCSSUI {
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ColorField ( int _indentLevel, exUIElement _el, string _name, exCSS_color _prop ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _prop.type = (exCSS_type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.val = EditorGUILayout.ColorField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(70.0f) } );
        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void PositionField ( int _indentLevel, exUIElement _el, string _name, ref exCSS_position _val ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _val = (exCSS_position)EditorGUILayout.EnumPopup ( _val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        EditorGUILayout.EndHorizontal ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void IntField ( int _indentLevel, exUIElement _el, string _name, exCSS_int _prop ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _prop.type = (exCSS_type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.val = EditorGUILayout.IntField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        EditorGUILayout.EndHorizontal ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void LockableIntField ( int _indentLevel, exUIElement _el, string _name, exCSS_int _prop, ref bool _lock ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            GUI.enabled = !_lock;
            _prop.type = (exCSS_type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.val = EditorGUILayout.IntField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUI.enabled = true;

            _lock = GUILayout.Toggle ( _lock, GUIContent.none, "IN LockButton", new GUILayoutOption [] {
                                       GUILayout.Width(20),
                                       GUILayout.Height(20),
                                       } );
        EditorGUILayout.EndHorizontal ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ImageField ( int _indentLevel, exUIElement _el, string _name, exCSS_image _prop ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _prop.type = (exCSS_type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(Object), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } );
        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // margin, padding
    // ------------------------------------------------------------------ 

    public static int MarginGroupField ( int _indentLevel, exUIElement _el, string _name, exCSS_int[] _props ) {

        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _props[0].type = (exCSS_type)EditorGUILayout.EnumPopup ( _props[0].type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );

            string text = _props[0].val 
                + ", " + _props[1].val 
                + ", " + _props[2].val
                + ", " + _props[3].val;
            text = EditorGUILayout.TextField ( text, new GUILayoutOption[] { GUILayout.Width(150.0f) } );


            int groupCount = 1;
        EditorGUILayout.EndHorizontal ();

        return groupCount; 
    }
}
