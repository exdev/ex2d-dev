using UnityEngine;
using System.Collections;

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
