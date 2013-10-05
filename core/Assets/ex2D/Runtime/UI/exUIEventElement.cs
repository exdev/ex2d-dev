// ======================================================================================
// File         : exUIEventElement.cs
// Author       : Wu Jie 
// Last Change  : 10/05/2013 | 12:01:28 PM | Saturday,October
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

public class exUIEventElement : exUIEventTrigger {

    public event System.Action onHoverIn;
    public event System.Action onHoverOut;

}

