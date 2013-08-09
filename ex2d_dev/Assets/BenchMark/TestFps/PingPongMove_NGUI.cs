//#define BENCHMARK_NGUI

using UnityEngine;
using System.Collections;

public class PingPongMove_NGUI : PingPongMoveBase {
#if BENCHMARK_NGUI
    protected new void Start() {
        ani = null;
        sprite = GetComponent<UISprite>();
        base.Start();
    }
    protected override void ShowHide() {
        sprite.enabled = !sprite.enabled;
    }
#endif
}