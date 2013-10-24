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
                        bool infoDeleted = false;
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
                                infoDeleted = true;
                            }

                            // Delete
                            if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                                infoDeleted = true;
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

                        // normal
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            if ( GUILayout.Button("Sync", GUILayout.Width(50)) ) {
                                info.normal = info.target.localScale;
                                EditorUtility.SetDirty(target);
                            }

                            EditorGUI.BeginChangeCheck();

                                GUILayout.Label("Normal");

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.normal.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.normal.y,GUILayout.Width(30));
                                GUILayout.Label("Z", GUILayout.Width(10));
                                z = EditorGUILayout.FloatField("",info.normal.z,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.normal = new Vector3( x, y, z );
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        // Properties
                        for ( int j = 0; j < info.propInfos.Count; ++j ) {
                            EffectInfo_Scale.PropInfo propInfo = info.propInfos[j];

                            EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(30);
                                // propInfoDeleted
                                bool propInfoDeleted = false;
                                if ( GUILayout.Button( styles.iconToolbarMinus, 
                                                       "InvisibleButton", 
                                                       GUILayout.Width(styles.iconToolbarMinus.width), 
                                                       GUILayout.Height(styles.iconToolbarMinus.height) ) ) 
                                {
                                    propInfoDeleted = true;
                                }

                                EditorGUI.BeginChangeCheck();

                                    GUILayout.Label(System.Enum.GetName( typeof(EffectEventType), propInfo.type ));
                                    GUILayout.Label("X", GUILayout.Width(10));
                                    x = EditorGUILayout.FloatField("",propInfo.val.x,GUILayout.Width(30));
                                    GUILayout.Label("Y", GUILayout.Width(10));
                                    y = EditorGUILayout.FloatField("",propInfo.val.y,GUILayout.Width(30));
                                    GUILayout.Label("Z", GUILayout.Width(10));
                                    z = EditorGUILayout.FloatField("",propInfo.val.z,GUILayout.Width(30));

                                if ( EditorGUI.EndChangeCheck() ) {
                                    propInfo.val = new Vector3( x, y, z );
                                    EditorUtility.SetDirty(target);
                                }

                                if ( propInfoDeleted ) {
                                    info.propInfos.RemoveAt(j);
                                    --j;
                                    EditorUtility.SetDirty(target);
                                }

                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }

                        // Add Property
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            List<string> eventNameList = new List<string>(); 
                            eventNameList.Add( "Add Property" );
                            eventNameList.AddRange( System.Enum.GetNames(typeof(EffectEventType)) );

                            foreach ( EffectInfo_Scale.PropInfo propInfo in info.propInfos ) {
                                int idx = eventNameList.IndexOf( System.Enum.GetName( typeof(EffectEventType), propInfo.type )  );
                                if ( idx != -1 ) {
                                    eventNameList.RemoveAt(idx);
                                }
                            }

                            int choice = EditorGUILayout.Popup ( 0, eventNameList.ToArray(), GUILayout.Width(100) );
                            if ( choice != 0 ) {
                                EffectInfo_Scale.PropInfo propInfo = new EffectInfo_Scale.PropInfo();
                                propInfo.type = (EffectEventType)System.Enum.Parse(typeof(EffectEventType), eventNameList[choice]);
                                propInfo.val = info.normal;
                                info.propInfos.Add(propInfo); 
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        //
                        if ( infoDeleted ) {
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
                        bool infoDeleted = false;
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
                                infoDeleted = true;
                            }

                            // Delete
                            if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                                infoDeleted = true;
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

                        // normal
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            if ( GUILayout.Button("Sync", GUILayout.Width(50)) ) {
                                info.normal = info.target.color;
                                EditorUtility.SetDirty(target);
                            }

                            EditorGUI.BeginChangeCheck();

                                GUILayout.Label("Normal");

                                info.normal = EditorGUILayout.ColorField("",info.normal,GUILayout.Width(80));
                            if ( EditorGUI.EndChangeCheck() ) {
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        // Properties
                        for ( int j = 0; j < info.propInfos.Count; ++j ) {
                            EffectInfo_Color.PropInfo propInfo = info.propInfos[j];

                            EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(30);
                                // propInfoDeleted
                                bool propInfoDeleted = false;
                                if ( GUILayout.Button( styles.iconToolbarMinus, 
                                                       "InvisibleButton", 
                                                       GUILayout.Width(styles.iconToolbarMinus.width), 
                                                       GUILayout.Height(styles.iconToolbarMinus.height) ) ) 
                                {
                                    propInfoDeleted = true;
                                }

                                EditorGUI.BeginChangeCheck();

                                    GUILayout.Label(System.Enum.GetName( typeof(EffectEventType), propInfo.type ));
                                    Color newColor = EditorGUILayout.ColorField("",propInfo.val,GUILayout.Width(80));

                                if ( EditorGUI.EndChangeCheck() ) {
                                    propInfo.val = newColor;
                                    EditorUtility.SetDirty(target);
                                }

                                if ( propInfoDeleted ) {
                                    info.propInfos.RemoveAt(j);
                                    --j;
                                    EditorUtility.SetDirty(target);
                                }

                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }

                        // Add Property
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            List<string> eventNameList = new List<string>(); 
                            eventNameList.Add( "Add Property" );
                            eventNameList.AddRange( System.Enum.GetNames(typeof(EffectEventType)) );

                            foreach ( EffectInfo_Color.PropInfo propInfo in info.propInfos ) {
                                int idx = eventNameList.IndexOf( System.Enum.GetName( typeof(EffectEventType), propInfo.type )  );
                                if ( idx != -1 ) {
                                    eventNameList.RemoveAt(idx);
                                }
                            }

                            int choice = EditorGUILayout.Popup ( 0, eventNameList.ToArray(), GUILayout.Width(100) );
                            if ( choice != 0 ) {
                                EffectInfo_Color.PropInfo propInfo = new EffectInfo_Color.PropInfo();
                                propInfo.type = (EffectEventType)System.Enum.Parse(typeof(EffectEventType), eventNameList[choice]);
                                propInfo.val = info.normal;
                                info.propInfos.Add(propInfo); 
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        //
                        if ( infoDeleted ) {
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
                        bool infoDeleted = false;
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
                                infoDeleted = true;
                            }

                            // Delete
                            if ( GUILayout.Button( styles.iconToolbarMinus, "InvisibleButton", GUILayout.Width(20f) ) ) {
                                infoDeleted = true;
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

                        // normal
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(30);
                            if ( GUILayout.Button("Sync", GUILayout.Width(50)) ) {
                                info.normal = info.target.offset;
                                EditorUtility.SetDirty(target);
                            }

                            EditorGUI.BeginChangeCheck();

                                GUILayout.Label("Normal");

                                GUILayout.Label("X", GUILayout.Width(10));
                                x = EditorGUILayout.FloatField("",info.normal.x,GUILayout.Width(30));
                                GUILayout.Label("Y", GUILayout.Width(10));
                                y = EditorGUILayout.FloatField("",info.normal.y,GUILayout.Width(30));
                            if ( EditorGUI.EndChangeCheck() ) {
                                info.normal = new Vector2( x, y );
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        // Properties
                        for ( int j = 0; j < info.propInfos.Count; ++j ) {
                            EffectInfo_Offset.PropInfo propInfo = info.propInfos[j];

                            EditorGUILayout.BeginHorizontal();
                                GUILayout.Space(30);
                                // propInfoDeleted
                                bool propInfoDeleted = false;
                                if ( GUILayout.Button( styles.iconToolbarMinus, 
                                                       "InvisibleButton", 
                                                       GUILayout.Width(styles.iconToolbarMinus.width), 
                                                       GUILayout.Height(styles.iconToolbarMinus.height) ) ) 
                                {
                                    propInfoDeleted = true;
                                }

                                EditorGUI.BeginChangeCheck();

                                    GUILayout.Label(System.Enum.GetName( typeof(EffectEventType), propInfo.type ));
                                    GUILayout.Label("X", GUILayout.Width(10));
                                    x = EditorGUILayout.FloatField("",propInfo.val.x,GUILayout.Width(30));
                                    GUILayout.Label("Y", GUILayout.Width(10));
                                    y = EditorGUILayout.FloatField("",propInfo.val.y,GUILayout.Width(30));

                                if ( EditorGUI.EndChangeCheck() ) {
                                    propInfo.val = new Vector2(x,y);
                                    EditorUtility.SetDirty(target);
                                }

                                if ( propInfoDeleted ) {
                                    info.propInfos.RemoveAt(j);
                                    --j;
                                    EditorUtility.SetDirty(target);
                                }

                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.Space();
                        }

                        // Add Property
                        EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                            List<string> eventNameList = new List<string>(); 
                            eventNameList.Add( "Add Property" );
                            eventNameList.AddRange( System.Enum.GetNames(typeof(EffectEventType)) );

                            foreach ( EffectInfo_Offset.PropInfo propInfo in info.propInfos ) {
                                int idx = eventNameList.IndexOf( System.Enum.GetName( typeof(EffectEventType), propInfo.type )  );
                                if ( idx != -1 ) {
                                    eventNameList.RemoveAt(idx);
                                }
                            }

                            int choice = EditorGUILayout.Popup ( 0, eventNameList.ToArray(), GUILayout.Width(100) );
                            if ( choice != 0 ) {
                                EffectInfo_Offset.PropInfo propInfo = new EffectInfo_Offset.PropInfo();
                                propInfo.type = (EffectEventType)System.Enum.Parse(typeof(EffectEventType), eventNameList[choice]);
                                propInfo.val = info.normal;
                                info.propInfos.Add(propInfo); 
                                EditorUtility.SetDirty(target);
                            }
                        EditorGUILayout.EndHorizontal();

                        //
                        if ( infoDeleted ) {
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
