Shader "Hidden/FogOfWarLegacy"
{
	Properties
	{
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FogTex ("Fog", 2D) = "white" {}
		_FogColorTex ("Fog Color", 2D) = "white" {}
	}

	SubShader
	{
		ZTest Always
		Cull Off
		ZWrite Off
		ZTest Always
		Fog { Mode Off }

		Pass
		{
			CGPROGRAM
			#include "../FogOfWarUtils.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile CAMERA_ORTHOGRAPHIC CAMERA_PERSPECTIVE
			#pragma multi_compile PLANE_XY PLANE_YZ PLANE_XZ
			#pragma multi_compile FOG_COLORED FOG_TEXTURED_WORLD FOG_TEXTURED_SCREEN
			#pragma multi_compile _ FOGFARPLANE
			#pragma multi_compile _ FOGOUTSIDEAREA

			half4 FogShader(FogData fogdata)
			{
				half4 fogcolor = GetDefaultFogColor(fogdata);
				return lerp(fogdata.backgroundColor, fogcolor, fogdata.fogAmount * fogcolor.a);
			}

			ENDCG
		}
	}

	Fallback off
}
