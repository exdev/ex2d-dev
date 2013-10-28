// ======================================================================================
// File         : exBitmapFontUtility.cs
// Author       : Wu Jie 
// Last Change  : 07/28/2013 | 16:25:29 PM | Sunday,July
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
/// exBitmapFont helper function
///
///////////////////////////////////////////////////////////////////////////////

public static class exBitmapFontUtility {

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static string ParseValue ( string[] _words, string _key ) {
        string mykey = _key + "="; 
        for ( int i = 0; i < _words.Length; ++i ) {
            string word = _words[i];
            if ( word.Length > mykey.Length &&
                 word.Substring(0,mykey.Length) == mykey )
            {
                string txtValue = word.Substring(mykey.Length);
                if ( txtValue[0] == '"' ) {
                    if ( txtValue[txtValue.Length-1] == '"' ) {
                        return txtValue; 
                    }
                    else {
                        for ( int j = i+1; j < _words.Length; ++j ) {
                            string word2 = _words[j];
                            txtValue = txtValue + " " + word2; 
                            if ( txtValue[txtValue.Length-1] == '"' ) {
                                return txtValue; 
                            }
                        }
                    }
                }
                return txtValue;
            }
        }
        return "";
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static bool Parse ( exBitmapFont _bitmapFont, Object _fontInfo ) {
        _bitmapFont.Reset();

        string fontInfoPath = AssetDatabase.GetAssetPath(_fontInfo);
        string dirname = Path.GetDirectoryName(fontInfoPath);

		string line;
        FileInfo fileInfo = new FileInfo(fontInfoPath);
		StreamReader reader = fileInfo.OpenText();
        int textureHeight = -1;
		while ( (line = reader.ReadLine()) != null ) {

            string[] words = line.Split(' ');
            if ( words[0] == "info" ) {
                _bitmapFont.size = int.Parse ( ParseValue( words, "size" ) ); 
            }
            else if ( words[0] == "common" ) {
                _bitmapFont.lineHeight = int.Parse ( ParseValue( words, "lineHeight" ) ); 
                _bitmapFont.baseLine = int.Parse ( ParseValue( words, "base" ) ); 
                // _bitmapFont.width = int.Parse ( ParseValue( words, "scaleW" ) ); 
                // _bitmapFont.height = int.Parse ( ParseValue( words, "scaleH" ) ); 

                int pages = int.Parse( ParseValue( words, "pages" ) );
                if ( pages != 1 ) {
                    Debug.LogError ( "Parse Error: only support one page" );
                    return false;
                }
            }
            else if ( words[0] == "page" ) {
                // load texture from file
                string filename = ParseValue( words, "file" );
                filename = filename.Substring( 1, filename.Length-2 ); // remove the "" in "foobar.png"
                string texturePath = Path.Combine( dirname, filename );
                Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath( texturePath, typeof(Texture2D) );
                if ( texture == null ) {
                    Debug.LogError("Parse Failed: The texture " + filename + " not found.");
                    return false;
                }
                if ( exEditorUtility.IsValidForBitmapFont(texture) == false ) {
                    exEditorUtility.ImportTextureForBitmapFont(texture);
                }
                textureHeight = texture.height;

                // add page info 
                _bitmapFont.texture = texture;
            }
            else if ( words[0] == "char" ) {
                exBitmapFont.CharInfo charInfo = new exBitmapFont.CharInfo(); 
                charInfo.id = int.Parse ( ParseValue( words, "id" ) );
                charInfo.width = int.Parse ( ParseValue( words, "width" ) );
                charInfo.height = int.Parse ( ParseValue( words, "height" ) );
                charInfo.trim_x = int.Parse ( ParseValue( words, "x" ) );
                charInfo.trim_y = int.Parse ( ParseValue( words, "y" ) );
                charInfo.xoffset = int.Parse ( ParseValue( words, "xoffset" ) );
                charInfo.yoffset = int.Parse ( ParseValue( words, "yoffset" ) );
                charInfo.xadvance = int.Parse ( ParseValue( words, "xadvance" ) );
                charInfo.rotated = false;
                // charInfo.page = int.Parse ( ParseValue( words, "page" ) );

                // add char info
                _bitmapFont.charInfos.Add(charInfo);
            }
            else if ( words[0] == "kerning" ) {
                exBitmapFont.KerningInfo kerningInfo = new exBitmapFont.KerningInfo();
                kerningInfo.first = int.Parse ( ParseValue( words, "first" ) );
                kerningInfo.second = int.Parse ( ParseValue( words, "second" ) );
                kerningInfo.amount = int.Parse ( ParseValue( words, "amount" ) );
                _bitmapFont.kernings.Add(kerningInfo);
            }
        }
        reader.Close();
        _bitmapFont.rawFontGUID = exEditorUtility.AssetToGUID(_fontInfo);
        _bitmapFont.rawTextureGUID = exEditorUtility.AssetToGUID(_bitmapFont.texture);

        // revert charInfo uv-y to fit the Unity's uv-coordination.
        foreach ( exBitmapFont.CharInfo charInfo in _bitmapFont.charInfos ) {
            charInfo.trim_y = textureHeight - (charInfo.trim_y + charInfo.height);
            charInfo.x = charInfo.trim_x;
            charInfo.y = charInfo.trim_y;
        }

        EditorUtility.SetDirty(_bitmapFont);

        return true;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static bool IsFontInfo ( Object _obj ) {
        string fontInfoPath = AssetDatabase.GetAssetPath(_obj);
        bool isFontInfo = (Path.GetExtension(fontInfoPath) == ".txt" || 
                           Path.GetExtension(fontInfoPath) == ".fnt");
        return isFontInfo;
    } 
}
