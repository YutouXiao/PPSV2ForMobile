Shader "Hidden/PostProcessing/Uber"
{
    HLSLINCLUDE

        #pragma target 3.0

        #pragma multi_compile __ BLOOM_LOW
        #pragma multi_compile __ FINALPASS
        // the following keywords are handled in API specific SubShaders below
        // #pragma multi_compile __ COLOR_GRADING_LDR_2D COLOR_GRADING_HDR_2D
        
        #pragma vertex VertUVTransform
        #pragma fragment FragUber
    
        #include "../StdLib.hlsl"
        #include "../Colors.hlsl"
        #include "../Sampling.hlsl"
        #include "Dithering.hlsl"

        #define MAX_CHROMATIC_SAMPLES 16

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float4 _MainTex_TexelSize;

        // Bloom
        TEXTURE2D_SAMPLER2D(_BloomTex, sampler_BloomTex);
        float4 _BloomTex_TexelSize;
        half3 _Bloom_Settings; // x: sampleScale, y: intensity
        half3 _Bloom_Color;

        // Color grading
        TEXTURE2D_SAMPLER2D(_Lut2D, sampler_Lut2D);
        float3 _Lut2D_Params;

        half _PostExposure; // EV (exp2)

        // Misc
        half _LumaInAlpha;

        half4 FragUber(VaryingsDefault i) : SV_Target
        {
            float2 uv = i.texcoord;
          
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

            // Gamma space... Gah.
            #if UNITY_COLORSPACE_GAMMA
            {
                color = SRGBToLinear(color);
            }
            #endif

            #if BLOOM_LOW
            {
                half4 bloom = UpsampleBox(TEXTURE2D_PARAM(_BloomTex, sampler_BloomTex), uv, _BloomTex_TexelSize.xy, _Bloom_Settings.x);

                // Additive bloom (artist friendly)
                bloom *= _Bloom_Settings.y;
                color += bloom * half4(_Bloom_Color, 1.0);
            }
            #endif

            #if COLOR_GRADING_HDR_2D
            {
                color *= _PostExposure;
                float3 colorLutSpace = saturate(LUT_SPACE_ENCODE(color.rgb));
                color.rgb = ApplyLut2D(TEXTURE2D_PARAM(_Lut2D, sampler_Lut2D), colorLutSpace, _Lut2D_Params);
            }
            #elif COLOR_GRADING_LDR_2D
            {
                color = saturate(color);

                // LDR Lut lookup needs to be in sRGB - for HDR stick to linear
                color.rgb = LinearToSRGB(color.rgb);
                color.rgb = ApplyLut2D(TEXTURE2D_PARAM(_Lut2D, sampler_Lut2D), color.rgb, _Lut2D_Params);
                color.rgb = SRGBToLinear(color.rgb);
            }
            #endif

            half4 output = color;

            #if FINALPASS
            {
                #if UNITY_COLORSPACE_GAMMA
                {
                    output = LinearToSRGB(output);
                }
                #endif

                output.rgb = Dither(output.rgb, uv);
            }
            #else
            {
                UNITY_BRANCH
                if (_LumaInAlpha > 0.5)
                {
                    // Put saturated luma in alpha for FXAA - higher quality than "green as luma" and
                    // necessary as RGB values will potentially still be HDR for the FXAA pass
                    half luma = Luminance(saturate(output));
                    output.a = luma;
                }

                #if UNITY_COLORSPACE_GAMMA
                {
                    output = LinearToSRGB(output);
                }
                #endif
            }
            #endif

            // Output RGB is still HDR at that point (unless range was crunched by a tonemapper)
            return output;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
                #pragma exclude_renderers gles vulkan switch
                #pragma multi_compile __ COLOR_GRADING_LDR_2D COLOR_GRADING_HDR_2D
            ENDHLSL
        }
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
                #pragma only_renderers vulkan
                #pragma multi_compile __ COLOR_GRADING_LDR_2D COLOR_GRADING_HDR_2D                
            ENDHLSL
        }
    }
    
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
                #pragma only_renderers gles
                #pragma multi_compile __ COLOR_GRADING_LDR_2D COLOR_GRADING_HDR_2D                
            ENDHLSL
        }
    }
}
