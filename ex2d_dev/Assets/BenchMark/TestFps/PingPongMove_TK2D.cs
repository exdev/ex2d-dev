using UnityEngine;
using System.Collections;


public class PingPongMove_TK2D : PingPongMoveBase {
    protected new void Start() {
        var tkAni = GetComponent<tk2dSpriteAnimator>();
        ani = tkAni;
        sprite = tkAni.Sprite;
        base.Start();
    }
    protected override void ShowHide() {
        renderer.enabled = !renderer.enabled;
    }
}