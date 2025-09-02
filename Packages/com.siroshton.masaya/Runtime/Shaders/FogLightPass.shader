
// built in shader variables
// https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html

Shader "Masaya/FogLightPass"
{
		HLSLINCLUDE

		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS 
		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE 
		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_SCREEN
		#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
		#include "ShaderCommon.hlsl"

		float3 _LightColor;
		float3 _LightDir;
		float _LightIntensity;
		int _LightIndex;
		float _PixelRange;

        struct Attributes
        {
            // The positionOS variable contains the vertex positions in object space.
            float4 positionOS : POSITION;
			float3 normal : NORMAL;
        };

        struct Varyings
        {
            // The positions in this struct must have the SV_POSITION semantic.
            float4 positionClip : SV_POSITION;
			float2 screenUV : TEXCOORD;
			float lightLineDistance : POSITION1;
			float alpha : POSITION2;
			float distanceToLight : POSITION3;
			float3 viewDir : POSITION4;
			float3 positionNDC : POSITION5;
        };

        Varyings VertexMain(Attributes input)
        {
            Varyings o;
            o.positionClip = TransformObjectToHClip(input.positionOS.xyz);
			o.positionNDC = ClipToNDC(o.positionClip);

			float3 lightPos = TransformObjectToWorld(float3(0, 0, 0));
			o.viewDir = -ClipToNormalizedViewDirWS(o.positionClip);

			float3 dirToLight = normalize(lightPos - cameraPos);
			o.distanceToLight = distance(cameraPos, lightPos);
			float camAngleToLight = GetAngle(o.viewDir, dirToLight);
			// find distance the light is to our view line (from camera position along camera view ray)
			// solve with simple right triangle formula: distance = side a = hypotenuse * sin(alpha = angle between side b and c)
			o.lightLineDistance = o.distanceToLight * sin(camAngleToLight);

			// calculate our screen position in uv coordinates (for the depth lookup)
			o.screenUV = ClipToScreenUV(o.positionClip);
			
			// determine distance fragment will be from light center on the screen
			float2 center = ObjectToScreen(float3(0, 0, 0));
			float d = distance(center, UVToScreen(o.screenUV));
			o.alpha = 0.5 - d / _PixelRange;
            
			return o;
        }

        float4 FogLightFragment(Varyings input) : SV_Target
        {
			float2 depthUV = input.positionClip.xy / _ScaledScreenParams.xy;
			#if UNITY_REVERSED_Z
				float depth = SampleSceneDepth(depthUV);
			#else
				// Adjust z to match NDC for OpenGL
				float depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(depthUV));
			#endif

			float3 worldPos = ComputeWorldSpacePosition(depthUV, depth, UNITY_MATRIX_I_VP);
			float distanceToFragment = LinearDepthToEyeDepth(depth);
			//depth = distance(worldPos, cameraPos);
			depth = distanceToFragment;

			//float3 viewDir = normalize(worldPos - cameraPos);

			//float depth = SampleSceneDepth(depthUV);
			//float depth = SampleSceneDepth(float2(input.screenUV.x, 1.0 - input.screenUV.y));
			//depth = LinearDepthToEyeDepth(depth);

			//float3 worldPos = ViewToWorld(UVToView(input.screenUV, depth));
            
			//float cameraDistance = depth / dot(input.viewDir, cameraForward);
            //float3 worldPos = input.viewDir * cameraDistance + cameraPos;

			//float3 worldPos = ComputeWorldSpacePosition(input.positionNDC, depth, unity_CameraInvProjection);
			
			//float3 worldPos = cameraPos + input.viewDir * distanceToFragment;

			//VertexPositionInputs positions = GetVertexPositionInputs(worldPos);
			//float4 shadowCoords = GetShadowCoord(positions);
			//half shadowAmount = MainLightRealtimeShadow(shadowCoords);

			half shadowAmount = AdditionalLightRealtimeShadow(_LightIndex, worldPos, _LightDir).x;

			float strength = shadowAmount * clamp(atan(depth / input.lightLineDistance) / input.lightLineDistance, 0, 5) * _LightIntensity;

			return float4(_LightColor.r, _LightColor.g, _LightColor.b, strength * input.alpha);
			//return float4(input.alpha, input.alpha, input.alpha, 1);
			//return float4(depth, depth, depth, 1);
			//return float4(cdepth, cdepth, ldepth, 1);
			//return float4(shadowAmount, shadowAmount, shadowAmount, 1);
			//return float4(worldPos.xyz, 1);
			//return float4(input.lightLineDistance * 0.5, input.lightLineDistance * 0.5, input.lightLineDistance * 0.5, 1);
			//return float4(input.viewDir, 1);
			//return float4(viewDir, 1);
        }
    
    ENDHLSL
    
    SubShader
    {
        Tags { "RenderType"="AlphaTest" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off 
		ZTest LEqual
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "FogLightPass"

            HLSLPROGRAM
            
            #pragma vertex VertexMain
            #pragma fragment FogLightFragment
            
            ENDHLSL
        }
       
    }
}