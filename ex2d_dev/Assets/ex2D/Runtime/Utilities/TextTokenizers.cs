// ======================================================================================
// File         : TextTokenizers.cs
// Author       : Jare
// Last Change  : 07/30/2013 | 17:13:54
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

static class L2RTokenizer
{
    public const string breakableSpaces = "\u0020\u0009\u000B\u000C\u00A0\u0085\u3000\u1680\u180E\u2000\u2001\u2002\u2003\u2004\u2005\u2006\u2007\u2008\u2009\u200A\u202F\u205F\u2028\u2029";
    //from http://www.unicode.org/reports/tr14/
    const string arabicNonStarterChars = "\u061B\u061E\u061F\u06D4\uFD3F";
    const string arabicNonEnderChars = "\uFD3E";//‘﴾’
    //from MS Word
    public const string otherNonStarterChars = @"!%),.:;>?]}¢¨°·ˇˉ―‖’”…‰′″›℃∶、。〃〉》」』】〕〗〞︶︺︾﹀﹄﹚﹜﹞！＂％＇），．：；？］｀｜｝～￠";
    public const string otherNonEnderChars = @"$([{£¥·‘“〈《「『【〔〖〝﹙﹛﹝＄（．［｛￡￥";
    ///nonStarterChars和nonEnderChars不应包含重复或相同的符号
    public static readonly char[] nonStarterChars = (breakableSpaces + otherNonStarterChars + arabicNonStarterChars).ToCharArray();
    public static readonly char[] nonEnderChars = (otherNonEnderChars + arabicNonEnderChars).ToCharArray();

    /// <summary> 缓存以避免GC </summary>
    static IEnumerable<string> cachedEnumerable;
    static string parsingText;

    /// <summary>
    /// 从左向右逐渐输出字串，以空格或标点为分隔，每次返回的字串都包含之前遍历过的内容。
    /// 该方法不可重入
    /// </summary>
    /// <example>foreach(var tryWrappedLine in text.GetL2RTokenizer())</example>
    public static IEnumerable<string> GetL2RTokenizer (this string text)
    {
        parsingText = text;
        if (cachedEnumerable == null) {
            cachedEnumerable = new L2RTokenizerEnumerator();
        }
        return cachedEnumerable;
    }
    //public static IEnumerable<string> GetL2RTokenizer(this string text)
    //{
    //    //Debug.Log("111111111111" + text);
    //    int lineEnd = text.IndexOf('\n');
    //    if (lineEnd >= 0) {
    //        text = text.Substring(0, lineEnd);
    //    }
    //    for (int nextStartIndex = 0; nextStartIndex < text.Length; ) {
    //        int onlyEndOfLine = text.IndexOfAny(nonStarterChars, nextStartIndex);
    //        int onlyStartOfLine = text.IndexOfAny(nonEnderChars, nextStartIndex);
    //        //Debug.Log("next " + onlyEndOfLine + " " + onlyStartOfLine);
    //        if (onlyStartOfLine > 0 && (onlyEndOfLine < 0 || onlyStartOfLine < onlyEndOfLine)) {
    //            nextStartIndex = onlyStartOfLine + 1;
    //            //Debug.Log("onlyStartOfLine " + onlyStartOfLine);
    //            //if (onlyStartOfLine == text.Length) {
    //            //    break;
    //            //}
    //            //else {
    //                yield return text.Substring(0, onlyStartOfLine);
    //            //}
    //        } else if (onlyEndOfLine >= 0/* && canEndOfLine > canStartOfLine*/) {
    //            onlyEndOfLine += 1;
    //            nextStartIndex = onlyEndOfLine;
    //            //Debug.Log("onlyEndOfLine " + onlyEndOfLine);
    //            //if (onlyEndOfLine == text.Length) {
    //            //    break;
    //            //}
    //            //else {
    //                yield return text.Substring(0, onlyEndOfLine);
    //            //}
    //        } else {
    //            //Debug.Log(22222222);
    //            break;
    //        }
    //    }
    //    //Debug.Log(333333333);
    //    yield return text;
    //}
    private sealed class L2RTokenizerEnumerator : IEnumerator, IDisposable, IEnumerator<string>, IEnumerable<string> {

