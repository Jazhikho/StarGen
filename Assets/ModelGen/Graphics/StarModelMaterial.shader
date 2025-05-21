Shader "Unlit/NewUnlitShader"
{
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Pass
		{
			Blend One One
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};
			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};
#define hlsl_atan(x,y) atan2(x, y)
#define mod(x,y) ((x)-(y)*floor((x)/(y)))
inline float4 textureLod(sampler2D tex, float2 uv, float lod) {
    return tex2D(tex, uv);
}
inline float2 tofloat2(float x) {
    return float2(x, x);
}
inline float2 tofloat2(float x, float y) {
    return float2(x, y);
}
inline float3 tofloat3(float x) {
    return float3(x, x, x);
}
inline float3 tofloat3(float x, float y, float z) {
    return float3(x, y, z);
}
inline float3 tofloat3(float2 xy, float z) {
    return float3(xy.x, xy.y, z);
}
inline float3 tofloat3(float x, float2 yz) {
    return float3(x, yz.x, yz.y);
}
inline float4 tofloat4(float x, float y, float z, float w) {
    return float4(x, y, z, w);
}
inline float4 tofloat4(float x) {
    return float4(x, x, x, x);
}
inline float4 tofloat4(float x, float3 yzw) {
    return float4(x, yzw.x, yzw.y, yzw.z);
}
inline float4 tofloat4(float2 xy, float2 zw) {
    return float4(xy.x, xy.y, zw.x, zw.y);
}
inline float4 tofloat4(float3 xyz, float w) {
    return float4(xyz.x, xyz.y, xyz.z, w);
}
inline float4 tofloat4(float2 xy, float z, float w) {
    return float4(xy.x, xy.y, z, w);
}
inline float2x2 tofloat2x2(float2 v1, float2 v2) {
    return float2x2(v1.x, v1.y, v2.x, v2.y);
}
// EngineSpecificDefinitions
float rand(float2 x) {
    return frac(cos(mod(dot(x, tofloat2(13.9898, 8.141)), 3.14)) * 43758.5453);
}
float2 rand2(float2 x) {
    return frac(cos(mod(tofloat2(dot(x, tofloat2(13.9898, 8.141)),
						      dot(x, tofloat2(3.4562, 17.398))), tofloat2(3.14))) * 43758.5453);
}
float3 rand3(float2 x) {
    return frac(cos(mod(tofloat3(dot(x, tofloat2(13.9898, 8.141)),
							  dot(x, tofloat2(3.4562, 17.398)),
                              dot(x, tofloat2(13.254, 5.867))), tofloat3(3.14))) * 43758.5453);
}
float param_rnd(float minimum, float maximum, float seed) {
	return minimum+(maximum-minimum)*rand(tofloat2(seed));
}
float3 rgb2hsv(float3 c) {
	float4 K = tofloat4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = c.g < c.b ? tofloat4(c.bg, K.wz) : tofloat4(c.gb, K.xy);
	float4 q = c.r < p.x ? tofloat4(p.xyw, c.r) : tofloat4(c.r, p.yzx);
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return tofloat3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
float3 hsv2rgb(float3 c) {
	float4 K = tofloat4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
float value_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float p00 = rand(mod(o, size));
	float p01 = rand(mod(o + tofloat2(0.0, 1.0), size));
	float p10 = rand(mod(o + tofloat2(1.0, 0.0), size));
	float p11 = rand(mod(o + tofloat2(1.0, 1.0), size));
	p00 = sin(p00 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	p01 = sin(p01 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	p10 = sin(p10 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	p11 = sin(p11 * 6.28318530718 + offset * 6.28318530718) / 2.0 + 0.5;
	float2 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return lerp(lerp(p00, p10, t.x), lerp(p01, p11, t.x), t.y);
}
float fbm_2d_value(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = value_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float perlin_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float a00 = rand(mod(o, size)) * 6.28318530718 + offset * 6.28318530718;
	float a01 = rand(mod(o + tofloat2(0.0, 1.0), size)) * 6.28318530718 + offset * 6.28318530718;
	float a10 = rand(mod(o + tofloat2(1.0, 0.0), size)) * 6.28318530718 + offset * 6.28318530718;
	float a11 = rand(mod(o + tofloat2(1.0, 1.0), size)) * 6.28318530718 + offset * 6.28318530718;
	float2 v00 = tofloat2(cos(a00), sin(a00));
	float2 v01 = tofloat2(cos(a01), sin(a01));
	float2 v10 = tofloat2(cos(a10), sin(a10));
	float2 v11 = tofloat2(cos(a11), sin(a11));
	float p00 = dot(v00, f);
	float p01 = dot(v01, f - tofloat2(0.0, 1.0));
	float p10 = dot(v10, f - tofloat2(1.0, 0.0));
	float p11 = dot(v11, f - tofloat2(1.0, 1.0));
	float2 t =  f * f * f * (f * (f * 6.0 - 15.0) + 10.0);
	return 0.5 + lerp(lerp(p00, p10, t.x), lerp(p01, p11, t.x), t.y);
}
float fbm_2d_perlin(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = perlin_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float perlinabs_noise_2d(float2 coord, float2 size, float offset, float seed) {
	return abs(2.0*perlin_noise_2d(coord, size, offset, seed)-1.0);
}
float fbm_2d_perlinabs(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = perlinabs_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float fbm_2d_mod289(float x) {
	return x - floor(x * (1.0 / 289.0)) * 289.0;
}
float fbm_2d_permute(float x) {
	return fbm_2d_mod289(((x * 34.0) + 1.0) * x);
}
float2 fbm_2d_rgrad2(float2 p, float rot, float seed) {
	float u = fbm_2d_permute(fbm_2d_permute(p.x) + p.y) * 0.0243902439 + rot; // Rotate by shift
	u = frac(u) * 6.28318530718; // 2*pi
	return tofloat2(cos(u), sin(u));
}
float simplex_noise_2d(float2 coord, float2 size, float offset, float seed) {
	coord *= 2.0; // needed for it to tile
	coord += rand2(tofloat2(seed, 1.0-seed)) + size;
	size *= 2.0; // needed for it to tile
	coord.y += 0.001;
	float2 uv = tofloat2(coord.x + coord.y*0.5, coord.y);
	float2 i0 = floor(uv);
	float2 f0 = frac(uv);
	float2 i1 = (f0.x > f0.y) ? tofloat2(1.0, 0.0) : tofloat2(0.0, 1.0);
	float2 p0 = tofloat2(i0.x - i0.y * 0.5, i0.y);
	float2 p1 = tofloat2(p0.x + i1.x - i1.y * 0.5, p0.y + i1.y);
	float2 p2 = tofloat2(p0.x + 0.5, p0.y + 1.0);
	i1 = i0 + i1;
	float2 i2 = i0 + tofloat2(1.0, 1.0);
	float2 d0 = coord - p0;
	float2 d1 = coord - p1;
	float2 d2 = coord - p2;
	float3 xw = mod(tofloat3(p0.x, p1.x, p2.x), size.x);
	float3 yw = mod(tofloat3(p0.y, p1.y, p2.y), size.y);
	float3 iuw = xw + 0.5 * yw;
	float3 ivw = yw;
	float2 g0 = fbm_2d_rgrad2(tofloat2(iuw.x, ivw.x), offset, seed);
	float2 g1 = fbm_2d_rgrad2(tofloat2(iuw.y, ivw.y), offset, seed);
	float2 g2 = fbm_2d_rgrad2(tofloat2(iuw.z, ivw.z), offset, seed);
	float3 w = tofloat3(dot(g0, d0), dot(g1, d1), dot(g2, d2));
	float3 t = 0.8 - tofloat3(dot(d0, d0), dot(d1, d1), dot(d2, d2));
	t = max(t, tofloat3(0.0));
	float3 t2 = t * t;
	float3 t4 = t2 * t2;
	float n = dot(t4, w);
	return 0.5 + 5.5 * n;
}
float fbm_2d_simplex(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = simplex_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node =  0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718 * node);
			float2 diff = neighbor + node - f;
			float dist = length(diff);
			min_dist = min(min_dist, dist);
		}
	}
	return min_dist;
}
float fbm_2d_cellular(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular2_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist1 = 2.0;
	float min_dist2 = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = length(diff);
			if (min_dist1 > dist) {
				min_dist2 = min_dist1;
				min_dist1 = dist;
			} else if (min_dist2 > dist) {
				min_dist2 = dist;
			}
		}
	}
	return min_dist2-min_dist1;
}
float fbm_2d_cellular2(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular2_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular3_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = abs((diff).x) + abs((diff).y);
			min_dist = min(min_dist, dist);
		}
	}
	return min_dist;
}
float fbm_2d_cellular3(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular3_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular4_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist1 = 2.0;
	float min_dist2 = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = abs((diff).x) + abs((diff).y);
			if (min_dist1 > dist) {
				min_dist2 = min_dist1;
				min_dist1 = dist;
			} else if (min_dist2 > dist) {
				min_dist2 = dist;
			}
		}
	}
	return min_dist2-min_dist1;
}
float fbm_2d_cellular4(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular4_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular5_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.5 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = max(abs((diff).x), abs((diff).y));
			min_dist = min(min_dist, dist);
		}
	}
	return min_dist;
}
float fbm_2d_cellular5(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular5_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float cellular6_noise_2d(float2 coord, float2 size, float offset, float seed) {
	float2 o = floor(coord)+rand2(tofloat2(seed, 1.0-seed))+size;
	float2 f = frac(coord);
	float min_dist1 = 2.0;
	float min_dist2 = 2.0;
	for(float x = -1.0; x <= 1.0; x++) {
		for(float y = -1.0; y <= 1.0; y++) {
			float2 neighbor = tofloat2(float(x),float(y));
			float2 node = rand2(mod(o + tofloat2(x, y), size)) + tofloat2(x, y);
			node = 0.5 + 0.25 * sin(offset * 6.28318530718 + 6.28318530718*node);
			float2 diff = neighbor + node - f;
			float dist = max(abs((diff).x), abs((diff).y));
			if (min_dist1 > dist) {
				min_dist2 = min_dist1;
				min_dist1 = dist;
			} else if (min_dist2 > dist) {
				min_dist2 = dist;
			}
		}
	}
	return min_dist2-min_dist1;
}
float fbm_2d_cellular6(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = cellular6_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
// MIT License Inigo Quilez - https://www.shadertoy.com/view/Xd23Dh
float voronoise_noise_2d( float2 coord, float2 size, float offset, float seed) {
	float2 i = floor(coord) + rand2(tofloat2(seed, 1.0-seed)) + size;
	float2 f = frac(coord);
	
	float2 a = tofloat2(0.0);
	
	for( int y=-2; y<=2; y++ ) {
		for( int x=-2; x<=2; x++ ) {
			float2  g = tofloat2( float(x), float(y) );
			float3  o = rand3( mod(i + g, size) + tofloat2(seed) );
			o.xy += 0.25 * sin(offset * 6.28318530718 + 6.28318530718*o.xy);
			float2  d = g - f + o.xy;
			float w = pow( 1.0-smoothstep(0.0, 1.414, length(d)), 1.0 );
			a += tofloat2(o.z*w,w);
		}
	}
	
	return a.x/a.y;
}
float fbm_2d_voronoise(float2 coord, float2 size, int folds, int octaves, float persistence, float offset, float seed) {
	float normalize_factor = 0.0;
	float value = 0.0;
	float scale = 1.0;
	for (int i = 0; i < octaves; i++) {
		float noise = voronoise_noise_2d(coord*size, size, offset, seed);
		for (int f = 0; f < folds; ++f) {
			noise = abs(2.0*noise-1.0);
		}
		value += noise * scale;
		normalize_factor += scale;
		size *= 2.0;
		scale *= persistence;
	}
	return value / normalize_factor;
}
float2 transform2_clamp(float2 uv) {
	return clamp(uv, tofloat2(0.0), tofloat2(1.0));
}
float2 transform2(float2 uv, float2 translate, float rotate, float2 scale) {
 	float2 rv;
	uv -= translate;
	uv -= tofloat2(0.5);
	rv.x = cos(rotate)*uv.x + sin(rotate)*uv.y;
	rv.y = -sin(rotate)*uv.x + cos(rotate)*uv.y;
	rv /= scale;
	rv += tofloat2(0.5);
	return rv;	
}
float3 blend_normal(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*c1 + (1.0-opacity)*c2;
}
float3 blend_dissolve(float2 uv, float3 c1, float3 c2, float opacity) {
	if (rand(uv) < opacity) {
		return c1;
	} else {
		return c2;
	}
}
float3 blend_multiply(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*c1*c2 + (1.0-opacity)*c2;
}
float3 blend_screen(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*(1.0-(1.0-c1)*(1.0-c2)) + (1.0-opacity)*c2;
}
float blend_overlay_f(float c1, float c2) {
	return (c1 < 0.5) ? (2.0*c1*c2) : (1.0-2.0*(1.0-c1)*(1.0-c2));
}
float3 blend_overlay(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_overlay_f(c1.x, c2.x), blend_overlay_f(c1.y, c2.y), blend_overlay_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float3 blend_hard_light(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*0.5*(c1*c2+blend_overlay(uv, c1, c2, 1.0)) + (1.0-opacity)*c2;
}
float blend_soft_light_f(float c1, float c2) {
	return (c2 < 0.5) ? (2.0*c1*c2+c1*c1*(1.0-2.0*c2)) : 2.0*c1*(1.0-c2)+sqrt(c1)*(2.0*c2-1.0);
}
float3 blend_soft_light(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_soft_light_f(c1.x, c2.x), blend_soft_light_f(c1.y, c2.y), blend_soft_light_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_burn_f(float c1, float c2) {
	return (c1==0.0)?c1:max((1.0-((1.0-c2)/c1)),0.0);
}
float3 blend_burn(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_burn_f(c1.x, c2.x), blend_burn_f(c1.y, c2.y), blend_burn_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float blend_dodge_f(float c1, float c2) {
	return (c1==1.0)?c1:min(c2/(1.0-c1),1.0);
}
float3 blend_dodge(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*tofloat3(blend_dodge_f(c1.x, c2.x), blend_dodge_f(c1.y, c2.y), blend_dodge_f(c1.z, c2.z)) + (1.0-opacity)*c2;
}
float3 blend_lighten(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*max(c1, c2) + (1.0-opacity)*c2;
}
float3 blend_darken(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*min(c1, c2) + (1.0-opacity)*c2;
}
float3 blend_linear(float2 uv, float3 c1, float3 c2, float opacity) {
	return c1 + opacity * (2.0 *(c2 - tofloat3(0.5)));
}
float3 blend_difference(float2 uv, float3 c1, float3 c2, float opacity) {
	return opacity*clamp(c2-c1, tofloat3(0.0), tofloat3(1.0)) + (1.0-opacity)*c2;
}
float3 blend_additive(float2 uv, float3 c1, float3 c2, float oppacity) {
	return c2 + c1 * oppacity;
}
float3 blend_addsub(float2 uv, float3 c1, float3 c2, float oppacity) {
	return c2 + (c1 - .5) * 2.0 * oppacity;
}
static const float p_o10003_gradient_0_pos = 0.000000000;
static const float4 p_o10003_gradient_0_col = tofloat4(0.000000000, 0.000000000, 0.000000000, 0.996078014);
static const float p_o10003_gradient_1_pos = 0.041064000;
static const float4 p_o10003_gradient_1_col = tofloat4(0.273438007, 0.105742998, 0.105742998, 0.998057008);
static const float p_o10003_gradient_2_pos = 0.122883000;
static const float4 p_o10003_gradient_2_col = tofloat4(0.847656012, 0.585713029, 0.443695009, 1.000000000);
static const float p_o10003_gradient_3_pos = 1.000000000;
static const float4 p_o10003_gradient_3_col = tofloat4(1.000000000, 1.000000000, 1.000000000, 0.996078014);
float4 o10003_gradient_gradient_fct(float x) {
  if (x < p_o10003_gradient_0_pos) {
    return p_o10003_gradient_0_col;
  } else if (x < p_o10003_gradient_1_pos) {
    return lerp(lerp(p_o10003_gradient_1_col, p_o10003_gradient_2_col, (x-p_o10003_gradient_1_pos)/(p_o10003_gradient_2_pos-p_o10003_gradient_1_pos)), lerp(p_o10003_gradient_0_col, p_o10003_gradient_1_col, (x-p_o10003_gradient_0_pos)/(p_o10003_gradient_1_pos-p_o10003_gradient_0_pos)), 1.0-0.5*(x-p_o10003_gradient_0_pos)/(p_o10003_gradient_1_pos-p_o10003_gradient_0_pos));
  } else if (x < p_o10003_gradient_2_pos) {
    return 0.5*(lerp(p_o10003_gradient_1_col, p_o10003_gradient_2_col, (x-p_o10003_gradient_1_pos)/(p_o10003_gradient_2_pos-p_o10003_gradient_1_pos)) + lerp(lerp(p_o10003_gradient_0_col, p_o10003_gradient_1_col, (x-p_o10003_gradient_0_pos)/(p_o10003_gradient_1_pos-p_o10003_gradient_0_pos)), lerp(p_o10003_gradient_2_col, p_o10003_gradient_3_col, (x-p_o10003_gradient_2_pos)/(p_o10003_gradient_3_pos-p_o10003_gradient_2_pos)), 0.5-0.5*cos(3.14159265359*(x-p_o10003_gradient_1_pos)/(p_o10003_gradient_2_pos-p_o10003_gradient_1_pos))));
  } else if (x < p_o10003_gradient_3_pos) {
    return lerp(lerp(p_o10003_gradient_1_col, p_o10003_gradient_2_col, (x-p_o10003_gradient_1_pos)/(p_o10003_gradient_2_pos-p_o10003_gradient_1_pos)), lerp(p_o10003_gradient_2_col, p_o10003_gradient_3_col, (x-p_o10003_gradient_2_pos)/(p_o10003_gradient_3_pos-p_o10003_gradient_2_pos)), 0.5+0.5*(x-p_o10003_gradient_2_pos)/(p_o10003_gradient_3_pos-p_o10003_gradient_2_pos));
  }
  return p_o10003_gradient_3_col;
}
static const float p_o10001_amount = 0.880000000;
static const float p_o10018_gradient_0_pos = 0.000000000;
static const float4 p_o10018_gradient_0_col = tofloat4(1.000000000, 1.000000000, 1.000000000, 1.000000000);
static const float p_o10018_gradient_1_pos = 0.477919000;
static const float4 p_o10018_gradient_1_col = tofloat4(0.000000000, 0.000000000, 0.000000000, 1.000000000);
float4 o10018_gradient_gradient_fct(float x) {
  if (x < p_o10018_gradient_0_pos) {
    return p_o10018_gradient_0_col;
  } else if (x < p_o10018_gradient_1_pos) {
    return lerp(p_o10018_gradient_0_col, p_o10018_gradient_1_col, (x-p_o10018_gradient_0_pos)/(p_o10018_gradient_1_pos-p_o10018_gradient_0_pos));
  }
  return p_o10018_gradient_1_col;
}
static const float seed_o10000 = 0.000000000;
static const float p_o10000_rotate = 1.000000000;
static const float p_o10000_scale_x = 1.000000000;
static const float p_o10000_scale_y = 1.000000000;
static const float p_o9996_amount = 0.175000000;
static const float p_o9996_eps = 0.325000000;
static const float seed_o9995 = 0.000000000;
static const float p_o9995_scale_x = 8.000000000;
static const float p_o9995_scale_y = 8.000000000;
static const float p_o9995_folds = 0.000000000;
static const float p_o9995_iterations = 24.000000000;
static const float p_o9995_persistence = 0.750000000;
float o9996_input_d(float2 uv, float _seed_variation_) {
float o9995_0_1_f = fbm_2d_simplex((uv), tofloat2(p_o9995_scale_x, p_o9995_scale_y), int(p_o9995_folds), int(p_o9995_iterations), p_o9995_persistence, (_Time.y*.9), (seed_o9995+frac(_seed_variation_)));
return o9995_0_1_f;
}
float2 o9996_slope(float2 uv, float epsilon, float _seed_variation_) {
	return tofloat2(o9996_input_d(frac(uv+tofloat2(epsilon, 0.0)), _seed_variation_)-o9996_input_d(frac(uv-tofloat2(epsilon, 0.0)), _seed_variation_), o9996_input_d(frac(uv+tofloat2(0.0, epsilon)), _seed_variation_)-o9996_input_d(frac(uv-tofloat2(0.0, epsilon)), _seed_variation_));
}static const float p_o10002_translate_x = 0.000000000;
static const float p_o10002_translate_y = 0.000000000;
static const float p_o10002_rotate = 0.000000000;
static const float p_o10002_scale_x = 27.460000000;
static const float p_o10002_scale_y = 6.950000000;
static const float p_o10031_curve_0_x = 0.000000000;
static const float p_o10031_curve_0_y = 0.000000000;
static const float p_o10031_curve_0_ls = 0.000000000;
static const float p_o10031_curve_0_rs = -0.161685000;
static const float p_o10031_curve_1_x = 1.000000000;
static const float p_o10031_curve_1_y = 1.000000000;
static const float p_o10031_curve_1_ls = -0.404213000;
static const float p_o10031_curve_1_rs = 0.000000000;
float o10031_curve_curve_fct(float x) {
{
float dx = x - p_o10031_curve_0_x;
float d = p_o10031_curve_1_x - p_o10031_curve_0_x;
float t = dx/d;
float omt = (1.0 - t);
float omt2 = omt * omt;
float omt3 = omt2 * omt;
float t2 = t * t;
float t3 = t2 * t;
d /= 3.0;
float y1 = p_o10031_curve_0_y;
float yac = p_o10031_curve_0_y + d*p_o10031_curve_0_rs;
float ybc = p_o10031_curve_1_y - d*p_o10031_curve_1_ls;
float y2 = p_o10031_curve_1_y;
return y1*omt3 + yac*omt2*t*3.0 + ybc*omt*t2*3.0 + y2*t3;
}
}
		
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			fixed4 frag (v2f i) : SV_Target {
				float _seed_variation_ = 0.0;
				float2 uv = i.uv;
float2 o9996_0_slope = o9996_slope((transform2((uv), tofloat2((param_rnd(0,0.5, (seed_o10000+frac(_seed_variation_))+0.841471)*_Time.y)*(2.0*1.0-1.0), (param_rnd(0,0.01, (seed_o10000+frac(_seed_variation_))+17.841471)*_Time.y*-.3)*(2.0*1.0-1.0)), p_o10000_rotate*0.01745329251*(2.0*1.0-1.0), tofloat2(p_o10000_scale_x*(2.0*1.0-1.0), p_o10000_scale_y*(2.0*1.0-1.0)))), p_o9996_eps, _seed_variation_);
float2 o9996_0_warp = o9996_0_slope;float o9995_0_1_f = fbm_2d_simplex(((transform2((uv), tofloat2((param_rnd(0,0.5, (seed_o10000+frac(_seed_variation_))+0.841471)*_Time.y)*(2.0*1.0-1.0), (param_rnd(0,0.01, (seed_o10000+frac(_seed_variation_))+17.841471)*_Time.y*-.3)*(2.0*1.0-1.0)), p_o10000_rotate*0.01745329251*(2.0*1.0-1.0), tofloat2(p_o10000_scale_x*(2.0*1.0-1.0), p_o10000_scale_y*(2.0*1.0-1.0))))+p_o9996_amount*o9996_0_warp), tofloat2(p_o9995_scale_x, p_o9995_scale_y), int(p_o9995_folds), int(p_o9995_iterations), p_o9995_persistence, (_Time.y*.9), (seed_o9995+frac(_seed_variation_)));
float4 o9996_0_1_rgba = tofloat4(tofloat3(o9995_0_1_f), 1.0);
float4 o10000_0_1_rgba = o9996_0_1_rgba;
float4 o10018_0_1_rgba = o10018_gradient_gradient_fct((dot((o10000_0_1_rgba).rgb, tofloat3(1.0))/3.0));
float o10031_0_1_f = o10031_curve_curve_fct(((transform2((uv), tofloat2(p_o10002_translate_x*(2.0*1.0-1.0), p_o10002_translate_y*(2.0*1.0-1.0)), p_o10002_rotate*0.01745329251*(2.0*1.0-1.0), tofloat2(p_o10002_scale_x*(2.0*1.0-1.0), p_o10002_scale_y*(2.0*1.0-1.0))))).x);
float4 o10002_0_1_rgba = tofloat4(tofloat3(o10031_0_1_f), 1.0);
float4 o10001_0_s1 = o10018_0_1_rgba;
float4 o10001_0_s2 = o10002_0_1_rgba;
float o10001_0_a = p_o10001_amount*1.0;
float4 o10001_0_2_rgba = tofloat4(blend_normal((uv), o10001_0_s1.rgb, o10001_0_s2.rgb, o10001_0_a*o10001_0_s1.a), min(1.0, o10001_0_s2.a+o10001_0_a*o10001_0_s1.a));
float4 o10003_0_1_rgba = o10003_gradient_gradient_fct((dot((o10001_0_2_rgba).rgb, tofloat3(1.0))/3.0));

				// sample the generated texture
				fixed4 col = o10003_0_1_rgba;

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}



