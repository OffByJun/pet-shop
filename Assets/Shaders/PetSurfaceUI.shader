Shader "PetShop/PetSurfaceUI"
{
    // 펫 파츠 스프라이트 위에 표면 상태를 입혀 그립니다.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _SurfaceTex ("Surface", 2D) = "black" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _DirtColor ("Dirt Colour", Color) = (0.40, 0.30, 0.19, 1)
        _FoamColor ("Foam Colour", Color) = (0.97, 0.98, 1.0, 1)
        _DirtStrength ("Dirt Strength", Range(0,1)) = 0.85
        _WetStrength ("Wet Strength", Range(0,1)) = 0.55
        _FoamStrength ("Foam Strength", Range(0,1)) = 0.9
        _MessStrength ("Mess Strength", Range(0,1)) = 0.35

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _SurfaceTex;
            fixed4 _Color;
            fixed4 _DirtColor;
            fixed4 _FoamColor;
            float _DirtStrength;
            float _WetStrength;
            float _FoamStrength;
            float _MessStrength;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 sprite = tex2D(_MainTex, i.texcoord) * i.color;
                // 모든 파츠가 같은 캔버스를 공유하므로 스프라이트 UV가 곧 펫 표면 좌표입니다.
                float4 s = tex2D(_SurfaceTex, i.texcoord);

                float3 col = sprite.rgb;

                // 헝클어진 결은 가늘고 어두운 줄로 보입니다.
                float mess = saturate(1.0 - s.b);
                float streak = sin(i.texcoord.y * 220.0) * 0.5 + 0.5;
                col *= 1.0 - mess * streak * _MessStrength * 0.35;

                // 오염은 색을 흙빛으로 끌어당깁니다.
                col = lerp(col, _DirtColor.rgb, saturate(s.r) * _DirtStrength);

                // 젖으면 어두워지고 살짝 채도가 죽습니다.
                float wet = saturate(s.g) * _WetStrength;
                float grey = dot(col, float3(0.299, 0.587, 0.114));
                col = lerp(col, lerp(col, grey.xxx, 0.18) * 0.76, wet);

                // 거품은 위에 하얗게 얹힙니다.
                col = lerp(col, _FoamColor.rgb, saturate(s.a) * _FoamStrength);

                return fixed4(col, sprite.a);
            }
            ENDCG
        }
    }
}
