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

    /// <summary>
    /// 从左向右逐渐输出字串，以空格或标点为分隔，每次返回的字串都包含之前遍历过的内容。
    /// </summary>
    /// <example>foreach(var tryWrappedLine in text.GetL2RTokenizer())</example>
    public static IEnumerable<string> GetL2RTokenizer(this string text)
    {
        //Debug.Log("111111111111" + text);
        int lineEnd = text.IndexOf('\n');
        if (lineEnd >= 0) {
            text = text.Substring(0, lineEnd);
        }
        for (int nextStartIndex = 0; nextStartIndex < text.Length; ) {
            int onlyEndOfLine = text.IndexOfAny(nonStarterChars, nextStartIndex);
            int onlyStartOfLine = text.IndexOfAny(nonEnderChars, nextStartIndex);
            //Debug.Log("next " + onlyEndOfLine + " " + onlyStartOfLine);
            if (onlyStartOfLine > 0 && (onlyEndOfLine < 0 || onlyStartOfLine < onlyEndOfLine)) {
                nextStartIndex = onlyStartOfLine + 1;
                //Debug.Log("onlyStartOfLine " + onlyStartOfLine);
                //if (onlyStartOfLine == text.Length) {
                //    break;
                //}
                //else {
                    yield return text.Substring(0, onlyStartOfLine);
                //}
            } else if (onlyEndOfLine >= 0/* && canEndOfLine > canStartOfLine*/) {
                onlyEndOfLine += 1;
                nextStartIndex = onlyEndOfLine;
                //Debug.Log("onlyEndOfLine " + onlyEndOfLine);
                //if (onlyEndOfLine == text.Length) {
                //    break;
                //}
                //else {
                    yield return text.Substring(0, onlyEndOfLine);
                //}
            } else {
                //Debug.Log(22222222);
                break;
            }
        }
        //Debug.Log(333333333);
        yield return text;
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
    /// <summary>
    /// 从左向右逐渐输出字串，除了中文以外，以空格或标点为分隔，每次返回的字串都包含之前遍历过的内容。
    /// </summary>
    /// <example>foreach(var tryWrappedLine in text.GetChineseL2RTokenizer())</example>
    public static IEnumerable<string> GetChineseL2RTokenizer(this string text)
    {
        int parsed = 0;
        foreach (var subText in text.GetL2RTokenizer()) {
            ///前后如果有标点符号，和相邻的字符一起输出。如果是中文，独立输出
            bool wrapBeforeChinese = true;
            for (int i = parsed; i < subText.Length - 1; ++i) {
                if (i == parsed && L2RTokenizer.nonEnderChars.Contains(subText[i])) {
                    wrapBeforeChinese = false;
                    continue;
                }
                else if (i + 1 == subText.Length - 1 && L2RTokenizer.nonStarterChars.Contains(subText[i + 1])) {
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
    }
}