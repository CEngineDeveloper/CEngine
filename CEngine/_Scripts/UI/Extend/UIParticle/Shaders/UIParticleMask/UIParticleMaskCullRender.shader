Shader "MODev/UIParticle/Mask/CullRender"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		_MaskVal("_MaskVal", Range(0,1)) = 0
		[HideInInspector]_AlphaTestTreshold("_AlphaTestTreshold", Float) = 0
		[HideInInspector]_DitheringStep("_DitheringStep", Float) = 0
		[HideInInspector]_TranslucencyFactor("_TranslucencyFactor", Float) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "PreviewType"="Plane" }
		ColorMask BA

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
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _MaskVal;
		#if ALPHAMODE_TRANSLUCENCY
			fixed _TranslucencyFactor;
		#endif
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.screenPos = o.pos;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				discardMaskByAlpha(col.a, i.screenPos);
			#if ALPHAMODE_TRANSLUCENCY
				return fixed4(0,0,col.a * _TranslucencyFactor,_MaskVal);
			#else
				return fixed4(0,0,0,_MaskVal);
			#endif
			}
			ENDCG
		}
	}
}
