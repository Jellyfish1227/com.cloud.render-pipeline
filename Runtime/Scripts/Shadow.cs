using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private const string bufferName = "Shadows";
    private const int maxShadowDirectionalLightCount = 1;
    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }
    
    private CommandBuffer cmd = new CommandBuffer
    {
        name = bufferName
    };

    private ScriptableRenderContext context;

    private CullingResults cullingResults;

    private ShadowSettings settings;

    private ShadowedDirectionalLight[] shadowedDirectionalLight = 
        new ShadowedDirectionalLight[maxShadowDirectionalLightCount];

    private int shadowDirectionalLightCount;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = shadowSettings;
        shadowDirectionalLightCount = 0;
    }

    public void ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (shadowDirectionalLightCount < maxShadowDirectionalLightCount && light.shadows != LightShadows.None && 
            light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionalLight[shadowDirectionalLightCount++] = new ShadowedDirectionalLight
            {
                visibleLightIndex = visibleLightIndex
            };
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        cmd.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32,
            FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        cmd.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store);
        cmd.ClearRenderTarget(true, true, Color.clear);
        ExecuteBuffer();
    }

    public void Render()
    {
        if (shadowDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            cmd.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32,
                FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    public void Cleanup()
    {
        cmd.ReleaseTemporaryRT(dirShadowAtlasId);
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
}
