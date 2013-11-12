// ======================================================================================
// File         : exUIPanelInspector.cs
// Author       : Wu Jie 
// Last Change  : 11/01/2013 | 16:23:32 PM | Friday,November
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

///////////////////////////////////////////////////////////////////////////////
// BoardPatternInspector
///////////////////////////////////////////////////////////////////////////////

[CanEditMultipleObjects]
[CustomEditor(typeof(exUIPanel))]
class exUIPanelInspector : exUIControlInspector {
}
