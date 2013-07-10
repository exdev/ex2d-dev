// ======================================================================================
// File         : exRectSelection.cs
// Author       : Wu Jie 
// Last Change  : 07/10/2013 | 22:49:48 PM | Wednesday,July
// Description  : 
// ======================================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

    ///////////////////////////////////////////////////////////////////////////////
    // properties
    ///////////////////////////////////////////////////////////////////////////////

    System.Func<Vector2,Object> cb_PickObject;
    System.Func<Rect,Object[]> cb_PickRectObjects;
    System.Action<Object[]> cb_ConfirmSelection;

    int controlID = 10000; 
    bool isRectSelecting = false;
    Vector2 selectStartPoint;
    Rect selectRect;
    Object[] selectedObjs;

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
        Event e = Event.current;

        switch ( e.GetTypeForControl(controlID) ) {
        case EventType.Repaint:
            // draw select rect 
            if ( isRectSelecting ) {
                exEditorUtility.DrawRect( selectRect, new Color( 0.0f, 0.5f, 1.0f, 0.2f ), new Color( 0.0f, 0.5f, 1.0f, 1.0f ) );
            }
            break;

        case EventType.MouseDown:
            if ( e.button == 0 ) {
                GUIUtility.hotControl = controlID;
                GUIUtility.keyboardControl = controlID;
                selectStartPoint = e.mousePosition;
                UpdateSelectRect ();

                e.Use();
            }
            break;

        case EventType.MouseDrag:
            if ( GUIUtility.hotControl == controlID ) {

                if ( isRectSelecting == false && (e.mousePosition - selectStartPoint).magnitude > 6f ) {
                    isRectSelecting = true;
                }

                if ( isRectSelecting ) {
                    UpdateSelectRect ();

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
                    selectedObjs = new Object[0];
                }
                else {
                    // UnityEngine.Object @object = HandleUtility.PickGameObject(Event.current.mousePosition, true);
                    if ( e.shift ) {
                        // if (Selection.activeGameObject == @object) {
                        //     this.UpdateSelection(@object, RectSelection.SelectionType.Subtractive);
                        // }
                        // else {
                        //     this.UpdateSelection(@object, RectSelection.SelectionType.Additive);
                        // }
                    }
                    else {
                        if ( EditorGUI.actionKey ) {
                            // if (Selection.Contains(@object)) {
                            //     this.UpdateSelection(@object, RectSelection.SelectionType.Subtractive);
                            // }
                            // else {
                            //     this.UpdateSelection(@object, RectSelection.SelectionType.Additive);
                            // }
                        }
                        else {
                            // this.UpdateSelection(@object, RectSelection.SelectionType.Normal);
                            cb_ConfirmSelection( selectedObjs );
                        }
                    }
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

    void UpdateSelectRect () {
        selectRect = FromToRect( selectStartPoint, Event.current.mousePosition );
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
}
