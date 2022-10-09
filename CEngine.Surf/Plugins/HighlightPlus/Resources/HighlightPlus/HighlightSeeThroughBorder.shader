Shader "HighlightPlus/Geometry/SeeThroughBorder" {
Properties {
    _MainTex ("Texture", Any) = "white" {}
    _SeeThroughBorderColor ("Outline Color", Color) = (0,0,0,1)
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _CutOff("CutOff", Float ) = 0.5
    _SeeThroughStencilRef ("Stencil Ref", Int) = 2
    _SeeThroughStencilComp ("Stencil Comp", Int) = 5
    _SeeThroughDepthOffset ("Depth Offset", Float) = 0
    _SeeThroughMaxDepth("Max Depth", Float) = 0
    _SeeThroughStencilPassOp ("Stencil Pass Operation", Int) = 0
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
            ZWrite On
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
            fixed4 _SeeThroughBorderColor;
            fixed _CutOff;
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
                #if UNITY_REVERSED_Z
                    o.pos.z -= 0.0001;
                    o.pos.z = _SeeThroughOrdered ? o.pos.w - (o.pos.w - o.pos.z) * 0.001 : o.pos.z;
                #else
                    o.pos.z += 0.0001;
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
                #if HP_ALPHACLIP
                    fixed4 col = tex2D(_MainTex, i.uv);
            	    clip(col.a - _CutOff);
                #endif
                return _SeeThroughBorderColor;
            }
            ENDCG
        }

    }
}