// ======================================================================================
// File         : exElementContentWizard.cs
// Author       : Wu Jie 
// Last Change  : 09/04/2013 | 14:20:20 PM | Wednesday,September
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

class exElementContentWizard : ScriptableWizard {
    public exUILayoutInfo curLayout;
    public exUIElement curEdit;

	Vector2 scroll;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDisable () {
        curEdit = null;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        Event e = Event.current;
        switch ( e.type ) {
        case EventType.KeyDown:
            if ( e.keyCode == KeyCode.Escape 
              || ( e.keyCode == KeyCode.Return && e.control ) 
              || ( e.keyCode == KeyCode.Return && e.command ) ) 
            {
                Close();
                return;
            }
            break;
        }

        //
        EditorGUILayout.Space ();

        //
        scroll = EditorGUILayout.BeginScrollView(scroll);		
        EditorGUI.BeginChangeCheck();
            curEdit.content = EditorGUILayout.TextArea(curEdit.content, GUILayout.Height(position.height - 50));		
        if ( EditorGUI.EndChangeCheck() ) {
            curLayout.Apply();
        }
        EditorGUILayout.EndScrollView();

        GUILayout.Label( "Press Ctrl/Command + Enter to commit" );
    }
}
