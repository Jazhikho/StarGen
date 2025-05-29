Shader "Custom/PlanetaryRingShadowShader"
{
    Properties
    {
        _DensityMap ("Density Map", 2D) = "white" {}
        _InnerRingDiameter ("Inner Ring Diameter", Range(0,1)) = 0.5
        _ShadowDensityThreshold ("Shadow Density Threshold", Range(0,1)) = 0.1
        _ShadowIntensity ("Shadow Intensity", Range(0,1)) = 1.0
    }
    
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent-1"
            "RenderPipeline" = "UniversalPipeline"
        }
        LOD 100
        
        // Make the material invisible
        ColorMask 0
        ZWrite Off
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL
        
        // Shadow Caster Pass
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull Off

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            
            TEXTURE2D(_DensityMap);
            SAMPLER(sampler_DensityMap);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _DensityMap_ST;
            float _InnerRingDiameter;
            float _ShadowDensityThreshold;
            float _ShadowIntensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 texcoord     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                // Apply shadow bias manually, adjusted by shadow intensity
                float bias = 0.05 * (1.0 - _ShadowIntensity);
                positionWS = positionWS + normalWS * bias;
                
                float4 positionCS = TransformWorldToHClip(positionWS);
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                return positionCS;
            }

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.uv = input.texcoord;
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Calculate ring shape
                float2 position = float2((0.5 - input.uv.x) * 2, (0.5 - input.uv.y) * 2);
                float ringDistanceFromCenter = sqrt(position.x * position.x + position.y * position.y);
                
                // Discard fragments outside the ring
                if (ringDistanceFromCenter < _InnerRingDiameter || ringDistanceFromCenter > 1.0)
                    discard;
                
                // Sample density map with normalized distance from inner ring to outer edge
                float normalizedRingPosition = (ringDistanceFromCenter - _InnerRingDiameter) / (1.0 - _InnerRingDiameter);
                half4 density = SAMPLE_TEXTURE2D(_DensityMap, sampler_DensityMap, float2(normalizedRingPosition, 0.5));
                
                // Discard fragments with density below threshold (multiplied by shadow intensity)
                float threshold = _ShadowDensityThreshold * (1.0 - _ShadowIntensity * 0.5);
                if (density.a < threshold)
                    discard;
                
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
} 