#ifndef CLOUD_LIT_PASS_INCLUDED
#define CLOUD_LIT_PASS_INCLUDED

#include "LitInput.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/BRDF.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

Varyings vert(Attributes input)
{
    Varyings output;
    VertexPositionInputs posInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.uv = input.texcoord * _BaseMap_ST.xy + _BaseMap_ST.zw;
    output.positionWS = posInput.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionCS = posInput.positionCS;
    output.positionNDC = posInput.positionNDC;
    return output;
}

float4 frag(Varyings input) : SV_TARGET
{
    float4 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.color = base.rgb;
    surface.alpha = base.a;
    surface.metallic = _Metallic;
    surface.smoothness = _Smoothness;
    surface.viewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
    BRDFData data = GetBRDF(surface);
    float3 col = GetLighting(surface, data);
    float2 screenUV = input.positionNDC.xy / input.positionNDC.w * _Resolution.xy;
    float z = saturate((dot(input.positionWS - _WorldSpaceCameraPos, _CameraForward.xyz) - _ProjectionParams.y) / (_ProjectionParams.z - _ProjectionParams.y)) * _Resolution.z;
    uint3 voxelIndex = uint3((uint2)screenUV, (uint)z);
    float2 index = _PointLightTexture[voxelIndex];
    float4 color = 1.0;
    for (int i = index.x; i < index.y; i++)
    {
        PointLight light = _PointLightsBuffer[_PointLightsIndexBuffer[i] - 1];
        color *= light.lightColor;
    }
    //return float4(col, surface.alpha);
    return color;//float4(index, 0.0, 1.0);//
}

#endif