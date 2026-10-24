// 해킹 빔 v2 (LineRenderer용) - 진한 빨강 버전
// - 빔 안쪽을 어두운 색으로 채우고, 그 위에 진한 빨간 0/1 숫자를 그립니다.
// - 이전 버전(Additive)은 배경색에 더해져서 밝은 배경에서 연하게 보였는데,
//   이 버전은 일반 투명(Alpha) 방식이라 어떤 배경에서도 색이 진하게 유지됩니다.
//
// 설정
// - LineRenderer: Texture Mode = Tile, Use World Space 체크, Width 0.5~1
// - BinaryTex(T_BinaryDigits) Import: Filter Mode = Point, Wrap Mode = Repeat, Compression = None
// - 숫자가 세로로 길쭉하거나 납작하면: Tiling X = 0.25 × Tiling Y ÷ LineRenderer Width
// - LineRenderer Color의 알파를 줄이면 빔이 서서히 사라집니다.
Shader "Custom/HackBeam"
{
    Properties
    {
        [Header(Texture)]
        [NoScaleOffset] _BinaryTex ("Binary Tex", 2D) = "black" {}
        _Tiling      ("Tiling (X: 길이 방향, Y: 두께 방향)", Vector) = (0.25, 1, 0, 0)
        _DigitCell   ("Digit Cell px (텍스처를 안 바꿨으면 그대로)", Vector) = (4, 8, 0, 0)
        _ScrollSpeed ("Scroll Speed", Float) = 2

        [Header(Colors)]
        [HDR] _DigitColor ("Digit Color (숫자)", Color) = (1, 0.08, 0.06, 1)
        [HDR] _GlowColor  ("Glow Color (숫자 주변 번짐)", Color) = (0.75, 0, 0.02, 1)
        _BackColor        ("Back Color (빔 안쪽 배경)", Color) = (0.06, 0, 0.01, 1)
        _BackAlpha        ("Back Alpha (0이면 배경 없이 숫자만)", Range(0, 1)) = 0.8
        [HDR] _CoreColor  ("Core Color (가운데 심지)", Color) = (1, 0.2, 0.15, 1)
        _CoreStrength     ("Core Strength (0이면 심지 없음)", Range(0, 2)) = 0.5
        _CoreSharpness    ("Core Sharpness (클수록 얇음)", Float) = 12

        [Header(Effects)]
        _BackLayer     ("Back Layer (뒤쪽 작은 숫자 밝기)", Range(0, 1)) = 0.5
        _GlowStrength  ("Glow Strength", Range(0, 2)) = 0.9
        _GlowSize      ("Glow Size (픽셀)", Range(0, 4)) = 1.5
        _FlickerFPS    ("Flicker FPS (0이면 안 깜빡임)", Float) = 6
        _FlickerAmount ("Flicker Amount (숫자마다 밝기 차이)", Range(0, 1)) = 0.5
        _EdgeSoftness  ("Edge Softness (빔 위아래 흐려짐)", Range(0.01, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha    // 일반 투명 (배경색에 섞이지 않고 진하게)
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;      // LineRenderer의 Color
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            TEXTURE2D(_BinaryTex);
            SAMPLER(sampler_BinaryTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BinaryTex_TexelSize;   // Unity가 자동으로 채움 (1/너비, 1/높이, 너비, 높이)
                float4 _Tiling;
                float4 _DigitCell;
                float  _ScrollSpeed;
                float4 _DigitColor;
                float4 _GlowColor;
                float4 _BackColor;
                float  _BackAlpha;
                float4 _CoreColor;
                float  _CoreStrength;
                float  _CoreSharpness;
                float  _BackLayer;
                float  _GlowStrength;
                float  _GlowSize;
                float  _FlickerFPS;
                float  _FlickerAmount;
                float  _EdgeSoftness;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            float Hash (float2 p)
            {
                p = frac(p / 1024.0) * 1024.0;     // 값이 너무 커지지 않게
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float Digit (float2 uv)
            {
                return SAMPLE_TEXTURE2D(_BinaryTex, sampler_BinaryTex, uv).r;
            }

            // 숫자 한 칸마다 다른 밝기 (일정 간격으로 바뀜)
            float Flicker (float2 uv, float seed)
            {
                float2 cells = _BinaryTex_TexelSize.zw / max(_DigitCell.xy, 1.0);
                float2 cell  = floor(uv * cells);
                float  tick  = floor(_Time.y * _FlickerFPS);
                float  r     = Hash(cell + float2(tick * 7.13 + seed, tick * 3.71));
                return lerp(1.0, r, _FlickerAmount);
            }

            // 주변 픽셀을 모아서 숫자 둘레에 번지는 빛
            float Glow (float2 uv)
            {
                float2 o = _BinaryTex_TexelSize.xy * _GlowSize;
                float g = 0;
                g += Digit(uv + float2( o.x, 0)) + Digit(uv + float2(-o.x, 0));
                g += Digit(uv + float2(0,  o.y)) + Digit(uv + float2(0, -o.y));
                g += (Digit(uv + o) + Digit(uv - o)
                    + Digit(uv + float2(o.x, -o.y)) + Digit(uv + float2(-o.x, o.y))) * 0.7;
                g += (Digit(uv + float2(2 * o.x, 0)) + Digit(uv - float2(2 * o.x, 0))
                    + Digit(uv + float2(0, 2 * o.y)) + Digit(uv - float2(0, 2 * o.y))) * 0.5;
                return g / 8.8;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // A. 숫자 흐르게 (두 겹)
                float2 flow = float2(-_Time.y * _ScrollSpeed, 0);
                float2 uv1  = IN.uv * _Tiling.xy + flow;
                float2 uv2  = IN.uv * _Tiling.xy * 1.6 + flow * 0.6 + float2(0.31, 0.37);

                float f1   = Flicker(uv1, 0);
                float d1   = Digit(uv1) * f1;
                float d2   = Digit(uv2) * Flicker(uv2, 50) * _BackLayer;
                float glow = Glow(uv1) * f1 * _GlowStrength;

                // B. 빔 모양
                float profile = 1 - abs(IN.uv.y - 0.5) * 2;                          // 가운데 1, 끝 0
                float edge    = smoothstep(0, _EdgeSoftness, profile);
                float core    = pow(saturate(profile), _CoreSharpness) * _CoreStrength;

                // C. 색 쌓기: 어두운 배경 → 번짐 → 뒤쪽 숫자 → 앞쪽 숫자 → 심지
                float3 col = _BackColor.rgb;
                col += _GlowColor.rgb * glow;
                col  = lerp(col, _DigitColor.rgb * 0.6, saturate(d2));
                col  = lerp(col, _DigitColor.rgb, saturate(d1));
                col += _CoreColor.rgb * core;
                col *= IN.color.rgb;

                float lit   = saturate(d1 + d2 + glow * 0.8 + core);
                float alpha = saturate(max(_BackAlpha, lit)) * edge * IN.color.a;

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