        internal string current;
        internal int state;
        internal int lineEnd;
        internal int nextStartIndex;
        internal int onlyEndOfLine;
        internal int onlyStartOfLine;
        public void Dispose () {
            state = -1;
            current = null;
        }
        public bool MoveNext () {
            uint num = (uint)state;
            state = -1;
            switch (num) {
            case 0:
                lineEnd = L2RTokenizer.parsingText.IndexOf('\n');
                if (lineEnd >= 0) {
                    L2RTokenizer.parsingText = L2RTokenizer.parsingText.Substring(0, lineEnd);
                }
                nextStartIndex = 0;
                break;
            case 1:
            case 2:
                break;
            case 3:
                state = -1;
                return false;
            default:
                return false;
            }
            while (nextStartIndex < L2RTokenizer.parsingText.Length) {
                onlyEndOfLine = L2RTokenizer.parsingText.IndexOfAny(L2RTokenizer.nonStarterChars, nextStartIndex);
                onlyStartOfLine = L2RTokenizer.parsingText.IndexOfAny(L2RTokenizer.nonEnderChars, nextStartIndex);
                if ((onlyStartOfLine > 0) && ((onlyEndOfLine < 0) || (onlyStartOfLine < onlyEndOfLine))) {
                    nextStartIndex = onlyStartOfLine + 1;
                    current = L2RTokenizer.parsingText.Substring(0, onlyStartOfLine);
                    state = 1;
                }
                else {
                    if (onlyEndOfLine < 0) {
                        break;
                    }
                    onlyEndOfLine++;
                    nextStartIndex = onlyEndOfLine;
                    current = L2RTokenizer.parsingText.Substring(0, onlyEndOfLine);
                    state = 2;
                }
                return true;
            }
            current = L2RTokenizer.parsingText;
            state = 3;
            return true;
        }
        public void Reset () {
            current = null;
            state = 0;
            lineEnd = 0;
            nextStartIndex = 0;
            onlyEndOfLine = 0;
            onlyStartOfLine = 0;
        }
        string IEnumerator<string>.Current {
            get {
                return current;
            }
        }
        object IEnumerator.Current {
            get {
                return current;
            }
        }
        public IEnumerator<string> GetEnumerator () {
            Reset();
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }
    }
}

static class ChineseL2RTokenizer
{
    //static ChineseL2RTokenizer() {
    //    for (char c = '\u2E80'; c < '\uFFFF'; ++c) {
    //        if (IsChineseCharacter(c)) {
    //            if (L2RTokenizer.nonStarterChars.Contains(c)) {
    //                Debug.Log("[ChineseL2RTokenizer|ChineseL2RTokenizer] 111 " + (int)c);
    //                break;
    //            }
    //            if (L2RTokenizer.nonEnderChars.Contains(c)) {
    //                Debug.Log("[ChineseL2RTokenizer|ChineseL2RTokenizer] 222 ");
    //                break;
    //            }
    //        }
    //    }
    //}

    ///nonStarterChars和nonEnderChars不应包含重复或相同的符号
    public static readonly char[] nonStarterChars = (L2RTokenizer.breakableSpaces + L2RTokenizer.otherNonStarterChars).ToCharArray();
    public static readonly char[] nonEnderChars = L2RTokenizer.otherNonEnderChars.ToCharArray();

    private static bool IsChineseCharacter(char ch) {
        ///http://www.ipmtea.net/javascript/201009/23_294.html
        return  '\u2E80' <= ch && 
                (
                    ('\u4E00' <= ch && ch <= '\u9FBF') ||
                    ('\u2E80' <= ch && ch <= '\u2EFF') ||
                    ('\u2F00' <= ch && ch <= '\u2FDF') ||
                    //('\u3000' <= ch && ch <= '\u303F') || CJK 符号和标点 
                    ('\u31C0' <= ch && ch <= '\u31EF') ||
                    ('\u3200' <= ch && ch <= '\u32FF') ||
                    ('\u3300' <= ch && ch <= '\u33FF') ||
                    ('\u3400' <= ch && ch <= '\u4DBF') ||
                    ('\u4DC0' <= ch && ch <= '\u4DFF') ||
                    ('\u4E00' <= ch && ch <= '\u9FBF') ||
                    ('\uF900' <= ch && ch <= '\uFAFF') 
                    // || ('\uFE30' <= ch && ch <= '\uFE4F') CJK 兼容形式 (CJK Compatibility Forms)
                    // || ('\uFF00' <= ch && ch <= '\uFFEF') 全角ASCII、全角标点
                );
    }

    /// <summary> 缓存以避免GC </summary>
    static IEnumerable<string> cachedEnumerable;
    static string parsingText;

    /// <summary>
    /// 从左向右逐渐输出字串，除了中文以外，以空格或标点为分隔，每次返回的字串都包含之前遍历过的内容。
    /// 该方法不可重入
    /// </summary>
    /// <example>foreach(var tryWrappedLine in text.GetChineseL2RTokenizer())</example>
    public static IEnumerable<string> GetChineseL2RTokenizer(this string text)
    {
        parsingText = text;
        if (cachedEnumerable == null) {
            cachedEnumerable = new ChineseL2RTokenizerEnumerator();
        }
        return cachedEnumerable;
    }
    
