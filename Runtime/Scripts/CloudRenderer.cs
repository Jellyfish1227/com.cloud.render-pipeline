using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

public partial class CloudRenderer
{
    private ScriptableRenderContext context;
    private Camera camera;
    private CullingResults cullingResults;
    private const string BufferName = "Cloud Renderer";
    private CommandBuffer buffer = new CommandBuffer
    {
        name = BufferName
    };

    //private Gbuffer gBuffer = new Gbuffer();

    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private static ShaderTagId[] ShaderTagIds = new[]
    {
        new ShaderTagId("Lit"),
    };

    private static Material errorMaterial;
    bool useDynamicBatching;
    bool useZPrePass;
    private bool fillGBuffer;
    private RenderTexture depthTexture;
    private RenderTexture frameBuffer;
    private static int cameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");
    private static int cameraFrameTexture = Shader.PropertyToID("_CameraFrameTexture");
    private static Material preDepthPassMat;
    private Lighting lighting = new Lighting();
    private ClusterLight clusterLight = new ClusterLight();

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useZPrePass,
        bool fillGBuffer, ComputeShader clusterLight, bool clusterDebug, ShadowSettings shadowSettings) 
    {
        this.camera = camera;
        this.context = context;
        this.useDynamicBatching = useDynamicBatching;
        this.useZPrePass = useZPrePass;
        this.fillGBuffer = fillGBuffer;
        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance))
        {
            return;
        }
        this.clusterLight.Setup(ref context, ref cullingResults, camera, clusterLight, clusterDebug);
        lighting.Setup(ref context, ref cullingResults, shadowSettings);
        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizmos();
        Cleanup();
        Submit();
    }

    bool Cull(float maxShadowDistance)
    {
        if(camera.TryGetCullingParameters(out ScriptableCullingParameters param))
        {
            param.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
            cullingResults = context.Cull(ref param);
            return true;
        }
        return false;
    }

    void Cleanup()
    {
        lighting.Cleanup();
        buffer.ReleaseTemporaryRT(cameraFrameTexture);
        buffer.ReleaseTemporaryRT(cameraDepthTexture);
        //gBuffer.Cleanup();
    }

    void SetTexture()
    {
        buffer.GetTemporaryRT(cameraFrameTexture, camera.pixelWidth, camera.pixelHeight, 32, FilterMode.Bilinear,
            RenderTextureFormat.DefaultHDR);
        buffer.GetTemporaryRT(cameraDepthTexture, camera.pixelWidth, camera.pixelHeight, 24, FilterMode.Point,
            RenderTextureFormat.Depth);
    }

    void Setup()
    {
        preDepthPassMat = new Material(Shader.Find("Cloud/ZPrePass"));
        context.SetupCameraProperties(camera);
        CameraClearFlags flags = camera.clearFlags;
        SetTexture();
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.SetRenderTarget(cameraFrameTexture, depth: cameraDepthTexture);
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags <= CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        if (fillGBuffer)
        {
            //gBuffer.Setup(context, camera, cullingResults, ref preDepth);
        }
    }

    void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera);
        var drawSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
        {
            enableDynamicBatching = useDynamicBatching
        };
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        for (int i = 0; i < ShaderTagIds.Length; i++)
        {
            drawSettings.SetShaderPassName(i + 1, ShaderTagIds[i]);
        }
        if (useZPrePass)
        {
            drawSettings.overrideMaterial = preDepthPassMat;
            context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
            drawSettings.overrideMaterial = null;
        }
        
        if (fillGBuffer)
        {
            //gBuffer.DoGBufferPass(useDynamicBatching);
            buffer.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            ExecuteBuffer();
        }
        
        //buffer.SetGlobalTexture(cameraDepthTexture, preDepth);
        
        context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);

        ExecuteBuffer();
        context.DrawSkybox(camera);
        
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawSettings.sortingSettings = sortingSettings;
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        
        context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
        
        buffer.Blit(cameraFrameTexture, BuiltinRenderTextureType.CameraTarget);
    }

    void Submit()
    {
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }

    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}
