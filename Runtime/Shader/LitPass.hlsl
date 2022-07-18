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
    return float4(col, surface.alpha);
}

#endif