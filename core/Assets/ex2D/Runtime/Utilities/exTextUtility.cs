// ======================================================================================
// File         : exTextUtility.cs
// Author       : Wu Jie 
// Last Change  : 09/05/2013 | 14:52:01 PM | Thursday,September
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

///////////////////////////////////////////////////////////////////////////////
//
/// the font utility
//
///////////////////////////////////////////////////////////////////////////////

public static partial class exTextUtility {

    public static GUIStyle fontHelper = new GUIStyle();

    public enum WrapMode {
        None,
        Word,
        Pre,
        PreWrap,
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // http://baike.baidu.com/view/40801.htm
    // http://www.ipmtea.net/javascript/201009/23_294.html
    // ------------------------------------------------------------------ 

    public static bool IsChinese ( char _ch ) {
        return (0x2E80 <= _ch && _ch <= 0xFAFF) 
            && (
                (0x4E00 <= _ch && _ch <= 0x9FBF) // CJK Unified Ideographs (*most frequency) 
                || (0x2E80 <= _ch && _ch <= 0x2EFF) // CJK Radicals Supplement
                || (0x2F00 <= _ch && _ch <= 0x2FDF) // Kangxi Radicals
                || (0x3000 <= _ch && _ch <= 0x303F) // CJK Symbols and Punctuation
                || (0x31C0 <= _ch && _ch <= 0x31EF) // CJK Strokes
                || (0x3200 <= _ch && _ch <= 0x32FF) // Enclosed CJK Letters and Months
                || (0x3300 <= _ch && _ch <= 0x33FF) // CJK Compatibility
                || (0x3400 <= _ch && _ch <= 0x4DBF) // CJK Unified Ideographs Extension A
                || (0x4DC0 <= _ch && _ch <= 0x4DFF) // Yijing Hexagrams Symbols
                || (0xF900 <= _ch && _ch <= 0xFAFF) // CJK Compatibility Ideographs
               );
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static bool IsJapanese ( char _ch ) {
        return (0x3040 <= _ch && _ch <= 0x309F) // Hiragana 
            || (0x30A0 <= _ch && _ch <= 0x30FF) // Katakana
            || (0x31F0 <= _ch && _ch <= 0x31FF) // Katakana Phonetic Extensions
            ;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static bool CanWordBreak ( char _ch ) {
        return (_ch == ' ' || _ch == '\t' || _ch == '\f' || _ch == '\n' || _ch == '\r')
            || IsChinese (_ch)
            || IsJapanese (_ch);
    }

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static bool CalcTextLine ( out int _end_x, 
                                      out int _end_index,
                                      StringBuilder _builder,
                                      string _text, 
                                      int _index,
                                      int _maxWidth,
                                      exFont _font, 
                                      int _wordSpacing,
                                      int _letterSpacing,
                                      bool _wrapWord,
                                      bool _collapseSpace,
                                      bool _collapseLinebreak )
    {
        int cur_index = _index; 
        int next_index = _index;
        int word_start_index = _index;
        int cur_x = 0; 
        int word_start_x = 0;
        bool linebreak = false;
        bool trimWhitespace = true; 
        bool beginningOfLine = true;
        char last_ch = '\0';

        while ( cur_index < _text.Length ) {
            bool skipcpy = false;
            char ch = _text[cur_index];
            next_index = cur_index+1;

            // if this is line-break
            if ( ch == '\n' || ch == '\r' ) {
                if ( _collapseLinebreak ) {
                    ch = ' '; // turn it to space
                }
                else {
                    linebreak = true;
                }
            }

            // if this is space 
            if ( ch == ' ' || ch == '\t' || ch == '\f' ) {
                if ( _collapseSpace ) {
                    while ( next_index < _text.Length ) {
                        char next_ch = _text[next_index];
                        next_index = next_index + 1;

                        // if next_ch is white-space, then collapse this char
                        if ( next_ch == ' ' || next_ch == '\t' || next_ch == '\f' ) {
                            cur_index = next_index-1;
                            continue;
                        }

                        // if next_ch is line-break and collapseLinebreak is true, then collapse this char
                        if ( next_ch == '\n' || next_ch == '\r' ) {
                            if ( _collapseLinebreak ) {
                                cur_index = next_index-1;
                                continue;
                            }
                        }

                        //
                        break;
                    }

                    // skip first-time collapse
                    if ( trimWhitespace ) {
                        trimWhitespace = false;
                        cur_index = next_index;
                        continue;
                    }

                    // yes, must turn it to space to make sure only one space
                    ch = ' ';
                }
            }

            //
            trimWhitespace = false;

            // process word-break, word-wrap
            if ( _wrapWord ) {
                word_start_index = cur_index;
                word_start_x = cur_x;

                // if this character can break
                if ( next_index >= _text.Length || CanWordBreak (ch) ) {
                    // advanced character
                    if ( last_ch != '\0' ) {
                        cur_x += _font.GetKerning(last_ch, ch);
                    }
                    cur_x += _font.GetAdvance(ch);
                    last_ch = ch;

                    // check if the word exceed content width
                    if ( cur_x > _maxWidth ) {
                        if ( !beginningOfLine ) {
                            linebreak = true;

                            // skip copy the white-space if it is at the end of the wrap
                            if ( ch == ' ' || ch == '\t' || ch == '\f' ) {
                                skipcpy = true;
                            }
                            else {
                                next_index = word_start_index;
                                cur_x = word_start_x;
                            }
                        }
                    }

                    beginningOfLine = false;
                }
                else {
                    // advanced current character
                    if ( last_ch != '\0' ) {
                        cur_x += _font.GetKerning(last_ch, ch);
                    }
                    cur_x += _font.GetAdvance(ch);
                    last_ch = ch;

                    while ( next_index < _text.Length ) {
                        char next_ch = _text[next_index];
                        next_index = next_index + 1;

                        // if this character can break
                        if ( CanWordBreak (next_ch) ) {
                            next_index -= 1;
                            break;
                        }

                        // advanced character
                        if ( last_ch != '\0' ) {
                            cur_x += _font.GetKerning(last_ch, next_ch);
                        }
                        cur_x += _font.GetAdvance(next_ch);
                        last_ch = next_ch;

                        // TODO: process word-break
                        // check if the word exceed content width
                        if ( cur_x > _maxWidth ) {
                            if ( !beginningOfLine ) {
                                linebreak = true;

                                next_index = word_start_index;
                                cur_x = word_start_x;
                                skipcpy = true;
                                break;
                            }
                        }
                    }
                }
            }
            else {
                // advanced character
                if ( last_ch != '\0' ) {
                    cur_x += _font.GetKerning(last_ch, ch);
                }
                cur_x += _font.GetAdvance(ch);
                last_ch = ch;
            } 

            // copy character to newtext_p
            if ( !skipcpy ) {
                int cpylen = next_index-cur_index;
                if ( cpylen > 0 ) {
                    _builder.Append(_text, cur_index, cpylen );
                }
            }

            // step
            cur_index = next_index;
            if ( linebreak ) {
                break;
            }
        }

        _end_x = cur_x;
        _end_index = cur_index;

        return linebreak;
    }

    ///////////////////////////////////////////////////////////////////////////////
    // build mesh
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static void BuildTextLine ( Vector3[] _vertices, Vector2[] _uvs, 
                                       string _text, 
                                       Font _font, 
                                       int _lineHeight,
                                       int _fontSize, 
                                       int _wordSpacing, 
                                       int _letterSpacing ) 
    {
        int cur_x = 0;
        int cur_y = 0;

        // yes, Unity's GetCharacterInfo have y problem, you should get lowest character j's y-offset adjust it.
        CharacterInfo jCharInfo;
        _font.RequestCharactersInTexture ( "j", _fontSize, FontStyle.Normal );
        _font.GetCharacterInfo('j', out jCharInfo, _fontSize, FontStyle.Normal);
        float ttf_offset = (_fontSize + jCharInfo.vert.yMax);

        //
        for ( int i = 0; i < _text.Length; ++i ) {
            char cur_char = _text[i];

            // NOTE: we skip new-line operation, since we believe this function only have one-line text
            if ( cur_char == '\n' ) {
                continue;
            }

            // generate mesh
            CharacterInfo charInfo;
            _font.GetCharacterInfo ( cur_char, out charInfo, _fontSize );
            int idx = 4*i;

            // build vertices
            _vertices[idx + 0] = new Vector3(cur_x + charInfo.vert.xMin, cur_y - charInfo.vert.yMin + ttf_offset, 0.0f);
            _vertices[idx + 1] = new Vector3(cur_x + charInfo.vert.xMax, cur_y - charInfo.vert.yMin + ttf_offset, 0.0f);
            _vertices[idx + 2] = new Vector3(cur_x + charInfo.vert.xMax, cur_y - charInfo.vert.yMax + ttf_offset, 0.0f);
            _vertices[idx + 3] = new Vector3(cur_x + charInfo.vert.xMin, cur_y - charInfo.vert.yMax + ttf_offset, 0.0f);

            // build uv
            if ( charInfo.flipped ) {
                _uvs[idx + 0] = new Vector2(charInfo.uv.xMax, charInfo.uv.yMin);
                _uvs[idx + 1] = new Vector2(charInfo.uv.xMax, charInfo.uv.yMax);
                _uvs[idx + 2] = new Vector2(charInfo.uv.xMin, charInfo.uv.yMax);
                _uvs[idx + 3] = new Vector2(charInfo.uv.xMin, charInfo.uv.yMin);
            }
            else {
                _uvs[idx + 0] = new Vector2(charInfo.uv.xMin, charInfo.uv.yMax);
                _uvs[idx + 1] = new Vector2(charInfo.uv.xMax, charInfo.uv.yMax);
                _uvs[idx + 2] = new Vector2(charInfo.uv.xMax, charInfo.uv.yMin);
                _uvs[idx + 3] = new Vector2(charInfo.uv.xMin, charInfo.uv.yMin);
            }

            // advance x
            cur_x += (int)charInfo.width + _letterSpacing;
            if ( cur_char == ' ' )
                cur_x += _wordSpacing;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static void BuildTextLine ( Vector3[] _vertices, Vector2[] _uvs, 
                                       string _text, 
                                       exBitmapFont _font, 
                                       int _lineHeight,
                                       int _fontSize, 
                                       int _wordSpacing, 
                                       int _letterSpacing ) 
    {
        int cur_x = 0;
        int cur_y = 0;

        //
        for ( int i = 0; i < _text.Length; ++i ) {
            char cur_char = _text[i];

            // NOTE: we skip new-line operation, since we believe this function only have one-line text
            if ( cur_char == '\n' ) {
                continue;
            }

            // generate mesh
            exBitmapFont.CharInfo charInfo = _font.GetCharInfo(cur_char);
            if ( charInfo != null ) {
                int idx = 4*i;
                float x = cur_x + charInfo.xoffset;
                float y = cur_y + charInfo.yoffset;

                Vector2 texelSize = _font.texture.texelSize;
                Vector2 start = new Vector2( charInfo.x * texelSize.x, 
                                             charInfo.y * texelSize.y );
                Vector2 end = new Vector2( (charInfo.x + charInfo.rotatedWidth) * texelSize.x, 
                                           (charInfo.y + charInfo.rotatedHeight) * texelSize.y );

                // build vertices
                _vertices[idx + 0] = new Vector3(x,                  y, 0.0f);
                _vertices[idx + 1] = new Vector3(x + charInfo.width, y, 0.0f);
                _vertices[idx + 2] = new Vector3(x + charInfo.width, y + charInfo.height, 0.0f);
                _vertices[idx + 3] = new Vector3(x,                  y + charInfo.height, 0.0f);

                // build uv
                if ( charInfo.rotated ) {
                    _uvs[idx + 0] = new Vector2(end.x,   start.y);
                    _uvs[idx + 1] = new Vector2(end.x,   end.y);
                    _uvs[idx + 2] = new Vector2(start.x, end.y);
                    _uvs[idx + 3] = new Vector2(start.x, start.y);
                }
                else {
                    _uvs[idx + 0] = new Vector2(start.x, end.y);
                    _uvs[idx + 1] = new Vector2(end.x,   end.y);
                    _uvs[idx + 2] = new Vector2(end.x,   start.y);
                    _uvs[idx + 3] = new Vector2(start.x, start.y);
                }

                // advance x
                cur_x += (int)charInfo.xadvance + _letterSpacing;
                if ( cur_char == ' ' )
                    cur_x += _wordSpacing;
            }
        }
    }
}
