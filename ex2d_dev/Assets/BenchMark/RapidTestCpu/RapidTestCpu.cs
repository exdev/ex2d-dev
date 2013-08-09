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

public class RapidTestCpu : EasyProfiler {
    
    public class RefInfo {
        public class SearchComparer : IComparer<RefInfo> {
            private static SearchComparer instance_;
            private static int frame;
            public static int BinarySearch (List<RefInfo> _list, int _frame) {
                frame = _frame;
                if (instance_ == null) {
                    instance_ = new SearchComparer();
                }
                return _list.BinarySearch(null, instance_);
            }
            public int Compare (RefInfo _x, RefInfo _y) {
                if (_x == null && _y == null) {
                    exDebug.Assert(false, "Failed to trigger current event because event list contains null event.", false);
                    return 0;
                }
                if (_x != null) {
                    if (_x.id > frame)
                        return 1;
                    else if (_x.id < frame)
                        return -1;
                    else
                        return 0;
                }
                else {
                    if (frame > _y.id)
                        return 1;
                    else if (frame < _y.id)
                        return -1;
                    else
                        return 0;
                }
            }
        }
        public int x = -1;                 ///< the x pos
        public char id = '\x0';                ///< the character id 
        public int y = -1;                 ///< the y pos
        //public int width = -1;             ///< the width
        //public int height = -1;            ///< the height                          
        //public int xoffset = -1;           ///< the xoffset
        //public int yoffset = -1;           ///< the yoffset
        //public int xadvance = -1;          ///< the xadvance
        //public bool rotated = false;

        public RefInfo () {}
        public RefInfo ( RefInfo _c ) {
            id = _c.id;
            x = _c.x;
            y = _c.y;
            //width = _c.width;
            //height = _c.height;
            //xoffset = _c.xoffset;
            //yoffset = _c.yoffset;
            //xadvance = _c.xadvance;
            //rotated = _c.rotated;
        }
    }

    public struct ValInfo {
        public class SearchComparer : IComparer<ValInfo> {
            private static SearchComparer instance_;
            private static int frame;
            public static int BinarySearch (List<ValInfo> _list, int _frame) {
                frame = _frame;
                if (instance_ == null) {
                    instance_ = new SearchComparer();
                }
                var v = new ValInfo();
                v.id = -1;
                return _list.BinarySearch(v, instance_);
            }
            public int Compare (ValInfo _x, ValInfo _y) {
                if (_x.id == -1 && _y.id == -1) {
                    exDebug.Assert(false, "Failed to trigger current event because event list contains null event.", false);
                    return 0;
                }
                if (_x.id != -1) {
                    if (_x.id > frame)
                        return 1;
                    else if (_x.id < frame)
                        return -1;
                    else
                        return 0;
                }
                else {
                    if (frame > _y.id)
                        return 1;
                    else if (frame < _y.id)
                        return -1;
                    else
                        return 0;
                }
            }
        }
        public int x;                 ///< the x pos
        public int id;                ///< the character id 
        public int y;                 ///< the y pos
        //public int width;             ///< the width
        //public int height;            ///< the height                          
        //public int xoffset;           ///< the xoffset
        //public int yoffset;           ///< the yoffset
        //public int xadvance;          ///< the xadvance
        //public bool rotated;
    }

    const int Count = 2000;
    List<ValInfo> valInfoList = new List<ValInfo>(Count);
    List<RefInfo> refInfoList = new List<RefInfo>(Count);
    Dictionary<char, RefInfo> refInfoDict = new Dictionary<char, RefInfo>(Count);
    Dictionary<int, ValInfo> valInfoDict = new Dictionary<int, ValInfo>(Count);

    IEnumerator Start () {
        int loopCount = 1000 * 1000;
        loopCount = 10000 * 10000;
        loopCount = 10000;
        

        for (char i = '\x0'; i < Count; i++) {
            var e = new RefInfo();
            e.id = i;
            refInfoList.Add(e);
            refInfoDict[i] = e;
        }

        for (int i = 0; i < Count; i++) {
            var e = new ValInfo();
            e.id = i;
            valInfoList.Add(e);
            valInfoDict[i] = e;
        }

        yield return new WaitForSeconds(1);

        CpuProfiler("基准测试...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < Count; j++) {
                    }
                }
            }
        );

        //yield return new WaitForSeconds(1);

        //CpuProfiler("BinarySearch ref...", 
        //    () => {
        //        for (int i = 0; i < loopCount; ++i) {
        //            for (int j = 0; j < Count; j++) {
        //                RefInfo.SearchComparer.BinarySearch(refInfoList, j);
        //            }
        //        }
        //    }
        //);

        //yield return new WaitForSeconds(1);

        //CpuProfiler("BinarySearch val...", 
        //    () => {
        //        for (int i = 0; i < loopCount; ++i) {
        //            for (int j = 0; j < Count; j++) {
        //                ValInfo.SearchComparer.BinarySearch(valInfoList, j);
        //            }
        //        }
        //    }
        //);

        yield return new WaitForSeconds(1);

        CpuProfiler("Dict ref...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (char j = '\x0'; j < Count; j++) {
                        RefInfo res;
                        refInfoDict.TryGetValue(j, out res);
                    }
                }
            }
        );

        yield return new WaitForSeconds(1);

        CpuProfiler("Dict val...", 
            () => {
                for (int i = 0; i < loopCount; ++i) {
                    for (int j = 0; j < Count; j++) {
                        ValInfo res;
                        valInfoDict.TryGetValue(j, out res);
                    }
                }
            }
        );
    }
}

#pragma warning restore 0162
#pragma warning restore 0219