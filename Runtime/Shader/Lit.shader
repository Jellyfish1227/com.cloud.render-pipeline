Shader "Cloud/Lit"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write", Float) = 1
        [Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply Alpha", Float) = 0
        _Metallic("Metallic", Range(0,1)) = 0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
        }
        HLSLINCLUDE
        #include "../ShaderLibrary/Common.hlsl"
        ENDHLSL

        Pass
        {
            Name "GBufferPass"
            Tags 
            {
                "LightMode" = "GBuffer"
            }
            ZWrite On
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            #include "GBufferPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "Lit Pass"
            Tags
            {
                "LightMode" = "Lit"
            }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _PREMULTIPLY_ALPHA
        
            #include "LitPass.hlsl"

            ENDHLSL
        }
    }
}
