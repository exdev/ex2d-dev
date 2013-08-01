// ======================================================================================
// File         : exBitmapFontInspector.cs
// Author       : Wu Jie 
// Last Change  : 07/28/2013 | 17:05:48 PM | Sunday,July
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
[CustomEditor(typeof(exBitmapFont))]
class exBitmapFontInspector : Editor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	override public void OnInspectorGUI () {
        EditorGUIUtility.LookLikeInspector();
        DrawDefaultInspector(); 

        EditorGUILayout.Space();


        exBitmapFont bitmapFont = target as exBitmapFont;
        Object oldRef = exEditorUtility.LoadAssetFromGUID<Object>( bitmapFont.rawFontGUID );
        Object newRef = EditorGUILayout.ObjectField ( "Import Data"
                                                      , oldRef
                                                      , typeof(Object)
                                                      , false );
        if ( oldRef != newRef ) {
            bitmapFont.rawFontGUID = exEditorUtility.AssetToGUID(newRef);
        }

        GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if ( GUILayout.Button("Rebuild...", GUILayout.Width(80), GUILayout.Height(20) ) ) {
                exBitmapFontUtility.Parse( bitmapFont, newRef );
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}

