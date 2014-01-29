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

public class ControlSorterByLevel: IComparer<exUIControl> {
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

public class ControlSorterByPriority: IComparer<exUIControl> {
    public int Compare( exUIControl _a, exUIControl _b ) {
        int priority_a = _a.priority;
        int priority_b = _b.priority;

        if ( _a == _b )
            return 0;

        if ( _a.gameObject.activeInHierarchy == false || _a.activeInHierarchy == false )
            priority_a = -999;

        if ( _b.gameObject.activeInHierarchy == false || _b.activeInHierarchy == false )
            priority_b = -999;

        if ( priority_a == priority_b )
            return -1;

        return priority_b - priority_a;
    }
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

public class ControlSorterByPriority2: IComparer<exUIControl> {
    public int Compare( exUIControl _a, exUIControl _b ) {
        exUIControl parent = null;
        int priority_a = _a.priority;
        int priority_b = _b.priority;

        // a level
        parent = _a.parent;
        while ( parent ) {
            priority_a += parent.priority; 
            parent = parent.parent;
        }

        // b level
        parent = _b.parent;
        while ( parent ) {
            priority_b += parent.priority; 
            parent = parent.parent;
        }

        return priority_a - priority_b;
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

public class exHotPoint {
    public int id = -1;
    public bool active = false;
    public bool pressDown = false;
    public bool pressUp = false;
    public Vector2 pos = Vector2.zero;
    public Vector2 delta = Vector2.zero;
    public Vector3 worldPos = Vector3.zero;
    public Vector3 worldDelta = Vector3.zero;

    public exUIControl hover = null;
    public exUIControl pressed = null;

    public bool isMouse = false;
    public bool isTouch { get { return isMouse == false; } }  

    // 0: left, 1: right, 2: middle
    public bool GetMouseButton ( int _id ) { return isMouse && (id == _id); }

    public void Reset () {
        active = false;
        pressDown = false;
        pressUp = false;
        pos = Vector2.zero;
        delta = Vector2.zero;
        worldPos = Vector3.zero;
        worldDelta = Vector3.zero;
        hover = null;
        pressed = null;
    } 
}

// ------------------------------------------------------------------ 
// Desc: 
// ------------------------------------------------------------------ 

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

    static ControlSorterByPriority controlSorterByPrioirty = new ControlSorterByPriority();
    static ControlSorterByLevel controlSorterByLevel = new ControlSorterByLevel();

    ///////////////////////////////////////////////////////////////////////////////
    // serializable 
    ///////////////////////////////////////////////////////////////////////////////

    public bool simulateMouseAsTouch = false;
    public bool showDebugInfo = false;
    public bool showDebugInfoInGameView = false;

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    bool initialized = false;
    bool hasMouse = false;
    bool hasTouch = false;
    // bool hasKeyboard = false;
    // bool hasController = false;

    List<exUIControl> controls = new List<exUIControl>(); // root contrls

    // internal ui status
    exHotPoint[] mousePoints = new exHotPoint[3];
    exHotPoint[] touchPoints = new exHotPoint[10];
    exUIControl focus = null; // the Input focus ( usually, the keyboard focus )

    ///////////////////////////////////////////////////////////////////////////////
    // static functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static List<exUIControl> GetRoutine ( exUIControl _ctrl ) {
        List<exUIControl> routine = new List<exUIControl>();

        exUIControl parentCtrl = _ctrl.parent;
        while ( parentCtrl != null ) {
            routine.Add(parentCtrl);
            parentCtrl = parentCtrl.parent;
        }
        return routine;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static exUIControl FindRoot ( exUIControl _ctrl ) {
        exUIControl root = null;
        exUIControl parentCtrl = _ctrl;
        while ( parentCtrl != null ) {
            root = parentCtrl;
            parentCtrl = parentCtrl.parent;
        }
        return root;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static exUIControl FindParent ( exUIControl _ctrl ) {
        Transform tranParent = _ctrl.transform.parent;
        while ( tranParent != null ) {
            exUIControl el = tranParent.GetComponent<exUIControl>();
            if ( el != null )
                return el;
            tranParent = tranParent.parent;
        }
        return null;
    } 

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void FindAndAddChild ( exUIControl _ctrl ) {
        _ctrl.children.Clear();
        FindAndAddChildRecursively (_ctrl, _ctrl.transform );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    static void FindAndAddChildRecursively ( exUIControl _ctrl, Transform _trans ) {
        foreach ( Transform child in _trans ) {
            exUIControl childCtrl = child.GetComponent<exUIControl>();
            if ( childCtrl ) {
                _ctrl.AddChild (childCtrl);
                FindAndAddChild (childCtrl);
            }
            else {
                FindAndAddChildRecursively( _ctrl, child );
            }
        }
    }

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
        if ( controls.IndexOf(_ctrl) == -1 ) {
            controls.Add(_ctrl);
            exUIMng.FindAndAddChild (_ctrl);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetFocus ( exUIControl _ctrl ) {
        if ( focus != _ctrl ) {
            exUIControl unfocus = focus;
            if ( focus != null ) {
                exUIFocusEvent focusEvent = new exUIFocusEvent();
                focusEvent.relatedTarget = focus;
                focus.OnUnfocus(focusEvent);
            }

            focus = _ctrl;

            if ( focus != null ) {
                exUIFocusEvent focusEvent = new exUIFocusEvent();
                focusEvent.relatedTarget = unfocus;
                focus.OnFocus(focusEvent);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void DispatchEvent ( exUIControl _sender, List<exUIEventListener> _eventListeners, exUIEvent _event ) {
        if ( _eventListeners.Count <= 0 )
            return;

        if ( _event.bubbles ) {
            List<exUIControl> routine = GetRoutine( _sender );
            _event.target = _sender; 

            for ( int i = 0; i < _eventListeners.Count; ++i ) {
                exUIEventListener listener = _eventListeners[i];

                // capture phase
                if ( listener.capturePhase ) {
                    _event.eventPhase = exUIEventPhase.Capture;

                    for ( int j = routine.Count-1; j >= 0; --j ) {
                        _event.currentTarget = routine[j];
                        listener.func ( _event );
                        if ( _event.isPropagationStopped )
                            continue;
                    }
                }

                // target phase
                _event.eventPhase = exUIEventPhase.Target;
                _event.currentTarget = _sender;
                listener.func ( _event );
                if ( _event.isPropagationStopped )
                    continue;

                // bubble phase
                _event.eventPhase = exUIEventPhase.Bubble;
                for ( int j = 0; j < routine.Count; ++j ) {
                    _event.currentTarget = routine[j];
                    listener.func ( _event );
                    if ( _event.isPropagationStopped )
                        continue;
                }
            }
        }
        else {
            _event.target = _sender; 

            for ( int i = 0; i < _eventListeners.Count; ++i ) {
                exUIEventListener listener = _eventListeners[i];

                // target phase
                _event.eventPhase = exUIEventPhase.Target;
                _event.currentTarget = _sender;
                listener.func ( _event );
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // NOTE: FindObjectsOfType() will not find deactived GameObjects, 
    //       so you need to manually add them to exUIMng 
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
        if ( Application.platform == RuntimePlatform.Android
          || Application.platform == RuntimePlatform.IPhonePlayer
#if UNITY_4_2
          || Application.platform == RuntimePlatform.WP8Player
          || Application.platform == RuntimePlatform.BB10Player
#endif
          )
		{
			hasMouse = false;
			hasTouch = true;
            // hasKeyboard = false;
            // hasController = true;
		}
        else if ( Application.platform == RuntimePlatform.PS3
               || Application.platform == RuntimePlatform.XBOX360 
               )
		{
			hasMouse = false;
			hasTouch = false;
            // hasKeyboard = false;
            // hasController = true;
		}
		else if ( Application.platform == RuntimePlatform.WindowsEditor 
               || Application.platform == RuntimePlatform.OSXEditor
               )
		{
			hasMouse = true;
			hasTouch = false;
            // hasKeyboard = true;
            // hasController = true;
		}

        //
        for ( int i = 0; i < 10; ++i ) {
            touchPoints[i] = new exHotPoint();
            touchPoints[i].Reset();
            touchPoints[i].id = i;
        }
        for ( int i = 0; i < 3; ++i ) {
            mousePoints[i] = new exHotPoint();
            mousePoints[i].Reset();
            mousePoints[i].id = i;
            mousePoints[i].isMouse = true;
        }

        // find all controls in the scene, and add root controls to UIMng
        exUIControl[] allControls = FindObjectsOfType(typeof(exUIControl)) as exUIControl[];
        for ( int i = 0; i < allControls.Length; ++i ) {
            exUIControl ctrl = allControls[i];
            exUIControl parent_ctrl = exUIMng.FindParent(ctrl);
            if ( parent_ctrl == null ) {
                AddControl (ctrl);
            }
        }

        //
        initialized = true;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void PressDown ( int _id, exUIControl _ctrl, exUIEvent _event ) {
        exHotPoint[] hotPoints = null;

        if ( hasTouch || simulateMouseAsTouch ) {
            hotPoints = touchPoints;
        }
        else {
            hotPoints = mousePoints;
        }

        //
        for ( int i = 0; i < hotPoints.Length; ++i ) {
            exHotPoint hotPoint = hotPoints[i];

            if ( hotPoint.id != _id )
                continue;

            hotPoint.pressed = _ctrl;
            _ctrl.OnPressDown(_event);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void HandleEvents () {
        // make sure all hotpoints de-active at first
        for ( int i = 0; i < touchPoints.Length; ++i ) {
            exHotPoint hotPoint = touchPoints[i];
            hotPoint.active = false;
            hotPoint.pressDown = false;
            hotPoint.pressUp = false;
        }
        for ( int i = 0; i < mousePoints.Length; ++i ) {
            exHotPoint hotPoint = mousePoints[i];
            hotPoint.active = false;
            hotPoint.pressDown = false;
            hotPoint.pressUp = false;
        }

        //
        if ( hasMouse )
            HandleMouse();

        if ( hasTouch )
            HandleTouches();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void HandleHotPoints ( exHotPoint[] _hotPoints, bool _isMouse ) {

        // ======================================================== 
        // handle hover event
        // ======================================================== 

        int hotPointCountForMouse = _isMouse ? 1 : _hotPoints.Length;
        for ( int i = 0; i < hotPointCountForMouse; ++i ) {
            exHotPoint hotPoint = _hotPoints[i];

            if ( hotPoint.active == false )
                continue;

            if ( hotPoint.pressed != null && hotPoint.pressed.grabMouseOrTouch )
                continue;

            // get hot control
            exUIControl lastCtrl = hotPoint.hover;
            exUIControl curCtrl = PickControl(hotPoint.pos);
            hotPoint.hover = curCtrl;

            if ( lastCtrl != curCtrl ) {
                exUIPointInfo pointInfo = new exUIPointInfo();
                pointInfo.id = hotPoint.id;
                pointInfo.pos = hotPoint.pos;
                pointInfo.delta = hotPoint.delta;
                pointInfo.worldPos = hotPoint.worldPos;
                pointInfo.worldDelta = hotPoint.worldDelta;

                exUIPointEvent pointEvent = new exUIPointEvent();
                pointEvent.isMouse = hotPoint.isMouse;
                pointEvent.pointInfos = new exUIPointInfo [] {
                    pointInfo
                };

                // on hover out
                if ( lastCtrl != null ) {
                    lastCtrl.OnHoverOut(pointEvent);
                }

                // on hover in
                if ( curCtrl != null ) {
                    pointEvent.Reset();
                    curCtrl.OnHoverIn(pointEvent);
                }
            }
        }

        if ( _isMouse ) {
            for ( int i = 1; i < _hotPoints.Length; ++i ) {
                _hotPoints[i].hover = _hotPoints[0].hover;
            }

            // ======================================================== 
            // send scroll wheel event
            // ======================================================== 

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if ( scroll != 0.0f && _hotPoints[0].hover != null ) {
                exUIWheelEvent wheelEvent = new exUIWheelEvent();
                wheelEvent.delta = scroll;
                _hotPoints[0].hover.OnMouseWheel(wheelEvent);
            }
        }

        // ======================================================== 
        // handle press down event
        // ======================================================== 

        for ( int i = 0; i < _hotPoints.Length; ++i ) {
            exHotPoint hotPoint = _hotPoints[i];

            if ( hotPoint.active == false )
                continue;

            if ( hotPoint.pressDown ) {
                exUIControl curCtrl = hotPoint.hover;
                if ( hotPoint.pressed != null && hotPoint.pressed.grabMouseOrTouch ) {
                    curCtrl = hotPoint.pressed;
                }

                // send press down event
                if ( curCtrl != null ) {
                    exUIPointInfo pointInfo = new exUIPointInfo();
                    pointInfo.id = hotPoint.id;
                    pointInfo.pos = hotPoint.pos;
                    pointInfo.delta = hotPoint.delta;
                    pointInfo.worldPos = hotPoint.worldPos;
                    pointInfo.worldDelta = hotPoint.worldDelta;

                    exUIPointEvent pointEvent = new exUIPointEvent();
                    pointEvent.isMouse = hotPoint.isMouse;
                    pointEvent.pointInfos = new exUIPointInfo [] {
                        pointInfo
                    };

                    curCtrl.OnPressDown(pointEvent);
                }
                hotPoint.pressed = curCtrl;
            }
        }

        // ======================================================== 
        // handle moving before press-up
        // ======================================================== 

        Dictionary<exUIControl, List<exHotPoint>> moveEvents = new Dictionary<exUIControl, List<exHotPoint>>();

        // collect press move event
        for ( int i = 0; i < _hotPoints.Length; ++i ) {
            exHotPoint hotPoint = _hotPoints[i];

            if ( hotPoint.active == false )
                continue;

            if ( hotPoint.delta != Vector2.zero ) {
                exUIControl curCtrl = hotPoint.hover;
                if ( hotPoint.pressed != null && hotPoint.pressed.grabMouseOrTouch ) {
                    curCtrl = hotPoint.pressed;
                }

                if ( curCtrl != null ) {
                    List<exHotPoint> hotPointList = null;
                    if ( moveEvents.ContainsKey(curCtrl) ) {
                        hotPointList = moveEvents[curCtrl];
                    }
                    else {
                        hotPointList = new List<exHotPoint>();
                        moveEvents.Add( curCtrl, hotPointList );
                    }
                    hotPointList.Add(hotPoint);
                }
            }
        }

        // send hot-point move event
        foreach (KeyValuePair<exUIControl, List<exHotPoint>> iter in moveEvents ) {

            exUIPointEvent pointEvent = new exUIPointEvent();
            pointEvent.pointInfos = new exUIPointInfo [iter.Value.Count];

            for ( int i = 0; i < iter.Value.Count; ++i ) {
                exHotPoint hotPoint = iter.Value[i];

                exUIPointInfo pointInfo = new exUIPointInfo();
                pointInfo.id = hotPoint.id;
                pointInfo.pos = hotPoint.pos;
                pointInfo.delta = hotPoint.delta;
                pointInfo.worldPos = hotPoint.worldPos;
                pointInfo.worldDelta = hotPoint.worldDelta;

                pointEvent.pointInfos[i] = pointInfo;
                pointEvent.isMouse = hotPoint.isMouse;
            }

            iter.Key.OnHoverMove (pointEvent);
        }

        // ======================================================== 
        // handle press up event 
        // ======================================================== 

        for ( int i = 0; i < _hotPoints.Length; ++i ) {
            exHotPoint hotPoint = _hotPoints[i];

            if ( hotPoint.active == false )
                continue;

            // 
            if ( hotPoint.pressUp ) {
                exUIPointInfo pointInfo = new exUIPointInfo();
                pointInfo.id = hotPoint.id;
                pointInfo.pos = hotPoint.pos;
                pointInfo.delta = hotPoint.delta;
                pointInfo.worldPos = hotPoint.worldPos;
                pointInfo.worldDelta = hotPoint.worldDelta;

                exUIPointEvent pointEvent = new exUIPointEvent();
                pointEvent.isMouse = hotPoint.isMouse;
                pointEvent.pointInfos = new exUIPointInfo [] {
                    pointInfo
                };

                exUIControl curCtrl = hotPoint.hover;
                if ( hotPoint.pressed != null && hotPoint.pressed.grabMouseOrTouch ) {
                    curCtrl = hotPoint.pressed;
                }

                // send press down event
                if ( curCtrl != null ) {
                    curCtrl.OnPressUp(pointEvent);

                    if ( hotPoint.isTouch )
                        curCtrl.OnHoverOut(pointEvent);
                }

                hotPoint.pressed = null;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void HandleTouches () {
		for ( int i = 0; i < Input.touchCount; ++i ) {
			Touch touch = Input.GetTouch(i);

            if ( touch.fingerId >= 10 )
                continue;

            exHotPoint hotPoint = touchPoints[touch.fingerId];
            hotPoint.active = true;

            // we need clear all internal state when hotpoint is de-active
            if ( hotPoint.active == false ) {
                hotPoint.Reset();
            }
            else {
                hotPoint.pos = touch.position;
                hotPoint.delta = touch.deltaPosition;

                Vector3 lastWorldPos = camera.ScreenToWorldPoint(touch.position - touch.deltaPosition);
                hotPoint.worldPos = camera.ScreenToWorldPoint(touch.position);
                hotPoint.worldDelta = hotPoint.worldPos - lastWorldPos;

                hotPoint.pressDown = (touch.phase == TouchPhase.Began);
                hotPoint.pressUp = (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended);
            }
        }

        HandleHotPoints ( touchPoints, false );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void HandleMouse () {
        if ( simulateMouseAsTouch ) {
            for ( int i = 0; i < 3; ++i ) {
                exHotPoint hotPoint = touchPoints[i];
                hotPoint.id = i;

                // check if active the hotPoint
                hotPoint.active = Input.GetMouseButtonDown(i) || Input.GetMouseButton(i) || Input.GetMouseButtonUp(i);

                // we need clear all internal state when hotpoint is de-active
                if ( hotPoint.active == false ) {
                    hotPoint.Reset();
                }
                else {
                    Vector2 lastMousePos = hotPoint.pos;
                    hotPoint.pos = Input.mousePosition;
                    if ( Input.GetMouseButtonDown(i) )
                        hotPoint.delta = Vector2.zero;
                    else
                        hotPoint.delta = hotPoint.pos - lastMousePos;

                    Vector3 lastMouseWorldPos = hotPoint.worldPos;
                    hotPoint.worldPos = camera.ScreenToWorldPoint(Input.mousePosition);
                    hotPoint.worldDelta = hotPoint.worldPos - lastMouseWorldPos;

                    hotPoint.pressDown = Input.GetMouseButtonDown(i);
                    hotPoint.pressUp = Input.GetMouseButtonUp(i);
                }
            }

            HandleHotPoints ( touchPoints, false );
        }
        else {
            for ( int i = 0; i < 3; ++i ) {
                exHotPoint hotPoint = mousePoints[i];

                // check if active the hotPoint
                hotPoint.active = true;

                // we need clear all internal state when hotpoint is de-active
                if ( hotPoint.active == false ) {
                    hotPoint.Reset();
                }
                else {
                    Vector2 lastMousePos = hotPoint.pos;
                    hotPoint.pos = Input.mousePosition;
                    hotPoint.delta = hotPoint.pos - lastMousePos;

                    Vector3 lastMouseWorldPos = hotPoint.worldPos;
                    hotPoint.worldPos = camera.ScreenToWorldPoint(Input.mousePosition);
                    hotPoint.worldDelta = hotPoint.worldPos - lastMouseWorldPos;

                    hotPoint.pressDown = Input.GetMouseButtonDown(i);
                    hotPoint.pressUp = Input.GetMouseButtonUp(i);
                }
            }

            HandleHotPoints ( mousePoints, true );
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    exUIControl PickControl ( Vector2 _screenPos ) {
        // pick 2D controls
        Vector3 worldPointerPos = camera.ScreenToWorldPoint ( _screenPos );
        controls.Sort(controlSorterByPrioirty);
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
            hitControls.Sort(controlSorterByLevel);
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

            // Touch-Point State
            if ( hasTouch || simulateMouseAsTouch ) {
                for ( int i = 0; i < touchPoints.Length; ++i ) {
                    exHotPoint hotPoint = touchPoints[i];

                    if ( hotPoint.active == false )
                        continue;

                    GUILayout.Label( "Touch[" + i + "]" );
                    GUILayout.BeginHorizontal ();
                        GUILayout.Space (15);
                        GUILayout.BeginVertical ();
                            GUILayout.Label( "pos: " + hotPoint.pos.ToString() );
                            GUILayout.Label( "hover: " + (hotPoint.hover ? hotPoint.hover.name : "None") );
                            GUILayout.Label( "pressed: " + (hotPoint.pressed ? hotPoint.pressed.name : "None") );
                        GUILayout.EndVertical ();
                    GUILayout.EndHorizontal ();
                }
            }

            // Mouse-Point State
            if ( hasMouse && simulateMouseAsTouch == false ) {
                GUILayout.Label( "Mouse" );
                GUILayout.BeginHorizontal ();
                    GUILayout.Space (15);
                    GUILayout.BeginVertical ();
                        GUILayout.Label( "pos: " + mousePoints[0].pos.ToString() );
                        GUILayout.Label( "hover: " + (mousePoints[0].hover ? mousePoints[0].hover.name : "None") );
                        GUILayout.Label( "left-pressed: " + (mousePoints[0].pressed ? mousePoints[0].pressed.name : "None") );
                        GUILayout.Label( "right-pressed: " + (mousePoints[1].pressed ? mousePoints[1].pressed.name : "None") );
                        GUILayout.Label( "middle-pressed: " + (mousePoints[2].pressed ? mousePoints[2].pressed.name : "None") );
                    GUILayout.EndVertical ();
                GUILayout.EndHorizontal ();
            }

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
