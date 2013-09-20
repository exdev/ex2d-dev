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

public class exRectSelection<T> {

    public enum SelectionType {
        Normal,
        Additive,
        Subtractive
    }

    static int controlID = 10000; 

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    System.Func<Vector2,T> cb_PickObject;
    System.Func<Rect,T[]> cb_PickRectObjects;
    System.Action<T,T[]> cb_ConfirmSelection;
    System.Func<Vector2,Vector2,Rect> cb_UpdateRect;

    bool isRectSelecting = false;
    Vector2 selectStartPoint;
    T activeObj = default(T);
    T[] selectedObjs = new T[0];
    T[] selectionStart = new T[0];
    Dictionary<T, bool> lastSelection;
    // T[] currentSelection;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public Vector2 GetSelectStartPoint () { return selectStartPoint; }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public exRectSelection ( System.Func<Vector2,T> _pickObjectCallback,
                             System.Func<Rect,T[]> _pickRectObjectsCallback,
                             System.Action<T,T[]> _confirmSelectionCallback,
                             System.Func<Vector2,Vector2,Rect> _updateRectCallback = null ) 
    {
        cb_PickObject = _pickObjectCallback;
        cb_PickRectObjects = _pickRectObjectsCallback;
        cb_ConfirmSelection = _confirmSelectionCallback;

        cb_UpdateRect = _updateRectCallback;
        if ( cb_UpdateRect == null )
            cb_UpdateRect = FromToRect;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void OnGUI () {

        // DEBUG { 
        // EditorGUILayout.BeginHorizontal();
        // GUILayout.Label ( "active = " + ((activeObj == null) ? "null" : activeObj.name) );
        // foreach ( T obj in selectedObjs )
        //     if ( obj != null )
        //         GUILayout.Label ( obj.name );
        // EditorGUILayout.EndHorizontal();
        // } DEBUG end 

        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.Repaint:
            // draw select rect 
            if ( isRectSelecting ) {
                Rect selectRect = cb_UpdateRect( selectStartPoint, e.mousePosition );
                exEditorUtility.GUI_DrawRect( selectRect, new Color( 0.0f, 0.5f, 1.0f, 0.2f ), new Color( 0.0f, 0.5f, 1.0f, 1.0f ) );
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
                    Rect selectRect = cb_UpdateRect( selectStartPoint, e.mousePosition );
                    T[] array = cb_PickRectObjects ( selectRect );
                    // currentSelection = array;
                    bool flag = false;

                    if ( lastSelection == null ) {
                        lastSelection = new Dictionary<T, bool>();
                        flag = true;
                    }

                    flag |= (lastSelection.Count != array.Length);
                    if ( !flag ) {
                        Dictionary<T, bool> dictionary = new Dictionary<T, bool>(array.Length);
                        T[] array2 = array;
                        for ( int i = 0; i < array2.Length; ++i ) {
                            T key = array2[i];
                            dictionary.Add(key, false);
                        }
                        foreach ( T current2 in lastSelection.Keys ) {
                            if ( !dictionary.ContainsKey(current2) ) {
                                flag = true;
                                break;
                            }
                        }
                    }

                    if ( flag ) {
                        lastSelection = new Dictionary<T, bool>(array.Length);
                        T[] array3 = array;
                        for ( int j = 0; j < array3.Length; ++j ) {
                            T key2 = array3[j];
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
                    selectionStart = new T[0];
                }
                else {
                    T obj = cb_PickObject(e.mousePosition);
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
        if ( result.width < 0f ) {
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

    public void SetSelection ( T[] _objs ) {
        selectedObjs = _objs;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateSelection( T _obj, SelectionType _type ) {
        T[] objs;
        if ( _obj == null ) {
            objs = new T[0];
        }
        else {
            objs = new T[] {
                _obj
            };
        }
        UpdateSelection(objs, _type);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateSelection ( T[] _objs, SelectionType _type ) {
        switch (_type) {
        case SelectionType.Additive:
            if ( _objs.Length > 0 ) {
                T[] array = new T[selectionStart.Length + _objs.Length];
                System.Array.Copy(selectionStart, array, selectionStart.Length);

                // add unique object
                int count = selectionStart.Length;
                for ( int i = 0; i < _objs.Length; ++i ) {
                    bool exists = false;
                    for ( int j = 0; j < selectionStart.Length; ++j ) {
                        if ( ReferenceEquals( selectionStart[j], _objs[i] ) ) {
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
            Dictionary<T, bool> dictionary = new Dictionary<T, bool>(selectionStart.Length);
            for ( int j = 0; j < selectionStart.Length; ++j ) {
                T key = selectionStart[j];
                dictionary.Add(key, false);
            }
            for ( int k = 0; k < _objs.Length; ++k ) {
                T key2 = _objs[k];
                if ( dictionary.ContainsKey(key2) ) {
                    dictionary.Remove(key2);
                }
            }
            T[] array = new T[dictionary.Keys.Count];
            dictionary.Keys.CopyTo(array, 0);

            selectedObjs = array;

            if ( IsInSelectedList ( activeObj ) == false ) {
                activeObj = selectedObjs.Length > 0 ? selectedObjs[0] : default(T); 
            }
            return;
        }

        selectedObjs = _objs;
        if ( IsInSelectedList ( activeObj ) == false ) {
            activeObj = selectedObjs.Length > 0 ? selectedObjs[0] : default(T); 
        }

        return;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    bool IsInSelectedList ( T _obj ) {
        foreach ( T obj in selectedObjs ) {
            if ( ReferenceEquals ( obj, _obj ) )
                return true;
        }
        return false;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    bool IsActiveSelection ( T _obj ) {
        return ReferenceEquals ( activeObj, _obj );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool IsInRectSelecting () { return isRectSelecting; }
}
