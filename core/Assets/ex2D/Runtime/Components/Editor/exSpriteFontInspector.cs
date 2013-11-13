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
    protected SerializedProperty spacingProp;
    protected SerializedProperty topColorProp;
    protected SerializedProperty botColorProp;
    protected SerializedProperty useOutlineProp;
    protected SerializedProperty outlineWidthProp;
    protected SerializedProperty outlineColorProp;
    protected SerializedProperty useShadowProp;
    protected SerializedProperty shadowBiasProp;
    protected SerializedProperty shadowColorProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        textProp = serializedObject.FindProperty("text_");
        textAlignProp = serializedObject.FindProperty("textAlign_");
        useKerningProp = serializedObject.FindProperty("useKerning_");
        spacingProp = serializedObject.FindProperty("spacing_");
        topColorProp = serializedObject.FindProperty("topColor_");
        botColorProp = serializedObject.FindProperty("botColor_");
        useOutlineProp = serializedObject.FindProperty("useOutline_");
        outlineWidthProp = serializedObject.FindProperty("outlineWidth_");
        outlineColorProp = serializedObject.FindProperty("outlineColor_");
        useShadowProp = serializedObject.FindProperty("useShadow_");
        shadowBiasProp = serializedObject.FindProperty("shadowBias_");
        shadowColorProp = serializedObject.FindProperty("shadowColor_");
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

        // spacing
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( spacingProp, new GUIContent("Spacing"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.spacing = spacingProp.vector2Value;
                    EditorUtility.SetDirty(sp);
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

        // useOutline
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( useOutlineProp, new GUIContent("Use Outline") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.useOutline = useOutlineProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        GUI.enabled = useOutlineProp.boolValue;
        EditorGUI.indentLevel++;
        // outlineWidth
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( outlineWidthProp, new GUIContent("Outline Width") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.outlineWidth = outlineWidthProp.floatValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // outlineColor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( outlineColorProp, new GUIContent("Outline Color") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.outlineColor = outlineColorProp.colorValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }
        EditorGUI.indentLevel--;
        GUI.enabled = true;

        // useShadow
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( useShadowProp, new GUIContent("Use Shadow") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.useShadow = useShadowProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        GUI.enabled = useShadowProp.boolValue;
        EditorGUI.indentLevel++;

        // shadowBias
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( shadowBiasProp, new GUIContent("Shadow Bias"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.shadowBias = shadowBiasProp.vector2Value;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // shadowColor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( shadowColorProp, new GUIContent("Shadow Color") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteFont sp = obj as exSpriteFont;
                if ( sp ) {
                    sp.shadowColor = shadowColorProp.colorValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        EditorGUI.indentLevel--;
        GUI.enabled = true;
        
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

