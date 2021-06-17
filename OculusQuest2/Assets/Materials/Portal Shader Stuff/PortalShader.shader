Shader "Custom/PortalShader"
{
    Properties
    { 
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [NoScaleOffset] _FlowMap ("Flow (RG, Noise)", 2D) = "black" {}
        _UJump ("U jump per phase", Range(-0.25, 0.25)) = 0.25
        _VJump ("V jump per phase", Range(-0.25, 0.25)) = 0.25
        _Tiling ("Tiling", float) = 1
        _Speed ("Speed", Float) = 1
        _Strength ("Strength", float) = 1

        _NoiseTexture("Noise Texture", 2D) = "white" {}
        // Each instance has a unique value
        [PerRendererData]_DisolveAmount("Disolve Amount", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };


            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_FlowMap);
            SAMPLER(sampler_FlowMap);

            TEXTURE2D(_NoiseTexture);
            SAMPLER(sampler_NoiseTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _FlowMap_ST;
                float4 _NoiseTexture_ST;

                half4 _Color;
                float _UJump, _VJump;
                float _Tiling;
                float _Speed;
                float _Strength;

                float _DisolveAmount;
            CBUFFER_END


            float3 FlowUV(float2 uv, float2 flowVector, float time, bool flowB)
            {
                float phaseOffset = flowB ? 0.5 : 0;
                float progress = frac(time + phaseOffset);

                float3 uvw;
                // 'Move' the UV using the flow vector
                uvw.xy = uv - flowVector * progress;
                uvw.xy *= _Tiling;
                uvw.xy += phaseOffset;
                uvw.xy += (time - progress) * float2(_UJump, _VJump);
                // Store the amount to fade this phase in the third element
                uvw.z = 1 - abs(1 - 2 * progress);

                return uvw;
            }


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Disolve effect uses a noise texture, and discards any pixel below it
                float disolve = SAMPLE_TEXTURE2D(_NoiseTexture, sampler_NoiseTexture, IN.uv).r;
                clip(disolve - _DisolveAmount);

                float2 flowVector = SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, IN.uv).rg * 2 - 1;
                flowVector *= _Strength;
                // Apply noise in flow map alpha channel to time for phase transitions
                float time = _Time.y * _Speed + SAMPLE_TEXTURE2D(_FlowMap, sampler_FlowMap, IN.uv).a;

                // Get UVs for two phases
                float3 uvwA = FlowUV(IN.uv, flowVector, time, false);
                float3 uvwB = FlowUV(IN.uv, flowVector, time, true);
                // Get normals and colors from textures
                half4 texA = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvwA.xy) * uvwA.z;
                half4 texB = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvwB.xy) * uvwB.z;

                // Use both phases for the final color
                half4 c = (texA + texB) * _Color;
                // Make the disolved edges brighter
                c += step(disolve - _DisolveAmount, 0.02f) * c;

                return c;
            }
            ENDHLSL
        }
    }
}