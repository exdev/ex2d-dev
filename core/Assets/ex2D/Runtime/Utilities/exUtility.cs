// ======================================================================================
// File         : exUtilities.cs
// Author       : 
// Last Change  : 08/17/2013 | 15:27:56
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
using System.Collections.Generic;
using Diagnostics = System.Diagnostics;

///////////////////////////////////////////////////////////////////////////////
//
/// The utilities
//
///////////////////////////////////////////////////////////////////////////////

public struct exUtility {

    public static void Swap<T> (ref T lhs, ref T rhs) {
        T tmp = lhs;
        lhs = rhs;
        rhs = tmp;
    }
}

///////////////////////////////////////////////////////////////////////////////
//
/// Define specific HideFlags for debugging
//
///////////////////////////////////////////////////////////////////////////////

internal class exReleaseFlags
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
    public static void Assert (bool _test, string _msg = "", bool _logError = true, UnityEngine.Object _context = null) {
        if (!_test) {
            if (_logError) {
                Debug.LogError("Assert Failed! " + _msg, _context);
            }
            else {
                Debug.LogWarning("Assert Failed! " + _msg, _context);
            }
        }
    }

#if UNITY_EDITOR

    // ------------------------------------------------------------------ 
    /// 在Editor中访问当前平台的EX_DEBUG宏定义，修改它将触发脚本重新编译
    // ------------------------------------------------------------------ 

    public static bool enabled {
        get {
            string defs = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.activeBuildTarget.ToBuildTargetGroup());
            return ArrayUtility.Contains(defs.Split(';'), "EX_DEBUG");
        }
        set {
            if (EditorApplication.isPlaying) {
                Debug.LogError("can't toggle debugging mode when editor is playing");
                return;
            }
            if (enabled != value) {
                BuildTargetGroup target = EditorUserBuildSettings.activeBuildTarget.ToBuildTargetGroup();
                string defs = PlayerSettings.GetScriptingDefineSymbolsForGroup(target);
                var defsAry = defs.Split(';');
                if (value) {
                    ArrayUtility.Add(ref defsAry, "EX_DEBUG");
                }
                else {
                    ArrayUtility.Remove(ref defsAry, "EX_DEBUG");
                }
                defs = string.Join(";", defsAry);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defs);
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    /// Get the corresponding BuildTargetGroup via BuildTarget
    // ------------------------------------------------------------------ 

    public static BuildTargetGroup ToBuildTargetGroup (this BuildTarget buildTarget) {
        switch (buildTarget) {
        case BuildTarget.StandaloneOSXUniversal:
        // case BuildTarget.StandaloneOSXPPC:
        case BuildTarget.StandaloneOSXIntel:
        case BuildTarget.StandaloneWindows:
        case BuildTarget.StandaloneLinux:
        case BuildTarget.StandaloneWindows64:
        // case BuildTarget.MetroPlayerX86:
        // case BuildTarget.MetroPlayerX64:
        // case BuildTarget.MetroPlayerARM:
        case BuildTarget.StandaloneLinux64:
        case BuildTarget.StandaloneLinuxUniversal:
            return BuildTargetGroup.Standalone;
        case BuildTarget.WebPlayer:
        case BuildTarget.WebPlayerStreamed:
            return BuildTargetGroup.WebPlayer;
        // case BuildTarget.Wii:
        //     return BuildTargetGroup.Wii;
        case BuildTarget.iPhone:
            return BuildTargetGroup.iPhone;
        case BuildTarget.PS3:
            return BuildTargetGroup.PS3;
        case BuildTarget.XBOX360:
            return BuildTargetGroup.XBOX360;
        case BuildTarget.Android:
            return BuildTargetGroup.Android;
        case BuildTarget.StandaloneGLESEmu:
            return BuildTargetGroup.GLESEmu;
        case BuildTarget.NaCl:
            return BuildTargetGroup.NaCl;
        case BuildTarget.FlashPlayer:
            return BuildTargetGroup.FlashPlayer;
        default:
            return BuildTargetGroup.Unknown;
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

    public static partial class UnityEngineExtends {
    
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

        #region Destroy

        // ------------------------------------------------------------------ 
        /// If in edit mode, destory immediatelly
        // ------------------------------------------------------------------ 

        public static void Destroy(this GameObject obj) {
            obj.transform.parent = null;
#           if UNITY_EDITOR
                if (EditorApplication.isPlaying) {
                    Object.Destroy(obj);
                }
                else {
                    Object.DestroyImmediate(obj, false);
                }
#           else
                Object.Destroy(obj);
#           endif
        }
        public static void Destroy(this Object obj) {
#           if UNITY_EDITOR
                if (EditorApplication.isPlaying) {
                    Object.Destroy(obj);
                }
                else {
                    Object.DestroyImmediate(obj, false);
                }
#           else
                Object.Destroy(obj);
#           endif
        }
        public static void Destroy(this GameObject obj, float waitTime) {
            obj.transform.parent = null;
            ((Object)obj).Destroy(waitTime);
        }
        public static void Destroy(this Object obj, float waitTime) {
#           if UNITY_EDITOR
                if (EditorApplication.isPlaying) {
                    Object.Destroy(obj, waitTime);
                }
                else {
                    Object.DestroyImmediate(obj, false);
                }
#           else
                Object.Destroy(obj, waitTime);
#           endif
        }
        public static void DestroyImmediate(this GameObject obj) {
            obj.transform.parent = null;
/*
#           if UNITY_EDITOR
                if (UnityEditor.Selection.activeGameObject == obj) {
                    UnityEditor.Selection.activeTransform = null;
                    int index = UnityEditor.ArrayUtility.IndexOf(UnityEditor.Selection.transforms, obj.transform);
                    if (index != -1) {
                        UnityEditor.Selection.transforms[index] = null;
                        Debug.Log("[DestroyImmediate|UnityEngineExtends] ");
                    }
                }
                obj.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
                UnityEditor.EditorUtility.SetDirty(obj);
#           endif
*/
            Object.DestroyImmediate(obj); 
        }
        public static void DestroyImmediate(this GameObject obj, bool allowDestroyingAssets) {
            obj.transform.parent = null;
            Object.DestroyImmediate(obj, allowDestroyingAssets);
        }
        public static void DestroyImmediate(this Object obj) {
            Object.DestroyImmediate(obj);
        }
        public static void DestroyImmediate(this Object obj, bool allowDestroyingAssets) {
            Object.DestroyImmediate(obj, allowDestroyingAssets);
        }

        #endregion

        // ------------------------------------------------------------------ 
        /// Set the global scale of the object
        /// lossyScale is a convenience property that attempts to match the actual world scale as much as it can. 
        /// If your objects are not skewed the value will be completely correct and 
        /// most likely the value will not be very different if it contains skew too
        // ------------------------------------------------------------------ 
    
        public static void SetLossyScale(this Transform trans, Vector3 worldScale) {
            Vector3 oldWorldScale = trans.lossyScale;
            Vector3 localScale = trans.localScale;
            trans.localScale = new Vector3( worldScale.x / oldWorldScale.x * localScale.x,
                                            worldScale.y / oldWorldScale.y * localScale.y,
                                            worldScale.z / oldWorldScale.z * localScale.z );
        }

        // ------------------------------------------------------------------ 
        /// Returns the component of Type in any of the GameObject's parents.
        // ------------------------------------------------------------------ 

        public static T GetComponentUpwards<T> (this GameObject _go) where T : Component {
            Transform parentTransform = _go.transform.parent;
            while (parentTransform != null) {
                T component = parentTransform.GetComponent(typeof(T)) as T;
                if (component != null) {
                    return component;
                }
                parentTransform = parentTransform.parent;
            }
            return null;
        }
        public static T FindParentComponent<T> (this T component) where T : Component {
            return component.gameObject.GetComponentUpwards<T>();
        }

        // ------------------------------------------------------------------ 
        /// Use this method instead of GetComponentInChildren that may alloc lots of GC
        // ------------------------------------------------------------------ 

        public static T GetComponentInChildrenFast<T>(this GameObject go) where T : Component {
            if (go.activeInHierarchy) {
                Component component = go.GetComponent(typeof(T));
                if (component != null) {
                    return component as T;
                }
            }
            Transform transform = go.transform;
            if (transform != null) {
                int childCount = transform.childCount;
                for (int i = 0; i < childCount; ++i) {
                    Transform child = transform.GetChild(i);
                    T componentInChildren = child.gameObject.GetComponentInChildrenFast<T>();
                    if (componentInChildren != null) {
                        return componentInChildren;
                    }
                }
            }
            return null;
        }
        public static T GetComponentInChildrenFast<T> (this Component component) where T : Component {
            return component.gameObject.GetComponentInChildrenFast<T>();
        }
    }
}

/*
///////////////////////////////////////////////////////////////////////////////
//
//
//
///////////////////////////////////////////////////////////////////////////////

public struct exRect {
    public int x;
    public int y;
    public int width;
    public int height;

    public exRect (int _x, int _y, int _width, int _height) {
        x = _x; y = _y; width = _width; height = _height;
    }
}
*/

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

public struct MaterialTableKey { /*: System.IComparable<MaterialTableKey>, System.IEquatable<MaterialTableKey>*/ 

    // ------------------------------------------------------------------ 
    /// In Xamarin.iOS, if MaterialTableKey is a type as dictionary keys, we should manually implement its IEqualityComparer,
    /// and provide an instance to the Dictionary<TKey, TValue>(IEqualityComparer<TKey>) constructor.
    /// See http://docs.xamarin.com/guides/ios/advanced_topics/limitations for more info.
    // ------------------------------------------------------------------ 

    public class Comparer : IEqualityComparer<MaterialTableKey> {
        static Comparer instance_;
        public static Comparer instance {
            get {
                if (instance_ == null) {
                    instance_ = new Comparer();
                }
                return instance_;
            }
        }
        public bool Equals (MaterialTableKey _lhs, MaterialTableKey _rhs) {
            return object.ReferenceEquals(_lhs.shader, _rhs.shader) && object.ReferenceEquals(_lhs.texture, _rhs.texture);
        }
        public int GetHashCode(MaterialTableKey _obj) {
            int shaderHashCode, texHashCode;
            if (_obj.shader != null) {
                shaderHashCode = _obj.shader.GetHashCode();
            }
            else {
                shaderHashCode = 0x00000000;
            }
            if (_obj.texture != null) {
                texHashCode = _obj.texture.GetHashCode() * 1313;
            }
            else {
                texHashCode = 0x00000000;
            }
            return shaderHashCode ^ texHashCode;
        }
    } 

    public Shader shader;
    public Texture texture;

    public MaterialTableKey (Shader _shader, Texture _texture) {
        shader = _shader;
        texture = _texture;
    }
    public MaterialTableKey (Material _material) 
        : this(_material.shader, _material.mainTexture) {
    }
    //        public bool Equals (MaterialTableKey _other) {
    //            return Comparer.instance.Equals(this, _other);
    //        }
    //        public override int GetHashCode () {
    //            return Comparer.instance.GetHashCode(this);
    //        }
    //        public int CompareTo(MaterialTableKey _other) {
    //            int texCompare = texture.GetHashCode().CompareTo(_other.texture.GetHashCode());
    //            if (texCompare == 0) {
    //                return shader.GetHashCode().CompareTo(_other.shader.GetHashCode());
    //            }
    //            else {
    //                return texCompare;
    //            }
    //        }
}
