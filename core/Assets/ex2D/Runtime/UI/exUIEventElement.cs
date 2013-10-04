// ======================================================================================
// File         : exUIEventElement.cs
// Author       : Wu Jie 
// Last Change  : 10/04/2013 | 11:48:44 AM | Friday,October
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

public class exUIEventElement : exUIEventDispatcher {
    public List<MessageInfo> hoverInSlots   = new List<MessageInfo>();
    public List<MessageInfo> hoverOutSlots  = new List<MessageInfo>();
    // public List<MessageInfo> pressSlots     = new List<MessageInfo>();
    // public List<MessageInfo> releaseSlots   = new List<MessageInfo>();
    // public List<MessageInfo> moveSlots   = new List<MessageInfo>();
}

