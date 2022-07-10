#ifndef CLOUD_LIT_PASS_INCLUDED
#define CLOUD_LIT_PASS_INCLUDED

#include "LitInput.hlsl"
#include "../ShaderLibrary/Surface.hlsl"

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

float4 frag(Varyings input) : SV_TARGET
{
    float4 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
    Surface surface;
    surface.normal = normalize(input.normalWS);
    surface.color = base.rgb;
    surface.alpha = base.a;
    return float4(surface.color, surface.alpha);
}

#endif