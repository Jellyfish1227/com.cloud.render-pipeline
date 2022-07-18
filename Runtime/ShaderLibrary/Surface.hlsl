#ifndef CLOUD_SURFACE_INCLUDED
#define CLOUD_SURFACE_INCLUDED

struct Surface
{
    float3 normal;
    float3 color;
    float3 posWS;
    float3 viewDir;
    float alpha;
    float metallic;
    float smoothness;
};

#endif