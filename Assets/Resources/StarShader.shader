Shader "Custom/StarShader"
{
    Properties
    {
        _TextureEmission ("Emission Texture", 2D) = "white" {}
        _EmissionEnergy ("Emission Energy", Float) = 1.5
        _ColorGamma ("Color Gamma", Range(0.0, 10.0)) = 3.0
        _SizeMultiplier ("Size Multiplier", Float) = 3.5
        _BrightnessLimit ("Brightness Limit", Float) = 50.0
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        LOD 100
        
        Blend One One // Additive blending
        ColorMask RGB
        Cull Off
        Lighting Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_particles
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _TextureEmission;
            float4 _TextureEmission_ST;
            float _EmissionEnergy;
            float _ColorGamma;
            float _SizeMultiplier;
            float _BrightnessLimit;
            
            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _TextureEmission);
                
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Get base texture
                float3 emissionTex = tex2D(_TextureEmission, i.texcoord).rgb;
                
                // Apply radial falloff
                float2 centeredUV = i.texcoord * 2.0 - 1.0;
                float dist = length(centeredUV);
                float falloff = 1.0 - smoothstep(0.0, 1.0, dist);
                
                emissionTex *= falloff;
                
                // Apply color and intensity
                float3 starColor = pow(i.color.rgb, _ColorGamma) * _EmissionEnergy;
                
                // Brightness limiting
                float brightness = length(starColor);
                if (brightness > _BrightnessLimit)
                {
                    starColor = starColor * (_BrightnessLimit / brightness);
                }
                
                float3 finalColor = starColor * emissionTex * i.color.a;
                
                // Discard very dim pixels
                if (length(finalColor) < 0.001)
                discard;
                
                return float4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}