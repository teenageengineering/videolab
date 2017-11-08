Shader "Kvant/Wig/Filament"
{
    Properties
    {
        [HideInInspector] _PositionBuffer("", 2D) = ""{}
        [HideInInspector] _BasisBuffer("", 2D) = ""{}

        [Gamma] _Metallic("Metallic", Range(0, 1)) = 0
        _Smoothness("Smoothness", Range(0, 1)) = 0

        [Space]
        _Thickness("Thickness", Range(0, 0.1)) = 0.02
        _ThickRandom("Randomize", Range(0, 1)) = 0.5

        [Header(Base)]

        _BaseColor("Color", Color) = (1, 1, 1)
        _BaseRandom("Randomize", Range(0, 1)) = 1

        [Header(Glow)]

        _GlowIntensity("Intensity", Range(0, 20)) = 1
        _GlowProb("Probability", Range(0, 1)) = 0.1
        _GlowColor("Color", Color) = (1, 1, 1)
        _GlowRandom("Randomize", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode" = "MotionVectors" }
            ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Motion.cginc"
            ENDCG
        }
        CGPROGRAM
        #pragma surface surf Standard vertex:vert nolightmap addshadow
        #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
        #pragma target 3.0
        #include "Filament.cginc"
        ENDCG
    }
}
