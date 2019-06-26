namespace UnityEngine.Rendering.PPSMobile
{
    // Pre-hashed shader ids - naming conventions are a bit off in this file as we use the same
    // fields names as in the shaders for ease of use... Would be nice to clean this up at some
    // point.
    static class ShaderIDs
    {
        internal static readonly int MainTex                         = Shader.PropertyToID("_MainTex");

        internal static readonly int HistogramBuffer                 = Shader.PropertyToID("_HistogramBuffer");
        internal static readonly int Params                          = Shader.PropertyToID("_Params");
        internal static readonly int ScaleOffsetRes                  = Shader.PropertyToID("_ScaleOffsetRes");

        internal static readonly int BloomTex                        = Shader.PropertyToID("_BloomTex");
        internal static readonly int SampleScale                     = Shader.PropertyToID("_SampleScale");
        internal static readonly int Threshold                       = Shader.PropertyToID("_Threshold");
        internal static readonly int ColorIntensity                  = Shader.PropertyToID("_ColorIntensity");
        internal static readonly int Bloom_Settings                  = Shader.PropertyToID("_Bloom_Settings");
        internal static readonly int Bloom_Color                     = Shader.PropertyToID("_Bloom_Color");
        
        internal static readonly int Lut2D                           = Shader.PropertyToID("_Lut2D");
        internal static readonly int Lut3D                           = Shader.PropertyToID("_Lut3D");
        internal static readonly int Lut2D_Params                    = Shader.PropertyToID("_Lut2D_Params");
        internal static readonly int PostExposure                    = Shader.PropertyToID("_PostExposure");
        internal static readonly int ColorBalance                    = Shader.PropertyToID("_ColorBalance");
        internal static readonly int ColorFilter                     = Shader.PropertyToID("_ColorFilter");
        internal static readonly int HueSatCon                       = Shader.PropertyToID("_HueSatCon");
        internal static readonly int Brightness                      = Shader.PropertyToID("_Brightness");
        internal static readonly int ChannelMixerRed                 = Shader.PropertyToID("_ChannelMixerRed");
        internal static readonly int ChannelMixerGreen               = Shader.PropertyToID("_ChannelMixerGreen");
        internal static readonly int ChannelMixerBlue                = Shader.PropertyToID("_ChannelMixerBlue");
        internal static readonly int Lift                            = Shader.PropertyToID("_Lift");
        internal static readonly int InvGamma                        = Shader.PropertyToID("_InvGamma");
        internal static readonly int Gain                            = Shader.PropertyToID("_Gain");
        internal static readonly int CustomToneCurve                 = Shader.PropertyToID("_CustomToneCurve");
        internal static readonly int ToeSegmentA                     = Shader.PropertyToID("_ToeSegmentA");
        internal static readonly int ToeSegmentB                     = Shader.PropertyToID("_ToeSegmentB");
        internal static readonly int MidSegmentA                     = Shader.PropertyToID("_MidSegmentA");
        internal static readonly int MidSegmentB                     = Shader.PropertyToID("_MidSegmentB");
        internal static readonly int ShoSegmentA                     = Shader.PropertyToID("_ShoSegmentA");
        internal static readonly int ShoSegmentB                     = Shader.PropertyToID("_ShoSegmentB");

        internal static readonly int LumaInAlpha                     = Shader.PropertyToID("_LumaInAlpha");

        internal static readonly int DitheringTex                    = Shader.PropertyToID("_DitheringTex");
        internal static readonly int Dithering_Coords                = Shader.PropertyToID("_Dithering_Coords");

        internal static readonly int From                            = Shader.PropertyToID("_From");
        internal static readonly int To                              = Shader.PropertyToID("_To");
        internal static readonly int Interp                          = Shader.PropertyToID("_Interp");
        internal static readonly int TargetColor                     = Shader.PropertyToID("_TargetColor");

        internal static readonly int HalfResFinalCopy                = Shader.PropertyToID("_HalfResFinalCopy");
        internal static readonly int WaveformSource                  = Shader.PropertyToID("_WaveformSource");
        internal static readonly int WaveformBuffer                  = Shader.PropertyToID("_WaveformBuffer");
        internal static readonly int VectorscopeBuffer               = Shader.PropertyToID("_VectorscopeBuffer");

        internal static readonly int RenderViewportScaleFactor       = Shader.PropertyToID("_RenderViewportScaleFactor");

        internal static readonly int UVTransform                     = Shader.PropertyToID("_UVTransform");
    }
}
