Shader "Custom/Invisible"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        Pass {
            ColorMask 0
        }
    }
}
