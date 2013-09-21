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
using System.Collections;
using System.Collections.Generic;
using ex2D.Detail;

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

#if UNITY_EDITOR

    [SerializeField]
    //[HideInInspector]
    private int editDiceUnitWidth_ = -1;    ///< not committed value, used for editor
    public int editDiceUnitWidth {
        get {
            if (editDiceUnitWidth_ >= 0) {
                return editDiceUnitWidth_;
            }
            return diceUnitWidth;
        }
        set {
            editDiceUnitWidth_ = Mathf.Max(value, 0);
        }
    }

    [SerializeField]
    //[HideInInspector]
    private int editDiceUnitHeight_ = -1;   ///< not committed value, used for editor
    public int editDiceUnitHeight {
        get {
            if (editDiceUnitHeight_ >= 0) {
                return editDiceUnitHeight_;
            }
            return diceUnitHeight;
        }
        set {
            editDiceUnitHeight_ = Mathf.Max(value, 0);
        }
    }
    
    public bool shouldDiced {
        get { return editDiceUnitWidth_ > 0 || editDiceUnitHeight_ > 0; }
    }

#endif

    [SerializeField]
    //[HideInInspector]
    private List<int> diceData = new List<int>();

    public int diceUnitWidth {  ///< committed value, used for rendering
        get {
            if (diceData != null && diceData.Count > 0) {
                return diceData[0];
            }
            return 0;
        }
    }
    public int diceUnitHeight {  ///< committed value, used for rendering
        get {
            if (diceData != null && diceData.Count > 0) {
                return diceData[1];
            }
            return 0;
        }
    }

    public bool isDiced {
        get {
            if (diceData != null && diceData.Count > 0) {
                return diceData[0] > 0 || diceData[1] > 0;
            }
            return false;
        }
    }

    public DiceEnumerator dices {  ///< get dice enumerator
        get {
            return new DiceEnumerator(diceData, this);    // No GC
        }
    }
    
    public enum DiceType {
        Empty,
        Max,
        Trimmed,
    }

    public struct Dice {  ///< the dice data used in dice enumerator
        public DiceType sizeType;
        public int offset_x;
        public int offset_y;
        public int width;
        public int height;
        public int x;
        public int y;
        public bool rotated;
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
#if UNITY_EDITOR
        public DiceEnumerator enumerator;
        public int trim_x {
            get {
                int col = enumerator.tileIndex % enumerator.columnCount;
                return enumerator.textureInfo.trim_x + col * enumerator.textureInfo.diceUnitWidth + offset_x;
            }
        }
        public int trim_y {
            get {
                int row = enumerator.tileIndex / enumerator.rowCount;
                return enumerator.textureInfo.trim_y + row * enumerator.textureInfo.diceUnitHeight + offset_y;
            }
        }
#endif
    }

#if UNITY_EDITOR

    public void ClearDiceData () {
        diceData.Clear();
    }
    
    /* tile index:
    8  9  10 11
    4  5  6  7 
    0  1  2  3 
     */
    public void AddDiceData ( Rect _rect, int _x, int _y, bool _rotated ) {
        if ( shouldDiced == false ) {
            return;
        }
        //Debug.Log("rect " + _rect + " " + _x + " " + _y);
        if ( diceData.Count == 0 ) {
            // save committed value
            diceData.Add( editDiceUnitWidth_ == 0 ? width : editDiceUnitWidth_ );
            diceData.Add( editDiceUnitHeight_ == 0 ? height : editDiceUnitHeight_ );
        }
        
        if ( _rect.width <= 0 || _rect.height <= 0 ) {
            diceData.Add( DiceEnumerator.EMPTY );
        }
        else {
            if ( _rect.width == diceUnitWidth && _rect.height == diceUnitHeight ) {
                diceData.Add( _rotated ? DiceEnumerator.MAX_ROTATED : DiceEnumerator.MAX );
                // TODO: use y instead of this flag
            }
            else {
                diceData.Add( (int)_rect.x );   // trim_x
                diceData.Add( (int)_rect.y );   // trim_y
                diceData.Add( _rotated ? (int)-_rect.width : (int)_rect.width );
                diceData.Add( (int)_rect.height );
            }
            diceData.Add( _x );
            diceData.Add( _y );
        }
        
        // DELME { 
        // List<float> data = new List<float>( _tileRects.Length * 6 + 2 );
        // 
        // // save committed value
        // data.Add( editDiceUnitWidth );
        // data.Add( editDiceUnitHeight );
        // 
        // bool hasTile = false;
        // for ( int i = 0; i < _tileRects.Length; ++i ) {
        //     Rect rect = _tileRects[i];
        //     if ( rect.width <= 0 || rect.height <= 0 ) {
        //         data.Add( DiceEnumerator.EMPTY );
        //         continue;
        //     }
        //     if (rect.width == editDiceUnitWidth && rect.height == editDiceUnitHeight) {
        //         data.Add( _rotated[i] ? DiceEnumerator.MAX_ROTATED : DiceEnumerator.MAX );
        //     }
        //     else {
        //         data.Add( _tileRects[i].x );   // trim_x
        //         data.Add( _tileRects[i].y );   // trim_y
        //         data.Add( _rotated[i] ? - _tileRects[i].width : _tileRects[i].width );
        //         data.Add( _tileRects[i].height );
        //     }
        //     data.Add( _x[i] );
        //     data.Add( _y[i] );
        //     hasTile = true;
        // }
        // if (hasTile == false) {
        //     data.RemoveRange (2, data.Count - 2);
        // }
        // diceData = data;
        // } DELME end 
    }
    
