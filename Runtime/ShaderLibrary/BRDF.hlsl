#ifndef CLOUD_BRDF_INCLUDED
#define CLOUD_BRDF_INCLUDED

#define MIN_REFLECTIVITY 0.04

struct BRDFData
{
    float3 diffuse;
    float3 specular;
    float roughness;
};

float OneMinuesReflectivity(float metallic)
{
    float range = 1.0 - MIN_REFLECTIVITY;
    return range - metallic * range;
}

BRDFData GetBRDF(Surface surface)
{
    float oneMinusReflectivity = OneMinuesReflectivity(surface.metallic);
    float roughness = PerceptualSmoothnessToRoughness(surface.smoothness);
    BRDFData brdf;
    brdf.diffuse = surface.color * oneMinusReflectivity;
    #if _PREMULTIPLY_ALPHA
        brdf.diffuse *= surface.alpha;
    #endif
    brdf.specular = lerp(MIN_REFLECTIVITY, surface.color, surface.metallic);
    brdf.roughness = roughness;
    return brdf;
}

float SpecularStrength(Surface surface, BRDFData data, Light light)
{
    float3 h = SafeNormalize(light.direction + surface.viewDir);
    float nh2 = Square(saturate(dot(surface.normal, h)));
    float lh2 = Square(saturate(dot(light.direction, h)));
    float r2 = Square(data.roughness);
    float d2 = Square(nh2 * (r2 - 1) + 1.0001);
    float normalization = data.roughness * 4.0 + 2.0;
    return r2 / (d2 *max(0.1, lh2) * normalization);
}

float DirectBRDF(Surface surface, BRDFData data, Light light)
{
    return SpecularStrength(surface, data, light) * data.specular + data.diffuse;
}

#endif
