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

    public enum WrapMode {
        None,
        Word,
        Pre,
        PreWrap,
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void CalcTextSize ( ref int _height, ref int _end_x, 
                                      string _text, int _width, int _start_x,
                                      exBitmapFont _font, int _fontSize, WrapMode _wrap, 
                                      int _lineHeight, int _wordSpacing, int _letterSpacing ) 
    {
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void CalcTextSize ( ref int _height, ref int _end_x, 
                                      string _text, int _width, int _start_x,
                                      Font _font, int _fontSize, WrapMode _wrap, 
                                      int _lineHeight, int _wordSpacing, int _letterSpacing ) 
    {
        _font.RequestCharactersInTexture ( _text, _fontSize, FontStyle.Normal );

        int cur_x = _start_x;
        int cur_y = 0;
        char last_char = '\n';

        for ( int i = 0; i < _text.Length; ++i ) {
            char cur_char = _text[i];

            // process new-line
            if ( cur_char == '\n' ) {
                // replace new-line as white-space in mode "wrap-none", "wrap-word" 
                if ( _wrap == WrapMode.Word || _wrap == WrapMode.None ) {
                    cur_char = ' ';
                }
                // advance y
                else {
                    cur_x = 0;
                    cur_y += _lineHeight;
                    continue;
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
                    for ( int j = i; j < _text.Length; ++j ) {
                        tmp_char = _text[j];
                        if ( tmp_char == ' ' )
                            break;

                        CharacterInfo tmp_charInfo;
                        _font.GetCharacterInfo ( cur_char, out tmp_charInfo, _fontSize );

                        tmp_x += (int)tmp_charInfo.width;
                        if ( tmp_x > _width ) {
                            cur_x = _start_x;
                            cur_y += _lineHeight;
                            break;
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
        cur_y += _lineHeight;

        _height = cur_y;
        _end_x = cur_x;
    }


    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public static void BuildText ( string _text, int _width, int _start_x,
                                   Font _font, int _fontSize, WrapMode _wrap, 
                                   int _lineHeight, int _wordSpacing, int _letterSpacing, TextAlignment _align ) 
    {
        _font.RequestCharactersInTexture ( _text, _fontSize, FontStyle.Normal );

        int cur_x = _start_x;
        int cur_y = 0;
        char last_char = '\n';

        for ( int i = 0; i < _text.Length; ++i ) {
            char cur_char = _text[i];

            // process new-line
            if ( cur_char == '\n' ) {
                // replace new-line as white-space in mode "wrap-none", "wrap-word" 
                if ( _wrap == WrapMode.Word || _wrap == WrapMode.None ) {
                    cur_char = ' ';
                }
                // advance y
                else {
                    cur_x = 0;
                    cur_y += _lineHeight;
                    continue;
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
                    for ( int j = i; j < _text.Length; ++j ) {
                        tmp_char = _text[j];
                        if ( tmp_char == ' ' )
                            break;

                        CharacterInfo tmp_charInfo;
                        _font.GetCharacterInfo ( cur_char, out tmp_charInfo, _fontSize );

                        tmp_x += (int)tmp_charInfo.width;
                        if ( tmp_x > _width ) {
                            cur_x = _start_x;
                            cur_y += _lineHeight;
                            break;
                        }
                    }
                }
            }

            // generate mesh
            CharacterInfo charInfo;
            _font.GetCharacterInfo ( cur_char, out charInfo, _fontSize );
            // TODO:

            // advance x
            cur_x += (int)charInfo.width;

            //
            last_char = cur_char;
        }
        cur_y += _lineHeight;
    }
}
