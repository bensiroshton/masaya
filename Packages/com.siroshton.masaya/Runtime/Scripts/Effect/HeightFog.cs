using Siroshton.Masaya.Mesh;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using Siroshton.Masaya.Util;

namespace Siroshton.Masaya.Effect
{

    public class HeightFog : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public Material fog;
            [Range(0, 1)] public float lightIntensity = 1.0f;
        }

        [SerializeField] private Settings _settings = new Settings();

        private class MaterialProperties
        {
            public int lightColor = Shader.PropertyToID("_LightColor");
            public int lightDir = Shader.PropertyToID("_LightDir");
            public int fogLightTexture = Shader.PropertyToID("_FogLightTexture");
            public int blitTexture = Shader.PropertyToID("_BlitTexture");
            public int pixelRange = Shader.PropertyToID("_PixelRange");
            public int lightIntensity = Shader.PropertyToID("_LightIntensity");
            public int lightIndex = Shader.PropertyToID("_LightIndex");
        }

        private static MaterialProperties materialProperties = new MaterialProperties();
        
        private struct LightDetails
        {
            public Matrix4x4 transform;
            public LightType type;
            public float range;
            public MaterialPropertyBlock properties;
        }

        private class FogLightPassData
        {
            public RendererListHandle rendererListHandle;
            public UnityEngine.Mesh sphere;
            public List<LightDetails> lights;
            public Material material;
            public TextureHandle depthTexture;
            public Util.ObjectPool<MaterialPropertyBlock> propertyPool;
        }

        private class FogLightResult : ContextItem
        {
            public TextureHandle fogLightTexture;

            public override void Reset()
            {
                fogLightTexture = TextureHandle.nullHandle;
            }
        }

        private class LightPass : ScriptableRenderPass
        {
            private const string k_FogLightTextureName = "_FogLightTexture";

            private Settings _settings;
            private Material _material;
            private RenderTextureDescriptor _outputDesc;
            private UnityEngine.Mesh _sphere;
            private Util.ObjectPool<MaterialPropertyBlock> _propertyPool;

            public LightPass(Settings settings)
            {
                _settings = settings;
                _material = Resources.Load<Material>("FogLightPass");
                _sphere = MeshUtil.CreateSphere();
                _propertyPool = new Util.ObjectPool<MaterialPropertyBlock>(() => { return new MaterialPropertyBlock(); });
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if( _settings.fog == null ) return;

                using (var builder = renderGraph.AddRasterRenderPass<FogLightPassData>("Fog Lights Pass", out var passData))
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                    // create output texture
                    _outputDesc = cameraData.cameraTargetDescriptor;
                    _outputDesc.depthBufferBits = 0;
                    //_outputDesc.width /= 2;
                    //_outputDesc.height /= 2;

                    TextureHandle outputTexture = UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        _outputDesc,
                        k_FogLightTextureName, 
                        true);

                    passData.propertyPool = _propertyPool;
                    passData.material = _material;
                    passData.sphere = _sphere;
                    //passData.depthTexture = resourceData.activeDepthTexture;
                    
                    // add light details
                    UniversalLightData lightData = frameData.Get<UniversalLightData>();
                    if (lightData.visibleLights != null)
                    {
                        passData.lights = new List<LightDetails>();
                        LightDetails ld;

                        int lightIndex = -1;
                        for (int i = 0; i < lightData.visibleLights.Length; i++)
                        {   
                            if( i == lightData.mainLightIndex ) continue;
                            lightIndex++;

                            VisibleLight light = lightData.visibleLights[i];
                            if (light.lightType == LightType.Point)
                            {
                                ld = new LightDetails
                                {
                                    transform = light.localToWorldMatrix,
                                    type = light.lightType,
                                    range = light.range,
                                    properties = _propertyPool.GetObject()
                                };
                                
                                Vector2 pixelPos = cameraData.camera.WorldToScreenPoint(light.light.gameObject.transform.position);
                                Vector2 pixelOffset = cameraData.camera.WorldToScreenPoint(light.light.gameObject.transform.position + cameraData.camera.transform.right * light.range);

                                ld.properties.SetInt(materialProperties.lightIndex, lightIndex);
                                ld.properties.SetColor(materialProperties.lightColor, light.finalColor);
                                ld.properties.SetVector(materialProperties.lightDir, light.light.transform.forward);
                                ld.properties.SetFloat(materialProperties.pixelRange, Vector2.Distance(pixelPos, pixelOffset));
                                ld.properties.SetFloat(materialProperties.lightIntensity, _settings.lightIntensity);
                                passData.lights.Add(ld);
                            }
                        }
                    }

                    // add output result to frame data
                    FogLightResult result = frameData.Create<FogLightResult>();
                    result.fogLightTexture = outputTexture;

                    // set builder data
                    builder.AllowPassCulling(false);
                    builder.SetRenderAttachment(outputTexture, 0);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Read);
                    builder.SetRenderFunc((FogLightPassData data, RasterGraphContext context) => ExecutePass(data, context));
                }
            }

            static void ExecutePass(FogLightPassData data, RasterGraphContext context)
            {
#if UNITY_EDITOR
                if( data.sphere == null ) return;
#endif

                if( data.lights != null )
                {
                    LightDetails ld;
                    for(int i=0;i<data.lights.Count;i++)
                    {
                        ld = data.lights[i];
                        context.cmd.DrawMesh(
                            data.sphere, 
                            ld.transform * Matrix4x4.Scale(new Vector3(ld.range, ld.range, ld.range)), 
                            data.material, 
                            0, 
                            0, 
                            ld.properties);

                        data.propertyPool.ReturnObject(ld.properties);
                    }
                }
            }
        }

        private class FogPass : ScriptableRenderPass
        {
            private const string k_FogTextureName = "_FogTexture";

            private RenderTextureDescriptor _outputDesc;
            private UnityEngine.Mesh _fullScreenQuad;
            private Settings _settings;

            private class FogPassData
            {
                public TextureHandle source;
                public TextureHandle destination;
                public Material material;
                public UnityEngine.Mesh mesh;
                public TextureHandle fogTexture;
            }

            public FogPass(Settings settings)
            {
                _settings = settings;
                _fullScreenQuad = MeshUtil.CreateFullscreenQuad();
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (_settings.fog == null) return;

                using (var builder = renderGraph.AddUnsafePass<FogPassData>(passName, out var passData, "HeightFog.cs"))
                {
                    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

                    _outputDesc = cameraData.cameraTargetDescriptor;
                    _outputDesc.depthBufferBits = 0;

                    TextureHandle outputTexture = UniversalRenderer.CreateRenderGraphTexture(
                        renderGraph,
                        _outputDesc,
                        k_FogTextureName,
                        true);

                    TextureHandle srcCamColor = resourceData.activeColorTexture;

                    FogLightResult lightResult = frameData.Get<FogLightResult>();
                    builder.UseTexture(lightResult.fogLightTexture, AccessFlags.Read);

                    passData.fogTexture = lightResult.fogLightTexture;
                    passData.source = srcCamColor;
                    passData.destination = outputTexture;
                    passData.material = _settings.fog;
                    passData.mesh = _fullScreenQuad;

                    resourceData.cameraColor = outputTexture;

                    builder.UseTexture(srcCamColor, AccessFlags.Read);
                    builder.UseTexture(outputTexture, AccessFlags.Write);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((FogPassData data, UnsafeGraphContext context) => ExecutePass(data, context));
                }
            }

            static void ExecutePass(FogPassData data, UnsafeGraphContext context)
            {
                if( data.mesh == null ) return;

                CommandBuffer unsafeCmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                data.material.SetTexture(materialProperties.blitTexture, data.source);
                data.material.SetTexture(materialProperties.fogLightTexture, data.fogTexture);

                context.cmd.SetRenderTarget(data.destination, 0, CubemapFace.Unknown, 0);
                context.cmd.DrawMesh(data.mesh, Matrix4x4.identity, data.material, 0, 0, null);
            }
        }

        private LightPass _lightPass;
        private FogPass _fogPass;

        public override void Create()
        {
            _lightPass = new LightPass(_settings);
            _lightPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            _fogPass = new FogPass(_settings);
            _fogPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            renderer.EnqueuePass(_lightPass);
            renderer.EnqueuePass(_fogPass);
        }

    }

}