#endif

}

namespace ex2D.Detail {

///////////////////////////////////////////////////////////////////////////////
//
/// The dice data enumerator. Use struct type to avoid GC.
//
///////////////////////////////////////////////////////////////////////////////

public struct DiceEnumerator : IEnumerator<exTextureInfo.Dice>, IEnumerable<exTextureInfo.Dice> {

    // compressing tag
    public const int EMPTY = -1;
    public const int MAX = -2;
    public const int MAX_ROTATED = -3;
    
    private List<int> diceData;
    private int dataIndex;
    private int diceUnitWidth;
    private int diceUnitHeight;
    
#if UNITY_EDITOR
    public int columnCount;
    public int rowCount;
    public int tileIndex;     ///< current tile index
    public exTextureInfo textureInfo;
#endif

    public DiceEnumerator (List<int> _diceData, exTextureInfo _textureInfo) {
        diceData = _diceData;
        diceUnitWidth = _diceData[0];
        diceUnitHeight = _diceData[1];
        textureInfo = _textureInfo;
        dataIndex = -1;
#if UNITY_EDITOR
        exSpriteUtility.GetDicingCount(textureInfo, out columnCount, out rowCount);
        tileIndex = -1;
#endif
        Reset();
    }

    public IEnumerator<exTextureInfo.Dice> GetEnumerator () { return this; }
    IEnumerator IEnumerable.GetEnumerator () { return this; }

    public exTextureInfo.Dice Current {
        get {
            exTextureInfo.Dice d = new exTextureInfo.Dice();
#if UNITY_EDITOR
            d.enumerator = this;
#endif
            if (diceData[dataIndex] == EMPTY) {
                d.sizeType = exTextureInfo.DiceType.Empty;
                return d;
            }
            if (diceData[dataIndex] >= 0) {
                d.sizeType = exTextureInfo.DiceType.Trimmed;
                d.offset_x = diceData[dataIndex];
                d.offset_y = diceData[dataIndex + 1];
                d.width = diceData[dataIndex + 2];
                d.height = diceData[dataIndex + 3];
                d.x = diceData[dataIndex + 4];
                d.y = diceData[dataIndex + 5];
                if (d.width < 0) {
                    d.rotated = true;
                    d.width = -d.width;
                }
            }
            else {
                exDebug.Assert(diceData[dataIndex] == MAX || diceData[dataIndex] == MAX_ROTATED);
                d.sizeType = exTextureInfo.DiceType.Max;
                d.x = diceData[dataIndex + 1];
                d.y = diceData[dataIndex + 2];
                d.width = diceUnitWidth;
                d.height = diceUnitHeight;
                d.rotated = (diceData[dataIndex] == MAX_ROTATED);
            }
            return d;
        }
    }
    
    public bool MoveNext () {
        ++tileIndex;
        if (dataIndex == -1) {
            dataIndex = 2;  // skip width and height
            if (diceData == null) {
                return false;
            }
        }
        else if (diceData[dataIndex] >= 0) {
            dataIndex += 6;
        }
        else if (diceData[dataIndex] == MAX || diceData[dataIndex] == MAX_ROTATED) {
            dataIndex += 3;
        }
        else {
            exDebug.Assert (diceData[dataIndex] == EMPTY);
            dataIndex += 1;
        }
        return dataIndex < diceData.Count;
    }

    public void Dispose () {
        diceData = null;
    }

    object System.Collections.IEnumerator.Current {
        get { return Current; }
    }

    public void Reset () {
        dataIndex = -1;
#if UNITY_EDITOR
        tileIndex = -1;
#endif
    }
}
}