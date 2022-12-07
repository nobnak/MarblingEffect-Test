Shader "Marbling/Surface" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _OffsetTex ("Offset", 2D) = "black" {}
    }
    SubShader {
        CGINCLUDE
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 clip : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
        ENDCG

        // Render
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _OffsetTex;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.clip = o.vertex;
                return o;
            }
            float4 frag (v2f i) : SV_Target {
                float2 uv = tex2D(_OffsetTex, i.uv).xy;
                float2 uv_MainTex = TRANSFORM_TEX(uv, _MainTex);
                float4 csrc = tex2D(_MainTex, uv_MainTex);
                return csrc;
            }
            ENDCG
        }
    }
}
