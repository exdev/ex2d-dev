// ======================================================================================
// File         : exClipping.cs
// Author       : 
// Last Change  : 10/07/2013
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// A component handles the a list of exPlane GameObjects, clip them
/// to the boundingRect.
///
///////////////////////////////////////////////////////////////////////////////

[AddComponentMenu("ex2D/Clipping")]
public class exClipping : exPlane {
    
    const string shaderPostfix = " (Clipping)";
    
    ///////////////////////////////////////////////////////////////////////////////
    // serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    private bool clipChildren_ = true;
    public bool clipChildren {
        get {
            return clipChildren_;
        }
        set {
            clipChildren_ = value;
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    private Dictionary<MaterialTableKey, Material> materialTable = new Dictionary<MaterialTableKey, Material>(MaterialTableKey.Comparer.instance);
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////
    
    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////
    
    // ------------------------------------------------------------------ 
    /// Add a sprite to this clip. 
    /// NOTE: You can also use exSpriteBase.SetClip for convenience.
    // ------------------------------------------------------------------ 

    public void Add (exSpriteBase _sprite) {
        exClipping oldClip = _sprite.clip;
        if (ReferenceEquals (oldClip, this)) {
            return;
        }
        if (oldClip != null) {
            oldClip.Remove (_sprite);
        }
        if (clipChildren_) {
            exSpriteBase[] spritesToAdd = _sprite.GetComponentsInChildren<exSpriteBase> (true);
            for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
                spritesToAdd [spriteIndex].SetClip(this);
            }
            if (_sprite.transform.IsChildOf (transform) == false) {
                _sprite.transform.parent = transform;
            }
        }
        else {
            _sprite.SetClip(this);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Add (GameObject _gameObject) {
        if (clipChildren_) {
            exSpriteBase[] spritesToAdd = _gameObject.GetComponentsInChildren<exSpriteBase> (true);
            for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
                spritesToAdd [spriteIndex].SetClip(this);
            }
            if (_gameObject.transform.IsChildOf (transform) == false) {
                _gameObject.transform.parent = transform;
            }
        }
        else {
            exSpriteBase sprite = _gameObject.GetComponent<exSpriteBase> ();
            Add (sprite);
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _sprite) {
        if (clipChildren_) {
            Remove(_sprite.gameObject);
        }
        else {
            if (ReferenceEquals(_sprite.clip, this)) {
                _sprite.SetClip(null);
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (GameObject _gameObject) {
        if (clipChildren_) {
            exSpriteBase[] spritesToRemove = _gameObject.GetComponentsInChildren<exSpriteBase> (true);
            for (int i = 0; i < spritesToRemove.Length; ++i) {
                Remove (spritesToRemove [i]);
            }
        }
        else {
            exSpriteBase sprite = _gameObject.GetComponent<exSpriteBase> ();
            if (sprite != null) {
                Remove (sprite);
            }
        }
    }
    
    // ------------------------------------------------------------------ 
    /// Return shared material matchs given shader and texture for the clipping
    // ------------------------------------------------------------------ 

    private Material GetClippedMaterial (Shader _shader, Texture _texture) {
        if (_shader == null) {
            _shader = Shader.Find("ex2D/Alpha Blended" + shaderPostfix);
            if (_shader == null) {
                return null;
            }
        }
        else {
            Shader clipShader = Shader.Find(_shader.name + shaderPostfix);
            if (clipShader == null) {
                Debug.LogError("Failed to find clip shader named " + _shader.name + shaderPostfix);
                return null;
            }
            _shader = clipShader;
        }
        MaterialTableKey key = new MaterialTableKey(_shader, _texture);
        Material mat;
        if ( !materialTable.TryGetValue(key, out mat) || mat == null ) {
            mat = new Material(_shader);
            mat.hideFlags = HideFlags.DontSave;
            mat.mainTexture = _texture;
            materialTable[key] = mat;
        }
        return mat;
    }
}
