// ======================================================================================
// File         : exUIEffectInspector.cs
// Author       : Wu Jie 
// Last Change  : 10/23/2013 | 17:05:59 PM | Wednesday,October
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
[CustomEditor(typeof(exUIEffect))]
class exUIEffectInspector : Editor {

    public class Styles {
        public GUIStyle toolbarDropDown = "TE ToolbarDropDown";
        public Texture iconToolbarPlus = EditorGUIUtility.FindTexture ("Toolbar Plus");
        public Texture iconToolbarMinus = EditorGUIUtility.FindTexture("Toolbar Minus");
    }
    protected static Styles styles = null;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	public override void OnInspectorGUI () {
        // if settingsStyles is null
        if ( styles == null ) {
            styles = new Styles();
        }

        if ( serializedObject.isEditingMultipleObjects == false ) {
            exUIEffect uiEffect = target as exUIEffect;

            EditorGUILayout.Space ();
            ScaleInfoField ( uiEffect.scaleInfos );

            EditorGUILayout.Space ();
            ColorInfoField ( uiEffect.colorInfos );

            EditorGUILayout.Space ();
            OffsetInfoField ( uiEffect.offsetInfos );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void ScaleInfoField ( List<EffectInfo_Scale> _infos ) {
		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);

            GUILayout.BeginVertical();
                // name
                GUILayout.Toggle( true, "Scale", "dragtab");

                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
                GUILayout.BeginVertical();

                    // infos
                    for ( int i = 0; i < _infos.Count; ++i ) {
                        bool delete = false;
                        EffectInfo_Scale info = _infos[i];

                        // target
                        EditorGUILayout.BeginHorizontal();

                            // receiver
                            EditorGUI.BeginChangeCheck();
                            info.target = EditorGUILayout.ObjectField( info.target, typeof(Transform), true ) as Transform;
                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }

                            //
                            if ( info.target != null ) {
                            }
                            else {
                                delete = true;
                            }

                            // Delete
                            if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                                delete = true;
                            }
                            GUILayout.Space(3f);

                        EditorGUILayout.EndHorizontal();

                        // curve, duration
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                GUILayout.Label("Custom Curve");
                                info.customCurve = EditorGUILayout.Toggle(info.customCurve,GUILayout.Width(15));
                                if ( info.customCurve ) {
                                    info.curve = EditorGUILayout.CurveField(info.curve);
                                }
                                else {
                                    info.curveType = (exEase.Type)EditorGUILayout.EnumPopup(info.curveType);
                                }
                                info.duration = EditorGUILayout.FloatField(info.duration);

                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        float x = 0.0f;
                        float y = 0.0f;
                        float z = 0.0f;

                        // deactive
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasDeactive = EditorGUILayout.Toggle(info.hasDeactive,GUILayout.Width(15));
                                GUILayout.Label("Deactive");
                                GUI.enabled = info.hasDeactive;

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.deactive.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.deactive.y,GUILayout.Width(30));
                                GUILayout.Label("Z", GUILayout.Width(10));
                                z = EditorGUILayout.FloatField("",info.deactive.z,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.deactive = new Vector3( x, y, z );
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        // press
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasPress = EditorGUILayout.Toggle(info.hasPress,GUILayout.Width(15));
                                GUILayout.Label("Press");
                                GUI.enabled = info.hasPress;

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.press.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.press.y,GUILayout.Width(30));
                                GUILayout.Label("Z", GUILayout.Width(10));
                                z = EditorGUILayout.FloatField("",info.press.z,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.press = new Vector3( x, y, z );
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        // hover
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasHover = EditorGUILayout.Toggle(info.hasHover,GUILayout.Width(15));
                                GUILayout.Label("Hover");
                                GUI.enabled = info.hasHover;

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.hover.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.hover.y,GUILayout.Width(30));
                                GUILayout.Label("Z", GUILayout.Width(10));
                                z = EditorGUILayout.FloatField("",info.hover.z,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.hover = new Vector3( x, y, z );
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        //
                        if ( delete ) {
                            _infos.RemoveAt(i);
                            --i;
                            EditorUtility.SetDirty(target);
                        }
                    }

                    // new slot
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        Transform receiver = EditorGUILayout.ObjectField( null, typeof(Transform), true, GUILayout.Width(150) ) as Transform;
                        if ( receiver != null ) {

                            EffectInfo_Scale info = new EffectInfo_Scale();
                            info.target = receiver;

                            _infos.Add(info);
                            EditorUtility.SetDirty(target);
                        }
                        GUILayout.Label( styles.iconToolbarPlus, GUILayout.Width(20) );
                    EditorGUILayout.EndHorizontal();

                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();

		GUILayout.Space(4f);
		GUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void ColorInfoField ( List<EffectInfo_Color> _infos ) {
		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);

            GUILayout.BeginVertical();
                // name
                GUILayout.Toggle( true, "Color", "dragtab");

                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
                GUILayout.BeginVertical();

                    // infos
                    for ( int i = 0; i < _infos.Count; ++i ) {
                        bool delete = false;
                        EffectInfo_Color info = _infos[i];

                        // target
                        EditorGUILayout.BeginHorizontal();

                            // receiver
                            EditorGUI.BeginChangeCheck();
                            info.target = EditorGUILayout.ObjectField( info.target, typeof(exSpriteBase), true ) as exSpriteBase;
                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }

                            //
                            if ( info.target != null ) {
                            }
                            else {
                                delete = true;
                            }

