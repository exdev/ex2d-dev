using UnityEngine;
using System.Collections;

public class PingPongMove : MonoBehaviour {
    static Rect screenEdge;

    public float speed = 1.0f;
    public float randomStop = 0.0f;
    public float randomShowHide = 0.0f;
    private Vector3 step;
    exSpriteAnimation ani;

	void Start () {
        ani = GetComponent<exSpriteAnimation>();
        if (Menu.setted) {
            randomShowHide = Menu.showhide;
            randomStop = Menu.stopmove;
            ani.enabled = Menu.enableAni;
            speed = Menu.speed;
        }
        step = Random.onUnitSphere;
        step.z = 0;
        step.Normalize();
        step *= speed;
        screenEdge = new Rect(-Screen.width * 0.5f, -Screen.height * 0.5f, Screen.width, Screen.height);
	}
	
	void Update () {
        bool flip = Menu.testMeshBuffer;
        if (flip || (randomShowHide != 0 && Random.value < randomShowHide * Time.deltaTime)) {
            ani.sprite.enabled = !ani.sprite.enabled;
        }
        if (randomStop != 0) {
            if (Random.value < randomStop * Time.deltaTime) {
                ani.enabled = !ani.enabled;
            }
        }
        Vector3 newPos = transform.position + step * Time.deltaTime;
        if (newPos.x <= screenEdge.xMin || newPos.x >= screenEdge.xMax) {
            step.x = -step.x;
        }
        else if (newPos.y <= screenEdge.yMin || newPos.y >= screenEdge.yMax) {
            step.y = -step.y;
        }
        else {
            transform.position = newPos;
        }
	}
}
