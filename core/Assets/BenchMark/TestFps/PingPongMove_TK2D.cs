//#define BENCHMARK_TK2D

using UnityEngine;
using System.Collections;

public class PingPongMove_TK2D : PingPongMoveBase {
#if BENCHMARK_TK2D
    protected new void Start() {
        var tkAni = GetComponent<tk2dSpriteAnimator>();
        ani = tkAni;
        sprite = tkAni.Sprite;
        base.Start();
    }
    protected override void ShowHide() {
        renderer.enabled = !renderer.enabled;
    }
#endif
}