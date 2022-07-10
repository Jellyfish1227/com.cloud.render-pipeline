Shader "Cloud/ZPrePass"
{
    Properties
    {
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
        }
        ZWrite On
        ColorMask 0

        Pass
        {
            Name "Z-Pre Pass"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "../ShaderLibrary/Common.hlsl"
            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(vertex.xyz);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
