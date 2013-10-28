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

	public override void OnInspectorGUI () {
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

            // Select
            if ( GUILayout.Button("Select...", GUILayout.Width(60), GUILayout.Height(20) ) ) {
                Object obj = exEditorUtility.LoadAssetFromGUID<Object>( bitmapFont.rawFontGUID );
                EditorGUIUtility.PingObject(obj);
            }

            // Rebuild
            if ( GUILayout.Button("Rebuild...", GUILayout.Width(80), GUILayout.Height(20) ) ) {
                if ( exBitmapFontUtility.IsFontInfo(newRef) == false ) {
                    Debug.LogError ( "The file you choose to parse is not a font-info file. Must be \".txt\", \".fnt\" file" );
                    return;
                }

                exBitmapFontUtility.Parse( bitmapFont, newRef );
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}

