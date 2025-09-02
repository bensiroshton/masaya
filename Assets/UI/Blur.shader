Shader "CustomRenderTexture/Blur"
{
	Properties
	{
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
    }

	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
		Cull Off
		Blend Off
		ZTest Off
		ZWrite Off

		Pass
		{
			Name "Blur"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#define HALF_SIZE 8

			sampler2D   _BaseMap;
			half4       _BaseMap_TexelSize;

			struct MyAttributes
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct MyVaryings
			{
				float4 vertex : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			MyVaryings vert(MyAttributes input)
			{
				MyVaryings output;
				output.vertex = float4(input.vertex.xy * 2.0 - 1.0, 0.0, 1.0);
				output.texcoord = input.texcoord;
				return output;
			}

			half4 frag(MyVaryings input) : SV_Target
			{
				float2 uv = input.texcoord.xy;
				float4 c = float4(0, 0, 0, 0);

				[unroll] for (int y = -HALF_SIZE; y < HALF_SIZE; y++)
				{
					[unroll] for (int x = -HALF_SIZE; x < HALF_SIZE; x++)
					{
						c += tex2D(_BaseMap, uv + float2(x * _BaseMap_TexelSize.x, y * _BaseMap_TexelSize.y));
					}
				}

				return c / (HALF_SIZE * HALF_SIZE);
			}
			ENDHLSL
		}
	}
}
