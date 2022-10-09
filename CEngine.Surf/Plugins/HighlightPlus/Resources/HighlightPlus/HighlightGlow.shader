Shader "HighlightPlus/Geometry/Glow" {
Properties {
    _MainTex ("Texture", Any) = "white" {}
    _Glow ("Glow", Vector) = (1, 0.025, 0.75, 0.5)
    _Glow2 ("Glow2", Vector) = (0.01, 1, 0.5, 0)
    _GlowColor ("Glow Color", Color) = (1,1,1)
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _GlowDirection("GlowDir", Vector) = (1,1,0)
    _Cull ("Cull Mode", Int) = 2
    _ConstantWidth ("Constant Width", Float) = 1
	_GlowZTest ("ZTest", Int) = 4
    _GlowStencilOp ("Stencil Operation", Int) = 0
    _GlowStencilComp ("Stencil Comp", Int) = 6
}
    SubShader
    {
        Tags { "Queue"="Transparent+102" "RenderType"="Transparent" "DisableBatching"="True" }
      
        // Glow passes
        Pass
        {
        	Stencil {
                Ref 2
                Comp [_GlowStencilComp]
                Pass [_GlowStencilOp]
                ReadMask 2
                WriteMask 2
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]
            ZTest [_GlowZTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "CustomVertexTransform.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 pos   : SV_POSITION;
                fixed4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
            };

            //float4 _Glow; // x = intensity, y = width, z = magic number 1, w = magic number 2
            float3 _Glow2; // x = outline width, y = glow speed, z = dither on/off
            float _ConstantWidth;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _GlowColor)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Glow)
                UNITY_DEFINE_INSTANCED_PROP(float4, _GlowDirection)
            UNITY_INSTANCING_BUFFER_END(Props)


            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 glowDirection = UNITY_ACCESS_INSTANCED_PROP(Props, _GlowDirection);

                v.vertex.xyz += v.normal * glowDirection.z;
                float4 pos = ComputeVertexPosition(v.vertex);
                float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = TransformViewToProjection(normalize(norm.xy));

                offset += glowDirection.xy;
                float z = lerp(UNITY_Z_0_FAR_FROM_CLIPSPACE(pos.z), 2.0, UNITY_MATRIX_P[3][3]);
                z = _ConstantWidth * (z - 2.0) + 2.0;
                float outlineWidth = _Glow2.x;

                float4 glow = UNITY_ACCESS_INSTANCED_PROP(Props, _Glow);

                float animatedWidth = glow.y * (1.0 + 0.25 * sin(_Time.w * _Glow2.y));
                offset *= z * (outlineWidth + animatedWidth);
                pos.xy += offset;
				o.pos = pos;
                o.color = UNITY_ACCESS_INSTANCED_PROP(Props, _GlowColor);
                o.color.a = glow.x;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                fixed4 color = i.color;
                float4 glow = UNITY_ACCESS_INSTANCED_PROP(Props, _Glow);
                float2 screenPos = floor( i.pos.xy * glow.z ) * glow.w;
                color.a *= saturate(_Glow2.z + frac(screenPos.x + screenPos.y));
                return color;
            }
            ENDCG
        }
 
    }
}