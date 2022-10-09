Shader "MODev/UIParticle/ParticleWithDistortion"
{
	Properties
	{
		_DistortionMap("_DistortionMap", 2D) = "bump" {}
		_DistortionPower("Distortion power", Range(0, 0.1)) = 0.05
			
		[Toggle(USE_MAIN_TEX_COLORIZE)] _UseMainTexToColorize("Use main texture to colorize?", Float) = 0
		[Toggle(USE_MAIN_TEX_ALPHA_MASK)] _UseMainTexAsAlphaMask("Use main texture as alpha mask?", Float) = 0
		_MainTex("_MainTex", 2D) = "white" {}

		[Space]
		[Toggle(COLORIZE)] _Colorize("Colorize main texture?", Float) = 0
		_Color("_Color", Color) = (1,1,1,1)

		[Space]
		[Toggle(DISABLE_UI_CULLING)] _DisableCulling("Disable culling? (disables UI depth test)", Float) = 0

		[Space]
		[Enum(UnityEngine.Rendering.BlendMode)] _Blend("Blend mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _Blend2("Blend mode subset", Float) = 1

		[Space]
		[Toggle(SOFT_COLLISION_MODE)] _SoftCollisionMode("Enable Soft particle?", Float) = 0
		_UISoftModeFadeSmooth("Soft particle factor", Range(0,5)) = 1
			
		[HideInInspector]_ClippingMaskVal("_ClippingMaskVal", Range(0,1)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent+10" "PreviewType"="Plane" "IgnoreProjector" = "True" }
		Blend [_Blend] [_Blend2]
		Cull Off Lighting Off ZWrite Off

		GrabPass
        {
            "_GUIForDistortion"
        }
			
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature _ USE_MAIN_TEX_COLORIZE
			#pragma shader_feature _ USE_MAIN_TEX_ALPHA_MASK
			#pragma shader_feature _ DISABLE_UI_CULLING
			#pragma shader_feature _ COLORIZE
			#pragma shader_feature _ USE_CLIPPING_MASK
			#pragma shader_feature _ SOFT_COLLISION_MODE
			#pragma shader_feature CLIPPINGMODE_INSIDE CLIPPINGMODE_OUTSIDE

			#include "UnityCG.cginc"
			#include "UIDepthLib.cginc"

			#if defined(USE_MAIN_TEX_ALPHA_MASK) || defined(USE_MAIN_TEX_COLORIZE)
				#define USE_MAIN_TEX
			#endif

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
		#if defined(USE_MAIN_TEX)
				float2 uv : TEXCOORD0;
		#endif
				float2 depthTexUV : TEXCOORD1;
				float worldZPos : TEXCOORD2;
				fixed4 color : TEXCOORD3;
				float2 uvDistort : TEXCOORD4;
                float4 grabPos : TEXCOORD5;
			};

			sampler2D _DistortionMap;
			float4 _DistortionMap_ST;

			sampler2D _GUIForDistortion;
			float _DistortionPower;

		#if defined(USE_MAIN_TEX)
			sampler2D _MainTex;
			float4 _MainTex_ST;
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
			#if defined(USE_MAIN_TEX)
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			#endif
				o.uvDistort = TRANSFORM_TEX(v.uv, _DistortionMap);
				o.grabPos = ComputeGrabScreenPos(o.pos);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
			#if !defined(DISABLE_UI_CULLING)
				fixed2 depthMask = tex2D(_UIDepthTex, i.depthTexUV).ra;
				#if USE_CLIPPING_MASK
					#if CLIPPINGMODE_OUTSIDE
						clip(abs(_ClippingMaskVal - depthMask.g) - 0.0039/*aprox 1/255*/);
					#else //inside
						clip(0.0039/*aprox 1/255*/ - abs(_ClippingMaskVal - depthMask.g));
					#endif
				#endif
						
				#if SOFT_COLLISION_MODE
					float depthMaskZ = depthMask.r * (_UIParticleCanvasZMax - _UIParticleCanvasZMin) + _UIParticleCanvasZMin;
					clip(depthMaskZ - i.worldZPos);
					fixed softAlphaMul = saturate((depthMaskZ - i.worldZPos) * _UISoftModeFadeSmooth);
				#else
					fixed zDepthVal = saturate((i.worldZPos - _UIParticleCanvasZMin) / (_UIParticleCanvasZMax - _UIParticleCanvasZMin));
					clip(depthMask.r - zDepthVal);
				#endif
			#endif

				float3 distortionTex = UnpackNormal(tex2D(_DistortionMap, i.uvDistort));
				float distortionPower = i.color.a * _DistortionPower;
			#if defined(SOFT_COLLISION_MODE) && !defined(DISABLE_UI_CULLING)
				distortionPower *= softAlphaMul;
			#endif

				i.grabPos.xy += distortionTex.rg * distortionPower;

				fixed4 col = tex2Dproj(_GUIForDistortion, i.grabPos);
				col.a = 1.0 - abs(distortionTex.b);
				
			#if defined(USE_MAIN_TEX)
				fixed4 mainTexCol = tex2D(_MainTex, i.uv);
				#if USE_MAIN_TEX_ALPHA_MASK
					col.a *= mainTexCol.a;
				#endif

				#if USE_MAIN_TEX_COLORIZE
					#if COLORIZE
						mainTexCol *= _Color;
					#endif
					col = lerp(col, mainTexCol, mainTexCol.a);
				#endif
			#endif

				return col;
			}
			ENDCG
		}
	}

	CustomEditor "UIParticleShaderEditor"
}
