Shader "Hidden/MyMotion/MaterialPropertyIdBenchmark"
{
    Properties
    {
        _Value ("Float Value", Float) = 0
        _IntegerValue ("Integer Value", Integer) = 0
        _Color ("Color", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            struct Attributes
            {
                float4 Position : POSITION;
            };

            struct Varyings
            {
                float4 Position : SV_POSITION;
            };

            float _Value;
            int _IntegerValue;
            float4 _Color;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.Position = input.Position;
                return output;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
