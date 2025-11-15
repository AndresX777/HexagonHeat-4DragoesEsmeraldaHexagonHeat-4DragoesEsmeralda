Shader "Hidden/AccessibilityFilter"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Contrast("Contrast", Float) = 1
        _ApplyMatrix("Apply Matrix", Float) = 0

    }

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Contrast;
            float _ApplyMatrix;

            float4x4 _ColorMatrix;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv);

                // High Contrast
                col.rgb = ((col.rgb - 0.5) * _Contrast) + 0.5;

                // Color-blindness
                if (_ApplyMatrix > 0.5)
                    col.rgb = mul(_ColorMatrix, float4(col.rgb, 1)).rgb;

                return col;
            }
            ENDCG
        }
    }
}

