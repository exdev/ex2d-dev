// ======================================================================================
// File         : exRichText.cs
// Author       : Jare
// Last Change  : 03/23/2014
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
/// 
/// A component to render rich text in the scene
/// See the Unity Rich Text page: http://docs.unity3d.com/Documentation/Manual/StyledText.html
/// 
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/Rich Text")]
public class exRichText : exSpriteFont {

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    /// <summary> 保存生成的sprite </summary>
    private List<exSpriteFont> spriteFontList;
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected new void OnEnable () {
        Debug.Log("[OnEnable|exRichText] ");
        exSpriteFont[] spriteList = GetComponentsInChildren<exSpriteFont>(true);
        foreach (exSpriteFont sprite in spriteList) {
            Debug.Log(string.Format("[OnEnable|exRichText] sprite: {0}", sprite.name));
        }
        base.OnEnable();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected new void OnDisable () {
        Debug.Log("[OnDisable|exRichText] ");
        base.OnDisable();
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    protected override Vector3[] GetVertices (Space _space) {
        return exUtility<Vector3>.emptyArray;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Other Functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override void SetFont (exBitmapFont _bitmapFont) {
        for (int i = 0, iMax = spriteFontList.Count; i < iMax; ++i) {
            exSpriteFont spriteFont = spriteFontList[i];
            if (spriteFont != null) {
                spriteFont.SetFont(_bitmapFont);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public override void SetFont (Font _dynamicFont) {
        updateFlags |= exUpdateFlags.Text;
    }
}
//#endif
