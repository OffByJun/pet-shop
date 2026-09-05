Shader "PetShop/PetSurfaceStamp"
{
    // 표면 상태 텍스처를 한 번 갱신합니다.
    //   R = 오염   G = 젖음   B = 털 정돈도   A = 거품
    Properties
    {
        _MainTex ("Surface", 2D) = "black" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _StampUV;      // xy = 중심, z = 반지름, w = 세기
            float4 _StampDir;     // xy = 진행 방향(정규화), z = 도구 종류, w = 델타타임
            float  _Aspect;

            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata_img v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 s = tex2D(_MainTex, i.uv);

                float2 d = i.uv - _StampUV.xy;
                d.x *= _Aspect;
                float radius = max(_StampUV.z, 1e-4);
                // 가장자리로 갈수록 부드럽게 약해지는 원형 브러시입니다.
                float falloff = 1.0 - smoothstep(radius * 0.30, radius, length(d));
                float amount = falloff * _StampUV.w;

                uint kind = (uint)(_StampDir.z + 0.5);

                if (amount > 0.0)
                {
                    if (kind == 0)          // 물: 적시고 헹궈 냅니다.
                    {
                        s.g = saturate(s.g + amount);
                        s.r = saturate(s.r - amount * 0.10);
                        s.a = saturate(s.a - amount * 0.55);
                    }
                    else if (kind == 1)     // 비누: 젖은 곳에만 거품이 섭니다.
                    {
                        s.a = saturate(s.a + amount * saturate(s.g * 1.4));
                    }
                    else if (kind == 2)     // 브러시: 젖고 거품 낸 곳일수록 잘 지워집니다.
                    {
                        float power = 0.25 + s.g * 0.45 + s.a * 0.60;
                        s.r = saturate(s.r - amount * power);
                        s.a = saturate(s.a - amount * 0.30);
                        // 오른쪽으로 빗으면 결이 정돈되고, 거스르면 헝클어집니다.
                        float align = dot(normalize(_StampDir.xy + 1e-6), float2(1.0, 0.0));
                        s.b = saturate(s.b + amount * align * 0.9);
                    }
                    else if (kind == 3)     // 수건: 물기를 걷어 냅니다.
                    {
                        s.g = saturate(s.g - amount * 0.9);
                        s.a = saturate(s.a - amount * 0.4);
                    }
                    else                    // 가위: 헝클어진 결을 잘라 정돈합니다.
                    {
                        s.b = saturate(s.b + amount * 0.7);
                    }
                }

                // 시간이 지나면 물기와 거품이 스스로 마릅니다.
                float dry = _StampDir.w;
                s.g = saturate(s.g - dry * 0.030);
                s.a = saturate(s.a - dry * 0.015);
                return s;
            }
            ENDCG
        }
    }
}
