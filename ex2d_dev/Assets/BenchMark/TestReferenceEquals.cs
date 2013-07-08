using UnityEngine;
using System.Collections;

public class TestReferenceEquals : EasyProfiler {

    IEnumerator Start () {
        GameObject UnityObjA = new GameObject();
        UnityObjA.hideFlags = HideFlags.DontSave;
        GameObject UnityObjB = new GameObject();
        UnityObjB.hideFlags = HideFlags.DontSave;
        
        Debug.Log(string.Format("[Start|TestReferenceEqual] UnityObjA == UnityObjB: {0}", UnityObjA == UnityObjB));
        Debug.Log(string.Format("[Start|TestReferenceEqual] ReferenceEquals(UnityObjA, UnityObjB): {0}", ReferenceEquals(UnityObjA, UnityObjB)));

        yield return new WaitForSeconds(1);

        CpuProfilerBegin("»ù×¼²âÊÔ...");
        for (int i = 0; i < 10000 * 1000; ++i) {
            ;	
        }
        CpuProfilerEnd();

        yield return new WaitForSeconds(1);

        bool equal = true;
        CpuProfilerBegin("==²âÊÔ...");
        for (int i = 0; i < 10000 * 1000; ++i) {
            equal = (UnityObjA == UnityObjB);
        }
        CpuProfilerEnd();

        yield return new WaitForSeconds(1);

        CpuProfilerBegin("ReferenceEquals²âÊÔ...");
        for (int i = 0; i < 10000 * 1000; ++i) {
            equal = ReferenceEquals(UnityObjA, UnityObjB);
        }
        CpuProfilerEnd();

        if (equal) {
        }

        UnityObjA.Destroy();
        UnityObjB.Destroy();
    }
}
