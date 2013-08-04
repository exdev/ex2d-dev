using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : EasyProfiler {

    public string menuSceneName;
    public static Main instance;
    public float defaultTestTime = 5;

    protected override void Awake() {
        base.Awake ();
        instance = this;
    }
	// Use this for initialization
	void Start () {
        Object.DontDestroyOnLoad(gameObject);
        BackToMenu ();
	}
    IEnumerator<YieldInstruction> Test (float _time) {

        yield return new WaitForSeconds(1);
        Application.targetFrameRate = 6000;
        yield return null;

        RenderProfilerBegin ("Testing");

        yield return new WaitForSeconds(_time);

        RenderProfilerEnd ();

        ShowReturnToMenu (true);
    }
    public static void BackToMenu () {
        instance.ShowReturnToMenu (false);
        Application.LoadLevel (instance.menuSceneName);
        instance.ClearScreen ();
    }
    public static void RunTest (string _sceneName) {
        instance.DoRunTest (_sceneName);
    }
    public void DoRunTest (string _sceneName) {
        Application.LoadLevel (_sceneName);
        StartCoroutine (Test(defaultTestTime));
    }
}
