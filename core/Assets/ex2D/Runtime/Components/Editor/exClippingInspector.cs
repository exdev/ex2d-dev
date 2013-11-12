// ======================================================================================
// File         : exClippingInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/28/2013 | 14:02:05 PM | Monday,October
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
[CustomEditor(typeof(exClipping))]
class exClippingInspector : exPlaneInspector {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public new void OnSceneGUI () {
        exClipping clipping = target as exClipping;
        exEditorUtility.GL_DrawWireFrame(clipping, new Color( 0.0f, 1.0f, 0.5f, 1.0f ), false);

        if ( clipping.hasSprite == false ) {
            Vector3 size;
            Vector3 center;
            bool changed = ProcessSceneEditorHandles ( out size, out center );
            if ( changed ) {
                exPlaneInspector.ApplyPlaneScale ( clipping, size, center );
                clipping.CheckDirty();
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void InitProperties () {
        base.InitProperties();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void DoInspectorGUI () {
        base.DoInspectorGUI();

        foreach ( Object obj in serializedObject.targetObjects ) {
            exClipping clipping = obj as exClipping;
            if ( clipping ) {
                clipping.CheckDirty();
            }
        }
    }
}

