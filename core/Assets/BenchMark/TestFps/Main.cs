using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Main : EasyProfiler {

    public string menuSceneName;
    private static Main instance_;
    public static Main instance {
        get {
            if (instance_ == null) {
                Debug.LogError("No instance of Main, you should only play scene: Launch");
            }
            return instance_;
        }
    }
    public float defaultTestTime = 5;

    protected new void Awake() {
        base.Awake ();
        instance_ = this;
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
