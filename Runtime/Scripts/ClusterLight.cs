using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using UnityEngine.Assertions.Must;

public class ClusterLight
{
    private const int maxAdditionalLightCount = 1024;
    private const int maxVoxelAdditionalLightCount = 64;
    private const int XRes = 32;
    private const int YRes = 32;
    private const int ZRes = 64;
    private const int maxVoxelCount = XRes * YRes * ZRes;
    
    private static int pointLightCountID = Shader.PropertyToID("_PointLightCount");
    private static int XYPlaneBuffer = Shader.PropertyToID("_XYPlaneBuffer");
    private static int ZPlaneBuffer = Shader.PropertyToID("_ZPlaneBuffer");
    private static int PointLightTexture = Shader.PropertyToID("_PointLightTexture");
    private static int invVP = Shader.PropertyToID("_InvVP");
    private static int nearPlane = Shader.PropertyToID("_CameraNearPos");
    private static int farPlane = Shader.PropertyToID("_CameraFarPos");
    private static int cameraForward = Shader.PropertyToID("_CameraForward");
    private static int pointLightsBufferID = Shader.PropertyToID("_PointLightsBuffer");
    private static int pointLightsIndexBufferID = Shader.PropertyToID("_PointLightsIndexBuffer");
    private static int resolutionID = Shader.PropertyToID("_Resolution");

    struct PointLight
    {
        public Vector4 lightColor;
        public Vector4 sphere;
    }
    
    struct XYPlanes
    {
        public Vector4 topPlane;
        public Vector4 bottomPlane;
        public Vector4 rightPlane;
        public Vector4 leftPlane;
    };

    struct ZPlanes
    {
        public Vector4 frontPlane;
        public Vector4 backPlane;
    };

    struct DebugBox
    {
        public Vector3 p0, p1, p2, p3, p4, p5, p6, p7;
    }

    private PointLight[] pointLights;
    private int[] pointLightsIdnex;
    private XYPlanes[] xyPlanesArray;
    private ZPlanes[] zPlanesArray;
    private ScriptableRenderContext ctx;
    private CommandBuffer cmd;
    private CullingResults cullingResults;
    
    private ComputeShader clusterComputeShader;
    
    private ComputeBuffer xyPlaneBuffer;
    private ComputeBuffer zPlaneBuffer;
    private ComputeBuffer pointLightsIndexBuffer;
    private ComputeBuffer pointLightsBuffer;
    
    private RenderTexture pointLightTexture;

    private Camera camera;

    private bool debug;

    public void Setup(ref ScriptableRenderContext ctx, ref CullingResults cullingResults, Camera camera, ComputeShader clusterLight, bool clusterDebug)
    {
        clusterComputeShader = clusterLight;
        this.camera = camera;
        this.ctx = ctx;
        this.cullingResults = cullingResults;
        debug = clusterDebug;
        cmd = new CommandBuffer()
        {
            name = "Cluster Light"
        };
        cmd.BeginSample(cmd.name);
        ctx.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        if (camera.cameraType == CameraType.Game)
        {
            pointLightsIdnex = new int[maxVoxelAdditionalLightCount * maxVoxelCount];
            if (debug)
            {
                SetDebugArrays();
            }
            PrepareAdditionalLight();
            SetupClusterLight();
            SetupGlobalVal();
            DoClusterLight();
        }
        if (xyPlanesArray != null && zPlanesArray != null && clusterDebug)
        {
            DoDebugDrawLines();
        }
        cmd.EndSample(cmd.name);
        ctx.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }
    
    void SetDebugArrays()
    {
        xyPlanesArray = new XYPlanes[XRes * YRes];
        zPlanesArray = new ZPlanes[ZRes];
    }

