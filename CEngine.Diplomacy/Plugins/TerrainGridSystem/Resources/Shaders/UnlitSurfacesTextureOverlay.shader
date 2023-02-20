Shader "Terrain Grid System/Unlit Surface Texture Overlay" {
 
Properties {
    _Color ("Color", Color) = (1,1,1)
    _MainTex ("Texture", 2D) = "black"
    _Offset ("Depth Offset", Int) = -1
    _ZWrite("ZWrite", Int) = 0
}
 
SubShader {
    Tags {
        "Queue"="Geometry+151"
        "RenderType"="Opaque"
    }
    Blend SrcAlpha OneMinusSrcAlpha
   	ZTest Always
   	ZWrite [_ZWrite]
   	Offset [_Offset], [_Offset]
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