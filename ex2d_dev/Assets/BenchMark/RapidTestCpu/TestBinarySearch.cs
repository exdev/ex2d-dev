using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#pragma warning disable 0219
#pragma warning disable 0162

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

        List<exSpriteAnimationClip.EventInfo> eventList = new List<exSpriteAnimationClip.EventInfo>(eventCount);
        Dictionary<int, exSpriteAnimationClip.EventInfo> eventDict = new Dictionary<int, exSpriteAnimationClip.EventInfo>(eventCount);
        for (int i = 0; i < eventCount; i++) {
            var e = new exSpriteAnimationClip.EventInfo();
            e.frame = i;
            eventList.Add(e);
            eventDict.Add(i, e);
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
                        exSpriteAnimationClip.EventInfo.SearchComparer.BinarySearch(eventList, j);
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
                            if (eventList[t].frame == j) {
                                break;
                            }
                        }
                    }
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("FindIndex...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < eventCount; j++) {
                        eventList.FindIndex((x) => x.frame == j);
                    }
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Dict...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < eventCount; j++) {
                        exSpriteAnimationClip.EventInfo e;
                        eventDict.TryGetValue(j, out e);
                    }
                }
            }
        );
    }
}

#pragma warning restore 0162
#pragma warning restore 0219