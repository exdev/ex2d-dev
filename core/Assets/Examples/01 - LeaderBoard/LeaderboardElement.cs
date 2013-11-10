// ======================================================================================
// File         : LeaderboardElement.cs
// Author       : Wu Jie 
// Last Change  : 11/04/2013 | 21:55:46 PM | Monday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
// \class 
// 
// \brief 
// 
///////////////////////////////////////////////////////////////////////////////

public class LeaderboardElement : MonoBehaviour {

    exSprite headIcon;
    exSpriteFont userName;
    exSpriteFont score;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        headIcon = transform.Find("head_icon").GetComponent<exSprite>();
        userName = transform.Find("name").GetComponent<exSpriteFont>();
        score = transform.Find("score").GetComponent<exSpriteFont>();
    }
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Init ( Texture2D _icon, string _name, int _score ) {
        headIcon.textureInfo = exTextureInfo.Create(_icon);
        userName.text = _name;
        score.text = _score.ToString("0,000,000");
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Init ( string _url, string _name, int _score ) {
        StartCoroutine ( Init_CO (_url, _name, _score) );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    IEnumerator Init_CO ( string _url, string _name, int _score ) {
        WWW www = new WWW(_url);
        yield return www;

        Texture2D textureIcon = new Texture2D(80,80);
        www.LoadImageIntoTexture(textureIcon);

        Init( textureIcon, _name, _score );
    }
}
