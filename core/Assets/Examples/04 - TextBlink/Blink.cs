using UnityEngine;
using System.Collections;

public class Blink : MonoBehaviour {

    exSpriteFontOverlap blinkEffect;
    float timer = 0.0f;

	// Use this for initialization
	void Awake () {
        blinkEffect = GetComponent<exSpriteFontOverlap>();
	}
	
	// Update is called once per frame
	void Update () {
        if ( Input.GetKeyDown("1") ) {
            timer = 0.0f;
        }

        float t = timer/0.4f;
        if ( t >= 1.0f ) {
            t = 1.0f;
        }

        float ratio = exEase.ExpoOut(t);
        blinkEffect.overlap.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f-ratio );

        timer += Time.deltaTime;
	}

    void OnGUI () {
        GUI.Label ( new Rect( 10, 10, 200, 30 ), "Press 1 to blink the text" );
    }
}
