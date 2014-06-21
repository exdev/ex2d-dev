// ======================================================================================
// File         : exGenericAssetUtility.cs
// Author       : Wu Jie 
// Last Change  : 02/19/2012 | 21:22:54 PM | Sunday,February
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

public static class exGenericAssetUtility<T> where T : ScriptableObject {

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public static string SaveFileInProject ( string _title, string _dirPath, string _fileName, string _extension ) {
		string path = EditorUtility.SaveFilePanel(_title, _dirPath, _fileName, _extension);

        // cancelled
		if ( path.Length == 0 )
			return "";

		string cwd = System.IO.Directory.GetCurrentDirectory().Replace("\\","/") + "/assets/";
		if ( path.ToLower().IndexOf(cwd.ToLower()) != 0 ) {
			path = "";
			EditorUtility.DisplayDialog(_title, "Assets must be saved inside the Assets folder", "Ok");
		}
		else {
			path = path.Substring ( cwd.Length - "/assets".Length );
		}
		return path;
	}

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static T LoadExistsOrCreate ( string _path, string _name ) {
        T asset = AssetDatabase.LoadAssetAtPath ( Path.Combine(_path,_name+".asset"), typeof(T) ) as T;
        if ( asset == null ) {
            asset = Create ( _path, _name );
        }
        return asset;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static T Create ( string _path, string _name ) {
        // check if the asset is valid to create
        if ( new DirectoryInfo(_path).Exists == false ) {
            Debug.LogError ( "can't create asset, path not found" );
            return null;
        }
        if ( string.IsNullOrEmpty(_name) ) {
            Debug.LogError ( "can't create asset, the name is empty" );
            return null;
        }
        string assetPath = Path.Combine( _path, _name + ".asset" );
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

        //
        T newAsset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(newAsset, assetPath);
        return newAsset;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void CreateInCurrentDirectory ( string _assetName ) {
        // get current selected directory
        string assetPath = "Assets";
        Object selectFolder = null;

        //
        if ( Selection.activeObject ) {
            selectFolder = Selection.activeObject;
        }
        // else if ( Selection.objects.Length > 0 ) {
        //     foreach ( Object obj in Selection.objects ) {
        //         if ( exEditorUtility.IsDirectory(obj) ) {
        //             selectFolder = obj;
        //             break;
        //         }
        //     }
        // }

        //
        if ( selectFolder != null ) {
            assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if ( Path.GetExtension(assetPath) != "" ) {
                assetPath = Path.GetDirectoryName(assetPath);
            }
        }
        else {
            assetPath = SaveFileInProject ( "Create...", "Assets/", _assetName, "asset" );
            if ( string.IsNullOrEmpty (assetPath) )
                return;
        }

        //
        bool doCreate = true;
        // DISABLE: we use AssetDatabase.GenerateUniqueAssetPath in Create instead { 
        // string path = Path.Combine( assetPath, _assetName + ".asset" );
        // FileInfo fileInfo = new FileInfo(path);
        // if ( fileInfo.Exists ) {
        //     doCreate = EditorUtility.DisplayDialog( _assetName + " already exists.",
        //                                             "Do you want to overwrite the old one?",
        //                                             "Yes", "No" );
        // }
        // } DISABLE end 
        if ( doCreate ) {
            T newAsset = Create ( assetPath, _assetName );
            Selection.activeObject = newAsset;
            // EditorGUIUtility.PingObject(border);
        }
    }
}
