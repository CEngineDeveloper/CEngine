Shader "MODev/UIParticle/Mask/DepthRender"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		[HideInInspector]_AlphaTestTreshold("_AlphaTestTreshold", Float) = 0
		[HideInInspector]_DitheringStep("_DitheringStep", Float) = 0
		[HideInInspector]_TranslucencyFactor("_TranslucencyFactor", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "PreviewType"="Plane" }
		ColorMask RG

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile ALPHAMODE_NOALPHA ALPHAMODE_ALPHATEST ALPHAMODE_DITHERING ALPHAMODE_TRANSLUCENCY

			#include "UnityCG.cginc"
			#include "../UIDepthLib.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 screenPos : TEXCOORD1;
				float worldZPos : TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _UIParticleCanvasZMin;
			float _UIParticleCanvasZMax;
			
		#if ALPHAMODE_TRANSLUCENCY
			fixed _TranslucencyFactor;
		#endif
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.screenPos = o.pos;
				o.worldZPos = mul(unity_ObjectToWorld, v.vertex).z;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				discardMaskByAlpha(col.a, i.screenPos);
				fixed zDepthVal = saturate((i.worldZPos - _UIParticleCanvasZMin) / (_UIParticleCanvasZMax - _UIParticleCanvasZMin));
			#if ALPHAMODE_TRANSLUCENCY
				return fixed4(zDepthVal,col.a * _TranslucencyFactor,0,0);
			#else
				return fixed4(zDepthVal,0,0,0);
			#endif
			}
			ENDCG
		}
	}
}
