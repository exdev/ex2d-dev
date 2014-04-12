using UnityEngine;
using System.Collections;

public class EasyProfiler : MonoBehaviour {

    private int beginFrame;
    private float beginTime;
    private float baseTime;

    string testName;
    bool showReturnToMenu = false;

    protected void Awake() {
        useGUILayout = false;
        Application.targetFrameRate = 6000;
        QualitySettings.vSyncCount = 0;
    }

    public void Print (string _info) {
        exDebugHelper.ScreenLog(_info, exDebugHelper.LogType.Normal, null, false);
    }
    public void Print (string _format, params object[] _args) {
        Print(string.Format(_format, _args));
    }
    public void ClearScreen () {
        exDebugHelper.ClearScreen ();
    }

    public void RenderProfilerBegin (string _testName) {
        Print(_testName);
        beginFrame = Time.frameCount;
        beginTime = Time.realtimeSinceStartup;
    }
    public void RenderProfilerEnd () {
        float elapse = Time.realtimeSinceStartup - beginTime;
        int frameCount = Time.frameCount - beginFrame;
        Print("{0}秒运行了{1}帧 FPS: {2}", elapse, frameCount, frameCount / elapse);
    }

    public void CpuProfilerBegin (string _testName) {
        testName = _testName;
        Print("开始执行" + testName);
        beginTime = Time.realtimeSinceStartup;
    }
    public void CpuProfilerEnd (bool baseTest = false) {
        float elapse = Time.realtimeSinceStartup - beginTime;
        if (baseTest) {
            baseTime = elapse;
            Print("完成基准测试, 用时 {0} 秒", baseTime);
        }
        else {
            Print("完成{0}, 用时 {1} 秒，比基准多出 {2} 秒", testName, elapse, elapse - baseTime);
        }
    }
    public void CpuProfiler (string _testName, System.Action _testFunc) {
        CpuProfilerBegin(_testName);
        _testFunc();
        CpuProfilerEnd();
    }
    public void CpuProfilerBase (System.Action _testFunc) {
        CpuProfilerBegin("基准测试...");
        _testFunc();
        CpuProfilerEnd(true);
    }

    [ContextMenu("Test")]
    public virtual void Test () {
        StartCoroutine("Start");
    }

    void OnGUI() {
        if (showReturnToMenu) {
            if (GUI.Button(new Rect(20, 300, 200, 50), "Return To Menu")) {
                Main.BackToMenu();
            }
        }
    }
    public void ShowReturnToMenu (bool _show) {
        showReturnToMenu = _show;
    }
}
