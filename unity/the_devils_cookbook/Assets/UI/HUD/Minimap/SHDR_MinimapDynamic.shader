Shader "TDC/UI/SHDR_MinimapDynamic"
{
    Properties
    {
        
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always 
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            #ifdef INSTANCING_ON
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #else
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            #endif

            #ifdef INSTANCING_ON
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            #else
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            #endif
            

            StructuredBuffer<int> textureIndices;
            // StructuredBuffer<float4> colours;
            UNITY_DECLARE_TEX2D(staticTex);
            UNITY_DECLARE_TEX2DARRAY(icons);
            
            #ifdef INSTANCING_ON
            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                if (i.instanceID == 0)
                {
                    return UNITY_SAMPLE_TEX2D(staticTex, i.uv);
                }
                return UNITY_SAMPLE_TEX2DARRAY(icons, float3(i.uv.xy, textureIndices[i.instanceID]));
            }
            #else
            fixed4 frag(v2f i) : SV_Target
            {
                return fixed4(1,0,1,1);
            }

            #endif
            
            ENDHLSL
        }
    }
}
