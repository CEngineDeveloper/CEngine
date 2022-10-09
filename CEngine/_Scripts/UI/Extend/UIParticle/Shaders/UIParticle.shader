Shader "MODev/UIParticle/Particle"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}

		[Space]
		[Toggle(COLORIZE)] _Colorize("Colorize?", Float) = 0
		_Color("_Color", Color) = (1,1,1,1)

		[Space]
		[Toggle(HDR_MODE)] _UseHDRMode("Enable HDR and _Intensity use?", Float) = 0
		_Intensity("_Intensity", Range(0,5)) = 1

		[Space]
		[Toggle(DISABLE_UI_CULLING)] _DisableCulling("Disable culling? (disables UI depth test)", Float) = 0

		[Space]
		[Enum(UnityEngine.Rendering.BlendMode)] _Blend("Blend mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _Blend2("Blend mode subset", Float) = 1

		[Space]
		[Toggle(SOFT_COLLISION_MODE)] _SoftCollisionMode("Enable Soft particle?", Float) = 0
		_UISoftModeFadeSmooth("Soft particle factor", Range(0,5)) = 1
			
		[Space]
		[Toggle(TRANSLUCENCY_MODE)] _TranslucencyMode("Enable UI Mask Translucency use?", Float) = 0

		[HideInInspector]_ClippingMaskVal("_ClippingMaskVal", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent+10" "PreviewType"="Plane" "IgnoreProjector" = "True" }
		Blend [_Blend] [_Blend2]
		Cull Off Lighting Off ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature DISABLE_UI_CULLING
			#pragma shader_feature COLORIZE
			#pragma shader_feature USE_CLIPPING_MASK
			#pragma shader_feature SOFT_COLLISION_MODE
			#pragma shader_feature HDR_MODE
			#pragma shader_feature CLIPPINGMODE_INSIDE CLIPPINGMODE_OUTSIDE
			#pragma shader_feature TRANSLUCENCY_MODE

			#include "UnityCG.cginc"
			#include "UIDepthLib.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 depthTexUV : TEXCOORD1;
				float worldZPos : TEXCOORD2;
				fixed4 color : TEXCOORD3;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			
		#if HDR_MODE
			#define precision half
			#define precision2 half2
			#define precision3 half3
			#define precision4 half4
			half _Intensity;
		#else
			#define precision fixed
			#define precision2 fixed2
			#define precision3 fixed3
			#define precision4 fixed4
		#endif

		#if COLORIZE
			fixed4 _Color;
		#endif

		#if USE_CLIPPING_MASK
			fixed _ClippingMaskVal;
		#endif

		#if SOFT_COLLISION_MODE
			float _UISoftModeFadeSmooth;
		#endif

			sampler2D _UIDepthTex;
			float2 _UIDepthTex_TexelSize;
			float _UIParticleCanvasZMin;
			float _UIParticleCanvasZMax;
			float4 _UIParticleDepthTexPosParams;//world xy, width, height
			
			inline float2 calcUIDepthTexUv(float3 worldPos)
			{
				return float2((worldPos.x - _UIParticleDepthTexPosParams.x) / _UIParticleDepthTexPosParams.z,
							  (worldPos.y - _UIParticleDepthTexPosParams.y) / _UIParticleDepthTexPosParams.w);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.color = v.color;
				o.pos = UnityObjectToClipPos(v.vertex);
				float3 wPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.worldZPos = wPos.z;
				o.depthTexUV = calcUIDepthTexUv(wPos);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			precision4 frag (v2f i) : SV_Target
			{
			#if !defined(DISABLE_UI_CULLING)
				fixed4 depthMask = tex2D(_UIDepthTex, i.depthTexUV);
				fixed translucencyAdd = 0;
				#if USE_CLIPPING_MASK
					#if CLIPPINGMODE_OUTSIDE
						clip(abs(_ClippingMaskVal - depthMask.a) - 0.0039/*aprox 1/255*/);
					#else //inside
						clip(0.0039/*aprox 1/255*/ - abs(_ClippingMaskVal - depthMask.a));
					#endif
					#if defined(TRANSLUCENCY_MODE)
						translucencyAdd = (1 - depthMask.b) * step(abs(_ClippingMaskVal - depthMask.a), 0.0039/*aprox 1/255*/);
					#endif
				#endif
						
				#if defined(TRANSLUCENCY_MODE)
					#if SOFT_COLLISION_MODE
						float depthMaskZ = depthMask.r * (_UIParticleCanvasZMax - _UIParticleCanvasZMin) + _UIParticleCanvasZMin;
						fixed softAlphaMul = saturate((depthMaskZ - i.worldZPos) * _UISoftModeFadeSmooth);
						depthMask.g *= step(i.worldZPos, depthMaskZ);
					#else
						fixed zDepthVal = saturate((i.worldZPos - _UIParticleCanvasZMin) / (_UIParticleCanvasZMax - _UIParticleCanvasZMin));
						depthMask.g *= step(depthMask.r, zDepthVal);
					#endif
				#else
				#if SOFT_COLLISION_MODE
					float depthMaskZ = depthMask.r * (_UIParticleCanvasZMax - _UIParticleCanvasZMin) + _UIParticleCanvasZMin;
					clip(depthMaskZ - i.worldZPos);
					fixed softAlphaMul = saturate((depthMaskZ - i.worldZPos) * _UISoftModeFadeSmooth);
				#else
					fixed zDepthVal = saturate((i.worldZPos - _UIParticleCanvasZMin) / (_UIParticleCanvasZMax - _UIParticleCanvasZMin));
					clip(depthMask.r - zDepthVal);
				#endif
			#endif
			#endif

				precision4 col = tex2D(_MainTex, i.uv) * i.color;
			#if COLORIZE
				col *= _Color;
			#endif
			#if HDR_MODE
				col.rgb *= _Intensity;
			#endif

			#if !defined(DISABLE_UI_CULLING)
				#if defined(SOFT_COLLISION_MODE)
				col.a *= softAlphaMul;
				#endif

				#if defined(TRANSLUCENCY_MODE)
					col.a *= saturate(1.0 - (translucencyAdd + depthMask.g));
				#endif
			#endif
				return col;
			}
			ENDCG
		}
	}

	CustomEditor "UIParticleShaderEditor"
}
