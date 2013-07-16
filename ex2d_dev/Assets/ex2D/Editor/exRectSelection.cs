// ======================================================================================
// File         : exRectSelection.cs
// Author       : Wu Jie 
// Last Change  : 07/10/2013 | 22:49:48 PM | Wednesday,July
// Description  : 
// ======================================================================================

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
/// 
/// Rect Selection
/// 
///////////////////////////////////////////////////////////////////////////////

public class exRectSelection {

    public enum SelectionType {
        Normal,
        Additive,
        Subtractive
    }

    static int controlID = 10000; 

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    System.Func<Vector2,Object> cb_PickObject;
    System.Func<Rect,Object[]> cb_PickRectObjects;
    System.Action<Object,Object[]> cb_ConfirmSelection;

    bool isRectSelecting = false;
    Vector2 selectStartPoint;
    Object activeObj = null;
    Object[] selectedObjs = new Object[0];
    Object[] selectionStart = new Object[0];
    Dictionary<Object, bool> lastSelection;
    // Object[] currentSelection;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public exRectSelection ( System.Func<Vector2,Object> _pickObjectCallback,
                             System.Func<Rect,Object[]> _pickRectObjectsCallback,
                             System.Action<Object,Object[]> _confirmSelectionCallback ) 
    {
        cb_PickObject = _pickObjectCallback;
        cb_PickRectObjects = _pickRectObjectsCallback;
        cb_ConfirmSelection = _confirmSelectionCallback;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void OnGUI () {

        // DEBUG { 
        // EditorGUILayout.BeginHorizontal();
        // GUILayout.Label ( "active = " + ((activeObj == null) ? "null" : activeObj.name) );
        // foreach ( Object obj in selectedObjs )
        //     if ( obj != null )
        //         GUILayout.Label ( obj.name );
        // EditorGUILayout.EndHorizontal();
        // } DEBUG end 

        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.Repaint:
            // draw select rect 
            if ( isRectSelecting ) {
                Rect selectRect = FromToRect( selectStartPoint, e.mousePosition );
                exEditorUtility.DrawRect( selectRect, new Color( 0.0f, 0.5f, 1.0f, 0.2f ), new Color( 0.0f, 0.5f, 1.0f, 1.0f ) );
                // DISABLE { 
                // GUIStyle selectionRectStyle = "SelectionRect";
                // selectionRectStyle.Draw(selectRect, GUIContent.none, false, false, false, false);
                // } DISABLE end 
            }
            break;

        case EventType.MouseDown:
            if ( e.button == 0 ) {
                GUIUtility.hotControl = controlID;
                GUIUtility.keyboardControl = controlID;

                selectionStart = selectedObjs;
                selectStartPoint = e.mousePosition;
                isRectSelecting = false;

                e.Use();
            }
            break;

        // case EventType.MouseMove:
        //     if ( GUIUtility.hotControl == controlID ) {
        //         if ( e.shift ) {
        //             this.UpdateSelection(currentSelection, SelectionType.Additive);
        //         }
        //         else
        //         {
        //             if ( EditorGUI.actionKey ) {
        //                 this.UpdateSelection(currentSelection, SelectionType.Subtractive);
        //             }
        //             else {
        //                 this.UpdateSelection(currentSelection, SelectionType.Normal);
        //             }
        //         }
        //         e.Use();
        //     }
        //     break;

        case EventType.MouseDrag:
            if ( GUIUtility.hotControl == controlID ) {

                if ( isRectSelecting == false && (e.mousePosition - selectStartPoint).magnitude > 6f ) {
                    isRectSelecting = true;
                    lastSelection = null;
                    // currentSelection = null;
                }

                if ( isRectSelecting ) {
                    Rect selectRect = FromToRect( selectStartPoint, e.mousePosition );
                    Object[] array = cb_PickRectObjects ( selectRect );
                    // currentSelection = array;
                    bool flag = false;

                    if ( lastSelection == null ) {
                        lastSelection = new Dictionary<Object, bool>();
                        flag = true;
                    }

                    flag |= (lastSelection.Count != array.Length);
                    if ( !flag ) {
                        Dictionary<Object, bool> dictionary = new Dictionary<Object, bool>(array.Length);
                        Object[] array2 = array;
                        for ( int i = 0; i < array2.Length; ++i ) {
                            Object key = array2[i];
                            dictionary.Add(key, false);
                        }
                        foreach ( Object current2 in lastSelection.Keys ) {
                            if ( !dictionary.ContainsKey(current2) ) {
                                flag = true;
                                break;
                            }
                        }
                    }

                    if ( flag ) {
                        lastSelection = new Dictionary<Object, bool>(array.Length);
                        Object[] array3 = array;
                        for ( int j = 0; j < array3.Length; ++j ) {
                            Object key2 = array3[j];
                            lastSelection.Add(key2, false);
                        }
                        if ( array != null ) {
                            if ( e.shift ) {
                                UpdateSelection(array, SelectionType.Additive);
                            }
                            else {
                                if ( EditorGUI.actionKey ) {
                                    UpdateSelection(array, SelectionType.Subtractive);
                                }
                                else {
                                    UpdateSelection(array, SelectionType.Normal);
                                }
                            }

                            cb_ConfirmSelection( activeObj, selectedObjs );
                        }
                    }
                }

                e.Use();
            }
            break;

        case EventType.MouseUp:
            if ( GUIUtility.hotControl == controlID && e.button == 0 ) {
                GUIUtility.hotControl = 0;

                if ( isRectSelecting ) {
                    isRectSelecting = false;
                    selectionStart = new Object[0];
                }
                else {
                    Object obj = cb_PickObject(e.mousePosition);
                    // like command/ctrl selecting, but also switch the active object
                    if ( e.shift ) {
                        if ( IsActiveSelection (obj) ) {
                            UpdateSelection(obj, SelectionType.Subtractive);
                        }
                        else {
                            UpdateSelection(obj, SelectionType.Additive);
                        }
                    }
                    else {
                        if ( EditorGUI.actionKey ) {
                            if ( IsInSelectedList (obj) ) {
                                UpdateSelection(obj, SelectionType.Subtractive);
                            }
                            else {
                                UpdateSelection(obj, SelectionType.Additive);
                            }
                        }
                        else {
                            UpdateSelection(obj, SelectionType.Normal);
                        }
                    }
                    cb_ConfirmSelection( activeObj, selectedObjs );
                }

                e.Use();
            }
            break;

        case EventType.KeyDown:
            // TODO:
            break;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Rect FromToRect ( Vector2 _start, Vector2 _end ) {
        Rect result = new Rect(_start.x, _start.y, _end.x - _start.x, _end.y - _start.y);
        if (( result.width < 0f )) {
            result.x += result.width;
            result.width = -result.width;
        }
        if ( result.height < 0f ) {
            result.y += result.height;
            result.height = -result.height;
        }
        return result;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetSelection ( Object[] _objs ) {
        selectedObjs = _objs;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateSelection( Object _obj, SelectionType _type ) {
        Object[] objs;
        if ( _obj == null ) {
            objs = new Object[0];
        }
        else {
            objs = new Object[] {
                _obj
            };
        }
        UpdateSelection(objs, _type);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateSelection ( Object[] _objs, SelectionType _type ) {
        switch (_type) {
        case SelectionType.Additive:
            if ( _objs.Length > 0 ) {
                Object[] array = new Object[selectionStart.Length + _objs.Length];
                System.Array.Copy(selectionStart, array, selectionStart.Length);

                // add unique object
                int count = selectionStart.Length;
                for ( int i = 0; i < _objs.Length; ++i ) {
                    bool exists = false;
                    for ( int j = 0; j < selectionStart.Length; ++j ) {
                        if ( selectionStart[j] == _objs[i] ) {
                            exists = true;
                            break;
                        }
                    }
                    if ( exists == false ) {
                        array[count] = _objs[i];
                        ++count;
                    }
                }
                System.Array.Resize( ref array, count );

                // switch active object
                if ( isRectSelecting ) {
                    activeObj = array[0];
                }
                else {
                    activeObj = _objs[0];
                }

                selectedObjs = array;
                return;
            }

            selectedObjs = selectionStart;
            return;

        case SelectionType.Subtractive:
            Dictionary<Object, bool> dictionary = new Dictionary<Object, bool>(selectionStart.Length);
            for ( int j = 0; j < selectionStart.Length; ++j ) {
                Object key = selectionStart[j];
                dictionary.Add(key, false);
            }
            for ( int k = 0; k < _objs.Length; ++k ) {
                Object key2 = _objs[k];
                if ( dictionary.ContainsKey(key2) ) {
                    dictionary.Remove(key2);
                }
            }
            Object[] array = new Object[dictionary.Keys.Count];
            dictionary.Keys.CopyTo(array, 0);

            selectedObjs = array;

            if ( IsInSelectedList ( activeObj ) == false ) {
                activeObj = selectedObjs.Length > 0 ? selectedObjs[0] : null; 
            }
            return;
        }

        selectedObjs = _objs;
        if ( IsInSelectedList ( activeObj ) == false ) {
            activeObj = selectedObjs.Length > 0 ? selectedObjs[0] : null; 
        }

        return;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    bool IsInSelectedList ( Object _obj ) {
        foreach ( Object obj in selectedObjs ) {
            if ( obj == _obj )
                return true;
        }
        return false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    bool IsActiveSelection ( Object _obj ) {
        return activeObj == _obj;
    }
}