    void PrepareAdditionalLight()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        pointLights = new PointLight[maxAdditionalLightCount];
        int pointLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight light = visibleLights[i];
            switch (light.lightType)
            {
                case LightType.Point:
                    if (pointLightCount < maxAdditionalLightCount)
                    {
                        PointLight point;
                        point.lightColor = light.finalColor;
                        point.sphere = light.localToWorldMatrix.GetColumn(3);
                        point.sphere.w = light.range;
                        pointLights[pointLightCount++] = point;
                    }
                    break;
            }
        }
        clusterComputeShader.SetInt(pointLightCountID, pointLightCount);
    }

    void SetupGlobalVal()
    {
        Matrix4x4 viewPro = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false)* camera.worldToCameraMatrix;
        clusterComputeShader.SetMatrix(invVP, Matrix4x4.Inverse(viewPro));
        clusterComputeShader.SetVector(cameraForward, camera.transform.forward);
        cmd.SetGlobalVector(cameraForward, camera.transform.forward);
        
        Vector3 nearPos = camera.nearClipPlane * camera.transform.forward + camera.transform.position;
        Vector3 farPos = camera.farClipPlane * camera.transform.forward + camera.transform.position;
        
        clusterComputeShader.SetVector(nearPlane, nearPos);
        clusterComputeShader.SetVector(farPlane, farPos);
        pointLightsBuffer.SetData(pointLights);
    }

    void SetComputeBuffer(ref ComputeBuffer computeBuffer, int size, int stride)
    {
        if (computeBuffer != null)
        {
            computeBuffer.Dispose();
            computeBuffer = null;
        }
        computeBuffer = new ComputeBuffer(size, stride);
    }

    void SetRenderTexture(ref RenderTexture target)
    {
        if (target != null)
        {
            target.Release();
            target = null;
        }
        RenderTextureDescriptor desc = new RenderTextureDescriptor
        {
            autoGenerateMips = false,
            bindMS = false,
            colorFormat = RenderTextureFormat.RGInt,
            height = YRes,
            width = XRes,
            dimension = TextureDimension.Tex3D,
            msaaSamples = 1,
            volumeDepth = ZRes,
            enableRandomWrite = true
        };
        target = new RenderTexture(desc);
    }
    
    void SetupClusterLight()
    {
        SetComputeBuffer(ref pointLightsBuffer, maxAdditionalLightCount, 8 * 4);
        SetComputeBuffer(ref pointLightsIndexBuffer, maxVoxelAdditionalLightCount * maxVoxelCount, 4);
        SetComputeBuffer(ref xyPlaneBuffer, XRes * YRes, 16 * 4);
        SetComputeBuffer(ref zPlaneBuffer, ZRes, 8 * 4);
        
        SetRenderTexture(ref pointLightTexture);
    }

    void DoXYPlanes()
    {
        clusterComputeShader.SetBuffer(0, XYPlaneBuffer, xyPlaneBuffer);
        
        clusterComputeShader.Dispatch(0, 1, 1, 1);
        
        if (debug)
        {
            xyPlaneBuffer.GetData(xyPlanesArray);
        }
    }

    void DoZPlanes()
    {
        clusterComputeShader.SetBuffer(1, ZPlaneBuffer, zPlaneBuffer);
        
        clusterComputeShader.Dispatch(1, 1, 1, 1);
        
        if (debug)
        {
            zPlaneBuffer.GetData(zPlanesArray);
        }
    }

    Vector3 GetIntersectPoint(Vector4 plane, Ray line)
    {
        Vector3 normal = new Vector3(plane.x, plane.y, plane.z).normalized;
        Vector3 planePoint = -normal * plane.w;
        Plane unityPlane = new Plane(normal, planePoint);
        float enter = 0;
        unityPlane.Raycast(line, out enter);
        return line.GetPoint(enter);
    }
    
    Ray GetIntersectLine(Vector4 planeA, Vector4 planeB)
    {
        Vector3 normalA = new Vector3(planeA.x, planeA.y, planeA.z).normalized;
        Vector3 normalB = new Vector3(planeB.x, planeB.y, planeB.z).normalized;
        Vector3 dir = Vector3.Cross(normalA, normalB).normalized;
        Vector3 pointA = -normalA * planeA.w;
        Vector3 vertical = Vector3.Cross(dir, normalA).normalized;
        Ray line = new Ray(pointA, vertical);
        Vector3 pos = GetIntersectPoint(planeB, line);
        Ray res = new Ray(pos, dir);
        return res;
    }

    bool HasLight(int xyIndex, int zIndex)
    {
        int index = xyIndex % XRes * maxVoxelAdditionalLightCount + xyIndex / XRes * XRes * maxVoxelAdditionalLightCount + 
                    zIndex * XRes * YRes * maxVoxelAdditionalLightCount;
        for (int i = index; i < (index + maxVoxelAdditionalLightCount); i++)
        {
            if (pointLightsIdnex[i] > 0)
            {
                return true;
            }
        }
        return false;
    }

    void DrawBox(DebugBox box, int xyIndex, int zIndex)
    {
        bool hasLight = HasLight(xyIndex, zIndex);
        Color color = hasLight ? Color.red : Color.white;
        Debug.DrawLine(box.p0, box.p1, color);
        Debug.DrawLine(box.p0, box.p3, color);
        Debug.DrawLine(box.p0, box.p4, color);
        
        Debug.DrawLine(box.p1, box.p2, color);
        Debug.DrawLine(box.p1, box.p5, color);
        
        Debug.DrawLine(box.p2, box.p3, color);
        Debug.DrawLine(box.p2, box.p6, color);
        
        Debug.DrawLine(box.p3, box.p7, color);
        
        Debug.DrawLine(box.p4, box.p5, color);
        Debug.DrawLine(box.p4, box.p7, color);
        
        Debug.DrawLine(box.p5, box.p6, color);
        
        Debug.DrawLine(box.p6, box.p7, color);
    }

    void DoDebugDrawLines()
    {
        for (int i = 0; i < xyPlanesArray.Length; i++)
        {
            XYPlanes xyPlanes = xyPlanesArray[i];
            for (int j = 0; j < zPlanesArray.Length; j++)
            {
                ZPlanes zPlanes = zPlanesArray[j];
                Ray backAndTop = GetIntersectLine(xyPlanes.topPlane, zPlanes.backPlane);
                Ray backAndBottom = GetIntersectLine(xyPlanes.bottomPlane, zPlanes.backPlane);
                Ray frontAndTop = GetIntersectLine(xyPlanes.topPlane, zPlanes.frontPlane);
                Ray frontAndBottom = GetIntersectLine(xyPlanes.bottomPlane, zPlanes.frontPlane);
                DebugBox box = new DebugBox();
                box.p0 = GetIntersectPoint(xyPlanes.leftPlane, frontAndTop);
                box.p1 = GetIntersectPoint(xyPlanes.leftPlane, backAndTop);
                box.p2 = GetIntersectPoint(xyPlanes.rightPlane, backAndTop);
                box.p3 = GetIntersectPoint(xyPlanes.rightPlane, frontAndTop);
                box.p4 = GetIntersectPoint(xyPlanes.leftPlane, frontAndBottom);
                box.p5 = GetIntersectPoint(xyPlanes.leftPlane, backAndBottom);
                box.p6 = GetIntersectPoint(xyPlanes.rightPlane, backAndBottom);
                box.p7 = GetIntersectPoint(xyPlanes.rightPlane, frontAndBottom);
                DrawBox(box, i, j);
            }
        }
    }

    void DoLight()
    {
        pointLightsIndexBuffer.SetData(pointLightsIdnex);
        
        clusterComputeShader.SetBuffer(2, pointLightsBufferID, pointLightsBuffer);
        clusterComputeShader.SetBuffer(2, pointLightsIndexBufferID, pointLightsIndexBuffer);
        clusterComputeShader.SetBuffer(2, XYPlaneBuffer, xyPlaneBuffer);
        clusterComputeShader.SetBuffer(2, ZPlaneBuffer, zPlaneBuffer);
        
        clusterComputeShader.SetTexture(2, PointLightTexture, pointLightTexture);
        
        clusterComputeShader.Dispatch(2, 1, 1, ZRes);
        
        if (debug)
        {
            pointLightsIndexBuffer.GetData(pointLightsIdnex);
        }
    }

    void SetLightPassVal()
    {
        cmd.SetGlobalBuffer(pointLightsBufferID, pointLightsBuffer);
        cmd.SetGlobalBuffer(pointLightsIndexBufferID, pointLightsIndexBuffer);
        cmd.SetGlobalTexture(PointLightTexture, pointLightTexture);
        cmd.SetGlobalVector(resolutionID, new Vector4(XRes, YRes, ZRes, maxVoxelAdditionalLightCount));
    }
    
    void DoClusterLight()
    {
        DoXYPlanes();
        DoZPlanes();
        DoLight();
        SetLightPassVal();
        
    }

    ~ClusterLight()
    {
        pointLightsBuffer.Dispose();
        pointLightsIndexBuffer.Dispose();
        xyPlaneBuffer.Dispose();
        zPlaneBuffer.Dispose();
        pointLightTexture.Release();
    }
}
