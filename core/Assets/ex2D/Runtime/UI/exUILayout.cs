// ======================================================================================
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

[RequireComponent (typeof(exUIControl))]
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

        SyncElements ( transform, 0, layoutInfo.root, transform.position.x, transform.position.y, 0 );

        // add ui-control at the root
        exUIControl uiControl = GetComponent<exUIControl>();
        if ( uiControl == null )
            uiControl = gameObject.AddComponent<exUIControl>();
        uiControl.width = layoutInfo.width;
        uiControl.height = layoutInfo.height;
        uiControl.anchor = Anchor.TopLeft;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Transform SyncElements ( Transform _transParent, int _idx, exUIElement _el, float _x, float _y, int _depth ) {

        string childName = _el.GetName(_idx);
        GameObject go = FindOrNewCihld( _transParent, childName);
        Transform trans = go.transform;
        float x = _x + (_el.x - _el.paddingLeft - _el.borderSizeLeft);
        float y = _y - (_el.y - _el.paddingTop - _el.borderSizeTop);

        // set position
        trans.position = new Vector3 ( x, y, _transParent.position.z );

        // this is a content root
        if ( _el.display == exCSS_display.Inline && _el.isContent == false ) 
            return trans;

        // process current element
        if ( _el.borderColor.a > 0.0f &&
             ( _el.borderSizeLeft > 0 || _el.borderSizeRight > 0 || _el.borderSizeTop > 0 || _el.borderSizeBottom > 0 ) ) 
        {
            GameObject borderGO = FindOrNewCihld ( trans, "__border" );
            borderGO.transform.position = new Vector3 ( x, y, 0.0f );

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
                = ex2D.Detail.exSpriteUtility.NewSlicedSprite( borderGO, borderImage, 
                                                               _el.borderSizeLeft, _el.borderSizeRight, _el.borderSizeTop, _el.borderSizeBottom,
                                                               width, height, _el.borderColor, 
                                                               borderOnly );
            sprite.anchor = Anchor.TopLeft;
            sprite.depth = _depth;
        }
        else {
            Transform borderTrans = trans.Find("__border");
            if ( borderTrans != null ) {
                UnityEngineExtends.Destroy(borderTrans.gameObject);
            }
        }

        // process background
        if ( _el.backgroundColor.a > 0.0f ) {
            GameObject backgroundGO = FindOrNewCihld ( trans, "__background" );
            backgroundGO.transform.position = new Vector3 ( x + _el.borderSizeLeft, y - _el.borderSizeTop, 0.0f );

            exTextureInfo backgroundImage = _el.backgroundImage as exTextureInfo;
            if ( backgroundImage == null ) {
                backgroundImage = whiteTexture;
            }
            exSprite sprite 
                = ex2D.Detail.exSpriteUtility.NewSimpleSprite( backgroundGO, backgroundImage, 
                                                               _el.width + _el.paddingLeft + _el.paddingRight, 
                                                               _el.height + _el.paddingTop + _el.paddingBottom, 
                                                               _el.backgroundColor );
            sprite.anchor = Anchor.TopLeft;
            sprite.depth = _depth + 0.1f;
        }
        else {
            Transform backgroundTrans = trans.Find("__background");
            if ( backgroundTrans != null ) {
                UnityEngineExtends.Destroy(backgroundTrans.gameObject);
            }
        }

        // process content or children
        if ( _el.isContent ) {
            GameObject contentGO = FindOrNewCihld ( trans, "__content" );
            contentGO.transform.position = new Vector3 ( x + _el.borderSizeLeft, y - _el.borderSizeTop, 0.0f );

            switch ( _el.contentType ) {
            case exUIElement.ContentType.Text:
                if ( _el.font != null ) {
                    exSpriteFont spriteFont = null;
                    exBitmapFont bitmapFont = _el.font as exBitmapFont;
                    if ( bitmapFont != null ) {
                        spriteFont = ex2D.Detail.exSpriteUtility.NewSpriteFont( contentGO, bitmapFont, _el.contentColor, _el.text );
                    }
                    else {
                        Font font = _el.font as Font;
                        if ( font != null ) {
                            spriteFont = ex2D.Detail.exSpriteUtility.NewSpriteFont( contentGO, font, _el.fontSize, _el.contentColor, _el.text );
                        }
                    }

                    if ( spriteFont != null ) {
                        spriteFont.anchor = Anchor.TopLeft;
                        spriteFont.depth = _depth + 0.2f;
                    }
                }
                break;


            case exUIElement.ContentType.TextureInfo:
                if ( _el.image != null ) {
                    exTextureInfo image = _el.image as exTextureInfo;
                    exSprite sprite = null;
                    if ( image != null ) {
                        sprite = ex2D.Detail.exSpriteUtility.NewSimpleSprite( contentGO, image, _el.width, _el.height, _el.contentColor );
                    }

                    if ( sprite != null ) {
                        sprite.anchor = Anchor.TopLeft;
                        sprite.depth = _depth + 0.2f;
                    }

                    contentGO.transform.position = new Vector3 ( x + _el.borderSizeLeft, y - _el.borderSizeTop, 0.0f );
                }
                break;
            }
        }
        // sync children
        else {
            //
            Transform contentTrans = trans.Find("__content");
            if ( contentTrans != null ) {
                UnityEngineExtends.Destroy(contentTrans.gameObject);
            }

            //
            Transform owner = null;
            int idx = 0;
            for ( int i = 0; i < _el.normalFlows.Count; ++i ) {
                exUIElement childEL = _el.normalFlows[i];

                if ( childEL.display == exCSS_display.Inline && childEL.IsEmptyContent() )
                    continue;

                if ( childEL.display == exCSS_display.Inline ) {
                    if ( childEL.isContent == false ) {
                        owner = trans;
                    }
                }
                else {
                    owner = trans;
                }

                Transform newTrans = SyncElements ( owner, idx, childEL, x + (_el.borderSizeLeft + _el.paddingLeft), y - (_el.borderSizeTop + _el.paddingTop), _depth+1 );

                if ( childEL.display == exCSS_display.Inline && childEL.isContent == false ) {
                    owner = newTrans;

                    // remove unused lines (gameObject)
                    if ( newTrans.childCount > childEL.normalFlows.Count ) {
                        for ( int j = newTrans.childCount-1; j >= childEL.normalFlows.Count; --j ) {
                            Transform unusedChild = newTrans.GetChild(j);
                            UnityEngineExtends.Destroy(unusedChild.gameObject);
                        }
                    }
                }

                if ( childEL.isContent == false )
                    ++idx;
            }

            // check if we have unused children
            int searchStart = _el.children.Count-1;
            bool hasInlineContent = (_el.IsEmptyContent() == false);

            for ( int i = trans.childCount-1; i >= 0; --i ) {
                Transform childTrans = trans.GetChild(i);
                string name = childTrans.name;
                if ( name == "__background" ||
                     name == "__border" ||
                     name == "__content" ||
                     name == "[0]__inline_content"
                     /*name.IndexOf("__inline_content") != -1*/ )
                    continue;

                // 
                bool found = false;
                for ( int j = searchStart; j >= 0; --j ) {
                    exUIElement childEL = _el.children[j];
                    // NOTE: the j index sometimes different when we have __inline_content
                    if ( name == childEL.GetName( hasInlineContent ? j+1 : j ) ) {
                        searchStart -= 1;
                        found = true;
                        break;
                    }
                }

                if ( found == false )
                    UnityEngineExtends.Destroy(childTrans.gameObject);
            }
        }

        return null;
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
