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
    //
    ///////////////////////////////////////////////////////////////////////////////

    public override float width {
        get { return width_; }
        set { 
            if ( width_ != value ) {
                width_ = value; 
                dirty = true;
            }
        }
    }

    public override float height {
        get { return height_; }
        set { 
            if ( height_ != value ) {
                height_ = value; 
                dirty = true;
            }
        }
    }

    public override Anchor anchor {
        get { return anchor_; }
        set { 
            if ( anchor_ != value ) {
                anchor_ = value;
                dirty = true;
            }
        }
    }
    
    public override Vector2 offset {
        get { return offset_; }
        set { 
            if ( offset_ != value ) {
                offset_ = value; 
                dirty = true;
            }
        }
    }
    
    ///////////////////////////////////////////////////////////////////////////////
    // non-serialized
    ///////////////////////////////////////////////////////////////////////////////
    
    private Dictionary<MaterialTableKey, Material> materialTable = 
        new Dictionary<MaterialTableKey, Material>(MaterialTableKey.Comparer.instance);

    private Vector2 currentPos = Vector2.zero;
    private bool dirty = false;
    
    ///////////////////////////////////////////////////////////////////////////////
    // Overridable functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Awake () {
        // TODO: NOTE: without GetClippedMaterial, the material table can't store new material, and UpdateClipMaterials will do nothing { 
        exSpriteBase[] spritesToAdd = gameObject.GetComponentsInChildren<exSpriteBase> (true);
        for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
            exSpriteBase sprite = spritesToAdd [spriteIndex];
            sprite.clip = this;
            Material tmp = sprite.material;
        }
        // } TODO end 

        currentPos = new Vector2( transform.position.x, transform.position.y ); 
        dirty = false;

        UpdateClipMaterials ();
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void OnDestroy () {
        Remove (gameObject);
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void LateUpdate () {
        if ( transform.hasChanged ) {
            Vector2 newPos = new Vector2( transform.position.x, transform.position.y );
            if ( newPos != currentPos ) {
                currentPos = newPos; 
                dirty = true;
            }
        }

        if ( dirty ) {
            UpdateClipMaterials ();
            dirty = false;
        }
    } 
    
    ///////////////////////////////////////////////////////////////////////////////
    // Other functions
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void SetDirty () {
        dirty = true;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    public void CheckDirty () {
        if ( dirty ) {
            UpdateClipMaterials ();
            dirty = false;
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateClipMaterials () {
        Rect rect = GetWorldAABoundingRect ();
        Vector4 clipRect = new Vector4( rect.center.x, rect.center.y, rect.width, rect.height ); 
        foreach ( Material mat in materialTable.Values ) {
            mat.SetVector ( "_ClipRect", clipRect );
        }
    }
    
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
        exSpriteBase[] spritesToAdd = _sprite.GetComponentsInChildren<exSpriteBase> (true);
        for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
            spritesToAdd [spriteIndex].clip = this;
        }
        if (_sprite.transform.IsChildOf (transform) == false) {
            _sprite.transform.parent = transform;
        }

        dirty = true;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Add (GameObject _gameObject) {
        exSpriteBase[] spritesToAdd = _gameObject.GetComponentsInChildren<exSpriteBase> (true);
        for (int spriteIndex = 0; spriteIndex < spritesToAdd.Length; ++spriteIndex) {
            spritesToAdd [spriteIndex].clip = this;
        }
        if (_gameObject.transform.IsChildOf (transform) == false) {
            _gameObject.transform.parent = transform;
        }

        dirty = true;
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (exSpriteBase _sprite) {
        Remove(_sprite.gameObject);
    }

    // ------------------------------------------------------------------ 
    // Desc:
    // ------------------------------------------------------------------ 

    public void Remove (GameObject _gameObject) {
        exSpriteBase[] spritesToRemove = _gameObject.GetComponentsInChildren<exSpriteBase> (true);
        for (int i = 0; i < spritesToRemove.Length; ++i) {
            spritesToRemove[i].clip = null;
        }

        dirty = true;
    }

    // ------------------------------------------------------------------ 
    /// Return shared material matchs given shader and texture for the clipping
    // ------------------------------------------------------------------ 

    public Material GetClippedMaterial (Shader _shader, Texture _texture) {
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

            // also update clip rect
            Rect rect = GetWorldAABoundingRect ();
            Vector4 clipRect = new Vector4( rect.center.x, rect.center.y, rect.width, rect.height ); 
            mat.SetVector ( "_ClipRect", clipRect );
            materialTable[key] = mat;
        }
        return mat;
    }
}
