// ======================================================================================
// File         : exUIButtonInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/16/2013 | 13:39:33 PM | Wednesday,October
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
[CustomEditor(typeof(exUIButton))]
class exUIButtonInspector : exUIControlInspector {
}
