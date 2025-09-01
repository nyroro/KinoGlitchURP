using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KinoGlitchRenderFeature : ScriptableRendererFeature
{
    private KinoGlitchPass m_GlitchPass;
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.postProcessEnabled &&
            renderingData.cameraData.cameraType == CameraType.Game)
        {
            renderer.EnqueuePass(m_GlitchPass);
        }
    }

    public override void Create()
    {
        m_GlitchPass = new KinoGlitchPass();
    }

    protected override void Dispose(bool disposing)
    {
        m_GlitchPass?.Dispose();
    }
}

public class KinoGlitchPass : ScriptableRenderPass
{
    private const string k_ProfilerTag = "Kino Glitch Processing";
    private int tempId = Shader.PropertyToID("_KinoGlitchTemp");
    private RenderTargetIdentifier src, dest;

    private Dictionary<string, Material> glitchMaterials = new Dictionary<string, Material>();

    public KinoGlitchPass()
    {
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        var desc = renderingData.cameraData.cameraTargetDescriptor;
        src = renderingData.cameraData.renderer.cameraColorTarget;
        cmd.GetTemporaryRT(tempId, desc, FilterMode.Bilinear);
        dest = new RenderTargetIdentifier(tempId);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.postProcessEnabled)
        {
            return;
        }

        SetupMaterial<AnalogGlitchVolume>(context, "Hidden/Kino/Glitch/Analog");
        SetupMaterial<DigitalGlitchVolume>(context, "Hidden/Kino/Glitch/Digital");

        var cmd = CommandBufferPool.Get(k_ProfilerTag);
        foreach (var mat in glitchMaterials.Values)
        {
            Blit(cmd, src, dest, mat, 0);
            Blit(cmd, dest, src);
        }
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void BlitToRenderTexture(ScriptableRenderContext context, RenderTexture renderTexture)
    {
        var cmd = CommandBufferPool.Get(k_ProfilerTag);
        Blit(cmd, src, renderTexture);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void SetupMaterial<TVolume>(ScriptableRenderContext context, string shaderName) where TVolume : VolumeComponent, IPostProcessComponent, IGlitchVolume
    {
        var stack = VolumeManager.instance.stack;
        var glitchVolume = stack.GetComponent<TVolume>();
        if (glitchVolume == null || !glitchVolume.IsActive())
        {
            return;
        }

        var volumeTypeName = typeof(TVolume).Name;
        Material glitchMat;
        if (glitchMaterials.ContainsKey(volumeTypeName))
        {
            glitchMat = glitchMaterials[volumeTypeName];
        }
        else
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                return;
            }

            glitchMat = CoreUtils.CreateEngineMaterial(shader);
            glitchMaterials[volumeTypeName] = glitchMat;
        }
        glitchVolume.SetupGlitch(this, context, glitchMat);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(tempId);
    }

    public void Dispose()
    {
        foreach (var mat in glitchMaterials.Values)
        {
            CoreUtils.Destroy(mat);   
        }
    }
}