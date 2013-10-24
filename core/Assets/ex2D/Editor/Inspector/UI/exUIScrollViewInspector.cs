// ======================================================================================
// File         : exUIScrollViewInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/16/2013 | 16:29:15 PM | Wednesday,October
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
using System.Reflection;

///////////////////////////////////////////////////////////////////////////////
// BoardPatternInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(exUIScrollView))]
class exUIScrollViewInspector : exUIControlInspector {

    protected SerializedProperty draggableProp;
    protected SerializedProperty dragEffectProp;
    protected SerializedProperty showConditionProp;
    protected SerializedProperty contentAnchorProp;
    protected SerializedProperty contentSizeProp;
    protected SerializedProperty allowHorizontalScrollProp;
    protected SerializedProperty allowVerticalScrollProp;
    protected SerializedProperty scrollSpeedProp;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();

        draggableProp = serializedObject.FindProperty("draggable");
        dragEffectProp = serializedObject.FindProperty("dragEffect");
        showConditionProp = serializedObject.FindProperty("showCondition");
        contentAnchorProp = serializedObject.FindProperty("contentAnchor");
        contentSizeProp = serializedObject.FindProperty("contentSize_");
        allowHorizontalScrollProp = serializedObject.FindProperty("allowHorizontalScroll");
        allowVerticalScrollProp = serializedObject.FindProperty("allowVerticalScroll");
        scrollSpeedProp = serializedObject.FindProperty("scrollSpeed");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        EditorGUILayout.PropertyField ( draggableProp );
        EditorGUILayout.PropertyField ( dragEffectProp );
        EditorGUILayout.PropertyField ( showConditionProp );
        EditorGUILayout.PropertyField ( contentAnchorProp );
        EditorGUILayout.PropertyField ( contentSizeProp, new GUIContent("Content Size") );
        EditorGUILayout.PropertyField ( allowHorizontalScrollProp );
        EditorGUILayout.PropertyField ( allowVerticalScrollProp );
        EditorGUILayout.PropertyField ( scrollSpeedProp );

        if ( serializedObject.isEditingMultipleObjects == false ) {
            exUIScrollView scrollView = target as exUIScrollView;
            if ( scrollView != null &&
                 scrollView.contentAnchor == null &&
                 scrollView.transform.childCount > 0 ) 
            {
                scrollView.contentAnchor = scrollView.transform.GetChild(0);
                EditorUtility.SetDirty (target);
            }
        }

        EditorGUILayout.Space();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public new void OnSceneGUI () {
        exUIControl ctrl = target as exUIControl;
        Vector3[] vertices = ctrl.GetLocalVertices();
        if (vertices.Length > 0) {
            Rect aabb = exGeometryUtility.GetAABoundingRect(vertices);
            Matrix4x4 l2w = ctrl.transform.localToWorldMatrix;

            // draw control rect
            vertices = new Vector3[4] {
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMin, 0)),
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMax, 0)),
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMax, 0)),
                l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMin, 0)),
            };
            exEditorUtility.GL_DrawRectLine(vertices, new Color( 1.0f, 0.0f, 0.5f, 1.0f ), true);

            // draw scroll-view content
            exUIScrollView scrollView = ctrl as exUIScrollView;
            if ( scrollView != null ) {
                aabb.width = scrollView.contentSize.x;
                aabb.yMin = aabb.yMax - scrollView.contentSize.y;
                aabb.center += scrollView.GetScrollOffset();
                vertices = new Vector3[4] {
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMin, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMin, aabb.yMax, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMax, 0)),
                    l2w.MultiplyPoint3x4(new Vector3(aabb.xMax, aabb.yMin, 0)),
                };
                exEditorUtility.GL_DrawRectLine(vertices, new Color( 0.0f, 0.5f, 1.0f, 1.0f ), true);
            }
        }

        exPlane plane = target as exPlane;
        if ( plane.hasSprite == false ) {
            Vector3 size;
            Vector3 center;
            bool changed = ProcessSceneEditorHandles ( out size, out center );
            if ( changed ) {
                ApplyPlaneScale ( plane, size, center );
            }
        }
    }
}
