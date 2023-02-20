Shader "Terrain Grid System/Unlit Surface Color Overlay" {
 
Properties {
    _MainTex ("Texture", 2D) = "black" {}
    _Color ("Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", Int) = -1
    _ZWrite("ZWrite", Int) = 0
}
 
SubShader {
    Tags {
      "Queue"="Geometry+151"
      "RenderType"="Transparent"
  	}
  	Offset [_Offset], [_Offset]
  	Blend SrcAlpha OneMinusSrcAlpha
  	ZWrite [_ZWrite]
  	ZTest Always
    Pass {
    	CGPROGRAM
		#pragma vertex vert	
		#pragma fragment frag	
		#include "UnityCG.cginc"			

		fixed4 _Color;

		struct AppData {
			float4 vertex : POSITION;
		};

		struct VertexToFragment {
			fixed4 pos : SV_POSITION;	
		};

		//Vertex shader
		VertexToFragment vert(AppData v) {
			VertexToFragment o;							
			o.pos = UnityObjectToClipPos(v.vertex);
			return o;									
		}
		
		fixed4 frag(VertexToFragment i) : SV_Target {
			return _Color;
		}
			
		ENDCG
    }
    }
}
