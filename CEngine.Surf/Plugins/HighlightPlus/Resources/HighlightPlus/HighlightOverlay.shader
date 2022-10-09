Shader "HighlightPlus/Geometry/Overlay" {
Properties {
    _MainTex ("Texture", Any) = "white" {}
    _Color ("Color", Color) = (1,1,1) // not used; dummy property to avoid inspector warning "material has no _Color property"
    _OverlayColor ("Overlay Color", Color) = (1,1,1,1)
    _OverlayBackColor ("Overlay Back Color", Color) = (1,1,1,1)
    _OverlayData("Overlay Data", Vector) = (1,0.5,1,1)
    _OverlayHitPosData("Overlay Hit Pos Data", Vector) = (0,0,0,0)
    _OverlayTexture("Overlay Texture", 2D) = "white" {}
    _CutOff("CutOff", Float ) = 0.5
    _Cull ("Cull Mode", Int) = 2
}
    SubShader
    {
        Tags { "Queue"="Transparent+121" "RenderType"="Transparent" "DisableBatching"="True" }
    
        // Overlay
        Pass
        {
        	Stencil {
                Ref 4
                ReadMask 4
                Comp NotEqual
                Pass keep
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull [_Cull]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ HP_ALPHACLIP
            #pragma multi_compile_local _ HP_USES_OVERLAY_TEXTURE

            #include "UnityCG.cginc"
            #include "CustomVertexTransform.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 norm   : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float3 wpos   : TEXCOORD1;
                float3 wnorm  : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
            };

      		fixed4 _OverlayColor;
      		sampler2D _MainTex;
      		float4 _MainTex_ST;
      		fixed4 _OverlayBackColor;
      		fixed4 _OverlayData; // x = speed, y = MinIntensity, z = blend, w = texture scale
            float4 _OverlayHitPosData;
            float _OverlayHitStartTime;
      		fixed _CutOff;
            sampler2D _OverlayTexture;

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = ComputeVertexPosition(v.vertex);
                #if UNITY_REVERSED_Z
                    o.pos.z += 0.0001;
                #else
                    o.pos.z -= 0.0001;
                #endif
                o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.wnorm = UnityObjectToWorldNormal(v.norm);
                o.uv = TRANSFORM_TEX (v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
            	fixed4 color = tex2D(_MainTex, i.uv);
            	#if HP_ALPHACLIP
            	    clip(color.a - _CutOff);
            	#endif
                float time = _Time.y % 1000;
				fixed t = _OverlayData.y + (1.0 - _OverlayData.y) * 2.0 * abs(0.5 - frac(time * _OverlayData.x));
                fixed4 col = lerp(_OverlayColor, color * _OverlayBackColor * _OverlayColor, _OverlayData.z);
                col.a *= t;

                if (_OverlayHitPosData.w>0) {
                    float elapsed = _Time.y - _OverlayHitStartTime;
                    float hitDist = distance(i.wpos, _OverlayHitPosData.xyz);
                    float atten = saturate( min(elapsed, _OverlayHitPosData.w) / hitDist );
                    col.a *= atten;
                }

                #if HP_USES_OVERLAY_TEXTURE
                    half3 triblend = saturate(pow(i.wnorm, 4));
                    triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

                    // triplanar uvs
                    float3 tpos = i.wpos * _OverlayData.w;
                    float2 uvX = tpos.zy;
                    float2 uvY = tpos.xz;
                    float2 uvZ = tpos.xy;

                    // albedo textures
                    fixed4 colX = tex2D(_OverlayTexture, uvX);
                    fixed4 colY = tex2D(_OverlayTexture, uvY);
                    fixed4 colZ = tex2D(_OverlayTexture, uvZ);
                    fixed4 tex = colX * triblend.x + colY * triblend.y + colZ * triblend.z;
                    col *= tex;
                #endif

                return col;
            }
            ENDCG
        }

    }
}