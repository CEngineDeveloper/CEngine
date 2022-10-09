Shader "HighlightPlus/Geometry/SeeThroughInner" {
Properties {
    _MainTex ("Texture", Any) = "white" {}
    _SeeThrough ("See Through", Range(0,1)) = 0.8
    _SeeThroughTintColor ("See Through Tint Color", Color) = (1,0,0,0.8)
    _SeeThroughNoise("Noise", Float) = 1
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _SeeThroughBorderWidth ("Outline Offset", Float) = 0.01
    _SeeThroughBorderConstantWidth ("Constant Width", Float) = 1
    _SeeThroughStencilRef ("Stencil Ref", Int) = 2
    _SeeThroughStencilComp ("Stencil Comp", Int) = 5
    _SeeThroughStencilPassOp ("Stencil Pass Operation", Int) = 0
    _SeeThroughDepthOffset ("Depth Offset", Float) = 0
    _SeeThroughMaxDepth("Max Depth", Float) = 0
    _SeeThroughOrdered("Ordered", Int) = 1
    _ZTest("ZTest", Int) = 4
}
    SubShader
    {
        Tags { "Queue"="Transparent+201" "RenderType"="Transparent" "DisableBatching"="True" }
   
        // See through effect
        Pass
        { 
            Stencil {
                ReadMask 3
                WriteMask 3
                Ref [_SeeThroughStencilRef]
                Comp [_SeeThroughStencilComp]
                Pass [_SeeThroughStencilPassOp]
                Fail [_SeeThroughStencilPassOp]
            }

            ZTest [_ZTest]
            ZWrite On // Needs to be on so see-through border do not overlap 
//            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ HP_ALPHACLIP
            #pragma multi_compile_local _ HP_DEPTH_OFFSET

            #include "UnityCG.cginc"
            #include "CustomVertexTransform.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos: SV_POSITION;
                float2 uv : TEXCOORD0;
                #if HP_DEPTH_OFFSET
                    float4 scrPos : TEXCOORD1;
                    float  depth  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed _SeeThrough;
            fixed4 _SeeThroughTintColor;
            fixed _CutOff;
            fixed _SeeThroughNoise;
            float _SeeThroughBorderWidth;
            float _SeeThroughBorderConstantWidth;
            float _SeeThroughDepthOffset;
            float _SeeThroughMaxDepth;
            int _SeeThroughOrdered;
	        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos    = ComputeVertexPosition(v.vertex);
                #if HP_DEPTH_OFFSET
                    o.scrPos = ComputeScreenPos(o.pos);
                    COMPUTE_EYEDEPTH(o.depth);
                #endif
                float3 norm   = mul ((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                float2 offset = TransformViewToProjection(norm.xy);
                float z = lerp(UNITY_Z_0_FAR_FROM_CLIPSPACE(o.pos.z), 2.0, UNITY_MATRIX_P[3][3]);
                z = _SeeThroughBorderConstantWidth * (z - 2.0) + 2.0;
                o.pos.xy -= offset * z * _SeeThroughBorderWidth;
                #if UNITY_REVERSED_Z
				    o.pos.z = _SeeThroughOrdered ? o.pos.w - (o.pos.w - o.pos.z) * 0.001 : o.pos.z;
                #else
                    o.pos.z = _SeeThroughOrdered ? -o.pos.w + ( o.pos.z + o.pos.w ) * 0.001 : o.pos.z;
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float GetEyeDepth(float rawDepth) {
                float persp = LinearEyeDepth(rawDepth);
                float ortho = (_ProjectionParams.z-_ProjectionParams.y)*(1-rawDepth)+_ProjectionParams.y;
                return lerp(persp,ortho,unity_OrthoParams.w);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                #if HP_DEPTH_OFFSET
                    float sceneZ = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.scrPos.xy / i.scrPos.w);
                    float sceneDepth = GetEyeDepth(sceneZ);
                    if (i.depth - sceneDepth - _SeeThroughDepthOffset < 0 || i.depth - sceneDepth > _SeeThroughMaxDepth) discard;
                #endif
                fixed4 col = tex2D(_MainTex, i.uv);
                #if HP_ALPHACLIP
                    clip(col.a - _CutOff);
                #endif
                col.rgb = lerp(col.rgb, _SeeThroughTintColor.rgb, _SeeThroughTintColor.a);
                float scry = i.pos.y;
                float time = _Time.w % 1.0;
                col.rgb += _SeeThroughNoise *(frac( scry * time ) * 0.1);
                col.a = _SeeThrough;
                col.a = lerp(col.a, col.a * ( (scry % 2) - 1.0 ), _SeeThroughNoise);
                return col;
            }
            ENDCG
        }

    }
}