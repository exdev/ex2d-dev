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
class exSpriteBaseInspector : Editor {

    SerializedProperty customSizeProp;
    SerializedProperty widthProp;
    SerializedProperty heightProp;
    SerializedProperty anchorProp;
    SerializedProperty offsetProp;
    SerializedProperty shearProp;
    SerializedProperty colorProp;
    SerializedProperty shaderProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        InitProperties ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        // NOTE: DO NOT call serializedObject.ApplyModifiedProperties ();
        serializedObject.Update ();

        EditorGUILayout.Space();
        EditorGUIUtility.LookLikeInspector();

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
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected virtual void OnSceneGUI () {
        exSpriteBase sprite = target as exSpriteBase;
        exEditorUtility.GL_DrawWireFrame(sprite, Color.white, false);
        ProcessSceneEditorHandles ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessSceneEditorHandles () {
        // TODO: 由于mesh的transform，exLayeredSprite的控制点会有一定的z位移
        exSpriteBase spriteBase = target as exSpriteBase;
        Transform trans = spriteBase.transform;
        if ( trans ) {
            Vector3 trans_position = trans.position;
            float handleSize = HandleUtility.GetHandleSize(trans_position);

            // resize
            if ( spriteBase && spriteBase.customSize ) {
                // TODO: limit the size { 
                // float minWidth = float.MinValue;
                // float minHeight = float.MinValue;
                // if ( layeredSprite is exSprite ) {
                //     exSprite sp = layeredSprite as exSprite;
                //     if ( sp.spriteType == exSpriteType.Sliced ) {
                //         minWidth = sp.textureInfo.borderLeft + sp.textureInfo.borderRight;
                //         minHeight = sp.textureInfo.borderTop + sp.textureInfo.borderBottom;
                //     }
                // }
                // } TODO end 

                Vector3[] vertices = spriteBase.GetLocalVertices();
                Rect aabb = exGeometryUtility.GetAABoundingRect(vertices);
                Vector3 center = aabb.center; // NOTE: this value will become world center after Handles.Slider(s)
                Vector3 size = new Vector3( aabb.width, aabb.height, 0.0f );

                Vector3 tl = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f,
                                                                 center.y + size.y * 0.5f,
                                                                 0.0f ) );
                Vector3 tc = trans.TransformPoint ( new Vector3 ( center.x,
                                                                 center.y + size.y * 0.5f,
                                                                 0.0f ) );
                Vector3 tr = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f,
                                                                 center.y + size.y * 0.5f,
                                                                 0.0f ) );
                Vector3 ml = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f,
                                                                 center.y,
                                                                 0.0f ) );
                Vector3 mr = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f,
                                                                 center.y,
                                                                 0.0f ) );
                Vector3 bl = trans.TransformPoint ( new Vector3 ( center.x - size.x * 0.5f,
                                                                 center.y - size.y * 0.5f,
                                                                 0.0f ) );
                Vector3 bc = trans.TransformPoint ( new Vector3 ( center.x,
                                                                 center.y - size.y * 0.5f,
                                                                 0.0f ) );
                Vector3 br = trans.TransformPoint ( new Vector3 ( center.x + size.x * 0.5f,
                                                                 center.y - size.y * 0.5f,
                                                                 0.0f ) );

                Vector3 dir_up = trans.up;
                Vector3 dir_right = trans.right;
                Vector3 delta = Vector3.zero;
                bool changed = false;

                EditorGUI.BeginChangeCheck();
                Vector3 ml2 = Handles.Slider ( ml, dir_right, handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = ml2 - ml;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta = -delta;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (ml2 + mr) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 mr2 = Handles.Slider ( mr, dir_right, handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = mr2 - mr;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (mr2 + ml) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 tc2 = Handles.Slider ( tc, dir_up,    handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tc2 - tc;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (tc2 + bc) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 bc2 = Handles.Slider ( bc, dir_up,    handleSize * 0.05f, Handles.DotCap, -1 );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = bc2 - bc;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta = -delta;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (bc2 + tc) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 tr2 = Handles.FreeMoveHandle ( tr, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tr2 - tr;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (tr2 + bl) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 tl2 = Handles.FreeMoveHandle ( tl, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = tl2 - tl;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.x = -delta.x;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (tl2 + br) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 br2 = Handles.FreeMoveHandle ( br, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = br2 - br;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta.y = -delta.y;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (br2 + tl) * 0.5f;
                    changed = true;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 bl2 = Handles.FreeMoveHandle ( bl, trans.rotation, handleSize * 0.05f, Vector3.zero, Handles.DotCap );
                if ( EditorGUI.EndChangeCheck() ) {
                    delta = bl2 - bl;
                    delta = Quaternion.Inverse(trans.rotation) * delta.normalized * delta.magnitude;
                    delta = -delta;
                    delta.x /= trans.lossyScale.x;
                    delta.y /= trans.lossyScale.y;
                    size += delta;
                    center = (bl2 + tr) * 0.5f;
                    changed = true;
                }

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

            EditorGUI.BeginChangeCheck();
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

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void InitProperties () {
        customSizeProp = serializedObject.FindProperty("customSize_");
        widthProp = serializedObject.FindProperty("width_");
        heightProp = serializedObject.FindProperty("height_");
        anchorProp = serializedObject.FindProperty("anchor_");
        offsetProp = serializedObject.FindProperty("offset_");
        shearProp = serializedObject.FindProperty("shear_");
        colorProp = serializedObject.FindProperty("color_");
        shaderProp = serializedObject.FindProperty("shader_");
    }
}

