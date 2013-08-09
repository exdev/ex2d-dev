//#define BENCHMARK_NGUI

using UnityEngine;
using System.Collections;

public class DynamicScene0_NGUI : MonoBehaviour {
#if BENCHMARK_NGUI
    public UISprite ani;
#endif
    public int aniCount;

#if BENCHMARK_NGUI
	// Use this for initialization
	IEnumerator Start () {
        if (Menu.setted) {
            aniCount = (int)Menu.count;
        }
        for (int i = 1; i < aniCount; ++i) {
            var sprite = ani.Instantiate(Vector3.zero, Quaternion.Euler(0, 0, Random.Range(-180, 180)));
            sprite.transform.parent = ani.transform.parent;
            sprite.transform.localPosition = new Vector3((Random.value * 0.9f - 0.45f) * Screen.width, (Random.value * 0.9f - 0.45f) * Screen.height, 0.0f);
            sprite.MakePixelPerfect();
            if (i == aniCount / 2) {
                yield return null;
            }
        }
	}
#endif
}
