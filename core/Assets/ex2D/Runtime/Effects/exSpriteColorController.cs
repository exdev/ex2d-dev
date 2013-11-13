// ======================================================================================
// File         : exSpriteColorController.cs
// Author       : Wu Jie 
// Last Change  : 11/12/2013 | 10:11:20 AM | Tuesday,November
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

public class exSpriteColorController : MonoBehaviour {

    [System.Serializable]
    public class ColorInfo {
        public exSpriteBase sprite;
        public Color color;
    }


    [SerializeField] protected Color color_ = Color.white;
    public Color color {
        get { return color_; }
        set {
            if ( color_ != value ) {
                color_ = value;
                for ( int i = 0; i < colorInfos.Count; ++i ) {
                    ColorInfo colorInfo = colorInfos[i];
                    colorInfo.sprite.color = colorInfo.color * color_;
                }
            }
        }
    }
    public List<ColorInfo> colorInfos = new List<ColorInfo>();

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        enabled = false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void EnableSprites ( bool _enabled ) {
        for ( int i = 0; i < colorInfos.Count; ++i ) {
            ColorInfo colorInfo = colorInfos[i];
            colorInfo.sprite.enabled = _enabled;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RegisterColor ( GameObject _go ) {
        exSpriteBase[] sprites = _go.GetComponentsInChildren<exSpriteBase>();
        for ( int i = 0; i < sprites.Length; ++i ) {
            RegisterColor( sprites[i] );
        }
    }

    public void RegisterColor ( exSpriteBase _sprite ) {
        bool founded = false;
        for ( int i = 0; i < colorInfos.Count; ++i ) {
            ColorInfo colorInfo = colorInfos[i];
            if ( colorInfo.sprite == _sprite ) {
                colorInfo.color = _sprite.color;
                founded = true;
            }
        }

        if ( founded == false ) {
            ColorInfo colorInfo = new ColorInfo();
            colorInfo.sprite = _sprite;
            colorInfo.color = _sprite.color;
            colorInfos.Add(colorInfo);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void UnregisterColor ( GameObject _go ) {
        exSpriteBase[] sprites = _go.GetComponentsInChildren<exSpriteBase>();
        for ( int i = 0; i < sprites.Length; ++i ) {
            UnregisterColor( sprites[i] );
        }
    }

    public void UnregisterColor ( exSpriteBase _sprite ) {
        for ( int i = 0; i < colorInfos.Count; ++i ) {
            if ( colorInfos[i].sprite == _sprite ) {
                colorInfos.RemoveAt(i);
                break;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RegisterAllChildren () {
        colorInfos.Clear();
        exSpriteBase[] sprites = GetComponentsInChildren<exSpriteBase>();
        for ( int i = 0; i < sprites.Length; ++i ) {
            ColorInfo colorInfo = new ColorInfo();
            exSpriteBase sprite = sprites[i];
            colorInfo.sprite = sprite;
            colorInfo.color = sprite.color;
            colorInfos.Add(colorInfo);
        }
    } 
}
