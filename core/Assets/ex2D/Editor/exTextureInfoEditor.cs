// ======================================================================================
// File         : exTextureInfoEditor.cs
// Author       : Wu Jie 
// Last Change  : 08/21/2013 | 14:27:34 PM | Wednesday,August
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
/// The Texture Info Editor
///
///////////////////////////////////////////////////////////////////////////////

class exTextureInfoEditor : EditorWindow {

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    exTextureInfo curEdit = null;

    ///////////////////////////////////////////////////////////////////////////////
    // builtin function override
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnEnable () {
        title = "TextureInfo Editor";
        wantsMouseMove = true;
        autoRepaintOnSceneChange = false;
        minSize = new Vector2(500f, 500f);

        // quadMaterial = new Material( Shader.Find("ex2D/Alpha Blended") );
        // quadMaterial.hideFlags = HideFlags.DontSave;
        // quadMesh = new Mesh();
        // quadMesh.hideFlags = HideFlags.DontSave;

        // rectSelection = new exRectSelection<Object>( PickObject,
        //                                              PickRectObjects,
        //                                              ConfirmRectSelection );

        // UpdateEditObject ();
    }

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    /// \param _obj
    /// Check if the object is valid atlas and open it in atlas editor.
    // ------------------------------------------------------------------ 

    public void Edit ( exTextureInfo _info ) {
        if ( _info == null )
            return;

        curEdit = _info;

        Reset ();
        Repaint ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Reset () {
    }
}
