using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Gbuffer
{
   private RenderTexture[] gBuffers = new RenderTexture[4];
   private RenderTargetIdentifier[] targets = new RenderTargetIdentifier[4];
   private ScriptableRenderContext context;
   private const string bufferName = "GBuffer";
   private static ShaderTagId gBufferTagId = new ShaderTagId("GBuffer");
   private Camera camera;
   private CullingResults cullingResults;
   private static Material prePassMaterial;

   private CommandBuffer cmd = new CommandBuffer()
   {
      name = bufferName
   };

   public void DoGBufferPass(bool useDynamicBatching)
   {
       var sortingSettings = new SortingSettings(camera);
       var drawSettings = new DrawingSettings(gBufferTagId, sortingSettings)
       {
           enableDynamicBatching = useDynamicBatching
       };
       var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
       drawSettings.SetShaderPassName(0, gBufferTagId);
       context.DrawRenderers(cullingResults, ref drawSettings, ref filteringSettings);
       cmd.EndSample(bufferName);
       ExecuteBuffer();
   }

   public Gbuffer()
   { 
       gBuffers[0] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32,
               RenderTextureReadWrite.Linear); 
       gBuffers[1] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32,
               RenderTextureReadWrite.Linear); 
       gBuffers[2] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB64,
               RenderTextureReadWrite.Linear); 
       gBuffers[3] = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat,
               RenderTextureReadWrite.Linear); 
       for (int i = 0; i < 4; i++) 
       { 
           targets[i] = gBuffers[i]; 
       }
   }

   public void Setup(ScriptableRenderContext ctx, Camera camera, CullingResults results, ref RenderTexture depth)
   {
      context = ctx;
      this.camera = camera;
      cullingResults = results;
      cmd.SetRenderTarget(targets, depth);
      cmd.ClearRenderTarget(true, true, Color.clear);
      cmd.BeginSample(bufferName);
      ExecuteBuffer();
   }
   
   void ExecuteBuffer()
   {
      context.ExecuteCommandBuffer(cmd);
      cmd.Clear();
   }

   public void Cleanup()
   {
      
   }
}
