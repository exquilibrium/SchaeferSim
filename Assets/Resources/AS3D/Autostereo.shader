Shader "Autostereo"
{
	Properties
	{
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#include "UnityCG.cginc"

			#pragma multi_compile __ UI_TEXTURE_ON
			#pragma multi_compile __ RECONSTRUCT

			struct v2f
			{
				float2 uv : TEXCOORD0;
			};

			v2f vert(
				float4 vertex : POSITION,
				float2 uv : TEXCOORD0,
				out float4 screenPos : SV_POSITION
			)
			{
				v2f o;
				o.uv = uv;
				screenPos = UnityObjectToClipPos(vertex);
				return o;
			}

			int u_viewshift = 4; // view start offset
			int u_numRenderViews = 8;
			int u_textureXSize = 3840;
			float u_lenticularSlope = 2.0f / 3.0f; // 2/3 or 4/5 or 40/51
			
			int u_viewportInvert = 0;
			int u_reconstructViews;
			int u_bgrDisplay;

			sampler2D colorView_0;
			sampler2D colorView_1;
			sampler2D colorView_2;
			sampler2D colorView_3;
			sampler2D colorView_4;
			sampler2D colorView_5;
			sampler2D colorView_6;
			sampler2D colorView_7;
			
			float u_jojoMap[8];

#ifdef UI_TEXTURE_ON
			sampler2D uiOverlayTexture;
#endif

#ifdef RECONSTRUCT
			sampler2D depthView_0;

			float4x4 u_sourceInvViewProj;
			float4x4 u_targetViewProj[8];
#endif

			float4 sampleColor(int viewIndex, float2 uv) {
				switch (viewIndex)
				{
				case 0: return tex2D(colorView_0, uv);
				case 1: return tex2D(colorView_1, uv);
				case 2: return tex2D(colorView_2, uv);
				case 3:	return tex2D(colorView_3, uv);
				case 4: return tex2D(colorView_4, uv);
				case 5:	return tex2D(colorView_5, uv);
				case 6:	return tex2D(colorView_6, uv);
				case 7:	return tex2D(colorView_7, uv);
				}
				return float4(0, 0, 0, 1.0);
			}

			int3 getSubPixelViewIndices(UNITY_VPOS_TYPE screenPos : VPOS)
			{
				float yCoord = u_viewportInvert != 1 ? u_textureXSize - screenPos.y : screenPos.y;

				int view = int(screenPos.x * 3.f + yCoord * u_lenticularSlope) + u_viewshift;
				int3 viewIndices = int3(view, view, view);

				viewIndices += u_bgrDisplay == 1 ? int3(2, 1, 0) : int3(0, 1, 2);

				viewIndices.x = u_jojoMap[viewIndices.x % u_numRenderViews];
				viewIndices.y = u_jojoMap[viewIndices.y % u_numRenderViews];
				viewIndices.z = u_jojoMap[viewIndices.z % u_numRenderViews];

				return viewIndices;
			}
			
			float3 sampleColorFromSubPixels(int3 subPixelIndices, float2 uv)
			{
				float3 pixelR = sampleColor(subPixelIndices.r, uv);
				float3 pixelG = sampleColor(subPixelIndices.g, uv);
				float3 pixelB = sampleColor(subPixelIndices.b, uv);

				return float3(pixelR.r, pixelG.g, pixelB.b);
			}

#ifdef RECONSTRUCT
			float3 reconstruct(float2 uv, UNITY_VPOS_TYPE screenPos : VPOS)
			{
				int3 subPixelIndices = getSubPixelViewIndices(screenPos);

				// read depth and reconstruct world position of pixel
				float depth = SAMPLE_DEPTH_TEXTURE(depthView_0, uv);				

				float4 worldPos = mul(u_sourceInvViewProj, float4(uv*2.0 - 1.0, depth, 1.0));
				worldPos.xyz /= worldPos.w;

				float3 color;
				
				float4 clipSpacePos = mul(u_targetViewProj[subPixelIndices.r], float4(worldPos.xyz, 1.0));
				clipSpacePos.xyz /= clipSpacePos.w;
				//float2 viewUV = clipSpacePos.xy*0.5 + 0.5;

				//return tex2D(colorView_0, viewUV).rgb;
				color.r = tex2D(colorView_0, clipSpacePos.xy*0.5 + 0.5).r;

				clipSpacePos = mul(u_targetViewProj[subPixelIndices.g], float4(worldPos.xyz, 1.0));
				clipSpacePos.xyz /= clipSpacePos.w;
				//viewUV = clipSpacePos.xy*0.5 + 0.5;

				color.g = tex2D(colorView_0, clipSpacePos.xy*0.5 + 0.5).g;

				clipSpacePos = mul(u_targetViewProj[subPixelIndices.b], float4(worldPos.xyz, 1.0));
				clipSpacePos.xyz /= clipSpacePos.w;
				//viewUV = clipSpacePos.xy*0.5 + 0.5;

				color.b = tex2D(colorView_0, clipSpacePos.xy*0.5 + 0.5).b;

				//color.r = subPixelIndices.r < u_numRenderViews / 2 ? leftTarget.r : rightTarget.r;
				//color.g = subPixelIndices.g < u_numRenderViews / 2 ? leftTarget.g : rightTarget.g;
				//color.b = subPixelIndices.b < u_numRenderViews / 2 ? leftTarget.b : rightTarget.b;

				return color;
			}
#endif

			float4 frag(v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : COLOR
			{

#ifdef UI_TEXTURE_ON
				const float4 ui = tex2D(uiOverlayTexture, i.uv);
#else
				const float4 ui = float4(0, 0, 0, 0);
#endif

				float4 color = ui;

				if (ui.a < 1.0)
				{
				#ifdef RECONSTRUCT

					color.rgb = reconstruct(i.uv, screenPos);

				#else

					int3 subPixelIndices = getSubPixelViewIndices(screenPos);
					color.rgb = sampleColorFromSubPixels(subPixelIndices, i.uv);

				#endif

					color.rgb = lerp(color.rgb, ui.rgb, ui.a);
				}

				color.a = 1.0;
				return color;
			}
			ENDCG
		}
	}
}
