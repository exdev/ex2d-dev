// ======================================================================================
// File         : ex3DSpriteFontInspector.cs
// Author       : Wu Jie 
// Last Change  : 08/31/2013
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
[CustomEditor(typeof(ex3DSpriteFont))]
class ex3DSpriteFontInspector : exStandaloneSpriteInspector {

    //SerializedProperty fontProp;
    protected SerializedProperty textProp;
    protected SerializedProperty textAlignProp;
    protected SerializedProperty useKerningProp;
    protected SerializedProperty wrapModeProp;
    protected SerializedProperty lineHeightProp;
    protected SerializedProperty letterSpacingProp;
    protected SerializedProperty wordSpacingProp;
    protected SerializedProperty topColorProp;
    protected SerializedProperty botColorProp;
    //protected SerializedProperty useOutlineProp;
    //protected SerializedProperty outlineWidthProp;
    //protected SerializedProperty outlineColorProp;
    //protected SerializedProperty useShadowProp;
    //protected SerializedProperty shadowBiasProp;
    //protected SerializedProperty shadowColorProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        //fontProp = serializedObject.FindProperty("font_");
        textProp = serializedObject.FindProperty("text_");
        textAlignProp = serializedObject.FindProperty("textAlign_");
        useKerningProp = serializedObject.FindProperty("useKerning_");
        wrapModeProp = serializedObject.FindProperty("wrapMode_");
        lineHeightProp = serializedObject.FindProperty("lineHeight_");
        letterSpacingProp = serializedObject.FindProperty("letterSpacing_");
        wordSpacingProp = serializedObject.FindProperty("wordSpacing_");
        topColorProp = serializedObject.FindProperty("topColor_");
        botColorProp = serializedObject.FindProperty("botColor_");
        //useOutlineProp = serializedObject.FindProperty("useOutline_");
        //outlineWidthProp = serializedObject.FindProperty("outlineWidth_");
        //outlineColorProp = serializedObject.FindProperty("outlineColor_");
        //useShadowProp = serializedObject.FindProperty("useShadow_");
        //shadowBiasProp = serializedObject.FindProperty("shadowBias_");
        //shadowColorProp = serializedObject.FindProperty("shadowColor_");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        exSpriteFontInspectorHelper.DoInspectorGUI(this, 
                                                    textProp,
                                                    textAlignProp,
                                                    useKerningProp,
                                                    wrapModeProp,
                                                    lineHeightProp,
                                                    letterSpacingProp,
                                                    wordSpacingProp,
                                                    topColorProp,
                                                    botColorProp);

        //// useOutline
        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.PropertyField ( useOutlineProp, new GUIContent("Use Outline") );
        //if ( EditorGUI.EndChangeCheck() ) {
        //    foreach ( Object obj in serializedObject.targetObjects ) {
        //        ex3DSpriteFont sp = obj as ex3DSpriteFont;
        //        if ( sp ) {
        //            sp.useOutline = useOutlineProp.boolValue;
        //            EditorUtility.SetDirty(sp);
        //        }
        //    }
        //}

        //GUI.enabled = useOutlineProp.boolValue;
        //EditorGUI.indentLevel++;
        //// outlineWidth
        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.PropertyField ( outlineWidthProp, new GUIContent("Outline Width") );
        //if ( EditorGUI.EndChangeCheck() ) {
        //    foreach ( Object obj in serializedObject.targetObjects ) {
        //        ex3DSpriteFont sp = obj as ex3DSpriteFont;
        //        if ( sp ) {
        //            sp.outlineWidth = outlineWidthProp.floatValue;
        //            EditorUtility.SetDirty(sp);
        //        }
        //    }
        //}

        //// outlineColor
        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.PropertyField ( outlineColorProp, new GUIContent("Outline Color") );
        //if ( EditorGUI.EndChangeCheck() ) {
        //    foreach ( Object obj in serializedObject.targetObjects ) {
        //        ex3DSpriteFont sp = obj as ex3DSpriteFont;
        //        if ( sp ) {
        //            sp.outlineColor = outlineColorProp.colorValue;
        //            EditorUtility.SetDirty(sp);
        //        }
        //    }
        //}
        //EditorGUI.indentLevel--;
        //GUI.enabled = true;

        //// useShadow
        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.PropertyField ( useShadowProp, new GUIContent("Use Shadow") );
        //if ( EditorGUI.EndChangeCheck() ) {
        //    foreach ( Object obj in serializedObject.targetObjects ) {
        //        ex3DSpriteFont sp = obj as ex3DSpriteFont;
        //        if ( sp ) {
        //            sp.useShadow = useShadowProp.boolValue;
        //            EditorUtility.SetDirty(sp);
        //        }
        //    }
        //}

        //GUI.enabled = useShadowProp.boolValue;
        //EditorGUI.indentLevel++;

        //// shadowBias
        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.PropertyField ( shadowBiasProp, new GUIContent("Shadow Bias"), true );
        //if ( EditorGUI.EndChangeCheck() ) {
        //    foreach ( Object obj in serializedObject.targetObjects ) {
        //        ex3DSpriteFont sp = obj as ex3DSpriteFont;
        //        if ( sp ) {
        //            sp.shadowBias = shadowBiasProp.vector2Value;
        //            EditorUtility.SetDirty(sp);
        //        }
        //    }
        //}

        //// shadowColor
        //EditorGUI.BeginChangeCheck();
        //EditorGUILayout.PropertyField ( shadowColorProp, new GUIContent("Shadow Color") );
        //if ( EditorGUI.EndChangeCheck() ) {
        //    foreach ( Object obj in serializedObject.targetObjects ) {
        //        ex3DSpriteFont sp = obj as ex3DSpriteFont;
        //        if ( sp ) {
        //            sp.shadowColor = shadowColorProp.colorValue;
        //            EditorUtility.SetDirty(sp);
        //        }
        //    }
        //}

        //EditorGUI.indentLevel--;
        //GUI.enabled = true;
    }
}

