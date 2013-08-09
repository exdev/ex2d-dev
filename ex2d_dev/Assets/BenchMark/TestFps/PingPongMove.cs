using UnityEngine;
using System.Collections;

public class PingPongMoveBase : MonoBehaviour {
    static Rect screenEdge;

    public float speed = 1.0f;
    public float randomStop = 0.0f;
    public float randomShowHide = 0.0f;

    protected MonoBehaviour ani;
    protected MonoBehaviour sprite;

    private Vector3 step;
    private bool moving = true;

	protected void Start () {
        if (Menu.setted) {
            randomShowHide = Menu.showhide;
            randomStop = Menu.stopmove;
            if (ani != null) {
                ani.enabled = Menu.enableAni;
            }
            speed = Menu.speed;
        }
        step = Random.onUnitSphere;
        step.z = 0;
        step.Normalize();
        step *= speed;
        screenEdge = new Rect(-Screen.width * 0.5f, -Screen.height * 0.5f, Screen.width, Screen.height);
	}
    protected virtual void ShowHide() {
       sprite.enabled = !sprite.enabled;
    }
	void Update () {
        bool flip = Menu.testMeshBuffer;
        if (flip || (randomShowHide != 0 && Random.value < randomShowHide * Time.deltaTime)) {
            ShowHide();
        }
        if (randomStop != 0) {
            if (Random.value < randomStop * Time.deltaTime) {
                moving = !moving;
            }
        }
        if (moving && (step.x != 0 || step.y != 0)) {
            Vector3 newPos = transform.localPosition + step * Time.deltaTime;
            if (newPos.x <= screenEdge.xMin || newPos.x >= screenEdge.xMax) {
                step.x = -step.x;
            }
            else if (newPos.y <= screenEdge.yMin || newPos.y >= screenEdge.yMax) {
                step.y = -step.y;
            }
            else {
                transform.localPosition = newPos;
            }
        }
	}
}

public class PingPongMove : PingPongMoveBase {
    protected new void Start() {
        var exAni = GetComponent<exSpriteAnimation>();
        ani = exAni;
        sprite = exAni.sprite;
        base.Start();
    }
}