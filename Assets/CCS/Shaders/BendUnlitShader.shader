/*
 * Author:李传礼
 * DateTime:2018.04.10
 * Description:弯曲控制
 */

Shader "StevenLi/Bend Unlit Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_P ("BendSize", float) = 0.3
		_XScaleIsYTimes("XScaleIsY Times", float) = 1
		_XScaleIsZTimes("XScaleIsZ Times", float) = 1
		_CurveXBendRatio ("CurveX Bend Ratio", Range(0, 1)) = 0
		_CurveYBendRatio ("CurveY Bend Ratio", Range(0, 1)) = 0

		_R ("Radius", float) = 0.3
		_BendRatio ("Bend Ratio", Range(0, 1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"
			#define pi 3.14159265358979

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			float _P;//抛物线顶点P
			float _XScaleIsYTimes;//x缩放是y缩放几倍
			float _XScaleIsZTimes;//x缩放是z缩放几倍
									
			float _CurveXBendRatio;//抛物线水平弯曲度
			float _CurveYBendRatio;//抛物线垂直弯曲度
				
			float _R;//半径
			float _BendRatio;//总的弯曲度

			float4 CreateCurvePoint(float4 vertex)
			{
				float xBendRatio = _CurveXBendRatio * _BendRatio;
				float yBendRatio = _CurveYBendRatio * _BendRatio;
				float disSquare = pow(vertex.x * xBendRatio, 2) + pow(vertex.y * yBendRatio, 2);
				vertex.z = -disSquare / (2 * _P) * _XScaleIsZTimes;
				return vertex;
			}

			float4 CreateCylinderPoint(float4 vertex)
			{
				if(_BendRatio > 0)
				{
					float r = _R / sin(pi / 2 * _BendRatio);
	
					float2 circleZeroPoint = float2(0, -r);
					float radian;
					if(r != 0)
						radian = abs(vertex.x) / r;
					else
						radian = 0;

					int dir = 0;
					if(vertex.x > 0)
						dir = 1;
					else if(vertex.x < 0)
						dir = -1;

					if(dir > 0)
						radian = pi / 2.0 - radian;
					else
						radian = radian + pi / 2.0;

					float newX = cos(radian) * r;
					float newZ = sin(radian) * r;
					float2 circlePoint = float2(newX, newZ) + circleZeroPoint;
	
					vertex.x = circlePoint.x;
					vertex.z = circlePoint.y * _XScaleIsZTimes;
				}

				return vertex;
			}

			float4 CreateSpherePoint(float4 vertex)
			{
				if(_BendRatio > 0)
				{
					float r = _R / sin(pi / 2 * _BendRatio);
					float alpha;
					if(vertex.x != 0)
						alpha = atan(vertex.y / vertex.x);
					else
					{
						if(vertex.y > 0)
							alpha = pi / 2;
						else if(vertex.y < 0)
							alpha = -pi / 2;
						else
							return float4(0, 0, 0, 1);
					}

					if(vertex.x < 0)
						alpha = pi + alpha;

					float dis = sqrt(pow(vertex.x, 2) + pow(vertex.y, 2));
					float beta;
					if(r != 0)
						beta = dis / r;
					else
						beta = 0;

					float newX = sin(beta) * r * cos(alpha);
					float newY = sin(beta) * r * sin(alpha);
					float newZ = cos(beta) * r - r;

					return float4(newX, newY * _XScaleIsYTimes, newZ * _XScaleIsZTimes, 1);
				}
				else
					return vertex;
			}
			
			v2f vert (appdata v)
			{
				v2f o;

				//拉伸抛物线
				float4 bendVertex = CreateCurvePoint(v.vertex);

				//未拉伸圆柱
				//float4 bendVertex = CreateCylinderPoint(v.vertex);

				//未拉伸球形
				//float4 bendVertex = CreateSpherePoint(v.vertex);

				o.vertex = UnityObjectToClipPos(bendVertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