    /*public static IEnumerator<string> GetChineseL2RTokenizer(this string text)
    {
        int parsed = 0;
        foreach (var subText in text.GetL2RTokenizer()) {
            ///前后如果有标点符号，和相邻的字符一起输出。如果是中文，独立输出
            bool wrapBeforeChinese = true;
            for (int i = parsed; i < subText.Length - 1; ++i) {
                if (i == parsed && NoGcContains(L2RTokenizer.nonEnderChars, subText[i])) {
                    wrapBeforeChinese = false;
                    continue;
                }
                if (i + 1 == subText.Length - 1 && NoGcContains(L2RTokenizer.nonStarterChars, subText[i + 1])) {
                    wrapBeforeChinese = false;
                    continue;
                }
                if (IsChineseCharacter(subText[i])) {
                    if (wrapBeforeChinese) {
                        yield return subText.Substring(0, i);
                    }
                    yield return subText.Substring(0, i + 1);
                    wrapBeforeChinese = false;
                }
                else {
                    wrapBeforeChinese = true;
                }
            }
            yield return subText;
            parsed = subText.Length;
        }
        yield return text;
    }*/
    
    private sealed class ChineseL2RTokenizerEnumerator : IEnumerator, IDisposable, IEnumerator<string>, IEnumerable<string> {

        internal string current;
        internal int state;
        internal IEnumerator<string> l2rTokenizer;
        internal int i;
        internal int parsed;
        internal string subText;
        internal bool wrapBeforeChinese;

        public void Dispose () {
            uint num = (uint)state;
            state = -1;
            switch (num) {
            case 1:
            case 2:
            case 3:
                l2rTokenizer.Dispose();
                break;
            }
            current = null;
        }
        public bool MoveNext () {
            uint num = (uint)state;
            state = -1;
            bool jumpIn = false;
            switch (num) {
            case 0:
                parsed = 0;
                l2rTokenizer = ChineseL2RTokenizer.parsingText.GetL2RTokenizer().GetEnumerator();
                num = 0xfffffffd;
                break;
            case 1: //Label_0161
                current = subText.Substring(0, i + 1);
                state = 2;
                return true;
            case 2:
                wrapBeforeChinese = false;
                ++i;
                jumpIn = true;
                break;
            case 3:
                parsed = subText.Length;
                break;
            case 4:
                state = -1;
                return false;
            default:
                return false;
            }
            while (jumpIn || l2rTokenizer.MoveNext()) {
                if (jumpIn == false) {
                    subText = l2rTokenizer.Current;
                    wrapBeforeChinese = true;
                    i = parsed;
                }
                else {
                    jumpIn = false;
                }
                while (i < (subText.Length - 1)) {
                    if ((i == parsed) && ChineseL2RTokenizer.ContainsChar(L2RTokenizer.nonEnderChars, subText[i])) {
                        wrapBeforeChinese = false;
                        ++i;
                        continue;
                    }
                    if (((i + 1) == (subText.Length - 1)) && ChineseL2RTokenizer.ContainsChar(L2RTokenizer.nonStarterChars, subText[i + 1])) {
                        wrapBeforeChinese = false;
                        ++i;
                        continue;
                    }
                    if (!ChineseL2RTokenizer.IsChineseCharacter(subText[i])) {
                        wrapBeforeChinese = true;
                        ++i;
                        continue;
                    }
                    if (wrapBeforeChinese) {
                        current = subText.Substring(0, i);
                        state = 1;
                    }
                    else {
                        //Label_0161:
                        current = subText.Substring(0, i + 1);
                        state = 2;
                    }
                    return true;
                }
                current = subText;
                state = 3;
                return true;
            }
            l2rTokenizer.Dispose();
            current = ChineseL2RTokenizer.parsingText;
            state = 4;
            return true;
        }
        public void Reset () {
            current = null;
            state = 0;
            l2rTokenizer = null;
            i = 0;
            parsed = 0;
            subText = null;
            wrapBeforeChinese = false;
        }
        string IEnumerator<string>.Current {
            get {
                return current;
            }
        }
        object IEnumerator.Current {
            get {
                return current;
            }
        }
        public IEnumerator<string> GetEnumerator () {
            Reset();
            return this;
        }
        IEnumerator IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }
    }

    // No GC
    public static bool ContainsChar (char[] array, char c) {
        for (int i = 0; i < array.Length; ++i) {
            if (array[i] == c) {
                return true;
            }
        }
        return false;
    }
}