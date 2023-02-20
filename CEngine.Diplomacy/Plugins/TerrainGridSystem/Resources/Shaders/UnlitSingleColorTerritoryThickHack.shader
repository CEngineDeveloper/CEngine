Shader "Terrain Grid System/Unlit Single Color Territory Thick Hack" {
 
Properties {
    _MainTex ("Texture", 2D) = "black" {}
    _Color ("Color", Color) = (1,1,1,1)
    _SecondColor ("Second Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", float) = -0.01  
    _NearClip ("Near Clip", Range(0, 1000.0)) = 25.0
    _FallOff ("FallOff", Range(1, 1000.0)) = 50.0
    _FarFadeDistance ("Far Fade Distance", Float) = 10000
    _FarFadeFallOff ("Far Fade FallOff", Float) = 50.0
    _CircularFadeDistance ("Circular Fade Distance", Float) = 250000
    _CircularFadeFallOff ("Circular Fade FallOff", Float) = 50.0
    _Thickness ("Thickness", Float) = 0.05
    _ZWrite("ZWrite", Int) = 0
    _SrcBlend("Src Blend", Int) = 5
    _DstBlend("Dst Blend", Int) = 10
	_StencilComp("Stencil Comp", Int) = 6 // not equal
	_StencilOp("Stencil Op", Int) = 2 // replace
}

SubShader {
    Tags {
      "Queue"="Geometry+208"
      "RenderType"="Opaque"
      "DisableBatching"="True"
    }
    Blend [_SrcBlend] [_DstBlend]
  	ZWrite [_ZWrite]
  	Cull Off
    Stencil {
        Ref 4
        Comp [_StencilComp]
        Pass [_StencilOp]
    }

    Pass {
	   	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#pragma target 3.0
		#pragma multi_compile __ TGS_NEAR_CLIP_FADE
		#pragma multi_compile __ TGS_FAR_FADE
		#pragma multi_compile __ TGS_CIRCULAR_FADE
        #pragma multi_compile __ TGS_GRADIENT

		#include "UnityCG.cginc"
		#include "TGSCommon.cginc"

		struct appdata {
			float4 vertex : POSITION;
			fixed4 color  : COLOR;
            float4 uv     : TEXCOORD0;
		};


		struct v2f {
			float4 pos    : SV_POSITION;
			fixed4 color  : COLOR;
            #if TGS_GRADIENT
                fixed  gradient : TEXCOORD0;
                fixed4 secondColor  : TEXCOORD1;
            #endif
		};

        float4 ComputePos(float4 v) {
            float4 vertex = UnityObjectToClipPos(v);
			#if UNITY_REVERSED_Z
				vertex.z -= _Offset;
			#else
				vertex.z += _Offset;
			#endif
            return vertex;
        }

        fixed4 _SecondColor;
        fixed _AnimationSpeed;

        #if TGS_GRADIENT
		v2f vert(appdata v, uint vid : SV_VertexID) {
        #else
        v2f vert(appdata v) {
        #endif
            v2f o;
			float4 p0 = ComputePos(v.vertex);

           float4 p1 = ComputePos(v.uv);

           float4 ab = p1 - p0;

           float4 normal = float4(-ab.y, ab.x, 0, 0);
           normal.xy = normalize(normal.xy) * GetThickness();

           float aspect = _ScreenParams.x / _ScreenParams.y;
           normal.y *= aspect;

           o.pos = p0 + normal * v.uv.w - normal.yxww * float4(0.25,-0.25,0,0);

		   APPLY_FADE(_Color, v.vertex, o.pos, o.color)

           #if TGS_GRADIENT
               APPLY_FADE(_SecondColor, v.vertex, o.pos, o.secondColor)
               uint vorder = vid % 4;
               o.gradient = vorder == 0 || vorder == 2;
           #endif

           return o;
 		}
		
		fixed4 frag(v2f i) : SV_Target {
            fixed4 color;
            #if TGS_GRADIENT
                color = lerp(i.color, i.secondColor, frac(i.gradient + _Time.y * _AnimationSpeed));
            #else
                color = i.color;
            #endif
			return color;
		}
		ENDCG
    }
            
 }
 Fallback Off
}
