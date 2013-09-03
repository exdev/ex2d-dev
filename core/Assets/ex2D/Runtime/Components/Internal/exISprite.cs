// ======================================================================================
// File         : exISprite.cs
// Author       : 
// Last Change  : 09/03/2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
//
// Helper interfaces
//
///////////////////////////////////////////////////////////////////////////////

public interface IMonoBehaviour {
    Transform transform { get; }
    GameObject gameObject { get; }
}

public interface exISpriteBase : IMonoBehaviour {
    bool customSize { get; set; }
    float width { get; set; }
    float height { get; set; }
    Anchor anchor { get; set; }
    Color color { get; set; }
    Vector2 offset { get; set; }
    Vector2 shear { get; set; }
    Shader shader { get; set; }
    int vertexCount { get; }
    int indexCount { get; }
    Material material { get; }
    bool visible { get; }
}

///////////////////////////////////////////////////////////////////////////////
//
// Interface for exSprite and ex3DSprite
//
///////////////////////////////////////////////////////////////////////////////

public interface exISprite : exISpriteBase {
    exTextureInfo textureInfo { get; set; }
    bool useTextureOffset { get; set; }
    exSpriteType spriteType { get; set; }
}
