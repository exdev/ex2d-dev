﻿using UnityEngine;
using System.Collections;

public class PingPongMove_Unity: MonoBehaviour {
    static Rect screenEdge;

    public float speed = 1.0f;
    public float randomStop = 0.0f;
    public float randomShowHide = 0.0f;

    protected SpriteRenderer sprite;

    private Vector3 step;
    private bool moving = true;

	protected void Start () {
        // var exAni = GetComponent<Animation>();
        sprite = GetComponent<SpriteRenderer>();
        GetComponent<Animator>().enabled = Menu.enableAni;

        if (Menu.setted) {
            randomShowHide = Menu.showhide;
            randomStop = Menu.stopmove;
            //if (ani != null) {
            //    ani.enabled = Menu.enableAni;
            //}
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
        if (flip || (Random.value < randomShowHide * Time.deltaTime && randomShowHide != 0)) {
            ShowHide();
        }
        if (Random.value < randomStop * Time.deltaTime && randomStop != 0) {
            moving = !moving;
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


    protected void ShowHide() {
        sprite.enabled = !sprite.enabled;
    }
}

