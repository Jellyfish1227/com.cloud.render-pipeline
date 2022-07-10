using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CloudRP : RenderPipeline
{
    private CloudRenderer renderer = new CloudRenderer();
    private bool useDynamicBatching;
    private bool useZPrePass;
    private bool fillGBuffer;
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            renderer.Render(context, camera, useDynamicBatching, useZPrePass, fillGBuffer);
        }
    }

    public CloudRP(bool useDynamicBatching, bool useSRPBatcher, bool useZPrePass, bool fillGBuffer)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        this.useDynamicBatching = useDynamicBatching;
        this.useZPrePass = useZPrePass;
        this.fillGBuffer = fillGBuffer;
    }
}
