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

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static void CalcTextSize ( ref int _last_line_width,
                                      ref int _lines,
                                      string _text, 
                                      int _offset_x,
                                      int _width,
                                      Font _font, 
                                      int _fontSize, 
                                      WrapMode _wrap, 
                                      int _wordSpacing, 
                                      int _letterSpacing ) 
    {
        //
        _font.RequestCharactersInTexture ( _text, _fontSize, FontStyle.Normal );

        //
        _last_line_width = 0;
        _lines = 0;

        bool finished = false;
        int cur_x = _offset_x;
        int cur_index = 0;
        int cur_width = _width - _offset_x;

        while ( finished == false ) {
            finished = CalcTextLine ( ref _last_line_width, 
                                      ref cur_index,
                                      _text,
                                      cur_index,
                                      cur_width,
                                      _font,
                                      _fontSize,
                                      _wrap,
                                      _wordSpacing,
                                      _letterSpacing );
            cur_x = 0;
            cur_width = _width - cur_x;
            ++_lines; 
            ++cur_index;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static bool CalcTextLine ( ref int _end_x, 
                                      ref int _end_index,
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
        int cur_y = 0;
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
                    if ( last_char == ' ' )
                        continue;
                }
            }
            // process wrap
            else {
                if ( last_char == ' ' &&
                     ( _wrap == WrapMode.Word || _wrap == WrapMode.PreWrap ) )
                {
                    int tmp_x = cur_x;
                    char tmp_char = '\n'; 
                    for ( int j = cur_index; j < _text.Length; ++j ) {
                        tmp_char = _text[j];
                        if ( tmp_char == ' ' )
                            break;

                        CharacterInfo tmp_charInfo;
                        _font.GetCharacterInfo ( cur_char, out tmp_charInfo, _fontSize );

                        tmp_x += (int)tmp_charInfo.width;
                        if ( tmp_x > _width ) {
                            _end_x = cur_x;
                            _end_index = cur_index;
                            return false;
                        }
                    }
                }
            }

            // generate mesh
            CharacterInfo charInfo;
            _font.GetCharacterInfo ( cur_char, out charInfo, _fontSize );

            // advance x
            cur_x += (int)charInfo.width;

            //
            last_char = cur_char;
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

    public static void BuildTextMesh ( ref int _last_line_width,
                                       ref int _lines,
                                       string _text, 
                                       int _offset_x,
                                       int _width,
                                       Font _font, 
                                       int _fontSize, 
                                       WrapMode _wrap, 
                                       int _wordSpacing, 
                                       int _letterSpacing ) 
    {
        //
        _font.RequestCharactersInTexture ( _text, _fontSize, FontStyle.Normal );

        //
        _last_line_width = 0;
        _lines = 0;

        bool finished = false;
        int cur_x = _offset_x;
        int cur_index = 0;
        int cur_width = _width - _offset_x;

        while ( finished == false ) {
            finished = BuildTextLine ( ref _last_line_width, 
                                       ref cur_index,
                                       _text,
                                       cur_index,
                                       cur_width,
                                       _font,
                                       _fontSize,
                                       _wrap,
                                       _wordSpacing,
                                       _letterSpacing );
            cur_x = 0;
            cur_width = _width - cur_x;
            ++_lines; 
            ++cur_index;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static bool BuildTextLine ( ref int _end_x, 
                                       ref int _end_index,
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
        int cur_y = 0;
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
                    if ( last_char == ' ' )
                        continue;
                }
            }
            // process wrap
            else {
                if ( last_char == ' ' &&
                     ( _wrap == WrapMode.Word || _wrap == WrapMode.PreWrap ) )
                {
                    int tmp_x = cur_x;
                    char tmp_char = '\n'; 
                    for ( int j = cur_index; j < _text.Length; ++j ) {
                        tmp_char = _text[j];
                        if ( tmp_char == ' ' )
                            break;

                        CharacterInfo tmp_charInfo;
                        _font.GetCharacterInfo ( cur_char, out tmp_charInfo, _fontSize );

                        tmp_x += (int)tmp_charInfo.width;
                        if ( tmp_x > _width ) {
                            _end_x = cur_x;
                            _end_index = cur_index;
                            return false;
                        }
                    }
                }
            }

            // generate mesh
            CharacterInfo charInfo;
            _font.GetCharacterInfo ( cur_char, out charInfo, _fontSize );

            // advance x
            cur_x += (int)charInfo.width;

            //
            last_char = cur_char;
        }

        //
        _end_x = cur_x;
        _end_index = cur_index;

        return true;
    }
}
