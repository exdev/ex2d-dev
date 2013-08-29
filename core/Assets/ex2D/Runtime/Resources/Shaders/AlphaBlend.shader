// ======================================================================================
// File         : AlphaBlend.shader
// Author       : Wu Jie, Fredrik Ludvigsen
// Last Change  : 08/28/2013 | 11:41:33 PM | Wednesday,August
// Description  : 
// ======================================================================================

///////////////////////////////////////////////////////////////////////////////
//
///////////////////////////////////////////////////////////////////////////////

Shader "ex2D/Alpha Blended" {
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

        SubShader {
            BindChannels {
                Bind "Color", color
                Bind "Vertex", vertex
                Bind "TexCoord", texcoord
            }

            Pass {
                SetTexture [_MainTex] {
                    combine texture * primary
                }
            }
        }

        SubShader {
            pass {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;

                struct fragInput {
                    float4 pos        : SV_POSITION;
                    float2 uv_MainTex : TEXCOORD0;
                    float4 color      : COLOR;
                };

                fragInput vert( appdata_full v ) {
                    fragInput o;
                    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                    // Texture offset - GOOD
                    o.uv_MainTex = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.color = v.color;
                    return o;
                }

                float4 frag(fragInput i) : COLOR {
                    fixed4 main = tex2D(_MainTex, i.uv_MainTex);
                    return float4(main * i.color);
                }
                ENDCG
            }
        }
    }
}
