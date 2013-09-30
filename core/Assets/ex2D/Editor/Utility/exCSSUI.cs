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

    public static void WrapField ( int _indentLevel, exUIElement _el, string _name, ref exCSS_wrap _val ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _val = (exCSS_wrap)EditorGUILayout.EnumPopup ( _val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void HorizontalAlignField ( int _indentLevel, exUIElement _el, string _name, ref exCSS_horizontal_align _val ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _val = (exCSS_horizontal_align)EditorGUILayout.EnumPopup ( _val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void VerticalAlignField ( int _indentLevel, exUIElement _el, string _name, ref exCSS_vertical_align _val ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _val = (exCSS_vertical_align)EditorGUILayout.EnumPopup ( _val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        EditorGUILayout.EndHorizontal ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void DecorationField ( int _indentLevel, exUIElement _el, string _name, ref exCSS_decoration _val ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            _val = (exCSS_decoration)EditorGUILayout.EnumPopup ( _val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
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

    public static bool LockableSizeField ( int _indentLevel, exUIElement _el, string _name, exCSS_size _prop, bool _inherited, ref bool _lock ) {
        bool changed = false;
        EditorGUILayout.BeginHorizontal ();
            GUI.enabled = !_lock;
                DoSizeField ( _indentLevel, _el, _name, _prop, _inherited );
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();
            _lock = GUILayout.Toggle ( _lock, GUIContent.none, "IN LockButton", new GUILayoutOption [] {
                                       GUILayout.Width(20),
                                       GUILayout.Height(20),
                                       } );
            if ( EditorGUI.EndChangeCheck() ) {
                changed = true;
                GUI.changed = true;
            }
        EditorGUILayout.EndHorizontal ();
        return changed;
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void DoSizePushField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_push _prop, bool _inherited ) {
        GUILayout.Space( 15.0f * _indentLevel );
        GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

        // process enum with inherit variable
        if ( _inherited ) {
            _prop.type = (exCSS_size_push.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        }
        else {
            string[] names = System.Enum.GetNames(typeof(exCSS_size_push.Type));
            // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_size_push.Type), _prop.type));
            int idx = (int)_prop.type;
            string[] names2 = new string [names.Length-1];
            for ( int i = 0; i < names2.Length; ++i ) {
                names2[i] = names[i];
            }

            idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.type = (exCSS_size_push.Type)System.Math.Min( idx, names2.Length-1 );
        }

        // process value with type
        switch ( _prop.type ) {
        case exCSS_size_push.Type.Length:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "px" );
            break;

        case exCSS_size_push.Type.Percentage:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "%" );
            break;

        case exCSS_size_push.Type.Auto:
        case exCSS_size_push.Type.Push:
        case exCSS_size_push.Type.Inherit:
            bool old = GUI.enabled;
            GUI.enabled = false;
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUI.enabled = old;
            break;
        }
    }

    public static void SizePushField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_push _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            DoSizePushField ( _indentLevel, _el, _name, _prop, _inherited );
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

    public static bool LockableSizeNoAutoField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_noauto _prop, bool _inherited, ref bool _lock ) {
        bool changed = false;
        EditorGUILayout.BeginHorizontal ();
            GUI.enabled = !_lock;
                DoSizeNoAutoField ( _indentLevel, _el, _name, _prop, _inherited );
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();
            _lock = GUILayout.Toggle ( _lock, GUIContent.none, "IN LockButton", new GUILayoutOption [] {
                                       GUILayout.Width(20),
                                       GUILayout.Height(20),
                                       } );
            if ( EditorGUI.EndChangeCheck() ) {
                changed = true;
                GUI.changed = true;
            }
        EditorGUILayout.EndHorizontal ();
        return changed;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void DoSizeNoPercentageField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_nopercentage _prop, bool _inherited ) {
        GUILayout.Space( 15.0f * _indentLevel );
        GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

        // process enum with inherit variable
        if ( _inherited ) {
            _prop.type = (exCSS_size_nopercentage.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
        }
        else {
            string[] names = System.Enum.GetNames(typeof(exCSS_size_nopercentage.Type));
            // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_size_nopercentage.Type), _prop.type));
            int idx = (int)_prop.type;
            string[] names2 = new string [names.Length-1];
            for ( int i = 0; i < names2.Length; ++i ) {
                names2[i] = names[i];
            }

            idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            _prop.type = (exCSS_size_nopercentage.Type)System.Math.Min( idx, names2.Length-1 );
        }

        // process value with type
        switch ( _prop.type ) {
        case exCSS_size_nopercentage.Type.Length:
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUILayout.Label ( "px" );
            break;

        case exCSS_size_nopercentage.Type.Auto:
        case exCSS_size_nopercentage.Type.Inherit:
            bool old = GUI.enabled;
            GUI.enabled = false;
            _prop.val = EditorGUILayout.FloatField ( _prop.val, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            GUI.enabled = old;
            break;
        }
    }

    public static void SizeNoPercentageField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_nopercentage _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            DoSizeNoPercentageField ( _indentLevel, _el, _name, _prop, _inherited );
        EditorGUILayout.EndHorizontal ();
    }

    public static bool LockableSizeNoPercentageField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_nopercentage _prop, bool _inherited, ref bool _lock ) {
        bool changed = false;
        EditorGUILayout.BeginHorizontal ();
            GUI.enabled = !_lock;
                DoSizeNoPercentageField ( _indentLevel, _el, _name, _prop, _inherited );
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();
            _lock = GUILayout.Toggle ( _lock, GUIContent.none, "IN LockButton", new GUILayoutOption [] {
                                       GUILayout.Width(20),
                                       GUILayout.Height(20),
                                       } );
            if ( EditorGUI.EndChangeCheck() ) {
                changed = true;
                GUI.changed = true;
            }
        EditorGUILayout.EndHorizontal ();
        return changed;
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

    public static bool LockableSizeLengthOnlyField ( int _indentLevel, exUIElement _el, string _name, exCSS_size_lengthonly _prop, bool _inherited, ref bool _lock ) {
        bool changed = false;
        EditorGUILayout.BeginHorizontal ();
            GUI.enabled = !_lock;
                DoSizeLengthOnlyField ( _indentLevel, _el, _name, _prop, _inherited );
            GUI.enabled = true;
            EditorGUI.BeginChangeCheck();
            _lock = GUILayout.Toggle ( _lock, GUIContent.none, "IN LockButton", new GUILayoutOption [] {
                                       GUILayout.Width(20),
                                       GUILayout.Height(20),
                                       } );
            if ( EditorGUI.EndChangeCheck() ) {
                changed = true;
                GUI.changed = true;
            }
        EditorGUILayout.EndHorizontal ();
        return changed;
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
                _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(exTextureInfo), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } ) as exTextureInfo;
                break;

            case exCSS_image.Type.Texture2D:
                _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(Texture2D), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } ) as Texture2D;
                break;

            case exCSS_image.Type.Inherit:
                GUI.enabled = false;
                _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(Object), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } );
                GUI.enabled = true;
                break;
            }
        EditorGUILayout.EndHorizontal ();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void FontField ( int _indentLevel, exUIElement _el, string _name, exCSS_font _prop, bool _inherited ) {
        EditorGUILayout.BeginHorizontal ();
            GUILayout.Space( 15.0f * _indentLevel );
            GUILayout.Label ( _name, new GUILayoutOption[] { GUILayout.Width(80.0f) } );

            // process enum with inherit variable
            if ( _inherited ) {
                _prop.type = (exCSS_font.Type)EditorGUILayout.EnumPopup ( _prop.type, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
            }
            else {
                string[] names = System.Enum.GetNames(typeof(exCSS_font.Type));
                // int idx = System.Array.IndexOf<string>(names, System.Enum.GetName( typeof(exCSS_max_size.Type), _prop.type));
                int idx = (int)_prop.type;
                string[] names2 = new string [names.Length-1];
                for ( int i = 0; i < names2.Length; ++i ) {
                    names2[i] = names[i];
                }

                idx = EditorGUILayout.Popup ( "", idx, names2, new GUILayoutOption[] { GUILayout.Width(50.0f) } );
                _prop.type = (exCSS_font.Type)System.Math.Min( idx, names2.Length-1 );
            }

            switch ( _prop.type ) {
            case exCSS_font.Type.TTF:
                _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(Font), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } ) as Font;
                break;

            case exCSS_font.Type.BitmapFont:
                _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(exBitmapFont), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } ) as exBitmapFont;
                break;

            case exCSS_font.Type.Inherit:
                GUI.enabled = false;
                _prop.val = EditorGUILayout.ObjectField ( _prop.val, typeof(Object), false, new GUILayoutOption[] { GUILayout.Width(80.0f) } );
                GUI.enabled = true;
                break;
            }
        EditorGUILayout.EndHorizontal ();
    }
}
