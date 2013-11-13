// ======================================================================================
// File         : exSpriteBaseInspector.cs
// Author       : Wu Jie 
// Last Change  : 07/04/2013 | 15:34:38 PM | Thursday,July
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
using ex2D.Detail;

///////////////////////////////////////////////////////////////////////////////
// BoardPatternInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(exSpriteBase))]
class exSpriteBaseInspector : exPlaneInspector {

    protected SerializedProperty customSizeProp;
    protected SerializedProperty shearProp;
    protected SerializedProperty colorProp;
    protected SerializedProperty shaderProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        customSizeProp = serializedObject.FindProperty("customSize_");
        shearProp = serializedObject.FindProperty("shear_");
        colorProp = serializedObject.FindProperty("color_");
        shaderProp = serializedObject.FindProperty("shader_");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        // customSize
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( customSizeProp, new GUIContent("Custom Size") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.customSize = customSizeProp.boolValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // if customSize == true
        EditorGUI.indentLevel++;
        if ( customSizeProp.boolValue ) {
            // width
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( widthProp, new GUIContent("Width") );
            if ( EditorGUI.EndChangeCheck() ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSpriteBase sp = obj as exSpriteBase;
                    if ( sp ) {
                        sp.width = Mathf.Max(widthProp.floatValue, 0f);
                        EditorUtility.SetDirty(sp);
                    }
                }
            }

            // height
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField ( heightProp, new GUIContent("Height") );
            if ( EditorGUI.EndChangeCheck() ) {
                foreach ( Object obj in serializedObject.targetObjects ) {
                    exSpriteBase sp = obj as exSpriteBase;
                    if ( sp ) {
                        sp.height = Mathf.Max(heightProp.floatValue, 0f);
                        EditorUtility.SetDirty(sp);
                    }
                }
            }
        }
        // if customSize == false
        else {
            GUI.enabled = false;
            if ( serializedObject.isEditingMultipleObjects == false ) {
                exSpriteBase spriteBase = serializedObject.targetObject as exSpriteBase;
                EditorGUILayout.FloatField ( new GUIContent("Width"), spriteBase.width );
                EditorGUILayout.FloatField ( new GUIContent("Height"), spriteBase.height );
            }
            GUI.enabled = true;
        }
        EditorGUI.indentLevel--;

        // anchor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( anchorProp, new GUIContent("Anchor") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.anchor = (Anchor)anchorProp.enumValueIndex;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // offset
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( offsetProp, new GUIContent("Offset"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.offset = offsetProp.vector2Value;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // shear
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( shearProp, new GUIContent("Shear"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.shear = shearProp.vector2Value;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        // color
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( colorProp, new GUIContent("Color"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.color = colorProp.colorValue;
                    EditorUtility.SetDirty(sp);
                }
            }
        }
        
        // shader
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( shaderProp, new GUIContent("Shader") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exSpriteBase sp = obj as exSpriteBase;
                if ( sp ) {
                    sp.shader = shaderProp.objectReferenceValue as Shader;
                    EditorUtility.SetDirty(sp);
                }
            }
        }

        EditorGUILayout.Space();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public new void OnSceneGUI () {
        exSpriteBase spriteBase = target as exSpriteBase;
        exEditorUtility.GL_DrawWireFrame(spriteBase, Color.white, false);

        if ( spriteBase && spriteBase.customSize ) {
            Vector3 size;
            Vector3 center;
            bool changed = ProcessSceneEditorHandles ( out size, out center );
            if ( changed ) {
                //center.z = originalCenterZ;
                exISprite sprite = spriteBase as exISprite;
                if (sprite != null) {
                    ApplySpriteScale (sprite, size, center);

                    // also update all planes in the same compnent
                    exPlane[] planes = spriteBase.GetComponents<exPlane>();
                    for ( int i = 0; i < planes.Length; ++i ) {
                        exPlane plane = planes[i];
                        if ( plane != this ) {
                            plane.width = sprite.width;
                            plane.height = sprite.height;
                            plane.anchor = sprite.anchor;
                            plane.offset = sprite.offset;
                        }
                    }
                }
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    // Apply exSprite or ex3DSprite change 
    // ------------------------------------------------------------------ 

    public static void ApplySpriteScale (exISprite _sprite, Vector3 _size, Vector3 _center) {
        if (_sprite.spriteType == exSpriteType.Sliced && _sprite.textureInfo != null && _sprite.textureInfo.hasBorder) {
            _size.x = Mathf.Max(_size.x, _sprite.leftBorderSize + _sprite.rightBorderSize);
            _size.y = Mathf.Max(_size.y, _sprite.bottomBorderSize + _sprite.topBorderSize);
        }

        _sprite.width = _size.x;
        _sprite.height = _size.y;

        Vector3 offset = new Vector3( -_sprite.offset.x, -_sprite.offset.y, 0.0f );
        Vector3 anchorOffset = Vector3.zero;

        switch (_sprite.anchor) {
        case Anchor.TopLeft:    anchorOffset = new Vector3( -_size.x*0.5f,  _size.y*0.5f, 0.0f ); break;
        case Anchor.TopCenter:  anchorOffset = new Vector3(          0.0f,  _size.y*0.5f, 0.0f ); break;
        case Anchor.TopRight:   anchorOffset = new Vector3(  _size.x*0.5f,  _size.y*0.5f, 0.0f ); break;
        case Anchor.MidLeft:    anchorOffset = new Vector3( -_size.x*0.5f,          0.0f, 0.0f ); break;
        case Anchor.MidCenter:  anchorOffset = new Vector3(          0.0f,          0.0f, 0.0f ); break;
        case Anchor.MidRight:   anchorOffset = new Vector3(  _size.x*0.5f,          0.0f, 0.0f ); break;
        case Anchor.BotLeft:    anchorOffset = new Vector3( -_size.x*0.5f, -_size.y*0.5f, 0.0f ); break;
        case Anchor.BotCenter:  anchorOffset = new Vector3(          0.0f, -_size.y*0.5f, 0.0f ); break;
        case Anchor.BotRight:   anchorOffset = new Vector3(  _size.x*0.5f, -_size.y*0.5f, 0.0f ); break;
        }

        Vector3 scaledOffset = offset + anchorOffset - (Vector3)_sprite.GetTextureOffset();
        Transform trans = _sprite.transform;
        Vector3 lossyScale = trans.lossyScale;
        scaledOffset.x *= lossyScale.x;
        scaledOffset.y *= lossyScale.y;
        Vector3 newPos = _center + trans.rotation * scaledOffset;
        Vector3 localPos = trans.InverseTransformPoint (newPos);
        localPos.z = 0; // keep z unchagned
        trans.position = trans.TransformPoint (localPos);
    }
}

