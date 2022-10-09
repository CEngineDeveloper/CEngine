Shader "Hidden/FogOfWarHardware"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_CenterPosition("Center Position", Vector) = (0, 0, 0, 0)
		_EyePosition("Eye Position", Vector) = (0, 0, 0, 0)
		_EyeForward("Eye Forward", Vector) = (0, 1, 0, 0)
		_Offset("Offset", Vector) = (0, 0, 0, 0)
		_Size("Size", Vector) = (1, 1, 1, 1)
		_Angle("Angle", Range(-1, 1)) = 0.9
		_ForwardAngle("ForwardAngle", Range(-1, 1)) = 0.9
		_Brightness("Brightness", Range(0, 1)) = 1
		_LineOfSightTex("Line Of Sight Texture", 2D) = "white" {}
		_LineOfSightValues("Line Of Sight Values", Vector) = (1, 0, 360, 1) // distance, min angle, max angle, can see outside range
	}
	SubShader
	{
		ZTest Always
		Cull Off
		ZWrite Off
		BlendOp Min
		Fog { Mode Off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile SHAPE_CIRCLE SHAPE_BOX
			#pragma multi_compile _ LINE_OF_SIGHT LINE_OF_SIGHT_CELLS
			
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float2 _CenterPosition;
			float2 _EyePosition;
			float4 _EyeForward;
			float2 _Offset;
			float2 _Size;
			float _Angle;
			float _ForwardAngle;
			float _Brightness;
			sampler2D _LineOfSightTex;
			float4 _LineOfSightValues;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed InverseLerp(fixed a, fixed b, fixed t)
			{
				return (t - a) / (b - a);
			}

			fixed2 Rotate(fixed2 pos, fixed angle)
			{
				float ss = sin(angle);
				float cc = cos(angle);

				return fixed2(
					pos.x * cc - pos.y * ss,
					pos.y * cc + pos.x * ss);
			}

			fixed ClockwiseAngle(fixed2 from, fixed2 to)
			{
				fixed angle = acos(dot(normalize(from), normalize(to)));
				if (dot(from, fixed2(to.y, to.x)) < 0.0f)
					angle = -angle;
				return angle;
			}

			fixed AngleInverseLerp(fixed a, fixed b, fixed t)
			{
				t = InverseLerp(a, b, t);
				fixed total = 360 / (b - a);
				t %= total;
				if (t < 0)
					t += total;
				return t;
			}

			fixed frag (v2f i) : SV_Target
			{
				fixed lineofsightdistance = _LineOfSightValues.x;
				fixed lineofsightminangle = _LineOfSightValues.y;
				fixed lineofsightmaxangle = _LineOfSightValues.z;
				fixed lineofsightseeoutside = _LineOfSightValues.w;

#if LINE_OF_SIGHT
				fixed2 offset = i.uv - _EyePosition.xy;
				fixed maxdist = length(offset);
				fixed lineofsightangle = ClockwiseAngle(fixed2(1, 0), offset) * -360 / (3.1415 * 2) + 90;
				fixed lineofsightcoord = AngleInverseLerp(lineofsightminangle, lineofsightmaxangle, lineofsightangle);
				if (lineofsightcoord < 0 || lineofsightcoord > 1)
				{
					if (lineofsightseeoutside < 0.5)
						return 1;
				}
				else
				{
					fixed lineofsightdist = tex2D(_LineOfSightTex, fixed2(lineofsightcoord, 0)).r * lineofsightdistance;
					if (maxdist >= lineofsightdist)
						return 1;
				}
#elif LINE_OF_SIGHT_CELLS
				fixed2 uv = (i.uv - _EyePosition.xy - 0.0001) / lineofsightdistance + 0.5; // the 0.0001 here is a bit hacky but should work fine on low res maps
				if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
					return 1;

				if (tex2D(_LineOfSightTex, uv).a < 0.5)
					return 1;
#endif

#if SHAPE_CIRCLE
				fixed angle = dot(normalize(_EyeForward), normalize(i.uv - _CenterPosition.xy));
				if (_Angle > angle)
					return 1;

				fixed dist = length(i.uv - _CenterPosition.xy) / _Size.x;
				dist = InverseLerp(_Size.y, 1, dist);
				fixed c = saturate(dist);
#elif SHAPE_BOX
				fixed2 boxuv = i.uv - _CenterPosition.xy;
				boxuv = Rotate(boxuv, _ForwardAngle) / _Size.xy + 0.5;

				if (boxuv.x < 0 || boxuv.x > 1 || boxuv.y < 0 || boxuv.y > 1)
					return 1;

				fixed c = 1 - tex2D(_MainTex, boxuv).a;
#endif

				c = 1 - (1 - c) * _Brightness;
				return c;
			}
			ENDCG
		}
	}
}
