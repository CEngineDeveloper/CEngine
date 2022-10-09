Shader "TransparentShadowCaster"
{ 
	Properties 
	{ 
		_Color ("Main Color", Color) = (1,1,1,1)
	} 
 
	SubShader 
	{ 
		Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Transparent" }
 
		LOD 300
 
		Pass
		{ 
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
 
			Fog { Mode Off }
			ZWrite On
			ZTest Less Cull Off
			Offset 1, 1
 
			CGPROGRAM
			#pragma exclude_renderers gles
			#include "UnityCG.cginc"
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_shadowcaster
			
			float4 _Color; 
 
			struct v2f
			{ 
				V2F_SHADOW_CASTER; 
			};
 
			v2f vert(appdata_full v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER(o)
				return o;
			}
 
			float4 frag(v2f i) : COLOR
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		} 
 
 
		CGPROGRAM
		#pragma surface surf BlinnPhong alpha vertex:vert fullforwardshadows
 
		fixed4 _Color;
 
		struct Input
		{
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};
 
		struct v2f
		{ 
			V2F_SHADOW_CASTER; 
			float2 uv : TEXCOORD1;
		};
 
		v2f vert(inout appdata_full v)
		{ 
			v2f o;
			return o; 
		} 
 
		void surf(Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _Color.rgb;
			o.Alpha = _Color.a;
		}
		ENDCG
	}
 
	Fallback "Transparent/VertexLit"
}