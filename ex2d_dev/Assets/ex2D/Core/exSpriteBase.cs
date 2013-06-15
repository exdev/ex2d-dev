// ======================================================================================
// File         : exSpriteBase.cs
// Author       : 
// Last Change  : 06/15/2013 | 09:51:27 AM | Saturday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;

///////////////////////////////////////////////////////////////////////////////
///
/// The sprite base component
///
///////////////////////////////////////////////////////////////////////////////

public class exSpriteBase : MonoBehaviour {

    internal int layerCachedIndex;

    private exLayer _layer;
    internal exLayer layer {
        get {
            return _layer;
        }
        set {
            if (_layer == value) {
                return;
            }
            if (_layer) {
                _layer.Remove(this);
            }
            _layer = value;
            if (value) {
                value.Add(this);
            }
        }
    }

	void Start () {
	
	}
	
	void Update () {
	
	}
}
