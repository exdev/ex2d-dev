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

    Material quadMaterial = null;
    Mesh quadMesh = null;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        quadMaterial = new Material( Shader.Find("ex2D/Alpha Blended") );
        quadMaterial.hideFlags = HideFlags.DontSave;
        quadMesh = new Mesh();
        quadMesh.hideFlags = HideFlags.DontSave;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        Object.DestroyImmediate(quadMaterial);
        Object.DestroyImmediate(quadMesh);
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        // get old trim value
        serializedObject.Update ();
        SerializedProperty propTrim = serializedObject.FindProperty("trim");
        bool oldTrim = propTrim.boolValue;

        DrawDefaultInspector(); 

        // process trim property
        if ( propTrim.boolValue != oldTrim ) {
            foreach ( Object obj in serializedObject.targetObjects ) {
                exTextureInfo textureInfo = obj as exTextureInfo;
                if ( textureInfo == null ) {
                    continue;
                }

                Texture2D rawTexture = exEditorUtility.LoadAssetFromGUID<Texture2D> ( textureInfo.rawTextureGUID );
                if ( rawTexture == null ) {
                    continue;
                }

                if ( propTrim.boolValue ) {
                    Rect trimRect = exTextureUtility.GetTrimTextureRect( rawTexture,
                                                                         textureInfo.trimThreshold,
                                                                         new Rect( 0, 0, rawTexture.width, rawTexture.height ) );
                    if ( trimRect.width <= 0 || trimRect.height <= 0 ) {
                        Debug.LogWarning ( "Can't not trim texture " + obj.name + ", empty pixel in it" );
                        trimRect = new Rect ( 0, 0, rawTexture.width, rawTexture.height );
                        textureInfo.trim = false;
                    }
                    textureInfo.trim_x = (int)trimRect.x;
                    textureInfo.trim_y = (int)trimRect.y;
                    textureInfo.width = (int)trimRect.width;
                    textureInfo.height = (int)trimRect.height;
                }
                else {
                    textureInfo.trim_x = 0;
                    textureInfo.trim_y = 0;
                    textureInfo.width = rawTexture.width;
                    textureInfo.height = rawTexture.height;
                }
                EditorUtility.SetDirty(textureInfo);
            }
        }

        //
        if ( serializedObject.isEditingMultipleObjects == false ) {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

                // Select
                if ( GUILayout.Button("Select...", GUILayout.Width(60), GUILayout.Height(20) ) ) {
                    exTextureInfo textureInfo = target as exTextureInfo; 
                    Texture texture = exEditorUtility.LoadAssetFromGUID<Texture>( textureInfo.rawTextureGUID );
                    EditorGUIUtility.PingObject(texture);
                }

                // Atlas
                if ( GUILayout.Button("Atlas...", GUILayout.Width(60), GUILayout.Height(20) ) ) {
                    exAtlasEditor editor = EditorWindow.GetWindow<exAtlasEditor>();
                    exTextureInfo textureInfo = target as exTextureInfo; 
                    exAtlas atlas = exEditorUtility.LoadAssetFromGUID<exAtlas>( textureInfo.rawAtlasGUID );
                    editor.Edit(atlas);
                }

                // Edit
                if ( GUILayout.Button("Edit...", GUILayout.Width(60), GUILayout.Height(20) ) ) {
                    exTextureInfoEditor editor = EditorWindow.GetWindow<exTextureInfoEditor>();
                    exTextureInfo textureInfo = target as exTextureInfo; 
                    editor.Edit(textureInfo);
                }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();
        }
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
        rect = exGeometryUtility.Rect_FloorToInt(rect);
        // EditorGUI.DrawPreviewTexture(rect, textureInfo.texture);

        exEditorUtility.GUI_DrawTextureInfo ( rect,
                                              textureInfo,
                                              Color.white );
        // if ( textureInfo.rotated ) {
        //     float xStart = (float)textureInfo.x/(float)textureInfo.texture.width;
        //     float xEnd = xStart + (float)textureInfo.rotatedWidth/(float)textureInfo.texture.width;
        //     float yStart = (float)textureInfo.y/(float)textureInfo.texture.height;
        //     float yEnd = yStart + (float)textureInfo.rotatedHeight/(float)textureInfo.texture.height;

        //     quadMaterial.mainTexture = textureInfo.texture;
        //     quadMaterial.SetPass(0);

        //     quadMesh.hideFlags = HideFlags.DontSave;
        //     quadMesh.vertices = new Vector3[] {
        //         new Vector3 ( rect.x, rect.y, 0.0f ),
        //         new Vector3 ( rect.x, rect.y + rect.height, 0.0f ),
        //         new Vector3 ( rect.x + rect.width, rect.y + rect.height, 0.0f ),
        //         new Vector3 ( rect.x + rect.width, rect.y, 0.0f ),
        //     };
        //     quadMesh.uv = new Vector2[] {
        //         new Vector2 ( xStart, yStart ),
        //         new Vector2 ( xEnd, yStart ),
        //         new Vector2 ( xEnd, yEnd ),
        //         new Vector2 ( xStart, yEnd ),
        //     };
        //     quadMesh.colors32 = new Color32[] {
        //         new Color32 ( 255, 255, 255, 255 ),
        //         new Color32 ( 255, 255, 255, 255 ),
        //         new Color32 ( 255, 255, 255, 255 ),
        //         new Color32 ( 255, 255, 255, 255 ),
        //     };
        //     quadMesh.triangles = new int[] {
        //         0, 1, 2,
        //         0, 2, 3
        //     };

        //     Graphics.DrawMeshNow ( quadMesh, Vector3.zero, Quaternion.identity );
        // }
        // else {
        //     float uv_s  = (float)textureInfo.x / (float)textureInfo.texture.width;
        //     float uv_t  = (float)textureInfo.y / (float)textureInfo.texture.height;
        //     float uv_w  = (float)textureInfo.rotatedWidth / (float)textureInfo.texture.width;
        //     float uv_h  = (float)textureInfo.rotatedHeight / (float)textureInfo.texture.height;

        //     GUI.DrawTextureWithTexCoords( rect, 
        //                                   textureInfo.texture, 
        //                                   new Rect( uv_s, uv_t, uv_w, uv_h ) );
        // }
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

