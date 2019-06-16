Shader "Custom/Water"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_LineColor ("Line Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_DepthRange("Depth Range", Range(0, 15)) = 0.5
	}
	SubShader
	{
		Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		GrabPass
        {
            "_BackgroundTexture"
        }
		Pass
		{
			CGPROGRAM
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			#pragma vertex vert
			#pragma fragment frag
 			#pragma multi_compile __ APPEAR

			sampler2D _MainTex, _CameraDepthTexture;
			sampler2D _BackgroundTexture;
			float4 _MainTex_ST;
			float4 _Color, _LineColor;
			float _DepthRange;

			struct vertexInput
			{
			   float4 vertex : POSITION;
			   float2 uv : TEXCOORD0;
			};

			struct vertexOutput
			{
			   float4 pos : SV_POSITION;
			   float2 uv : TEXCOORD0;
			   float4 screenPos : TEXCOORD1;
			   float4 grabPos : TEXCOORD2;
			   UNITY_FOG_COORDS(3)
			   #ifdef APPEAR
					float4 worldPos : TEXCOORD4;
				#endif
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				output.pos = UnityObjectToClipPos(input.vertex);
				output.screenPos = ComputeScreenPos(output.pos);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				output.grabPos = ComputeGrabScreenPos(output.pos);
				UNITY_TRANSFER_FOG(output, output.vertex);
				#ifdef APPEAR
					output.worldPos = mul(unity_ObjectToWorld, input.vertex);
				#endif

				return output;
			}

			float nrand(float2 uv)
			{
				return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
			}

			float _Appear;
			float _AppearRange;
			float3 _AppearPos;
			inline void clipAnim(float4 worldPos)
			{
				fixed rand = frac(nrand(int2(worldPos.xz * 10 + _Time.zz)));
				float l = length(worldPos.xyz - _AppearPos);
				clip(-(l - _Appear) / _AppearRange + rand);
			}

			float4 frag(vertexOutput input) : COLOR
			{
				#ifdef APPEAR
				clipAnim(input.worldPos);
				#endif

				float depthSample = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.screenPos));
				float depth = _DepthRange * (LinearEyeDepth(depthSample.r) - input.screenPos.w);
				float l = saturate(depth);

				float4 col = lerp(_LineColor, _Color /* * tex2D(_MainTex, input.uv + float2(cos(_Time.z + input.uv.x * 5) * 0.01, cos(_Time.z * 0.5 + input.uv.y * 5) * 0.01))*/, l);
				col = col * lerp(_LightColor0, fixed4(1, 1, 1, 1), 0.5);
				UNITY_APPLY_FOG(input.fogCoord, col);

				//return col;
				float4 back = tex2Dproj(_BackgroundTexture, input.grabPos + saturate(depth * 0.5) * float4(cos(_Time.z + input.uv.x * 10) * 0.02, cos(_Time.z * 1.5 + input.uv.y * 10) * 0.02, 0, 0));
				return (1 - col.a) * back + col.a * col;
			}

			ENDCG
		}
	}
}