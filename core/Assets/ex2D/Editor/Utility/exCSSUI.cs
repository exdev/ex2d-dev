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

    public static void DisplayField ( int _indentLevel, exUIElement _el, string _name, ref exCSS_display _val ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _val = (exCSS_display)EditorGUILayout.EnumPopup ( _val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
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

    public static void ColorField ( int _indentLevel, exUIElement _el, string _name, exCSS_color _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            // process enum with inherit variable
            if ( _inherited ) {
                _prop.type = (exCSS_color.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            }
            else {
                string[] names = System.Enum.GetNames(typeof(exCSS_color.Type));
                int idx = (int)_prop.type;
                string[] names2 = new string [names.Length-1];
                for ( int i = 0; i < names2.Length; ++i ) {
                    names2[i] = names[i];
                }

                idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                _prop.type = (exCSS_color.Type)System.Math.Min( idx, names2.Length-1 );
            }

            // process value with type
            switch ( _prop.type ) {
            case exCSS_color.Type.Color:
                _prop.val = EditorGUILayout.ColorField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(70.0f) } );
                break;

            case exCSS_color.Type.Inherit:
                GUI.enabled = false;
                _prop.val = EditorGUILayout.ColorField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(70.0f) } );
                GUI.enabled = true;
                break;
            }
        EditorGUILayout.EndHorizontal ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void DoSizeField ( int _indentLevel, exUIElement _el, string _name, exCSS_size _prop, bool _inherited ) {
        GUILayout.Space( 15.0f * _indentLevel );
        GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

        // process enum with inherit variable
        if ( _inherited ) {
            _prop.type = (exCSS_size.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        }
        else {
            string[] names = System.Enum.GetNames(typeof(exCSS_size.Type));
            // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_size.Type), _prop.type));
            int idx = (int)_prop.type;
            string[] names2 = new string [names.Length-1];
            for ( int i = 0; i < names2.Length; ++i ) {
                names2[i] = names[i];
            }

            idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.type = (exCSS_size.Type)System.Math.Min( idx, names2.Length-1 );
        }

        // process value with type
        switch ( _prop.type ) {
        case exCSS_size.Type.Length:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "px" );
            break;

        case exCSS_size.Type.Percentage:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "%" );
            break;

        case exCSS_size.Type.Auto:
        case exCSS_size.Type.Inherit:
            bool old = GUI.enabled;
            GUI.enabled = false;
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUI.enabled = old;
            break;
        }
    }

    public static void SizeField ( int _indentLevel, exUIElement _el, string _name, exCSS_size _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            DoSizeField ( _indentLevel, _el, _name, _prop, _inherited );
        EditorGUILayout.EndHorizontal ();
    }

    public static void LockableSizeField ( int _indentLevel, exUIElement _el, string _name, exCSS_size _prop, bool _inherited, ref bool _lock ) {
        EditorGUILayout.BeginHorizontal ();
            GUI.enabled = !_lock;
                DoSizeField ( _indentLevel, _el, _name, _prop, _inherited );
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

    public static void DoSizeNoAutoField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_noauto _prop, bool _inherited ) {
        GUILayout.Space( 15.0f * _indentLevel );
        GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

        // process enum with inherit variable
        if ( _inherited ) {
            _prop.type = (exCSS_size_noauto.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        }
        else {
            string[] names = System.Enum.GetNames(typeof(exCSS_size_noauto.Type));
            // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_size_noauto.Type), _prop.type));
            int idx = (int)_prop.type;
            string[] names2 = new string [names.Length-1];
            for ( int i = 0; i < names2.Length; ++i ) {
                names2[i] = names[i];
            }

            idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.type = (exCSS_size_noauto.Type)System.Math.Min( idx, names2.Length-1 );
        }

        // process value with type
        switch ( _prop.type ) {
        case exCSS_size_noauto.Type.Length:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "px" );
            break;

        case exCSS_size_noauto.Type.Percentage:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "%" );
            break;

        case exCSS_size_noauto.Type.Inherit:
            bool old = GUI.enabled;
            GUI.enabled = false;
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUI.enabled = old;
            break;
        }
    }

    public static void SizeNoAutoField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_noauto _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            DoSizeNoAutoField ( _indentLevel, _el, _name, _prop, _inherited );
        EditorGUILayout.EndHorizontal ();
    }

    public static void LockableSizeNoAutoField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_noauto _prop, bool _inherited, ref bool _lock ) {
        EditorGUILayout.BeginHorizontal ();
            GUI.enabled = !_lock;
                DoSizeNoAutoField ( _indentLevel, _el, _name, _prop, _inherited );
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

    public static void DoSizeLengthOnlyField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_lengthonly _prop, bool _inherited ) {
        GUILayout.Space( 15.0f * _indentLevel );
        GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

        // process enum with inherit variable
        if ( _inherited ) {
            _prop.type = (exCSS_size_lengthonly.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        }
        else {
            string[] names = System.Enum.GetNames(typeof(exCSS_size_lengthonly.Type));
            // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_size_lengthonly.Type), _prop.type));
            int idx = (int)_prop.type;
            string[] names2 = new string [names.Length-1];
            for ( int i = 0; i < names2.Length; ++i ) {
                names2[i] = names[i];
            }

            idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.type = (exCSS_size_lengthonly.Type)System.Math.Min( idx, names2.Length-1 );
        }

        // process value with type
        switch ( _prop.type ) {
        case exCSS_size_lengthonly.Type.Length:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "px" );
            break;

        case exCSS_size_lengthonly.Type.Inherit:
            bool old = GUI.enabled;
            GUI.enabled = false;
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUI.enabled = old;
            break;
        }
    }

    public static void SizeLengthOnlyField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_lengthonly _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            DoSizeLengthOnlyField ( _indentLevel, _el, _name, _prop, _inherited );
        EditorGUILayout.EndHorizontal ();
    }

    public static void LockableSizeLengthOnlyField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_lengthonly _prop, bool _inherited, ref bool _lock ) {
        EditorGUILayout.BeginHorizontal ();
            GUI.enabled = !_lock;
                DoSizeLengthOnlyField ( _indentLevel, _el, _name, _prop, _inherited );
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

    public static void MinSizeField ( int _indentLevel, exUIElement _el, string _name, exCSS_min_size _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            // process enum with inherit variable
            if ( _inherited ) {
                _prop.type = (exCSS_min_size.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            }
            else {
                string[] names = System.Enum.GetNames(typeof(exCSS_min_size.Type));
                // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_min_size.Type), _prop.type));
                int idx = (int)_prop.type;
                string[] names2 = new string [names.Length-1];
                for ( int i = 0; i < names2.Length; ++i ) {
                    names2[i] = names[i];
                }

                idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                _prop.type = (exCSS_min_size.Type)System.Math.Min( idx, names2.Length-1 );
            }

            // process value with type
            switch ( _prop.type ) {
            case exCSS_min_size.Type.Length:
                _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                GUILayout.Label ( "px" );
                break;

            case exCSS_min_size.Type.Percentage:
                _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                GUILayout.Label ( "%" );
                break;

            case exCSS_min_size.Type.Inherit:
                GUI.enabled = false;
                _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                GUI.enabled = true;
                break;
            }

        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void MaxSizeField ( int _indentLevel, exUIElement _el, string _name, exCSS_max_size _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            // process enum with inherit variable
            if ( _inherited ) {
                _prop.type = (exCSS_max_size.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            }
            else {
                string[] names = System.Enum.GetNames(typeof(exCSS_max_size.Type));
                // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_max_size.Type), _prop.type));
                int idx = (int)_prop.type;
                string[] names2 = new string [names.Length-1];
                for ( int i = 0; i < names2.Length; ++i ) {
                    names2[i] = names[i];
                }

                idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                _prop.type = (exCSS_max_size.Type)System.Math.Min( idx, names2.Length-1 );
            }

            // process value with type
            switch ( _prop.type ) {
            case exCSS_max_size.Type.Length:
                _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                GUILayout.Label ( "px" );
                break;

            case exCSS_max_size.Type.Percentage:
                _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                GUILayout.Label ( "%" );
                break;

            case exCSS_max_size.Type.None:
            case exCSS_max_size.Type.Inherit:
                GUI.enabled = false;
                _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                GUI.enabled = true;
                break;
            }

        EditorGUILayout.EndHorizontal ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void ImageField ( int _indentLevel, exUIElement _el, string _name, exCSS_image _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            // process enum with inherit variable
            if ( _inherited ) {
                _prop.type = (exCSS_image.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            }
            else {
                string[] names = System.Enum.GetNames(typeof(exCSS_image.Type));
                // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_max_size.Type), _prop.type));
                int idx = (int)_prop.type;
                string[] names2 = new string [names.Length-1];
                for ( int i = 0; i < names2.Length; ++i ) {
                    names2[i] = names[i];
                }

                idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                _prop.type = (exCSS_image.Type)System.Math.Min( idx, names2.Length-1 );
            }

            switch ( _prop.type ) {
            case exCSS_image.Type.TextureInfo:
                _prop.val = (exTextureInfo)EditorGUILayout.ObjectField ( _prop.val, typeof(exTextureInfo), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } );
                break;

            case exCSS_image.Type.Texture2D:
                _prop.val = (Texture2D)EditorGUILayout.ObjectField ( _prop.val, typeof(Texture2D), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } );
                break;

            case exCSS_image.Type.Inherit:
                GUI.enabled = false;
                _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(Object), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } );
                GUI.enabled = true;
                break;
            }
        EditorGUILayout.EndHorizontal ();
    }
}
