// ======================================================================================
// File         : MenuItems.cs
// Author       : Wu Jie 
// Last Change  : 02/17/2013 | 21:51:52 PM | Sunday,February
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

public static class MenuItems {

    ///////////////////////////////////////////////////////////////////////////////
    // Create
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/Texture Info")]
    static void Create_TextureInfo () { exGenericAssetUtility<exTextureInfo>.CreateInCurrentDirectory ("New TextureInfo"); }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/Atlas")]
    static void Create_Atlas () {
        exGenericAssetUtility<exAtlas>.CreateInCurrentDirectory ("New Atlas");
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Window
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/Atlas Editor", false, 101)]
    static void Open_AtlasEditor () {
        EditorWindow.GetWindow<exAtlasEditor>();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/2D Scene Editor", false, 100)]
    static void Open_SceneEditor () {
        EditorWindow.GetWindow<exSceneEditor>();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/Preferences...", false, 1000)]
    public static void Open_PreferenceWindow () {
        ScriptableWizard.DisplayWizard<ex2DPreferencesWindow>("ex2D Preferences");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/About...", false, 1001)]
    public static void Open_AboutWindow () {
        ScriptableWizard.DisplayWizard<ex2DAboutWindow>("About ex2D");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/Unload Unused Assets", false, 200)]
    static void ex2D_UnloadUnusedAssets () {
        EditorUtility.UnloadUnusedAssets();
        // EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
    }
}
