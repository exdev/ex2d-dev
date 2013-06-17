// ======================================================================================
// File         : exAtlasUtility.cs
// Author       : Wu Jie 
// Last Change  : 06/18/2013 | 01:01:25 AM | Tuesday,June
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
/// exAtlas editor helper function
///
///////////////////////////////////////////////////////////////////////////////

public static class exAtlasUtility {

    public static void ImportObjects ( exAtlas _atlas, Object[] _objects ) {
        foreach ( Object o in _objects ) {
            if ( o is Texture2D ) {
                exTextureInfo textureInfo = ScriptableObject.CreateInstance<exTextureInfo>();
                textureInfo.name = o.name;
                textureInfo.texture = o as Texture2D;
                textureInfo.rawWidth = textureInfo.texture.width;
                textureInfo.rawHeight = textureInfo.texture.height;
                textureInfo.x = 0;
                textureInfo.y = 0;
                textureInfo.width = textureInfo.texture.width;
                textureInfo.height = textureInfo.texture.height;

                AssetDatabase.AddObjectToAsset(textureInfo, _atlas);
                AssetDatabase.ImportAsset( AssetDatabase.GetAssetPath(textureInfo) );
                Debug.Log(AssetDatabase.GetAssetPath(textureInfo));
            }
        }
    }
} 
