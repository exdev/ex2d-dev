//#define BENCHMARK_TK2D

using UnityEngine;
using System.Collections;

public class DynamicScene0_TK2D : MonoBehaviour {
#if BENCHMARK_TK2D
    public tk2dSpriteAnimator ani;
#endif
    public int aniCount;

#if BENCHMARK_TK2D
	// Use this for initialization
	IEnumerator Start () {
        if (Menu.setted) {
            aniCount = (int)Menu.count;
        }
        for (int i = 1; i < aniCount; ++i) {
            ani.Instantiate(new Vector3((Random.value * 0.9f - 0.45f) * Screen.width, (Random.value * 0.9f - 0.45f) * Screen.height, 0.0f), Quaternion.Euler(0, 0, Random.Range(-180, 180)));
            if (i == aniCount / 2) {
                yield return null;
            }
        }
	}
#endif
}
