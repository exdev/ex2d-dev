// ======================================================================================
// File         : exTextureInfo.cs
// Author       : Wu Jie 
// Last Change  : 02/17/2013 | 21:39:05 PM | Sunday,February
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// The texture-info asset
///
///////////////////////////////////////////////////////////////////////////////

public class exTextureInfo : ScriptableObject {
    public string rawTextureGUID = "";
    public string rawAtlasGUID = "";
    public Texture2D texture; ///< the atlas or raw texture

    public bool rotated = false; ///< if rotate the texture in atlas 
    public bool trim = false;    ///< if trimmed the texture
    public int trimThreshold = 1; ///< pixels with an alpha value below this value will be consider trimmed 

    // for texture offset
    public int trim_x = 0; ///< the trim offset x of the raw texture in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel
    public int trim_y = 0; ///< the trim offset y of the raw texture in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel
    public int rawWidth = 1;
    public int rawHeight = 1;

    public int x = 0; ///< the x in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel 
    public int y = 0; ///< the y in Unity3D texture coordinate. (0,0) start from bottom-left, same as mesh.uv and Texture2D.SetPixel
    public int width = 1;
    public int height = 1;

    public int borderLeft = 0;
    public int borderRight = 0;
    public int borderTop = 0;
    public int borderBottom = 0;
    
    public int diceUnitX = 0;
    public int diceUnitY = 0;

    [SerializeField] private int[] diceData;

    public int rotatedWidth {
        get {
            if ( rotated ) return height;
            return width;
        }
    }
    public int rotatedHeight {
        get {
            if ( rotated ) return width;
            return height;
        }
    }
    public bool hasBorder {
        get {
            return borderLeft != 0 || borderRight != 0 || borderTop != 0 || borderBottom != 0;
        }
    }

    public bool isDiced {
        get {
            exDebug.Assert((diceUnitX != 0) == (diceUnitY != 0));
            return diceUnitX != 0 && diceUnitY != 0;
        }
    }

    public DiceEnumerator GetDiceEnumerator() {
        return new DiceEnumerator(diceData);    // No GC
    }

    /* tile index:
    8  9  10 11
    4  5  6  7 
    0  1  2  3 
    */
    public void SetDiceData ( Rect[] _tileRects, int[] _x, int[] _y, bool[] _rotated ) {
        List<int> data = new List<int> (_tileRects.Length * 6);
        for ( int i = 0; i < _tileRects.Length; ++i ) {
            Rect rect = _tileRects[i];
            if ( rect.width == 0 || rect.height == 0 ) {
                data.Add( -1 );
                continue;
            }
            data.Add( (int)_tileRects[i].x );   // trim_x
            data.Add( (int)_tileRects[i].y );   // trim_y
            data.Add( (int)_tileRects[i].width );
            data.Add( (int)_tileRects[i].height );
            // TODO: compress max dice
            if ( _rotated[i] ) {
                data.Add( - _x[i] );
            }
            else {
                data.Add( _x[i] );
            }
            data.Add( _y[i] );
        }
        diceData = data.ToArray();  // TrimExcess
    }
}

///////////////////////////////////////////////////////////////////////////////
//
/// The dice data enumerator. Use struct type to avoid GC.
//
///////////////////////////////////////////////////////////////////////////////

public struct DiceEnumerator : System.Collections.Generic.IEnumerator<DiceEnumerator.DiceData> {

    public enum SizeType {
        Max,
        Empty,
        Trimmed,
    }

    public struct DiceData {
        public SizeType sizeType;
        public int trim_x;
        public int trim_y;
        public int width;
        public int height;
        public int x;
        public int y;
        public bool rotated;
    }

    private int[] diceData;
    private int dataIndex;

    public DiceEnumerator (int[] _diceData) {
        diceData = _diceData;
        dataIndex = 0;
        Reset();
    }

    public DiceData Current {
        get {
            DiceData d = new DiceData();
            if (diceData[dataIndex] == -1) {
                d.sizeType = SizeType.Empty;
                return d;
            }
            if (diceData[dataIndex] >= 0) {
                d.sizeType = SizeType.Trimmed;
                d.trim_x = diceData[dataIndex];
                d.trim_y = diceData[dataIndex + 1];
                d.width = diceData[dataIndex + 2];
                d.height = diceData[dataIndex + 3];
                d.x = diceData[dataIndex + 4];
                d.y = diceData[dataIndex + 5];
            }
            else {
                exDebug.Assert(diceData[dataIndex] == -2);
                d.sizeType = SizeType.Max;
                d.x = diceData[dataIndex + 1];
                d.y = diceData[dataIndex + 2];
            }
            if (d.x < 0) {
                d.rotated = true;
                d.x = -d.x;
            }
            return d;
        }
    }
    
    public bool MoveNext () {
        if (dataIndex == -1) {
            dataIndex = 0;
        }
        else if (diceData[dataIndex] >= 0) {
            dataIndex += 6;
        }
        else if (diceData[dataIndex] == -2) {
            dataIndex += 2;
        }
        else {
            dataIndex += 1;
        }
        return dataIndex < diceData.Length;
    }

    public void Dispose () {
        diceData = null;
    }

    object System.Collections.IEnumerator.Current {
        get { return Current; }
    }

    public void Reset () {
        dataIndex = -1;
    }
}