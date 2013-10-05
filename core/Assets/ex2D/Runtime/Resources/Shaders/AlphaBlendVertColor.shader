// ======================================================================================
// File         : AlphaBlendVertColor.shader
// Author       : Wu Jie 
// Last Change  : 09/13/2013 | 15:04:52 PM | Friday,September
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

Shader "ex2D/Alpha Blended (Use Vertex Color)" {
    Properties {
        _MainTex ("Atlas Texture", 2D) = "white" {}
    }

    Category {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

        // SubShader {
        //     BindChannels {
        //         Bind "Color", color
        //         Bind "Vertex", vertex
        //         Bind "TexCoord", texcoord
        //     }

        //     Pass {
        //         SetTexture [_MainTex] {
        //             combine texture * primary
        //         }
        //     }
        // }

        SubShader {
            pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                uniform sampler2D _MainTex;

                struct appdata_t {
                    float4 vertex   : POSITION;
                    fixed4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                };

                struct v2f {
                    float4 pos        : SV_POSITION;
                    fixed4 color      : COLOR;
                    float2 uv0        : TEXCOORD0;
                };

                v2f vert ( appdata_t _in ) {
                    v2f o;
                    o.pos = mul (UNITY_MATRIX_MVP, _in.vertex);
                    // Texture offset - GOOD
                    o.uv0 = _in.texcoord;
                    o.color = _in.color;
                    return o;
                }

                fixed4 frag ( v2f _in ) : COLOR {
                    fixed4 main = tex2D(_MainTex, _in.uv0);
                    main.rgb = _in.color.rgb;
                    main.a = main.a * _in.color.a;
                    return main;
                }
                ENDCG
            }
        }
    }
}
