// ======================================================================================
// File         : exPlaneInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/08/2013 | 10:59:46 AM | Tuesday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ex2D.Detail;

///////////////////////////////////////////////////////////////////////////////
// BoardPatternInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(exPlane))]
class exPlaneInspector : Editor {

    protected SerializedProperty widthProp;
    protected SerializedProperty heightProp;
    protected SerializedProperty anchorProp;
    protected SerializedProperty offsetProp;

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
        serializedObject.Update ();

        // NOTE DANGER: Unity didn't allow user change script in custom inspector { 
        SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField ( scriptProp );
        // } DANGER end 

        DoInspectorGUI ();
        serializedObject.ApplyModifiedProperties ();

        // if we have sprite
        exPlane targetPlane = target as exPlane;
        if ( targetPlane.hasSprite ) {
            exSpriteBase spriteBase = targetPlane.GetComponent<exSpriteBase>();
            if ( targetPlane.width != spriteBase.width ) {
                targetPlane.width = spriteBase.width;
                EditorUtility.SetDirty(targetPlane);
            }

            if ( targetPlane.height != spriteBase.height ) {
                targetPlane.height = spriteBase.height;
                EditorUtility.SetDirty(targetPlane);
            }

            if ( targetPlane.anchor != spriteBase.anchor ) {
                targetPlane.anchor = spriteBase.anchor;
                EditorUtility.SetDirty(targetPlane);
            }

            if ( targetPlane.offset != spriteBase.offset ) {
                targetPlane.offset = spriteBase.offset;
                EditorUtility.SetDirty(targetPlane);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void OnSceneGUI () {
        exPlane plane = target as exPlane;
        exEditorUtility.GL_DrawWireFrame(plane, new Color( 1.0f, 0.0f, 0.5f, 1.0f ), false);

        if ( plane.hasSprite == false ) {
            Vector3 size;
            Vector3 center;
            bool changed = ProcessSceneEditorHandles ( out size, out center );
            if ( changed ) {
                exPlaneInspector.ApplyPlaneScale ( plane, size, center );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected bool ProcessSceneEditorHandles ( out Vector3 _size, out Vector3 _center ) {
        bool changed = false;
        exPlane plane = target as exPlane;
        Transform trans = plane.transform;
        Vector3 size = Vector3.zero;
        Vector3 center = Vector3.zero;

        if ( trans ) {
            Vector3 trans_position = trans.position;
            float handleSize = HandleUtility.GetHandleSize(trans_position);

            // resize
            if ( plane ) {
                Vector3[] vertices = plane.GetLocalVertices();
                Rect aabb = exGeometryUtility.GetAABoundingRect(vertices);
                center = aabb.center; // NOTE: this value will become world center after Handles.Slider(s)
                size = new Vector3( aabb.width, aabb.height, 0.0f );

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
            }
        }

        _size = size;
        _center = center;
        return changed;
    }
    
    // ------------------------------------------------------------------ 
    // Apply exSprite or ex3DSprite change 
    // ------------------------------------------------------------------ 

    public static void ApplyPlaneScale (exPlane _plane, Vector3 _size, Vector3 _center) {
        _plane.width = _size.x;
        _plane.height = _size.y;

        Vector3 offset = new Vector3( -_plane.offset.x, -_plane.offset.y, 0.0f );
        Vector3 anchorOffset = Vector3.zero;

        switch (_plane.anchor) {
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

        Vector3 scaledOffset = offset + anchorOffset;
        Transform trans = _plane.transform;
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

    protected virtual void InitProperties () {
        widthProp = serializedObject.FindProperty("width_");
        heightProp = serializedObject.FindProperty("height_");
        anchorProp = serializedObject.FindProperty("anchor_");
        offsetProp = serializedObject.FindProperty("offset_");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected virtual void DoInspectorGUI () {
        // width
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( widthProp, new GUIContent("Width") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exPlane plane = obj as exPlane;
                if ( plane ) {
                    plane.width = Mathf.Max(widthProp.floatValue, 0f);
                    EditorUtility.SetDirty(plane);
                }
            }
        }

        // height
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( heightProp, new GUIContent("Height") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exPlane plane = obj as exPlane;
                if ( plane ) {
                    plane.height = Mathf.Max(heightProp.floatValue, 0f);
                    EditorUtility.SetDirty(plane);
                }
            }
        }

        // anchor
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( anchorProp, new GUIContent("Anchor") );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exPlane plane = obj as exPlane;
                if ( plane ) {
                    plane.anchor = (Anchor)anchorProp.enumValueIndex;
                    EditorUtility.SetDirty(plane);
                }
            }
        }

        // offset
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField ( offsetProp, new GUIContent("Offset"), true );
        if ( EditorGUI.EndChangeCheck() ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exPlane plane = obj as exPlane;
                if ( plane ) {
                    plane.offset = offsetProp.vector2Value;
                    EditorUtility.SetDirty(plane);
                }
            }
        }
    }
}

