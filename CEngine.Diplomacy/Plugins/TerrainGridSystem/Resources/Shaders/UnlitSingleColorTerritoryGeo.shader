Shader "Terrain Grid System/Unlit Single Color Territory Geo" {
 
Properties {
    _MainTex ("Texture", 2D) = "black" {}
    _Color ("Color", Color) = (1,1,1,1)
    _Offset ("Depth Offset", float) = -0.01  
    _NearClip ("Near Clip", Range(0, 1000.0)) = 25.0
    _FallOff ("FallOff", Range(1, 1000.0)) = 50.0
    _FarFadeDistance ("Far Fade Distance", Float) = 10000
    _FarFadeFallOff ("Far Fade FallOff", Range(1, 1000.0)) = 50.0
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
		#pragma geometry geom
		#pragma fragment frag				
		#pragma target 4.0
		#pragma multi_compile __ TGS_NEAR_CLIP_FADE
		#pragma multi_compile __ TGS_FAR_FADE
		#pragma multi_compile __ TGS_CIRCULAR_FADE

		#include "UnityCG.cginc"
		#include "TGSCommon.cginc"		


		struct v2g {
			float4 vertex : POSITION;
			fixed4 color  : COLOR;
		};

		struct g2f {
			float4 pos    : SV_POSITION;
			fixed4 color  : COLOR;
		};
		
		void vert(inout v2g v) {
            float4 origVertex = v.vertex;
			v.vertex = UnityObjectToClipPos(v.vertex);
			#if UNITY_REVERSED_Z
				v.vertex.z -= _Offset;
			#else
				v.vertex.z += _Offset;
			#endif
			APPLY_FADE(_Color, origVertex, v.vertex, v.color)

		}

		[maxvertexcount(6)]
        void geom(line v2g p[2], inout TriangleStream<g2f> outputStream) {
           float4 p0 = p[0].vertex;
           float4 p1 = p[1].vertex;

           float4 ab = p1 - p0;
           float4 normal = float4(-ab.y, ab.x, 0, 0);
           float thickness = GetThickness();
           normal.xy = normalize(normal.xy) * thickness;

           float aspect = _ScreenParams.x / _ScreenParams.y;
           normal.y *= aspect;

           float4 tl = p0 - normal;
           float4 bl = p0 + normal;
           float4 tr = p1 - normal;
           float4 br = p1 + normal;
  		   float4 dd = float4(normalize(p1.xy-p0.xy), 0, 0) * thickness;

           g2f pIn;
           pIn.color = p[0].color;
           pIn.pos = p0 - dd;
           outputStream.Append(pIn);
           pIn.pos = bl;
           outputStream.Append(pIn);
           pIn.pos = tl;
           outputStream.Append(pIn);
           pIn.color = p[1].color;
           pIn.pos = br;
           outputStream.Append(pIn);
           pIn.pos = tr;
           outputStream.Append(pIn);
           pIn.pos = p1 + dd;
           outputStream.Append(pIn);
 		}
		
		fixed4 frag(g2f i) : SV_Target {
			return i.color;
		}
		ENDCG
    }
            
 }
 Fallback Off
}
