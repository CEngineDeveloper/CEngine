#include "UnityCG.cginc"

uniform sampler2D _MainTex;
uniform sampler2D _FogTex;
uniform sampler2D_float _CameraDepthTexture;
uniform sampler2D _FogColorTex;
uniform float2 _FogColorTexScale;

// for fast world space reconstruction
uniform float4x4 _FoWInverseView;

uniform float3 _CameraWorldPosition;
uniform float _FogTextureSize;
uniform float _MapSize;
uniform float4 _MapOffset;
uniform float4 _MainFogColor;
uniform float _OutsideFogStrength;
uniform float _StereoSeparation;

struct v2f
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 interpolatedRay : TEXCOORD1;
};

v2f vert(float4 vertex : POSITION)
{
	float far = _ProjectionParams.z;
	float2 orthoSize = unity_OrthoParams.xy;
	float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

	// Vertex pos -> clip space vertex position
	float3 viewpos = float3(vertex.xy * 2 - 1, 1);

	// Perspective: view space vertex position of the far plane
	float3 rayPers = mul(unity_CameraInvProjection, viewpos.xyzz * far).xyz;

	// Orthographic: view space vertex position
	float3 rayOrtho = float3(orthoSize * viewpos.xy, 0);

	v2f o;
	o.pos = float4(viewpos.x, -viewpos.y, 1, 1);
	o.uv = (viewpos.xy + 1) * 0.5;
	o.interpolatedRay = lerp(rayPers, rayOrtho, isOrtho);

	//#if UNITY_UV_STARTS_AT_TOP // Unity's documentation says to do this, but it seems to cause issues with some graphics APIs
	if (_ProjectionParams.x > 0)
		o.pos.y = -o.pos.y;
	//#endif

	return o;
}

#include "HLSLSupport.cginc"

float3 ComputeViewSpacePosition(float2 texcoord, float3 ray, out float rawdepth)
{
	// Render settings
	float near = _ProjectionParams.y;
	float far = _ProjectionParams.z;
	float isOrtho = unity_OrthoParams.w; // 0: perspective, 1: orthographic

	// Z buffer sample
	float z = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, texcoord);

	// Far plane exclusion
	#if !defined(EXCLUDE_FAR_PLANE)
	float mask = 1;
	#elif defined(UNITY_REVERSED_Z)
	float mask = z > 0;
	#else
	float mask = z < 1;
	#endif

	// Perspective: view space position = ray * depth
	rawdepth = Linear01Depth(z);
	float3 vposPers = ray * rawdepth;

	// Orthographic: linear depth (with reverse-Z support)
	#if defined(UNITY_REVERSED_Z)
	float depthOrtho = -lerp(far, near, z);
	#else
	float depthOrtho = -lerp(near, far, z);
	#endif

	// Orthographic: view space position
	float3 vposOrtho = float3(ray.xy, depthOrtho);

	// Result: view space position
	return lerp(vposPers, vposOrtho, isOrtho) * mask;
}

float3 GetWorldPosition(float2 uv, float3 interpolatedray, out float rawdepth)
{
	// for VR
	uv = UnityStereoTransformScreenSpaceTex(uv);

	float3 viewspacepos = ComputeViewSpacePosition(uv, interpolatedray, rawdepth);
	float3 wsPos = mul(_FoWInverseView, float4(viewspacepos, 1)).xyz;

	// single pass VR requires the world space separation between eyes to be manually set
	#if UNITY_SINGLE_PASS_STEREO
	wsPos.x += unity_StereoEyeIndex * _StereoSeparation;
	#endif

	return wsPos;
}

float2 WorldPositionToFogPosition(float3 worldpos)
{
	#ifdef PLANE_XY
		float2 modepos = worldpos.xy;
	#elif PLANE_YZ
		float2 modepos = worldpos.yz;
	#else// PLANE_XZ
		float2 modepos = worldpos.xz;
	#endif

	return (modepos - _MapOffset.xy) / _MapSize + float2(0.5f, 0.5f);
}

float GetFogAmount(float2 fogpos, float rawdepth)
{
	// if it is beyond the map
	float isoutsidemap = min(1, step(fogpos.x, 0) + step(1, fogpos.x) + step(fogpos.y, 0) + step(1, fogpos.y));

	// if outside map, use the outside fog color
	float fog = lerp(tex2D(_FogTex, fogpos).a, _OutsideFogStrength, isoutsidemap);
				
	#ifndef FOGFARPLANE
		if (rawdepth == 0) // there's some weird behaviour with the far plane in some rare cases that this should fix...
			fog = 0;
		else
			fog *= step(rawdepth, 0.999); // don't show fog on the far plane
	#endif

	return fog;
}

struct FogData
{
	float3 cameraPosition;
	float3 worldPosition;
	float2 mapPosition;
	float2 screenPosition;
	float2 planePosition;
	float fogAmount;
	float4 backgroundColor;
};

float4 GetDefaultFogColor(FogData fogdata)
{
	#ifdef FOG_COLORED
		return _MainFogColor;
	#elif FOG_TEXTURED_WORLD
		return tex2D(_FogColorTex, fogdata.planePosition);
	#elif FOG_TEXTURED_SCREEN
		return tex2D(_FogColorTex, fogdata.screenPosition);
	#endif
}

FogData GetFogData(float2 uv, float3 interpolatedray)
{
	FogData fogdata;

	float rawdepth;
	fogdata.worldPosition = GetWorldPosition(uv, interpolatedray, rawdepth);
	fogdata.mapPosition = WorldPositionToFogPosition(fogdata.worldPosition);
	fogdata.fogAmount = GetFogAmount(fogdata.mapPosition, rawdepth);
	fogdata.screenPosition = uv;
	fogdata.cameraPosition = _CameraWorldPosition;
	fogdata.backgroundColor = tex2D(_MainTex, uv);

	float3 rayorigin = _CameraWorldPosition;
	float3 raydir = normalize(fogdata.worldPosition - rayorigin);
	float3 planeorigin = float3(0, _FogColorTexScale.y, 0);
	float3 planenormal = float3(0, 1, 0);
	float t = dot(planeorigin - rayorigin, planenormal) / dot(planenormal, raydir);
	fogdata.planePosition = (rayorigin + raydir * t).xz * _FogColorTexScale.x;

	return fogdata;
}

half4 FogShader(FogData fogdata);

half4 frag(v2f i) : SV_Target
{
	FogData fogdata = GetFogData(i.uv, i.interpolatedRay);
	return FogShader(fogdata);
}
