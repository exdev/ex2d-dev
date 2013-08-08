using UnityEngine;
using System.Collections;

public class DynamicScene0_ex2D : MonoBehaviour {
    public exSpriteAnimation ani;
    public int aniCount;

	// Use this for initialization
	IEnumerator Start () {
        if (Menu.setted) {
            aniCount = (int)Menu.count;
        }
        for (int i = 1; i < aniCount; ++i) {
            var s = ani.Instantiate(new Vector3((Random.value * 0.9f - 0.45f) * Screen.width, (Random.value * 0.9f - 0.45f) * Screen.height, 0.0f), Quaternion.Euler(0, 0, Random.Range(-180, 180)));
            ani.sprite.layer.Add(s.sprite);
            if (i == aniCount / 2) {
                yield return null;
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
