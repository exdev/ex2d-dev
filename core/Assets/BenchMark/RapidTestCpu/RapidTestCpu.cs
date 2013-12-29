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

public class RapidTestCpu : EasyProfiler
{

    interface ITest {
        int virtualProperty { get; }
    }

    public class myTest : ITest
    {
        public int value = 1;
        public int virtualProperty {
            get {
                return value;
            }
        }
        public int normalProperty {
            get {
                return value;
            }
        }
    }

    IEnumerator Start () {
        int loopCount = 1000 * 1000;
        loopCount = 10000 * 100000;
        //loopCount = 10000;

        var test = new myTest();
        var iTest = test as ITest;
        int sum = 0;
        
        yield return new WaitForSeconds(1);

        CpuProfilerBase(
            () => {
                for (int i = 0; i < loopCount; ++i) {
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("value...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    sum = test.value;
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("normalProperty...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    sum = test.normalProperty;
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("virtualProperty...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    sum = iTest.virtualProperty;
                }
            }
        );
    }
}

#pragma warning restore 0162
#pragma warning restore 0219