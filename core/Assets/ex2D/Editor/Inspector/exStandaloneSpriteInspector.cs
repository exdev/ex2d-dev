// ======================================================================================
// File         : exStandaloneSpriteInspector.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 15:17:17 PM | Friday,August
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
[CustomEditor(typeof(exStandaloneSpriteInspector))]
class exStandaloneSpriteInspector : exSpriteBaseInspector {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	/*protected virtual void OnSceneGUI () {
        exStandaloneSprite sprite = target as exStandaloneSprite;
        // Vector3[] vertices = sprite.GetWorldVertices();
        // if (vertices.Length > 0) {
        //     Vector3[] vertices2 = new Vector3[vertices.Length+1];
        //     for ( int i = 0; i < vertices.Length; ++i )
        //         vertices2[i] = vertices[i];
        //     vertices2[vertices.Length] = vertices[0];

        //     Handles.DrawPolyLine( vertices2 );
        // }
    }*/
}
