// ======================================================================================
// File         : exSpriteFontInspector.cs
// Author       : Wu Jie 
// Last Change  : 08/01/2013 | 15:01:06 PM | Thursday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
// BoardPatternInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(exSpriteFont))]
class exSpriteFontInspector : exLayeredSpriteInspector {

    protected SerializedProperty textProp;
    protected SerializedProperty textAlignProp;
    protected SerializedProperty useKerningProp;
    protected SerializedProperty wrapWordProp;
    protected SerializedProperty lineHeightProp;
    protected SerializedProperty customLineHeightProp;
    protected SerializedProperty letterSpacingProp;
    protected SerializedProperty wordSpacingProp;
    protected SerializedProperty topColorProp;
    protected SerializedProperty botColorProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        textProp = serializedObject.FindProperty("text_");
        textAlignProp = serializedObject.FindProperty("textAlign_");
        useKerningProp = serializedObject.FindProperty("useKerning_");
        wrapWordProp = serializedObject.FindProperty("wrapWord_");
        lineHeightProp = serializedObject.FindProperty("lineHeight_");
        customLineHeightProp = serializedObject.FindProperty("customLineHeight_");
        letterSpacingProp = serializedObject.FindProperty("letterSpacing_");
        wordSpacingProp = serializedObject.FindProperty("wordSpacing_");
        topColorProp = serializedObject.FindProperty("topColor_");
        botColorProp = serializedObject.FindProperty("botColor_");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        exISpriteFontInspector.DoInspectorGUI(this, 
                                                    textProp,
                                                    textAlignProp,
                                                    useKerningProp,
                                                    wrapWordProp,
                                                    lineHeightProp,
                                                    customLineHeightProp,
                                                    letterSpacingProp,
                                                    wordSpacingProp,
                                                    topColorProp,
                                                    botColorProp);
        
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Edit...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                EditorWindow.GetWindow<exSceneEditor>();
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
}

static class exISpriteFontInspector {
    public static void DoInspectorGUI (exSpriteBaseInspector _inspector, 
                                        SerializedProperty _textProp,
                                        SerializedProperty _textAlignProp,
                                        SerializedProperty _useKerningProp,
                                        SerializedProperty _wrapWordProp,
                                        SerializedProperty _lineHeightProp,
                                        SerializedProperty _customLineHeightProp,
                                        SerializedProperty _letterSpacingProp,
                                        SerializedProperty _wordSpacingProp,
                                        SerializedProperty _topColorProp,
                                        SerializedProperty _botColorProp)
    {
        _inspector.customSizeProp.boolValue = true;
        {
            // font
            exISpriteFont sp = _inspector.serializedObject.targetObject as exISpriteFont;
            if (sp != null) {
                EditorGUI.BeginChangeCheck();
                exFont.TypeForEditor fontType = (exFont.TypeForEditor)EditorGUILayout.EnumPopup("Font Type", sp.fontType);
                int oldFontSize = sp.fontSize;
                if (EditorGUI.EndChangeCheck()) {
                    sp.fontType = fontType;
                    if (fontType == exFont.TypeForEditor.Dynamic) {
                        if (sp.dynamicFont == null) {
                            sp.SetFont(Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font);
                        }
                        sp.fontSize = oldFontSize;
                    }
                    EditorUtility.SetDirty(sp as Object);
                    sp.lineHeight = sp.fontSize;        // 自动重设行高
                }
                EditorGUI.indentLevel++;
                if (fontType == exFont.TypeForEditor.Bitmap) {
                    EditorGUI.BeginChangeCheck();
                    exBitmapFont font = EditorGUILayout.ObjectField ("Font", sp.bitmapFont, typeof(exBitmapFont), false) as exBitmapFont;
                    if (EditorGUI.EndChangeCheck()) {
                        sp.SetFont(font);
                        EditorUtility.SetDirty(sp as Object);
                        sp.lineHeight = sp.fontSize;    // 自动重设行高
                    }
                }
                else {
                    EditorGUI.BeginChangeCheck();
                    Font font = EditorGUILayout.ObjectField ("Font", sp.dynamicFont, typeof(Font), false) as Font;
                    if (EditorGUI.EndChangeCheck()) {
                        sp.SetFont(font);
                        EditorUtility.SetDirty(sp as Object);
                        sp.lineHeight = sp.fontSize;    // 自动重设行高
                    }
                    EditorGUI.BeginChangeCheck();
                    var fontStyle = (FontStyle)EditorGUILayout.EnumPopup("Font Style", sp.fontStyle);
                    var fontSize = EditorGUILayout.IntField("Font Size", sp.fontSize);
                    //sp.lineHeight = EditorGUILayout.IntField("Line Height", sp.lineHeight);
                    if (EditorGUI.EndChangeCheck()) {
                        sp.fontStyle = fontStyle;
                        sp.fontSize = fontSize;
                        EditorUtility.SetDirty(sp as Object);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        // text
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _textProp, new GUIContent("Text") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.text = _textProp.stringValue;
                    EditorUtility.SetDirty(sp as Object);
                }
            }
        }

        // textAlign
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _textAlignProp, new GUIContent("Text Align") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.textAlign = (TextAlignment)_textAlignProp.enumValueIndex;
                    EditorUtility.SetDirty(sp as Object);
                }
            }
        }

        // useKerning
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _useKerningProp, new GUIContent("Use Kerning") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.useKerning = _useKerningProp.boolValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // wrap word
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _wrapWordProp, new GUIContent("Wrap Word"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.wrapWord = _wrapWordProp.boolValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // custom line height
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _customLineHeightProp, new GUIContent("Custom Line Height"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.customLineHeight = _customLineHeightProp.boolValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        if (_customLineHeightProp.boolValue) {
            EditorGUI.indentLevel++;

            // line height
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( _lineHeightProp, new GUIContent("Line Height"), true );
            if ( EditorGUI.EndChangeCheck() ) {
                foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                    exISpriteFont sp = obj as exISpriteFont;
                    if ( sp != null ) {
                        sp.lineHeight = _lineHeightProp.intValue;
                        EditorUtility.SetDirty(obj);
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        // letter spacing
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _letterSpacingProp, new GUIContent("Letter Spacing"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.letterSpacing = _letterSpacingProp.intValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // word spacing
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _wordSpacingProp, new GUIContent("Word Spacing"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.wordSpacing = _wordSpacingProp.intValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // topColor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _topColorProp, new GUIContent("Top Color"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.topColor = _topColorProp.colorValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // botColor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( _botColorProp, new GUIContent("Bot Color"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in _inspector.serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.botColor = _botColorProp.colorValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}
