Shader "Terrain Grid System/Unlit Surface Texture Ground" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "black"
    _Offset ("Depth Offset", Int) = -1
    _ZWrite("ZWrite", Int) = 0
    _SrcBlend("Src Blend", Int) = 5
    _DstBlend("Dst Blend", Int) = 10
    _StencilComp("Stencil Comp", Int) = 8
	_StencilRef("Stencil Ref", Int) = 8
    _StencilOp("Stencil Op", Int) = 0
}
 
SubShader {
    Tags {
        "Queue"="Geometry+101"
        "RenderType"="Opaque"
    }
    Blend [_SrcBlend] [_DstBlend]
   	ZWrite [_ZWrite]
   	Offset [_Offset], [_Offset]
	Stencil {
		Ref [_StencilRef]
		Comp [_StencilComp]
		Pass [_StencilOp]
		ReadMask [_StencilRef]
		WriteMask [_StencilRef]
    }
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag	
		#include "UnityCG.cginc"			

		sampler2D _MainTex;
		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
			float2 uv     : TEXCOORD0;
		};

		struct VertexToFragment {
			fixed4 pos : SV_POSITION;	
			float2 uv  : TEXCOORD0;
		};
		
		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv  = v.uv;
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			fixed4 color = tex2D(_MainTex, i.uv);
			return color * _Color;
		}
			
		ENDCG
    }
}
}