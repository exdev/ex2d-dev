using UnityEngine;
using System.Collections;

internal class exReleaseFlag {

#if EX_DEBUG

    public const HideFlags hideAndDontSave = HideFlags.DontSave;
    public const HideFlags notEditable = (HideFlags)0;

#else

    public const HideFlags hideAndDontSave = HideFlags.HideAndDontSave;
    public const HideFlags notEditable = HideFlags.NotEditable;

#endif

}
