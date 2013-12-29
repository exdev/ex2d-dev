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
    protected SerializedProperty wrapModeProp;
    protected SerializedProperty lineHeightProp;
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
        wrapModeProp = serializedObject.FindProperty("wrapMode_");
        lineHeightProp = serializedObject.FindProperty("lineHeight_");
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

        {
            // font
            exSpriteFont sp = serializedObject.targetObject as exSpriteFont;
            if (sp) {
                EditorGUI.BeginChangeCheck();
                exFont.TypeForEditor fontType = (exFont.TypeForEditor)EditorGUILayout.EnumPopup("Font Type", sp.fontType);
                if (EditorGUI.EndChangeCheck()) {
                    sp.fontType = fontType;
                    if (fontType == exFont.TypeForEditor.Dynamic && sp.dynamicFont == null) {
                        sp.SetFont(Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font);
                    }
                    EditorUtility.SetDirty(sp);
                }
                EditorGUI.indentLevel++;
                if (fontType == exFont.TypeForEditor.Bitmap) {
                    EditorGUI.BeginChangeCheck();
                    exBitmapFont font = EditorGUILayout.ObjectField ("Font", sp.bitmapFont, typeof(exBitmapFont), false) as exBitmapFont;
                    if (EditorGUI.EndChangeCheck()) {
                        sp.SetFont(font);
                        EditorUtility.SetDirty(sp);
                    }
                }
                else {
                    EditorGUI.BeginChangeCheck();
                    Font font = EditorGUILayout.ObjectField ("Font", sp.dynamicFont, typeof(Font), false) as Font;
                    sp.fontStyle = (FontStyle)EditorGUILayout.EnumPopup("Font Style", sp.fontStyle);
                    sp.fontSize = EditorGUILayout.IntField("Font Size", sp.fontSize);
                    //sp.lineHeight = EditorGUILayout.IntField("Line Height", sp.lineHeight);
                    if (EditorGUI.EndChangeCheck()) {
                        sp.SetFont(font);
                        EditorUtility.SetDirty(sp);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        // text
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( textProp, new GUIContent("Text") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.text = textProp.stringValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // textAlign
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( textAlignProp, new GUIContent("Text Align") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.textAlign = (TextAlignment)textAlignProp.enumValueIndex;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // useKerning
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( useKerningProp, new GUIContent("Use Kerning") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.useKerning = useKerningProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // wrap mode
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( wrapModeProp, new GUIContent("Wrap Mode"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.wrapMode = (exTextUtility.WrapMode)wrapModeProp.enumValueIndex;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // line height
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( lineHeightProp, new GUIContent("Line Height"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.lineHeight = lineHeightProp.intValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // letter spacing
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( letterSpacingProp, new GUIContent("Letter Spacing"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.letterSpacing = letterSpacingProp.intValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // word spacing
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( wordSpacingProp, new GUIContent("Word Spacing"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exISpriteFont sp = obj as exISpriteFont;
                if ( sp != null ) {
                    sp.wordSpacing = wordSpacingProp.intValue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }

        // topColor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( topColorProp, new GUIContent("Top Color"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.topColor = topColorProp.colorValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // botColor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( botColorProp, new GUIContent("Bot Color"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.botColor = botColorProp.colorValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }
        
        //
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

