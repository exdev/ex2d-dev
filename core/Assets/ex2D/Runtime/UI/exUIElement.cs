// ======================================================================================
// File         : exUIElement.cs
// Author       : Wu Jie 
// Last Change  : 08/30/2013 | 22:52:19 PM | Friday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

///////////////////////////////////////////////////////////////////////////////
///
/// The ui-layout element
///
///////////////////////////////////////////////////////////////////////////////

[System.Serializable]
public class exUIElement {
    public string name = "New Element";
    public string id = "el"; // for css
    public string content = "";
    public exUIStyle style;

    public List<exUIElement> children = new List<exUIElement>();

    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////

    public exUIElement parent_ = null;
    public exUIElement parent {
        set {
            if ( !ReferenceEquals(parent_, value) ) {
                if ( !ReferenceEquals(parent_, null) ) {
                    parent_.children.Remove(this);
                    parent_ = null;
                }

                if ( !ReferenceEquals(value, null) ) {
                    parent_ = value;
                    if ( parent_.children.IndexOf (this) == -1 ) {
                        parent_.children.Add(this);
                    }
                }
            }
        }
        get {
            return parent_;
        }
    }

    [System.NonSerialized] public Vector3 position;
    [System.NonSerialized] public Vector3 size;

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void AddElement ( exUIElement _el ) {
        _el.parent = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void RemoveElement ( exUIElement _el ) {
        if ( children.IndexOf(_el) != -1 ) {
            _el.parent = null;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public int GetElementIndex ( exUIElement _el ) {
        return children.IndexOf(_el);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void InsertAt ( int _idx, exUIElement _el ) {
        int idx = _idx;
        if ( _el.parent == this ) {
            int curIdx = this.children.IndexOf(_el);
            exDebug.Assert( curIdx != -1, 
                            "Can't find the element in the parent children list.", 
                            true );
            if ( curIdx == _idx )
                return;

            if ( curIdx > _idx ) 
                idx = curIdx - 1;
        }

        _el.parent = null;
        this.children.Insert( idx, _el );
        _el.parent_ = this;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public bool Exists ( exUIElement _el ) {
        if ( this != _el ) {
            for ( int i = 0; i < this.children.Count; ++i ) {
                exUIElement child = this.children[i];
                if ( child.Exists (_el) )
                    return true;
            }

            return false;
        }
        return true;
    } 
}


