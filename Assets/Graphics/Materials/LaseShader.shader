Shader "Custom/SnappyLaserShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {} // Main texture for UI
        _NoiseTex ("Noise Texture", 2D) = "white" {} // Noise texture
        _Color ("Color", Color) = (1,1,1,1) // Line color
        _GlowColor ("Glow Color", Color) = (1,0,0,1) // Glow color
        _GlowIntensity ("Glow Intensity", Range(0, 100)) = 1 // Intensity of glow
        _Width ("Width", Range(0.001, 0.1)) = 0.01 // Width of the laser
        _Softness ("Softness", Range(0, 1)) = 0.5 // Softness of the laser edges
        _NoiseScale ("Noise Scale", Range(1, 10)) = 1 // Scale of noise texture
        _NoiseSpeed ("Noise Speed", Range(0, 10)) = 1 // Speed of noise animation
        _AnimationSpeed ("Animation Speed", Range(0.1, 10)) = 1 // Speed of animation
        _PlasmaFrequency ("Plasma Frequency", Range(1, 100)) = 3 // Frequency of plasma effect
        _PlasmaSpeed ("Plasma Speed", Range(0.1, 100)) = 1 // Speed of plasma animation
        _PlasmaIntensity ("Plasma Intensity", Range(0, 1)) = 0.5 // Intensity of plasma effect
        _PlasmaColor1 ("Plasma Color 1", Color) = (1,0,0,1) // Color 1 of plasma effect
        _PlasmaColor2 ("Plasma Color 2", Color) = (0,0,1,1) // Color 2 of plasma effect
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" "UnityUI"="true" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex; // Noise texture sampler
            float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _GlowIntensity;
            float _Width;
            float _Softness;
            float _NoiseScale;
            float _NoiseSpeed;
            float _AnimationSpeed;
            float _PlasmaFrequency;
            float _PlasmaSpeed;
            float _PlasmaIntensity;
            float4 _PlasmaColor1;
            float4 _PlasmaColor2;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Pass UV coordinates for the main texture
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Generate noise using the noise texture
                float2 noiseUV = i.uv * _NoiseScale + _Time.y * _NoiseSpeed;
                float noise = tex2D(_NoiseTex, noiseUV).r;

                // Calculate distance from center line (range 0 to 1)
                float distFromCenter = abs(i.uv.y - 0.5) / (_Width * 0.5);
                float softFactor = smoothstep(0, _Softness, distFromCenter);

                // Animate glow intensity based on time
                float glowIntensity = sin(_Time.y * _AnimationSpeed) * 0.5 + 0.5;
                glowIntensity *= _GlowIntensity * softFactor;

                // Apply noise to color
                col.rgb *= noise;

                // Apply glow effect
                col.rgb += _GlowColor.rgb * glowIntensity;

                // Calculate plasma effect
                float2 plasmaUV = i.uv * _PlasmaFrequency + _Time.y * _PlasmaSpeed;
                float plasma = (sin(plasmaUV.x) + sin(plasmaUV.y)) * 0.5 * _PlasmaIntensity + 0.5;

                // Interpolate between plasma colors based on plasma effect
                col.rgb = lerp(_PlasmaColor1.rgb, _PlasmaColor2.rgb, plasma);

                // Apply alpha from main texture
                col.a *= tex2D(_MainTex, i.uv).a;

                return col;
            }
            ENDCG
        }
    }
    Fallback "UI/Default"
}
