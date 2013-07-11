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
    System.Action<Object[]> cb_ConfirmSelection;

    bool isRectSelecting = false;
    Vector2 selectStartPoint;
    Object[] selectedObjs = new Object[0];
    Dictionary<Object, bool> lastSelection;

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public exRectSelection ( System.Func<Vector2,Object> _pickObjectCallback,
                             System.Func<Rect,Object[]> _pickRectObjectsCallback,
                             System.Action<Object[]> _confirmSelectionCallback ) 
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
        EditorGUILayout.BeginHorizontal();
        foreach ( Object obj in selectedObjs )
            if ( obj != null )
                GUILayout.Label ( obj.name );
        EditorGUILayout.EndHorizontal();
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
                selectStartPoint = e.mousePosition;
                isRectSelecting = false;

                e.Use();
            }
            break;

        case EventType.MouseDrag:
            if ( GUIUtility.hotControl == controlID ) {

                if ( isRectSelecting == false && (e.mousePosition - selectStartPoint).magnitude > 6f ) {
                    isRectSelecting = true;
                }

                if ( isRectSelecting ) {
                    Rect selectRect = FromToRect( selectStartPoint, e.mousePosition );
                    selectedObjs = cb_PickRectObjects ( selectRect );
                    cb_ConfirmSelection( selectedObjs );
                }

                e.Use();
            }
            break;

        case EventType.MouseUp:
			if ( GUIUtility.hotControl == controlID && e.button == 0 ) {
				GUIUtility.hotControl = 0;

                if ( isRectSelecting ) {
                    isRectSelecting = false;
                }
                else {
                    Object obj = cb_PickObject(e.mousePosition);
                    // like command/ctrl selecting, but also switch the active object
                    if ( e.shift ) {
                        if ( IsActiveSelection (obj) ) {
                            selectedObjs = UpdateSelection(obj, SelectionType.Subtractive);
                        }
                        else {
                            selectedObjs = UpdateSelection(obj, SelectionType.Additive);
                        }
                    }
                    else {
                        if ( EditorGUI.actionKey ) {
                            if ( IsInSelectedList (obj) ) {
                                selectedObjs = UpdateSelection(obj, SelectionType.Subtractive);
                            }
                            else {
                                selectedObjs = UpdateSelection(obj, SelectionType.Additive);
                            }
                        }
                        else {
                            selectedObjs = UpdateSelection(obj, SelectionType.Normal);
                        }
                    }
                    cb_ConfirmSelection( selectedObjs );
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

    Object[] UpdateSelection( Object _obj, SelectionType _type ) {
        Object[] objs;
        if ( _obj == null ) {
            objs = new Object[0];
        }
        else {
            objs = new Object[] {
                _obj
            };
        }
        return UpdateSelection(objs, _type);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    Object[] UpdateSelection ( Object[] _objs, SelectionType _type ) {
        Object[] selectionStart = selectedObjs;
        switch (_type) {
        case SelectionType.Additive:
            if ( _objs.Length > 0 ) {
                Object[] array = new Object[selectionStart.Length + _objs.Length];
                System.Array.Copy(selectionStart, array, selectionStart.Length);
                for ( int i = 0; i < _objs.Length; ++i ) {
                    array[selectionStart.Length + i] = _objs[i];
                }
                return array;
            }
            return selectionStart;

        case SelectionType.Subtractive:
            Dictionary<Object, bool> dictionary = new Dictionary<Object, bool>(selectionStart.Length);
            Object[] array2 = selectionStart;
            for ( int j = 0; j < array2.Length; ++j ) {
                Object key = array2[j];
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
            return array;
        }

        return _objs;
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
        return selectedObjs.Length > 0 && selectedObjs[0] == _obj;
    }
}
