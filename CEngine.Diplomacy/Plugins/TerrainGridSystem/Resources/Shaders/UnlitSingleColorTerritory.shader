Shader "Terrain Grid System/Unlit Single Color Territory Thin Line" {
 
Properties {
    _MainTex ("Texture", 2D) = "black" {}
    _Color ("Color", Color) = (1,1,1)
    _Offset ("Depth Offset", float) = -0.01    
    _NearClip ("Near Clip", Range(0, 1000.0)) = 25.0
    _FallOff ("FallOff", Range(1, 1000.0)) = 50.0
    _FarFadeDistance ("Far Fade Distance", Float) = 10000
    _FarFadeFallOff ("Far Fade FallOff", Range(1, 1000.0)) = 50.0
    _CircularFadeDistance ("Circular Fade Distance", Float) = 250000
    _CircularFadeFallOff ("Circular Fade FallOff", Float) = 50.0
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
    }
    Blend [_SrcBlend] [_DstBlend]
  	ZWrite [_ZWrite]
    Stencil {
        Ref 4
        Comp [_StencilComp]
        Pass [_StencilOp]
    }

        Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag				
		#pragma multi_compile __ TGS_NEAR_CLIP_FADE
		#pragma multi_compile __ TGS_FAR_FADE
		#pragma multi_compile __ TGS_CIRCULAR_FADE

		#include "UnityCG.cginc"			
		#include "TGSCommon.cginc"

		struct AppData {
			float4 vertex : POSITION;					
		};

		struct VertexToFragment {
			float4 pos : SV_POSITION;		
			fixed4 color : COLOR;					
		};
		
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
				o.pos.z -= _Offset;
			#else
				o.pos.z += _Offset;
			#endif
			APPLY_FADE(_Color, v.vertex, o.pos, o.color)
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return i.color;
		}
			
		ENDCG
}
 }
}
