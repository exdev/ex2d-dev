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

        while ( cur_index < _text.Length ) {
            int next_index = cur_index+1;
            char cur_char = _text[cur_index];

            // process new-line
            if ( cur_char == '\n' ) {
                if ( _wrap == WrapMode.Pre || _wrap == WrapMode.PreWrap ) {
                    _end_x = cur_x;
                    _end_index = cur_index;
                    return false;
                }
                cur_char = ' ';
            }

            // process white-space
            if ( cur_char == ' ' ) {
                bool doShrink = false;
                bool skipThis = false;

                // shrink the started white-space
                if ( cur_index == _start_index &&
                     ( /*_wrap == WrapMode.PreWrap ||*/ _wrap == WrapMode.Word || _wrap == WrapMode.None ) ) 
                {
                    skipThis = true;
                    doShrink = true;
                }
                else if ( _wrap == WrapMode.Word || _wrap == WrapMode.None ) {
                    doShrink = true;
                }

                //
                if ( doShrink ) {
                    while ( next_index < _text.Length ) {
                        char next_char = _text[next_index];
                        if ( next_char != ' ' && next_char != '\n' )
                            break;
                        ++next_index;
                    }

                    // if we are at the end of the line, just ignore any white-space.
                    if ( next_index >= _text.Length ) {
                        _end_x = cur_x;
                        _end_index = cur_index;
                        return true;
                    }
                }
                if ( skipThis ) {
                    cur_index = next_index;
                    continue;
                }
            }

            // get character info
            CharacterInfo charInfo;
            _font.GetCharacterInfo ( cur_char, out charInfo, _fontSize );

            // advance-x
            cur_x += (int)charInfo.width + _letterSpacing;
            if ( cur_char == ' ' )
                cur_x += _wordSpacing;

            //
            cur_index = next_index;

            // check if wrap the next-word
            if ( cur_char == ' ' && (_wrap == WrapMode.Word || _wrap == WrapMode.PreWrap) ) {

                int tmp_x = cur_x;
                int tmp_index = next_index;

                if ( tmp_index >= _text.Length )
                    break;

                char tmp_char = _text[tmp_index];
                while ( tmp_char != ' ' && tmp_char != '\n' ) {
                    CharacterInfo tmp_charInfo;
                    _font.GetCharacterInfo ( tmp_char, out tmp_charInfo, _fontSize );

                    tmp_x += (int)tmp_charInfo.width + _letterSpacing;

                    if ( tmp_x > _width ) {
                        _end_x = cur_x - (int)charInfo.width - _letterSpacing - _wordSpacing;
                        _end_index = cur_index-1;
                        return false;
                    }

                    ++tmp_index;
                    if ( tmp_index >= _text.Length )
                        break;
                    tmp_char = _text[tmp_index];
                }
            }

            // NOTE: we put builder here to skip white-space when text been break into nextline
            _builder.Append(cur_char);
        }

        //
        _end_x = cur_x;
        _end_index = cur_index;

        return true;
    }

    // ------------------------------------------------------------------ 
    // Desc: This only calculate result in one line
    // ------------------------------------------------------------------ 

    public static bool CalcTextLine ( ref int _end_x, 
                                      ref int _end_index,
                                      ref StringBuilder _builder,
                                      string _text, 
                                      int _start_index, 
                                      int _width,
                                      exBitmapFont _font, 
                                      int _fontSize, 
                                      WrapMode _wrap, 
                                      int _wordSpacing, 
                                      int _letterSpacing ) 
    {
        int cur_index = _start_index;
        int cur_x = 0;
        int line_width = 0;

        while ( cur_index < _text.Length ) {
            int next_index = cur_index+1;
            char cur_char = _text[cur_index];

            // process new-line
            if ( cur_char == '\n' ) {
                if ( _wrap == WrapMode.Pre || _wrap == WrapMode.PreWrap ) {
                    _end_x = line_width;
                    _end_index = cur_index;
                    return false;
                }
                cur_char = ' ';
            }

            // process white-space
            if ( cur_char == ' ' ) {
                bool doShrink = false;
                bool skipThis = false;

                // pre-wrap will shrink the started white-space
                if ( cur_index == _start_index &&
                     ( /*_wrap == WrapMode.PreWrap ||*/ _wrap == WrapMode.Word || _wrap == WrapMode.None ) ) 
                {
                    skipThis = true;
                    doShrink = true;
                }
                else if ( _wrap == WrapMode.Word || _wrap == WrapMode.None ) {
                    doShrink = true;
                }

                //
                if ( doShrink ) {
                    while ( next_index < _text.Length ) {
                        char next_char = _text[next_index];
                        if ( next_char != ' ' && next_char != '\n' )
                            break;
                        ++next_index;
                    }

                    // if we are at the end of the line, just ignore any white-space.
                    if ( next_index >= _text.Length ) {
                        _end_x = line_width;
                        _end_index = cur_index;
                        return true;
                    }
                }
                if ( skipThis ) {
                    cur_index = next_index;
                    continue;
                }
            }

            // get character info
            exBitmapFont.CharInfo charInfo = _font.GetCharInfo(cur_char);
            if ( charInfo != null ) {
                // get the line-width
                line_width = cur_x + (int)charInfo.width + _letterSpacing;
                if ( cur_char == ' ' ) {
                    line_width += _wordSpacing;
                }

                // advance-x
                cur_x += (int)charInfo.xadvance + _letterSpacing;
                if ( cur_char == ' ' )
                    cur_x += _wordSpacing;

                //
                cur_index = next_index;

                // check if wrap the next-word
                if ( cur_char == ' ' && (_wrap == WrapMode.Word || _wrap == WrapMode.PreWrap) ) {

                    int tmp_x = cur_x;
                    int tmp_index = next_index;
                    int tmp_width = line_width;

                    if ( tmp_index >= _text.Length )
                        break;

                    char tmp_char = _text[tmp_index];
                    while ( tmp_char != ' ' && tmp_char != '\n' ) {
                        exBitmapFont.CharInfo tmp_charInfo = _font.GetCharInfo(tmp_char);
                        if ( tmp_charInfo != null ) {
                            tmp_width = tmp_x + (int)tmp_charInfo.width + _letterSpacing;
                            tmp_x += (int)tmp_charInfo.xadvance + _letterSpacing;

                            if ( tmp_width > _width ) {
                                // NOTE: in BitmapFont, space have zero width, so we use cur_x here NOT line_width
                                _end_x = cur_x - (int)charInfo.width - _letterSpacing - _wordSpacing;
                                _end_index = cur_index-1;
                                return false;
                            }
                        }

                        ++tmp_index;
                        if ( tmp_index >= _text.Length )
                            break;
                        tmp_char = _text[tmp_index];
                    }
                }

                // NOTE: we put builder here to skip white-space when text been break into nextline
                _builder.Append(cur_char);
            }
            else {
                cur_index = next_index;
            }
        }

        //
        _end_x = line_width;
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
