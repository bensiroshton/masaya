
Shader "Masaya/UIBackgroundShader"
{
	Properties
	{
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
		_Radius("Radius", float) = 30
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
			Name "GaussianBlurHorizontal"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature FLIP_Y
		
			sampler2D _BaseMap;
			half4 _BaseMap_TexelSize;
			static const float weight[] = {0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162};
			half _Radius;

			#define STEPS 5
			#define RADIUS 30.0

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
				half2 uv = input.texcoord.xy;
				#ifdef FLIP_Y
					uv.y = 1.0 - uv.y;
				#endif
				half2 uvoff = _BaseMap_TexelSize.xy * 0.5f; // we use this offset to let the GPU subsample our color between pixels (free blending).
				half4 c = tex2D(_BaseMap, uv) * weight[0];
				float offsetMultiplier = _Radius / STEPS;

				[unroll] for(int i=1;i<STEPS;i++)
				{
					c += tex2D(_BaseMap, uv + half2(i * _BaseMap_TexelSize.x * offsetMultiplier + uvoff.x, 0)) * weight[i];
					c += tex2D(_BaseMap, uv - half2(i * _BaseMap_TexelSize.x * offsetMultiplier + uvoff.x, 0)) * weight[i];
				}

				c.a = 1;
				return c;
			}
			ENDHLSL
		}

		Pass
		{
			Name "GaussianBlurVertical"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature FLIP_Y

			sampler2D _BaseMap;
			half4 _BaseMap_TexelSize;
			half _Radius;
			static const float weight[] = {0.2270270270, 0.1945945946, 0.1216216216, 0.0540540541, 0.0162162162};
			#define STEPS 5

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
				half2 uv = input.texcoord.xy;
				#ifdef FLIP_Y
					uv.y = 1.0 - uv.y;
				#endif
				half2 uvoff = _BaseMap_TexelSize.xy * 0.5f; // we use this offset to let the GPU subsample our color between pixels (free blending).
				half4 c = tex2D(_BaseMap, uv) * weight[0];
				float offsetMultiplier = _Radius / STEPS;

				[unroll] for(int i=1;i<STEPS;i++)
				{
					c += tex2D(_BaseMap, uv + half2(0, i * _BaseMap_TexelSize.y * offsetMultiplier + uvoff.y)) * weight[i];
					c += tex2D(_BaseMap, uv - half2(0, i * _BaseMap_TexelSize.y * offsetMultiplier + uvoff.y)) * weight[i];
				}

				c.a = 1;
				return c;
			}
			ENDHLSL
		}

		Pass
		{
			Name "Contrast"

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma shader_feature FLIP_Y
		
			sampler2D   _BaseMap;

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
				#ifdef FLIP_Y
					uv.y = 1.0 - uv.y;
				#endif
				half4 c = tex2D(_BaseMap, uv);
				half contrast = 1.05;

				c = saturate(lerp(half4(0.5, 0.5, 0.5, 1.0), c, contrast));

				return c;
			}
			ENDHLSL
		}

	}
}
