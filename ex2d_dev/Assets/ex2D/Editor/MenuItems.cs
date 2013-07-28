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
using System.IO;

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

    [MenuItem ("Assets/Create/ex2D/Texture Info", false, 1000)]
    static void Create_TextureInfo () { exGenericAssetUtility<exTextureInfo>.CreateInCurrentDirectory ("New TextureInfo"); }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/Atlas", false, 1001)]
    static void Create_Atlas () {
        exGenericAssetUtility<exAtlas>.CreateInCurrentDirectory ("New Atlas");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/Sprite Animation Clip", false, 1002)]
    static void Create_SpriteAnimationClip () {
        exGenericAssetUtility<exSpriteAnimationClip>.CreateInCurrentDirectory ("New SpriteAnimationClip");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/Bitmap Font", false, 1003)]
    static void ex2D_CreateBitmapFont () {
        Object fontInfo = Selection.activeObject; // font info is a ".txt" or ".fnt" text file
        string fontInfoPath = AssetDatabase.GetAssetPath(fontInfo);
        bool isFontInfo = (Path.GetExtension(fontInfoPath) == ".txt" || 
                           Path.GetExtension(fontInfoPath) == ".fnt");

        // check if this is a font info
        if ( isFontInfo == false ) {
            Debug.LogError ( "The file you choose to parse is not a font-info file. Must be \".txt\", \".fnt\" file" );
            return;
        }

        // check if the bitmapfont asset already exists
        string dirPath = Path.GetDirectoryName(fontInfoPath);
        string path = Path.Combine( dirPath, fontInfo.name + ".asset" );
        FileInfo fileInfo = new FileInfo(path);
        bool doCreate = true;
        if ( fileInfo.Exists ) {
            doCreate = EditorUtility.DisplayDialog( fontInfo.name + " already exists.",
                                                    "Do you want to overwrite the old one?",
                                                    "Yes", "No" );
        }
        if ( doCreate == false ) {
            return;
        }

        // parse the bitmap font
        exBitmapFont bitmapFont = exGenericAssetUtility<exBitmapFont>.LoadExistsOrCreate ( dirPath, fontInfo.name );
        bool result = exBitmapFontUtility.Parse ( bitmapFont, fontInfo );
        if ( result == false ) {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(bitmapFont));
            return;
        }

        AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath(bitmapFont) );
        Selection.activeObject = bitmapFont;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Window
    ///////////////////////////////////////////////////////////////////////////////

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

    [MenuItem ("ex2D/Atlas Editor", false, 101)]
    static void Open_AtlasEditor () {
        EditorWindow.GetWindow<exAtlasEditor>();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("ex2D/Sprite Animation Editor", false, 102)]
    static void Open_SpriteAnimationEditor () {
        EditorWindow.GetWindow<exSpriteAnimationEditor>();
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
