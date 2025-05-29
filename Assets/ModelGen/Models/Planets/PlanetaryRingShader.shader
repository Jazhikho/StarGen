Shader "Custom/PlanetaryRingShader"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DensityMap ("Density Map", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _WaveScale ("Wave Scale", Range(1,10)) = 4.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _MinimumRenderDistance ("Minimum Render Distance", Range(0,100)) = 10.0
        _MaximumFadeDistance ("Maximum Fade Distance", Range(0,100)) = 20.0
        _InnerRingDiameter ("Inner Ring Diameter", Range(0,1)) = 0.5
        [Toggle] _UseAlpha("Use Alpha Transparency", Float) = 0
    }
    
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True"
            "Queue" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }
        LOD 200
        Cull Off
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        ENDHLSL
        
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_DensityMap);
            SAMPLER(sampler_DensityMap);
            
            CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            float _WaveScale;
            half _Metallic;
            half _Smoothness;
            float _MinimumRenderDistance;
            float _MaximumFadeDistance;
            float _UseAlpha;
            float _InnerRingDiameter;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
                float3 normalOS     : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv           : TEXCOORD0;
                float3 positionWS   : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
            };
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // Calculate distance from camera
                float dist = distance(_WorldSpaceCameraPos, IN.positionWS);
                
                // Create position vector for ring calculations
                float2 position = float2((0.5 - IN.uv.x) * 2, (0.5 - IN.uv.y) * 2);
                float ringDistanceFromCenter = sqrt(position.x * position.x + position.y * position.y);
                
                // Create ring shape with inner and outer boundaries
                clip(ringDistanceFromCenter - _InnerRingDiameter);
                clip(1 - ringDistanceFromCenter);
                
                // Optional: Skip rendering pixels closer than minimum distance
                clip(dist - _MinimumRenderDistance);
                
                // Calculate alpha based on distance (opacity)
                float alpha = clamp((dist - _MinimumRenderDistance) / (_MaximumFadeDistance - _MinimumRenderDistance), 0, 1);
                
                // Sample density map with normalized distance from inner ring to outer edge
                float normalizedRingPosition = clamp((ringDistanceFromCenter - _InnerRingDiameter) / (1 - _InnerRingDiameter), 0, 1);
                half4 density = SAMPLE_TEXTURE2D(_DensityMap, sampler_DensityMap, float2(normalizedRingPosition, 0.5));
                
                // Wave effect
                float wave = sin(dist / _WaveScale);
                
                // Sample the texture
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                
                // Create color from position and density
                half3 color = half3(position.x, position.y, density.a);
                
                // Get lighting
                InputData lightingInput = (InputData)0;
                lightingInput.normalWS = normalize(IN.normalWS);
                lightingInput.positionWS = IN.positionWS;
                lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);
                lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                
                SurfaceData surfaceInput = (SurfaceData)0;
                surfaceInput.albedo = color;
                surfaceInput.metallic = _Metallic * alpha;
                surfaceInput.smoothness = _Smoothness * alpha;
                surfaceInput.alpha = alpha * density.a;
                
                half4 finalColor = UniversalFragmentPBR(lightingInput, surfaceInput);
                finalColor.a = alpha * density.a;
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
} 