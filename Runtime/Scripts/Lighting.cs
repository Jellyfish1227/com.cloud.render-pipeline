using System;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;

public class Lighting
{
    private const int maxDirLightCount = 4;
    private const string bufferName = "Lighting";
    private static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount");
    private static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColors");
    private static int dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirections");
    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];
    private CullingResults cullingResults;
    

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    private Shadows shadows = new Shadows();

    private ScriptableRenderContext context;

    public void Setup(ref ScriptableRenderContext ctx, ref CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        context = ctx;
        buffer.BeginSample(bufferName);
        shadows.Setup(ctx, cullingResults, shadowSettings);
        SetupLight();
        shadows.Render();
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    void SetupLight()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight light = visibleLights[i];
            switch (light.lightType)
            {
                case LightType.Directional:
                    if (dirLightCount < maxDirLightCount)
                    {
                        SetupDirectionalLight(dirLightCount++,ref light);
                    }
                    break;
            }
            
        }
        buffer.SetGlobalInt(dirLightCountId, dirLightCount);
        buffer.SetGlobalVectorArray(dirLightColorId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);
    }

    void SetupDirectionalLight(int index,ref VisibleLight light)
    {
        dirLightColors[index] = light.finalColor;
        dirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);
        shadows.ReserveDirectionalShadows(light.light, index);
    }

    public void Cleanup()
    {
        shadows.Cleanup();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    
}
