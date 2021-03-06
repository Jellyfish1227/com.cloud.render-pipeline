using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Cloud/Cloud Deferred Render Pipeline")]
public class CloudRPAsset : RenderPipelineAsset
{
    [Header("SRP Batcher")] 
    [SerializeField]
    bool useDynamicBatching = true;
    [SerializeField]
    bool useSRPBatcher = true;
    [Header("Z-Pre Pass")]
    [SerializeField]
    bool useZPrePass = true;
    [Header("Fill GBuffer")]
    [SerializeField]
    bool fillGBffer = false;
    [Header("Cluster Light")]
    [SerializeField] private ComputeShader clusterLight;
    [SerializeField] private bool debugMode = false;
    [Header("Shadow Settings")]
    [SerializeField] private ShadowSettings shadowSettings = default;
    protected override RenderPipeline CreatePipeline()
    {
        return new CloudRP(useDynamicBatching, useSRPBatcher, useZPrePass, fillGBffer, clusterLight, debugMode, shadowSettings);
    }
    
}
