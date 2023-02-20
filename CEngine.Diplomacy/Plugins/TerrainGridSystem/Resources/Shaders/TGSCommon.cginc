#ifndef TGS_DEPTH_FADE_COMMON
#define TGS_DEPTH_FADE_COMMON

float _Offset;
fixed4 _Color;
float _NearClip;
float _FarClip;
float _FallOff;
float _Thickness;
float _FarFadeDistance;
float _FarFadeFallOff;
float3 _CircularFadePosition;
float _CircularFadeDistanceSqr;
float _CircularFadeFallOff;

#define dot2(x) dot(x,x)

fixed4 ApplyNearClipFade(float4 pos, fixed4 color) {
	#if TGS_NEAR_CLIP_FADE
		if (UNITY_MATRIX_P[3][3]!=1.0) {	// Non Orthographic camera
			color = fixed4(color.rgb, color.a * saturate((UNITY_Z_0_FAR_FROM_CLIPSPACE(pos.z) - _NearClip)/_FallOff));
		}
	#endif
	return color;
}


fixed4 ApplyFarClipFade(float4 pos, fixed4 color) {
	#if TGS_FAR_FADE
		if (UNITY_MATRIX_P[3][3]!=1.0) {	// Non Orthographic camera
			color = fixed4(color.rgb, color.a * saturate((_FarFadeDistance - UNITY_Z_0_FAR_FROM_CLIPSPACE(pos.z))/_FarFadeFallOff));
		}
	#endif
	return color;
}

fixed4 ApplyCircularFade(float4 vertex, fixed4 color) {
	#if TGS_CIRCULAR_FADE
		float3 wpos = mul(unity_ObjectToWorld, vertex).xyz;
		float distSqr = dot2(wpos - _CircularFadePosition);
		color = fixed4(color.rgb, color.a * saturate((_CircularFadeDistanceSqr - distSqr)/_CircularFadeFallOff));
	#endif
	return color;
}

fixed4 ApplyFade(float4 vertex, float4 pos, fixed4 color) {
	color = ApplyNearClipFade(pos, color);
	color = ApplyFarClipFade(pos, color);
	color = ApplyCircularFade(vertex, color);
	return color;
}

float GetThickness() {
	float thick = _Thickness;
	thick *= 1.0 - 0.999 * unity_OrthoParams.w; // in ortho, reduce thickness 
	return thick;
}

#define APPLY_FADE(inputColor, vpos, pos, color) color = ApplyFade(vpos, pos, inputColor);

#endif