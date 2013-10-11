// ======================================================================================
// File         : exUIButton.cs
// Author       : Wu Jie 
// Last Change  : 10/11/2013 | 15:58:38 PM | Friday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIButton : exUIControl {

    // click
    public event System.Action<exUIControl> onClick;
    public void Send_OnClick () { if ( onClick != null ) onClick (this); }
    public List<SlotInfo> onClickSlots = new List<SlotInfo>();

    // button-down
    public event System.Action<exUIControl> onButtonDown;
    public void Send_OnButtonDown () { if ( onButtonDown != null ) onButtonDown (this); }
    public List<SlotInfo> onButtonDownSlots = new List<SlotInfo>();

    // button-up
    public event System.Action<exUIControl> onButtonUp;
    public void Send_OnButtonUp () { if ( onButtonUp != null ) onButtonUp (this); }
    public List<SlotInfo> onButtonUpSlots = new List<SlotInfo>();

    //
    bool pressing = false;
    Vector2 pressDownAt = Vector2.zero; 

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();

        AddSlotsToEvent ( "onClick",       onClickSlots,       new System.Type[] { typeof(exUIControl) }, typeof(System.Action<exUIControl>) );
        AddSlotsToEvent ( "onButtonDown",  onButtonDownSlots,  new System.Type[] { typeof(exUIControl) }, typeof(System.Action<exUIControl>) );
        AddSlotsToEvent ( "onButtonUp",    onButtonUpSlots,    new System.Type[] { typeof(exUIControl) }, typeof(System.Action<exUIControl>) );

        onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
            // only accept on hot-point
            if ( pressing )
                return;

            if ( (_point.isMouse == false) || _point.isMouse && _point.id == 0 ) {
                pressing = true;
                pressDownAt = _point.pos;

                exUIMng.inst.SetFocus(this);
                Send_OnButtonDown ();
            }
        };

        onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
            Send_OnButtonUp ();

            if ( pressing ) {
                pressing = false;
                Send_OnClick ();
            }
        };

        onHoverOut += delegate ( exUIControl _sender, exHotPoint _point ) {
            pressing = false;
        };
    }
}
