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
    private ComputeShader clusterLight;
    private bool clusterDebug;
    private ShadowSettings shadowSettings;
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            renderer.Render(context, camera, useDynamicBatching, useZPrePass, fillGBuffer, clusterLight, 
                clusterDebug, shadowSettings);
        }
    }

    public CloudRP(bool useDynamicBatching, bool useSRPBatcher, bool useZPrePass, bool fillGBuffer, 
        ComputeShader clusterLight, bool clusterDebug, ShadowSettings shadowSettings)
    {
        GraphicsSettings.useScriptableRenderPipelineBatching = useSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
        this.useDynamicBatching = useDynamicBatching;
        this.useZPrePass = useZPrePass;
        this.fillGBuffer = fillGBuffer;
        this.clusterLight = clusterLight;
        this.clusterDebug = clusterDebug;
        this.shadowSettings = shadowSettings;
    }
}
