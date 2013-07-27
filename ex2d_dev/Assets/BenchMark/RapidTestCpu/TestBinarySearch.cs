using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0219

///////////////////////////////////////////////////////////////////////////////
//
/// Test template
//
///////////////////////////////////////////////////////////////////////////////

public class TestBinarySearch : EasyProfiler {

    public int eventCount = 100;

    void Update () { 
        if (Input.GetMouseButtonDown(0)) {
            eventCount += 10;
            Print(string.Format("[Update|TestBinarySearch] eventCount: {0}", eventCount));
            Test();
        }
    }

    IEnumerator Start () {
        int loopCount = 1000 * 1000;
        loopCount = 10000 * 10000;
        loopCount = 10000;

        List<exSpriteAnimationClip.EventInfo> events = new List<exSpriteAnimationClip.EventInfo>(eventCount);
        for (int i = 0; i < eventCount; i++) {
            var e = new exSpriteAnimationClip.EventInfo();
            e.frame = i;
            events.Add(e);
        }

        yield return new WaitForSeconds(1);

        CpuProfiler("基准测试...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < eventCount; j++) {
                    }
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("BinarySearch...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < eventCount; j++) {
                        exSpriteAnimationClip.EventInfo.SearchComparer.BinarySearch(events, j);
                    }
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("for...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < eventCount; j++) {
                        for (int t = 0; t < eventCount; t++) {
                            if (events[t].frame == j) {
                                break;
                            }
                        }
                    }
                }
            }
        );

        yield break;
        yield return new WaitForSeconds(1);

        CpuProfiler("FindIndex...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < eventCount; j++) {
                        events.FindIndex((x) => x.frame == j);
                    }
                }
            }
        );
    }
}

#pragma warning restore 0219