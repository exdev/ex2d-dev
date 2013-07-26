using UnityEngine;
using System.Collections;

#pragma warning disable 0219

public class TestMatrix : EasyProfiler {

    IEnumerator Start () {
        int loopCount = 1000 * 1000;
        //  loopCount = 1;

        Transform ct = transform;

        yield return new WaitForSeconds(1);

        CpuProfiler("基准测试...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    Vector3 pos = new Vector3(1, 1, 1);
                    pos = new Vector3(1, 1, 1);
                    pos = new Vector3(1, 1, 1);
                    pos = new Vector3(1, 1, 1);
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Test TransformPoint", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    Vector3 pos = new Vector3(1, 1, 1);
                    pos = ct.TransformPoint(pos);
                    pos = ct.TransformPoint(pos);
                    pos = ct.TransformPoint(pos);
                    pos = ct.TransformPoint(pos);
                    //Debug.Log(string.Format("[Start|RapidTestCpu] pos1: {0}", pos));
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Test TRS MultiplyPoint3x4", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    Vector3 pos = new Vector3(1, 1, 1);
                    Matrix4x4 mat = Matrix4x4.TRS(ct.position, ct.rotation, ct.lossyScale);
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    //Debug.Log(string.Format("[Start|RapidTestCpu] pos2: {0}", pos));
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Test localToWorldMatrix MultiplyPoint3x4", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    Vector3 pos = new Vector3(1, 1, 1);
                    Matrix4x4 mat = transform.localToWorldMatrix;
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    //Debug.Log(string.Format("[Start|RapidTestCpu] pos3: {0}", pos));
                }
            }
        );
        
        yield return new WaitForSeconds(1);

        CpuProfiler("Test cached localToWorldMatrix MultiplyPoint3x4", 
            () => {
                Matrix4x4 mat = transform.localToWorldMatrix;
                for (int i = 0; i < loopCount; ++i) {
                    Vector3 pos = new Vector3(1, 1, 1);
                    if (transform.hasChanged) {
                        mat = transform.localToWorldMatrix;
                        transform.hasChanged = false;
                    }
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    pos = mat.MultiplyPoint3x4(pos);
                    //Debug.Log(string.Format("[Start|RapidTestCpu] pos3: {0}", pos));
                }
            }
        );
    }
}

#pragma warning restore 0219