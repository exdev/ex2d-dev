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
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
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
/// debug utilities
///
///////////////////////////////////////////////////////////////////////////////

public static class exDebug {
    
    // ------------------------------------------------------------------ 
    /// Only used and compiled for debugging
    /// 所有方法调用，及用作调用参数的表达式都不会被编译进*非*EX_DEBUG的版本
    // ------------------------------------------------------------------ 
    
    [/*Diagnostics.Conditional("UNITY_EDITOR"), */Diagnostics.Conditional("EX_DEBUG")]
    public static void Assert (bool _test, string _msg = "", bool _logError = true) {
        if (!_test) {
            if (_logError) {
                Debug.LogError("Assert Failed! " + _msg);
            }
            else {
                Debug.LogWarning("Assert Failed! " + _msg);
            }
        }
    }

#if UNITY_EDITOR

    // ------------------------------------------------------------------ 
    /// 在Editor中访问当前平台的EX_DEBUG宏定义，修改它将触发脚本重新编译
    // ------------------------------------------------------------------ 

    public static bool enabled {
        get {
            string defs = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            return ArrayUtility.Contains(defs.Split(';'), "EX_DEBUG");
        }
        set {
            if (EditorApplication.isPlaying) {
                Debug.LogError("can't toggle debugging mode when editor is playing");
                return;
            }
            if (enabled != value) {
                string defs = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
                var defsAry = defs.Split(';');
                if (value) {
                    ArrayUtility.Add(ref defsAry, "EX_DEBUG");
                }
                else {
                    ArrayUtility.Remove(ref defsAry, "EX_DEBUG");
                }
                defs = string.Join(";", defsAry);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defs);
            }
        }
    }

#endif
}

namespace UnityEngine {

    ///////////////////////////////////////////////////////////////////////////////
    ///
    /// The extension methods
    ///
    ///////////////////////////////////////////////////////////////////////////////

    internal static partial class UnityEngineExtends {
    
        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 
    
        public static T Instantiate<T>(this T original) where T : Object {
            return Object.Instantiate(original) as T;
        }
    
        // ------------------------------------------------------------------ 
        // Desc: 
        // ------------------------------------------------------------------ 
    
        public static T Instantiate<T>(this T original, Vector3 position, Quaternion rotation) where T : Object {
            return Object.Instantiate(original, position, rotation) as T;
        }
    }

}

///////////////////////////////////////////////////////////////////////////////
///
/// Simple pair used to combine ref type keys
///
///////////////////////////////////////////////////////////////////////////////

public struct Pair : IComparable<Pair>, IEquatable<Pair> {

    public object first;
    public object second;

    public Pair (object _first, object _second) {
        this.first = _first;
        this.second = _second;
    }
    public bool Equals (Pair _other) {
        return object.ReferenceEquals(first, _other.first) && object.ReferenceEquals(second, _other.second);
    }
    public override int GetHashCode () {
        int firstHashCode, secondHashCode;
        if (first != null) {
            firstHashCode = first.GetHashCode();
        }
        else {
            firstHashCode = 0x00000000;
        }
        if (second != null) {
            secondHashCode = second.GetHashCode() * 1313;
        }
        else {
            secondHashCode = 0x00000000;
        }
        return firstHashCode ^ secondHashCode;
    }
    public int CompareTo(Pair _other) {
        int secondCompare = second.GetHashCode().CompareTo(_other.second.GetHashCode());
        if (secondCompare == 0) {
            return first.GetHashCode().CompareTo(_other.first.GetHashCode());
        }
        else{
            return secondCompare;
        }
    }
}