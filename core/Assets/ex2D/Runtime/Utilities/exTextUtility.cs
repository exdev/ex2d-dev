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

public static class exTextUtility {

    public static GUIStyle fontHelper = new GUIStyle();

    public enum WrapMode {
        None,
        Word,
        Pre,
        PreWrap,
    }

    // // ------------------------------------------------------------------ 
    // // Desc: This only calculate result in one line
    // // ------------------------------------------------------------------ 

    // public static void CalcTextSize ( ref int _last_line_width,
    //                                   ref int _lines,
    //                                   string _text, 
    //                                   int _offset_x,
    //                                   int _width,
    //                                   Font _font, 
    //                                   int _fontSize, 
    //                                   WrapMode _wrap, 
    //                                   int _wordSpacing, 
    //                                   int _letterSpacing ) 
    // {
    //     //
    //     _font.RequestCharactersInTexture ( _text, _fontSize, FontStyle.Normal );

    //     //
    //     _last_line_width = 0;
    //     _lines = 0;

    //     bool finished = false;
    //     int cur_x = _offset_x;
    //     int cur_index = 0;
    //     int cur_width = _width - _offset_x;

    //     while ( finished == false ) {
    //         finished = CalcTextLine ( ref _last_line_width, 
    //                                   ref cur_index,
    //                                   _text,
    //                                   cur_index,
    //                                   cur_width,
    //                                   _font,
    //                                   _fontSize,
    //                                   _wrap,
    //                                   _wordSpacing,
    //                                   _letterSpacing );
    //         cur_x = 0;
    //         cur_width = _width - cur_x;
    //         ++_lines; 
    //         ++cur_index;
    //     }
    // }

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static bool CalcTextLine ( ref int _end_x, 
                                      ref int _end_index,
                                      ref StringBuilder _builder,
                                      string _text, 
                                      int _start_index, 
                                      int _width,
                                      Font _font, 
                                      int _fontSize, 
                                      WrapMode _wrap, 
                                      int _wordSpacing, 
                                      int _letterSpacing ) 
    {
        int cur_index = _start_index;
        int cur_x = 0;
        char last_char = ' '; // a new line always shrink

        for ( ; cur_index < _text.Length; ++cur_index ) {
            char cur_char = _text[cur_index];

            // process new-line
            if ( cur_char == '\n' ) {
                // replace new-line as white-space in mode "wrap-none", "wrap-word" 
                if ( _wrap == WrapMode.Word || _wrap == WrapMode.None ) {
                    cur_char = ' ';
                }
                // advance y
                else {
                    _end_x = cur_x;
                    _end_index = cur_index;
                    return false;
                }
            }

            // process white-space
            if ( cur_char == ' ' ) {
                // if last character is space, shrink it if not in "wrap-pre" mode
                if ( _wrap != WrapMode.Pre ) {
                    if ( last_char == ' ' ) {
                        continue;
                    }
                }
            }
            // process wrap
            else {
                if ( last_char == ' ' &&
                     ( _wrap == WrapMode.Word || _wrap == WrapMode.PreWrap ) )
                {
                    int tmp_x = cur_x;
                    char tmp_char = ' '; 
                    for ( int j = cur_index; j < _text.Length; ++j ) {
                        tmp_char = _text[j];
                        if ( tmp_char == ' ' )
                            break;

                        CharacterInfo tmp_charInfo;
                        _font.GetCharacterInfo ( cur_char, out tmp_charInfo, _fontSize );

                        tmp_x += (int)tmp_charInfo.width;
                        if ( tmp_x > _width ) {
                            _end_x = cur_x;
                            _end_index = cur_index-1;
                            return false;
                        }
                    }
                }
            }

            // generate mesh
            CharacterInfo charInfo;
            _font.GetCharacterInfo ( cur_char, out charInfo, _fontSize );

            // advance x
            cur_x += (int)charInfo.width + _letterSpacing;
            if ( cur_char == ' ' )
                cur_x += _wordSpacing;

            //
            last_char = cur_char;
            _builder.Append(cur_char);
        }

        //
        _end_x = cur_x;
        _end_index = cur_index;

        return true;
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
                _uvs[idx + 0] = new Vector2(charInfo.uv.xMax, charInfo.uv.yMax);
                _uvs[idx + 1] = new Vector2(charInfo.uv.xMax, charInfo.uv.yMin);
                _uvs[idx + 2] = new Vector2(charInfo.uv.xMin, charInfo.uv.yMin);
                _uvs[idx + 3] = new Vector2(charInfo.uv.xMin, charInfo.uv.yMax);
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
}
