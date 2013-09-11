// ======================================================================================
// File         : exSpriteInspector.cs
// Author       : Wu Jie 
// Last Change  : 07/04/2013 | 15:43:11 PM | Thursday,July
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
[CustomEditor(typeof(exSprite))]
class exSpriteInspector : exLayeredSpriteInspector {

    SerializedProperty textureInfoProp;
    SerializedProperty useTextureOffsetProp;
    SerializedProperty spriteTypeProp;
    SerializedProperty tiledSpacingProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        InitProperties();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        base.OnInspectorGUI();

        // NOTE: DO NOT call serializedObject.ApplyModifiedProperties ();
        serializedObject.Update ();

        EditorGUILayout.Space();

        // textureInfo
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(3);
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( textureInfoProp, new GUIContent("Texture Info") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.textureInfo = textureInfoProp.objectReferenceValue as exTextureInfo;
                    if ( sp.textureInfo != null ) {
                        if ( sp.textureInfo.hasBorder ) {
                            sp.spriteType = exSpriteType.Sliced;
                            sp.customSize = true;
                        }
                        else if (sp.spriteType == exSpriteType.Sliced) {
                            sp.spriteType = exSpriteType.Simple;
                            sp.customSize = false;
                        }
                    }
                    EditorUtility.SetDirty(sp);
                }
            }
        }
        EditorGUILayout.EndVertical();
        if ( GUILayout.Button("Refresh", GUILayout.Width(57), GUILayout.Height(16) ) ) {
            foreach (Object obj in serializedObject.targetObjects) {
                exSprite sp = obj as exSprite;
                if (sp) {
                    sp.textureInfo = sp.textureInfo;
                    EditorUtility.SetDirty(sp);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        // draw preview rect
        float preview_width = 100.0f;
        float preview_height = 100.0f;
        Rect lastRect = GUILayoutUtility.GetLastRect();
        if ( Event.current.type == EventType.Repaint && serializedObject.isEditingMultipleObjects == false ) {
            exTextureInfo textureInfo = textureInfoProp.objectReferenceValue as exTextureInfo;
            float indent_space = 20.0f;
            Rect previewRect = new Rect ( indent_space,
                                          lastRect.yMax,
                                          preview_width, 
                                          preview_height );

            // draw Checker
            Texture2D checker = exEditorUtility.textureCheckerboard;
            GUI.DrawTextureWithTexCoords ( previewRect, 
                                           checker, 
                                           new Rect( 0.0f, 0.0f, 3.0f, 3.0f ) );

            // draw TextureInfo
            if ( textureInfo != null ) {
                float scale = exEditorUtility.CalculateTextureInfoScale(previewRect,textureInfo);
                Rect pos = new Rect ( previewRect.center.x - textureInfo.width * 0.5f * scale + 2.0f,
                                      previewRect.center.y - textureInfo.height * 0.5f * scale + 2.0f,
                                      textureInfo.width * scale - 4.0f,
                                      textureInfo.height * scale - 4.0f );
                exEditorUtility.GUI_DrawTextureInfo ( pos,
                                                      textureInfo,
                                                      Color.white );
            }

            // draw border
            exEditorUtility.GL_DrawRectLine ( new Vector3 [] {
                                              new Vector3 ( indent_space, lastRect.yMax, 0.0f ),
                                              new Vector3 ( indent_space + preview_width, lastRect.yMax, 0.0f ),
                                              new Vector3 ( indent_space + preview_width, lastRect.yMax + preview_height, 0.0f ),
                                              new Vector3 ( indent_space, lastRect.yMax + preview_height, 0.0f ),
                                              },
                                              new Color( 0.8f, 0.8f, 0.8f, 1.0f ) );

            // edit button
            Rect editBtnPos = new Rect(previewRect.xMax + 2, previewRect.center.y - 10, 50, 20);
            if ( GUI.Button( editBtnPos, "Edit...") ) {
                EditorWindow.GetWindow<exTextureInfoEditor>().Edit( textureInfoProp.objectReferenceValue as exTextureInfo );
            }
        }
        GUILayoutUtility.GetRect( preview_width, preview_height );
        EditorGUILayout.Space();

        // useTextureOffset
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( useTextureOffsetProp, new GUIContent("Use Texture Offset") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.useTextureOffset = useTextureOffsetProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }
        
        // type
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( spriteTypeProp, new GUIContent("Sprite Type") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSprite sp = obj as exSprite;
                if ( sp ) {
                    sp.spriteType = (exSpriteType)spriteTypeProp.intValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        if (spriteTypeProp.enumValueIndex == (int)exSpriteType.Tiled) {
            EditorGUI.indentLevel++;
            // tiled spacing
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( tiledSpacingProp, new GUIContent("Tiled Spacing"), true );
            if ( EditorGUI.EndChangeCheck() ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSprite sp = obj as exSprite;
                    if ( sp ) {
                        sp.tiledSpacing = tiledSpacingProp.vector2Value;
                        EditorUtility.SetDirty(sp);
                    }
                }
            }
            if ( GUILayout.Button("Use Raw Size", GUILayout.Width(88), GUILayout.Height(16) ) ) {
                foreach (Object obj in serializedObject.targetObjects) {
                    exSprite sp = obj as exSprite;
                    if (sp && sp.textureInfo != null) {
                        exTextureInfo ti = sp.textureInfo;
                        sp.tiledSpacing = new Vector2(ti.rawWidth - ti.width, ti.rawHeight - ti.height) / 2;
                        EditorUtility.SetDirty(sp);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Edit...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                EditorWindow.GetWindow<exSceneEditor>();
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void OnSceneGUI () {
        base.OnSceneGUI();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void InitProperties () {
        base.InitProperties();
        textureInfoProp = serializedObject.FindProperty("textureInfo_");
        useTextureOffsetProp = serializedObject.FindProperty("useTextureOffset_");
        spriteTypeProp = serializedObject.FindProperty("spriteType_");
        tiledSpacingProp = serializedObject.FindProperty("tiledSpacing_");
    }
}

