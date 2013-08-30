// ======================================================================================
// File         : exUIElement.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 22:52:19 PM | Friday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
///
/// The ui-layout element
///
///////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public class exUIElement {
    public string name = "";
    public string id = ""; // for css
    public string content = "";

    public Vector3 position;
    public Vector3 size;

    public exUIElement parent;
    public List<exUIElement> children;

}


