﻿// ======================================================================================
// File         : exUILayout.cs
// Author       : Wu Jie 
// Last Change  : 10/02/2013 | 16:50:32 PM | Wednesday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
///
/// The ui-layout component
///
///////////////////////////////////////////////////////////////////////////////

public class exUILayout : MonoBehaviour {

    public static exTextureInfo whiteTexture = null;

    public exUILayoutInfo layoutInfo;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void Sync () {
        if ( whiteTexture == null ) {
            whiteTexture = Resources.Load ( "TextureInfos/WhiteTexture_2x2", typeof(exTextureInfo) ) as exTextureInfo;
        }

        layoutInfo.Apply();

        SyncElements ( transform, 0, layoutInfo.root, 0, 0, 0 );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SyncElements ( Transform _transParent, int _idx, exUIElement _el, int _x, int _y, int _depth ) {

        string childName =  "[" + _idx + "]" + _el.name;
        GameObject go = FindOrNewCihld( _transParent, childName);
        Transform trans = go.transform;

        int x = _x + _el.x - _el.paddingLeft - _el.borderSizeLeft;
        int y = -(_y + _el.y - _el.paddingTop - _el.borderSizeTop);

        //
        trans.localPosition = new Vector3 ( x, y, 0.0f );

        // this is a content root
        if ( _el.display == exCSS_display.Inline && _el.isContent == false ) 
            return;

        // process current element
        if ( _el.borderColor.a > 0.0f &&
             ( _el.borderSizeLeft > 0 || _el.borderSizeRight > 0 || _el.borderSizeTop > 0 || _el.borderSizeBottom > 0 ) ) 
        {
            int width = _el.width 
                + _el.borderSizeLeft + _el.borderSizeRight +
                + _el.paddingLeft + _el.paddingRight;
            int height = _el.height 
                + _el.borderSizeTop + _el.borderSizeBottom +
                + _el.paddingTop + _el.paddingBottom;

            bool borderOnly = false;
            exTextureInfo borderImage = _el.borderImage as exTextureInfo;
            if ( _el.borderImage == null ) {
                borderImage = whiteTexture;
                borderOnly = true;
            }

            exSprite sprite 
                = ex2D.Detail.exSpriteUtility.NewSlicedSprite( go, borderImage, 
                                                               _el.borderSizeLeft, _el.borderSizeRight, _el.borderSizeTop, _el.borderSizeBottom,
                                                               width, height, _el.borderColor, 
                                                               borderOnly );
            sprite.anchor = Anchor.TopLeft;
            sprite.depth = _depth;
        }

        // process background
        if ( _el.backgroundColor.a > 0.0f ) {
            exTextureInfo backgroundImage = _el.backgroundImage as exTextureInfo;
            if ( backgroundImage == null ) {
                backgroundImage = whiteTexture;
            }
            GameObject backgroundGO = FindOrNewCihld ( trans, _el.name + " background" );
            exSprite sprite 
                = ex2D.Detail.exSpriteUtility.NewSimpleSprite( backgroundGO, backgroundImage, 
                                                               _el.width + _el.paddingLeft + _el.paddingRight, 
                                                               _el.height + _el.paddingTop + _el.paddingBottom, 
                                                               _el.backgroundColor );
            backgroundGO.transform.localPosition = new Vector3 ( _el.borderSizeLeft, -_el.borderSizeTop, 0.0f );
            sprite.anchor = Anchor.TopLeft;
            sprite.depth = _depth;
        }

        // process content or children
        if ( _el.isContent ) {
            switch ( _el.contentType ) {
            case exUIElement.ContentType.Text:
                if ( _el.font != null ) {
                    exSpriteFont spriteFont = null;
                    exBitmapFont bitmapFont = _el.font as exBitmapFont;
                    if ( bitmapFont != null ) {
                        spriteFont = ex2D.Detail.exSpriteUtility.NewSpriteFont( go, bitmapFont, _el.contentColor, _el.text );
                    }
                    else {
                        Font font = _el.font as Font;
                        if ( font != null ) {
                            spriteFont = ex2D.Detail.exSpriteUtility.NewSpriteFont( go, font, _el.fontSize, _el.contentColor, _el.text );
                        }
                    }
                    spriteFont.anchor = Anchor.TopLeft;
                }
                break;


            case exUIElement.ContentType.TextureInfo:
                // exEditorUtility.GUI_DrawTextureInfo ( new Rect( element_x, element_y, _el.width, _el.height ),
                //                                       _el.image as exTextureInfo,
                //                                       _el.contentColor );
                break;
            }
        }
        else {
            // sync children
            for ( int i = 0; i < _el.normalFlows.Count; ++i ) {
                exUIElement childEL = _el.normalFlows[i];

                if ( childEL.IsEmpty() )
                    continue;

                SyncElements ( trans, i, childEL, _el.borderSizeLeft + _el.paddingLeft, _el.borderSizeTop + _el.paddingTop, _depth + 1 );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    GameObject FindOrNewCihld ( Transform _parent, string _name ) {
        GameObject newGO = null;
        Transform trans = _parent.Find(_name);
        if ( trans == null ) {
            newGO = new GameObject(_name);
            trans = newGO.transform;
            trans.parent = _parent;
        }
        else {
            newGO = trans.gameObject;
        }

        return newGO;
    }
}