                            // Delete
                            if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                                delete = true;
                            }
                            GUILayout.Space(3f);

                        EditorGUILayout.EndHorizontal();

                        // curve, duration
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                GUILayout.Label("Custom Curve");
                                info.customCurve = EditorGUILayout.Toggle(info.customCurve,GUILayout.Width(15));
                                if ( info.customCurve ) {
                                    info.curve = EditorGUILayout.CurveField(info.curve);
                                }
                                else {
                                    info.curveType = (exEase.Type)EditorGUILayout.EnumPopup(info.curveType);
                                }
                                info.duration = EditorGUILayout.FloatField(info.duration);

                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        // deactive
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasDeactive = EditorGUILayout.Toggle(info.hasDeactive,GUILayout.Width(15));
                                GUILayout.Label("Deactive");
                                GUI.enabled = info.hasDeactive;
                                info.deactive = EditorGUILayout.ColorField("",info.deactive,GUILayout.Width(80));
                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        // press
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasPress = EditorGUILayout.Toggle(info.hasPress,GUILayout.Width(15));
                                GUILayout.Label("Press");
                                GUI.enabled = info.hasPress;
                                info.press = EditorGUILayout.ColorField("",info.press,GUILayout.Width(80));
                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        // hover
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasHover = EditorGUILayout.Toggle(info.hasHover,GUILayout.Width(15));
                                GUILayout.Label("Hover");
                                GUI.enabled = info.hasHover;
                                info.hover = EditorGUILayout.ColorField("",info.hover,GUILayout.Width(80));
                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        //
                        if ( delete ) {
                            _infos.RemoveAt(i);
                            --i;
                            EditorUtility.SetDirty(target);
                        }
                    }

                    // new slot
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        exSpriteBase receiver = EditorGUILayout.ObjectField( null, typeof(exSpriteBase), true, GUILayout.Width(150) ) as exSpriteBase;
                        if ( receiver != null ) {

                            EffectInfo_Color info = new EffectInfo_Color();
                            info.target = receiver;

                            _infos.Add(info);
                            EditorUtility.SetDirty(target);
                        }
                        GUILayout.Label( styles.iconToolbarPlus, GUILayout.Width(20) );
                    EditorGUILayout.EndHorizontal();

                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();

		GUILayout.Space(4f);
		GUILayout.EndHorizontal();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected void OffsetInfoField ( List<EffectInfo_Offset> _infos ) {
		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);

            GUILayout.BeginVertical();
                // name
                GUILayout.Toggle( true, "Offset", "dragtab");

                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
                GUILayout.BeginVertical();

                    // infos
                    for ( int i = 0; i < _infos.Count; ++i ) {
                        bool delete = false;
                        EffectInfo_Offset info = _infos[i];

                        // target
                        EditorGUILayout.BeginHorizontal();

                            // receiver
                            EditorGUI.BeginChangeCheck();
                            info.target = EditorGUILayout.ObjectField( info.target, typeof(exSpriteBase), true ) as exSpriteBase;
                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }

                            //
                            if ( info.target != null ) {
                            }
                            else {
                                delete = true;
                            }

                            // Delete
                            if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                                delete = true;
                            }
                            GUILayout.Space(3f);

                        EditorGUILayout.EndHorizontal();

                        // curve, duration
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                GUILayout.Label("Custom Curve");
                                info.customCurve = EditorGUILayout.Toggle(info.customCurve,GUILayout.Width(15));
                                if ( info.customCurve ) {
                                    info.curve = EditorGUILayout.CurveField(info.curve);
                                }
                                else {
                                    info.curveType = (exEase.Type)EditorGUILayout.EnumPopup(info.curveType);
                                }
                                info.duration = EditorGUILayout.FloatField(info.duration);

                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        float x = 0.0f;
                        float y = 0.0f;

                        // deactive
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasDeactive = EditorGUILayout.Toggle(info.hasDeactive,GUILayout.Width(15));
                                GUILayout.Label("Deactive");
                                GUI.enabled = info.hasDeactive;

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.deactive.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.deactive.y,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.deactive = new Vector2( x, y );
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        // press
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasPress = EditorGUILayout.Toggle(info.hasPress,GUILayout.Width(15));
                                GUILayout.Label("Press");
                                GUI.enabled = info.hasPress;

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.press.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.press.y,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.press = new Vector2( x, y );
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        // hover
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            EditorGUI.BeginChangeCheck();
                                info.hasHover = EditorGUILayout.Toggle(info.hasHover,GUILayout.Width(15));
                                GUILayout.Label("Hover");
                                GUI.enabled = info.hasHover;

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.hover.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.hover.y,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.hover = new Vector2( x, y );
                                EditorUtility.SetDirty(target);
                            }

                            GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();

                        //
                        if ( delete ) {
                            _infos.RemoveAt(i);
                            --i;
                            EditorUtility.SetDirty(target);
                        }
                    }

                    // new slot
                    EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        exSpriteBase receiver = EditorGUILayout.ObjectField( null, typeof(exSpriteBase), true, GUILayout.Width(150) ) as exSpriteBase;
                        if ( receiver != null ) {

                            EffectInfo_Offset info = new EffectInfo_Offset();
                            info.target = receiver;

                            _infos.Add(info);
                            EditorUtility.SetDirty(target);
                        }
                        GUILayout.Label( styles.iconToolbarPlus, GUILayout.Width(20) );
                    EditorGUILayout.EndHorizontal();

                GUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

            GUILayout.EndVertical();

		GUILayout.Space(4f);
		GUILayout.EndHorizontal();
    }
}
