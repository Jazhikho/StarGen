Shader "Starlight/Star"
{
    Properties
    {
        _EmissionTexture ("Emission Texture", 2D) = "white" {}
        _EmissionTint ("Emission Tint", Color) = (0.252, 0.157, 0.292, 1)
        _BlurAmount ("Blur Amount", Range(0, 1)) = 0.2
        _EmissionEnergy ("Emission Energy", Float) = 50000000000
        _BillboardSizeDeg ("Billboard Size (degrees)", Range(0, 90)) = 45
        _LuminosityCap ("Luminosity Cap", Float) = 4000000
        _MetersPerLightyear ("Meters Per Lightyear", Float) = 100.0
        _ColorGamma ("Color Gamma", Range(0, 10)) = 0.5
        _TextureGamma ("Texture Gamma", Float) = 10000.0
        [Toggle] _ClampOutput ("Clamp Output", Float) = 0
        _MinSizeRatio ("Minimum Size Ratio", Range(0, 1)) = 0.3
        _MaxLuminosity ("Max Luminosity", Float) = 100000.0
        _ScalingGamma ("Scaling Gamma", Range(0, 2)) = 0.45
        [Toggle] _DebugShowRects ("Debug Show Rects", Float) = 0
        
        // Star properties
        _StarColor ("Star Color", Color) = (1,1,1,1)
        _StarLuminosity ("Star Luminosity", Float) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        Blend One One
        ZWrite Off
        ZTest Always
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // These two lines enable instancing
            #pragma multi_compile_instancing
            #pragma enable_d3d11_debug_symbols
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 color : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _EmissionTexture;
            float4 _EmissionTint;
            float _BlurAmount;
            float _EmissionEnergy;
            float _BillboardSizeDeg;
            float _LuminosityCap;
            float _MetersPerLightyear;
            float _ColorGamma;
            float _TextureGamma;
            float _ClampOutput;
            float _MinSizeRatio;
            float _MaxLuminosity;
            float _ScalingGamma;
            float _DebugShowRects;
            
            // Star-specific properties from MaterialPropertyBlock
            // Per-instance arrays
            #ifdef INSTANCING_ON
            float4 _StarColors[1023]; // Array of star colors for instancing
            float _StarLuminosities[1023]; // Array of star luminosities for instancing
            #endif
            // Fallback properties
            float4 _StarColor;
            float _StarLuminosity;
            
            // Constants
            static const float LUMINOSITY_SUN = 3.846e+26;
            static const float LIGHTYEAR_LENGTH = 9.460730e+15;
            
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // Get instance ID for accessing per-instance data
                uint instanceID = 0;
                float4 starColor = _StarColor;
                float starLuminosity = _StarLuminosity;
                
                #ifdef INSTANCING_ON
                instanceID = unity_InstanceID;
                starColor = _StarColors[instanceID];
                starLuminosity = _StarLuminosities[instanceID];
                #endif
                
                // Get star position from object to world matrix
                float4 worldPos = mul(unity_ObjectToWorld, float4(0,0,0,1));
                
                // Distance from camera
                float dist = distance(_WorldSpaceCameraPos.xyz, worldPos.xyz);
                
                // Use lightyear scaling for distance calculation
                float dist_adj = dist * (LIGHTYEAR_LENGTH / _MetersPerLightyear);
                
                // Calculate luminosity based on distance
                float dimming = 1.0 / max(dist_adj * dist_adj, 0.0001);
                float emission = LUMINOSITY_SUN * dimming;
                // Use per-instance luminosity from the array
                float luminosity = starLuminosity * _EmissionEnergy * emission;
                luminosity = min(luminosity, _LuminosityCap);
                
                // Calculate star color based on temperature
                // Use per-instance color from the array
                o.color = pow(abs(starColor.rgb), _ColorGamma) * luminosity;
                
                // Calculate scaling for PSF texture
                float scale_ratio = saturate(luminosity / _MaxLuminosity);
                scale_ratio = pow(scale_ratio, _ScalingGamma);
                scale_ratio = lerp(_MinSizeRatio, 1.0, scale_ratio);
                float size = scale_ratio * _BillboardSizeDeg;
                
                // Modify UVs for cropping
                o.uv = (v.uv - 0.5) * scale_ratio + 0.5;
                
                // Billboard the quad to always face the camera
                float3 cameraDir = normalize(_WorldSpaceCameraPos - worldPos.xyz);
                float3 up = normalize(float3(0, 1, 0) - cameraDir * dot(float3(0, 1, 0), cameraDir));
                float3 right = normalize(cross(up, cameraDir));
                
                // Calculate size in world units
                float camFOV = atan(1.0 / unity_CameraProjection._m11) * 2.0; // In radians
                float fovFactor = tan(size * 3.14159 / 180.0) / tan(camFOV * 0.5);
                float worldSize = dist * fovFactor;
                
                // Position the vertex for the billboard
                float3 vertexPos = worldPos.xyz
                + right * v.vertex.x * worldSize
                + up * v.vertex.y * worldSize;
                
                // Final position
                o.pos = UnityWorldToClipPos(vertexPos);
                
                // Force Z to stay at far plane
                o.pos.z = o.pos.w * 0.999;
                
                return o;
            }
            
            // Convert linear RGB to sRGB
            float3 to_srgb(float3 linearRGB)
            {
                float3 cutoff = linearRGB < 0.0031308;
                float3 higher = 1.055 * pow(abs(linearRGB), 1.0/2.4) - 0.055;
                float3 lower = linearRGB * 12.92;
                
                return lerp(higher, lower, cutoff);
            }
            
            float4 frag(v2f i) : SV_Target
            {
                // Discard pixels outside UV range
                if (i.uv.x < 0.0 || i.uv.x > 1.0 || i.uv.y < 0.0 || i.uv.y > 1.0)
                discard;
                
                float3 emission_tex = tex2D(_EmissionTexture, i.uv).rgb;
                float2 pixel_size = fwidth(i.uv * 1024.0);
                float2 pos = i.uv * 1024.0 - 512.0;
                float dist = dot(pos, pos) + 1.0 + pixel_size.x * _BlurAmount;
                
                // Apply the PSF texture
                emission_tex = (pow(_TextureGamma, emission_tex / dist) - 1.0) / 999.0 * _EmissionTint.rgb;
                
                // Get final color
                float3 color = i.color * emission_tex;
                
                if (_ClampOutput > 0.5)
                color = saturate(color);
                
                // Debug visualization
                if (_DebugShowRects > 0.5)
                color += float3(0.1, 0.1, 0.1);
                
                // Apply gamma correction in linear color space
                #ifndef UNITY_COLORSPACE_GAMMA
                color = to_srgb(color);
                #endif
                
                // Discard very dim pixels
                if (max(max(color.r, color.g), color.b) < 0.001)
                discard;
                
                return float4(color, 1);
            }
            ENDCG
        }
    }
    
    // This enables instancing on the material
    CustomEditor "StarlightMaterialEditor"
}