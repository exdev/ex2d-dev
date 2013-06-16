// ======================================================================================
// File         : exUtilities.cs
// Author       : Jare
// Last Change  : 06/16/2013 | 01:52:42
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using Diagnostics = System.Diagnostics;

///////////////////////////////////////////////////////////////////////////////
//
/// Define specific HideFlags for debugging
//
///////////////////////////////////////////////////////////////////////////////

internal class exReleaseFlag
{
#if EX_DEBUG
    public const HideFlags hideAndDontSave = HideFlags.DontSave;
    public const HideFlags notEditable = (HideFlags)0;
#else
    public const HideFlags hideAndDontSave = HideFlags.HideAndDontSave;
    public const HideFlags notEditable = HideFlags.NotEditable;
#endif
}

///////////////////////////////////////////////////////////////////////////////
///
/// Only used and compiled for debugging
/// 所有方法调用，及用作调用参数的表达式都不会被编译进*非*EX_DEBUG的导出版本
///
///////////////////////////////////////////////////////////////////////////////

public static class exAssert {

    [Diagnostics.Conditional("UNITY_EDITOR"), Diagnostics.Conditional("EX_DEBUG")]
    public static void True(bool test, string msg = "", bool logError = true) {
        if(!test) {
            if (logError) {
                Debug.LogError("Asset Failed! " + msg);
            }
            else {
                Debug.LogWarning("Asset Failed! " + msg);
            }
        }
    }

    [Diagnostics.Conditional("UNITY_EDITOR"), Diagnostics.Conditional("EX_DEBUG")]
    public static void False(bool test, string msg = "", bool logError = true) {
        True(!test, msg, logError);
    }
}