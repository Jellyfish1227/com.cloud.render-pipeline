Shader "Cloud/Lit"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)
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
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag
        
            #include "LitPass.hlsl"

            ENDHLSL
        }
    }
}
