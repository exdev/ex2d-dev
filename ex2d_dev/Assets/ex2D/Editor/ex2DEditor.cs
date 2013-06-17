// ======================================================================================
// File         : ex2DEditor.cs
// Author       : Wu Jie 
// Last Change  : 06/18/2013 | 00:02:57 AM | Tuesday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
 
[InitializeOnLoad]
class ex2DEditor {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static ex2DEditor () {
        // TODO: you can load texture_to_textureinfo table here
        // EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB
        // EditorApplication.projectWindowItemOnGUI += ProjectWindowItemOnGUI
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void ProjectWindowItemOnGUI ( string _guid, Rect _selectionRect ) {
    }
}
