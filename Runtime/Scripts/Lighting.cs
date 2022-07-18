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
    private CullingResults cullingResults;

    private static Vector4[] dirLightColors = new Vector4[maxDirLightCount];
    private static Vector4[] dirLightDirections = new Vector4[maxDirLightCount];

    private CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };

    public void Setup(ScriptableRenderContext ctx, CullingResults cullingResults)
    {
        this.cullingResults = cullingResults;
        buffer.BeginSample(bufferName);
        SetupLight();
        buffer.EndSample(bufferName);
        ctx.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }

    void SetupLight()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight light = visibleLights[i];
            if (light.lightType == LightType.Directional && dirLightCount < maxDirLightCount)
            {
                SetupDirectionalLight(dirLightCount++,ref light);
            }
        }
        buffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        buffer.SetGlobalVectorArray(dirLightColorId, dirLightColors);
        buffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);
    }

    void SetupDirectionalLight(int index,ref VisibleLight light)
    {
        dirLightColors[index] = light.finalColor;
        dirLightDirections[index] = -light.localToWorldMatrix.GetColumn(2);
    }
}
