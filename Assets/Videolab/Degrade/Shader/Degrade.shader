Shader "Hidden/Degrade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM

            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;

            float _colorDepth;

            fixed4 frag(v2f_img i) : SV_Target
            {
                float3 src = tex2D(_MainTex, i.uv).rgb;

                src = floor(src * _colorDepth) / _colorDepth;

                return fixed4(src, 1.0);
            }

            ENDCG
        }
    }
}
