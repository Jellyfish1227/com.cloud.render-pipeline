#ifndef CLOUD_LIT_INPUT_INCLUDED
#define CLOUD_LIT_INPUT_INCLUDED

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float2 texcoord : TEXCOORD0;
};

struct Varyings
{
    float2 uv : TEXCOORD0;
    float3 positionWS : TEXCOORD1;
    float3 normalWS : TEXCOORD2;
    float4 positionNDC : TEXCOORD3;
    float4 positionCS : SV_POSITION;
};

struct PointLight
{
    float4 lightColor;
    float4 sphere;
};

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_CameraDepthTexture);
SAMPLER(sampler_CameraDepthTexture);

Texture3D<uint2> _PointLightTexture;
StructuredBuffer<PointLight> _PointLightsBuffer;
StructuredBuffer<uint> _PointLightsIndexBuffer;
uint4 _Resolution;

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _Metallic;
float _Smoothness;
CBUFFER_END

#endif