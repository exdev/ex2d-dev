// ======================================================================================
// File         : AlphaBlendNoZTest.shader
// Author       : Wu Jie 
// Last Change  : 06/11/2013 | 17:24:37 PM | Tuesday,June
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

Shader "ex2D/Alpha Blended (No ZTest)" {
    Properties {
        _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    Category {
        Tags { 
            "Queue"="Overlay+1" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }
        Cull Off 
        Lighting Off 
        ZWrite Off 
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        BindChannels {
            Bind "Color", color
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }

        SubShader {
            Pass {
                SetTexture [_MainTex] {
                    combine texture * primary
                }
            }
        }
    }
}
