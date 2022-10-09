Shader "HighlightPlus/Geometry/Target" {
Properties {
    _MainTex ("Texture", 2D) = "white" {}
    _Color ("Color", Color) = (1,1,1,1)
    _ZTest ("ZTest", Int) = 0
}

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent-1" "DisableBatching" = "True" }

        // Target FX decal
        Pass
        {
            Stencil {
                Ref 2
                Comp NotEqual
                ReadMask 2
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest [_ZTest]
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
                float4 rayVS      : TEXCOORD1;
                float3 camPosVS   : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _Color;
            float4 _TargetFXRenderData;
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
        
            #define GROUND_NORMAL _TargetFXRenderData.xyz
            #define FADE_POWER _TargetFXRenderData.w

            #define UNITY_MATRIX_I_M   unity_WorldToObject

            v2f vert(appdata input)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = UnityObjectToClipPos(input.positionOS);
                o.screenPos = ComputeScreenPos(o.positionCS);

                float3 viewRay = UnityObjectToViewPos(input.positionOS);
                o.rayVS.w = viewRay.z;
                float4x4 viewToObject = mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V);
                o.rayVS.xyz = mul((float3x3)viewToObject, -viewRay);
                o.camPosVS = mul(viewToObject, float4(0,0,0,1)).xyz;
                return o;
            }



            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float depth = UNITY_SAMPLE_DEPTH(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w));
                float3 decalPos;
                if(unity_OrthoParams.w) {
                    #if defined(UNITY_REVERSED_Z)
                        depth = 1.0 - depth;
                    #endif
                    float sceneDepthVS = lerp(_ProjectionParams.y, _ProjectionParams.z, depth);
				    float2 rayVSEnd = float2(unity_OrthoParams.xy * (i.screenPos.xy - 0.5) * 2.0);
				    float4 posVS = float4(rayVSEnd, -sceneDepthVS, 1);       
				    float3 wpos = mul(UNITY_MATRIX_I_V, posVS).xyz;
                    decalPos = mul(UNITY_MATRIX_I_M, float4(wpos, 1)).xyz;
                } else {
                    float depthEye = LinearEyeDepth(depth);
                    decalPos = i.camPosVS + (i.rayVS.xyz / i.rayVS.w) * depthEye;
                }
                clip(0.5 - abs(decalPos));

                // check normal
                float3 normal = normalize(cross(ddx(decalPos), -ddy(decalPos)));
                float slope = dot(normal, GROUND_NORMAL);
                clip(slope - 0.01);
            
                float2 uv = decalPos.xz + 0.5;
                half4 col = tex2D(_MainTex, uv);
                col *= _Color;

                // atten with elevation
                col.a /= 1.0 + pow(1.0 + max(0, decalPos.y - 0.1), FADE_POWER);

                return col;
            }
            ENDCG
        }


        // Regular target fx
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest [_ZTest]
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
      		fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	return tex2D(_MainTex, i.uv) * _Color;
            }
            ENDCG
        }

    }
}