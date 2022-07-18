#ifndef CLOUD_LIGHTING_INCLUDED
#define CLOUD_LIGHTING_INCLUDED

float3 IncomingLight(Surface surface, Light light)
{
    return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting(Surface surface, BRDFData data, Light light)
{
    return IncomingLight(surface, light) * DirectBRDF(surface, data, light);
}

float3 GetLighting(Surface surface, BRDFData data)
{
    int dirLightCount = GetDirectionalLightCount();
    float3 color = 0.0;
    for (int i = 0; i < dirLightCount; i++)
    {
        color += GetLighting(surface, data, GetDirectionalLight(i));
    }
    return color;
}

#endif