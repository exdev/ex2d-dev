// ======================================================================================
// File         : exAtlas.cs
// Author       : Wu Jie 
// Last Change  : 02/17/2013 | 22:24:17 PM | Sunday,February
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
/// The atlas asset used in exSprite
///
///////////////////////////////////////////////////////////////////////////////

public class exAtlas : ScriptableObject {

    [System.Serializable]
    public class Element {
        public int x;
        public int y;
        public TextureInfo info;
    }

    public Element[] elements;
    public Texture2D texture;
}
