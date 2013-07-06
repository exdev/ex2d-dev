using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0219

public class RapidTestCpu : EasyProfiler {

    IEnumerator Start () {
        int loopCount = 1000 * 1000;
        loopCount = 10000 * 10000;
        //  loopCount = 1;

        yield return new WaitForSeconds(1);

        CpuProfiler("基准测试...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Test 1...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Test 2...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Test 3...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                }
            }
        );
    }
}

#pragma warning restore 0219