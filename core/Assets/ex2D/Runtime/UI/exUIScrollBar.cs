// ======================================================================================
// File         : exUIScrollBar.cs
// Author       : Wu Jie 
// Last Change  : 10/19/2013 | 17:09:10 PM | Saturday,October
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
// usings
///////////////////////////////////////////////////////////////////////////////

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

///////////////////////////////////////////////////////////////////////////////
///
/// Dispatch Event
///
///////////////////////////////////////////////////////////////////////////////

public class exUIScrollBar : exUIControl {

	public enum Direction {
		Vertical,
		Horizontal,
	};

	public Direction direction = Direction.Vertical;
    public exUIScrollView scrollView = null;
    public float cooldown = 0.5f;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    protected exSprite bar = null;
    protected exSprite background = null;

    protected float ratio = 1.0f;
    protected float scrollOffset = 0.0f;
    protected Vector3 scrollStart = Vector3.zero;

    protected bool dragging = false;
    protected int draggingID = -1;
    protected float cooldownTimer = 0.0f;
    protected bool isCoolingDown = false;

    ///////////////////////////////////////////////////////////////////////////////
    //
    ///////////////////////////////////////////////////////////////////////////////

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    protected new void Awake () {
        base.Awake();

        // handle scroll bar
        Transform transBar = transform.Find("__bar");
        if ( transBar ) {
            bar = transBar.GetComponent<exSprite>();
            if ( bar ) {
                bar.customSize = true;
                bar.anchor = Anchor.TopLeft;
            }

            //
            exUIButton btnBar = transBar.GetComponent<exUIButton>();
            if ( btnBar ) {
                btnBar.grabMouseOrTouch = true;

                btnBar.onPressDown += delegate ( exUIControl _sender, exHotPoint _point ) {
                    if ( dragging )
                        return;

                    if ( _point.isTouch || _point.GetMouseButton(0) ) {
                        dragging = true;
                        draggingID = _point.id;

                        exUIMng.inst.SetFocus(this);
                    }
                };

                btnBar.onPressUp += delegate ( exUIControl _sender, exHotPoint _point ) {
                    if ( ( _point.isTouch || _point.GetMouseButton(0) ) && _point.id == draggingID ) {
                        if ( dragging ) {
                            dragging = false;
                            draggingID = -1;
                        }
                    }
                };

                btnBar.onHoverMove += delegate ( exUIControl _sender, List<exHotPoint> _points ) {
                    if ( scrollView ) {
                        for ( int i = 0; i < _points.Count; ++i ) {
                            exHotPoint point = _points[i];
                            if ( dragging && ( point.isTouch || point.GetMouseButton(0) ) && point.id == draggingID  ) {
                                Vector2 delta = point.worldDelta; 
                                delta.y = -delta.y;
                                scrollView.Scroll (delta/ratio);
                            }
                        }
                    }
                };
            }
        }

        // handle background
        background = GetComponent<exSprite>();
        if ( background ) {
            scrollStart = transBar ? transBar.localPosition : Vector3.zero;

            if ( background.spriteType == exSpriteType.Sliced ) {
                if ( direction == Direction.Horizontal ) {
                    scrollStart.x = background.leftBorderSize;
                }
                else {
                    scrollStart.y = background.topBorderSize;
                }
            }
        }

        // handle scroll view
        if ( scrollView ) {
            scrollView.onContentResized += delegate ( exUIControl _sender, Vector2 _size ) {
                UpdateScrollBarRatio ();
                UpdateScrollBar ();
            };
            scrollView.onScroll += delegate ( exUIControl _sender, Vector2 _offset ) {
                if ( direction == Direction.Horizontal ) {
                    scrollOffset = _offset.x * ratio;
                }
                else {
                    scrollOffset = _offset.y * ratio;
                }
                UpdateScrollBar ();

                //
                if ( scrollView.showCondition == exUIScrollView.ShowCondition.WhenDragging ) {
                    activeSelf = true;
                }
            };
            scrollView.onScrollFinished += delegate ( exUIControl _sender ) {
                if ( scrollView.showCondition == exUIScrollView.ShowCondition.WhenDragging ) {
                    cooldownTimer = cooldown;
                    isCoolingDown = true;
                }
            };
            UpdateScrollBarRatio ();
            UpdateScrollBar ();

            // handle scrollbar effect
            if ( scrollView.showCondition == exUIScrollView.ShowCondition.WhenDragging ) {
                // 
                exUIEffect effect = GetComponent<exUIEffect>();
                if ( effect == null ) {
                    effect = gameObject.AddComponent<exUIEffect>();

                    if ( background != null ) {
                        effect.AddEffect_Color( background, 
                                                EffectEventType.Deactive,
                                                exEase.Type.Linear,
                                                new Color( background.color.r, background.color.g, background.color.b, 0.0f ),
                                                0.5f );
                    }

                    if ( bar != null ) {
                        effect.AddEffect_Color( bar, 
                                                EffectEventType.Deactive,
                                                exEase.Type.Linear,
                                                new Color( bar.color.r, bar.color.g, bar.color.b, 0.0f ),
                                                0.5f );
                    }
                }

                //
                if ( background ) {
                    Color tmpColor = background.color;
                    tmpColor.a = 0.0f;
                    background.color = tmpColor;
                }

                //
                if ( bar ) {
                    Color tmpColor = bar.color;
                    tmpColor.a = 0.0f;
                    bar.color = tmpColor;
                }

                //
                active_ = false;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void Update () {
        if ( isCoolingDown ) {
            cooldownTimer -= Time.deltaTime;
            if ( cooldownTimer <= 0.0f ) {
                isCoolingDown = false;
                activeSelf = false;
            }
        }
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateScrollBarRatio () {
        float contentSize = 0.0f;
        float bgSize = 0.0f;

        if ( direction == Direction.Horizontal ) {
            contentSize = scrollView.contentSize.x;
            bgSize = width;
            if ( background != null && background.spriteType == exSpriteType.Sliced ) {
                bgSize = bgSize - background.leftBorderSize - background.rightBorderSize; 
            }
        }
        else {
            contentSize = scrollView.contentSize.y;
            bgSize = height;
            if ( background != null && background.spriteType == exSpriteType.Sliced ) {
                bgSize = bgSize - background.topBorderSize - background.bottomBorderSize; 
            }
        }

        ratio = bgSize/contentSize;
    }

    // ------------------------------------------------------------------ 
    // Desc: 
    // ------------------------------------------------------------------ 

    void UpdateScrollBar () {
        if ( direction == Direction.Horizontal ) {
            float barSize = ratio * scrollView.width;
            float slicedOffset = 0.0f;
            float trimSize = 0.0f;
            float bgSize = width;
            float finalOffset = scrollOffset;

            //
            bar.width = ratio * scrollView.width;
            if ( bar.spriteType == exSpriteType.Sliced ) {
                bar.width = bar.width + bar.leftBorderSize + bar.rightBorderSize;
                slicedOffset = bar.leftBorderSize;
            }

            //
            if ( background != null && background.spriteType == exSpriteType.Sliced ) {
                bgSize = bgSize - background.leftBorderSize - background.rightBorderSize; 
            }

            //
            float minOffset = 0.0f;
            float maxOffset = bgSize - barSize;
            if ( finalOffset < minOffset ) {
                trimSize = minOffset - finalOffset;
                finalOffset = minOffset;
            }
            else if ( finalOffset > maxOffset ) {
                trimSize = finalOffset - maxOffset;
                finalOffset = maxOffset + trimSize;
            }
            bar.width -= trimSize;

            //
            bar.transform.localPosition = new Vector3( scrollStart.x + scrollOffset - slicedOffset,
                                                      -scrollStart.y,
                                                       scrollStart.z );
        }
        else {
            float barSize = ratio * scrollView.height;
            float slicedOffset = 0.0f;
            float trimSize = 0.0f;
            float bgSize = height;
            float finalOffset = scrollOffset;

            //
            bar.height = barSize;
            if ( bar.spriteType == exSpriteType.Sliced ) {
                bar.height = bar.height + bar.topBorderSize + bar.bottomBorderSize;
                slicedOffset = bar.topBorderSize;
            }

            //
            if ( background != null && background.spriteType == exSpriteType.Sliced ) {
                bgSize = bgSize - background.topBorderSize - background.bottomBorderSize; 
            }

            //
            float minOffset = 0.0f;
            float maxOffset = bgSize - barSize;
            if ( finalOffset < minOffset ) {
                trimSize = minOffset - finalOffset;
                finalOffset = minOffset;
            }
            else if ( finalOffset > maxOffset ) {
                trimSize = finalOffset - maxOffset;
                finalOffset = maxOffset + trimSize;
            }
            bar.height -= trimSize;

            //
            bar.transform.localPosition = new Vector3( scrollStart.x,
                                                     -(scrollStart.y + finalOffset - slicedOffset),
                                                       scrollStart.z );
        }
    }
}
