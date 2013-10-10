// ======================================================================================
// File         : exUIMng.cs
// Author       : Wu Jie 
// Last Change  : 10/05/2013 | 10:58:01 AM | Saturday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
// defines
///////////////////////////////////////////////////////////////////////////////

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class RaycastSorter : IComparer {
    int IComparer.Compare ( object _a, object _b ) {
        if ( !(_a is RaycastHit) || !(_b is RaycastHit) ) 
            return 0;

        RaycastHit raycastHitA = (RaycastHit)_a;
        RaycastHit raycastHitB = (RaycastHit)_b;

        return (int)Mathf.Sign(raycastHitA.distance - raycastHitB.distance);
    }
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class ControlSorter: IComparer<exUIControl> {
    public int Compare( exUIControl _a, exUIControl _b ) {
        exUIControl parent = null;
        int level_a = 0;
        int level_b = 0;

        // a level
        parent = _a.parent;
        while ( parent ) {
            ++level_a; 
            parent = parent.parent;
        }

        // b level
        parent = _b.parent;
        while ( parent ) {
            ++level_b; 
            parent = parent.parent;
        }

        return level_a - level_b;
    }
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class ControlSorterByZ: IComparer<exUIControl> {
    public int Compare( exUIControl _a, exUIControl _b ) {
        int r = Mathf.CeilToInt(_a.transform.position.z - _b.transform.position.z);
        if ( r != 0 )
            return r;
        return _a.GetInstanceID() - _b.GetInstanceID();
    }
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

[ExecuteInEditMode]
public class exUIMng : MonoBehaviour {

    protected static exUIMng inst_ = null; 
    public static exUIMng inst {
        get {
            if ( inst_ == null ) {
                inst_ = FindObjectOfType ( typeof(exUIMng) ) as exUIMng;
            }
            return inst_;
        }
    }

    // static  RaycastSorter raycastSorter = new RaycastSorter();
    static ControlSorter controlSorter = new ControlSorter();
    static ControlSorterByZ controlSorterByZ = new ControlSorterByZ();

    ///////////////////////////////////////////////////////////////////////////////
    // structures
    ///////////////////////////////////////////////////////////////////////////////

    //
    [System.Serializable]
    public class TouchState {
        public bool active = false;
        public exUIControl hover = null;
    }

    //
    [System.Serializable]
    public class MouseState {
        public Vector2 pos = Vector2.zero;
        public exUIControl hover = null;
        public exUIControl pressed = null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // serializable 
    ///////////////////////////////////////////////////////////////////////////////

    public bool showDebugInfo = false;
    public bool showDebugInfoInGameView = false;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    bool initialized = false;
    List<exUIControl> controls = new List<exUIControl>(); // root contrls

    // TODO: some control needs two more finger to make it drag or accept events, 
    //       when this happens, you need to detect all TouchState and generate hotPoint state based on your touches.
    // hotpointState

    // internal ui status
    MouseState mouseState = new MouseState();
    TouchState[] touchStates = new TouchState[10];
    exUIControl focus = null; // the Input focus ( usually, the keyboard focus )

    ///////////////////////////////////////////////////////////////////////////////
    // functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        Init ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Start () {
        mouseState.pos = Input.mousePosition;
    }
	
    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

	void Update () {
        HandleEvents ();
	}

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnGUI () {
        if ( showDebugInfoInGameView ) {
            ShowDebugInfo ( new Rect(10, 10, 300, 300) );
        }
    }

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddControl ( exUIControl _ctrl ) {
        if ( controls.IndexOf(_ctrl) == -1 )
            controls.Add(_ctrl);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetFocus ( exUIControl _ctrl ) {
        if ( focus != _ctrl ) {
            if ( focus != null ) {
                focus.Send_OnUnFocus();
            }

            focus = _ctrl;

            if ( focus != null ) {
                focus.Send_OnFocus();
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Init () {
        if ( initialized )
            return;

        //
        if ( camera == null ) {
            Debug.LogError ( "The exUIMng should attach to a camera" );
            return;
        }

        //
        for ( int i = 0; i < 10; ++i ) {
            touchStates[i] = new TouchState();
        }

        // recursively add ui-tree
        exUIControl[] allControls = FindObjectsOfType(typeof(exUIControl)) as exUIControl[];
        for ( int i = 0; i < allControls.Length; ++i ) {
            exUIControl ctrl = allControls[i];
            exUIControl parent_ctrl = ctrl.FindParent();
            if ( parent_ctrl == null ) {
                exUIControl.FindAndAddChild (ctrl);

                //
                if ( controls.IndexOf(ctrl) == -1 ) {
                    controls.Add(ctrl);
                }
            }
        }

        //
        mouseState.pos = Vector2.zero;
        mouseState.hover = null;
        mouseState.pressed = null;

        //
        initialized = true;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void HandleEvents () {
        ProcessMouse();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    // void ProcessTouches () {
    //     for ( int i = 0; i < Input.touches.Length; ++i ) {
    //         Touch touch = Input.touches[i];
    //         if ( touch.fingerId >= 10 )
    //             continue;

    //         TouchState touchState = null;
    //         exUIControl hover = PickControl(touch.position);
    //         
    //         //
    //         if ( touch.phase == TouchPhase.Began ) {
    //             if ( hover != null ) {
    //                 e.category = exUIEvent.Category.Touch;
    //                 e.type =  exUIEvent.Type.TouchDown;
    //                 e.position = touch.position;
    //                 e.delta = touch.deltaPosition;
    //                 e.touchID = touch.fingerId;

    //                 EventInfo info = new EventInfo();
    //                 info.primaryControl = hover;
    //                 info.uiEvent = e;
    //                 eventInfos.Add(info);
    //             }

    //             // NOTE: it must be null
    //             SetTouchFocus ( touch.fingerId, null );
    //             touchStates[touch.fingerId].hover = hover;
    //         }
    //         else {
    //             // find the touch state
    //             touchState = touchStates[touch.fingerId];

    //             // set the last and current hot control 
    //             exUIControl keyboardControl = null;
    //             exUIControl lastCtrl = null;
    //             if ( touchState != null ) {
    //                 lastCtrl = touchState.hover;
    //                 touchState.hover = hover;
    //                 keyboardControl = touchState.keyboardControl;
    //             }

    //             if ( touch.phase == TouchPhase.Ended ) {
    //                 if ( touchState != null ) {
    //                     if ( keyboardControl != null ) {
    //                         exUIEvent e = new exUIEvent(); 
    //                         e.category = exUIEvent.Category.Touch;
    //                         e.type =  exUIEvent.Type.TouchUp;
    //                         e.position = touch.position;
    //                         e.delta = touch.deltaPosition;
    //                         e.touchID = touch.fingerId;

    //                         EventInfo info = new EventInfo();
    //                         info.primaryControl = keyboardControl;
    //                         info.uiEvent = e;
    //                         eventInfos.Add(info);
    //                     }
    //                 }
    //             }
    //             else if ( touch.phase == TouchPhase.Canceled ) {
    //                 if ( touchState != null )
    //                     SetTouchFocus ( touch.fingerId, null );
    //             }
    //             else if ( touch.phase == TouchPhase.Moved ) {
    //                 // process hover event
    //                 if ( lastCtrl != hover ) {
    //                     // add hover-in event
    //                     if ( hover != null ) {
    //                         exUIEvent e = new exUIEvent(); 
    //                         e.category = exUIEvent.Category.Touch;
    //                         e.type =  exUIEvent.Type.TouchEnter;
    //                         e.position = touch.position;
    //                         e.delta = touch.deltaPosition;
    //                         e.touchID = touch.fingerId;

    //                         EventInfo info = new EventInfo();
    //                         info.primaryControl = hover;
    //                         info.uiEvent = e;
    //                         eventInfos.Add(info);
    //                     }

    //                     // add hover-out event
    //                     if ( lastCtrl != null ) {
    //                         exUIEvent e = new exUIEvent(); 
    //                         e.category = exUIEvent.Category.Touch;
    //                         e.type =  exUIEvent.Type.TouchExit;
    //                         e.position = touch.position;
    //                         e.delta = touch.deltaPosition;
    //                         e.touchID = touch.fingerId;

    //                         EventInfo info = new EventInfo();
    //                         info.primaryControl = lastCtrl;
    //                         info.uiEvent = e;
    //                         eventInfos.Add(info);
    //                     }
    //                 }

    //                 //
    //                 if ( hover != null || keyboardControl != null ) {
    //                     exUIEvent e = new exUIEvent(); 
    //                     e.category = exUIEvent.Category.Touch;
    //                     e.type =  exUIEvent.Type.TouchMove;
    //                     e.position = touch.position;
    //                     e.delta = touch.deltaPosition;
    //                     e.touchID = touch.fingerId;

    //                     EventInfo info = new EventInfo();
    //                     info.primaryControl = (keyboardControl != null) ? keyboardControl : hover;
    //                     info.uiEvent = e;
    //                     eventInfos.Add(info);
    //                 }
    //             }
    //         }
    //     }
    // }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void ProcessMouse () {
        // get current position and delta pos
        Vector2 lastMousePos = mouseState.pos;
        mouseState.pos = Input.mousePosition;
        Vector2 deltaPos = mouseState.pos - lastMousePos;
        
        // get hot control
        exUIControl lastCtrl = mouseState.hover;
        exUIControl curCtrl = PickControl(mouseState.pos);
        mouseState.hover = curCtrl;

        // ======================================================== 
        // handle hover event
        // ======================================================== 

        if ( lastCtrl != curCtrl ) {
            // on hover out
            if ( lastCtrl != null ) {
                lastCtrl.Send_OnHoverOut();
            }

            // on hover in
            if ( curCtrl != null ) {
                curCtrl.Send_OnHoverIn();
            }
        }

        // ======================================================== 
        // handle press down event
        // ======================================================== 

        // get press down ID
        int pressDownID = -1;
        if ( Input.GetMouseButtonDown(0) ) {
            pressDownID = 0;
        }
        else if ( Input.GetMouseButtonDown(1) ) {
            pressDownID = 1;
        }
        else if ( Input.GetMouseButtonDown(2) ) {
            pressDownID = 2;
        }

        // send event
        if ( pressDownID != -1 ) {
            exUIControl curPressCtrl = curCtrl;
            if ( mouseState.pressed != null && mouseState.pressed.grabMouseOrTouch ) {
                curPressCtrl = mouseState.pressed;
            }

            // send press down event
            if ( curPressCtrl != null ) {
                curPressCtrl.Send_OnPressDown(pressDownID);
            }
            mouseState.pressed = curPressCtrl;
        }

        // ======================================================== 
        // handle moving before press-up
        // ======================================================== 

        List<int> pressingIDs = new List<int>();
        if ( Input.GetMouseButton(0) ) {
            pressingIDs.Add(0);
        }
        if ( Input.GetMouseButton(1) ) {
            pressingIDs.Add(1);
        }
        if ( Input.GetMouseButton(2) ) {
            pressingIDs.Add(2);
        }

        if ( deltaPos != Vector2.zero ) {

            exUIControl curPressCtrl = curCtrl;
            if ( mouseState.pressed != null && mouseState.pressed.grabMouseOrTouch ) {
                curPressCtrl = mouseState.pressed;
            }

            // send press down event
            if ( curPressCtrl != null ) {
                curPressCtrl.Send_OnPressMove(mouseState.pos, pressingIDs);
            }
        }

        // ======================================================== 
        // handle press up event 
        // ======================================================== 

        // get press up ID
        int pressUpID = -1;
        if ( Input.GetMouseButtonUp(0) ) {
            pressUpID = 0;
        }
        else if ( Input.GetMouseButtonUp(1) ) {
            pressUpID = 1;
        }
        else if ( Input.GetMouseButtonUp(2) ) {
            pressUpID = 2;
        }

        // send event
        if ( pressUpID != -1 ) {
            exUIControl curPressCtrl = curCtrl;
            if ( mouseState.pressed != null && mouseState.pressed.grabMouseOrTouch ) {
                curPressCtrl = mouseState.pressed;
            }

            // send press down event
            if ( curPressCtrl != null ) {
                curPressCtrl.Send_OnPressUp(pressUpID);
            }

            // only be null when no button pressed
            if ( pressingIDs.Count == 0 )
                mouseState.pressed = null;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    exUIControl PickControl ( Vector2 _screenPos ) {
        // pick 2D controls
        Vector3 worldPointerPos = camera.ScreenToWorldPoint ( _screenPos );
        controls.Sort(controlSorterByZ);
        for ( int i = 0; i < controls.Count; ++i ) {
            exUIControl ctrl = controls[i];
            exUIControl resultCtrl = RecursivelyGetUIControl ( ctrl, worldPointerPos );
            if ( resultCtrl != null )
                return resultCtrl;
        }

        // pick ray-cast controls
        Ray ray = camera.ScreenPointToRay ( _screenPos );
        ray.origin = new Vector3 ( ray.origin.x, ray.origin.y, camera.transform.position.z );
        RaycastHit[] hits = Physics.RaycastAll(ray);

        // DISABLE { 
        // System.Array.Sort(hits, raycastSorter);
        // if ( hits.Length > 0 ) {
        //     for ( int i = 0; i < hits.Length; ++i ) {
        //         RaycastHit hit = hits[i];
        //         GameObject go = hit.collider.gameObject;
        //         exUIControl ctrl = go.GetComponent<exUIControl>();
        //         if ( ctrl && ctrl.activeSelf ) {
        //             return ctrl;
        //         }
        //     }
        // }
        // return null;
        // } DISABLE end 

        List<exUIControl> hitControls = new List<exUIControl>();
        for ( int i = 0; i < hits.Length; ++i ) {
            RaycastHit hit = hits[i];
            GameObject go = hit.collider.gameObject;
            exUIControl ctrl = go.GetComponent<exUIControl>();
            if ( ctrl && ctrl.gameObject.activeInHierarchy && ctrl.activeInHierarchy ) {
                hitControls.Add(ctrl);
            }
        }
        if ( hitControls.Count > 0 ) {
            hitControls.Sort(controlSorter);
            return hitControls[hitControls.Count-1]; 
        }

        return null;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    exUIControl RecursivelyGetUIControl ( exUIControl _ctrl, Vector2 _worldPos ) {
        if ( _ctrl.gameObject.activeSelf == false || _ctrl.activeSelf == false )
            return null;

        //
        bool checkChildren = false;
        if ( _ctrl.useCollider ) {
            checkChildren = true;
        }
        else {
            Vector2 localPos = new Vector2( _worldPos.x - _ctrl.transform.position.x, 
                                            _worldPos.y - _ctrl.transform.position.y );
            localPos.y = -localPos.y;

            Rect boundingRect = _ctrl.GetLocalAABoundingRect();
            checkChildren = boundingRect.Contains(localPos);
        }

        //
        if ( checkChildren ) {
            for ( int i = 0; i < _ctrl.children.Count; ++i ) {
                exUIControl childCtrl = _ctrl.children[i];
                exUIControl resultCtrl = RecursivelyGetUIControl ( childCtrl, _worldPos );
                if ( resultCtrl != null )
                    return resultCtrl;
            }

            if ( _ctrl.useCollider == false ) {
                return _ctrl;
            }
        }

        return null;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // Debug
    ///////////////////////////////////////////////////////////////////////////////

    public void ShowDebugInfo ( Rect _pos ) {
        GUILayout.BeginArea( new Rect( _pos.x, _pos.y, _pos.width, _pos.height), "Debug", GUI.skin.window);

            // Keyboard
            GUILayout.Label( "Keyboard Focus: " + (focus ? focus.name : "None") );

            // Mouse State
            GUILayout.Label( "Mouse State" );

            GUILayout.BeginHorizontal ();
            GUILayout.Space (15);
                GUILayout.BeginVertical ();
                    GUILayout.Label( "pos: " + mouseState.pos.ToString() );
                    GUILayout.Label( "hover: " + (mouseState.hover ? mouseState.hover.name : "None") );
                    GUILayout.Label( "pressed: " + (mouseState.pressed ? mouseState.pressed.name : "None") );

                GUILayout.EndVertical ();
            GUILayout.EndHorizontal ();

            // Root Controls
            GUILayout.Label( "Root Controls" );

            GUILayout.BeginHorizontal ();
            GUILayout.Space (15);
                GUILayout.BeginVertical ();
                    for ( int i = 0; i < controls.Count; ++i ) {
                        GUILayout.Label( "[" + i + "] " + controls[i].name );
                    }
                GUILayout.EndVertical ();
            GUILayout.EndHorizontal ();
        GUILayout.EndArea();
    }
}
