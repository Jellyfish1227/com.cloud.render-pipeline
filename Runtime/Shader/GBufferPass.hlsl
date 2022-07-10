#ifndef CLOUD_GBUFFERPASS_INCLUDED
#define CLOUD_GBUFFERPASS_INCLUDED

struct GBufferOutput
{
    half4 gBuffer0 : SV_TARGET0;
    half4 gBuffer1 : SV_TARGET1;
    half4 gBuffer2 : SV_TARGET2;
    half4 gBuffer3 : SV_TARGET3;
};

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
    float4 positionCS : SV_POSITION;
};

Varyings vert(Attributes input)
{
    Varyings output;
    VertexPositionInputs posInput = GetVertexPositionInputs(input.positionOS.xyz);
    output.uv = input.texcoord;
    output.positionWS = posInput.positionWS;
    output.normalWS = TransformObjectToWorldNormal(input.normalOS);
    output.positionCS = posInput.positionCS;
    return output;
}

GBufferOutput frag(Varyings input)
{
    GBufferOutput output;
    output.gBuffer0 = half4(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv).rgb, 0);
    output.gBuffer1 = half4(input.normalWS, 0);
    output.gBuffer2 = 0;
    output.gBuffer3 = 0;
    return output;
}

#endif