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
    static void Create_BitmapFont () {
        exGenericAssetUtility<exBitmapFont>.CreateInCurrentDirectory ("New BitmapFont");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/UI-Layout Info", false, 1004)]
    static void Create_UILayoutInfo () {
        exGenericAssetUtility<exUILayoutInfo>.CreateInCurrentDirectory ("New UILayoutInfo");
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Create From Selected
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/TextureInfo From Selected", false, 2000)]
    static void Create_TextureInfo_FromSelected () {
        Texture2D rawTexture = Selection.activeObject as Texture2D;

        // check if this is a font info
        if ( rawTexture == null ) {
            Debug.LogError ( "You can only create texture-info from selected texture" );
            return;
        }

        string rawTexturePath = AssetDatabase.GetAssetPath(rawTexture);
        string dirPath = Path.GetDirectoryName(rawTexturePath);

        exTextureInfo textureInfo = exGenericAssetUtility<exTextureInfo>.LoadExistsOrCreate ( dirPath, rawTexture.name );
        textureInfo.rawTextureGUID = exEditorUtility.AssetToGUID(rawTexture);
        textureInfo.rawAtlasGUID = exEditorUtility.AssetToGUID(rawTexture);
        textureInfo.texture = rawTexture;
        textureInfo.rotated = false;
        textureInfo.trim = true;
        textureInfo.trimThreshold = 1;
        Rect trimRect = exTextureUtility.GetTrimTextureRect( rawTexture, 
                                                             1,
                                                             new Rect( 0, 0, rawTexture.width, rawTexture.height ) );
        if ( trimRect.width <= 0 || trimRect.height <= 0 ) {
            textureInfo.trim = false;
            trimRect = new Rect ( 0, 0, rawTexture.width, rawTexture.height );
        }
        textureInfo.trim_x = (int)trimRect.x;
        textureInfo.trim_y = (int)trimRect.y;
        textureInfo.width = (int)trimRect.width;
        textureInfo.height = (int)trimRect.height;
        textureInfo.x = (int)trimRect.x;
        textureInfo.y = (int)trimRect.y;
        textureInfo.rawWidth = rawTexture.width;
        textureInfo.rawHeight = rawTexture.height;


        AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath(textureInfo) );
        Selection.activeObject = textureInfo;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("Assets/Create/ex2D/Bitmap Font From Selected", false, 2001)]
    static void Create_BitmapFont_FromSelected () {
        Object fontInfo = Selection.activeObject; // font info is a ".txt" or ".fnt" text file

        // check if this is a font info
        if ( exBitmapFontUtility.IsFontInfo(fontInfo) == false ) {
            Debug.LogError ( "The file you choose to parse is not a font-info file. Must be \".txt\", \".fnt\" file" );
            return;
        }

        // check if the bitmapfont asset already exists
        string fontInfoPath = AssetDatabase.GetAssetPath(fontInfo);
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

    // DEBUG { 
    // // ------------------------------------------------------------------ 
    // // Desc: 
    // // ------------------------------------------------------------------ 

    // [MenuItem ("ex2D/Unload Unused Assets", false, 200)]
    // static void ex2D_UnloadUnusedAssets () {
    //     EditorUtility.UnloadUnusedAssets();
    //     // EditorUtility.UnloadUnusedAssetsIgnoreManagedReferences();
    // }
    // } DEBUG end 

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

    ///////////////////////////////////////////////////////////////////////////////
    // Create Other
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("GameObject/Create Other/ex2D/3D Sprite", false, 10000)]
    static void Create_3DSprite () {
        GameObject newGO = new GameObject("New 3D Sprite");
        ex3DSprite sprite = newGO.AddComponent<ex3DSprite>();
        sprite.shader = Shader.Find("ex2D/Alpha Blended");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    [MenuItem ("GameObject/Create Other/ex2D/3D Sprite Font", false, 10001)]
    static void Create_3DSpriteFont () {
        GameObject newGO = new GameObject("New 3D SpriteFont");
        ex3DSpriteFont spriteFont = newGO.AddComponent<ex3DSpriteFont>();
        spriteFont.shader = Shader.Find("ex2D/Alpha Blended");
        spriteFont.text = "Hello World";
    }
}
