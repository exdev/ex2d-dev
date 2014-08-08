// ======================================================================================
// File         : exAnimationDebugger.cs
// Author       : Wu Jie 
// Last Change  : 01/16/2014 | 15:21:41 PM | Thursday,January
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// the unity animation debugger
///
///////////////////////////////////////////////////////////////////////////////

class exAnimationDebugger : exGenericComponentDebugger<Animation> {

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \return the editor
    /// Open the animation debug window
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/Debugger/Animation Debugger")]
    public static exAnimationDebugger NewWindow () {
        exAnimationDebugger newWindow = EditorWindow.GetWindow<exAnimationDebugger>();
        return newWindow;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void OnEnable () {
        base.OnEnable();
        name = "Animation Debugger";
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected override void ShowDebugInfo () {
        Color colorTitle = Color.black;
        Color colorActive = Color.blue;
        Color colorDeactive = new Color( 0.5f, 0.5f, 0.5f );

        if ( EditorGUIUtility.isProSkin ) {
            colorTitle = new Color( 0.8f, 0.8f, 0.8f );
            colorActive = Color.green;
            colorDeactive = new Color( 0.4f, 0.4f, 0.4f );
        }

        GUILayout.BeginHorizontal ();
            GUILayout.Space(5);
            textStyle.normal.textColor = colorTitle;
            textStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label ( "layer"  , textStyle , GUILayout.Width(40) );
            GUILayout.Label ( "name"   , textStyle , new GUILayoutOption[] {} );
            GUILayout.Label ( "weight" , textStyle , GUILayout.Width(50) );
            GUILayout.Label ( "n-time" , textStyle , GUILayout.Width(50) );
            GUILayout.Label ( "speed"  , textStyle , GUILayout.Width(50) );
        GUILayout.EndHorizontal ();

        GUILayout.Space(5);

        foreach ( AnimationState state in curEdit ) {
            GUILayout.BeginHorizontal ();
                GUILayout.Space(5);
                textStyle.fontStyle = FontStyle.Normal;
                textStyle.normal.textColor = state.enabled ? colorActive : colorDeactive;
                GUILayout.Label ( "[" + state.layer + "]"             , textStyle , GUILayout.Width(40) );
                GUILayout.Label ( state.name                          , textStyle , new GUILayoutOption[] {} );
                GUILayout.Label ( state.weight.ToString("f3")         , textStyle , GUILayout.Width(50) );
                GUILayout.Label ( state.normalizedTime.ToString("f3") , textStyle , GUILayout.Width(50) );
                GUILayout.Label ( state.speed.ToString("f3")          , textStyle , GUILayout.Width(50) );
            GUILayout.EndHorizontal ();
        }
    } 
}
