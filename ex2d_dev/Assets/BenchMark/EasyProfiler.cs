using UnityEngine;
using System.Collections;

public class EasyProfiler : MonoBehaviour {

    public float eachTestTime = 5;

    private int beginFrame;
    private float beginTime;

    string testName;

    protected void Print (string _info) {
        exDebugHelper.ScreenLog(_info, exDebugHelper.LogType.Normal, null, false);
    }
    protected void Print (string _format, params object[] _args) {
        Print(string.Format(_format, _args));
    }
    protected void RenderProfilerBegin (string _testName) {
        Print(_testName);
        beginFrame = Time.frameCount;
        beginTime = Time.realtimeSinceStartup;
    }
    protected void RenderProfilerEnd () {
        float elapse = Time.realtimeSinceStartup - beginTime;
        int frameCount = Time.frameCount - beginFrame;
        Print("{0}秒运行了{1}帧 FPS: {2}", elapse, frameCount, frameCount / elapse);
    }
    protected void CpuProfilerBegin (string _testName) {
        testName = _testName;
        Print("开始执行" + testName);
        beginTime = Time.realtimeSinceStartup;
    }
    protected void CpuProfilerEnd () {
        float elapse = Time.realtimeSinceStartup - beginTime;
        Print("完成{0}, 用时 {1} 秒", testName, elapse);
    }
}
