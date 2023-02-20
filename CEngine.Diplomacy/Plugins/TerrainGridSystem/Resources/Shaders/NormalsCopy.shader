Shader "Terrain Grid System/NormalsCopy" {
Properties {
}

SubShader {

	CGINCLUDE
	#include "UnityCG.cginc"
	
	struct v2f {
    	float4 pos : SV_POSITION;
    	float3 norm : TEXCOORD0;
	};

	v2f vert( appdata_base v ) {
	    v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
	    o.norm = UnityObjectToWorldNormal(v.normal);
	    return o;
	}
	
	float4 frag(v2f i) : SV_Target {
		return float4(i.norm * 0.5 + 0.5, 1.0);
	}


	ENDCG

	Pass { // Depth snapshot
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		ENDCG
	}

}

Fallback Off
}
