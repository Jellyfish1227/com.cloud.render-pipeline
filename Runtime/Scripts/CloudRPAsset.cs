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
    protected override RenderPipeline CreatePipeline()
    {
        return new CloudRP(useDynamicBatching, useSRPBatcher, useZPrePass, fillGBffer);
    }
    
}
