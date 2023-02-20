Shader "Terrain Grid System/Unlit Single Color Cell Thick Hack" {
 
Properties {
    _MainTex ("Texture", 2D) = "black" {}
    _Color ("Color", Color) = (1,1,1,1)
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
      "Queue"="Geometry+206"
      "RenderType"="Opaque"
    }
    Blend [_SrcBlend] [_DstBlend]
  	ZWrite [_ZWrite]
  	Cull Off
    Stencil {
        Ref 2
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
		
		v2f vert(appdata v) {
            v2f o;
			float4 p0 = ComputePos(v.vertex);

           float4 p1 = ComputePos(v.uv);

           float4 ab = p1 - p0;
           float4 normal = float4(-ab.y, ab.x, 0, 0);
           normal.xy = normalize(normal.xy) * GetThickness();

           float aspect = _ScreenParams.x / _ScreenParams.y;
           normal.y *= aspect;

           o.pos = p0 + normal * v.uv.w;

           APPLY_FADE(_Color, v.vertex, o.pos, o.color)

           return o;
 		}
		
		fixed4 frag(v2f i) : SV_Target {
			return i.color;
		}
		ENDCG
    }
            
 }
 Fallback Off
}
