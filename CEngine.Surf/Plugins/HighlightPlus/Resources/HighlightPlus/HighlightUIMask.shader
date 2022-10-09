Shader "HighlightPlus/UI/Mask" {
Properties {
    _MainTex ("Texture", Any) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _StencilComp("Stencil Comparison", Float) = 8
    _Stencil("Stencil ID", Float) = 0
    _StencilOp("Stencil Operation", Float) = 0
    _StencilWriteMask("Stencil Write Mask", Float) = 255
    _StencilReadMask("Stencil Read Mask", Float) = 255
    _ColorMask("Color Mask", Float) = 15
}
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector" = "True" }

        // Create mask
        Pass
        {
			Stencil {
                Ref 6
                Comp always
                Pass replace
                ReadMask 6
                WriteMask 6
            }
            ColorMask 0
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "CustomVertexTransform.cginc"

            sampler2D _MainTex;
      		float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
            fixed _CutOff;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 pos: SV_POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = ComputeVertexPosition(v.vertex);
				o.uv = TRANSFORM_TEX (v.uv, _MainTex);
				return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
          	    fixed4 col = tex2D(_MainTex, i.uv);
           	    clip(col.a - _CutOff);
            	return 0;
            }
            ENDCG
        }

    }
}