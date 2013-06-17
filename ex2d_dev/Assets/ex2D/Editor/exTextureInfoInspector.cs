// ======================================================================================
// File         : exTextureInfoInspector.cs
// Author       : Wu Jie 
// Last Change  : 06/17/2013 | 22:06:55 PM | Monday,June
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
[CustomEditor(typeof(exTextureInfo))]
class exTextureInfoInspector : Editor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        EditorGUIUtility.LookLikeInspector();
        DrawDefaultInspector(); 

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            if ( GUILayout.Button("Edit...", GUILayout.Width(50), GUILayout.Height(20) ) ) {
                // exAtlasInfoEditor editor = EditorWindow.GetWindow<exAtlasInfoEditor>();
                // editor.Edit(target);
            }
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override bool HasPreviewGUI () {
        return base.target != null;
    }

    // TODO { 
    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // public override void OnPreviewSettings () {
    // }
    // } TODO end 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override void OnPreviewGUI( Rect _rect, GUIStyle _background ) {
        if ( Event.current.type != EventType.Repaint ) {
            return;
        }

        exTextureInfo textureInfo = base.target as exTextureInfo; 
        if ( textureInfo == null || textureInfo.texture == null ) {
            return;
        }

        _background.Draw(_rect, false, false, false, false);

        float ratio = Mathf.Min ( Mathf.Min(_rect.width / (float)textureInfo.width, _rect.height / (float)textureInfo.height), 1.0f );
        float w = (float)textureInfo.width * ratio;
        float h = (float)textureInfo.height * ratio;
        Rect rect = new Rect ( _rect.x + (_rect.width - w) * 0.5f, 
                               _rect.y + (_rect.height - h) * 0.5f, 
                               w, 
                               h );
        // EditorGUI.DrawPreviewTexture(rect, textureInfo.texture);

        float uv_s  = (float)textureInfo.x / (float)textureInfo.texture.width;
        float uv_t  = (float)textureInfo.y / (float)textureInfo.texture.height;
        float uv_w  = (float)textureInfo.width / (float)textureInfo.texture.width;
        float uv_h  = (float)textureInfo.height / (float)textureInfo.texture.height;

        GUI.DrawTextureWithTexCoords( rect, 
                                      textureInfo.texture, 
                                      new Rect( uv_s, uv_t, uv_w, uv_h ) );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override string GetInfoString () {
        exTextureInfo textureInfo = base.target as exTextureInfo;
        string text = textureInfo.width.ToString() + "x" + textureInfo.height.ToString();

        return text;
    }
}

