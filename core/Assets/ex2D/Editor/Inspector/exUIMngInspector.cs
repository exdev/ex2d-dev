// ======================================================================================
// File         : exUIMngInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/09/2013 | 10:02:43 AM | Wednesday,October
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
[CustomEditor(typeof(exUIMng))]
class exUIMngInspector : Editor {

    float areaY = 0.0f;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        DrawDefaultInspector(); 

        EditorGUILayout.Space();
        if ( Event.current.type == EventType.Repaint ) {
            Rect lastRect = GUILayoutUtility.GetLastRect ();
            areaY = lastRect.yMax;
        }

        exUIMng mng = target as exUIMng;
        if ( mng.showDebugInfo ) {

            int areaWidth = Screen.width-40;
            int areaHeight = 300;

            mng.ShowDebugInfo ( new Rect ( 10, areaY, areaWidth, areaHeight ) );
            GUILayoutUtility.GetRect ( areaWidth, areaHeight );
            EditorGUILayout.Space();

            // NOTE: without this, we can not catch state each frame.
            Repaint();
        }
    }
}

