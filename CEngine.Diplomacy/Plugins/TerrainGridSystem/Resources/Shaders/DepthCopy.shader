Shader "Terrain Grid System/DepthCopy" {
Properties {
}

SubShader {

	CGINCLUDE
	#include "UnityCG.cginc"
	
	struct v2f {
    	float4 pos : SV_POSITION;
    	float depth : TEXCOORD0;
	};

	v2f vert( appdata_base v ) {
	    v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
	    o.depth = COMPUTE_DEPTH_01;
	    return o;
	}
	
	float4 frag(v2f i) : SV_Target {
		return float4(i.depth, 0.0, 1.0, 1.0);
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
